using System;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.Schema.MachineTimeSpan;
using FrameworkAPI.Services.Interfaces;
using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Types;

namespace FrameworkAPI.Queries;

/// <summary>
/// GraphQL query class for machine time span entity.
/// </summary>
[ExtendObjectType("Query")]
public class MachineTimeSpanQuery
{
    /// <summary>
    /// Query to get data of one machine by id during a time span.
    /// For performance reasons, the queried time span must be shorter than 7 days.
    /// </summary>
    /// <param name="machineService">The machine service.</param>
    /// <param name="machineId">The machine id.</param>
    /// <param name="from">The start timestamp of the query.</param>
    /// <param name="to">The end timestamp of the query.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="MachineTimeSpan"/>.</returns>
    [Authorize(Roles = ["go-general"])]
    public async Task<MachineTimeSpan> GetMachineTimeSpan(
        [Service] IMachineService machineService,
        string machineId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        if (!await machineService.DoesMachineExist(machineId, cancellationToken))
        {
            throw new ArgumentException($"Machine '{machineId}' does not exist.", nameof(machineId));
        }

        if (from >= to)
        {
            throw new ArgumentException($"'from' ({from}) is not smaller than 'to' ({to}).");
        }

        if ((to - from).TotalDays > 7)
        {
            throw new ArgumentException("The queried time span is longer that 7 days.");
        }

        var machineBusinessUnit = await machineService.GetMachineBusinessUnit(machineId, cancellationToken);
        return MachineTimeSpan.CreateInstance(machineId, machineBusinessUnit, from.ToUniversalTime(), to.ToUniversalTime());
    }
}