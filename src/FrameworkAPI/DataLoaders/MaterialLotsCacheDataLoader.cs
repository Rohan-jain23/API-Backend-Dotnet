using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.Exceptions;
using FrameworkAPI.Models;
using GreenDonut;
using Microsoft.Extensions.Logging;
using WuH.Ruby.MaterialDataHandler.Client.HttpClient;
using WuH.Ruby.MaterialDataHandler.Client.Models.Lot;
using DataResult = FrameworkAPI.Models.DataResult<
    (System.Collections.Generic.List<WuH.Ruby.MaterialDataHandler.Client.Models.Lot.Lot> Result, int Count)?>;

namespace FrameworkAPI.DataLoaders;

public class MaterialLotsCacheDataLoader : CacheDataLoader<MaterialLotsFilter, DataResult>
{
    private readonly IMaterialDataHandlerHttpClient _materialDataHandlerHttpClient;
    private readonly ILogger<MaterialLotsCacheDataLoader> _logger;

    public MaterialLotsCacheDataLoader(
        IMaterialDataHandlerHttpClient materialDataHandlerHttpClient,
        ILogger<MaterialLotsCacheDataLoader> logger,
        DataLoaderOptions? options = null)
        : base(options)
    {
        ArgumentNullException.ThrowIfNull(materialDataHandlerHttpClient);
        _materialDataHandlerHttpClient = materialDataHandlerHttpClient;
        _logger = logger;
    }

    protected override async Task<DataResult> LoadSingleAsync(
        MaterialLotsFilter filter,
        CancellationToken cancellationToken)
    {
        var result = new List<Lot>();
        var totalCount = 0;

        // ToDo: Change MaterialDataHandler to also accept list of machineIds and move pagination into the MaterialDataHandler. 
        //       This implementation is ineffective and will result in long waiting times if lots are requested from the last pages.
        foreach (var machineIdFilter in filter.MachineIdsFilter)
        {
            var response = await _materialDataHandlerHttpClient.FindLots(
                cancellationToken,
                filter.RegexFilter,
                machineIdFilter,
                filter.From,
                filter.To,
                motherLotsOnly: false,
                maxResults: filter.Take + filter.Skip);

            if (response.HasError && response.Error.StatusCode == 204)
            {
                _logger.LogDebug("No lots for the given filter");
                continue;
            }

            if (response.HasError)
            {
                _logger.LogWarning(
                    $"{nameof(WuH.Ruby.MaterialDataHandler)}.{nameof(IMaterialDataHandlerHttpClient.FindLots)} failed with StatusCode: {response.Error.StatusCode}. ErrorMessage: {response.Error.ErrorMessage}");

                return new DataResult(value: null, new InternalServiceException(response.Error));
            }

            var countResponse = await _materialDataHandlerHttpClient.GetLotCount(cancellationToken, machineIdFilter);

            if (countResponse.HasError)
            {
                _logger.LogWarning(
                    $"{nameof(WuH.Ruby.MaterialDataHandler)}.{nameof(IMaterialDataHandlerHttpClient.GetLotCount)} failed with StatusCode: {countResponse.Error.StatusCode}. ErrorMessage: {countResponse.Error.ErrorMessage}");

                return new DataResult(value: null, new InternalServiceException(response.Error));
            }

            result.AddRange(response.Items);
            totalCount += countResponse.Item;
        }

        var sortedLots = result
            .OrderByDescending(x => x.GeneralProperties.StartTime)
            .Skip(filter.Skip)
            .Take(filter.Take)
            .ToList();

        return new DataResult((sortedLots, totalCount), exception: null);
    }
}