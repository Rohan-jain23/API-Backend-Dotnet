using System;
using FrameworkAPI.Schema.Misc;
using WuH.Ruby.MachineSnapShooter.Client;

namespace FrameworkAPI.Schema.MaterialLot.Extrusion;

public class Thickness(DateTime startTime, DateTime? endTime, string machineId)
{
    /// <summary>
    /// Value for the average 2-sigma value of the film on this roll.
    /// [Source: MachineSnapshots]
    /// </summary>
    // ToDo: currently incorrect logic in Snapshooter 
    public AverageSnapshotValue TwoSigma()
        => new(SnapshotColumnIds.ExtrusionQualityActualValuesTwoSigma, machineId, startTime, endTime);

    /// <summary>
    /// Value for the average actual thickness for the film on this roll.
    /// [Source: MachineSnapshots]
    /// </summary>
    public AverageSnapshotValue Average()
        => new(SnapshotColumnIds.ExtrusionFormatActualValuesThickness, machineId, startTime, endTime);
}