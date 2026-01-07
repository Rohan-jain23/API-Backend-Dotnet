using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.Exceptions;
using FrameworkAPI.Models;
using FrameworkAPI.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using WuH.Ruby.ProductionPeriodsDataHandler.Client;

namespace FrameworkAPI.Services;

public class JobInfoCachingService(
    IMachineTimeService machineTimeService,
    IProductionPeriodsDataHandlerHttpClient productionPeriodsDataHandlerHttpClient,
    IProductionPeriodChangesQueueWrapper productionPeriodChangesQueueWrapper,
    ILogger<JobInfoCachingService> logger) : IJobInfoCachingService, IDisposable
{
    private readonly ConcurrentDictionary<string, DataResult<JobInfo?>?> _jobInfoCache = new();

    private readonly IMachineTimeService _machineTimeService = machineTimeService;
    private readonly IProductionPeriodsDataHandlerHttpClient _productionPeriodsDataHandlerHttpClient = productionPeriodsDataHandlerHttpClient;
    private readonly IProductionPeriodChangesQueueWrapper _productionPeriodChangesQueueWrapper = productionPeriodChangesQueueWrapper;
    private readonly ILogger<JobInfoCachingService> _logger = logger;
    private readonly CancellationTokenSource _cancellationTokenForCallbacks = new();
    private readonly List<IDisposable> _subscriptionDisposables = [];

    public void Dispose()
    {
        _cancellationTokenForCallbacks.Cancel();
        foreach (var disposable in _subscriptionDisposables)
        {
            disposable.Dispose();
        }
    }

    public async Task<DataResult<JobInfo?>> GetLatest(string machineId, CancellationToken cancellationToken)
    {
        var cachedValue = _jobInfoCache.GetOrAdd(machineId, _ =>
        {
            var disposable = _productionPeriodChangesQueueWrapper.SubscribeForPeriodChanges(
                machineId,
                (_, machineId, periods, _) => OnProductionPeriodChanged(machineId, periods, _cancellationTokenForCallbacks.Token));
            _subscriptionDisposables.Add(disposable);
            return null;
        });

        if (cachedValue is not null)
        {
            var cachedJobIsActive = cachedValue.Value?.EndTime is null;
            return cachedJobIsActive
                ? await ChangeCurrentTimeRange(machineId, cachedValue, cancellationToken)
                : cachedValue;
        }

        var response = await _productionPeriodsDataHandlerHttpClient.GetLatestJobs(
            cancellationToken,
            [machineId],
            0,
            1);

        if (response.HasError && response.Error.StatusCode != StatusCodes.Status204NoContent)
        {
            _logger.LogWarning($"Could not get latest job by MachineId: {machineId}. ErrorMessage: {response.Error.ErrorMessage}");

            return new DataResult<JobInfo?>(value: null, new InternalServiceException(response.Error));
        }

        var dataResult = new DataResult<JobInfo?>(response.Items?.FirstOrDefault(), exception: null);
        UpdateCache(machineId, dataResult);

        var refreshResult = await ChangeCurrentTimeRange(machineId, dataResult, cancellationToken);

        return refreshResult;
    }

    private Task OnProductionPeriodChanged(string machineId, List<CombinedProductionPeriod> periods, CancellationToken _)
    {
        if (!periods.Any() || !_jobInfoCache.ContainsKey(machineId))
        {
            return Task.CompletedTask;
        }

        UpdateCache(machineId, null);

        return Task.CompletedTask;
    }

    private void UpdateCache(string machineId, DataResult<JobInfo?>? result)
    {
        _jobInfoCache.AddOrUpdate(machineId, _ => result, (_, _) => result);
    }

    private async Task<DataResult<JobInfo?>> ChangeCurrentTimeRange(
        string machineId, DataResult<JobInfo?> dataResult, CancellationToken cancellationToken)
    {
        if (dataResult.Value?.EndTime is not null || dataResult.Value?.TimeRanges?.Any() != true)
        {
            return dataResult;
        }

        var (machineTime, _) = await _machineTimeService.Get(machineId, cancellationToken);
        if (!machineTime.HasValue)
        {
            return dataResult;
        }

        var maxTo = dataResult.Value!.TimeRanges.Max(timeRange => timeRange.To);
        dataResult.Value.TimeRanges.First(timeRange => timeRange.To == maxTo).To = machineTime.Value;

        return dataResult;
    }
}