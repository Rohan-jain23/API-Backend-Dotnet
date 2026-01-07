using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.DataLoaders;
using FrameworkAPI.Schema.Misc;
using FrameworkAPI.Services.Interfaces;
using HotChocolate;
using KpiDataHandlerModels = WuH.Ruby.KpiDataHandler.Client.Models;

namespace FrameworkAPI.Schema.ProductGroup;

/// <summary>
/// Product group entity of paper sack machines.
/// In a product group, jobs with products sharing similar attributes are grouped together for joint analysis and to specify target values for future jobs.
/// While individual products may have different dimensions and manufactured for different customers, their performance should be very similar.
/// Products are categorized into different product groups when they differ in one of the 'Attributes'.
/// </summary>
public class PaperSackProductGroup(KpiDataHandlerModels.PaperSackProductGroup paperSackProductGroup)
{
    /// <summary>
    /// Unique identifier of the product group.
    /// This 'Id' is assembled from the 'ProductGroupDefinitionVersion'
    /// and the 'Attributes' (in a specific way that is define din the product group definition).
    /// [Source: KpiDataHandler]
    /// </summary>
    public string Id { get; set; } = paperSackProductGroup.Id;

    /// <summary>
    /// A version that is count-up if the product group definition changes
    /// (for example when a new attribute should be considered).
    /// This version is the prefix of the 'Id'.
    /// [Source: KpiDataHandler]
    /// </summary>
    public int ProductGroupDefinitionVersion { get; set; } = paperSackProductGroup.ProductGroupDefinitionVersion;

    /// <summary>
    /// Unique identifier of the product group in which most of the jobs of this product group have been in the last version.
    /// When the 'ProductGroupDefinitionVersion' is raised, the product groups of all jobs are re-determined.
    /// In that phase, all new product groups would not have user-defined properties (like friendly name, target speed or notes).
    /// As these user-defined properties should not get lost, these are taken from the parent product group.
    /// The parent product group is determined like this:
    /// - For all jobs of the new product group, the product group of the previous version is queried.
    /// - As this might return a list of product groups, the one with the most produced quantity is taken.
    /// [Source: KpiDataHandler]
    /// </summary>
    public string? ParentId { get; set; } = paperSackProductGroup.ParentId;

    /// <summary>
    /// A well readable name for this product group.
    /// As the 'Id' is not really human readable, the product groups initially get a number as 'FriendlyName'.
    /// The customer is able to change this 'FriendlyName'.
    /// After a change of the 'ProductGroupDefinitionVersion',
    /// the 'FriendlyName' of the parent product group is taken (and suffixed if a parent has multiple children).
    /// [Source: KpiDataHandler]
    /// </summary>
    public string FriendlyName { get; set; } = paperSackProductGroup.FriendlyName;

    /// <summary>
    /// Values of all attributes that define this product group.
    /// The attributes are defined in the product group definition.
    /// This also contains legacy attributes of old product group definition versions (these are marked in the description).
    /// All jobs of this product group have the same attributes.
    /// These attributes were selected by WuH because they can be derived from machine data
    /// and have significant impact on the production performance.
    /// [Source: KpiDataHandler]
    /// </summary>
    public PaperSackProductGroupAttributes Attributes { get; set; } = new PaperSackProductGroupAttributes(paperSackProductGroup.Attributes);

    /// <summary>
    /// Names of all products that are belonging to this product group.
    /// Attention: This list can contain incorrect items if the product of a job was corrected afterwards.
    /// [Source: KpiDataHandler]
    /// </summary>
    public List<string> ProductIds { get; set; } = paperSackProductGroup.ProductIds;

    /// <summary>
    /// Number of jobs from all machines that produced a product of this product group.
    /// Attention: This value can be incorrect if the id/times of a job were corrected afterwards.
    /// [Source: KpiDataHandler]
    /// </summary>
    public int ProducedJobsCount { get; set; } = paperSackProductGroup.JobIdsPerMachine.Values.SelectMany(x => x).Count();

