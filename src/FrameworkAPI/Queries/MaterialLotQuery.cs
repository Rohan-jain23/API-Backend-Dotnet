using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.DataLoaders;
using FrameworkAPI.Models;
using FrameworkAPI.Schema.Misc;
using FrameworkAPI.Schema.MaterialLot;
using FrameworkAPI.Services.Interfaces;
using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Types;
using HotChocolate.Types.Pagination;

namespace FrameworkAPI.Queries;

/// <summary>
/// GraphQL query class for material lot entity.
/// </summary>
[ExtendObjectType("Query")]
public class MaterialLotQuery
{
    /// <summary>
    /// Query to get a list of material lots.
    /// </summary>
    /// <param name="materialLotsCacheDataLoader">The material Lots cache data loader.</param>
    /// <param name="machineService">The machine service.</param>
    /// <param name="skip">Number of lots to be skipped (can be used for pagination).</param>
    /// <param name="take">Number of lots to be returned (can be used for pagination).</param>
    /// <param name="from">If set, only lots produced after this timestamp are returned.</param>
    /// <param name="to">If set, only lots produced before this timestamp are returned.</param>
    /// <param name="regexFilter">If set, only lots are returned where the material lot id fits to this regex expression.</param>
    /// <param name="machineIdFilter">If set, only lots produced on this machine are returned.</param>
    /// <param name="machineDepartmentFilter">If set, only lots produced on machines of this machine department are returned.</param>
    /// <param name="machineFamilyFilter">If set, only lots produced on machines of this machine family are returned.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    [Authorize(Roles = ["go-general"])]
    [UseOffsetPaging(IncludeTotalCount = true, DefaultPageSize = 20, MaxPageSize = 100)]
    public async Task<CollectionSegment<MaterialLot>> GetMaterialLots(
        MaterialLotsCacheDataLoader materialLotsCacheDataLoader,
        [Service] IMachineService machineService,
        DateTime? from,
        DateTime? to,
        string? regexFilter,
        string? machineIdFilter,
        MachineDepartment? machineDepartmentFilter,
        MachineFamily? machineFamilyFilter,
        CancellationToken cancellationToken,
        int skip = 0,
        int take = 20)
    {
        var filteredMachineIds = await machineService.GetMachineIdsByFilter(
            machineIdFilter,
            machineDepartmentFilter,
            machineFamilyFilter,
            cancellationToken);

        if (!filteredMachineIds.Any())
        {
            return new CollectionSegment<MaterialLot>(new List<MaterialLot>(), new CollectionSegmentInfo(false, false));
        }

        var (internalLotsAndTotalCount, exception) = await materialLotsCacheDataLoader.LoadAsync(
            new MaterialLotsFilter
            {
                From = from,
                MachineIdsFilter = filteredMachineIds,
                RegexFilter = regexFilter,
                Skip = skip,
                Take = take,
                To = to
            }, cancellationToken);

        if (exception is not null)
        {
            throw exception;
        }

        var (internalLots, totalCount) = internalLotsAndTotalCount!.Value;

        if (!internalLots.Any())
        {
            return new CollectionSegment<MaterialLot>(new List<MaterialLot>(), new CollectionSegmentInfo(false, false));
        }

        var pageInfo = new CollectionSegmentInfo(
            hasNextPage: skip + take < totalCount,
            hasPreviousPage: skip > 0);

        var externalLots = internalLots
            .Select(MaterialLot.CreateInstance)
            .OfType<MaterialLot>()
            .ToList();

        return new CollectionSegment<MaterialLot>(externalLots, pageInfo, totalCount);
    }

    /// <summary>
    /// Query to get one material lot by its unique id.
    /// </summary>
    [Authorize(Roles = ["go-general"])]
    public async Task<MaterialLot?> GetMaterialLot(
        MaterialLotCacheDataLoader materialLotCacheDataLoader,
        string materialLotId,
        CancellationToken cancellationToken)
    {
        var (internalLot, exception) = await materialLotCacheDataLoader.LoadAsync(materialLotId, cancellationToken);

        if (exception is not null)
        {
            throw exception;
        }

        var externalLot = MaterialLot.CreateInstance(internalLot);
        return externalLot;
    }
}