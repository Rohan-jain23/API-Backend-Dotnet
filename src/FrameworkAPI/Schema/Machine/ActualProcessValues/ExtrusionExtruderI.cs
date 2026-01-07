using System;
using FrameworkAPI.Schema.Misc;
using WuH.Ruby.MachineSnapShooter.Client;

namespace FrameworkAPI.Schema.Machine.ActualProcessValues;

/// <summary>
/// All process parameters related to the cooling system.
/// </summary>
public class ExtrusionExtruderI(DateTime? queryTimestamp, string machineId)
{
    private readonly DateTime? _queryTimestamp = queryTimestamp;
    private readonly string _machineId = machineId;

    /// <summary>
    /// Current speed (RPM) of extruder I.
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Speed()
        => new(SnapshotColumnIds.ExtrusionExtruderIActualValuesSpeed, _queryTimestamp, _machineId);

    /// <summary>
    /// Current throughput (kg/h) of extruder I.
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Throughtput()
        => new(SnapshotColumnIds.ExtrusionExtruderIActualValuesThroughput, _queryTimestamp, _machineId);

    /// <summary>
    /// Current thickness (µm) of the film layer corresponding to extruder I.
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Thickness()
        => new(SnapshotColumnIds.ExtrusionExtruderIActualValuesThickness, _queryTimestamp, _machineId);

    /// <summary>
    /// Current melt temperature (°C) in extruder I.
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Temperature()
        => new(SnapshotColumnIds.ExtrusionExtruderIActualValuesMeltTemperature, _queryTimestamp, _machineId);

    /// <summary>
    /// Current pressure (bar) inside of extruder I.
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Pressure()
        => new(SnapshotColumnIds.ExtrusionExtruderIActualValuesMeltPressure, _queryTimestamp, _machineId);

    /// <summary>
    /// Current feedrate (kg/60U) of extruder I.
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue FeedRate()
        => new(SnapshotColumnIds.ExtrusionExtruderIActualValuesFeedRate, _queryTimestamp, _machineId);

    /// <summary>
    /// Current motor load (%) of extruder I.
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue MotorLoad()
        => new(SnapshotColumnIds.ExtrusionExtruderIActualValuesMotorLoad, _queryTimestamp, _machineId);

    /// <summary>
    /// Current value for percentage of this components material in relation to the total of the extruder
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Component1Percentage()
        => new(SnapshotColumnIds.ExtrusionExtruderIActualValuesComponent1Percentage, _queryTimestamp, _machineId);

    /// <summary>
    /// Current value for percentage of this components material in relation to the total of the extruder
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Component2Percentage()
        => new(SnapshotColumnIds.ExtrusionExtruderIActualValuesComponent2Percentage, _queryTimestamp, _machineId);

    /// <summary>
    /// Current value for percentage of this components material in relation to the total of the extruder
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Component3Percentage()
        => new(SnapshotColumnIds.ExtrusionExtruderIActualValuesComponent3Percentage, _queryTimestamp, _machineId);

    /// <summary>
    /// Current value for percentage of this components material in relation to the total of the extruder
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Component4Percentage()
        => new(SnapshotColumnIds.ExtrusionExtruderIActualValuesComponent4Percentage, _queryTimestamp, _machineId);

    /// <summary>
    /// Current value for percentage of this components material in relation to the total of the extruder
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Component5Percentage()
        => new(SnapshotColumnIds.ExtrusionExtruderIActualValuesComponent5Percentage, _queryTimestamp, _machineId);

    /// <summary>
    /// Current value for percentage of this components material in relation to the total of the extruder
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Component6Percentage()
        => new(SnapshotColumnIds.ExtrusionExtruderIActualValuesComponent6Percentage, _queryTimestamp, _machineId);

    /// <summary>
    /// Current value for percentage of this components material in relation to the total of the extruder
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Component7Percentage()
        => new(SnapshotColumnIds.ExtrusionExtruderIActualValuesComponent7Percentage, _queryTimestamp, _machineId);
}