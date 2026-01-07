namespace FrameworkAPI.Schema.Misc;

/// <summary>
/// Cumulated minutes the machine was in each production status for the queried time span or job.
/// Also, the total times are provided.
/// These values are the base for many KPI calculations (like OEE).
/// </summary>
public class ProductionTimes(
    double totalTimeInMin,
    double totalPlannedProductionTimeInMin,
    double notQueryRelatedTimeInMin,
    double productionTimeInMin,
    double generalDownTimeInMin,
    double jobRelatedDownTimeInMin,
    double setupTimeInMin,
    double scrapTimeInMin,
    double scheduledNonProductionTimeInMin)
{
    /// <summary>
    /// Total calendar time
    /// (= TotalPlannedProductionTimeInMin + NotQueryRelatedTimeInMin)
    /// </summary>
    public double TotalTimeInMin { get; set; } = totalTimeInMin;

    /// <summary>
    /// Relevant time for KPI calculation
    /// </summary>
    public double TotalPlannedProductionTimeInMin { get; set; } = totalPlannedProductionTimeInMin;

    /// <summary>
    /// Time that is not relevant for KPI calculation
    /// (For time span queries, this only includes 'ScheduledNonProductionTimeInMin'.
    /// For job queries, this additionally includes 'GeneralDownTimeInMin' and time related to other jobs.)
    /// </summary>
    public double NotQueryRelatedTimeInMin { get; set; } = notQueryRelatedTimeInMin;

    /// <summary>
    /// Time the machine was in production status 'Production'.
    /// </summary>
    public double ProductionTimeInMin { get; set; } = productionTimeInMin;

    /// <summary>
    /// Time the machine was in production status 'DownTime', but this excludes job-related downtimes.
    /// This also includes times the machine was in production status 'Offline' or 'InvalidData'.
    /// </summary>
    public double GeneralDownTimeInMin { get; set; } = generalDownTimeInMin;

    /// <summary>
    /// Time the machine was in production status 'DownTime',
    /// if this downtime was job-related (this only exists on paper sack machines).
    /// </summary>
    public double JobRelatedDownTimeInMin { get; set; } = jobRelatedDownTimeInMin;

    /// <summary>
    /// Time the machine was in production status 'Setup'
    /// </summary>
    public double SetupTimeInMin { get; set; } = setupTimeInMin;

    /// <summary>
    /// Time the machine was in production status 'Scrap'.
    /// This production status does not exist on paper sack machines.
    /// </summary>
    public double ScrapTimeInMin { get; set; } = scrapTimeInMin;

    /// <summary>
    /// Time the machine was in production status 'ScheduledNonProduction'.
    /// </summary>
    public double ScheduledNonProductionTimeInMin { get; set; } = scheduledNonProductionTimeInMin;
}
