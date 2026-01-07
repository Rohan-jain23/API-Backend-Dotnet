using PhysicalAssetDataHandler.Client.Models.Dtos.History;

namespace FrameworkAPI.Schema.PhysicalAsset.History;

/// <summary>
/// A history item for a delivery of the physical asset.
/// </summary>
/// <param name="physicalAssetDeliveredHistoryItemDto">The physical asset delivered history item dto.</param>
public class PhysicalAssetDeliveredHistoryItem(PhysicalAssetDeliveredHistoryItemDto physicalAssetDeliveredHistoryItemDto) : PhysicalAssetHistoryItem(physicalAssetDeliveredHistoryItemDto);