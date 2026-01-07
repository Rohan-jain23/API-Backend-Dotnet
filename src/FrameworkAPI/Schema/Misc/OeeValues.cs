namespace FrameworkAPI.Schema.Misc;

/// <summary>
/// The overall equipment effectiveness (OEE) and its sub-values.
/// </summary>
public class OeeValues(double? oee, double? availability, double? effectiveness, double? quality)
{
    /// <summary>
    /// The overall equipment effectiveness is a measure that identifies the percentage of production time that is truly productive.
    /// For the comparison of jobs, the OEE has its weaknesses as the necessary setup time (which depends on the previous job) has a huge influence.
    /// It is 'Availability' * 'Effectiveness' * 'Quality'.
    /// </summary>
    public double? OEE { get; set; } = oee;

    /// <summary>
    /// This shows which percentage of the planned production time the machine was productive.
    /// The missing percentage to 100 % are the downtime and setup losses.
    /// </summary>
    public double? Availability { get; set; } = availability;

    /// <summary>
    /// This shows which percentage of the target speed was reached during the productive time.
    /// The missing percentage to 100 % is the performance/speed loss.
    /// Is 'null' if there is no 'TargetSpeed'.
    /// </summary>
    public double? Effectiveness { get; set; } = effectiveness;

    /// <summary>
    /// This shows which percentage of the production was in acceptable quality (inverted scrap ratio).
    /// The missing percentage to 100 % is the quality loss.
    /// </summary>
    public double? Quality { get; set; } = quality;
}
