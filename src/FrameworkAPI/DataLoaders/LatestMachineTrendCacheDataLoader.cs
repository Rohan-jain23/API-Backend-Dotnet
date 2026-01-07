using System;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.Services.Interfaces;
using GreenDonut;
using DataResult = FrameworkAPI.Models.DataResult<System.Collections.Generic.IReadOnlyDictionary<
    System.DateTime, System.Collections.Generic.IReadOnlyDictionary<string, double?>?>>;

namespace FrameworkAPI.DataLoaders;

public class LatestMachineTrendCacheDataLoader : CacheDataLoader<string, DataResult>
{
    private readonly IMachineTrendCachingService _machineTrendCachingService;

    public LatestMachineTrendCacheDataLoader(IMachineTrendCachingService machineTrendCachingService)
    {
        ArgumentNullException.ThrowIfNull(machineTrendCachingService);
        _machineTrendCachingService = machineTrendCachingService;
    }

    protected override async Task<DataResult> LoadSingleAsync(
        string machineId, CancellationToken cancellationToken)
    {
        try
        {
            var machineTrend = await _machineTrendCachingService.Get(machineId, cancellationToken);

            return new DataResult(machineTrend, exception: null);
        }
        catch (Exception ex)
        {
            return new DataResult(value: null, ex);
        }
    }
}