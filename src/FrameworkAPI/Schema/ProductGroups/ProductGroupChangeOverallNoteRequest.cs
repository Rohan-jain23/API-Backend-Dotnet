namespace FrameworkAPI.Schema.ProductGroup;

/// <summary>
/// A request the change the overall note for a product group.
/// </summary>
/// <param name="paperSackProductGroupId">ID of the product group (like: "v0-T0-VX").</param>
/// <param name="note">Note of the product group.</param>
public class ProductGroupChangeOverallNoteRequest(string paperSackProductGroupId, string? note)
{
    /// <summary>
    /// ID of the product group (like: "v0-T0-VX").
    /// </summary>
    public string PaperSackProductGroupId { get; set; } = paperSackProductGroupId;

    /// <summary>
    /// Note of the product group.
    /// </summary>
    public string? Note { get; set; } = note;
}