using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.Schema.Misc;
using FrameworkAPI.Schema.ProducedJob;
using FrameworkAPI.Services.Interfaces;
using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Types;
using HotChocolate.Types.Pagination;

namespace FrameworkAPI.Queries;

/// <summary>
/// GraphQL query class for produced job entity.
/// </summary>
[ExtendObjectType("Query")]
public class ProducedJobQuery
{
    /// <summary>
    /// Query to get a list of the last produced jobs.
    /// </summary>
    /// <param name="producedJobService">The produced jobs service.</param>
    /// <param name="machineService">The machine service.</param>
    /// <param name="skip">Number of jobs to be skipped (can be used for pagination).</param>
    /// <param name="take">Number of jobs to be returned (can be used for pagination).</param>
    /// <param name="from">If set, only jobs produced after this timestamp are returned.</param>
    /// <param name="to">If set, only jobs produced before this timestamp are returned.</param>
    /// <param name="regexFilter">If set, only jobs are returned where the job id, the product id or the customer fit to this regex expression.</param>
    /// <param name="machineIdFilter">If set, only jobs produced on this machine are returned.</param>
    /// <param name="machineDepartmentFilter">If set, only jobs produced on machines of this machine department are returned.</param>
    /// <param name="machineFamilyFilter">If set, only jobs produced on machines of this machine family are returned.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    [Authorize(Roles = ["go-general"])]
    [UseOffsetPaging(IncludeTotalCount = true, DefaultPageSize = 20, MaxPageSize = 100)]
    public async Task<CollectionSegment<ProducedJob>> GetProducedJobs(
        [Service] IProducedJobService producedJobService,
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
            return new CollectionSegment<ProducedJob>(new List<ProducedJob>(), new CollectionSegmentInfo(false, false));

        var producedJobs = (await producedJobService.GetLatestProducedJobs(
            filteredMachineIds,
            from,
            to,
            regexFilter,
            skip,
            take,
            cancellationToken)).ToList();

        if (!producedJobs.Any())
            return new CollectionSegment<ProducedJob>(new List<ProducedJob>(), new CollectionSegmentInfo(false, false));

        var totalCount = await producedJobService.GetLatestProducedJobsTotalCount(
            filteredMachineIds,
            regexFilter,
            from,
            to,
            cancellationToken);

        var pageInfo = new CollectionSegmentInfo(
            hasNextPage: skip + take < totalCount,
            hasPreviousPage: skip > 0);

        return new CollectionSegment<ProducedJob>(producedJobs.ToList(), pageInfo, totalCount);
    }

    /// <summary>
    /// Query to get one produced job by the unique combination of machine id and job id.
    /// </summary>
    [Authorize(Roles = ["go-general"])]
    public async Task<ProducedJob> GetProducedJob(
        [Service] IProducedJobService producedJobService,
        [Service] IMachineService machineService,
        string machineId,
        string jobId,
        CancellationToken cancellationToken)
    {
        var department = await machineService.GetMachineBusinessUnit(machineId, cancellationToken);
        var machineFamily = await machineService.GetMachineFamily(machineId, cancellationToken);
        var producedJob =
            await producedJobService.GetProducedJob(machineId, jobId, department, machineFamily, cancellationToken);
        return producedJob;
    }
}