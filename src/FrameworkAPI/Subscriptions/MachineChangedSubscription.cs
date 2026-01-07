using System;
using System.Net;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.Exceptions;
using FrameworkAPI.Schema.Machine;
using FrameworkAPI.Services;
using FrameworkAPI.Services.Interfaces;
using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Types;
using WuH.Ruby.AlarmDataHandler.Client;
using WuH.Ruby.MachineSnapShooter.Client;

namespace FrameworkAPI.Subscriptions;

/// <summary>
/// GraphQL subscription for machine entity.
/// </summary>
[ExtendObjectType("Subscription")]
public class MachineChangedSubscription
{
    /// <summary>
    /// Subscribe to the current status of the whole machine entity (this will not update, if the machine time changes).
    /// </summary>
    [Authorize(Roles = ["go-general"])]
    [Subscribe(With = nameof(WhenMachineChanged))]
    public async Task<Machine> MachineChanged(
        [EventMessage] string machineId,
        [Service] IMachineService machineService,
        CancellationToken cancellationToken)
    {
        return await machineService.GetMachine(machineId, cancellationToken);
    }

    /// <summary>
    /// Logic needed for the subscription of the whole machine entity.
    /// </summary>
    public IObservable<string> WhenMachineChanged(
        string machineId,
        [Service] IMachineService machineService,
        [Service] ILatestMachineSnapshotCachingService latestMachineSnapshotCachingService,
        [Service] IAlarmDataHandlerCachingService alarmDataHandlerCachingService,
        [Service] IStandardKpiChangesService standardKpiChangesService,
        [Service] ISchedulerProvider scheduler)
    {
        // SnapShooter and KpiDataHandler both send minutely updates. Therefore use a throttle here to not send the minutely update twice.
        return WhenMachineExist(machineId, machineService)
            .SelectMany(_ => Observable.Merge(
                WhenLatestSnapshotCacheChanged(machineId, latestMachineSnapshotCachingService),
                standardKpiChangesService.WhenMachineStandardKpisChanged(machineId),
                WhenActiveAlarmsCacheChanged(machineId, alarmDataHandlerCachingService)))
            .Throttle(TimeSpan.FromSeconds(1), scheduler.GetScheduler())
            .StartWith(machineId);
    }

    private static IObservable<Unit> WhenMachineExist(string machineId, IMachineService machineService)
    {
        return Observable.FromAsync(async cancellationToken =>
        {
            var isExisting = await machineService.DoesMachineExist(machineId, cancellationToken);

            if (!isExisting)
            {
                throw new InternalServiceException(
                    $"{nameof(MachineChangedSubscription)}: MachineId '{machineId}' does not exist",
                    (int)HttpStatusCode.BadRequest);
            }

            return new Unit();
        });
    }

    private static IObservable<string> WhenLatestSnapshotCacheChanged(
        string machineId, [Service] ILatestMachineSnapshotCachingService latestMachineSnapshotCachingService)
    {
        return Observable
            .FromEventPattern<LiveSnapshotEventArgs>(
                eventHandler => latestMachineSnapshotCachingService.CacheChanged += eventHandler,
                eventHandler => latestMachineSnapshotCachingService.CacheChanged -= eventHandler)
            .Where(eventPattern => eventPattern.EventArgs.MachineId == machineId)
            .Select(eventPattern => eventPattern.EventArgs.MachineId);
    }

    private static IObservable<string> WhenActiveAlarmsCacheChanged(
        string machineId, [Service] IAlarmDataHandlerCachingService alarmDataHandlerCachingService)
    {
        return Observable
            .FromEventPattern<ActiveAlarmsChangedEventArgs>(
                eventHandler => alarmDataHandlerCachingService.CacheChanged += eventHandler,
                eventHandler => alarmDataHandlerCachingService.CacheChanged -= eventHandler)
            .Where(eventPattern => eventPattern.EventArgs.MachineId == machineId)
            .Select(eventPattern => eventPattern.EventArgs.MachineId);
    }
}