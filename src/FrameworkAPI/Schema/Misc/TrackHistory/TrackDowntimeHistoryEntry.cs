using System;

namespace FrameworkAPI.Schema.Misc;

/// <summary>
/// Downtime entry of the RUBY Track production history.
/// These production history entries are like the ones displayed in RUBY Track Operator UI.
/// </summary>
public class TrackDowntimeHistoryEntry : TrackHistoryEntry
{
    public TrackDowntimeHistoryEntry(
        string jobId,
        DateTime startTime,
        DateTime? endTime,
        double startPosition,
        double? endPosition,
        string firstLevelReason,
        string secondLevelReason,
        string location,
        bool isDetectedByRuby,
        string reportingUserId,
        string reportingUserFullName,
        bool beforeApproval,
        string? comment)
        : base(TrackHistoryEntryType.Downtime, jobId, startTime, endTime, startPosition, endPosition)
    {
        FirstLevelReason = firstLevelReason;
        SecondLevelReason = secondLevelReason;
        Location = location;
        IsDetectedByRuby = isDetectedByRuby;
        ReportingUserId = reportingUserId;
        ReportingUserFullName = reportingUserFullName;
        BeforeApproval = beforeApproval;
        Comment = comment;
    }

    public TrackDowntimeHistoryEntry(WuH.Ruby.Common.Track.DowntimeHistoryEntry historyEntry)
        : base(TrackHistoryEntryType.Downtime, historyEntry)
    {
        FirstLevelReason = historyEntry.FirstLevelReason;
        SecondLevelReason = historyEntry.SecondLevelReason;
        Location = historyEntry.Location;
        IsDetectedByRuby = historyEntry.IsDetectedByRuby;
        ReportingUserId = historyEntry.ReportingUserId;
        ReportingUserFullName = historyEntry.ReportingUserFullName;
        BeforeApproval = historyEntry.BeforeApproval;
        Comment = historyEntry.Comment;
    }

    /// <summary>
    /// Category of the downtime reason (like machine, organization, ...).
    /// This is an i18n text key in old format (like REASON.MACHINE).
    /// This information is either detected by RUBY (ProblemAnalyzer),
    /// entered by the operator (OperatorUI) or corrected by the manager (TRACK).
    /// </summary>
    public string? FirstLevelReason { get; set; }

    /// <summary>
    /// The actual downtime reason (like jam, missing material, ...).
    /// This is an i18n text key in old format (like REASON.MACHINE.JAM).
    /// This information is either detected by RUBY (ProblemAnalyzer),
    /// entered by the operator (OperatorUI) or corrected by the manager (TRACK).
    /// </summary>
    public string? SecondLevelReason { get; set; }

    /// <summary>
    /// The downtime location (like valve unit 1, opening station, ...).
    /// This is an i18n text key in old format (like LOCATION.VALVE_UNIT_1).
    /// This information is either detected by RUBY (ProblemAnalyzer),
    /// entered by the operator (OperatorUI) or corrected by the manager (TRACK).
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// Is true, if downtime reason and location were detected by RUBY (ProblemAnalyzer).
    /// </summary>
    public bool IsDetectedByRuby { get; set; }

    /// <summary>
    /// The Keycloak identifier of the last user that changed the downtime reason/location or the comment.
    /// </summary>
    public string? ReportingUserId { get; set; }

    /// <summary>
    /// The full name of the last user that changed the downtime reason/location or the comment.
    /// </summary>
    public string? ReportingUserFullName { get; set; }

    /// <summary>
    /// Is true, if this downtime occurred before the production approval
    /// and also the production approval feature is activated (in the AdminUI).
    /// </summary>
    public bool BeforeApproval { get; set; }

    /// <summary>
    /// A free text that gives more information to this downtime.
    /// This information is either entered by the operator (OperatorUI) or by the manager (TRACK).
    /// </summary>
    public string? Comment { get; set; }
}
