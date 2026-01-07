using PhysicalAssetDataHandler.Client.Models.Dtos.History;
using PhysicalAssetDataHandler.Client.Models.Enums;

namespace FrameworkAPI.Schema.PhysicalAsset.History;

/// <summary>
/// A history item for a cleaning of the physical asset.
/// </summary>
/// <param name="physicalAssetCleanedHistoryItemDto">The physical asset cleaned history item dto.</param>
public class PhysicalAssetCleanedHistoryItem(PhysicalAssetCleanedHistoryItemDto physicalAssetCleanedHistoryItemDto) : PhysicalAssetHistoryItem(physicalAssetCleanedHistoryItemDto)
{
    /// <summary>
    /// Type of the cleaning operation.
    /// [Source: PhysicalAssetHistory]
    /// </summary>
    public CleaningOperationType CleaningOperationType { get; set; } = physicalAssetCleanedHistoryItemDto.CleaningOperationType;

    /// <summary>
    /// The flag which signals if the cleaning resets past volume defects.
    /// [Source: CleaningOperation]
    /// </summary>
    public bool ResetVolumeDefects { get; set; } = physicalAssetCleanedHistoryItemDto.ResetVolumeDefects;
}