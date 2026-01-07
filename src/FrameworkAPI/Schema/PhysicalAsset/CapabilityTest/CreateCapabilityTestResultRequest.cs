using System;

namespace FrameworkAPI.Schema.PhysicalAsset.CapabilityTest;

/// <summary>
/// A request to create a physical asset test result.
/// </summary>
public abstract class CreateCapabilityTestResultRequest(
    string physicalAssetId,
    DateTime testDateTime,
    string? note)
{
    /// <summary>
    /// Unique identifier of the tested physical asset.
    /// </summary>
    public string PhysicalAssetId { get; set; } = string.IsNullOrWhiteSpace(physicalAssetId)
            ? throw new ArgumentException($"{nameof(physicalAssetId)} is null or white space!")
            : physicalAssetId;

    /// <summary>
    /// Date the physical asset was tested.
    /// </summary>
    public DateTime TestDateTime { get; set; } = testDateTime;

    /// <summary>
    /// The additional note for this test result.
    /// </summary>
    public string? Note { get; set; } = note;
}