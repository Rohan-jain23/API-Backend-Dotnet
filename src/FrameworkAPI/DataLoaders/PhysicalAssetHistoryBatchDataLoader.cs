using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.Exceptions;
using GreenDonut;
using Microsoft.AspNetCore.Http;
using PhysicalAssetDataHandler.Client.HttpClients;
using DataResult =
    FrameworkAPI.Models.DataResult<System.Collections.Generic.IEnumerable<
        PhysicalAssetDataHandler.Client.Models.Dtos.History.PhysicalAssetHistoryItemDto>>;

namespace FrameworkAPI.DataLoaders;

public class
    PhysicalAssetHistoryBatchDataLoader : BatchDataLoader<string, DataResult>
{
    private readonly IPhysicalAssetHttpClient _physicalAssetHttpClient;

    public PhysicalAssetHistoryBatchDataLoader(
        IPhysicalAssetHttpClient physicalAssetHttpClient,
        IBatchScheduler batchScheduler,
        DataLoaderOptions? options = null)
        : base(batchScheduler, options)
    {
        ArgumentNullException.ThrowIfNull(physicalAssetHttpClient);
        _physicalAssetHttpClient = physicalAssetHttpClient;
    }

    protected override async Task<IReadOnlyDictionary<string, DataResult>>
        LoadBatchAsync(IReadOnlyList<string> physicalAssetIdsFilter, CancellationToken cancellationToken)
    {
        if (physicalAssetIdsFilter.Count == 1)
        {
            var physicalAssetId = physicalAssetIdsFilter[0];
            var response =
                await _physicalAssetHttpClient.GetPhysicalAssetHistory(physicalAssetId, cancellationToken);

            var dataResult = response.HasError && response.Error.StatusCode != StatusCodes.Status204NoContent
                ? new DataResult(value: null, new InternalServiceException(response.Error))
                : new DataResult(value: response.Items.Count == 0 ? null : response.Items, exception: null);

            return new Dictionary<string, DataResult>
            {
                { physicalAssetId, dataResult }
            };
        }
        else
        {
            var response = await _physicalAssetHttpClient.GetPhysicalAssetsHistory(
                    physicalAssetIdsFilter, cancellationToken: cancellationToken);

            if (response.HasError)
            {
                var dataResult = new DataResult(value: null, new InternalServiceException(response.Error));
                return physicalAssetIdsFilter.ToDictionary(physicalAssetId => physicalAssetId, _ => dataResult);
            }

            return physicalAssetIdsFilter.ToDictionary(
                physicalAssetId => physicalAssetId,
                physicalAssetId =>
                {
                    if (!response.Item.TryGetValue(physicalAssetId, out var physicalAssetDefectDtos))
                    {
                        return new DataResult(value: null, exception: null);
                    }

                    var physicalAssetDefectDtosAsList = physicalAssetDefectDtos.ToList();
                    var value = physicalAssetDefectDtosAsList.Count == 0 ? null : physicalAssetDefectDtosAsList;
                    return new DataResult(value, exception: null);
                }
            );
        }
    }
}