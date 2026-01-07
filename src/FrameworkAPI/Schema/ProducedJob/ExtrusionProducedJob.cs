using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.DataLoaders;
using FrameworkAPI.Extensions;
using FrameworkAPI.Models.Enums;
using FrameworkAPI.Schema.MaterialLot;
using FrameworkAPI.Schema.Misc;
using FrameworkAPI.Schema.ProducedJob.MachineSettings.Extrusion;
using FrameworkAPI.Schema.ProductDefinition;
using FrameworkAPI.Services.Interfaces;
using HotChocolate;
using Microsoft.AspNetCore.Http;
using WuH.Ruby.ProductionPeriodsDataHandler.Client;

namespace FrameworkAPI.Schema.ProducedJob;

/// <summary>
/// Produced job entity of extrusion machines.
/// </summary>
public class ExtrusionProducedJob(JobInfo jobInfo, DateTime? machineQueryTimestamp) : ProducedJob(jobInfo, machineQueryTimestamp)
{

    /// <summary>
    /// Meters of produced output in acceptable quality within this job.
    /// [Source: KPIs]
    /// </summary>
    public NumericValue GoodLength(
        JobStandardKpiCacheDataLoader jobStandardKpiCacheDataLoader,
        MachineMetaDataBatchDataLoader machineMetaDataBatchDataLoader,
        [Service] IKpiService kpiService)
        => kpiService.GetNumericValue(jobStandardKpiCacheDataLoader, machineMetaDataBatchDataLoader, KpiAttribute.GoodProductionCountInSecondUnit, MachineId, JobId);

    /// <summary>
    /// Meters of produced output in not-acceptable quality (= scrap/waste) within this job.
    /// This includes 'SetupScrapLength'.
    /// [Source: KPIs]
    /// </summary>
    public NumericValue ScrapLength(
        JobStandardKpiCacheDataLoader jobStandardKpiCacheDataLoader,
        MachineMetaDataBatchDataLoader machineMetaDataBatchDataLoader,
        [Service] IKpiService kpiService)
        => kpiService.GetNumericValue(jobStandardKpiCacheDataLoader, machineMetaDataBatchDataLoader, KpiAttribute.ScrapProductionCountInSecondUnit, MachineId, JobId);

    /// <summary>
    /// Meters of produced output in not-acceptable quality (= scrap/waste) during setup of this job.
    /// This is a sub-set of 'ScrapLength'.
    /// [Source: KPIs]
    /// </summary>
    public NumericValue SetupScrapLength(
        JobStandardKpiCacheDataLoader jobStandardKpiCacheDataLoader,
        MachineMetaDataBatchDataLoader machineMetaDataBatchDataLoader,
        [Service] IKpiService kpiService)
        => kpiService.GetNumericValue(jobStandardKpiCacheDataLoader, machineMetaDataBatchDataLoader, KpiAttribute.SetupScrapCountInSecondUnit, MachineId, JobId);

    /// <summary>
    /// Kilograms of produced output in acceptable quality within this job.
    /// Together with the 'ScrapWeight' this is the total raw material consumption within this job.
    /// [Source: KPIs]
    /// </summary>
    public NumericValue GoodWeight(
        JobStandardKpiCacheDataLoader jobStandardKpiCacheDataLoader,
        MachineMetaDataBatchDataLoader machineMetaDataBatchDataLoader,
        [Service] IKpiService kpiService)
        => kpiService.GetNumericValue(jobStandardKpiCacheDataLoader, machineMetaDataBatchDataLoader, KpiAttribute.GoodProductionCount, MachineId, JobId);

    /// <summary>
    /// Kilograms of produced output in not-acceptable quality (= scrap/waste) within this job.
    /// Together with the 'GoodWeight' this is the total raw material consumption within this job.
    /// This includes 'SetupScrapWeight'.
    /// [Source: KPIs]
    /// </summary>
    public NumericValue ScrapWeight(
        JobStandardKpiCacheDataLoader jobStandardKpiCacheDataLoader,
        MachineMetaDataBatchDataLoader machineMetaDataBatchDataLoader,
        [Service] IKpiService kpiService)
        => kpiService.GetNumericValue(jobStandardKpiCacheDataLoader, machineMetaDataBatchDataLoader, KpiAttribute.ScrapProductionCount, MachineId, JobId);

    /// <summary>
    /// Kilograms of produced output in not-acceptable quality (= scrap/waste) during setup of this job.
    /// This is a sub-set of 'ScrapWeight'.
    /// [Source: KPIs]
    /// </summary>
    public NumericValue SetupScrapWeight(
        JobStandardKpiCacheDataLoader jobStandardKpiCacheDataLoader,
        MachineMetaDataBatchDataLoader machineMetaDataBatchDataLoader,
        [Service] IKpiService kpiService)
        => kpiService.GetNumericValue(jobStandardKpiCacheDataLoader, machineMetaDataBatchDataLoader, KpiAttribute.SetupScrapCount, MachineId, JobId);

