using System;
using System.Collections.Concurrent;
using System.Reactive.Linq;
using System.Threading.Tasks;
using WuH.Ruby.KpiDataHandler.Client;

namespace FrameworkAPI.Services;

/// <summary>
/// This service ensures that only one KPI queue subscription is maintained per machine.
/// </summary>
public class StandardKpiChangesService(IKpiChangesQueueWrapper kpiChangesQueueWrapper) : IStandardKpiChangesService
{
    private readonly ConcurrentDictionary<string, IObservable<string>> _machineObservables = new();

    /// <summary>
    /// Emits the machineId when 
    /// </summary>
    public IObservable<string> WhenMachineStandardKpisChanged(string machineId)
    {
        return GetObservable(machineId);
    }

    /// <summary>
    /// Ensure a shared subscription for each machine that only connects when there are active subscribers.
    /// </summary>
    private IObservable<string> GetObservable(string machineId)
    {
        return _machineObservables.GetOrAdd(
            machineId,
            _ => Observable
                .Create<string>(
                    observer => kpiChangesQueueWrapper.SubscribeForKpiChangesOfActiveJob(
                            machineId,
                            (_, _, _) =>
                            {
                                observer.OnNext(machineId);
                                return Task.CompletedTask;
                            }))
                .Publish()
                .RefCount());
    }
}