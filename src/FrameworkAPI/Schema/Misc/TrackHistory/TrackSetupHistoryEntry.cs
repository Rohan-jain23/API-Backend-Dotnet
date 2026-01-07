using System;

namespace FrameworkAPI.Schema.Misc;

/// <summary>
/// Setup entry of the RUBY Track production history.
/// These production history entries are like the ones displayed in RUBY Track Operator UI.
/// </summary>
public class TrackSetupHistoryEntry : TrackHistoryEntry
{
    public TrackSetupHistoryEntry(
        string jobId,
        DateTime startTime,
        DateTime? endTime,
        double startPosition,
        double? endPosition,
        bool beforeApproval)
        : base(TrackHistoryEntryType.Setup, jobId, startTime, endTime, startPosition, endPosition)
    {
        BeforeApproval = beforeApproval;
    }

    public TrackSetupHistoryEntry(WuH.Ruby.Common.Track.SetupHistoryEntry historyEntry)
        : base(TrackHistoryEntryType.Setup, historyEntry)
    {
        BeforeApproval = historyEntry.BeforeApproval;
    }

    /// <summary>
    /// Is true, if this setup time was before the production approval
    /// and also the production approval feature is activated (in the AdminUI).
    /// </summary>
    public bool BeforeApproval { get; set; }
}
