namespace FrameworkAPI.Schema.ProducedJob;

/// <summary>
/// A request to change the target scrap count during production for a job.
/// </summary>
/// <param name="machineId">Unique machine identifier (usually WuH equipment number, like: "EQ12345").</param>
/// <param name="associatedJob">Unique job identifier.</param>
/// <param name="targetScrapCountDuringProduction">Target scrap count during production to set for the job.</param>
public class ProducedJobUpdateTargetScrapCountDuringProductionRequest(
    string machineId,
    string associatedJob,
    double targetScrapCountDuringProduction)
{
    /// <summary>
    /// Unique job identifier.
    /// </summary>
    public string AssociatedJob { get; set; } = associatedJob;

    /// <summary>
    /// Unique machine identifier (usually WuH equipment number, like: "EQ12345").
    /// </summary>
    public string MachineId { get; set; } = machineId;

    public double TargetScrapCountDuringProduction { get; set; } = targetScrapCountDuringProduction;
}