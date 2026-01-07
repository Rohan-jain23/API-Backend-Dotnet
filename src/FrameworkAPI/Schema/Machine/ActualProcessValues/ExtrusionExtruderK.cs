using System;
using FrameworkAPI.Schema.Misc;
using WuH.Ruby.MachineSnapShooter.Client;

namespace FrameworkAPI.Schema.Machine.ActualProcessValues;

/// <summary>
/// All process parameters related to the cooling system.
/// </summary>
public class ExtrusionExtruderK(DateTime? queryTimestamp, string machineId)
{
    private readonly DateTime? _queryTimestamp = queryTimestamp;
    private readonly string _machineId = machineId;

    /// <summary>
    /// Current speed (RPM) of extruder K.
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Speed()
        => new(SnapshotColumnIds.ExtrusionExtruderKActualValuesSpeed, _queryTimestamp, _machineId);

    /// <summary>
    /// Current throughput (kg/h) of extruder K.
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Throughtput()
        => new(SnapshotColumnIds.ExtrusionExtruderKActualValuesThroughput, _queryTimestamp, _machineId);

    /// <summary>
    /// Current thickness (µm) of the film layer corresponding to extruder K.
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Thickness()
        => new(SnapshotColumnIds.ExtrusionExtruderKActualValuesThickness, _queryTimestamp, _machineId);

    /// <summary>
    /// Current melt temperature (°C) in extruder K.
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Temperature()
        => new(SnapshotColumnIds.ExtrusionExtruderKActualValuesMeltTemperature, _queryTimestamp, _machineId);

    /// <summary>
    /// Current pressure (bar) inside of extruder K.
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Pressure()
        => new(SnapshotColumnIds.ExtrusionExtruderKActualValuesMeltPressure, _queryTimestamp, _machineId);

    /// <summary>
    /// Current feedrate (kg/60U) of extruder K.
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue FeedRate()
        => new(SnapshotColumnIds.ExtrusionExtruderKActualValuesFeedRate, _queryTimestamp, _machineId);

    /// <summary>
    /// Current motor load (%) of extruder K.
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue MotorLoad()
        => new(SnapshotColumnIds.ExtrusionExtruderKActualValuesMotorLoad, _queryTimestamp, _machineId);

    /// <summary>
    /// Current value for percentage of this components material in relation to the total of the extruder
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Component1Percentage()
        => new(SnapshotColumnIds.ExtrusionExtruderKActualValuesComponent1Percentage, _queryTimestamp, _machineId);

    /// <summary>
    /// Current value for percentage of this components material in relation to the total of the extruder
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Component2Percentage()
        => new(SnapshotColumnIds.ExtrusionExtruderKActualValuesComponent2Percentage, _queryTimestamp, _machineId);

    /// <summary>
    /// Current value for percentage of this components material in relation to the total of the extruder
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Component3Percentage()
        => new(SnapshotColumnIds.ExtrusionExtruderKActualValuesComponent3Percentage, _queryTimestamp, _machineId);

    /// <summary>
    /// Current value for percentage of this components material in relation to the total of the extruder
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Component4Percentage()
        => new(SnapshotColumnIds.ExtrusionExtruderKActualValuesComponent4Percentage, _queryTimestamp, _machineId);

    /// <summary>
    /// Current value for percentage of this components material in relation to the total of the extruder
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Component5Percentage()
        => new(SnapshotColumnIds.ExtrusionExtruderKActualValuesComponent5Percentage, _queryTimestamp, _machineId);

    /// <summary>
    /// Current value for percentage of this components material in relation to the total of the extruder
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Component6Percentage()
        => new(SnapshotColumnIds.ExtrusionExtruderKActualValuesComponent6Percentage, _queryTimestamp, _machineId);

    /// <summary>
    /// Current value for percentage of this components material in relation to the total of the extruder
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Component7Percentage()
        => new(SnapshotColumnIds.ExtrusionExtruderKActualValuesComponent7Percentage, _queryTimestamp, _machineId);
}