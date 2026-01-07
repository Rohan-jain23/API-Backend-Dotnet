using System;
using FrameworkAPI.Schema.Misc;
using WuH.Ruby.MachineSnapShooter.Client;

namespace FrameworkAPI.Schema.Machine.ActualProcessValues;

/// <summary>
/// All process parameters related to the cooling system.
/// </summary>
public class ExtrusionReversingUnit(DateTime? queryTimestamp, string machineId)
{
    private readonly DateTime? _queryTimestamp = queryTimestamp;
    private readonly string _machineId = machineId;

    /// <summary>
    /// Current position for the reversing unit.
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Position()
        => new(SnapshotColumnIds.ExtrusionHaulOffActualValuesReversionAngle, _queryTimestamp, _machineId);

    /// <summary>
    /// Status of the reversing unit (on/off).
    /// [Source: MachineSnapshot]
    /// </summary>
    public SnapshotValue<bool> Status()
        => new(SnapshotColumnIds.ExtrusionHaulOffSettingsIsReversionActive, _queryTimestamp, _machineId);
}