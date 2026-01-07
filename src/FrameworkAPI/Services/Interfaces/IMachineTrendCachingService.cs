using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FrameworkAPI.Services.Interfaces;

public interface IMachineTrendCachingService
{
    Task<IReadOnlyDictionary<DateTime, IReadOnlyDictionary<string, double?>?>?> Get(
        string machineId, CancellationToken cancellationToken);
}