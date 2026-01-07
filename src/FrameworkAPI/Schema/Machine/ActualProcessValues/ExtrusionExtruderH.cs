using System;
using FrameworkAPI.Schema.Misc;
using WuH.Ruby.MachineSnapShooter.Client;

namespace FrameworkAPI.Schema.Machine.ActualProcessValues;

/// <summary>
/// All process parameters related to the cooling system.
/// </summary>
public class ExtrusionExtruderH(DateTime? queryTimestamp, string machineId)
{
    private readonly DateTime? _queryTimestamp = queryTimestamp;
    private readonly string _machineId = machineId;

    /// <summary>
    /// Current speed (RPM) of extruder H.
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Speed()
        => new(SnapshotColumnIds.ExtrusionExtruderHActualValuesSpeed, _queryTimestamp, _machineId);

    /// <summary>
    /// Current throughput (kg/h) of extruder H.
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Throughtput()
        => new(SnapshotColumnIds.ExtrusionExtruderHActualValuesThroughput, _queryTimestamp, _machineId);

    /// <summary>
    /// Current thickness (µm) of the film layer corresponding to extruder H.
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Thickness()
        => new(SnapshotColumnIds.ExtrusionExtruderHActualValuesThickness, _queryTimestamp, _machineId);

    /// <summary>
    /// Current melt temperature (°C) in extruder H.
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Temperature()
        => new(SnapshotColumnIds.ExtrusionExtruderHActualValuesMeltTemperature, _queryTimestamp, _machineId);

    /// <summary>
    /// Current pressure (bar) inside of extruder H.
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Pressure()
        => new(SnapshotColumnIds.ExtrusionExtruderHActualValuesMeltPressure, _queryTimestamp, _machineId);

    /// <summary>
    /// Current feedrate (kg/60U) of extruder H.
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue FeedRate()
        => new(SnapshotColumnIds.ExtrusionExtruderHActualValuesFeedRate, _queryTimestamp, _machineId);

    /// <summary>
    /// Current motor load (%) of extruder H.
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue MotorLoad()
        => new(SnapshotColumnIds.ExtrusionExtruderHActualValuesMotorLoad, _queryTimestamp, _machineId);

    /// <summary>
    /// Current value for percentage of this components material in relation to the total of the extruder
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Component1Percentage()
        => new(SnapshotColumnIds.ExtrusionExtruderHActualValuesComponent1Percentage, _queryTimestamp, _machineId);

    /// <summary>
    /// Current value for percentage of this components material in relation to the total of the extruder
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Component2Percentage()
        => new(SnapshotColumnIds.ExtrusionExtruderHActualValuesComponent2Percentage, _queryTimestamp, _machineId);

    /// <summary>
    /// Current value for percentage of this components material in relation to the total of the extruder
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Component3Percentage()
        => new(SnapshotColumnIds.ExtrusionExtruderHActualValuesComponent3Percentage, _queryTimestamp, _machineId);

    /// <summary>
    /// Current value for percentage of this components material in relation to the total of the extruder
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Component4Percentage()
        => new(SnapshotColumnIds.ExtrusionExtruderHActualValuesComponent4Percentage, _queryTimestamp, _machineId);

    /// <summary>
    /// Current value for percentage of this components material in relation to the total of the extruder
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Component5Percentage()
        => new(SnapshotColumnIds.ExtrusionExtruderHActualValuesComponent5Percentage, _queryTimestamp, _machineId);

    /// <summary>
    /// Current value for percentage of this components material in relation to the total of the extruder
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Component6Percentage()
        => new(SnapshotColumnIds.ExtrusionExtruderHActualValuesComponent6Percentage, _queryTimestamp, _machineId);

    /// <summary>
    /// Current value for percentage of this components material in relation to the total of the extruder
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Component7Percentage()
        => new(SnapshotColumnIds.ExtrusionExtruderHActualValuesComponent7Percentage, _queryTimestamp, _machineId);
}