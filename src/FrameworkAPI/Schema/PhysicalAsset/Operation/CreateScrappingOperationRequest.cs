using System;

namespace FrameworkAPI.Schema.PhysicalAsset.Operation;

/// <summary>
/// A request to create a physical asset scrapping operation.
/// </summary>
public class CreateScrappingOperationRequest(
    string physicalAssetId,
    string? note,
    DateTime scrapDateTime) : CreateOperationRequest(physicalAssetId, note)
{
    /// <summary>
    /// Date the physical asset is scrapped.
    /// </summary>
    public DateTime ScrapDateTime { get; set; } = scrapDateTime;
}