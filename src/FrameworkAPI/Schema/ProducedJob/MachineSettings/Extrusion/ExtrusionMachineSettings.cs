using System;
using System.Collections.Generic;
using FrameworkAPI.Schema.Misc;
using WuH.Ruby.MachineSnapShooter.Client;

namespace FrameworkAPI.Schema.ProducedJob.MachineSettings.Extrusion;

/// <summary>
/// Machine settings during an extrusion job (all properties should be derived from MachineSnapshots).
/// </summary>
public class ExtrusionMachineSettings(
    string machineId,
    DateTime? endTime,
    IEnumerable<TimeRange>? timeRanges,
    DateTime? machineQueryTimestamp)
{
    /// <summary>
    /// The order of layers - "the output of which extruder goes into what layer". Also called plug code.
    /// </summary>
    public SnapshotValuesDuringProduction<string> OrderOfLayers() => new(SnapshotColumnIds.ExtrusionPlugCode, endTime,
        machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Set value of the product width. Depending on the operating mode this can be the primary, secondary or winding width (set).
    /// </summary>
    public NumericSnapshotValuesDuringProduction Width()
        => new(SnapshotColumnIds.ExtrusionFormatSettingsWidth, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Set value of the product thickness.
    /// </summary>
    public NumericSnapshotValuesDuringProduction Thickness()
        => new(SnapshotColumnIds.ExtrusionFormatSettingsThickness, endTime, machineId, timeRanges,
            machineQueryTimestamp);

    /// <summary>
    /// Set value of the roll length for winding station A.
    /// </summary>
    public NumericSnapshotValuesDuringProduction RollLengthA()
        => new(SnapshotColumnIds.ExtrusionWindingStationASettingsRollLength, endTime, machineId, timeRanges,
            machineQueryTimestamp);

    /// <summary>
    /// Set value of the roll length for winding station B.
    /// </summary>
    public NumericSnapshotValuesDuringProduction RollLengthB()
        => new(SnapshotColumnIds.ExtrusionWindingStationBSettingsRollLength, endTime, machineId, timeRanges,
            machineQueryTimestamp);

    /// <summary>
    /// Set value for the reversing time.
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction ReversingTime()
        => new(SnapshotColumnIds.ExtrusionHaulOffSettingsReversionTime, endTime, machineId, timeRanges,
            machineQueryTimestamp);

    /// <summary>
    /// Settings for extruder A.
    /// </summary>
    public ExtrusionExtruderASettings ExtruderA => new(machineId, endTime, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Settings for extruder B.
    /// </summary>
    public ExtrusionExtruderBSettings ExtruderB => new(machineId, endTime, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Settings for extruder C.
    /// </summary>
    public ExtrusionExtruderCSettings ExtruderC => new(machineId, endTime, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Settings for extruder D.
    /// </summary>
    public ExtrusionExtruderDSettings ExtruderD => new(machineId, endTime, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Settings for extruder E.
    /// </summary>
    public ExtrusionExtruderESettings ExtruderE => new(machineId, endTime, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Settings for extruder F.
    /// </summary>
    public ExtrusionExtruderFSettings ExtruderF => new(machineId, endTime, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Settings for extruder G.
    /// </summary>
    public ExtrusionExtruderGSettings ExtruderG => new(machineId, endTime, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Settings for extruder H.
    /// </summary>
    public ExtrusionExtruderHSettings ExtruderH => new(machineId, endTime, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Settings for extruder I.
    /// </summary>
    public ExtrusionExtruderISettings ExtruderI => new(machineId, endTime, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Settings for extruder J.
    /// </summary>
    public ExtrusionExtruderJSettings ExtruderJ => new(machineId, endTime, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Settings for extruder K.
    /// </summary>
    public ExtrusionExtruderKSettings ExtruderK => new(machineId, endTime, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Settings for MDO.
    /// </summary>
    public ExtrusionMdoSettings Mdo => new(machineId, endTime, timeRanges, machineQueryTimestamp);
}