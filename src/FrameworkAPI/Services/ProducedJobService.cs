using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.Exceptions;
using FrameworkAPI.Schema.Misc;
using FrameworkAPI.Schema.ProducedJob;
using FrameworkAPI.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using WuH.Ruby.Common.Core;
using WuH.Ruby.KpiDataHandler.Client;
using WuH.Ruby.ProductionPeriodsDataHandler.Client;

namespace FrameworkAPI.Services;

public class ProducedJobService(
    IJobInfoCachingService jobInfoCachingService,
    IProductionPeriodsDataHandlerHttpClient productionPeriodsDataHandlerHttpClient,
    IMachineService machineService,
    ILogger<ProducedJobService> logger,
    IKpiEventQueueWrapper kpiEventQueueWrapper) : IProducedJobService
{
    private readonly IJobInfoCachingService _jobInfoCachingService = jobInfoCachingService;
    private readonly IProductionPeriodsDataHandlerHttpClient _productionPeriodsDataHandlerHttpClient = productionPeriodsDataHandlerHttpClient;
    private readonly IMachineService _machineService = machineService;
    private readonly ILogger<ProducedJobService> _logger = logger;
    private readonly IKpiEventQueueWrapper _kpiEventQueueWrapper = kpiEventQueueWrapper;

    public async Task<ProducedJob?> GetProducedJob(
        string machineId,
        MachineDepartment machineDepartment,
        MachineFamily machineFamily,
        DateTime? timestamp,
        CancellationToken cancellationToken)
    {
        if (timestamp.HasValue)
        {
            return await GetHistoricProducedJob(machineId, machineDepartment, machineFamily, timestamp.Value, cancellationToken);
        }

        return await GetActiveProducedJob(machineId, machineDepartment, machineFamily, cancellationToken);
    }

    public async Task<ProducedJob> GetProducedJob(
        string machineId,
        string jobId,
        MachineDepartment machineDepartment,
        MachineFamily machineFamily,
        CancellationToken cancellationToken)
    {
        var getJobInfoResponse = await _productionPeriodsDataHandlerHttpClient.GetJobInfo(cancellationToken, machineId, jobId);

        if (getJobInfoResponse.HasError)
        {
            throw new InternalServiceException(getJobInfoResponse.Error);
        }

        return ProducedJob.CreateInstance(getJobInfoResponse.Item, machineDepartment, machineFamily, machineQueryTimestamp: null)!;
    }

    public async Task<IEnumerable<ProducedJob>> GetLatestProducedJobs(
        List<string> filteredMachineIds,
        DateTime? from,
        DateTime? to,
        string? regexFilter,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        var getLatestJobsResponse = await _productionPeriodsDataHandlerHttpClient.GetAllJobInfos(
            cancellationToken,
            filteredMachineIds,
            from,
            to,
            skip,
            take,
            regexFilter,
            true);

        if (getLatestJobsResponse.HasError)
        {
            throw new InternalServiceException(getLatestJobsResponse.Error);
        }

        var results = new List<ProducedJob>();

        foreach (var latestJob in getLatestJobsResponse.Items)
        {
            var machineDepartment = await _machineService.GetMachineBusinessUnit(latestJob.MachineId, cancellationToken);
            var machineFamily = await _machineService.GetMachineFamily(latestJob.MachineId, cancellationToken);
            var jobInstance = ProducedJob.CreateInstance(
                latestJob,
                machineDepartment,
                machineFamily,
                machineQueryTimestamp: null);

            if (jobInstance is not null)
            {
                results.Add(jobInstance);
            }
        }

        return results;
    }

    public async Task<int> GetLatestProducedJobsTotalCount(
        List<string> filteredMachineIds,
        string? regexFilter,
        DateTime? from,
        DateTime? to,
        CancellationToken cancellationToken)
    {
        var getJobCountResponse = await _productionPeriodsDataHandlerHttpClient.GetAllJobIds(
            cancellationToken,
            filteredMachineIds,
            from,
            to,
            regexFilter);

        if (getJobCountResponse.HasError)
        {
            throw new InternalServiceException(getJobCountResponse.Error);
        }

        return getJobCountResponse.Items.Count;
    }

    public async Task<ProducedJob> UpdateProducedJobMachineTargetSpeed(
        double targetSpeed,
        string machineId,
        string associatedJob,
        string userId,
        CancellationToken cancellationToken)
    {
        var setTargetSpeedOfJobEventMessage = new SetTargetSpeedOfJobEventMessage(targetSpeed, machineId, associatedJob, DateTime.UtcNow, userId);
        var response = await _kpiEventQueueWrapper.SendSetTargetSpeedOfJobEventAndWaitForReply(setTargetSpeedOfJobEventMessage);

        HandleKpiEventQueueError(response);

        var department = await _machineService.GetMachineBusinessUnit(machineId, cancellationToken);
        var machineFamily = await _machineService.GetMachineFamily(machineId, cancellationToken);
        var producedJob = await GetProducedJob(machineId, associatedJob, department, machineFamily, cancellationToken);

        return producedJob;
    }

    public async Task<ProducedJob> UpdateProducedJobTargetSetupTimeInMin(
        double targetSetupTimeInMin,
        string machineId,
        string associatedJob,
        string userId,
        CancellationToken cancellationToken)
    {
        var setTargetSpeedOfJobEventMessage = new SetTargetSetupTimeOfJobEventMessage(targetSetupTimeInMin, machineId, associatedJob, DateTime.UtcNow, userId);
        var response = await _kpiEventQueueWrapper.SendSetTargetSetupTimeOfJobEventAndWaitForReply(setTargetSpeedOfJobEventMessage);

        HandleKpiEventQueueError(response);

        var department = await _machineService.GetMachineBusinessUnit(machineId, cancellationToken);
        var machineFamily = await _machineService.GetMachineFamily(machineId, cancellationToken);
        var producedJob = await GetProducedJob(machineId, associatedJob, department, machineFamily, cancellationToken);

        return producedJob;

    }

    public async Task<ProducedJob> UpdateProducedJobTargetDownTimeInMin(
        double targetDownTimeInMin,
        string machineId,
        string associatedJob,
        string userId,
        CancellationToken cancellationToken)
    {
        var setTargetSpeedOfJobEventMessage = new SetTargetDownTimeOfJobEventMessage(targetDownTimeInMin, machineId, associatedJob, DateTime.UtcNow, userId);
        var response = await _kpiEventQueueWrapper.SendSetTargetDownTimeOfJobEventAndWaitForReply(setTargetSpeedOfJobEventMessage);

        HandleKpiEventQueueError(response);

        var department = await _machineService.GetMachineBusinessUnit(machineId, cancellationToken);
        var machineFamily = await _machineService.GetMachineFamily(machineId, cancellationToken);
        var producedJob = await GetProducedJob(machineId, associatedJob, department, machineFamily, cancellationToken);

        return producedJob;
    }

    public async Task<ProducedJob> UpdateProducedJobTargetScrapCountDuringProduction(
        double targetScrapCountDuringProduction,
        string machineId,
        string associatedJob,
        string userId,
        CancellationToken cancellationToken)
    {
        var setTargetSpeedOfJobEventMessage = new SetTargetScrapDuringProductionOfJobEventMessage(targetScrapCountDuringProduction, machineId, associatedJob, DateTime.UtcNow, userId);
        var response = await _kpiEventQueueWrapper.SendSetTargetScrapDuringProductionOfJobEventAndWaitForReply(setTargetSpeedOfJobEventMessage);

        HandleKpiEventQueueError(response);

        var department = await _machineService.GetMachineBusinessUnit(machineId, cancellationToken);
        var machineFamily = await _machineService.GetMachineFamily(machineId, cancellationToken);
        var producedJob = await GetProducedJob(machineId, associatedJob, department, machineFamily, cancellationToken);

        return producedJob;

    }

    private void HandleKpiEventQueueError(InternalResponse response)
    {
        if (response.HasError)
        {
            throw response.Error.StatusCode switch
            {
                StatusCodes.Status400BadRequest => new ParameterInvalidException(response.Error.ErrorMessage),
                StatusCodes.Status204NoContent => new ParameterInvalidException($"{nameof(Schema.ProducedJob.ProducedJob)} not found"),
                _ => new InternalServiceException(response.Error)
            };
        }
    }

    private async Task<ProducedJob?> GetHistoricProducedJob(
        string machineId,
        MachineDepartment machineDepartment,
        MachineFamily machineFamily,
        DateTime timestamp,
        CancellationToken cancellationToken)
    {
        var response = await _productionPeriodsDataHandlerHttpClient.GetAllJobInfos(
                cancellationToken,
                machineId,
                timestamp,
                timestamp);

        if (response.HasError && response.Error.StatusCode != StatusCodes.Status204NoContent)
        {
            _logger.LogWarning($"Could not get historic job at {timestamp} by MachineId: {machineId}. ErrorMessage: {response.Error.ErrorMessage}");

            throw new InternalServiceException(response.Error);
        }

        return ProducedJob.CreateInstance(
            response.Items?.FirstOrDefault(),
            machineDepartment,
            machineFamily,
            timestamp);
    }

    private async Task<ProducedJob?> GetActiveProducedJob(
        string machineId,
        MachineDepartment machineDepartment,
        MachineFamily machineFamily,
        CancellationToken cancellationToken)
    {
        var (jobInfo, exception) = await _jobInfoCachingService.GetLatest(machineId, cancellationToken);

        if (exception is not null)
        {
            throw exception;
        }

        if (jobInfo is null)
        {
            return null;
        }

        if (jobInfo.EndTime is not null)
        {
            return null;
        }

        return ProducedJob.CreateInstance(
            jobInfo,
            machineDepartment,
            machineFamily,
            machineQueryTimestamp: null);
    }
}