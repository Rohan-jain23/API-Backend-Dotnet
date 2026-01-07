using System;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.Exceptions;
using GreenDonut;
using WuH.Ruby.ProductionPeriodsDataHandler.Client;
using DataResult = FrameworkAPI.Models.DataResult<WuH.Ruby.ProductionPeriodsDataHandler.Client.ProductionPeriodResponseItem>;

namespace FrameworkAPI.DataLoaders;

public class ProductionPeriodByTimestampCacheDataLoader(IProductionPeriodsDataHandlerHttpClient productionPeriodsDataHandlerHttpClient)
    : CacheDataLoader<(string machineId, DateTime timestamp), DataResult>
{

    protected override async Task<DataResult> LoadSingleAsync((string machineId, DateTime timestamp) request, CancellationToken cancellationToken)
    {
        var response = await productionPeriodsDataHandlerHttpClient.GetPeriodByTimestamp(cancellationToken, request.machineId, request.timestamp);

        if (response.HasError)
        {
            return new DataResult(null, new InternalServiceException(response.Error));
        }

        return new DataResult(response.Item, null);
    }
}