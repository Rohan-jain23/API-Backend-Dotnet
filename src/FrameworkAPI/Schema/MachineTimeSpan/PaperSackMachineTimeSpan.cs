using System;
using System.Collections.Generic;
using FrameworkAPI.Schema.Misc;
using HotChocolate;
using WuH.Ruby.MachineSnapShooter.Client;

namespace FrameworkAPI.Schema.MachineTimeSpan;

/// <summary>
/// Machine time span entity of paper sack machines.
/// </summary>
public class PaperSackMachineTimeSpan(string machineId, MachineDepartment machineDepartment, DateTime from, DateTime to)
    : MachineTimeSpan(machineId, machineDepartment, from, to)
{

    /// <summary>
    /// Count of produced items in acceptable quality within this time span.
    /// [Source: MachineSnapshots]
    /// </summary>
    public SummedSnapshotValue? GoodQuantity() => new(SnapshotColumnIds.ProducedQuantityGoodProduction, MachineId, [new TimeRange(From, To)]);

    /// <summary>
    /// Count of produced items in not-acceptable quality (= scrap/waste) within this time span during production.
    /// [Source: MachineSnapshots]
    /// </summary>
    public SummedSnapshotValue? ScrapQuantityDuringProduction() => new(SnapshotColumnIds.ProducedQuantityScrapDuringProduction, MachineId, [new TimeRange(From, To)]);

    /// <summary>
    /// Count of produced items in not-acceptable quality (= scrap/waste) within this time span during setup.
    /// [Source: MachineSnapshots]
    /// </summary>
    public SummedSnapshotValue? ScrapQuantityDuringSetup() => new(SnapshotColumnIds.ProducedQuantityScrapDuringSetup, MachineId, [new TimeRange(From, To)]);

    /// <summary>
    /// The target speed during the time span.
    /// The target speed is usually defined by the production planning department.
    /// The origin of the value can be different (priority is 1 to 5):
    /// 1.) Job-specific value defined/corrected in RUBY (via OperatorUI or Track)
    /// 2.) Product(group)-specific setting (via Track)
    /// 3.) Job-specific value defined in customer system (via Connect 4 Flow)
    /// 4.) Value entered in ProControl (ProcessData)
    /// 5.) Default setting (via Track section in Admin)
    /// Attention: For now, this property does not consider step 1-3.
    /// [Source: MachineSnapshots]
    /// </summary>
    [GraphQLIgnore]
    public NumericSnapshotValuesDuringProduction TargetSpeed()
        => new(SnapshotColumnIds.PaperSackTargetSpeedFromProcessData, To, MachineId, new List<TimeRange> { new(From, To) }, machineQueryTimestamp: null);

    /// <summary>
    /// Machines production speed during the time span.
    /// [Source: MachineSnapshots]
    /// </summary>
    public NumericSnapshotValuesDuringProduction Speed()
        => new(SnapshotColumnIds.PaperSackSpeed, To, MachineId, new List<TimeRange> { new(From, To) }, machineQueryTimestamp: null);

    /// <summary>
    /// Speed level during this time span, which is determined for the speed histogram and target speed recommendation.
    /// Other than the normal 'Speed' property, similar speeds are grouped into categories (rounded to tens).
    /// Also, job-specific downtimes are still accounted for in the speed level as they affect productivity.
    /// Certain situations, like roll changes or the speed-up, are excluded to maintain an accurate picture.
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction SpeedLevel()
        => new(SnapshotColumnIds.PaperSackSpeedLevel, To, MachineId, new List<TimeRange> { new(From, To) }, machineQueryTimestamp: null);
}