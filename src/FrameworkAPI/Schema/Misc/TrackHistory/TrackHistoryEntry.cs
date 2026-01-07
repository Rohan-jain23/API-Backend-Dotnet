using System;
using HotChocolate.Types;

namespace FrameworkAPI.Schema.Misc;

/// <summary>
/// Abstract class for all EntryTypes of the RUBY Track production history.
/// These production history entries are like the ones displayed in RUBY Track Operator UI.
/// </summary>
[InterfaceType]
public abstract class TrackHistoryEntry
{
    protected TrackHistoryEntry(
        TrackHistoryEntryType entryType,
        string? jobId,
        DateTime startTime,
        DateTime? endTime,
        double startPosition,
        double? endPosition)
    {
        EntryType = entryType;
        JobId = jobId;
        StartTime = startTime;
        EndTime = endTime;
        StartPosition = startPosition;
        EndPosition = endPosition;
    }

    protected TrackHistoryEntry(TrackHistoryEntryType entryType, WuH.Ruby.Common.Track.HistoryEntry historyEntry)
    {
        EntryType = entryType;
        JobId = historyEntry.JobId;
        StartTime = historyEntry.Timestamp;
        EndTime = historyEntry.DurationInMin is null ? null : historyEntry.Timestamp.AddMinutes(historyEntry.DurationInMin.Value);
        StartPosition = historyEntry.StartPosition;
        EndPosition = historyEntry.EndPosition;
    }

    /// <summary>
    /// Type of the history entry.
    /// This also defines which additional data will be available.
    /// </summary>
    public TrackHistoryEntryType EntryType { get; set; }

    /// <summary>
    /// Id of produced job (unique for the machine).
    /// Is null, if this history entry is not job-related.
    /// </summary>
    public string? JobId { get; set; }

    /// <summary>
    /// Start timestamp of the history entry.
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// End timestamp of the history entry.
    /// Is null, if the history entry is active.
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// Start position (total operation counter) of the history entry.
    /// This information is necessary when a history entry should be changed.
    /// </summary>
    public double StartPosition { get; set; }

    /// <summary>
    /// End position (total operation counter) of the history entry.
    /// Is null, if the history entry is active.
    /// This information is necessary when a history entry should be changed.
    /// </summary>
    public double? EndPosition { get; set; }

    /// <summary>
    /// Is true, if the operator still needs to enter information for this entry (for example: downtime reason).
    /// </summary>
    public bool HasPendingToDo { get; }

    internal static TrackHistoryEntry CreateInstance(WuH.Ruby.Common.Track.HistoryEntry historyEntry)
    {
        return historyEntry.EntryType switch
        {
            WuH.Ruby.Common.Track.HistoryEntryType.Setup => new TrackSetupHistoryEntry((WuH.Ruby.Common.Track.SetupHistoryEntry)historyEntry),
            WuH.Ruby.Common.Track.HistoryEntryType.Downtime => new TrackDowntimeHistoryEntry((WuH.Ruby.Common.Track.DowntimeHistoryEntry)historyEntry),
            WuH.Ruby.Common.Track.HistoryEntryType.Production => new TrackProductionHistoryEntry((WuH.Ruby.Common.Track.ProductionHistoryEntry)historyEntry),
            WuH.Ruby.Common.Track.HistoryEntryType.Break => new TrackProductionBreakHistoryEntry((WuH.Ruby.Common.Track.BreakHistoryEntry)historyEntry),
            WuH.Ruby.Common.Track.HistoryEntryType.Scrap => new TrackScrapHistoryEntry((WuH.Ruby.Common.Track.ScrapHistoryEntry)historyEntry),
            WuH.Ruby.Common.Track.HistoryEntryType.Offline => new TrackOfflineHistoryEntry((WuH.Ruby.Common.Track.OfflineHistoryEntry)historyEntry),
            _ => throw new ArgumentException($"Creating a TrackHistoryEntry is not supported for the type '{historyEntry.EntryType}'.")
        };
    }
}
