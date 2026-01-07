using System;
using System.Collections.Generic;
using FrameworkAPI.Schema.Misc;
using WuH.Ruby.MachineSnapShooter.Client;

namespace FrameworkAPI.Schema.ProducedJob.MachineSettings.Extrusion;

/// <summary>
/// Extruder settings during an extrusion job (all properties should be derived from MachineSnapshots).
/// </summary>
public class ExtrusionExtruderCSettings(
    string machineId,
    DateTime? endTime,
    IEnumerable<TimeRange>? timeRanges,
    DateTime? machineQueryTimestamp)
{
    /// <summary>
    /// Set value for the material name of component 1
    /// </summary>
    public SnapshotValuesDuringProduction<string> Component1MaterialName()
        => new(
            SnapshotColumnIds.ExtrusionExtruderCSettingsComponent1MaterialName,
            endTime,
            machineId,
            timeRanges,
            machineQueryTimestamp);

    /// <summary>
    /// Set value for the percentage of component 1
    /// </summary>
    public NumericSnapshotValuesDuringProduction Component1Percentage()
        => new(
            SnapshotColumnIds.ExtrusionExtruderCSettingsComponent1Percentage,
            endTime,
            machineId,
            timeRanges,
            machineQueryTimestamp);

    /// <summary>
    /// Set value for the material name of component 2
    /// </summary>
    public SnapshotValuesDuringProduction<string> Component2MaterialName()
        => new(
            SnapshotColumnIds.ExtrusionExtruderCSettingsComponent2MaterialName,
            endTime,
            machineId,
            timeRanges,
            machineQueryTimestamp);

    /// <summary>
    /// Set value for the percentage of component 2
    /// </summary>
    public NumericSnapshotValuesDuringProduction Component2Percentage()
        => new(
            SnapshotColumnIds.ExtrusionExtruderCSettingsComponent2Percentage,
            endTime,
            machineId,
            timeRanges,
            machineQueryTimestamp);

    /// <summary>
    /// Set value for the material name of component 3
    /// </summary>
    public SnapshotValuesDuringProduction<string> Component3MaterialName()
        => new(
            SnapshotColumnIds.ExtrusionExtruderCSettingsComponent3MaterialName,
            endTime,
            machineId,
            timeRanges,
            machineQueryTimestamp);

    /// <summary>
    /// Set value for the percentage of component 3
    /// </summary>
    public NumericSnapshotValuesDuringProduction Component3Percentage()
        => new(
            SnapshotColumnIds.ExtrusionExtruderCSettingsComponent3Percentage,
            endTime,
            machineId,
            timeRanges,
            machineQueryTimestamp);

    /// <summary>
    /// Set value for the material name of component 4
    /// </summary>
    public SnapshotValuesDuringProduction<string> Component4MaterialName()
        => new(
            SnapshotColumnIds.ExtrusionExtruderCSettingsComponent4MaterialName,
            endTime,
            machineId,
            timeRanges,
            machineQueryTimestamp);

    /// <summary>
    /// Set value for the percentage of component 4
    /// </summary>
    public NumericSnapshotValuesDuringProduction Component4Percentage()
        => new(
            SnapshotColumnIds.ExtrusionExtruderCSettingsComponent4Percentage,
            endTime,
            machineId,
            timeRanges,
            machineQueryTimestamp);

    /// <summary>
    /// Set value for the material name of component 5
    /// </summary>
    public SnapshotValuesDuringProduction<string> Component5MaterialName()
        => new(
            SnapshotColumnIds.ExtrusionExtruderCSettingsComponent5MaterialName,
            endTime,
            machineId,
            timeRanges,
            machineQueryTimestamp);

    /// <summary>
    /// Set value for the percentage of component 5
    /// </summary>
    public NumericSnapshotValuesDuringProduction Component5Percentage()
        => new(
            SnapshotColumnIds.ExtrusionExtruderCSettingsComponent5Percentage,
            endTime,
            machineId,
            timeRanges,
            machineQueryTimestamp);

    /// <summary>
    /// Set value for the material name of component 6
    /// </summary>
    public SnapshotValuesDuringProduction<string> Component6MaterialName()
        => new(
            SnapshotColumnIds.ExtrusionExtruderCSettingsComponent6MaterialName,
            endTime,
            machineId,
            timeRanges,
            machineQueryTimestamp);

    /// <summary>
    /// Set value for the percentage of component 6
    /// </summary>
    public NumericSnapshotValuesDuringProduction Component6Percentage()
        => new(
            SnapshotColumnIds.ExtrusionExtruderCSettingsComponent6Percentage,
            endTime,
            machineId,
            timeRanges,
            machineQueryTimestamp);

    /// <summary>
    /// Set value for the material name of component 7
    /// </summary>
    public SnapshotValuesDuringProduction<string> Component7MaterialName()
        => new(
            SnapshotColumnIds.ExtrusionExtruderCSettingsComponent7MaterialName,
            endTime,
            machineId,
            timeRanges,
            machineQueryTimestamp);

    /// <summary>
    /// Set value for the percentage of component 7
    /// </summary>
    public NumericSnapshotValuesDuringProduction Component7Percentage()
        => new(
            SnapshotColumnIds.ExtrusionExtruderCSettingsComponent7Percentage,
            endTime,
            machineId,
            timeRanges,
            machineQueryTimestamp);
}