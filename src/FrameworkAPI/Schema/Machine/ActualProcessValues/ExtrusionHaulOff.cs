using System;
using FrameworkAPI.Schema.Misc;
using WuH.Ruby.MachineSnapShooter.Client;

namespace FrameworkAPI.Schema.Machine.ActualProcessValues;

/// <summary>
/// All process parameters related to the cooling system.
/// </summary>
public class ExtrusionHaulOff(DateTime? queryTimestamp, string machineId)
{
    private readonly DateTime? _queryTimestamp = queryTimestamp;
    private readonly string _machineId = machineId;

    /// <summary>
    /// Current value for the collapser.
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Collapser()
        => new(SnapshotColumnIds.ExtrusionCageSettingsHeight, _queryTimestamp, _machineId);

    /// <summary>
    /// Current value for the center guide.
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue CenterGuide()
        => new(SnapshotColumnIds.ExtrusionCageSettingsCenterGuide, _queryTimestamp, _machineId);

}