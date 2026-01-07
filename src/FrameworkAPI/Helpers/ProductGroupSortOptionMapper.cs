using KpiDataHandlerModels = WuH.Ruby.KpiDataHandler.Client.Models;
using FrameworkAPI.Schema.ProductGroup;

namespace FrameworkAPI.Helpers;

internal static class ProductGroupSortOptionMapper
{
    internal static ProductGroupSortOption MapToSchemaProductGroupSortOption(this KpiDataHandlerModels.ProductGroupSortOption productGroupSortOption)
    => productGroupSortOption switch
    {
        KpiDataHandlerModels.ProductGroupSortOption.None => ProductGroupSortOption.None,
        KpiDataHandlerModels.ProductGroupSortOption.IdAscending => ProductGroupSortOption.IdAscending,
        KpiDataHandlerModels.ProductGroupSortOption.IdDescending => ProductGroupSortOption.IdDescending,
        KpiDataHandlerModels.ProductGroupSortOption.FriendlyNameAscending => ProductGroupSortOption.FriendlyNameAscending,
        KpiDataHandlerModels.ProductGroupSortOption.FriendlyNameDescending => ProductGroupSortOption.FriendlyNameDescending,
        KpiDataHandlerModels.ProductGroupSortOption.FirstProductionDateAscending => ProductGroupSortOption.FirstProductionDateAscending,
        KpiDataHandlerModels.ProductGroupSortOption.FirstProductionDateDescending => ProductGroupSortOption.FirstProductionDateDescending,
        KpiDataHandlerModels.ProductGroupSortOption.LastProductionDateAscending => ProductGroupSortOption.LastProductionDateAscending,
        KpiDataHandlerModels.ProductGroupSortOption.LastProductionDateDescending => ProductGroupSortOption.LastProductionDateDescending,
        _ => ProductGroupSortOption.None
    };

    internal static KpiDataHandlerModels.ProductGroupSortOption MapToInternalProductGroupSortOption(this ProductGroupSortOption productGroupSortOption)
    => productGroupSortOption switch
    {
        ProductGroupSortOption.None => KpiDataHandlerModels.ProductGroupSortOption.None,
        ProductGroupSortOption.IdAscending => KpiDataHandlerModels.ProductGroupSortOption.IdAscending,
        ProductGroupSortOption.IdDescending => KpiDataHandlerModels.ProductGroupSortOption.IdDescending,
        ProductGroupSortOption.FriendlyNameAscending => KpiDataHandlerModels.ProductGroupSortOption.FriendlyNameAscending,
        ProductGroupSortOption.FriendlyNameDescending => KpiDataHandlerModels.ProductGroupSortOption.FriendlyNameDescending,
        ProductGroupSortOption.FirstProductionDateAscending => KpiDataHandlerModels.ProductGroupSortOption.FirstProductionDateAscending,
        ProductGroupSortOption.FirstProductionDateDescending => KpiDataHandlerModels.ProductGroupSortOption.FirstProductionDateDescending,
        ProductGroupSortOption.LastProductionDateAscending => KpiDataHandlerModels.ProductGroupSortOption.LastProductionDateAscending,
        ProductGroupSortOption.LastProductionDateDescending => KpiDataHandlerModels.ProductGroupSortOption.LastProductionDateDescending,
        _ => KpiDataHandlerModels.ProductGroupSortOption.None
    };
}