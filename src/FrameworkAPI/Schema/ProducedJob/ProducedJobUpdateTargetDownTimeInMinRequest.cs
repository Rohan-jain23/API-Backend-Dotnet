namespace FrameworkAPI.Schema.ProducedJob;

/// <summary>
/// A request to change the target downtime for a job.
/// </summary>
/// <param name="machineId">Unique machine identifier (usually WuH equipment number, like: "EQ12345").</param>
/// <param name="associatedJob">Unique job identifier.</param>
/// <param name="targetDownTimeInMin">Target downtime to set for the job.</param>
public class ProducedJobUpdateTargetDownTimeInMinRequest(
    string machineId,
    string associatedJob,
    double targetDownTimeInMin)
{
    /// <summary>
    /// Unique job identifier.
    /// </summary>
    public string AssociatedJob { get; set; } = associatedJob;

    /// <summary>
    /// Unique machine identifier (usually WuH equipment number, like: "EQ12345").
    /// </summary>
    public string MachineId { get; set; } = machineId;

    /// <summary>
    /// Target downtime to set for the job.
    /// </summary>
    public double TargetDownTimeInMin { get; set; } = targetDownTimeInMin;
}