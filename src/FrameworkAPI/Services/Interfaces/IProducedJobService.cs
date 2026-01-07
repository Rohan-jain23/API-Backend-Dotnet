using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.Schema.Misc;
using FrameworkAPI.Schema.ProducedJob;

namespace FrameworkAPI.Services.Interfaces;

public interface IProducedJobService
{
    Task<ProducedJob?> GetProducedJob(
        string machineId,
        MachineDepartment machineDepartment,
        MachineFamily machineFamily,
        DateTime? timestamp,
        CancellationToken cancellationToken);

    Task<ProducedJob> GetProducedJob(
        string machineId,
        string jobId,
        MachineDepartment machineDepartment,
        MachineFamily machineFamily,
        CancellationToken cancellationToken);

    Task<IEnumerable<ProducedJob>> GetLatestProducedJobs(
        List<string> filteredMachineIds,
        DateTime? from,
        DateTime? to,
        string? regexFilter,
        int skip,
        int take,
        CancellationToken cancellationToken);

    Task<int> GetLatestProducedJobsTotalCount(
        List<string> filteredMachineIds,
        string? regexFilter,
        DateTime? from,
        DateTime? to,
        CancellationToken cancellationToken);

    Task<ProducedJob> UpdateProducedJobMachineTargetSpeed(double targetSpeed, string machineId, string associatedJob, string userId, CancellationToken cancellationToken);

    Task<ProducedJob> UpdateProducedJobTargetSetupTimeInMin(double targetSetupTimeInMin, string machineId, string associatedJob, string userId, CancellationToken cancellationToken);

    Task<ProducedJob> UpdateProducedJobTargetDownTimeInMin(double targetDownTimeInMin, string machineId, string associatedJob, string userId, CancellationToken cancellationToken);

    Task<ProducedJob> UpdateProducedJobTargetScrapCountDuringProduction(double targetScrapCountDuringProduction, string machineId, string associatedJob, string userId, CancellationToken cancellationToken);
}