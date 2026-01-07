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
        PhysicalAssetDataHandler.Client.Models.Dtos.Defect.PhysicalAssetDefectDto>>;

namespace FrameworkAPI.DataLoaders;

public class
    PhysicalAssetDefectsBatchDataLoader : BatchDataLoader<string, DataResult>
{
    private readonly IPhysicalAssetHttpClient _physicalAssetHttpClient;

    public PhysicalAssetDefectsBatchDataLoader(
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
            var getPhysicalAssetDefectsResponse =
                await _physicalAssetHttpClient.GetPhysicalAssetDefects(physicalAssetId, cancellationToken);

            var dataResult = getPhysicalAssetDefectsResponse.HasError &&
                             getPhysicalAssetDefectsResponse.Error.StatusCode != StatusCodes.Status204NoContent
                ? new DataResult(value: null, new InternalServiceException(getPhysicalAssetDefectsResponse.Error))
                : new DataResult(
                    value: getPhysicalAssetDefectsResponse.Items.Count == 0
                        ? null
                        : getPhysicalAssetDefectsResponse.Items, exception: null);

            return new Dictionary<string, DataResult>
            {
                { physicalAssetId, dataResult }
            };
        }

        var getPhysicalAssetsDefectsResponse = await _physicalAssetHttpClient.GetPhysicalAssetsDefects(
                physicalAssetIdsFilter, cancellationToken: cancellationToken);

        if (getPhysicalAssetsDefectsResponse.HasError)
        {
            var dataResult = new DataResult(
                value: null, new InternalServiceException(getPhysicalAssetsDefectsResponse.Error));
            return physicalAssetIdsFilter.ToDictionary(physicalAssetId => physicalAssetId, _ => dataResult);
        }

        return physicalAssetIdsFilter.ToDictionary(
            physicalAssetId => physicalAssetId,
            physicalAssetId =>
            {
                if (!getPhysicalAssetsDefectsResponse.Item.TryGetValue(
                        physicalAssetId, out var physicalAssetDefectDtos))
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