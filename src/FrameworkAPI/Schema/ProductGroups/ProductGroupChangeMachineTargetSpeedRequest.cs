namespace FrameworkAPI.Schema.ProductGroup;

/// <summary>
/// A request to change the target speed for a machine.
/// </summary>
/// <param name="paperSackProductGroupId">ID of the product group (like: "v0-T0-VX").</param>
/// <param name="machineId">Unique machine identifier (usually WuH equipment number, like: "EQ12345").</param>
/// <param name="targetSpeed">Target speed to set for the machine id used by matching jobs.</param>
public class ProductGroupChangeMachineTargetSpeedRequest(string paperSackProductGroupId, string machineId, double? targetSpeed)
{
    /// <summary>
    /// ID of the product group (like: "v0-T0-VX").
    /// </summary>
    public string PaperSackProductGroupId { get; set; } = paperSackProductGroupId;

    /// <summary>
    /// Unique machine identifier (usually WuH equipment number, like: "EQ12345").
    /// </summary>
    public string MachineId { get; set; } = machineId;

    /// <summary>
    /// Target speed to set for the machine id used by matching jobs.
    /// </summary>
    public double? TargetSpeed { get; set; } = targetSpeed;
}