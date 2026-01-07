namespace FrameworkAPI.Schema.ProducedJob;

/// <summary>
/// A request to change the target speed for a job.
/// </summary>
/// <param name="machineId">Unique machine identifier (usually WuH equipment number, like: "EQ12345").</param>
/// <param name="associatedJob">Unique job identifier.</param>
/// <param name="targetSpeed">Target speed to set for the job.</param>
public class ProducedJobUpdateTargetSpeedRequest(string machineId, string associatedJob, double targetSpeed)
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
    /// Target speed to set for the job.
    /// </summary>
    public double TargetSpeed { get; set; } = targetSpeed;
}