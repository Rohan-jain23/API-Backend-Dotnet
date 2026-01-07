using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.Exceptions;
using FrameworkAPI.Models;
using GreenDonut;
using WuH.Ruby.KpiDataHandler.Client;

namespace FrameworkAPI.DataLoaders;

public class ProductGroupStandardKpiCacheDataLoader : CacheDataLoader
    <(string PaperSackProductGroupId, List<string> MachineIds, DateTime From, DateTime? To, string? ProductIdFilter),
    DataResult<Dictionary<string, PaperSackProductGroupKpis?>>>
{
    private readonly IKpiDataHandlerClient _kpiDataHandlerClient;

    public ProductGroupStandardKpiCacheDataLoader(
        IKpiDataHandlerClient kpiDataHandlerClient,
        DataLoaderOptions? options = null)
        : base(options)
    {
        ArgumentNullException.ThrowIfNull(kpiDataHandlerClient);
        _kpiDataHandlerClient = kpiDataHandlerClient;
    }

    protected override async Task<DataResult<Dictionary<string, PaperSackProductGroupKpis?>>> LoadSingleAsync(
        (string PaperSackProductGroupId, List<string> MachineIds, DateTime From, DateTime? To, string? ProductIdFilter) filter,
        CancellationToken cancellationToken)
    {
        var response = await _kpiDataHandlerClient.GetPaperSackProductGroupKpis(
            cancellationToken, filter.PaperSackProductGroupId, filter.From, filter.To, filter.MachineIds, filter.ProductIdFilter);

        if (response.HasError)
        {
            return new DataResult<Dictionary<string, PaperSackProductGroupKpis?>>(value: null, exception: new InternalServiceException(response.Error));
        }

        return new DataResult<Dictionary<string, PaperSackProductGroupKpis?>>(value: response.Item, exception: null);
    }
}