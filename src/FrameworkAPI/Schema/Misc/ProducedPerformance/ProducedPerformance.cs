namespace FrameworkAPI.Schema.Misc;

/// <summary>
/// Values that are measuring the productivity of a job / product group (-> RUBYs alternative for OEE).
/// These values are calculated by comparing actual values to target/expected values.
/// Therefore, some values are 'null' when the related target values are not given for the job / product group.
/// If the target values are set properly, this is the perfect measure to evaluate the performance of this job / product group.
/// This basically says how well the production was running compared to the expectations.
/// The greatest advantage over OEE is that the 'Total.WonProductivity' percentage allows comparison of jobs / product groups
/// that not depends on the different setup efforts (which are highly dependant on the previous jobs).
/// </summary>
public class ProducedPerformance(
    ProducedPerformanceValue speed,
    ProducedPerformanceValue setup,
    ProducedPerformanceValue downtime,
    ProducedPerformanceValue scrap,
    ProducedPerformanceValue total)
{
    /// <summary>
    /// Values to measure the speed performance of this job / product group.
    /// This compares the average speed during production with the target speed.
    /// These values are only meaningful, if the target speed is set properly.
    /// </summary>
    public ProducedPerformanceValue Speed { get; set; } = speed;

    /// <summary>
    /// Values to measure the setup performance of this job / product group.
    /// This compares the actual setup time with the expected setup time.
    /// If the expected setup time is not given for this job / product group, the actual setup time is taken to calculate the total value.
    /// Like this, the other values stay meaningful even if the expected setup time is not set (currently only possible via Connect4Flow).
    /// </summary>
    public ProducedPerformanceValue Setup { get; set; } = setup;

    /// <summary>
    /// Values to measure the downtime performance of this job / product group.
    /// This compares the job-related downtime with the target downtime ratio.
    /// </summary>
    public ProducedPerformanceValue Downtime { get; set; } = downtime;

    /// <summary>
    /// Values to measure the scrap performance of this job / product group.
    /// This compares the scrap during production with the target scrap ratio.
    /// </summary>
    public ProducedPerformanceValue Scrap { get; set; } = scrap;

    /// <summary>
    /// Values to measure the overall performance of this job / product group.
    /// This compared the actual overall good production rate (which is derived by average speed, setup time, job-related downtimes and scrap counts)
    /// with the target overall good production rate (which is derived by target speed, (expected) setup time, target downtime ratio and target scrap ratio).
    /// If the target values are set properly, this is the perfect measure to evaluate the performance of this job / product group.
    /// This basically says how well the production was running compared to the expectations.
    /// The greatest advantage over OEE is that the 'WonProductivity' percentage allows comparison of jobs / product groups
    /// which not depends on the necessary setup effort because of different previous jobs.
    /// </summary>
    public ProducedPerformanceValue Total { get; set; } = total;
}
