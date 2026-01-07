using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.DataLoaders;
using FrameworkAPI.Extensions;
using FrameworkAPI.Models.Enums;
using FrameworkAPI.Schema.Misc;
using FrameworkAPI.Schema.ProducedJob.MachineSettings.PaperSack;
using FrameworkAPI.Schema.ProductGroup;
using FrameworkAPI.Services.Interfaces;
using HotChocolate;
using Microsoft.AspNetCore.Http;
using WuH.Ruby.KpiDataHandler.Client;
using WuH.Ruby.MachineSnapShooter.Client;
using WuH.Ruby.ProductionPeriodsDataHandler.Client;

namespace FrameworkAPI.Schema.ProducedJob;

/// <summary>
/// Produced job entity of paper sack machines.
/// </summary>
public class PaperSackProducedJob(
    JobInfo jobInfo,
    MachineFamily machineFamily,
    DateTime? machineQueryTimestamp,
    StandardJobKpis? standardJobKpis = null) : ProducedJob(jobInfo, machineQueryTimestamp, standardJobKpis)
{
    /// <summary>
    /// Unique produced job identifier (in format MachineId_Job, like: "EQ12345_JobA-1234") of the related tuber/bottomer job.
    /// The two main steps of paper sack production (tube and bottom forming) are executed on different machines (tuber and bottomer) in a production line.
    /// These two main machines are connected separately to RUBY and therefore there are two separate jobs for the production of one paper sack order.
    /// For many reasons it is interesting how the job was produced on the other machine and so this relation can be made with this value.
    /// If this entity is a produced job of a bottomer, this value is the unique id of the related produced job on the tuber.
    /// If this entity is a produced job of a tuber, this value is the unique id of the related produced job on the bottomer.
    /// The relation between these jobs is only done by the naming of the produced jobs or the products.
    /// If there is no similar named job or product in the last three days, this value is 'null'.
    /// [Source: KPIs]
    /// </summary>
    public async Task<string?> UniqueIdOfRelatedProducedJobFromOtherMachine(
        JobStandardKpiCacheDataLoader jobStandardKpiCacheDataLoader,
        [Service] IKpiService kpiService,
        CancellationToken cancellationToken)
        => await kpiService.GetUniqueIdOfRelatedProducedJobFromOtherMachine(jobStandardKpiCacheDataLoader, MachineId, JobId, StandardJobKpis, cancellationToken);

    /// <summary>
    /// Count of produced items in acceptable quality within this job.
    /// This value can be corrected by a following machine (like Arcomat) or via RUBY Track.
    /// [Source: KPIs]
    /// </summary>
    public NumericValue GoodQuantity(
        JobStandardKpiCacheDataLoader jobStandardKpiCacheDataLoader,
        MachineMetaDataBatchDataLoader machineMetaDataBatchDataLoader,
        [Service] IKpiService kpiService)
        => kpiService.GetNumericValue(jobStandardKpiCacheDataLoader, machineMetaDataBatchDataLoader, KpiAttribute.GoodProductionCount, MachineId, JobId, StandardJobKpis);

    /// <summary>
    /// The machines counter of produced items in acceptable quality within this job.
    /// This is the original value of the machine, which was not corrected by a following machine (like Arcomat) or via RUBY Track.
    /// [Source: KPIs]
    /// </summary>
    public NumericValue OriginalGoodQuantity(
        JobStandardKpiCacheDataLoader jobStandardKpiCacheDataLoader,
        MachineMetaDataBatchDataLoader machineMetaDataBatchDataLoader,
        [Service] IKpiService kpiService)
        => kpiService.GetNumericValue(jobStandardKpiCacheDataLoader, machineMetaDataBatchDataLoader, KpiAttribute.OriginalGoodProductionCount, MachineId, JobId, StandardJobKpis);

    /// <summary>
    /// Is true, if 'OriginalGoodQuantity' is smaller than 'GoodQuantity'.
    /// This indicates an irregularity during production (like re-palletizing via control station).
    /// [Source: KPIs]
    /// </summary>
    public async Task<bool> IsApparentlyWrongGoodQuantity(
        JobStandardKpiCacheDataLoader jobStandardKpiCacheDataLoader,
        [Service] IKpiService kpiService,
        CancellationToken cancellationToken)
        => await kpiService.GetBool(jobStandardKpiCacheDataLoader, KpiAttribute.IsApparentlyWrongGoodProductionCount, MachineId, JobId, StandardJobKpis, cancellationToken) ?? false;

    /// <summary>
    /// Count of produced items in not-acceptable quality (= scrap/waste) within this job.
    /// This value can be corrected by a following machine (like Arcomat) or via RUBY Track.
    /// This includes 'SetupScrapQuantity'.
    /// [Source: KPIs]
    /// </summary>
    public NumericValue ScrapQuantity(
        JobStandardKpiCacheDataLoader jobStandardKpiCacheDataLoader,
        MachineMetaDataBatchDataLoader machineMetaDataBatchDataLoader,
        [Service] IKpiService kpiService)
        => kpiService.GetNumericValue(jobStandardKpiCacheDataLoader, machineMetaDataBatchDataLoader, KpiAttribute.ScrapProductionCount, MachineId, JobId, StandardJobKpis);

    /// <summary>
    /// Count of produced items in not-acceptable quality (= scrap/waste) during setup of this job.
    /// This is a sub-set of 'ScrapQuantity'.
    /// [Source: KPIs]
    /// </summary>
    public NumericValue SetupScrapQuantity(
        JobStandardKpiCacheDataLoader jobStandardKpiCacheDataLoader,
        MachineMetaDataBatchDataLoader machineMetaDataBatchDataLoader,
        [Service] IKpiService kpiService)
        => kpiService.GetNumericValue(jobStandardKpiCacheDataLoader, machineMetaDataBatchDataLoader, KpiAttribute.SetupScrapCount, MachineId, JobId, StandardJobKpis);

    /// <summary>
    /// Machines production speed during this job.
    /// Other than the AverageSpeedDuringProduction property these values are determined over all time-ranges of the job (including setup and downtime).
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction Speed =>
        new(SnapshotColumnIds.PaperSackSpeed, EndTime, MachineId, TimeRanges, MachineQueryTimestamp);

    /// <summary>
    /// Speed level during this job, which is determined for the speed histogram and target speed recommendation.
    /// Other than the normal 'Speed' property, similar speeds are grouped into categories (rounded to tens).
    /// Also, job-specific downtimes are still accounted for in the speed level as they affect productivity.
    /// Certain situations, like roll changes or ramp-ups, are excluded to maintain an accurate picture.
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction SpeedLevel =>
        new(SnapshotColumnIds.PaperSackSpeedLevel, EndTime, MachineId, TimeRanges, MachineQueryTimestamp);

    /// <summary>
    /// The target machine speed of this job, which is usually defined by the production planning department.
    /// The origin of the value can be different (priority is 1 to 5):
    /// 1.) Job-specific value defined/corrected in RUBY (via OperatorUI or Track)
    /// 2.) Product(group)-specific setting (via Track)
    /// 3.) Job-specific value defined in customer system (via Connect 4 Flow)
    /// 4.) Value entered in ProControl (ProcessData)
    /// 5.) Default setting (via Track section in Admin)
    /// [Source: KpiDataHandler]
    /// </summary>
    public NumericValue? TargetSpeed(
        JobStandardKpiCacheDataLoader jobStandardKpiCacheDataLoader,
        MachineMetaDataBatchDataLoader machineMetaDataBatchDataLoader,
        [Service] IKpiService kpiService)
        => kpiService.GetNumericValue(jobStandardKpiCacheDataLoader, machineMetaDataBatchDataLoader, KpiAttribute.TargetSpeed, MachineId, JobId, StandardJobKpis);

    /// <summary>
    /// Average machine speed during all time-ranges the machine was in status production within this job.
    /// [Source: KpiDataHandler]
    /// </summary>
    public NumericValue AverageSpeedDuringProduction(
        JobStandardKpiCacheDataLoader jobStandardKpiCacheDataLoader,
        MachineMetaDataBatchDataLoader machineMetaDataBatchDataLoader,
        [Service] IKpiService kpiService)
        => kpiService.GetNumericValue(jobStandardKpiCacheDataLoader, machineMetaDataBatchDataLoader, KpiAttribute.AverageProductionSpeed, MachineId, JobId, StandardJobKpis);

    /// <summary>
    /// Machine settings during this job.
    /// [Source: MachineSnapshot]
    /// </summary>
    public PaperSackMachineSettings MachineSettings()
        => new(MachineId, EndTime, machineFamily, TimeRanges, MachineQueryTimestamp);

    /// <summary>
    /// Free text that can be used to describe product specs (like valve type), material or other information
    /// (first tab on PROCONTROL product administration page).
    /// [Source: MachineSnapshot]
    /// </summary>
    public SnapshotValuesDuringProduction<string> MaterialText =>
        new(SnapshotColumnIds.PaperSackMaterialText, EndTime, MachineId, TimeRanges, MachineQueryTimestamp);

    /// <summary>
    /// Free text that can be used to describe product specs (like valve type), material or other information
    /// (third tab on PROCONTROL product administration page).
    /// [Source: MachineSnapshot]
    /// </summary>
    public SnapshotValuesDuringProduction<string> MaterialInformation =>
        new(SnapshotColumnIds.PaperSackMaterialInformation, EndTime, MachineId, TimeRanges, MachineQueryTimestamp);

    /// <summary>
    /// Information about the production approval event of this job.
    /// The production approval can be performed on the OperatorUI (usually by the shift supervisor).
    /// This feature is only visible to the user, if it is activated in the 'Track' section of the AdminUI.
    /// Because of performance reasons, it is not possible to subscribe to this property (-> returns 'null').
    /// [Source: KpiDataHandler]
    /// </summary>
    public async Task<ProductionApprovalEvent?> ProductionApproval(
        [Service] IKpiService kpiService,
        [Service] IHttpContextAccessor context,
        CancellationToken cancellationToken)
    {
        if (context.HttpContext.IsSubscriptionOrNull())
            return null;

        return await kpiService.GetProductionApproval(
            MachineId,
            TimeRanges?.Select(timeRange => (WuH.Ruby.Common.Core.TimeRange)timeRange),
            cancellationToken);
    }

    /// <summary>
    /// Condensed production history of this job, like it is displayed in RUBY Track Operator UI.
    /// This list contains all history entries in ascending order (-> usually first entry is 'Setup').
    /// Because of performance reasons, it is not possible to subscribe to this property (-> returns 'null').
    /// [Source: ProductionPeriods via Track.Common]
    /// </summary>
    public async Task<List<TrackHistoryEntry>?> TrackProductionHistory(
        [Service] ITrackProductionHistoryService trackProductionHistoryService,
        [Service] IHttpContextAccessor context,
        CancellationToken cancellationToken)
    {
        if (context.HttpContext.IsSubscriptionOrNull())
            return null;

        return await trackProductionHistoryService.GetProductionHistory(
            JobInfo,
            cancellationToken);
    }

    /// <summary>
    /// Product group of this job.
    /// In a product group, jobs with products sharing similar attributes are grouped together for joint analysis and to specify target values for future jobs.
    /// Because of performance reasons, it is not possible to subscribe to this property (-> returns 'null').
    /// [Source: KpiDataHandler]
    /// </summary>
    public async Task<PaperSackProductGroup?> ProductGroup(
        [Service] IProductGroupService productGroupService,
        [Service] IHttpContextAccessor context,
        CancellationToken cancellationToken)
    {
        if (context.HttpContext.IsSubscriptionOrNull())
            return null;

        return await productGroupService.GetPaperSackProductGroupByJobId(
            MachineId, JobId, cancellationToken);
    }
}