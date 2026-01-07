using System;
using FrameworkAPI.Schema.Misc;
using WuH.Ruby.MachineSnapShooter.Client;

namespace FrameworkAPI.Schema.Machine.ActualProcessValues;

/// <summary>
/// All process parameters related to the cooling system.
/// </summary>
public class ExtrusionExtruderE(DateTime? queryTimestamp, string machineId)
{
    private readonly DateTime? _queryTimestamp = queryTimestamp;
    private readonly string _machineId = machineId;

    /// <summary>
    /// Current speed (RPM) of extruder E.
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Speed()
        => new(SnapshotColumnIds.ExtrusionExtruderEActualValuesSpeed, _queryTimestamp, _machineId);

    /// <summary>
    /// Current throughput (kg/h) of extruder E.
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Throughtput()
        => new(SnapshotColumnIds.ExtrusionExtruderEActualValuesThroughput, _queryTimestamp, _machineId);

    /// <summary>
    /// Current thickness (µm) of the film layer corresponding to extruder E.
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Thickness()
        => new(SnapshotColumnIds.ExtrusionExtruderEActualValuesThickness, _queryTimestamp, _machineId);

    /// <summary>
    /// Current melt temperature (°C) in extruder E.
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Temperature()
        => new(SnapshotColumnIds.ExtrusionExtruderEActualValuesMeltTemperature, _queryTimestamp, _machineId);

    /// <summary>
    /// Current pressure (bar) inside of extruder E.
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Pressure()
        => new(SnapshotColumnIds.ExtrusionExtruderEActualValuesMeltPressure, _queryTimestamp, _machineId);

    /// <summary>
    /// Current feedrate (kg/60U) of extruder E.
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue FeedRate()
        => new(SnapshotColumnIds.ExtrusionExtruderEActualValuesFeedRate, _queryTimestamp, _machineId);

    /// <summary>
    /// Current motor load (%) of extruder E.
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue MotorLoad()
        => new(SnapshotColumnIds.ExtrusionExtruderEActualValuesMotorLoad, _queryTimestamp, _machineId);

    /// <summary>
    /// Current value for percentage of this components material in relation to the total of the extruder
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Component1Percentage()
        => new(SnapshotColumnIds.ExtrusionExtruderEActualValuesComponent1Percentage, _queryTimestamp, _machineId);

    /// <summary>
    /// Current value for percentage of this components material in relation to the total of the extruder
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Component2Percentage()
        => new(SnapshotColumnIds.ExtrusionExtruderEActualValuesComponent2Percentage, _queryTimestamp, _machineId);

    /// <summary>
    /// Current value for percentage of this components material in relation to the total of the extruder
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Component3Percentage()
        => new(SnapshotColumnIds.ExtrusionExtruderEActualValuesComponent3Percentage, _queryTimestamp, _machineId);

    /// <summary>
    /// Current value for percentage of this components material in relation to the total of the extruder
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Component4Percentage()
        => new(SnapshotColumnIds.ExtrusionExtruderEActualValuesComponent4Percentage, _queryTimestamp, _machineId);

    /// <summary>
    /// Current value for percentage of this components material in relation to the total of the extruder
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Component5Percentage()
        => new(SnapshotColumnIds.ExtrusionExtruderEActualValuesComponent5Percentage, _queryTimestamp, _machineId);

    /// <summary>
    /// Current value for percentage of this components material in relation to the total of the extruder
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Component6Percentage()
        => new(SnapshotColumnIds.ExtrusionExtruderEActualValuesComponent6Percentage, _queryTimestamp, _machineId);

    /// <summary>
    /// Current value for percentage of this components material in relation to the total of the extruder
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Component7Percentage()
        => new(SnapshotColumnIds.ExtrusionExtruderEActualValuesComponent7Percentage, _queryTimestamp, _machineId);
}