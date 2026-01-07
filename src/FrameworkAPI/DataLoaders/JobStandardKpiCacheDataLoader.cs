using System;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.Exceptions;
using FrameworkAPI.Models;
using GreenDonut;
using WuH.Ruby.KpiDataHandler.Client;

namespace FrameworkAPI.DataLoaders;

public class JobStandardKpiCacheDataLoader : CacheDataLoader<(string MachineId, string JobId), DataResult<StandardJobKpis>>
{
    private readonly IKpiDataCachingService _kpiDataCachingService;

    public JobStandardKpiCacheDataLoader(
        IKpiDataCachingService kpiDataCachingService,
        DataLoaderOptions? options = null)
        : base(options)
    {
        ArgumentNullException.ThrowIfNull(kpiDataCachingService);
        _kpiDataCachingService = kpiDataCachingService;
    }

    protected override async Task<DataResult<StandardJobKpis>> LoadSingleAsync(
        (string MachineId, string JobId) key, CancellationToken cancellationToken)
    {
        var response = await _kpiDataCachingService.GetStandardKpis(key.MachineId, key.JobId, cancellationToken);

        if (response.HasError)
        {
            return new DataResult<StandardJobKpis>(value: null, new InternalServiceException(response.Error));
        }

        return new DataResult<StandardJobKpis>(value: response.Item, exception: null);
    }
}