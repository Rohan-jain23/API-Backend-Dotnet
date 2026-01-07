using FrameworkAPI.Schema.Machine.ActualProcessValues;
using FrameworkAPI.Schema.MaterialLot;
using FrameworkAPI.Schema.Misc;
using FrameworkAPI.Schema.ProductDefinition;
using HotChocolate;
using WuH.Ruby.MachineSnapShooter.Client;
using MachineDataHandler = WuH.Ruby.MachineDataHandler.Client;

namespace FrameworkAPI.Schema.Machine;

/// <summary>
/// Machine entity of extrusion machines.
/// </summary>
public class ExtrusionMachine(MachineDataHandler.Machine internalMachine) : Machine(internalMachine)
{

    /// <summary>
    /// The amount of raw material that is going into the extruders in a certain time measured by the gravimetric
    /// (in common parlance this is the 'machine speed').
    /// [Source: MachineSnapshots]
    /// </summary>
    public NumericSnapshotValueAndTrend ThroughputRate()
        => new(SnapshotColumnIds.ExtrusionThroughput, QueryTimestamp, MachineId);

    /// <summary>
    /// The line speed is the machines speed at the haul-off (blown film) respectively at the chill roll (cast film).
    /// [Source: MachineSnapshots]
    /// </summary>
    public NumericSnapshotValueAndTrend LineSpeed()
        => new(SnapshotColumnIds.ExtrusionSpeed, QueryTimestamp, MachineId);

    /// <summary>
    /// The number of layers of the currently produced film.
    /// [Source: MachineSnapshots]
    /// </summary>
    public NumericSnapshotValueAndTrend NumberOfLayers()
        => new(SnapshotColumnIds.ExtrusionNumberOfLayers, QueryTimestamp, MachineId);

    /// <summary>
    /// Indicates whether MDO is used, side gussets are used or nothing special, which has an impact on the interpretation of widths, thicknesses and profiles.
    /// [Source: MachineSnapshots]
    /// </summary>
    public SnapshotValue<string> OperatingMode()
        => new(SnapshotColumnIds.ExtrusionOperatingMode, QueryTimestamp, MachineId);

    /// <summary>
    /// Indicates which winding station is being used. 'A' or 'B' means that tube film is produced.
    /// [Source: MachineSnapshots]
    /// </summary>
    public SnapshotValue<string> ActiveWindingStation()
        => new(SnapshotColumnIds.ExtrusionActiveWindingStation, QueryTimestamp, MachineId);

    /// <summary>
    /// The current profile control mode
    /// [Source: MachineSnapshots]
    /// </summary>
    public NumericSnapshotValueAndTrend ProfileControlMode()
        => new(SnapshotColumnIds.ExtrusionQualitySettingsProfileControlMode, QueryTimestamp, MachineId);

    /// <summary>
    /// Indicates which profile control mode should be used given the current operating mode.
    /// [Source: MachineSnapshots]
    /// </summary>
    public NumericSnapshotValueAndTrend TargetProfileControlMode()
        => new(SnapshotColumnIds.ExtrusionQualityTargetProfileControlMode, QueryTimestamp, MachineId);

    /// <summary>
    /// Currently produced roll on winder A.
    /// [Source: MaterialDataHandler]
    /// </summary>
    [GraphQLIgnore]
    public ExtrusionProducedRoll? ProducedRollWinderA { get; set; }

    /// <summary>
    /// Currently produced roll on winder B.
    /// [Source: MaterialDataHandler]
    /// </summary>
    [GraphQLIgnore]
    public ExtrusionProducedRoll? ProducedRollWinderB { get; set; }

    /// <summary>
    /// Values measured by sensors, quality measurements, actual values of machine settings, values calculated by PLC, ...
    /// [Source: MachineSnapshot]
    /// </summary>
    public ExtrusionActualProcessValues ActualProcessValues()
        => new(QueryTimestamp, MachineId, MachineFamily);

    /// <summary>
    /// Definition of the currently produced extrusion product.
    /// [Source: ProductRecognizer]
    /// </summary>
    [GraphQLIgnore]
    public ExtrusionProductDefinition? ProductDefinition { get; set; }

    /// <summary>
    /// Current count of violated limits that are inspected by RUBY Gain.
    /// [Source: LimitDataHandler]
    /// </summary>
    [GraphQLIgnore]
    public int? LimitViolationCount { get; set; }
}