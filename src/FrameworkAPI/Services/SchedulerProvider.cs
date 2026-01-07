using System.Reactive.Concurrency;
using FrameworkAPI.Services.Interfaces;

namespace FrameworkAPI.Services;

/// <summary>
/// We have added this implementation so that we can add a TestScheduler to the service collection. 
/// This is required for integration tests.
/// </summary>
public class SchedulerProvider : ISchedulerProvider
{
    private readonly IScheduler _scheduler = Scheduler.Default;

    public IScheduler GetScheduler()
    {
        return _scheduler;
    }
}