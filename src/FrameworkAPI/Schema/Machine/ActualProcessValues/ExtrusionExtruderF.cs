using System;
using FrameworkAPI.Schema.Misc;
using WuH.Ruby.MachineSnapShooter.Client;

namespace FrameworkAPI.Schema.Machine.ActualProcessValues;

/// <summary>
/// All process parameters related to the cooling system.
/// </summary>
public class ExtrusionExtruderF(DateTime? queryTimestamp, string machineId)
{
    private readonly DateTime? _queryTimestamp = queryTimestamp;
    private readonly string _machineId = machineId;

    /// <summary>
    /// Current speed (RPM) of extruder F.
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Speed()
        => new(SnapshotColumnIds.ExtrusionExtruderFActualValuesSpeed, _queryTimestamp, _machineId);

    /// <summary>
    /// Current throughput (kg/h) of extruder F.
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Throughtput()
        => new(SnapshotColumnIds.ExtrusionExtruderFActualValuesThroughput, _queryTimestamp, _machineId);

    /// <summary>
    /// Current thickness (µm) of the film layer corresponding to extruder F.
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Thickness()
        => new(SnapshotColumnIds.ExtrusionExtruderFActualValuesThickness, _queryTimestamp, _machineId);

    /// <summary>
    /// Current melt temperature (°C) in extruder F.
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Temperature()
        => new(SnapshotColumnIds.ExtrusionExtruderFActualValuesMeltTemperature, _queryTimestamp, _machineId);

    /// <summary>
    /// Current pressure (bar) inside of extruder F.
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Pressure()
        => new(SnapshotColumnIds.ExtrusionExtruderFActualValuesMeltPressure, _queryTimestamp, _machineId);

    /// <summary>
    /// Current feedrate (kg/60U) of extruder F.
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue FeedRate()
        => new(SnapshotColumnIds.ExtrusionExtruderFActualValuesFeedRate, _queryTimestamp, _machineId);

    /// <summary>
    /// Current motor load (%) of extruder F.
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue MotorLoad()
        => new(SnapshotColumnIds.ExtrusionExtruderFActualValuesMotorLoad, _queryTimestamp, _machineId);

    /// <summary>
    /// Current value for percentage of this components material in relation to the total of the extruder
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Component1Percentage()
        => new(SnapshotColumnIds.ExtrusionExtruderFActualValuesComponent1Percentage, _queryTimestamp, _machineId);

    /// <summary>
    /// Current value for percentage of this components material in relation to the total of the extruder
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Component2Percentage()
        => new(SnapshotColumnIds.ExtrusionExtruderFActualValuesComponent2Percentage, _queryTimestamp, _machineId);

    /// <summary>
    /// Current value for percentage of this components material in relation to the total of the extruder
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Component3Percentage()
        => new(SnapshotColumnIds.ExtrusionExtruderFActualValuesComponent3Percentage, _queryTimestamp, _machineId);

    /// <summary>
    /// Current value for percentage of this components material in relation to the total of the extruder
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Component4Percentage()
        => new(SnapshotColumnIds.ExtrusionExtruderFActualValuesComponent4Percentage, _queryTimestamp, _machineId);

    /// <summary>
    /// Current value for percentage of this components material in relation to the total of the extruder
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Component5Percentage()
        => new(SnapshotColumnIds.ExtrusionExtruderFActualValuesComponent5Percentage, _queryTimestamp, _machineId);

    /// <summary>
    /// Current value for percentage of this components material in relation to the total of the extruder
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Component6Percentage()
        => new(SnapshotColumnIds.ExtrusionExtruderFActualValuesComponent6Percentage, _queryTimestamp, _machineId);

    /// <summary>
    /// Current value for percentage of this components material in relation to the total of the extruder
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Component7Percentage()
        => new(SnapshotColumnIds.ExtrusionExtruderFActualValuesComponent7Percentage, _queryTimestamp, _machineId);
}