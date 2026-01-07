using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.Schema.Machine;
using FrameworkAPI.Schema.Misc;

namespace FrameworkAPI.Services.Interfaces;

public interface IMachineService
{
    Task<bool> DoesMachineExist(string machineId, CancellationToken cancellationToken);

    Task<Machine> GetMachine(string machineId, CancellationToken cancellationToken);

    Task<MachineDepartment> GetMachineBusinessUnit(string machineId, CancellationToken cancellationToken);

    Task<MachineFamily> GetMachineFamily(string machineId, CancellationToken cancellationToken);

    Task<List<string>> GetMachineIdsByFilter(
        string? machineIdFilter,
        MachineDepartment? machineDepartmentFilter,
        MachineFamily? machineFamilyFilter,
        CancellationToken cancellationToken);

    Task<IEnumerable<Machine>> GetAllMachines(CancellationToken cancellationToken);
}