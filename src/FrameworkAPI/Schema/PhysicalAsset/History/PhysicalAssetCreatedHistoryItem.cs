using PhysicalAssetDataHandler.Client.Models.Dtos.History;

namespace FrameworkAPI.Schema.PhysicalAsset.History;

/// <summary>
/// A history item for an inventory of the physical asset.
/// <remarks>Creation date of the history item is used as fallback when there is no inventory date set.</remarks>
/// </summary>
/// <param name="physicalAssetCreatedHistoryItemDto">The physical asset created history item dto.</param>
public class PhysicalAssetCreatedHistoryItem(PhysicalAssetCreatedHistoryItemDto physicalAssetCreatedHistoryItemDto) : PhysicalAssetHistoryItem(physicalAssetCreatedHistoryItemDto);