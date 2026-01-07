using FrameworkAPI.Schema.Misc;
using FrameworkAPI.Schema.ProducedJob;

namespace FrameworkAPI.Schema.MaterialLot;

/// <summary>
/// Produced roll entity of printing machines.
/// </summary>
public class PrintingProducedRoll(WuH.Ruby.MaterialDataHandler.Client.Models.Lot.Lot materialLot) : MaterialLot(materialLot)
{

    /// <summary>
    /// The job for which this roll was produced.
    /// [Source: Not yet set; later ProductionPeriods]
    /// </summary>
    public PrintingProducedJob? Job { get; set; }

    /// <summary>
    /// Set value for the roll length.
    /// [Source: MaterialDataHandler]
    /// </summary>
    public NumericValue? SetRollLength { get; set; }

    /// <summary>
    /// Actual length of the roll.
    /// [Source: MaterialDataHandler]
    /// </summary>
    public NumericValue? ActualRollLength { get; set; }

    /// <summary>
    /// Number of this roll in the current job.
    /// [Source: CheckDataHandler]
    /// </summary>
    public int? RollNumberInJob { get; set; }

    /// <summary>
    /// Detailed data from the print inspection systems about this roll.
    /// [Source: CheckDataHandler]
    /// </summary>
    public PrintInspectionSystemRollData? InspectionSystem { get; set; }

    /// <summary>
    /// Length of acceptable quality in the produced roll.
    /// [Source: CheckDataHandler]
    /// </summary>
    public NumericValue? GoodLength { get; set; }

    /// <summary>
    /// Length of not-acceptable quality (= scrap/waste) in the produced roll.
    /// [Source: CheckDataHandler]
    /// </summary>
    public NumericValue? WasteLength { get; set; }
}