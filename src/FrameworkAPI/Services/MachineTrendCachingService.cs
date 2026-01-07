using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.Exceptions;
using FrameworkAPI.Extensions;
using FrameworkAPI.Models;
using FrameworkAPI.Services.Interfaces;
using Microsoft.Extensions.Logging;
using WuH.Ruby.Common.Core;
using WuH.Ruby.MachineSnapShooter.Client;
using WuH.Ruby.MachineSnapShooter.Client.Enums;
using WuH.Ruby.MachineSnapShooter.Client.Models;
using WuH.Ruby.MachineSnapShooter.Client.Queue;

namespace FrameworkAPI.Services;

public class MachineTrendCachingService : IMachineTrendCachingService, IDisposable
{
    private readonly ConcurrentDictionary<string, MachineTrendCache>
        _machineTrendsCache = new();

    private readonly IMachineSnapshotHttpClient _machineSnapshotHttpClient;
    private readonly ILatestMachineSnapshotCachingService _latestMachineSnapshotCachingService;
    private readonly IMachineTimeService _machineTimeService;
    private readonly IDisposable _liveSnapshotSubscriptionDisposable;
    private readonly IDisposable _historicSubscriptionDisposable;
    private readonly ILogger<MachineTrendCachingService> _logger;

    public MachineTrendCachingService(
        IMachineSnapshotHttpClient machineSnapshotHttpClient,
        ILatestMachineSnapshotCachingService latestMachineSnapshotCachingService,
        IMachineTimeService machineTimeService,
        IMachineSnapshotQueueWrapper machineSnapshotQueueWrapper,
        ILogger<MachineTrendCachingService> logger)
    {
        _machineSnapshotHttpClient = machineSnapshotHttpClient;
        _latestMachineSnapshotCachingService = latestMachineSnapshotCachingService;
        _machineTimeService = machineTimeService;
        _logger = logger;

        _liveSnapshotSubscriptionDisposable = Observable.FromEventPattern<LiveSnapshotEventArgs>(
                h => _latestMachineSnapshotCachingService.CacheChanged += h,
                h => _latestMachineSnapshotCachingService.CacheChanged -= h)
            .GroupBy(ev => ev.EventArgs.MachineId)
            .SelectMany(machineGroup => machineGroup
                .Select(ev => Observable.FromAsync(
                    ctFromAsync => LatestMachineSnapshotCachingServiceOnCacheChanged(ev.EventArgs, ctFromAsync)))
                .Catch<IObservable<Unit>, Exception>(error =>
                {
                    _logger.LogWarning($"MachineTrendCache: Exception processing snapshot message. {error.Message}");
                    return Observable.Empty<IObservable<Unit>>();
                })
                .Concat())
            .Subscribe();

        _historicSubscriptionDisposable = machineSnapshotQueueWrapper.SubscribeForHistoricSnapshotChangeMessage(
            HistoricSnapshotChanged);
    }

    public void Dispose()
    {
        _liveSnapshotSubscriptionDisposable.Dispose();
        _historicSubscriptionDisposable.Dispose();
    }

    public async Task<IReadOnlyDictionary<DateTime, IReadOnlyDictionary<string, double?>?>?> Get(
        string machineId, CancellationToken cancellationToken)
    {
        var timeRange = await GetCurrentTrendTimeRange(machineId, cancellationToken);
        var cache = await GetInitializedCacheForMachine(machineId, timeRange, cancellationToken);
        return cache?.Get(timeRange);
    }

    private async Task<MachineTrendCache?> GetInitializedCacheForMachine(
        string machineId,
        TimeRange timeRange,
        CancellationToken cancellationToken)
    {
        var machineTrend = _machineTrendsCache.GetOrAdd(machineId, _ => new MachineTrendCache(machineId));

        if (!machineTrend.IsEmpty) return machineTrend;
        if (!await TryFillLatestGapsInCache(machineTrend, timeRange, cancellationToken))
            return null;

        return machineTrend;
    }

    private async Task<bool> TryFillLatestGapsInCache(
        MachineTrendCache machineTrendCache,
        TimeRange validTimeRange,
        CancellationToken cancellationToken)
    {
        var timeRangeFrom = validTimeRange.From;
        var lastCacheDate = machineTrendCache.LatestDateTime;

        // When there is no cached value for the machine we need to get the values for the full time range.
        // In this case we first check if snapshots exist for the full time range and might adjust the from.
        if (!lastCacheDate.HasValue)
        {
            var firstSnapshotResponse =
                await _machineSnapshotHttpClient.GetFirstSnapshot(
                    machineTrendCache.MachineId,
                    cancellationToken);

            if (firstSnapshotResponse.HasError && firstSnapshotResponse.Error.ErrorMessage ==
                MachineSnapshotErrorItemType.WaitingForFirstMinutelySnapshot.ToString())
            {
                return false;
            }

            if (firstSnapshotResponse.HasError)
                throw new InternalServiceException(firstSnapshotResponse.Error);

            if (timeRangeFrom < firstSnapshotResponse.Item.Data.SnapshotTime)
                timeRangeFrom = firstSnapshotResponse.Item.Data.SnapshotTime;
        }
        else
        {
            // Only fill gaps
            var diff = validTimeRange.To - lastCacheDate.Value;
            if (diff < TimeSpan.FromMinutes(2))
                return true;
            timeRangeFrom = lastCacheDate.Value;
        }

        return await LoadAndUpdateCacheValues(machineTrendCache, new TimeRange(timeRangeFrom, validTimeRange.To), cancellationToken);
    }

