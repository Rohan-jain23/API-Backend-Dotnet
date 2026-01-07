using System;
using FrameworkAPI.Schema.Misc;
using WuH.Ruby.MachineSnapShooter.Client;

namespace FrameworkAPI.Schema.Machine.ActualProcessValues;

/// <summary>
/// All process parameters related to the cooling system.
/// </summary>
public class ExtrusionCage(DateTime? queryTimestamp, string machineId)
{
    private readonly DateTime? _queryTimestamp = queryTimestamp;
    private readonly string _machineId = machineId;

    /// <summary>
    /// Current value for the height that the cage is at
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Height()
        => new(SnapshotColumnIds.ExtrusionCageSettingsHeight, _queryTimestamp, _machineId);

    /// <summary>
    /// Current value for the width of the cage
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Width()
        => new(SnapshotColumnIds.ExtrusionCageSettingsWidth, _queryTimestamp, _machineId);

}