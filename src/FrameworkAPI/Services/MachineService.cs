using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.Exceptions;
using FrameworkAPI.Helpers;
using FrameworkAPI.Schema.Misc;
using FrameworkAPI.Services.Interfaces;
using Microsoft.Extensions.Logging;
using WuH.Ruby.MachineDataHandler.Client;
using Machine = FrameworkAPI.Schema.Machine.Machine;
using MachineFamily = FrameworkAPI.Schema.Misc.MachineFamily;

namespace FrameworkAPI.Services;

public class MachineService(
    IMachineCachingService machineCachingService,
    ILogger<MachineService> logger) : IMachineService
{
    private readonly IMachineCachingService _machineCachingService = machineCachingService;
    private readonly ILogger<MachineService> _logger = logger;

    public async Task<bool> DoesMachineExist(string machineId, CancellationToken cancellationToken)
    {
        var internalMachine = await _machineCachingService.GetMachine(machineId, cancellationToken);
        return internalMachine is not null;
    }

    public async Task<Machine> GetMachine(string machineId, CancellationToken cancellationToken)
    {
        var internalMachine = await _machineCachingService.GetMachine(machineId, cancellationToken)
                              ?? throw new InternalServiceException(
                                  $"{nameof(IMachineCachingService)}: MachineId '{machineId}' does not exist", 400);

        return Machine.CreateInstance(internalMachine);
    }

    public async Task<MachineDepartment> GetMachineBusinessUnit(
        string machineId,
        CancellationToken cancellationToken)
    {
        var getMachinesResponse = await _machineCachingService.GetMachinesAsInternalListResponse(cancellationToken);

        if (getMachinesResponse.HasError)
        {
            throw new InternalServiceException(getMachinesResponse.Error);
        }

        var businessUnit = getMachinesResponse.Items.First(machine => machine.MachineId == machineId).BusinessUnit;

        return businessUnit.MapToSchemaMachineDepartment();
    }

    public async Task<MachineFamily> GetMachineFamily(string machineId, CancellationToken cancellationToken)
    {
        var getMachinesResponse = await _machineCachingService.GetMachinesAsInternalListResponse(cancellationToken);

        if (getMachinesResponse.HasError)
        {
            throw new InternalServiceException(getMachinesResponse.Error);
        }

        var machineFamily = getMachinesResponse.Items
            .First(machine => machine.MachineId == machineId).MachineFamilyEnum;

        return machineFamily.MapToSchemaMachineFamily();
    }

    public async Task<List<string>> GetMachineIdsByFilter(
        string? machineIdFilter,
        MachineDepartment? machineDepartmentFilter,
        MachineFamily? machineFamilyFilter,
        CancellationToken cancellationToken)
    {
        var getMachinesResponse =
            await _machineCachingService.GetMachinesAsInternalListResponse(cancellationToken: cancellationToken);

        if (getMachinesResponse.HasError && getMachinesResponse.Error.StatusCode == 204)
        {
            _logger.LogWarning("There are no machines connected to Ruby.");
            return new List<string>();
        }

        if (getMachinesResponse.HasError)
        {
            throw new InternalServiceException(getMachinesResponse.Error);
        }

        var filteredMachineIds = getMachinesResponse.Items
            .Where(machine =>
                IsMachineMatchingFilter(machine, machineIdFilter, machineDepartmentFilter, machineFamilyFilter))
            .Select(machine => machine.MachineId)
            .ToList();

        return filteredMachineIds;
    }

    public async Task<IEnumerable<Machine>> GetAllMachines(CancellationToken cancellationToken)
    {
        var getMachinesResponse = await _machineCachingService.GetMachinesAsInternalListResponse(cancellationToken);

        if (getMachinesResponse.HasError)
        {
            throw new InternalServiceException(getMachinesResponse.Error);
        }

        return getMachinesResponse.Items.Select(Machine.CreateInstance);
    }

    private static bool IsMachineMatchingFilter(
        WuH.Ruby.MachineDataHandler.Client.Machine machine,
        string? machineIdFilter,
        MachineDepartment? machineDepartmentFilter,
        MachineFamily? machineFamilyFilter)
    {
        var isMatchingMachineId = machineIdFilter is null || machineIdFilter == machine.MachineId;
        var isMatchingMachineDepartment = machineDepartmentFilter is null ||
                                          machineDepartmentFilter.Value.MapToInternalBusinessUnit() ==
                                          machine.BusinessUnit;
        var isMatchingMachineFamily = machineFamilyFilter is null ||
                                      machineFamilyFilter.Value.MapToInternalMachineFamily() ==
                                      machine.MachineFamilyEnum;

        return isMatchingMachineId && isMatchingMachineDepartment && isMatchingMachineFamily;
    }
}