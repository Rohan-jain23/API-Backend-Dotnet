namespace FrameworkAPI.Schema.ProductGroup;

/// <summary>
/// A request to change the note for a machine.
/// </summary>
/// <param name="paperSackProductGroupId">ID of the product group (like: "v0-T0-VX").</param>
/// <param name="machineId">Unique machine identifier (usually WuH equipment number, like: "EQ12345").</param>
/// <param name="note">Note of the product group for this machine id.</param>
public class ProductGroupChangeMachineNoteRequest(string paperSackProductGroupId, string machineId, string? note)
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
    /// Note of the product group for this machine id.
    /// </summary>
    public string? Note { get; set; } = note;
}