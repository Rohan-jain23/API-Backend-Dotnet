using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.DataLoaders;
using FrameworkAPI.Extensions;
using FrameworkAPI.Helpers;
using FrameworkAPI.Models.Enums;
using FrameworkAPI.Schema.Misc;
using FrameworkAPI.Services.Interfaces;
using WuH.Ruby.KpiDataHandler.Client;

namespace FrameworkAPI.Services;

public class KpiService(
    IUnitService unitService,
    IMachineMetaDataService machineMetaDataService,
    IKpiDataHandlerClient kpiDataHandlerClient
) : IKpiService
{
    public NumericValue GetNumericValue(
        JobStandardKpiCacheDataLoader jobStandardKpiCacheDataLoader,
        MachineMetaDataBatchDataLoader machineMetaDataBatchDataLoader,
        KpiAttribute kpiAttribute,
        string machineId,
        string jobId,
        StandardJobKpis? standardJobKpis = null)
    {
        return new NumericValue(
            cancellationToken => GetValue(
                jobStandardKpiCacheDataLoader,
                machineMetaDataBatchDataLoader,
                kpiAttribute,
                machineId,
                jobId,
                standardJobKpis,
                cancellationToken),
            cancellationToken => GetUnit(machineMetaDataBatchDataLoader, kpiAttribute, machineId, cancellationToken));
    }

    public async Task<double?> GetValue(
        JobStandardKpiCacheDataLoader jobStandardKpiCacheDataLoader,
        MachineMetaDataBatchDataLoader machineMetaDataBatchDataLoader,
        KpiAttribute kpiAttribute,
        string machineId,
        string jobId,
        StandardJobKpis? standardJobKpis = null,
        CancellationToken cancellationToken = default)
    {
        standardJobKpis ??= await GetStandardJobKpis(jobStandardKpiCacheDataLoader, machineId, jobId, cancellationToken);

        var value = standardJobKpis?.GetDoubleValue(kpiAttribute);

        if (value is null) return null;

        var variableIdentifier = kpiAttribute.ToVariableIdentifier();
        var machineMetadata =
            await machineMetaDataService.GetMachineMetadata(
                machineMetaDataBatchDataLoader,
                machineId,
                variableIdentifier,
                cancellationToken);

        var convertedValue = unitService.CalculateSiValue(value.Value, machineMetadata);

        return convertedValue;
    }

    public async Task<double?> GetRawDouble(
        JobStandardKpiCacheDataLoader jobStandardKpiCacheDataLoader,
        KpiAttribute kpiAttribute,
        string machineId,
        string jobId,
        StandardJobKpis? standardJobKpis = null,
        CancellationToken cancellationToken = default)
    {
        standardJobKpis ??= await GetStandardJobKpis(jobStandardKpiCacheDataLoader, machineId, jobId, cancellationToken);
        return standardJobKpis?.GetDoubleValue(kpiAttribute);
    }

    public async Task<bool?> GetBool(
        JobStandardKpiCacheDataLoader jobStandardKpiCacheDataLoader,
        KpiAttribute kpiAttribute,
        string machineId,
        string jobId,
        StandardJobKpis? standardJobKpis = null,
        CancellationToken cancellationToken = default)
    {
        standardJobKpis ??= await GetStandardJobKpis(jobStandardKpiCacheDataLoader, machineId, jobId, cancellationToken);
        return standardJobKpis?.GetBoolValue(kpiAttribute);
    }

    public async Task<string?> GetUnit(
        MachineMetaDataBatchDataLoader machineMetaDataBatchDataLoader,
        KpiAttribute kpiAttribute,
        string machineId,
        CancellationToken cancellationToken = default)
    {
        var variableIdentifier = kpiAttribute.ToVariableIdentifier();
        var machineMetadata =
            await machineMetaDataService.GetMachineMetadata(
                machineMetaDataBatchDataLoader,
                machineId,
                variableIdentifier,
                cancellationToken);

        var unit = unitService.GetSiUnit(machineMetadata);

        return unit;
    }

    public async Task<ProductionApprovalEvent?> GetProductionApproval(
        string machineId,
        IEnumerable<WuH.Ruby.Common.Core.TimeRange>? timeRanges,
        CancellationToken cancellationToken = default)
    {
        var timeRangeList = timeRanges?.ToList();
        if (timeRangeList is null || timeRangeList.Count == 0)
            return null;

        var minDate = timeRangeList.Select(tr => tr.From).Min();
        var maxDate = timeRangeList.Select(tr => tr.To).Max();

        var productionApprovalByTimeRangeResponse = await kpiDataHandlerClient.GetProductionApprovalByTimespan(
            cancellationToken, machineId, minDate, maxDate);

        return productionApprovalByTimeRangeResponse.HasError switch
        {
            true when productionApprovalByTimeRangeResponse.Error.StatusCode == (int)HttpStatusCode.NoContent =>
                null,
            true =>
                throw new Exception(
                    $"ProductionApprovalByTimespan request failed: {productionApprovalByTimeRangeResponse.Error.ErrorMessage}"),
            _ =>
                new ProductionApprovalEvent(
                    productionApprovalByTimeRangeResponse.Item.TriggerDate,
                    productionApprovalByTimeRangeResponse.Item.Signature)
        };
    }

    public async Task<string?> GetUniqueIdOfRelatedProducedJobFromOtherMachine(
        JobStandardKpiCacheDataLoader jobStandardKpiCacheDataLoader,
        string machineId,
        string jobId,
        StandardJobKpis? standardJobKpis = null,
        CancellationToken cancellationToken = default)
    {
        standardJobKpis ??= await GetStandardJobKpis(jobStandardKpiCacheDataLoader, machineId, jobId, cancellationToken);

        var relatedJobMachineId = standardJobKpis?.RelatedJobMachineId;
        var relatedJobId = standardJobKpis?.RelatedJobId;

        if (relatedJobMachineId is null || relatedJobId is null) return null;

        return ProducedJobsHelper.SerializeProducedJobId(relatedJobMachineId, relatedJobId);
    }

    public async Task<ProductionTimes?> GetJobProductionTimes(
        JobStandardKpiCacheDataLoader jobStandardKpiCacheDataLoader,
        string machineId,
        string jobId,
        StandardJobKpis? standardJobKpis = null,
        CancellationToken cancellationToken = default)
    {
        standardJobKpis ??= await GetStandardJobKpis(jobStandardKpiCacheDataLoader, machineId, jobId, cancellationToken);

        if (standardJobKpis is null) return null;

        return new ProductionTimes(
            standardJobKpis.TotalTimeInMin,
            standardJobKpis.TotalPlannedProductionTimeInMin,
            standardJobKpis.NotQueryRelatedTimeInMin,
            standardJobKpis.ProductionData.ProductionTimeInMin,
            standardJobKpis.ProductionData.DownTimeInMin - standardJobKpis.ProductionData.JobRelatedDownTimeInMin,
            standardJobKpis.ProductionData.JobRelatedDownTimeInMin,
            standardJobKpis.ProductionData.SetupTimeInMin,
            standardJobKpis.ProductionData.ScrapTimeInMin,
            standardJobKpis.ProductionData.PlannedNoProductionTimeInMin);
    }

    public async Task<double?> GetJobsRemainingProductionTime(
        JobStandardKpiCacheDataLoader jobStandardKpiCacheDataLoader,
        string machineId,
        string jobId,
        StandardJobKpis? standardJobKpis = null,
        CancellationToken cancellationToken = default)
    {
        standardJobKpis ??= await GetStandardJobKpis(jobStandardKpiCacheDataLoader, machineId, jobId, cancellationToken);

        if (standardJobKpis?.ProductionData is null) return null;

        if (standardJobKpis.ProductionData.ProductionTimeInMin <= 1 || // -> 'null' during setup
            standardJobKpis.ProductionData.GoodProductionCount <= 1 ||
            standardJobKpis.ProductionData.AverageProductionSpeed is null ||
            standardJobKpis.ProductionData.AverageProductionSpeed <= 1 ||
            standardJobKpis.JobSize is null ||
            standardJobKpis.JobSize <= 1)
        {
            return null;
        }

        var remainingQuantity = standardJobKpis.JobSize.Value - standardJobKpis.ProductionData.GoodProductionCount;
        if (remainingQuantity <= standardJobKpis.ProductionData.AverageProductionSpeed) return 0;

        return remainingQuantity / standardJobKpis.ProductionData.AverageProductionSpeed.Value;
    }

    public async Task<OeeValues?> GetOverallEquipmentEffectiveness(
        JobStandardKpiCacheDataLoader jobStandardKpiCacheDataLoader,
        string machineId,
        string jobId,
        StandardJobKpis? standardJobKpis = null,
        CancellationToken cancellationToken = default)
    {
        standardJobKpis ??= await GetStandardJobKpis(jobStandardKpiCacheDataLoader, machineId, jobId, cancellationToken);

        if (standardJobKpis is null) return null;

        return new OeeValues(
            standardJobKpis.OEE,
            standardJobKpis.Availability,
            standardJobKpis.Effectiveness,
            standardJobKpis.QualityRatio);
    }

    public async Task<ProducedPerformance?> GetJobPerformance(
        JobStandardKpiCacheDataLoader jobStandardKpiCacheDataLoader,
        MachineMetaDataBatchDataLoader machineMetaDataBatchDataLoader,
        string machineId,
        string jobId,
        StandardJobKpis? standardJobKpis = null,
        CancellationToken cancellationToken = default)
    {
        standardJobKpis ??= await GetStandardJobKpis(jobStandardKpiCacheDataLoader, machineId, jobId, cancellationToken);

        if (standardJobKpis is null) return null;

        var units = await GetProducedPerformanceUnits(machineMetaDataBatchDataLoader, machineId, cancellationToken);

        var speed = new ProducedPerformanceValue(
            standardJobKpis.ProductionData?.AverageProductionSpeed,
            standardJobKpis.TargetSpeed,
            standardJobKpis.TargetSpeedSource.ToFrameworkApiEnum(),
            standardJobKpis.PerformanceComparedToTargets?.LostTimeDueToSpeedInMin,
            standardJobKpis.PerformanceComparedToTargets?.WonProductivityDueToSpeedInPercent,
            units.RateUnit);

        var setup = new ProducedPerformanceValue(
            standardJobKpis.ProductionData?.SetupTimeInMin,
            standardJobKpis.TargetSetupTimeInMin,
            standardJobKpis.TargetSetupTimeSource.ToFrameworkApiEnum(),
            standardJobKpis.PerformanceComparedToTargets?.LostTimeDueToSetupInMin,
            standardJobKpis.PerformanceComparedToTargets?.WonProductivityDueToSetupInPercent,
            units.TimeUnit);

        var downtime = new ProducedPerformanceValue(
            standardJobKpis.ProductionData?.JobRelatedDownTimeInMin,
            standardJobKpis.TargetDowntimeInMin,
            standardJobKpis.TargetDowntimeSource.ToFrameworkApiEnum(),
            standardJobKpis.PerformanceComparedToTargets?.LostTimeDueToDowntimeInMin,
            standardJobKpis.PerformanceComparedToTargets?.WonProductivityDueToDowntimeInPercent,
            units.TimeUnit);

        var scrap = new ProducedPerformanceValue(
            standardJobKpis.ProductionData?.ScrapProductionCount - standardJobKpis.ProductionData?.SetupScrapCount,
            standardJobKpis.TargetScrapCountDuringProduction,
            standardJobKpis.TargetScrapSource.ToFrameworkApiEnum(),
            standardJobKpis.PerformanceComparedToTargets?.LostTimeDueToScrapInMin,
            standardJobKpis.PerformanceComparedToTargets?.WonProductivityDueToScrapInPercent,
            units.QualityUnit);

        var total = new ProducedPerformanceValue(
            standardJobKpis.ThroughputRate,
            standardJobKpis.TargetThroughputRate,
            null,
            standardJobKpis.PerformanceComparedToTargets?.LostTimeTotalInMin,
            standardJobKpis.PerformanceComparedToTargets?.WonProductivityTotalInPercent,
            units.RateUnit);

        return new ProducedPerformance(speed, setup, downtime, scrap, total);
    }

    public async Task<(string QualityUnit, string RateUnit, string TimeUnit)> GetProducedPerformanceUnits(
        MachineMetaDataBatchDataLoader machineMetaDataBatchDataLoader,
        string machineId,
        CancellationToken cancellationToken = default)
    {
        var timeUnit = "label.minutesShort";
        var qualityUnit = await GetUnit(machineMetaDataBatchDataLoader, KpiAttribute.GoodProductionCount, machineId,
            cancellationToken);

        var rateUnit = string.Empty;
        if (qualityUnit is not null)
        {
            var isQuantityUnitAi18nLabel = Constants.Units.SpecialUnitsTranslation.Values.Contains(qualityUnit);
            var qualityUnitPart = isQuantityUnitAi18nLabel ? $"{{{{ {qualityUnit} }}}}" : qualityUnit;
            rateUnit =
                $"{qualityUnitPart}/{{{{ {timeUnit} }}}}"; // for example => '{{ label.items }}/{{ label.minutesShort }}'
        }

        return (qualityUnit ?? string.Empty, rateUnit, timeUnit);
    }

    private static async Task<StandardJobKpis?> GetStandardJobKpis(
        JobStandardKpiCacheDataLoader jobStandardKpiCacheDataLoader,
        string machineId,
        string jobId,
        CancellationToken cancellationToken = default)
    {
        var (standardJobKpis, exception) = await jobStandardKpiCacheDataLoader.LoadAsync((machineId, jobId), cancellationToken);

        if (exception is not null) throw exception;

        return standardJobKpis;
    }
}