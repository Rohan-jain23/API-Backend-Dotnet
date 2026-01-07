using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.Exceptions;
using FrameworkAPI.Models;
using FrameworkAPI.Services.Interfaces;
using Microsoft.Extensions.Logging;
using WuH.Ruby.MachineSnapShooter.Client;
using WuH.Ruby.MachineSnapShooter.Client.Enums;
using WuH.Ruby.MachineSnapShooter.Client.Models;
using WuH.Ruby.MachineSnapShooter.Client.Queue;

namespace FrameworkAPI.Services;

public class SnapshotColumnValueChangedTimestampCachingService : ISnapshotColumnIdChangedTimestampCachingService, IDisposable
{
    private readonly ConcurrentDictionary<string, SnapshotColumnValueChangeTimestampCache>
        _machineSnapshotColumnIdChangedTimestampsCache = new();

    private readonly ILatestMachineSnapshotCachingService _latestMachineSnapshotCachingService;
    private readonly IMachineSnapshotHttpClient _machineSnapshotHttpClient;
    private readonly IMachineSnapshotQueueWrapper _machineSnapshotQueueWrapper;
    private readonly ILogger<SnapshotColumnValueChangedTimestampCachingService> _logger;
    private readonly IDisposable _subscriptionDisposable;
    private readonly IDisposable _snapshotSubscriptionDisposable;

    public SnapshotColumnValueChangedTimestampCachingService(
        ILatestMachineSnapshotCachingService latestMachineSnapshotCachingService,
        IMachineSnapshotHttpClient machineSnapshotHttpClient,
        IMachineSnapshotQueueWrapper machineSnapshotQueueWrapper,
        ILogger<SnapshotColumnValueChangedTimestampCachingService> logger)
    {
        _latestMachineSnapshotCachingService = latestMachineSnapshotCachingService;
        _machineSnapshotHttpClient = machineSnapshotHttpClient;
        _machineSnapshotQueueWrapper = machineSnapshotQueueWrapper;
        _logger = logger;

        _snapshotSubscriptionDisposable = Observable.FromEventPattern<LiveSnapshotEventArgs>(
                handler => _latestMachineSnapshotCachingService.CacheChanged += handler,
                handler => _latestMachineSnapshotCachingService.CacheChanged -= handler)
            .Subscribe(ev => LatestMachineSnapshotCachingServiceOnCacheChanged(ev.EventArgs));
        _subscriptionDisposable = _machineSnapshotQueueWrapper.SubscribeForHistoricSnapshotChangeMessage(
            WhenHistoricSnapshotChangeMessageReceived);
    }

    public void Dispose()
    {
        _subscriptionDisposable.Dispose();
        _snapshotSubscriptionDisposable.Dispose();
        GC.SuppressFinalize(this);
    }

    public async Task<DateTime?> Get(
        string machineId, string columnId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(machineId))
        {
            throw new ArgumentNullException(nameof(columnId));
        }

        if (string.IsNullOrWhiteSpace(columnId) || !SnapshotColumnIds.IsSnapshotColumnId(columnId))
        {
            throw new ArgumentException($"{nameof(columnId)} \"{columnId}\" is no valid column id.");
        }

        var cache = _machineSnapshotColumnIdChangedTimestampsCache.GetOrAdd(
            machineId,
            _ => new SnapshotColumnValueChangeTimestampCache(machineId));

        if (cache.TryGetValue(columnId, out var lastChangeTimestamp)) return lastChangeTimestamp;

        return await InitializeCache(cache, columnId, cancellationToken);
    }

    private async Task<DateTime?> InitializeCache(SnapshotColumnValueChangeTimestampCache cache, string columnId, CancellationToken cancellationToken)
    {
        var getLatestSnapshotColumnValueChangedTimestampResponse =
            await _machineSnapshotHttpClient.GetLatestSnapshotColumnValueChangedTimestamp(
                cache.MachineId, columnId, cancellationToken);

        if (getLatestSnapshotColumnValueChangedTimestampResponse.HasError)
        {
            var successful = Enum.TryParse(getLatestSnapshotColumnValueChangedTimestampResponse.Error.ErrorItem?.ToString(), out MachineSnapshotErrorItemType errorType);
            if (successful && errorType is MachineSnapshotErrorItemType.WaitingForFirstMinutelySnapshot)
            {
                return null;
            }
            throw new InternalServiceException(getLatestSnapshotColumnValueChangedTimestampResponse.Error);
        }

        var changedTimestamp = getLatestSnapshotColumnValueChangedTimestampResponse.Item.ValueEqualSince;
        var changedValue = getLatestSnapshotColumnValueChangedTimestampResponse.Item.ColumnValue;
        var latestSnapshotTime = getLatestSnapshotColumnValueChangedTimestampResponse.Item.LatestSnapshotTime;
        return cache.InitializeValueForColumnId(columnId, changedValue, changedTimestamp, latestSnapshotTime);
    }

    private void LatestMachineSnapshotCachingServiceOnCacheChanged(LiveSnapshotEventArgs machineSnapshotChangedEventArgs)
    {
        var cache = _machineSnapshotColumnIdChangedTimestampsCache.GetOrAdd(
            machineSnapshotChangedEventArgs.MachineId,
            machineId => new SnapshotColumnValueChangeTimestampCache(machineId));

        if (machineSnapshotChangedEventArgs.SnapshotQueueMessageDto is null)
        {
            cache.Clear();
            return;
        }

        IntegrateNewSnapshotMessage(
            cache,
            machineSnapshotChangedEventArgs.SnapshotQueueMessageDto,
            machineSnapshotChangedEventArgs.IsMinutelySnapshot);
    }

    private static void IntegrateNewSnapshotMessage(
        SnapshotColumnValueChangeTimestampCache cache,
        SnapshotQueueMessageDto snapshotQueueMessage,
        bool isMinutelySnapshot)
    {
        foreach (var columnId in cache.ColumnIds)
        {
            var snapshotColumn = snapshotQueueMessage.ColumnValues.FirstOrDefault(columnValue => columnValue.Id == columnId);
            if (snapshotColumn is null) return;

            cache.UpdateLiveValueForColumnId(columnId, snapshotColumn.Value, snapshotQueueMessage.SnapshotTime, isMinutelySnapshot);
        }
    }

    private Task WhenHistoricSnapshotChangeMessageReceived(HistoricSnapshotChangeMessage message)
    {
        var success = _machineSnapshotColumnIdChangedTimestampsCache.TryRemove(message.MachineId, out var _);
        _logger.LogDebug($"{message.MachineId}: Clear {nameof(SnapshotColumnValueChangedTimestampCachingService)} for machine. Success: {success}.");
        return Task.CompletedTask;
    }
}