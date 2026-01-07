using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.DataLoaders;
using FrameworkAPI.Schema.Misc;
using FrameworkAPI.Schema.ProductGroup;

namespace FrameworkAPI.Services.Interfaces;

public interface IProductGroupService
{
    Task<PaperSackProductGroup> GetPaperSackProductGroupById(string productGroupId, CancellationToken cancellationToken);

    Task<PaperSackProductGroup?> GetPaperSackProductGroupByJobId(string machineId, string jobId, CancellationToken cancellationToken);

    Task<IEnumerable<PaperSackProductGroup>> GetPaperSackProductGroups(
        string? regexFilter,
        int limit,
        int offset,
        ProductGroupSortOption sortOption,
        CancellationToken cancellationToken);

    Task<int> GetPaperSackProductGroupsCount(string? regexFilter, CancellationToken cancellationToken);

    Task<Dictionary<string, PaperSackProductGroupStatisticsPerMachine?>> GetPaperSackProductGroupStatisticsPerMachine(
        ProductGroupStandardKpiCacheDataLoader productGroupStandardKpiCacheDataLoader,
        MachineMetaDataBatchDataLoader machineMetaDataBatchDataLoader,
        string paperSackProductGroupId,
        DateTime from,
        DateTime? to,
        string? productIdFilter,
        string? machineIdFilter,
        PaperSackMachineFamilyFilter machineFamilyFilter,
        CancellationToken cancellationToken);

    Task<PaperSackProductGroup> UpdatePaperSackProductGroupNote(
        string paperSackProductGroupId,
        string? note,
        string userId,
        CancellationToken cancellationToken);

    Task<PaperSackProductGroup> UpdatePaperSackProductGroupMachineNote(
        string paperSackProductGroupId,
        string machineId,
        string? note,
        string userId,
        CancellationToken cancellationToken);

    Task<PaperSackProductGroup> UpdatePaperSackProductGroupMachineTargetSpeed(
        string paperSackProductGroupId,
        string machineId,
        double? targetSpeed,
        string userId,
        CancellationToken cancellationToken);

    Dictionary<string, NumericValue> MapTargetSpeedPerMachineToSchema(
        MachineMetaDataBatchDataLoader machineMetaDataBatchDataLoader,
        IKpiService kpiService,
        Dictionary<string, double> targetSpeedPerMachine);
}