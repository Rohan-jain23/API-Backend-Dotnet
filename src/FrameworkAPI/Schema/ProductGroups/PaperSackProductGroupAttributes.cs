using System;
using System.Collections.Generic;
using System.Linq;
using WuH.Ruby.KpiDataHandler.Client.Models;
using WuH.Ruby.MachineSnapShooter.Client;

namespace FrameworkAPI.Schema.ProductGroup;

/// <summary>
/// Values of all attributes that define a product group.
/// This also contains legacy attributes of old product group definition versions (these are marked in the description).
/// All jobs of a product group have the same attributes.
/// These attributes were selected by WuH because they can be derived from machine data
/// and have significant impact on the production performance.
/// </summary>
public class PaperSackProductGroupAttributes(List<PaperSackProductGroupAttribute> kpiDataHandlerAttributes)
{
    /// <summary>
    /// Set value for sack width from product data.
    /// Similar values are aggregated in buckets.
    /// The bucket size varies (170-325 => 20 mm; 325-475 => 50 mm; 475-790 => 20 mm).
    /// [Source: Bottomer Snapshot]
    /// </summary>
    public ProductGroupBucketAttributeValue SackWidth { get; set; } = new(
        GetStringValue(kpiDataHandlerAttributes, SnapshotColumnIds.PaperSackProductSackDataSackWidth),
        "mm");

    /// <summary>
    /// Set value for stand-up bottom width.
    /// Similar values are aggregated in buckets (bucket size: 10 mm).
    /// [Source: Bottomer Snapshot]
    /// </summary>
    public ProductGroupBucketAttributeValue BottomWidth { get; set; } = new(
        GetStringValue(kpiDataHandlerAttributes, SnapshotColumnIds.PaperSackProductStandUpBottomBottomWidth),
        "mm");

    /// <summary>
    /// Is true, if the tube has a flush cut (with or without slit-cuts).
    /// Is false, if the tube has stepped-end cut.
    /// [Source: Tuber Snapshot]
    /// </summary>
    public bool? IsFlushCut { get; set; } = GetBoolValue(kpiDataHandlerAttributes, SnapshotColumnIds.PaperSackProductIsFlushCut);

    /// <summary>
    /// Number of paper/film layers in the tube (max. 6), which is derived from the number of active unwinds.
    /// [Source: Tuber Snapshot]
    /// </summary>
    public int? TubeLayers { get; set; } = GetIntValue(kpiDataHandlerAttributes, SnapshotColumnIds.PaperSackProductTubeLayers);

    /// <summary>
    /// Is true, if a second slitting tool (knife) can be used to cut the endless tube into single tubes.
    /// On small tubes (approx. segment length smaller 780 mm) it is possible to use a second knife
    /// and therefore the machine can run faster than on large tubes (where only one knife can be used).
    /// [Source: Tuber Snapshot]
    /// </summary>
    public bool? IsSecondSlittingToolPossible { get; set; } = GetBoolValue(kpiDataHandlerAttributes, SnapshotColumnIds.PaperSackProductIsSecondSlittingToolPossible);

    /// <summary>
    /// Is true, if the material of one of the embedded layers is probably film or thin paper.
    /// It is assumed that this is true, when one of the unwinders (that is not the inner layer) is active
    /// and a low web tension (smaller or equal 50 N) is used.
    /// [Source: Tuber Snapshot]
    /// </summary>
    public bool? HasEmbeddedFilmOrThinPaperLayer { get; set; } = GetBoolValue(kpiDataHandlerAttributes, SnapshotColumnIds.PaperSackProductHasEmbeddedFilmOrThinPaperLayer);

    /// <summary>
    /// Is true, if the material of the inner layer is probably film or thin paper.
    /// It is assumed that this is true, when the highest active unwinder uses a low web tension (smaller or equal 50 N).
    /// [Source: Tuber Snapshot]
    /// </summary>
    public bool? HasFilmTubeAsInnerLayer { get; set; } = GetBoolValue(kpiDataHandlerAttributes, SnapshotColumnIds.PaperSackProductHasFilmTubeAsInnerLayer);

