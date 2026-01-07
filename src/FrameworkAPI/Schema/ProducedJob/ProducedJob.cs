using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.DataLoaders;
using FrameworkAPI.Helpers;
using FrameworkAPI.Models.Enums;
using FrameworkAPI.Schema.Misc;
using FrameworkAPI.Services.Interfaces;
using HotChocolate;
using HotChocolate.Types;
using WuH.Ruby.KpiDataHandler.Client;
using WuH.Ruby.MachineSnapShooter.Client;

namespace FrameworkAPI.Schema.ProducedJob;

/// <summary>
/// Generic interface for produced job entities of all machine families.
/// </summary>
[InterfaceType]
public abstract class ProducedJob
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProducedJob"/> class.
    /// </summary>
    protected ProducedJob(
        WuH.Ruby.ProductionPeriodsDataHandler.Client.JobInfo jobInfo,
        DateTime? machineQueryTimestamp)
    {
        JobInfo = jobInfo;
        UniqueId = ProducedJobsHelper.SerializeProducedJobId(jobInfo.MachineId, jobInfo.JobId);
        JobId = jobInfo.JobId;
        MachineId = jobInfo.MachineId;
        ProductId = jobInfo.ProductId;
        StartTime = jobInfo.StartTime;
        EndTime = jobInfo.EndTime;
        IsActive = jobInfo.EndTime is null;

        if (jobInfo.TimeRanges is not null)
        {
            TimeRanges = jobInfo.TimeRanges.Select(x => new TimeRange(x));
        }

        MachineQueryTimestamp = machineQueryTimestamp;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProducedJob"/> class.
    /// </summary>
    protected ProducedJob(
        WuH.Ruby.ProductionPeriodsDataHandler.Client.JobInfo jobInfo,
        DateTime? machineQueryTimestamp,
        StandardJobKpis? standardJobKpis) : this(jobInfo, machineQueryTimestamp)
    {
        StandardJobKpis = standardJobKpis;
    }

    protected readonly WuH.Ruby.ProductionPeriodsDataHandler.Client.JobInfo JobInfo;

    /// <summary>
    /// This can be set, if this data is already fetched on creation of this instance (for example in PaperSackProductGroupStatisticsPerMachine).
    /// In that case, the data does not need to be resolved again.
    /// </summary>
    protected readonly StandardJobKpis? StandardJobKpis;

    /// <summary>
    /// Unique produced job identifier (in format MachineId_Job, like: "EQ12345_JobA-1234").
    /// The machineId needs to be added, because it is possible that a job with the same name is produced on different machines.
    /// [Source: FrameworkAPI]
    /// </summary>
    public string UniqueId { get; set; }

    /// <summary>
    /// Id of produced job (unique for the machine)
    /// (The id of the produced job might be equal to the productionRequestId, but some machines add a suffix,
    /// for example most printing presses behave like this: productionRequestId='JobA' => jobId='JobA-0023').
    /// [Source: ProductionPeriods]
    /// </summary>
    public string JobId { get; set; }

    /// <summary>
    /// Unique identifier of the machine this job is produced on (usually WuH equipment number, like: "EQ12345").
    /// [Source: ProductionPeriods]
    /// </summary>
    public string MachineId { get; set; }

    /// <summary>
    /// Id of the product which is produced (not the product definition).
    /// Usually, this is the article number.
    /// [Source: ProductionPeriods]
    /// </summary>
    public string? ProductId { get; set; }

    /// <summary>
    /// Customer for which this job is produced.
    /// [Source: MachineSnapshots]
    /// </summary>
    public SnapshotValuesDuringProduction<string> Customer() => new(SnapshotColumnIds.Customer, EndTime, MachineId, TimeRanges, MachineQueryTimestamp);

    /// <summary>
    /// Start timestamp of production in UTC (machine time).
    /// [Source: ProductionPeriods]
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// End timestamp of production in UTC (machine time) | This is 'null' if the job is active.
    /// [Source: ProductionPeriods]
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// True, if the job is currently active.
    /// [Source: ProductionPeriods]
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// All time ranges in which this job was produced
    /// (excludes job breaks like weekends;
    /// on active jobs the 'to' is the machine time at query moment).
    /// [Source: ProductionPeriods]
    /// </summary>
    public IEnumerable<TimeRange>? TimeRanges { get; set; }

    /// <summary>
    /// Target quantity to produce with this job.
    /// [Source: MachineSnapshots]
    /// </summary>
    public NumericSnapshotValuesDuringProduction JobSize() => new(SnapshotColumnIds.JobSize, EndTime, MachineId, TimeRanges, MachineQueryTimestamp);

    /// <summary>
    /// Cumulated minutes the machine was in each production status during this job.
    /// Also, the total times are provided.
    /// [Source: KPIs]
    /// </summary>
    public Task<ProductionTimes?> ProductionTimes(
        JobStandardKpiCacheDataLoader jobStandardKpiCacheDataLoader,
        [Service] IKpiService kpiService,
        CancellationToken cancellationToken)
        => kpiService.GetJobProductionTimes(jobStandardKpiCacheDataLoader, MachineId, JobId, StandardJobKpis, cancellationToken);

    /// <summary>
    /// Time which the remaining production of this job is estimated to take (is 'null' for completed jobs).
    /// Calculated based on average machine speed during production and remaining quantity to produce.
    /// [Source: KPIs]
    /// </summary>
    public Task<double?> RemainingProductionTimeInMin(
        JobStandardKpiCacheDataLoader jobStandardKpiCacheDataLoader,
        [Service] IKpiService kpiService,
        CancellationToken cancellationToken)
        => kpiService.GetJobsRemainingProductionTime(jobStandardKpiCacheDataLoader, MachineId, JobId, StandardJobKpis, cancellationToken);

    /// <summary>
    /// The overall time the production of this job should take,
    /// which includes setup time, job-related downtimes and scrap losses.
    /// This is the job size divided by the target value of the total good production rate,
    /// which is calculated by the target values for speed, scrap, downtime and setup.
    /// This value is 'null', if there is no target value for speed, scrap or downtime.
    /// If there is no expected setup time for this job, the actual setup time is taken.
    /// [Source: KPIs]
    /// </summary>
    public Task<double?> TargetJobTimeInMin(
        JobStandardKpiCacheDataLoader jobStandardKpiCacheDataLoader,
        [Service] IKpiService kpiService,
        CancellationToken cancellationToken)
        => kpiService.GetRawDouble(jobStandardKpiCacheDataLoader, KpiAttribute.TargetJobTimeInMin, MachineId, JobId, StandardJobKpis, cancellationToken);

    /// <summary>
    /// The overall equipment effectiveness (OEE) is a measure that identifies the percentage of production time that is truly productive.
    /// The OEE is calculated from three sub values (Availability, Effectiveness, Quality) which indicate how the productivity was lost.
    /// For the comparison of jobs, the OEE has its weaknesses as the necessary setup time (which depends on the previous job) has a huge influence.
    /// </summary>
    public Task<OeeValues?> OverallEquipmentEffectiveness(
        JobStandardKpiCacheDataLoader jobStandardKpiCacheDataLoader,
        [Service] IKpiService kpiService,
        CancellationToken cancellationToken)
        => kpiService.GetOverallEquipmentEffectiveness(jobStandardKpiCacheDataLoader, MachineId, JobId, StandardJobKpis, cancellationToken);

    /// <summary>
    /// Values that are measuring the productivity of a job (-> RUBYs alternative for OEE).
    /// These values are calculated by comparing actual values to target/expected values.
    /// Therefore, some values are 'null' when the related target values are not given for the job.
    /// If the target values are set properly, this is the perfect measure to evaluate the performance of this job.
    /// This basically says how well the production was running compared to the expectations.
    /// The greatest advantage over OEE is that the 'Total.WonProductivity' percentage allows comparison of jobs
    /// that not depends on the different setup efforts (which are highly dependant on the previous jobs).
    /// [Source: KPIs]
    /// </summary>
    public Task<ProducedPerformance?> Performance(
        JobStandardKpiCacheDataLoader jobStandardKpiCacheDataLoader,
        MachineMetaDataBatchDataLoader machineMetaDataBatchDataLoader,
        [Service] IKpiService kpiService,
        CancellationToken cancellationToken)
        => kpiService.GetJobPerformance(jobStandardKpiCacheDataLoader, machineMetaDataBatchDataLoader, MachineId, JobId, StandardJobKpis, cancellationToken);

    /// <summary>
    /// Timestamp of the machine query, when this job is queried via machine query.
    /// </summary>
    [GraphQLIgnore]
    internal DateTime? MachineQueryTimestamp { get; set; }

    internal static ProducedJob? CreateInstance(
        WuH.Ruby.ProductionPeriodsDataHandler.Client.JobInfo? jobInfo,
        MachineDepartment department,
        MachineFamily machineFamily,
        DateTime? machineQueryTimestamp)
    {
        if (jobInfo is null)
        {
            return null;
        }

        return department switch
        {
            MachineDepartment.Extrusion => new ExtrusionProducedJob(jobInfo, machineQueryTimestamp),
            MachineDepartment.Printing => new PrintingProducedJob(jobInfo, machineQueryTimestamp),
            MachineDepartment.PaperSack => new PaperSackProducedJob(jobInfo, machineFamily, machineQueryTimestamp),
            _ => new OtherProducedJob(jobInfo, machineQueryTimestamp)
        };
    }
}