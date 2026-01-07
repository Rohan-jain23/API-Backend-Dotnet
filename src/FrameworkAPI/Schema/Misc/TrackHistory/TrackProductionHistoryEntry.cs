using System;

namespace FrameworkAPI.Schema.Misc;

/// <summary>
/// Production entry of the RUBY Track production history.
/// These production history entries are like the ones displayed in RUBY Track Operator UI.
/// </summary>
public class TrackProductionHistoryEntry : TrackHistoryEntry
{
    public TrackProductionHistoryEntry(
        string jobId,
        DateTime startTime,
        DateTime? endTime,
        double startPosition,
        double? endPosition,
        double? averageSpeed,
        double? minorStopsCount,
        double? minorStopsDuration,
        bool beforeApproval)
        : base(TrackHistoryEntryType.Production, jobId, startTime, endTime, startPosition, endPosition)
    {
        AverageSpeed = averageSpeed;
        MinorStopsCount = minorStopsCount;
        MinorStopsDurationInMin = minorStopsDuration;
        BeforeApproval = beforeApproval;
    }

    public TrackProductionHistoryEntry(WuH.Ruby.Common.Track.ProductionHistoryEntry historyEntry)
        : base(TrackHistoryEntryType.Production, historyEntry)
    {
        AverageSpeed = historyEntry.AverageSpeed;
        MinorStopsCount = historyEntry.MinorStopsCount;
        MinorStopsDurationInMin = historyEntry.MinorStopsDuration;
        BeforeApproval = historyEntry.BeforeApproval;
    }

    /// <summary>
    /// Average value of the machine speed during the machine was in status production
    /// (minor stops are not considered here).
    /// </summary>
    public double? AverageSpeed { get; set; }

    /// <summary>
    /// Count of minor stops which occurred in between the production times during this time span.
    /// Minor stops are downtimes that are shorter than the 'minor stop threshold' (configured in AdminUI).
    /// Is null, if there are no minor stops in this time span.
    /// </summary>
    public double? MinorStopsCount { get; set; }

    /// <summary>
    /// Total duration of minor stops (in minutes) which occurred in between the production times during this time span.
    /// Minor stops are downtimes that are shorter than the 'minor stop threshold' (configured in AdminUI)
    /// Is null, if there are no minor stops in this time span.
    /// </summary>
    public double? MinorStopsDurationInMin { get; set; }

    /// <summary>
    /// Is true, if this production time was before the production approval
    /// and also the production approval feature is activated (in the AdminUI).
    /// </summary>
    public bool BeforeApproval { get; set; }
}
