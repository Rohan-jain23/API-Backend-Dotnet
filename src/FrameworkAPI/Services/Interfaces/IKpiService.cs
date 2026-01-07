using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.DataLoaders;
using FrameworkAPI.Models.Enums;
using FrameworkAPI.Schema.Misc;
using WuH.Ruby.KpiDataHandler.Client;

namespace FrameworkAPI.Services.Interfaces;

public interface IKpiService
{
    NumericValue GetNumericValue(
        JobStandardKpiCacheDataLoader jobStandardKpiCacheDataLoader,
        MachineMetaDataBatchDataLoader machineMetaDataBatchDataLoader,
        KpiAttribute kpiAttribute,
        string machineId,
        string jobId,
        StandardJobKpis? standardJobKpis = null);

    Task<double?> GetValue(
        JobStandardKpiCacheDataLoader jobStandardKpiCacheDataLoader,
        MachineMetaDataBatchDataLoader machineMetaDataBatchDataLoader,
        KpiAttribute kpiAttribute,
        string machineId,
        string jobId,
        StandardJobKpis? standardJobKpis = null,
        CancellationToken cancellationToken = default);

    Task<double?> GetRawDouble(
        JobStandardKpiCacheDataLoader jobStandardKpiCacheDataLoader,
        KpiAttribute kpiAttribute,
        string machineId,
        string jobId,
        StandardJobKpis? standardJobKpis = null,
        CancellationToken cancellationToken = default);

    Task<bool?> GetBool(
        JobStandardKpiCacheDataLoader jobStandardKpiCacheDataLoader,
        KpiAttribute kpiAttribute,
        string machineId,
        string jobId,
        StandardJobKpis? standardJobKpis = null,
        CancellationToken cancellationToken = default);

    Task<string?> GetUnit(
        MachineMetaDataBatchDataLoader machineMetaDataBatchDataLoader,
        KpiAttribute kpiAttribute,
        string machineId,
        CancellationToken cancellationToken = default);

    Task<ProductionApprovalEvent?> GetProductionApproval(
        string machineId,
        IEnumerable<WuH.Ruby.Common.Core.TimeRange>? timeRanges,
        CancellationToken cancellationToken = default);

    Task<string?> GetUniqueIdOfRelatedProducedJobFromOtherMachine(
        JobStandardKpiCacheDataLoader jobStandardKpiCacheDataLoader,
        string machineId,
        string jobId,
        StandardJobKpis? standardJobKpis = null,
        CancellationToken cancellationToken = default);

    Task<ProductionTimes?> GetJobProductionTimes(
        JobStandardKpiCacheDataLoader jobStandardKpiCacheDataLoader,
        string machineId,
        string jobId,
        StandardJobKpis? standardJobKpis = null,
        CancellationToken cancellationToken = default);

    Task<double?> GetJobsRemainingProductionTime(
        JobStandardKpiCacheDataLoader jobStandardKpiCacheDataLoader,
        string machineId,
        string jobId,
        StandardJobKpis? standardJobKpis = null,
        CancellationToken cancellationToken = default);

    Task<OeeValues?> GetOverallEquipmentEffectiveness(
        JobStandardKpiCacheDataLoader jobStandardKpiCacheDataLoader,
        string machineId,
        string jobId,
        StandardJobKpis? standardJobKpis = null,
        CancellationToken cancellationToken = default);

    Task<ProducedPerformance?> GetJobPerformance(
        JobStandardKpiCacheDataLoader jobStandardKpiCacheDataLoader,
        MachineMetaDataBatchDataLoader machineMetaDataBatchDataLoader,
        string machineId,
        string jobId,
        StandardJobKpis? standardJobKpis = null,
        CancellationToken cancellationToken = default);

    Task<(string QualityUnit, string RateUnit, string TimeUnit)> GetProducedPerformanceUnits(
        MachineMetaDataBatchDataLoader machineMetaDataBatchDataLoader,
        string machineId,
        CancellationToken cancellationToken = default);
}