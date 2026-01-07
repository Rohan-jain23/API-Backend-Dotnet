using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.Exceptions;
using FrameworkAPI.Models;
using FrameworkAPI.Models.Events;
using FrameworkAPI.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Threading;
using WuH.Ruby.MachineSnapShooter.Client;

namespace FrameworkAPI.Services;

public class MachineTimeService : IMachineTimeService, IDisposable
{
    private readonly IOpcUaServerTimeCachingService _opcUaServerTimeCachingService;
    private readonly ILatestMachineSnapshotCachingService _latestMachineSnapshotCachingService;
    private readonly ILogger<MachineTimeService> _logger;

    public MachineTimeService(
        IOpcUaServerTimeCachingService opcUaServerTimeCachingService,
        ILatestMachineSnapshotCachingService latestMachineSnapshotCachingService,
        ILogger<MachineTimeService> logger)
    {
        _opcUaServerTimeCachingService = opcUaServerTimeCachingService;
        _latestMachineSnapshotCachingService = latestMachineSnapshotCachingService;
        _logger = logger;

        _opcUaServerTimeCachingService.CacheChanged += OpcUaServerTimeCachingServiceOnCacheChanged;
        _latestMachineSnapshotCachingService.CacheChanged += LatestMachineSnapshotCachingServiceOnCacheChanged;
    }

    public event AsyncEventHandler<MachineTimeChangedEventArgs>? MachineTimeChanged;

    public async Task<DataResult<DateTime?>> Get(string machineId, CancellationToken cancellationToken)
    {
        var (machineTime1, exception) = await _opcUaServerTimeCachingService.Get(machineId, cancellationToken);
        var response =
            await _latestMachineSnapshotCachingService.GetLatestMachineSnapshot(machineId, cancellationToken);

        if (exception is not null && response.HasError)
        {
            return new DataResult<DateTime?>(
                value: null,
                new InternalServiceException(
                    new AggregateException(exception, response.Error.Exception).Message,
                    (int)HttpStatusCode.InternalServerError));
        }

        var machineTime2 = response.Item?.Data?.SnapshotTime;

        var machineTime = new[] { machineTime1, machineTime2 }.Max();

        return new DataResult<DateTime?>(machineTime, exception: null);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _opcUaServerTimeCachingService.CacheChanged -= OpcUaServerTimeCachingServiceOnCacheChanged;
        _latestMachineSnapshotCachingService.CacheChanged -= LatestMachineSnapshotCachingServiceOnCacheChanged;
    }

    private async Task OpcUaServerTimeCachingServiceOnCacheChanged(object? sender, MachineTimeChangedEventArgs e)
    {
        await InvokeMachineTimeChangedEvent(e.MachineId);
    }

    private void LatestMachineSnapshotCachingServiceOnCacheChanged(
        object? sender, LiveSnapshotEventArgs machineSnapshotChangedEventArgs)
    {
        _ = InvokeMachineTimeChangedEvent(machineSnapshotChangedEventArgs.MachineId).ContinueWith(
            task =>
                _logger.LogWarning(task.Exception, "Invoking a machine time changed event failed!"),
            TaskContinuationOptions.OnlyOnFaulted);
    }

    private async Task InvokeMachineTimeChangedEvent(string machineId)
    {
        var (machineTime, exception) = await Get(machineId, CancellationToken.None);

        if (exception is not null)
        {
            throw exception;
        }

        var invokeTask =
            MachineTimeChanged?.InvokeAsync(this, new MachineTimeChangedEventArgs(machineId, machineTime));

        if (invokeTask is not null)
        {
            await invokeTask;
        }
    }
}