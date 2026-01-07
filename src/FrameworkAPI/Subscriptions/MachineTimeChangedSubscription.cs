using System;
using System.Net;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FrameworkAPI.Exceptions;
using FrameworkAPI.Models.Events;
using FrameworkAPI.Services.Interfaces;
using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Types;

namespace FrameworkAPI.Subscriptions;

/// <summary>
/// GraphQL subscription for machine entity.
/// </summary>
[ExtendObjectType("Subscription")]
public class MachineTimeChangedSubscription
{
    /// <summary>
    /// Subscribe to the current status of the machine time.
    /// </summary>
    [Authorize(Roles = ["go-general"])]
    [Subscribe(With = nameof(WhenMachineTimeChanged))]
    public DateTime? MachineTimeChanged([EventMessage] DateTime? changedMachineTime)
    {
        return changedMachineTime;
    }

    /// <summary>
    /// Logic needed for the subscription of the machine time.
    /// </summary>
    public IObservable<DateTime?> WhenMachineTimeChanged(
        string machineId,
        [Service] IMachineService machineService,
        [Service] IMachineTimeService machineTimeService)
    {
        var initial = Observable
            .FromAsync(ct => machineTimeService.Get(machineId, ct))
            .Select(dataResult => dataResult.Value);

        var changed = MachineTimeChanged(machineId, machineTimeService);

        return WhenMachineExist(machineId, machineService).SelectMany(initial.Merge(changed));
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

    private static IObservable<DateTime?> MachineTimeChanged(
        string machineId, IMachineTimeService machineTimeService)
    {
        return Observable.Create<DateTime?>(observer =>
        {
            Task ChangedHandler(object? sender, MachineTimeChangedEventArgs args)
            {
                if (args.MachineId == machineId)
                {
                    observer.OnNext(args.MachineTime);
                }

                return Task.CompletedTask;
            }
            machineTimeService.MachineTimeChanged += ChangedHandler;

            return () => machineTimeService.MachineTimeChanged -= ChangedHandler;
        });
    }
}