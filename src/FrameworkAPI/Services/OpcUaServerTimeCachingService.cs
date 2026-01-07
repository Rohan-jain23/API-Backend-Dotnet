using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.Exceptions;
using FrameworkAPI.Models;
using FrameworkAPI.Models.Events;
using FrameworkAPI.Services.Interfaces;
using Microsoft.VisualStudio.Threading;
using WuH.Ruby.OpcUaForwarder.Client;
using WuH.Ruby.ProcessDataReader.Client;

namespace FrameworkAPI.Services;

public class OpcUaServerTimeCachingService(
    IProcessDataReaderHttpClient processDataReaderClient, IProcessDataQueueWrapper processDataQueueWrapper) : IOpcUaServerTimeCachingService, IDisposable
{
    private readonly ConcurrentDictionary<string, DateTime?> _opcUaServerTimesCache = new();
    private readonly List<IDisposable> _subscriptionDisposables = [];
    private readonly IProcessDataReaderHttpClient _processDataReaderClient = processDataReaderClient;
    private readonly IProcessDataQueueWrapper _processDataQueueWrapper = processDataQueueWrapper;

    public event AsyncEventHandler<MachineTimeChangedEventArgs>? CacheChanged;

    public void Dispose()
    {
        _subscriptionDisposables.ForEach(x => x.Dispose());
    }

    public async Task<DataResult<DateTime?>> Get(string machineId, CancellationToken cancellationToken)
    {
        var cachedOpcUaServerTime = _opcUaServerTimesCache.GetOrAdd(machineId, _ =>
        {
            var disposable = _processDataQueueWrapper.SubscribeForOpcUaServerTime(
                machineId,
                OnOpcUaServerTimeChanged);
            _subscriptionDisposables.Add(disposable);
            return null;
        });

        if (cachedOpcUaServerTime is not null)
        {
            return new DataResult<DateTime?>(cachedOpcUaServerTime, exception: null);
        }

        var response = await _processDataReaderClient.GetLastReceivedOpcUaServerTime(cancellationToken, machineId);

        if (response.HasError)
        {
            return new DataResult<DateTime?>(value: null, new InternalServiceException(response.Error));
        }

        var opcUaServerTime = response.Item;
        var dataResult = new DataResult<DateTime?>(opcUaServerTime, exception: null);

        if (!_opcUaServerTimesCache.TryUpdate(machineId, opcUaServerTime, comparisonValue: null))
        {
            return dataResult;
        }

        var cacheChangedTask =
            CacheChanged?.InvokeAsync(this, new MachineTimeChangedEventArgs(machineId, opcUaServerTime));
        if (cacheChangedTask is not null)
        {
            await cacheChangedTask;
        }

        return dataResult;
    }

    private async Task OnOpcUaServerTimeChanged(string machineId, DateTime? opcUaServerTime)
    {
        var cachedOpcUaServerTime = _opcUaServerTimesCache.AddOrUpdate(
            machineId,
            addValueFactory: _ => null,
            updateValueFactory: (_, _) => opcUaServerTime);

        if (cachedOpcUaServerTime is not null)
        {
            var cacheChangedTask = CacheChanged?.InvokeAsync(this,
                new MachineTimeChangedEventArgs(machineId, cachedOpcUaServerTime));
            if (cacheChangedTask is not null)
            {
                await cacheChangedTask;
            }
        }
    }
}