namespace FrameworkAPI.Schema.Misc;

/// <summary>
/// Generic status category which a production status is assigned to.
/// This is the basis for calculation of key performance indicators like OEE (overall equipment effectiveness).
/// </summary>
public enum ProductionStatusCategory
{
    /// <summary>
    /// Machine is running well.
    /// </summary>
    Production,

    /// <summary>
    /// Machine is running but produces waste.
    /// </summary>
    Scrap,

    /// <summary>
    /// Machine needs to be adjusted on job change-over (includes run-up).
    /// </summary>
    Setup,

    /// <summary>
    /// Unplanned machine stop.
    /// </summary>
    DownTime,

    /// <summary>
    /// Scheduled non-production / planned downtime.
    /// </summary>
    ScheduledNonProduction,

    /// <summary>
    /// Connection between machine and RUBY is interrupted.
    /// </summary>
    Offline,

    /// <summary>
    /// Machine transmitted invalid data.
    /// </summary>
    InvalidData
}