using System;
using System.Collections.Generic;
using FrameworkAPI.DataLoaders;
using FrameworkAPI.Models.Enums;
using FrameworkAPI.Schema.MaterialLot;
using FrameworkAPI.Schema.Misc;
using FrameworkAPI.Schema.ProducedJob.MachineSettings.Printing;
using FrameworkAPI.Services.Interfaces;
using HotChocolate;
using WuH.Ruby.ProductionPeriodsDataHandler.Client;

namespace FrameworkAPI.Schema.ProducedJob;

/// <summary>
/// Produced job entity of printing machines.
/// </summary>
public class PrintingProducedJob(JobInfo jobInfo, DateTime? machineQueryTimestamp) : ProducedJob(jobInfo, machineQueryTimestamp)
{
    /// <summary>
    /// Meters of produced output in acceptable quality within this job.
    /// [Source: KPIs]
    /// </summary>
    public NumericValue GoodLength(
        JobStandardKpiCacheDataLoader jobStandardKpiCacheDataLoader,
        MachineMetaDataBatchDataLoader machineMetaDataBatchDataLoader,
        [Service] IKpiService kpiService)
        => kpiService.GetNumericValue(jobStandardKpiCacheDataLoader, machineMetaDataBatchDataLoader, KpiAttribute.GoodProductionCount, MachineId, JobId);

    /// <summary>
    /// Meters of produced output in not-acceptable quality (= scrap/waste/maculature) within this job.
    /// This includes 'SetupScrapLength'.
    /// [Source: KPIs]
    /// </summary>
    public NumericValue ScrapLength(
        JobStandardKpiCacheDataLoader jobStandardKpiCacheDataLoader,
        MachineMetaDataBatchDataLoader machineMetaDataBatchDataLoader,
        [Service] IKpiService kpiService)
        => kpiService.GetNumericValue(jobStandardKpiCacheDataLoader, machineMetaDataBatchDataLoader, KpiAttribute.ScrapProductionCount, MachineId, JobId);

    /// <summary>
    /// Meters of produced items in not-acceptable quality (= scrap/waste) during setup of this job.
    /// This is a sub-set of 'ScrapLength'.
    /// [Source: KPIs]
    /// </summary>
    public NumericValue SetupScrapLength(
        JobStandardKpiCacheDataLoader jobStandardKpiCacheDataLoader,
        MachineMetaDataBatchDataLoader machineMetaDataBatchDataLoader,
        [Service] IKpiService kpiService)
        => kpiService.GetNumericValue(jobStandardKpiCacheDataLoader, machineMetaDataBatchDataLoader, KpiAttribute.SetupScrapCount, MachineId, JobId);

    /// <summary>
    /// The target machine speed of this job, which is usually defined by the production planning department.
    /// The origin of the value can be different (priority is 1-3):
    /// 1.) Job-specific value defined in customer system (via Connect 4 Flow)
    /// 2.) Value entered in ProControl (ProcessData)
    /// 3.) Default setting (via Track section in Admin)
    /// [Source: KPIs]
    /// </summary>
    public NumericValue TargetSpeed(
        JobStandardKpiCacheDataLoader jobStandardKpiCacheDataLoader,
        MachineMetaDataBatchDataLoader machineMetaDataBatchDataLoader,
        [Service] IKpiService kpiService)
        => kpiService.GetNumericValue(jobStandardKpiCacheDataLoader, machineMetaDataBatchDataLoader, KpiAttribute.TargetSpeed, MachineId, JobId);

    /// <summary>
    /// Average machine speed during all time-ranges the machine was in production within this job.
    /// [Source: KPIs]
    /// </summary>
    public NumericValue AverageSpeedDuringProduction(
        JobStandardKpiCacheDataLoader jobStandardKpiCacheDataLoader,
        MachineMetaDataBatchDataLoader machineMetaDataBatchDataLoader,
        [Service] IKpiService kpiService)
        => kpiService.GetNumericValue(jobStandardKpiCacheDataLoader, machineMetaDataBatchDataLoader, KpiAttribute.AverageProductionSpeed, MachineId, JobId);

    /// <summary>
    /// Machine settings during this job.
    /// [Source: MachineSnapshot]
    /// </summary>
    public PrintingMachineSettings? MachineSettings => new(MachineId, EndTime, TimeRanges, MachineQueryTimestamp);

    /// <summary>
    /// Set value of the roll length.
    /// </summary>
    [GraphQLIgnore]
    public NumericSnapshotValuesDuringProduction? RollLength { get; set; }

    /// <summary>
    /// Detailed data from the print inspection systems about this job.
    /// [Source: CheckDataHandler]
    /// </summary>
    [GraphQLIgnore]
    public PrintInspectionSystemJobData? InspectionSystem { get; set; }

    /// <summary>
    /// Printed rolls which have been produced during this job.
    /// [Source: MaterialDataHandler]
    /// </summary>
    [GraphQLIgnore]
    public List<PrintingProducedRoll>? ProducedRolls { get; set; }
}