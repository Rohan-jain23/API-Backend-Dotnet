using FluentAssertions;
using FrameworkAPI.Helpers;
using FrameworkAPI.Schema.ProductGroup;
using Xunit;
using KpiDataHandlerModels = WuH.Ruby.KpiDataHandler.Client.Models;

namespace FrameworkAPI.Test.Helpers;

public class ProductGroupSortOptionMapperTests
{
    public ProductGroupSortOptionMapperTests() { }

    [Theory]
    [InlineData(KpiDataHandlerModels.ProductGroupSortOption.FirstProductionDateAscending, ProductGroupSortOption.FirstProductionDateAscending)]
    [InlineData(KpiDataHandlerModels.ProductGroupSortOption.FirstProductionDateDescending, ProductGroupSortOption.FirstProductionDateDescending)]
    [InlineData(KpiDataHandlerModels.ProductGroupSortOption.FriendlyNameAscending, ProductGroupSortOption.FriendlyNameAscending)]
    [InlineData(KpiDataHandlerModels.ProductGroupSortOption.FriendlyNameDescending, ProductGroupSortOption.FriendlyNameDescending)]
    [InlineData(KpiDataHandlerModels.ProductGroupSortOption.IdAscending, ProductGroupSortOption.IdAscending)]
    [InlineData(KpiDataHandlerModels.ProductGroupSortOption.IdDescending, ProductGroupSortOption.IdDescending)]
    [InlineData(KpiDataHandlerModels.ProductGroupSortOption.LastProductionDateAscending, ProductGroupSortOption.LastProductionDateAscending)]
    [InlineData(KpiDataHandlerModels.ProductGroupSortOption.LastProductionDateDescending, ProductGroupSortOption.LastProductionDateDescending)]
    [InlineData(KpiDataHandlerModels.ProductGroupSortOption.None, ProductGroupSortOption.None)]
    public void MapToSchemaProductGroupSortOption(KpiDataHandlerModels.ProductGroupSortOption internalSortOption, ProductGroupSortOption expectedSortOption)
    {
        // Act
        var result = internalSortOption.MapToSchemaProductGroupSortOption();

        // Assert
        result.Should().Be(expectedSortOption);
    }

    [Theory]
    [InlineData(ProductGroupSortOption.FirstProductionDateAscending, KpiDataHandlerModels.ProductGroupSortOption.FirstProductionDateAscending)]
    [InlineData(ProductGroupSortOption.FirstProductionDateDescending, KpiDataHandlerModels.ProductGroupSortOption.FirstProductionDateDescending)]
    [InlineData(ProductGroupSortOption.FriendlyNameAscending, KpiDataHandlerModels.ProductGroupSortOption.FriendlyNameAscending)]
    [InlineData(ProductGroupSortOption.FriendlyNameDescending, KpiDataHandlerModels.ProductGroupSortOption.FriendlyNameDescending)]
    [InlineData(ProductGroupSortOption.IdAscending, KpiDataHandlerModels.ProductGroupSortOption.IdAscending)]
    [InlineData(ProductGroupSortOption.IdDescending, KpiDataHandlerModels.ProductGroupSortOption.IdDescending)]
    [InlineData(ProductGroupSortOption.LastProductionDateAscending, KpiDataHandlerModels.ProductGroupSortOption.LastProductionDateAscending)]
    [InlineData(ProductGroupSortOption.LastProductionDateDescending, KpiDataHandlerModels.ProductGroupSortOption.LastProductionDateDescending)]
    [InlineData(ProductGroupSortOption.None, KpiDataHandlerModels.ProductGroupSortOption.None)]
    public void MapToInternalProductGroupSortOption(ProductGroupSortOption schemaSortOption, KpiDataHandlerModels.ProductGroupSortOption expectedSortOption)
    {
        // Act
        var result = schemaSortOption.MapToInternalProductGroupSortOption();

        // Assert
        result.Should().Be(expectedSortOption);
    }
}