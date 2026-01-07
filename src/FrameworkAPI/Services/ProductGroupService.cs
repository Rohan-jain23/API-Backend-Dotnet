using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.DataLoaders;
using FrameworkAPI.Exceptions;
using FrameworkAPI.Helpers;
using FrameworkAPI.Models.Enums;
using FrameworkAPI.Schema.Misc;
using FrameworkAPI.Schema.ProducedJob;
using FrameworkAPI.Schema.ProductGroup;
using FrameworkAPI.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using WuH.Ruby.KpiDataHandler.Client;
using WuH.Ruby.ProductionPeriodsDataHandler.Client;

namespace FrameworkAPI.Services;

public class ProductGroupService(
    IMachineService machineService,
    IKpiService kpiService,
    IKpiEventQueueWrapper kpiEventQueueWrapper,
    IKpiDataHandlerClient kpiDataHandlerClient,
    IProductionPeriodsDataHandlerHttpClient productionPeriodsDataHandlerHttpClient) : IProductGroupService
{
    private readonly IMachineService _machineService = machineService;
    private readonly IKpiService _kpiService = kpiService;
    private readonly IKpiEventQueueWrapper _kpiEventQueueWrapper = kpiEventQueueWrapper;
    private readonly IKpiDataHandlerClient _kpiDataHandlerClient = kpiDataHandlerClient;
    private readonly IProductionPeriodsDataHandlerHttpClient _productionPeriodsDataHandlerHttpClient = productionPeriodsDataHandlerHttpClient;

    public async Task<PaperSackProductGroup> GetPaperSackProductGroupById(string productGroupId, CancellationToken cancellationToken)
    {
        var productGroupResponse = await _kpiDataHandlerClient.GetPaperSackProductGroupById(cancellationToken, productGroupId);

        if (productGroupResponse.HasError)
        {
            throw new InternalServiceException(productGroupResponse.Error);
        }

        return new PaperSackProductGroup(productGroupResponse.Item);
    }

    public async Task<PaperSackProductGroup?> GetPaperSackProductGroupByJobId(string machineId, string jobId, CancellationToken cancellationToken)
    {
        var productGroupResponse = await _kpiDataHandlerClient.GetPaperSackProductGroupByJobId(cancellationToken, machineId, jobId);

        if (productGroupResponse.HasError && productGroupResponse.Error.StatusCode != StatusCodes.Status204NoContent)
        {
            throw new InternalServiceException(productGroupResponse.Error);
        }

        return productGroupResponse.Item is null ? null : new PaperSackProductGroup(productGroupResponse.Item);
    }

    public async Task<IEnumerable<PaperSackProductGroup>> GetPaperSackProductGroups(
        string? regexFilter,
        int limit,
        int offset,
        ProductGroupSortOption sortOption,
        CancellationToken cancellationToken)
    {
        var productGroupResponse = await _kpiDataHandlerClient.GetPaperSackProductGroups(cancellationToken, regexFilter, limit, offset, sortOption.MapToInternalProductGroupSortOption());

        if (productGroupResponse.HasError && productGroupResponse.Error.StatusCode == 204)
        {
            return [];
        }

        if (productGroupResponse.HasError)
        {
            throw new InternalServiceException(productGroupResponse.Error);
        }

        var convertedProductGroups = productGroupResponse.Items
            .Select(productGroup => new PaperSackProductGroup(productGroup))
            .ToList();

        return convertedProductGroups;
    }

    public async Task<int> GetPaperSackProductGroupsCount(string? regexFilter, CancellationToken cancellationToken)
    {
        var productGroupResponse = await _kpiDataHandlerClient.GetPaperSackProductGroupsCount(cancellationToken, regexFilter);

        if (productGroupResponse.HasError)
        {
            throw new InternalServiceException(productGroupResponse.Error);
        }

        return productGroupResponse.Item;
    }

    public async Task<Dictionary<string, PaperSackProductGroupStatisticsPerMachine?>> GetPaperSackProductGroupStatisticsPerMachine(
        ProductGroupStandardKpiCacheDataLoader productGroupStandardKpiCacheDataLoader,
        MachineMetaDataBatchDataLoader machineMetaDataBatchDataLoader,
        string paperSackProductGroupId,
        DateTime from,
        DateTime? to,
        string? productIdFilter,
        string? machineIdFilter,
        PaperSackMachineFamilyFilter machineFamilyFilter,
        CancellationToken cancellationToken)
    {
        var machineIds = new List<string>();
        if (machineFamilyFilter is PaperSackMachineFamilyFilter.Both or PaperSackMachineFamilyFilter.Tuber)
        {
            machineIds.AddRange(await _machineService.GetMachineIdsByFilter(machineIdFilter, MachineDepartment.PaperSack, MachineFamily.PaperSackTuber, cancellationToken));
        }

        if (machineFamilyFilter is PaperSackMachineFamilyFilter.Both or PaperSackMachineFamilyFilter.Bottomer)
        {
            machineIds.AddRange(await _machineService.GetMachineIdsByFilter(machineIdFilter, MachineDepartment.PaperSack, MachineFamily.PaperSackBottomer, cancellationToken));
        }

        if (machineIds.Count == 0)
        {
            return [];
        }

        var (paperSackProductGroupKpis, exception) = await productGroupStandardKpiCacheDataLoader.LoadAsync(
            (paperSackProductGroupId, machineIds, from, to, productIdFilter), cancellationToken);

        if (exception is not null)
        {
            throw exception;
        }

        var kpisByMachineId = new Dictionary<string, PaperSackProductGroupStatisticsPerMachine?>();

        foreach (var (machineId, paperSackProductGroupKpi) in paperSackProductGroupKpis!)
        {
            if (paperSackProductGroupKpi is null)
            {
                continue;
            }

            var machineFamily = await _machineService.GetMachineFamily(machineId, cancellationToken);
            var (qualityUnit, rateUnit, timeUnit) = await _kpiService.GetProducedPerformanceUnits(machineMetaDataBatchDataLoader, machineId, cancellationToken);

            var jobIds = paperSackProductGroupKpi.StandardJobKpis.Select(kpi => kpi.JobId).ToList();
            var getJobInfosByIdsResponse = await _productionPeriodsDataHandlerHttpClient.GetJobInfosByIds(cancellationToken, machineId, jobIds);
            if (getJobInfosByIdsResponse.HasError && getJobInfosByIdsResponse.Error.StatusCode != StatusCodes.Status204NoContent)
            {
                throw new InternalServiceException(getJobInfosByIdsResponse.Error);
            }

            kpisByMachineId[machineId] = MapToPaperSackProductGroupStatisticsPerMachine(
                machineId,
                machineFamily,
                getJobInfosByIdsResponse.Items,
                paperSackProductGroupKpi,
                rateUnit,
                timeUnit,
                qualityUnit);
        }

        return kpisByMachineId;
    }

    public async Task<PaperSackProductGroup> UpdatePaperSackProductGroupNote(string paperSackProductGroupId, string? note, string userId, CancellationToken cancellationToken)
    {
        var setOverallNoteOfProductGroupEventMessage = new SetOverallNoteOfProductGroupEventMessage(
            paperSackProductGroupId, note, DateTime.UtcNow, userId);
        var response = await _kpiEventQueueWrapper.SendSetOverallNoteOfProductGroupEventAndWaitForReply(setOverallNoteOfProductGroupEventMessage);

        if (response.HasError)
        {
            throw response.Error.StatusCode switch
            {
                StatusCodes.Status400BadRequest => new ParameterInvalidException(response.Error.ErrorMessage),
                StatusCodes.Status204NoContent => new ParameterInvalidException($"{nameof(PaperSackProductGroup)} not found"),
                _ => new InternalServiceException(response.Error)
            };
        }

        return await GetPaperSackProductGroupById(paperSackProductGroupId, cancellationToken);
    }

    public async Task<PaperSackProductGroup> UpdatePaperSackProductGroupMachineNote(string paperSackProductGroupId, string machineId, string? note, string userId, CancellationToken cancellationToken)
    {
        var setMachineNoteOfProductGroupEventMessage = new SetMachineNoteOfProductGroupEventMessage(
            paperSackProductGroupId, machineId, note, DateTime.UtcNow, userId);
        var response = await _kpiEventQueueWrapper.SendSetMachineNoteOfProductGroupEventAndWaitForReply(setMachineNoteOfProductGroupEventMessage);

        if (response.HasError)
        {
            throw response.Error.StatusCode switch
            {
                StatusCodes.Status400BadRequest => new ParameterInvalidException(response.Error.ErrorMessage),
                StatusCodes.Status204NoContent => new ParameterInvalidException($"{nameof(PaperSackProductGroup)} not found"),
                _ => new InternalServiceException(response.Error)
            };
        }

        return await GetPaperSackProductGroupById(paperSackProductGroupId, cancellationToken);
    }

    public async Task<PaperSackProductGroup> UpdatePaperSackProductGroupMachineTargetSpeed(string paperSackProductGroupId, string machineId, double? targetSpeed, string userId, CancellationToken cancellationToken)
    {
        var setMachineTargetSpeedOfProductGroupEventMessage = new SetMachineTargetSpeedOfProductGroupEventMessage(
            paperSackProductGroupId, machineId, targetSpeed, DateTime.UtcNow, userId);
        var response = await _kpiEventQueueWrapper.SendSetMachineTargetSpeedOfProductGroupEventAndWaitForReply(setMachineTargetSpeedOfProductGroupEventMessage);

        if (response.HasError)
        {
            throw response.Error.StatusCode switch
            {
                StatusCodes.Status400BadRequest => new ParameterInvalidException(response.Error.ErrorMessage),
                StatusCodes.Status204NoContent => new ParameterInvalidException($"{nameof(PaperSackProductGroup)} not found"),
                _ => new InternalServiceException(response.Error)
            };
        }

        return await GetPaperSackProductGroupById(paperSackProductGroupId, cancellationToken);
    }

    public Dictionary<string, NumericValue> MapTargetSpeedPerMachineToSchema(
        MachineMetaDataBatchDataLoader machineMetaDataBatchDataLoader,
        IKpiService kpiService,
        Dictionary<string, double> targetSpeedPerMachine)
    {
        var mappedTargetSpeedPerMachine = new Dictionary<string, NumericValue>();

        foreach (var (machineId, targetSpeed) in targetSpeedPerMachine)
        {
            var targetSpeedAsNumericValue = new NumericValue(
                _ => Task.FromResult((double?)targetSpeed),
                ct => kpiService.GetUnit(machineMetaDataBatchDataLoader, KpiAttribute.TargetSpeed, machineId, ct));

            mappedTargetSpeedPerMachine.Add(machineId, targetSpeedAsNumericValue);
        }

        return mappedTargetSpeedPerMachine;
    }

    private static PaperSackProductGroupStatisticsPerMachine MapToPaperSackProductGroupStatisticsPerMachine(
        string machineId,
        MachineFamily machineFamily,
        List<JobInfo> jobInfos,
        PaperSackProductGroupKpis paperSackProductGroupKpis,
        string rateUnit,
        string timeUnit,
        string qualityUnit)
    {
        var totalProducedGoodQuantity = paperSackProductGroupKpis.StandardJobKpis.Sum(x => x.ProductionData.GoodProductionCount);

        var productTimes = new ProductionTimes(
            paperSackProductGroupKpis.StandardProductGroupKpis.TotalTimeInMin,
            paperSackProductGroupKpis.StandardProductGroupKpis.TotalPlannedProductionTimeInMin,
            paperSackProductGroupKpis.StandardProductGroupKpis.NotQueryRelatedTimeInMin,
            paperSackProductGroupKpis.StandardProductGroupKpis.ProductionData.ProductionTimeInMin,
            paperSackProductGroupKpis.StandardProductGroupKpis.ProductionData.DownTimeInMin - paperSackProductGroupKpis.StandardProductGroupKpis.ProductionData.JobRelatedDownTimeInMin,
            paperSackProductGroupKpis.StandardProductGroupKpis.ProductionData.JobRelatedDownTimeInMin,
            paperSackProductGroupKpis.StandardProductGroupKpis.ProductionData.SetupTimeInMin,
            paperSackProductGroupKpis.StandardProductGroupKpis.ProductionData.ScrapTimeInMin,
            paperSackProductGroupKpis.StandardProductGroupKpis.ProductionData.PlannedNoProductionTimeInMin);

        var speed = new ProducedPerformanceValue(
            paperSackProductGroupKpis.StandardProductGroupKpis.ProductionData?.AverageProductionSpeed,
            paperSackProductGroupKpis.StandardProductGroupKpis.TargetSpeed,
            null,
            paperSackProductGroupKpis.StandardProductGroupKpis.PerformanceComparedToTargets?.LostTimeDueToSpeedInMin,
            paperSackProductGroupKpis.StandardProductGroupKpis.PerformanceComparedToTargets?.WonProductivityDueToSpeedInPercent,
            rateUnit);
        var setup = new ProducedPerformanceValue(
            paperSackProductGroupKpis.StandardProductGroupKpis.ProductionData?.SetupTimeInMin,
            paperSackProductGroupKpis.StandardProductGroupKpis.TargetSetupTimeInMin,
            null,
            paperSackProductGroupKpis.StandardProductGroupKpis.PerformanceComparedToTargets?.LostTimeDueToSetupInMin,
            paperSackProductGroupKpis.StandardProductGroupKpis.PerformanceComparedToTargets?.WonProductivityDueToSetupInPercent,
            timeUnit);
        var downtime = new ProducedPerformanceValue(
            paperSackProductGroupKpis.StandardProductGroupKpis.ProductionData?.JobRelatedDownTimeInMin,
            paperSackProductGroupKpis.StandardProductGroupKpis.TargetDowntimeInMin,
            null,
            paperSackProductGroupKpis.StandardProductGroupKpis.PerformanceComparedToTargets?.LostTimeDueToDowntimeInMin,
            paperSackProductGroupKpis.StandardProductGroupKpis.PerformanceComparedToTargets?.WonProductivityDueToDowntimeInPercent,
            timeUnit);
        var scrap = new ProducedPerformanceValue(
            paperSackProductGroupKpis.StandardProductGroupKpis.ProductionData?.ScrapProductionCount - paperSackProductGroupKpis.StandardProductGroupKpis.ProductionData?.SetupScrapCount,
            paperSackProductGroupKpis.StandardProductGroupKpis.TargetTotalScrapCount,
            null,
            paperSackProductGroupKpis.StandardProductGroupKpis.PerformanceComparedToTargets?.LostTimeDueToScrapInMin,
            paperSackProductGroupKpis.StandardProductGroupKpis.PerformanceComparedToTargets?.WonProductivityDueToScrapInPercent,
            qualityUnit);
        var total = new ProducedPerformanceValue(
            paperSackProductGroupKpis.StandardProductGroupKpis.ThroughputRate,
            paperSackProductGroupKpis.StandardProductGroupKpis.TargetThroughputRate,
            null,
            paperSackProductGroupKpis.StandardProductGroupKpis.PerformanceComparedToTargets?.LostTimeTotalInMin,
            paperSackProductGroupKpis.StandardProductGroupKpis.PerformanceComparedToTargets?.WonProductivityTotalInPercent,
            rateUnit);

        var performance = new ProducedPerformance(speed, setup, downtime, scrap, total);

        var producedJobs = paperSackProductGroupKpis.StandardJobKpis
            .Select(kpis => CreateProducedJobInstance(machineId, machineFamily, kpis, jobInfos))
            .ToList();

        var bestJob = paperSackProductGroupKpis.StandardJobKpis
            .OrderByDescending(kpis => kpis?.PerformanceComparedToTargets?.WonProductivityTotalInPercent)
            .Select(kpis => CreateProducedJobInstance(machineId, machineFamily, kpis, jobInfos))
            .FirstOrDefault();

        var recommendedTargetSpeed = new NumericValue(paperSackProductGroupKpis.StandardProductGroupKpis.RecommendedTargetSpeed, rateUnit);

        var speedHistogram = paperSackProductGroupKpis.StandardProductGroupKpis.HistogramItems?
            .Select(histogramItem => new SpeedHistogramItem(
                histogramItem.SpeedLevel,
                histogramItem.DurationInMin,
                histogramItem.CapacityUtilizationRate)
            ).ToList();

        return new PaperSackProductGroupStatisticsPerMachine(
            machineId,
            producedJobs,
            bestJob,
            totalProducedGoodQuantity,
            productTimes,
            performance,
            recommendedTargetSpeed,
            speedHistogram);
    }

    private static PaperSackProducedJob CreateProducedJobInstance(string machineId, MachineFamily machineFamily, StandardJobKpis kpis, List<JobInfo> jobInfos)
    {
        var jobInfoOfThisJob = jobInfos?.FirstOrDefault(x => x.JobId == kpis.JobId);
        if (jobInfoOfThisJob is null)
        {
            // Don't fail here. Instead proceed with the data we have.
            jobInfoOfThisJob = new JobInfo
            {
                JobId = kpis.JobId,
                MachineId = machineId,
                ProductId = $"ERROR: No corresponding job info found in {nameof(ProductGroupService)}.",
                StartTime = kpis.ProductionData.From,
                EndTime = kpis.ProductionData.To,
                TimeRanges = kpis.ProductionData.AllQueriedTimeRanges,
            };
        }

        return new PaperSackProducedJob(jobInfoOfThisJob, machineFamily, machineQueryTimestamp: null, kpis);
    }
}