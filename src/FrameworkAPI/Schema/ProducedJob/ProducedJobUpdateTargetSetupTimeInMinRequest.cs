namespace FrameworkAPI.Schema.ProducedJob;

/// <summary>
/// A request to change the target setup time for a job.
/// </summary>
/// <param name="machineId">Unique machine identifier (usually WuH equipment number, like: "EQ12345").</param>
/// <param name="associatedJob">Unique job identifier.</param>
/// <param name="targetSetupTimeInMin">Target setup time to set for the job.</param>
public class ProducedJobUpdateTargetSetupTimeInMinRequest(
    string machineId,
    string associatedJob,
    double targetSetupTimeInMin)
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
    /// Target setup time to set for the job.
    /// </summary>
    public double TargetSetupTimeInMin { get; set; } = targetSetupTimeInMin;
}