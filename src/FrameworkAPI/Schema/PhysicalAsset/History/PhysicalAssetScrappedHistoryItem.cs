using PhysicalAssetDataHandler.Client.Models.Dtos.History;

namespace FrameworkAPI.Schema.PhysicalAsset.History;

/// <summary>
/// A history item for a scrapping of the physical asset.
/// </summary>
/// <param name="physicalAssetScrappedHistoryItemDto">The physical asset scrapped history item dto.</param>
public class PhysicalAssetScrappedHistoryItem(PhysicalAssetScrappedHistoryItemDto physicalAssetScrappedHistoryItemDto) : PhysicalAssetHistoryItem(physicalAssetScrappedHistoryItemDto);