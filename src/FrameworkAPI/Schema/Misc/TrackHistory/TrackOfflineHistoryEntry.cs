using System;

namespace FrameworkAPI.Schema.Misc;

/// <summary>
/// Offline entry of the RUBY Track production history.
/// These production history entries are like the ones displayed in RUBY Track Operator UI.
/// </summary>
public class TrackOfflineHistoryEntry : TrackHistoryEntry
{
    public TrackOfflineHistoryEntry(
        DateTime startTime,
        DateTime? endTime,
        double startPosition,
        double? endPosition)
        : base(TrackHistoryEntryType.Offline, null, startTime, endTime, startPosition, endPosition)
    {
    }

    public TrackOfflineHistoryEntry(WuH.Ruby.Common.Track.OfflineHistoryEntry historyEntry)
        : base(TrackHistoryEntryType.Offline, historyEntry)
    {
    }
}
