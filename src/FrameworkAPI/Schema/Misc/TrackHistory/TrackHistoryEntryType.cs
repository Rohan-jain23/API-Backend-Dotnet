using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FrameworkAPI.Schema.Misc;

/// <summary>
/// Enumeration of all supported history entry types.
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum TrackHistoryEntryType
{
    /// <summary>
    /// Changeover from one job to another (including run-up).
    /// </summary>
    Setup,
    /// <summary>
    /// Unplanned downtime of the machine.
    /// </summary>
    Downtime,
    /// <summary>
    /// Machine is producing.
    /// </summary>
    Production,
    /// <summary>
    /// Either scheduled non-production (lunch break, scheduled cleaning shift, weekend, holiday, ...)
    /// or this is shown in job history if the job got interrupted by another job.
    /// </summary>
    ProductionBreak,
    /// <summary>
    /// Machine is only producing waste (this status does not exist on paper sack machines).
    /// </summary>
    Scrap,
    /// <summary>
    /// Machine is turned-off.
    /// In error cases, this could also be a connection issue or the machine is sending invalid data.
    /// </summary>
    Offline,
}
