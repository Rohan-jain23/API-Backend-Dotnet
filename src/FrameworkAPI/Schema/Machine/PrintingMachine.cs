using FrameworkAPI.Schema.Machine.ActualProcessValues;
using FrameworkAPI.Schema.MaterialLot;
using FrameworkAPI.Schema.Misc;
using HotChocolate;
using WuH.Ruby.MachineSnapShooter.Client;
using MachineDataHandler = WuH.Ruby.MachineDataHandler.Client;

namespace FrameworkAPI.Schema.Machine;

/// <summary>
/// Machine entity of printing machines.
/// </summary>
public class PrintingMachine(MachineDataHandler.Machine internalMachine) : Machine(internalMachine)
{

    /// <summary>
    /// Machines production speed.
    /// [Source: MachineSnapshots]
    /// </summary>
    public NumericSnapshotValueAndTrend Speed()
        => new(SnapshotColumnIds.PrintingSpeed, QueryTimestamp, MachineId);

    /// <summary>
    /// Values measured by sensors, quality measurements, actual values of machine settings, values calculated by PLC, ...
    /// [Source: MachineSnapshot]
    /// </summary>
    public PrintingActualProcessValues ActualProcessValues()
        => new(QueryTimestamp, MachineId);

    /// <summary>
    /// Currently produced roll.
    /// [Source: MachineSnapshots]
    /// </summary>
    [GraphQLIgnore]
    public PrintingProducedRoll? ProducedRoll { get; set; }
}