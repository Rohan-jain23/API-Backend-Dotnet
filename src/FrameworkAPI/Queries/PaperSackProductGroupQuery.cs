using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.Schema.ProductGroup;
using FrameworkAPI.Services.Interfaces;
using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Types;
using HotChocolate.Types.Pagination;

namespace FrameworkAPI.Queries;

/// <summary>
/// GraphQL query class for product group entity of paper sack department.
/// </summary>
[ExtendObjectType("Query")]
public class PaperSackProductGroupQuery
{
    /// <summary>
    /// Query to get a list with product groups of paper sack department.
    /// </summary>
    /// <param name="productGroupService">The product group service.</param>
    /// <param name="regexFilter">If set, only product groups are returned where the id, the friendly name or one of the productIds fits to this regex expression.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="skip">Number of product groups to be skipped (can be used for pagination).</param>
    /// <param name="take">Number of product groups to be returned (can be used for pagination).</param>
    /// <param name="sortOption">Option to sort the query results.</param>
    [Authorize(Roles = ["go-general"])]
    [UseOffsetPaging(IncludeTotalCount = true, DefaultPageSize = 20, MaxPageSize = 100)]
    public async Task<CollectionSegment<PaperSackProductGroup>> GetPaperSackProductGroups(
        [Service] IProductGroupService productGroupService,
        string? regexFilter,
        CancellationToken cancellationToken,
        int skip = 0,
        int take = 20,
        ProductGroupSortOption sortOption = ProductGroupSortOption.LastProductionDateDescending)
    {
        var productGroups = (await productGroupService.GetPaperSackProductGroups(
            regexFilter, take, skip, sortOption, cancellationToken)).ToList();
        var totalCount = await productGroupService.GetPaperSackProductGroupsCount(regexFilter, cancellationToken);

        if (productGroups.Count == 0)
        {
            return new CollectionSegment<PaperSackProductGroup>([], new CollectionSegmentInfo(false, false));
        }

        var pageInfo = new CollectionSegmentInfo(
            hasNextPage: skip + take < totalCount,
            hasPreviousPage: skip > 0);

        return new CollectionSegment<PaperSackProductGroup>(productGroups, pageInfo, totalCount);
    }

    /// <summary>
    /// Query to get one paper sack product group by its ID.
    /// </summary>
    /// <param name="productGroupService">The product group service.</param>
    /// <param name="id">Unique identifier of the product group</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    [Authorize(Roles = ["go-general"])]
    public async Task<PaperSackProductGroup> GetPaperSackProductGroup(
        [Service] IProductGroupService productGroupService,
        string id,
        CancellationToken cancellationToken)
    {
        var productGroup = await productGroupService.GetPaperSackProductGroupById(id, cancellationToken);

        return productGroup;
    }
}