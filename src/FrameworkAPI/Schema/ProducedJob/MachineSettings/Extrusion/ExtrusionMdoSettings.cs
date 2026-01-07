using System;
using System.Collections.Generic;
using FrameworkAPI.Schema.Misc;
using WuH.Ruby.MachineSnapShooter.Client;

namespace FrameworkAPI.Schema.ProducedJob.MachineSettings.Extrusion;

/// <summary>
/// Machine settings during an extrusion job (all properties should be derived from MachineSnapshots).
/// </summary>
public class ExtrusionMdoSettings(
    string machineId,
    DateTime? endTime,
    IEnumerable<TimeRange>? timeRanges,
    DateTime? machineQueryTimestamp)
{
    /// <summary>
    /// Set value for the secondary thickness, which is the thickness after the film exits the MDO and has been stretched.
    /// </summary>
    public SnapshotValuesDuringProduction<string> ThicknessSecondary()
        => new(SnapshotColumnIds.ExtrusionMDOSettingsThicknessSecondary, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Set value of the secondary width, which is the width after the film exits the MDO and has been stretched.
    /// </summary>
    public NumericSnapshotValuesDuringProduction WidthSecondary()
        => new(SnapshotColumnIds.ExtrusionMDOSettingsWidthSecondary, endTime, machineId, timeRanges, machineQueryTimestamp);

}