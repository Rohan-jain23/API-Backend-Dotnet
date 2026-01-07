using System;
using System.Collections.Generic;
using FrameworkAPI.Helpers;
using FrameworkAPI.Schema.Misc;
using WuH.Ruby.MachineSnapShooter.Client;

namespace FrameworkAPI.Schema.ProducedJob.MachineSettings.PaperSack;

/// <summary>
/// Machine settings during a paper sack job.
/// </summary>
public class PaperSackMachineSettings(string machineId, DateTime? endTime, MachineFamily machineFamily, IEnumerable<TimeRange>? timeRanges, DateTime? machineQueryTimestamp)
{
    /// <summary>
    /// Is true, if at least one of the valve units is used to produce a valve sack. Otherwise, it is an open-mouth sack (only available on bottomer jobs).
    /// </summary>
    public SnapshotValuesDuringProduction<bool?>? IsValveSack()
        => machineFamily is MachineFamily.PaperSackBottomer
            ? new SnapshotValuesDuringProduction<bool?>(
                SnapshotColumnIds.PaperSackProductIsValveSack, endTime, machineId, timeRanges, machineQueryTimestamp)
            : null;

    /// <summary>
    /// Number of valve layers (only available on bottomer jobs).
    /// </summary>
    public SnapshotValuesDuringProduction<int?>? ValveLayers()
        => machineFamily is MachineFamily.PaperSackBottomer
            ? new SnapshotValuesDuringProduction<int?>(
                SnapshotColumnIds.PaperSackProductValveLayers,
                endTime,
                machineId,
                timeRanges,
                machineQueryTimestamp,
                val => val is not null
                    ? Convert.ToInt32(val)
                    : null)
            : null;

    /// <summary>
    /// Set value of the sack (and tube) width (only available on bottomer jobs).
    /// </summary>
    public NumericSnapshotValuesDuringProduction? SackWidth()
        => machineFamily is MachineFamily.PaperSackBottomer
            ? new NumericSnapshotValuesDuringProduction(
                SnapshotColumnIds.PaperSackProductSackDataSackWidth, endTime, machineId, timeRanges, machineQueryTimestamp)
            : null;

    /// <summary>
    /// Set value of the sack length (only available on bottomer jobs).
    /// </summary>
    public NumericSnapshotValuesDuringProduction? SackLength()
        => machineFamily is MachineFamily.PaperSackBottomer
            ? new NumericSnapshotValuesDuringProduction(
                SnapshotColumnIds.PaperSackProductSackDataSackLength, endTime, machineId, timeRanges, machineQueryTimestamp)
            : null;

    /// <summary>
    /// Set value of the tube length (only available on tuber jobs).
    /// </summary>
    public NumericSnapshotValuesDuringProduction? TubeLength()
        => machineFamily is MachineFamily.PaperSackTuber
            ? new NumericSnapshotValuesDuringProduction(
                SnapshotColumnIds.PaperSackProductTubeDataTubeLength, endTime, machineId, timeRanges, machineQueryTimestamp)
            : null;

    /// <summary>
    /// Set value of the tube width (only available on tuber jobs).
    /// </summary>
    public NumericSnapshotValuesDuringProduction? TubeWidth()
        => machineFamily is MachineFamily.PaperSackTuber
            ? new NumericSnapshotValuesDuringProduction(
                SnapshotColumnIds.PaperSackProductTubeDataTubeWidth, endTime, machineId, timeRanges, machineQueryTimestamp)
            : null;

    /// <summary>
    /// Set value of the bottom width on stand-up bottom side (only available on bottomer jobs).
    /// </summary>
    public NumericSnapshotValuesDuringProduction? StandUpBottomWidth()
        => machineFamily is MachineFamily.PaperSackBottomer
            ? new NumericSnapshotValuesDuringProduction(
                SnapshotColumnIds.PaperSackProductStandUpBottomBottomWidth, endTime, machineId, timeRanges, machineQueryTimestamp)
            : null;

    /// <summary>
    /// Is true, if a bottom patch is pasted onto valve bottom (only available on bottomer jobs).
    /// </summary>
    public SnapshotValuesDuringProduction<bool?>? HasStandUpBottomPatch()
        => machineFamily is MachineFamily.PaperSackBottomer
            ? new SnapshotValuesDuringProduction<bool?>(
                SnapshotColumnIds.PaperSackProductStandUpBottomHasCoverPatch, endTime, machineId, timeRanges, machineQueryTimestamp)
            : null;

    /// <summary>
    /// Is true, if a inner bottom patch is pasted into valve bottom (only available on bottomer jobs).
    /// </summary>
    public SnapshotValuesDuringProduction<bool?>? HasStandUpBottomInnerPatch()
        => machineFamily is MachineFamily.PaperSackBottomer
            ? new SnapshotValuesDuringProduction<bool?>(
                SnapshotColumnIds.PaperSackProductStandUpBottomHasInnerPatch, endTime, machineId, timeRanges, machineQueryTimestamp)
            : null;

    /// <summary>
    /// Set value of the bottom width on valve bottom side (only available on bottomer jobs).
    /// </summary>
    public NumericSnapshotValuesDuringProduction? ValveBottomWidth()
        => machineFamily is MachineFamily.PaperSackBottomer
            ? new NumericSnapshotValuesDuringProduction(
                SnapshotColumnIds.PaperSackProductValveBottomBottomWidth, endTime, machineId, timeRanges, machineQueryTimestamp)
            : null;

    /// <summary>
    /// Is true, if a bottom patch is pasted onto valve bottom. Is 'null', on open-mouth sacks (only available on bottomer jobs).
    /// </summary>
    public SnapshotValuesDuringProduction<bool?>? HasValveBottomPatch()
        => machineFamily is MachineFamily.PaperSackBottomer
            ? new SnapshotValuesDuringProduction<bool?>(
                SnapshotColumnIds.PaperSackProductValveBottomHasCoverPatch, endTime, machineId, timeRanges, machineQueryTimestamp)
            : null;

    /// <summary>
    /// Is true, if a inner bottom patch is pasted into valve bottom. Is 'null', on open-mouth sacks (only available on bottomer jobs).
    /// </summary>
    public SnapshotValuesDuringProduction<bool?>? HasValveBottomInnerPatch()
        => machineFamily is MachineFamily.PaperSackBottomer
            ? new SnapshotValuesDuringProduction<bool?>(
                SnapshotColumnIds.PaperSackProductValveBottomHasInnerPatch, endTime, machineId, timeRanges, machineQueryTimestamp)
            : null;

    /// <summary>
    /// Set value of the cut type of the paper sack tube, which has a high impact on the whole sack construction.
    /// </summary>
    public SnapshotValuesDuringProduction<PaperSackCutType?>? CutType()
        => machineFamily is MachineFamily.PaperSackTuber
            ? new SnapshotValuesDuringProduction<PaperSackCutType?>(
                SnapshotColumnIds.PaperSackProductIsFlushCut,
                endTime,
                machineId,
                timeRanges,
                machineQueryTimestamp,
                SnapshotValueConverter.ConvertToPaperSackCutType)
            : null;
}