using System;
using System.Collections.Generic;
using FrameworkAPI.Schema.Misc;
using WuH.Ruby.MachineSnapShooter.Client;

namespace FrameworkAPI.Schema.MachineTimeSpan;

/// <summary>
/// Machine time span entity of printing machines.
/// </summary>
public class PrintingMachineTimeSpan(string machineId, MachineDepartment machineDepartment, DateTime from, DateTime to)
    : MachineTimeSpan(machineId, machineDepartment, from, to)
{

    /// <summary>
    /// Meters of produced output in acceptable quality within this time span.
    /// [Source: MachineSnapshots]
    /// </summary>
    public SummedSnapshotValue? GoodLength() => new(SnapshotColumnIds.ProducedQuantityGoodProduction, MachineId, [new TimeRange(From, To)]);

    /// <summary>
    /// Meters of produced output in not-acceptable quality (= scrap/waste/maculature) within this time span during production.
    /// [Source: MachineSnapshots]
    /// </summary>
    public SummedSnapshotValue? ScrapLengthDuringProduction() => new(SnapshotColumnIds.ProducedQuantityScrapDuringProduction, MachineId, [new TimeRange(From, To)]);

    /// <summary>
    /// Meters of produced output in not-acceptable quality (= scrap/waste/maculature) within this time span during setup.
    /// [Source: MachineSnapshots]
    /// </summary>
    public SummedSnapshotValue? ScrapLengthDuringSetup() => new(SnapshotColumnIds.ProducedQuantityScrapDuringSetup, MachineId, [new TimeRange(From, To)]);

    /// <summary>
    /// The target speed during the time span.
    /// The target speed is usually defined by the production planning department.
    /// The origin of the value can be different (priority is 1-3):
    /// 1.) Job-specific value defined in customer system (via Connect 4 Flow)
    /// 2.) Value entered in ProControl (ProcessData)
    /// 3.) Default setting (via Track section in Admin)
    /// [Source: MachineSnapshots]
    /// </summary>
    public NumericSnapshotValuesDuringProduction? TargetSpeed()
        => new(SnapshotColumnIds.PrintingTargetSpeedFromProcessData, To, MachineId, new List<TimeRange> { new(From, To) }, machineQueryTimestamp: null);

    /// <summary>
    /// Machines production speed during the time span.
    /// [Source: MachineSnapshots]
    /// </summary>
    public NumericSnapshotValuesDuringProduction Speed()
        => new(SnapshotColumnIds.PrintingSpeed, To, MachineId, new List<TimeRange> { new(From, To) }, machineQueryTimestamp: null);
}