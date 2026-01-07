using System;

namespace FrameworkAPI.Schema.Misc;

/// <summary>
/// Production break entry of the RUBY Track production history.
/// These production breaks are times that are counted as scheduled non-production (lunch break, scheduled cleaning shift, weekend, holiday, ...)
/// and also this is shown in job history if the job got interrupted by another job.
/// These production history entries are like the ones displayed in RUBY Track Operator UI.
/// </summary>
public class TrackProductionBreakHistoryEntry : TrackHistoryEntry
{
    public TrackProductionBreakHistoryEntry(
        DateTime startTime,
        DateTime? endTime,
        double startPosition,
        double? endPosition)
        : base(TrackHistoryEntryType.ProductionBreak, null, startTime, endTime, startPosition, endPosition)
    {
    }

    public TrackProductionBreakHistoryEntry(WuH.Ruby.Common.Track.BreakHistoryEntry historyEntry)
        : base(TrackHistoryEntryType.ProductionBreak, historyEntry)
    {
    }
}
