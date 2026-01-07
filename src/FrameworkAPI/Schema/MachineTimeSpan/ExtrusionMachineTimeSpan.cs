using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.DataLoaders;
using FrameworkAPI.Schema.Misc;
using FrameworkAPI.Services.Interfaces;
using HotChocolate;
using WuH.Ruby.MachineSnapShooter.Client;

namespace FrameworkAPI.Schema.MachineTimeSpan;

/// <summary>
/// Machine time span entity of extrusion machines.
/// </summary>
public class ExtrusionMachineTimeSpan(string machineId, MachineDepartment machineDepartment, DateTime from, DateTime to)
    : MachineTimeSpan(machineId, machineDepartment, from, to)
{

    /// <summary>
    /// Meters of produced output in acceptable quality within this time span.
    /// [Source: MachineSnapshots]
    /// </summary>
    /// <returns>double</returns>
    public SummedSnapshotValue? GoodLength() => new(SnapshotColumnIds.ExtrusionProducedLengthGoodProduction, MachineId, [new TimeRange(From, To)]);

    /// <summary>
    /// Meters of produced output in not-acceptable quality (= scrap/waste) within this time span.
    /// [Source: MachineSnapshots]
    /// </summary>
    public SummedSnapshotValue? ScrapLength() => new(SnapshotColumnIds.ExtrusionProducedLengthScrapDuringProduction, MachineId, [new TimeRange(From, To)]);

    /// <summary>
    /// Kilograms of produced output in acceptable quality within this time span.
    /// Together with the <see cref="ScrapWeightDuringProduction"/> and <see cref="ScrapWeightDuringSetup"/> this is the total raw material consumption within this time span.
    /// [Source: MachineSnapshots]
    /// </summary>
    public SummedSnapshotValue? GoodWeight => new(SnapshotColumnIds.ProducedQuantityGoodProduction, MachineId, [new TimeRange(From, To)]);

    /// <summary>
    /// Kilograms of produced output in not-acceptable quality (= scrap/waste) within this time span in production.
    /// Together with the <see cref="GoodWeight"/> and <see cref="ScrapWeightDuringSetup"/> this is the total raw material consumption within this time span.
    /// [Source: MachineSnapshots]
    /// </summary>
    public SummedSnapshotValue? ScrapWeightDuringProduction => new(SnapshotColumnIds.ProducedQuantityScrapDuringProduction, MachineId, [new TimeRange(From, To)]);

    /// <summary>
    /// Kilograms of produced output in not-acceptable quality (= scrap/waste) within this time span in setup.
    /// Together with the <see cref="GoodWeight"/> and <see cref="ScrapWeightDuringProduction"/> this is the total raw material consumption within this time span.
    /// [Source: MachineSnapshots]
    /// </summary>
    public SummedSnapshotValue? ScrapWeightDuringSetup => new(SnapshotColumnIds.ProducedQuantityScrapDuringSetup, MachineId, [new TimeRange(From, To)]);

    /// <summary>
    /// Consumption of extruded raw material during this time span grouped by material name.
    /// The material names are entered in ProControl (machine HMI) for each component.
    /// The material consumption is measured for each component by machine.
    /// With this data first the material consumption per material name is calculated for each component.
    /// Then the consumption per material name is summed-up from all components and returned.
    /// [Source: MachineSnapShooter]
    /// </summary>
    /// <returns>Dictionary (key: material name; value: consumption)</returns>
    public async Task<Dictionary<string, NumericValue>?> RawMaterialConsumptionByMaterial(
        SnapshotGroupedSumBatchDataLoader dataLoader,
        [Service] IMaterialConsumptionService service,
        CancellationToken cancellationToken)
        => await service.GetRawMaterialConsumptionByMaterial(
            dataLoader,
            MachineId,
            [new TimeRange(From, To)],
            cancellationToken
        );

    /// <summary>
    /// The target throughput rate during the time span.
    /// The origin of the value can be different (priority is 1-3):
    /// 1.) Job-specific value defined in customer system (via Connect 4 Flow)
    /// 2.) Value entered in ProControl (ProcessData)
    /// 3.) Default setting (via Track section in Admin)
    /// [Source: MachineSnapshots]
    /// </summary>
    public NumericSnapshotValuesDuringProduction? TargetThroughputRate()
        => new(SnapshotColumnIds.ExtrusionTargetThroughputFromProcessData, To, MachineId, new List<TimeRange> { new(From, To) }, machineQueryTimestamp: null);

    /// <summary>
    /// Machines throughput rate during the time span.
    /// The throughput rate is the amount of raw material that is going into the extruders in a certain time measured by the gravimetric
    /// (in common parlance this is the 'machine speed').
    /// [Source: MachineSnapshots]
    /// </summary>
    public NumericSnapshotValuesDuringProduction ThroughputRate()
        => new(SnapshotColumnIds.ExtrusionThroughput, To, MachineId, [new TimeRange(From, To)], machineQueryTimestamp: null);

    /// <summary>
    /// Machines line speed during the time span.
    /// The line speed is the speed at the haul-off (blown film) respectively at the chill roll (cast film).
    /// [Source: MachineSnapshots]
    /// </summary>
    public NumericSnapshotValuesDuringProduction LineSpeed()
        => new(SnapshotColumnIds.ExtrusionSpeed, To, MachineId, [new TimeRange(From, To)], machineQueryTimestamp: null);
}