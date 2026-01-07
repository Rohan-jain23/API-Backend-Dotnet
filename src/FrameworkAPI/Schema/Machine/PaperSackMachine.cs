using FrameworkAPI.Schema.Misc;
using WuH.Ruby.MachineSnapShooter.Client;
using MachineDataHandler = WuH.Ruby.MachineDataHandler.Client;

namespace FrameworkAPI.Schema.Machine;

/// <summary>
/// Machine entity of paper sack machines.
/// </summary>
public class PaperSackMachine(MachineDataHandler.Machine internalMachine) : Machine(internalMachine)
{
    /// <summary>
    /// Machines production speed.
    /// [Source: MachineSnapshots]
    /// </summary>
    public NumericSnapshotValueAndTrend Speed()
        => new(SnapshotColumnIds.PaperSackSpeed, QueryTimestamp, MachineId);
}