using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.Schema.Machine;
using FrameworkAPI.Services.Interfaces;
using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Data;
using HotChocolate.Types;

namespace FrameworkAPI.Queries;

/// <summary>
/// GraphQL query class for machine entity.
/// </summary>
[ExtendObjectType("Query")]
public class MachineQuery
{
    /// <summary>
    /// Query to get data of all machine at one moment (either live or at query timestamp).
    /// </summary>
    /// <param name="machineService">The machine service.</param>
    /// <param name="timestamp">If this is <c>null</c>, the current status of the machine is queried. Otherwise, the historic values on this timestamp are returned.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Queryable <see cref="Machine"/>s.</returns>
    [Authorize(Roles = ["go-general"])]
    [UseSorting]
    [UseFiltering]
    public async Task<IQueryable<Machine>> GetMachines(
        [Service] IMachineService machineService,
        DateTime? timestamp,
        CancellationToken cancellationToken)
    {
        var machines = (await machineService.GetAllMachines(cancellationToken)).ToList();

        foreach (var machine in machines)
        {
            machine.QueryTimestamp = timestamp?.ToUniversalTime();
        }

        return machines.AsQueryable();
    }

    /// <summary>
    /// Query to get data of one machine by id at one moment (either live or at query timestamp).
    /// </summary>
    /// <param name="machineService">The machine service.</param>
    /// <param name="machineId">The machine id.</param>
    /// <param name="timestamp">If this is <c>null</c>, the current status of the machine is queried. Otherwise, the historic values on this timestamp are returned.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Machine"/>.</returns>
    [Authorize(Roles = ["go-general"])]
    public async Task<Machine> GetMachine(
        [Service] IMachineService machineService,
        string machineId,
        DateTime? timestamp,
        CancellationToken cancellationToken)
    {
        var machine = await machineService.GetMachine(machineId, cancellationToken);
        machine.QueryTimestamp = timestamp?.ToUniversalTime();
        return machine;
    }
}