    /// <summary>
    /// Is true, if the paper thickness measurement of the 1st unwinder is valid and the thickness is below 75 mm.
    /// [Source: Tuber Snapshot]
    /// </summary>
    public bool? Is70mmPaperOnOuterLayer { get; set; } = GetBoolValue(kpiDataHandlerAttributes, SnapshotColumnIds.PaperSackProductIs70mmPaperOnOuterLayer);

    /// <summary>
    /// Is true, if at least one of the valve units is used to produce a valve sack.
    /// Otherwise, it is an open-mouth sack
    /// [Source: Bottomer Snapshot]
    /// </summary>
    public bool? IsValveSack { get; set; } = GetBoolValue(kpiDataHandlerAttributes, SnapshotColumnIds.PaperSackProductIsValveSack);

    /// <summary>
    /// Is true, if both valve units are active.
    /// Is 'null', if it is an open-mouth sack.
    /// [Source: Bottomer Snapshot]
    /// </summary>
    public bool? IsSecondValveUnitNeeded { get; set; } = GetBoolValue(kpiDataHandlerAttributes, SnapshotColumnIds.PaperSackProductValveIsSecondValveUnitNeeded);

    /// <summary>
    /// Number of valve layers.
    /// Is 'null', if it is an open-mouth sack.
    /// [Source: Bottomer Snapshot]
    /// </summary>
    public int? ValveLayers { get; set; } = GetIntValue(kpiDataHandlerAttributes, SnapshotColumnIds.PaperSackProductValveLayers);

    /// <summary>
    /// Is true, if the valve is pasted in front of the sack (in transport direction).
    /// This is calculated by checking if one of the valve positions is greater then the sack width.
    /// Is 'null', if it is an open-mouth sack.
    /// [Source: Bottomer Snapshot]
    /// </summary>
    public bool? IsLayUpPositionOnLeadingEdge { get; set; } = GetBoolValue(kpiDataHandlerAttributes, SnapshotColumnIds.PaperSackProductValveIsLayUpPositionOnLeadingEdge);

    /// <summary>
    /// Is true, if it is an offset valve.
    /// Is 'null', if it is an open-mouth sack.
    /// [Source: Bottomer Snapshot]
    /// </summary>
    public bool? IsOffsetValve { get; set; } = GetBoolValue(kpiDataHandlerAttributes, SnapshotColumnIds.PaperSackProductValveIsOffsetValve);

    /// <summary>
    /// Is true, if an active valve unit has 0 as foldover length.
    /// Is 'null', if it is an open-mouth sack.
    /// [Source: Bottomer Snapshot]
    /// </summary>
    public bool? HasValveNoFoldover { get; set; } = GetBoolValue(kpiDataHandlerAttributes, SnapshotColumnIds.PaperSackProductValveHasNoFoldover);

    /// <summary>
    /// Is true, if an active valve unit pastes a patch outside the center of the bottom square (+ 5 mm tolerance).
    /// Is 'null', if it is an open-mouth sack.
    /// [Source: Bottomer Snapshot]
    /// </summary>
    public bool? IsValveOutsideOfBottomSquareCenter { get; set; } = GetBoolValue(kpiDataHandlerAttributes, SnapshotColumnIds.PaperSackProductValveIsOutsideOfBottomSquareCenter);

    /// <summary>
    /// Is true, if an active valve unit uses the slitting unit for PE film.
    /// Is 'null', if it is an open-mouth sack.
    /// [Source: Bottomer Snapshot]
    /// </summary>
    public bool? IsValveFilmSlittingUnitNeeded { get; set; } = GetBoolValue(kpiDataHandlerAttributes, SnapshotColumnIds.PaperSackProductValveIsFilmSlittingUnitNeeded);

    /// <summary>
    /// Is true, if an active valve unit produces with valve type 'Tube' (2).
    /// Is 'null', if it is an open-mouth sack.
    /// [Source: Bottomer Snapshot]
    /// </summary>
    public bool? IsInUnitCreatedTubeValve { get; set; } = GetBoolValue(kpiDataHandlerAttributes, SnapshotColumnIds.PaperSackProductValveIsInUnitCreatedTubeValve);