    /// <summary>
    /// Consumption of extruded raw material during the jobs time ranges grouped by material name.
    /// The material names are entered in ProControl (machine HMI) for each component.
    /// The material consumption is measured for each component by machine.
    /// With this data first the material consumption per material name is calculated for each component.
    /// Then the consumption per material name is summed-up from all components and returned.
    /// It is not possible to get the material consumption for the current job via subscription.
    /// Because of performance reasons, it is not possible to subscribe to this property (-> returns 'null').
    /// [Source: MachineSnapShooter]
    /// </summary>
    /// <returns>Dictionary (key: material name; value: consumption)</returns>
    public async Task<Dictionary<string, NumericValue>?> RawMaterialConsumptionByMaterial(
        SnapshotGroupedSumBatchDataLoader groupedSumBatchDataLoader,
        [Service] IHttpContextAccessor context,
        [Service] IMaterialConsumptionService materialConsumptionService,
        CancellationToken cancellationToken)
    {
        if (TimeRanges is null || context.HttpContext.IsSubscriptionOrNull())
        {
            return null;
        }

        return await materialConsumptionService.GetRawMaterialConsumptionByMaterial(
            groupedSumBatchDataLoader,
            MachineId,
            TimeRanges.Select(timeRange => (WuH.Ruby.Common.Core.TimeRange)timeRange).ToList(),
            cancellationToken);
    }

    /// <summary>
    /// The product which is produced during this job is described in a generic 'RUBY language'.
    /// Like this, similar products can be grouped and data of other production runs can be analyzed.
    /// This information in wrapped in the ValuesDuringProduction class, because it is possible,
    /// that the product was changed during the production of the job
    /// (for example because the jobId was not changed on an actual job change).
    /// [Source: ExtrusionProductRecognizer]
    /// </summary>
    [GraphQLIgnore]
    public SnapshotValuesDuringProduction<ExtrusionProductDefinition>? ProductDefinition { get; set; }

    /// <summary>
    /// The target throughput rate of this job, which is usually defined by the production planning department.
    /// The origin of the value can be different (priority is 1-3):
    /// 1.) Job-specific value defined in customer system (via Connect 4 Flow)
    /// 2.) Value entered in ProControl (ProcessData)
    /// 3.) Default setting (via Track section in Admin)
    /// [Source: KPIs]
    /// </summary>
    public async Task<NumericValue> TargetThroughputRate(
        JobStandardKpiCacheDataLoader jobStandardKpiCacheDataLoader,
        MachineMetaDataBatchDataLoader machineMetaDataBatchDataLoader,
        [Service] IKpiService kpiService,
        CancellationToken cancellationToken)
    {
        var kpiNumericValue = kpiService.GetNumericValue(
            jobStandardKpiCacheDataLoader,
            machineMetaDataBatchDataLoader,
            KpiAttribute.TargetSpeed,
            MachineId,
            JobId);

        var kpiValue = await kpiNumericValue.Value(cancellationToken);
        var kpiUnit = await kpiNumericValue.Unit(cancellationToken);

        return new NumericValue(kpiValue, kpiUnit);
    }

    /// <summary>
    /// Average throughput rate during all time-ranges the machine was in production within this job.
    /// [Source: KPIs]
    /// </summary>
    public async Task<NumericValue> AverageThroughputRateDuringProduction(
        JobStandardKpiCacheDataLoader jobStandardKpiCacheDataLoader,
        MachineMetaDataBatchDataLoader machineMetaDataBatchDataLoader,
        [Service] IKpiService kpiService,
        CancellationToken cancellationToken)
    {
        var kpiNumericValue = kpiService.GetNumericValue(
            jobStandardKpiCacheDataLoader,
            machineMetaDataBatchDataLoader,
            KpiAttribute.AverageProductionSpeed,
            MachineId,
            JobId);

        var kpiValue = await kpiNumericValue.Value(cancellationToken);
        var kpiUnit = await kpiNumericValue.Unit(cancellationToken);

        // The throughput rate has to be multiplied by 60 because we want
        // the throughput per hour and not the speed per minute (which is what the KPI data handler returns).
        // The unit is already "kg/h", so no changes are necessary here.
        var returnValue = kpiValue is null ? null : kpiValue * 60;

        return new NumericValue(returnValue, kpiUnit);
    }

    /// <summary>
    /// Machine settings during this job.
    /// [Source: MachineSnapshot]
    /// </summary>
    public ExtrusionMachineSettings MachineSettings() => new(MachineId, EndTime, TimeRanges, MachineQueryTimestamp);

    /// <summary>
    /// Extruded rolls which have been produced during this job.
    /// This list only contains mother rolls. Slit rolls can be accessed via the mother rolls.
    /// [Source: MaterialDataHandler]
    /// </summary>
    [GraphQLIgnore]
    public List<ExtrusionProducedRoll>? ProducedRolls { get; set; }
}