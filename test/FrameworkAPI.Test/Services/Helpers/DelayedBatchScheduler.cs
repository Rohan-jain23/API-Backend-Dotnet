using System;
using System.Threading.Tasks;
using GreenDonut;

namespace FrameworkAPI.Test.Services.Helpers;

public class DelayedBatchScheduler : IBatchScheduler
{
    public void Schedule(Func<ValueTask> dispatch)
    {
        // Missing/not catching exceptions is fine here, because this class is only used in tests
        _ = Task.Run(async () =>
        {
            await Task.Delay(150);
            await dispatch();
        });
    }
}