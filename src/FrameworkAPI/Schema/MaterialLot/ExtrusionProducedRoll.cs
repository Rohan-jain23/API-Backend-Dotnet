using System.Collections.Generic;
using FrameworkAPI.Schema.MaterialLot.Extrusion;
using FrameworkAPI.Schema.Misc;
using FrameworkAPI.Schema.ProducedJob;
using HotChocolate;

namespace FrameworkAPI.Schema.MaterialLot;

/// <summary>
/// Produced roll entity of extrusion machines.
/// </summary>
public class ExtrusionProducedRoll(WuH.Ruby.MaterialDataHandler.Client.Models.Lot.Lot materialLot) : MaterialLot(materialLot)
{
    /// <summary>
    /// Actual thickness of the film on this roll.
    /// Can be average 2-sigma value or average value.
    /// [Source: MachineSnapshots]
    /// </summary>
    public Thickness ThicknessActual() => new(StartTime, EndTime, MachineId);

    /// <summary>
    /// The job for which this roll was produced
    /// (if the job is not changed correctly at the machine, there might be more than one job per roll).
    /// [Source: Not yet set; later ProductionPeriods]
    /// </summary>
    [GraphQLIgnore]
    public SnapshotValuesDuringProduction<ExtrusionProducedJob>? Job { get; set; }

    /// <summary>
    /// Set value for the roll length.
    /// [Source: MaterialDataHandler]
    /// </summary>
    [GraphQLIgnore]
    public NumericValue? SetRollLength { get; set; }

    /// <summary>
    /// Actual length of the roll.
    /// [Source: MaterialDataHandler]
    /// </summary>
    [GraphQLIgnore]
    public NumericValue? ActualRollLength { get; set; }

    /// <summary>
    /// On slit rolls (ups), this property is the mother roll where the slit roll (up) was cut from.
    /// Is 'null', if this is a mother roll.
    /// [Source: Not yet set; later MaterialDataHandler]
    /// </summary>
    [GraphQLIgnore]
    public ExtrusionProducedRoll? MotherRoll { get; set; }

    /// <summary>
    /// On mother rolls, this list contains all slit rolls (ups) which have been cut from this mother roll.
    /// Is 'null', if this is a slit roll.
    /// [Source: Not yet set; later MaterialDataHandler]
    /// </summary>
    [GraphQLIgnore]
    public IEnumerable<ExtrusionProducedRoll>? SlitRolls { get; set; }

    /// <summary>
    /// The winder on which this roll was wound up.
    /// [Source: MaterialDataHandler]
    /// </summary>
    [GraphQLIgnore]
    public Winder? Winder { get; set; }

    /// <summary>
    /// Status of parameter limit violations monitored by RUBY Gain.
    /// [Source: MaterialDataHandler]
    /// </summary>
    [GraphQLIgnore]
    public LimitViolationStatus? LimitViolationStatus { get; set; }
}