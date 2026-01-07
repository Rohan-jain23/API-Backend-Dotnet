using System.Reactive.Concurrency;

namespace FrameworkAPI.Services.Interfaces;

public interface ISchedulerProvider
{
    IScheduler GetScheduler();
}