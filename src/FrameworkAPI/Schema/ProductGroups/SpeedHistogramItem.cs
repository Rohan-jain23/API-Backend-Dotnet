namespace FrameworkAPI.Schema.ProductGroup;

/// <summary>
/// Speed histogram values for one speed level
/// </summary>
public class SpeedHistogramItem(int speedLevel, double durationInMin, double? capacityUtilizationRate)
{
    /// <summary>
    /// Bucket value of this speed level.
    /// </summary>
    public int SpeedLevel { get; } = speedLevel;

    /// <summary>
    /// Duration in minutes the machine was running at this speed level.
    /// This also includes job-specific downtimes happening at this speed level as they affect productivity.
    /// Certain situations, like roll changes or ramp-ups, are excluded to maintain an accurate picture.
    /// </summary>
    public double DurationInMin { get; } = durationInMin;

    /// <summary>
    /// Capacity utilization rate in percent during the time the machine was running at this speed level.
    /// This is the total good production rate (which includes losses due to scrap and job-related downtimes) divided by the maximum machine speed.
    /// This value is 'null', if the machine was not running long enough at that speed level (minimum 1 hour).
    /// The higher the capacity utilization rate is, the better is the productivity.
    /// For example:
    /// A machine can run maximal 400 items/min.
    /// If the machine runs at speed level 200 items/min, the capacity utilization rate can maximum be 50 %.
    /// In this example, we don't have any downtimes and have a scrap rate of 2 % (-> 4 items/min scrap).
    /// This results in a capacity utilization rate of 49 % (196 / 400) at speed level 200.
    /// If the machine runs at speed level 300 items/min, the capacity utilization rate can maximum be 75 %.
    /// But in this example, we get a lot of downtimes (30 %) and scrap rate raises to 10 % (total good production rate: 300 * 0.70 - 300 * 0.1 = 180).
    /// This results in a capacity utilization rate of 45 % (180 / 400) at speed level 300.
    /// Therefore, the productivity is better at 200 items/min.
    /// </summary>
    public double? CapacityUtilizationRate { get; } = capacityUtilizationRate;
}