    private async Task<bool> LoadAndUpdateCacheValues(
        MachineTrendCache machineTrendCache,
        TimeRange timeRange,
        CancellationToken cancellationToken,
        IReadOnlyList<string>? columnIds = null)
    {
        var getSnapshotsInTimeRangesResponse =
            await _machineSnapshotHttpClient.GetSnapshotsInTimeRanges(
                machineTrendCache.MachineId,
                [.. columnIds ?? Constants.MachineTrend.TrendingSnapshotColumnIds],
                [timeRange],
                null,
                cancellationToken);

        if (getSnapshotsInTimeRangesResponse.HasError && getSnapshotsInTimeRangesResponse.Error.ErrorMessage ==
            MachineSnapshotErrorItemType.WaitingForFirstMinutelySnapshot.ToString())
        {
            return false;
        }

        if (getSnapshotsInTimeRangesResponse.HasError)
            throw new InternalServiceException(getSnapshotsInTimeRangesResponse.Error);

        machineTrendCache.UpdateCacheValues(getSnapshotsInTimeRangesResponse.Item.Data);
        return true;
    }

    private async Task<TimeRange> GetCurrentTrendTimeRange(string machineId, CancellationToken cancellationToken)
    {
        var (to, exception) = await _machineTimeService.Get(machineId, cancellationToken);

        if (exception is not null)
        {
            throw exception;
        }

        if (!to.HasValue)
        {
            throw new InternalServiceException(new InternalError(statusCode: 500,
                $"Cannot {nameof(GetCurrentTrendTimeRange)}. Unable to get current machine time of machine {machineId}."));
        }

        var trendTimeSpanDifference = Constants.MachineTrend.TrendTimeSpan.Subtract(TimeSpan.FromMinutes(1));
        var from = to.Value.RoundDown(TimeSpan.FromMinutes(1)).Subtract(trendTimeSpanDifference);

        return new TimeRange(from, to.Value.RoundDown(TimeSpan.FromMinutes(1)));
    }

    private async Task HistoricSnapshotChanged(
        HistoricSnapshotChangeMessage historicSnapshotChangeMessage)
    {
        var machineId = historicSnapshotChangeMessage.MachineId;

        if (!_machineTrendsCache.TryGetValue(machineId, out var machineTrendCache))
            return;

        var changedColumnIds =
            historicSnapshotChangeMessage.AffectedColumnIds.Intersect(Constants.MachineTrend.TrendingSnapshotColumnIds).ToList();
        if (changedColumnIds.Count == 0)
            return;

        var currentTimeRange = await GetCurrentTrendTimeRange(machineId, CancellationToken.None);

        var from = new[] { currentTimeRange.From, historicSnapshotChangeMessage.ChangedFrom }.ToList().Max();
        var to = new[] { currentTimeRange.To, historicSnapshotChangeMessage.ChangedTo }.ToList().Min();
        if (from > to) return;

        await LoadAndUpdateCacheValues(machineTrendCache, new TimeRange(from, to), CancellationToken.None, changedColumnIds);
    }

    private async Task LatestMachineSnapshotCachingServiceOnCacheChanged(
        LiveSnapshotEventArgs machineSnapshotChangedEventArgs,
        CancellationToken cancellationToken)
    {
        var machineId = machineSnapshotChangedEventArgs.MachineId;

        if (!_machineTrendsCache.TryGetValue(machineId, out var machineTrendCache))
            return;

        var snapshotDto = machineSnapshotChangedEventArgs.SnapshotQueueMessageDto;

        if (snapshotDto is null)
        {
            _machineTrendsCache.TryRemove(machineId, out _);
            return;
        }

        if (!machineSnapshotChangedEventArgs.IsMinutelySnapshot)
            return;

        var to = machineSnapshotChangedEventArgs.SnapshotQueueMessageDto!.SnapshotTime;
        var trendTimeSpanDifference = Constants.MachineTrend.TrendTimeSpan.Subtract(TimeSpan.FromMinutes(1));
        var from = to.Subtract(trendTimeSpanDifference);
        var timeRange = new TimeRange(from, to);

        try
        {
            if (!await TryFillLatestGapsInCache(machineTrendCache, timeRange, cancellationToken)) return;
        }
        catch (InternalServiceException)
        {
            return;
        }

        machineTrendCache.UpdateCacheValues([snapshotDto]);
        machineTrendCache.DeleteOldSnapshotsFromCache(timeRange);
    }
}