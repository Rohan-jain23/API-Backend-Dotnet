using System;
using FrameworkAPI.Schema.Misc;
using WuH.Ruby.MachineSnapShooter.Client;

namespace FrameworkAPI.Schema.Machine.ActualProcessValues;

/// <summary>
/// All process parameters related to the cooling system.
/// </summary>
public class ExtrusionCooling(DateTime? queryTimestamp, string machineId)
{
    private readonly DateTime? _queryTimestamp = queryTimestamp;
    private readonly string _machineId = machineId;

    /// <summary>
    /// Current value for the air that is blown into the bubble.
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue InnerAirSupplyControl()
        => new(SnapshotColumnIds.ExtrusionCoolingActualValuesInnerAirSupplyControl, _queryTimestamp, _machineId);

    /// <summary>
    /// Current value for the air that is blown out of the bubble
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue InnerAirExhaustControl()
        => new(SnapshotColumnIds.ExtrusionCoolingActualValuesInnerAirExhaustControl, _queryTimestamp, _machineId);

    /// <summary>
    /// Current value for the air that is blown around the bubble.
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue OuterAirControl()
        => new(SnapshotColumnIds.ExtrusionCoolingActualValuesOuterAirControl, _queryTimestamp, _machineId);
}