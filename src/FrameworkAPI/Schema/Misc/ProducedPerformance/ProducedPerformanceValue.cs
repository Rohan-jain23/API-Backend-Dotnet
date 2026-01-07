namespace FrameworkAPI.Schema.Misc;

/// <summary>
/// Values to measure the performance of a job / product group in a specific discipline (speed, setup, downtime, scrap) or in total.
/// </summary>
public class ProducedPerformanceValue(
    double? actualValue,
    double? targetValue,
    TargetValueSource? targetValueSource,
    double? lostTimeInMin,
    double? wonProductivity,
    string unit)
{
    /// <summary>
    /// The value that was measured during production of this job / product group.
    /// On product groups, this is the average value per job.
    /// </summary>
    public double? ActualValue { get; set; } = actualValue;

    /// <summary>
    /// The target/expected value that should be reached for this job / product group.
    /// Is 'null' if no target value was given for this job / product group.
    /// On product groups, this is the average value per job.
    /// </summary>
    public double? TargetValue { get; set; } = targetValue;

    /// <summary>
    /// Describes how the 'TargetValue' for this job was defined.
    /// is 'null' for product groups.
    /// </summary>
    public TargetValueSource? TargetValueSource { get; set; } = targetValueSource;

    /// <summary>
    /// Minutes that were lost compared to the expectation in total / because of this discipline
    /// (-> negative values indicate that it was better than expected).
    /// </summary>
    public double? LostTimeInMin { get; set; } = lostTimeInMin;

    /// <summary>
    /// Percentage of productivity that was won compared to the expectation in total / because of this discipline
    /// (-> negative values indicate that it was worse than expected).
    /// For the total value, this is the relation between 'ActualValue' and 'TargetValue'
    /// (-> this 'total productivity indicator' is the perfect value to compare jobs / product groups).
    /// For the disciplines, this is difference of 'ActualValue' and 'TargetValue' in relation to the total productivity
    /// (-> sum of all disciplines equals total percentage).
    /// </summary>
    public double? WonProductivity { get; set; } = wonProductivity;

    /// <summary>
    /// The unit of 'ActualValue' and 'TargetValue'.
    /// (if the unit needs to be translated, the corresponding i18n tag is provided here; for example '{{ label.items }}/{{ label.minutesShort }}').
    /// </summary>
    public string Unit { get; set; } = unit;
}
