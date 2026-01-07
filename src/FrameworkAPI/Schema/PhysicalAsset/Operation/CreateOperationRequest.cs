using System;

namespace FrameworkAPI.Schema.PhysicalAsset.Operation;

/// <summary>
/// A request to create a physical asset operation.
/// </summary>
public abstract class CreateOperationRequest
{
    protected CreateOperationRequest(string physicalAssetId, string? note)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(physicalAssetId);

        PhysicalAssetId = physicalAssetId;
        Note = note;
    }

    /// <summary>
    /// Unique identifier of the sc physical asset.
    /// </summary>
    public string PhysicalAssetId { get; set; }

    /// <summary>
    /// The additional note for this operation.
    /// </summary>
    public string? Note { get; set; }
}