    /// <summary>
    /// The start time of the first job that produced a product of this product group.
    /// [Source: KpiDataHandler]
    /// </summary>
    public DateTime FirstProductionDate { get; set; } = paperSackProductGroup.FirstProductionDate;

    /// <summary>
    /// The end time of the last job that produced a product of this product group.
    /// If the production is currently active, this is should nearly be the machine time.
    /// [Source: KpiDataHandler]
    /// </summary>
    public DateTime LastProductionDate { get; set; } = paperSackProductGroup.LastProductionDate;

    /// <summary>
    /// Note with information/comments/instructions for the whole product group (independent from the machine).
    /// Is 'null', if there is no overall note, yet.
    /// [Source: KpiDataHandler]
    /// </summary>
    public string? OverallNote { get; set; } = paperSackProductGroup.Note;

    /// <summary>
    /// Machine-specific notes with information/comments/instructions for the product group.
    /// This dictionary contains one item for all machines that have an machine-specific note for this product group
    /// (-> is the machine is not in this dictionary, there is no note, yet).
    /// [Source: KpiDataHandler]
    /// <returns>Dictionary (key: machineId; value: machine-specific note)</returns>
    /// </summary>
    public Dictionary<string, string> NotePerMachine { get; set; } = paperSackProductGroup.NotesPerMachine;

    /// <summary>
    /// Machine-specific target speed settings for the product group.
    /// This dictionary contains one item for all machines that have an target speed setting for this product group
    /// (-> is the machine is not in this dictionary, there is no target speed setting, yet).
    /// [Source: KpiDataHandler]
    /// <returns>Dictionary (key: machineId; value: target speed)</returns>
    /// </summary>
    public Dictionary<string, NumericValue> TargetSpeedSettingPerMachine(
        MachineMetaDataBatchDataLoader machineMetaDataBatchDataLoader,
        [Service] IKpiService kpiService,
        [Service] IProductGroupService productGroupService) => productGroupService.MapTargetSpeedPerMachineToSchema(
        machineMetaDataBatchDataLoader, kpiService, paperSackProductGroup.TargetSpeedPerMachine);

    /// <summary>
    /// Statistics (produced jobs and aggregated KPIs) of this product group per machine.
    /// This dictionary contains one item for all machines that have jobs that fit to the filters
    /// (-> is the machine is not in this dictionary, there is no fitting job on this machine).
    /// The list is sorted by 'TotalProducedGoodQuantity' (the machine with the highest value is the first item).
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="productGroupStandardKpiCacheDataLoader">Internal productGroupStandardKpi data loader.</param>
    /// <param name="machineMetaDataBatchDataLoader">Internal machineMetaData data loader.</param>
    /// <param name="productGroupService">Internal service.</param>
    /// <param name="from">Only jobs that were produced after this timestamp are considered.</param>
    /// <param name="to">Only jobs that were produced before this timestamp are considered. If this is 'null', all jobs until now are considered.</param>
    /// <param name="productIdFilter">Only jobs with this product are considered.</param>
    /// <param name="machineIdFilter">Only jobs from this machine are considered.</param>
    /// <param name="machineFamilyFilter">Only jobs from this machine family are considered.</param>
    /// <returns>Dictionary (key: machineId; value: statistics)</returns>
    /// </summary>
    public async Task<Dictionary<string, PaperSackProductGroupStatisticsPerMachine?>?> StatisticsPerMachine(
        CancellationToken cancellationToken,
        ProductGroupStandardKpiCacheDataLoader productGroupStandardKpiCacheDataLoader,
        MachineMetaDataBatchDataLoader machineMetaDataBatchDataLoader,
        [Service] IProductGroupService productGroupService,
        DateTime from,
        DateTime? to = null,
        string? productIdFilter = null,
        string? machineIdFilter = null,
        PaperSackMachineFamilyFilter machineFamilyFilter = PaperSackMachineFamilyFilter.Both)
        => await productGroupService.GetPaperSackProductGroupStatisticsPerMachine(
            productGroupStandardKpiCacheDataLoader,
            machineMetaDataBatchDataLoader,
            Id,
            from,
            to,
            productIdFilter,
            machineIdFilter,
            machineFamilyFilter,
            cancellationToken);
}