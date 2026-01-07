using System;

namespace FrameworkAPI.Schema.Misc;

/// <summary>
/// Scrap entry of the RUBY Track production history.
/// These production history entries are like the ones displayed in RUBY Track Operator UI.
/// </summary>
public class TrackScrapHistoryEntry : TrackHistoryEntry
{
    public TrackScrapHistoryEntry(
        string jobId,
        DateTime startTime,
        DateTime? endTime,
        double startPosition,
        double? endPosition)
        : base(TrackHistoryEntryType.Scrap, jobId, startTime, endTime, startPosition, endPosition)
    {
    }

    public TrackScrapHistoryEntry(WuH.Ruby.Common.Track.ScrapHistoryEntry historyEntry)
        : base(TrackHistoryEntryType.Scrap, historyEntry)
    {
    }
}