    /// <summary>
    /// Is true, if a inner bottom patch is pasted into valve bottom.
    /// Is 'null', on open-mouth sacks or if the machine has no inner bottom patch unit.
    /// [Source: Bottomer Snapshot]
    /// </summary>
    public bool? HasValveBottomInnerPatch { get; set; } = GetBoolValue(kpiDataHandlerAttributes, SnapshotColumnIds.PaperSackProductValveBottomHasInnerPatch);

    /// <summary>
    /// Is true, if a inner bottom patch is pasted into stand-up bottom.
    /// Is 'null', on open-mouth sacks or if the machine has no inner bottom patch unit.
    /// [Source: Bottomer Snapshot]
    /// </summary>
    public bool? HasStandUpBottomInnerPatch { get; set; } = GetBoolValue(kpiDataHandlerAttributes, SnapshotColumnIds.PaperSackProductStandUpBottomHasInnerPatch);

    /// <summary>
    /// Is true, if a bottom patch is pasted onto valve bottom (cover patch).
    /// Is 'null', on open-mouth sacks or if the machine has no bottom patch unit.
    /// [Source: Bottomer Snapshot]
    /// </summary>
    public bool? HasValveBottomCoverPatch { get; set; } = GetBoolValue(kpiDataHandlerAttributes, SnapshotColumnIds.PaperSackProductValveBottomHasCoverPatch);

    /// <summary>
    /// Is true, if a bottom patch is pasted onto stand-up bottom (cover patch).
    /// Is 'null', on open-mouth sacks or if the machine has no bottom patch unit.
    /// [Source: Bottomer Snapshot]
    /// </summary>
    public bool? HasStandUpBottomCoverPatch { get; set; } = GetBoolValue(kpiDataHandlerAttributes, SnapshotColumnIds.PaperSackProductStandUpBottomHasCoverPatch);

    /// <summary>
    /// Is true, if a very thick paper (> 150 mm) is used in one of the cover patch units.
    /// Is 'null', when no residual roll length measurement is valid.
    /// [Source: Bottomer Snapshot]
    /// </summary>
    public bool? IsThickCoverPatchPaper { get; set; } = GetBoolValue(kpiDataHandlerAttributes, SnapshotColumnIds.PaperSackProductCoverPatchIsThickPaper);

    private static PaperSackProductGroupAttribute? GetMatchingAttribute(List<PaperSackProductGroupAttribute> kpiDataHandlerAttributes, string columnId)
    {
        return kpiDataHandlerAttributes.FirstOrDefault(x => x.SnapshotColumnId == columnId);
    }

    private static bool? GetBoolValue(List<PaperSackProductGroupAttribute> kpiDataHandlerAttributes, string columnId)
    {
        var matchingAttribute = GetMatchingAttribute(kpiDataHandlerAttributes, columnId);

        // We would like to know if this case occurred (null + error, but like this GraphQL will error out)
        // TODO: Find a way to make this work
        // ?? throw new System.Exception($"Attribute with columnId '{columnId}' is not in attributes from KpiDataHandler.");
        if (matchingAttribute is null || matchingAttribute.Value is null)
        {
            return default;
        }

        return Convert.ToBoolean(matchingAttribute.Value);
    }

    private static int? GetIntValue(List<PaperSackProductGroupAttribute> kpiDataHandlerAttributes, string columnId)
    {
        var matchingAttribute = GetMatchingAttribute(kpiDataHandlerAttributes, columnId);

        if (matchingAttribute is null || matchingAttribute.Value is null)
        {
            return default;
        }

        return Convert.ToInt32(matchingAttribute.Value);
    }

    private static string? GetStringValue(List<PaperSackProductGroupAttribute> kpiDataHandlerAttributes, string columnId)
    {
        var matchingAttribute = GetMatchingAttribute(kpiDataHandlerAttributes, columnId);

        if (matchingAttribute is null || matchingAttribute.Value is null)
        {
            return default;
        }

        return Convert.ToString(matchingAttribute.Value);
    }
}