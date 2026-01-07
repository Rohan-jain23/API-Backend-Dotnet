using System;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.Exceptions;
using GreenDonut;
using Microsoft.Extensions.Logging;
using WuH.Ruby.MaterialDataHandler.Client.HttpClient;
using DataResult = FrameworkAPI.Models.DataResult<WuH.Ruby.MaterialDataHandler.Client.Models.Lot.Lot>;

namespace FrameworkAPI.DataLoaders;

public class MaterialLotCacheDataLoader : CacheDataLoader<string, DataResult>
{
    private readonly IMaterialDataHandlerHttpClient _materialDataHandlerHttpClient;
    private readonly ILogger<MaterialLotCacheDataLoader> _logger;

    public MaterialLotCacheDataLoader(
        IMaterialDataHandlerHttpClient materialDataHandlerHttpClient,
        ILogger<MaterialLotCacheDataLoader> logger,
        DataLoaderOptions? options = null)
        : base(options)
    {
        ArgumentNullException.ThrowIfNull(materialDataHandlerHttpClient);
        _materialDataHandlerHttpClient = materialDataHandlerHttpClient;
        _logger = logger;
    }

    protected override async Task<DataResult> LoadSingleAsync(
        string materialLotId,
        CancellationToken cancellationToken)
    {
        var response = await _materialDataHandlerHttpClient.GetLot(cancellationToken, materialLotId);

        // When a 204 is returned while getting a entity by id its also a failure
        if (response.HasError)
        {
            _logger.LogWarning(
                $"Could not get MaterialLot by MaterialLotId: {materialLotId}. ErrorMessage: {response.Error.ErrorMessage}");
            return new DataResult(value: null, new InternalServiceException(response.Error));
        }

        return new DataResult(response.Item, exception: null);
    }
}