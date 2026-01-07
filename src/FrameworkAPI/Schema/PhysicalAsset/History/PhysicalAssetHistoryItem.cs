using System;
using HotChocolate.Types;
using PhysicalAssetDataHandler.Client.Models.Dtos.History;
using PhysicalAssetDataHandler.Client.Models.Enums;

namespace FrameworkAPI.Schema.PhysicalAsset.History;

/// <summary>
/// Generic interface for physical asset history items.
/// </summary>
/// <param name="physicalAssetHistoryItemDto">The physical asset history item dto.</param>
[InterfaceType]
public abstract class PhysicalAssetHistoryItem(PhysicalAssetHistoryItemDto physicalAssetHistoryItemDto)
{

    /// <summary>
    /// Type of the physical asset history item.
    /// [Source: PhysicalAssetHistory]
    /// </summary>
    public PhysicalAssetHistoryItemType PhysicalAssetHistoryItemType { get; set; } = physicalAssetHistoryItemDto.PhysicalAssetHistoryItemType;

    /// <summary>
    /// Generated unique id of the source resulting into the history item.
    /// [Source: PhysicalAssetHistory]
    /// </summary>
    public string SourceId { get; set; } = physicalAssetHistoryItemDto.SourceId;

    /// <summary>
    /// Date on which the history event occurred (is within the lifetime of the physical asset).
    /// [Source: PhysicalAssetHistory]
    /// </summary>
    public DateTime CreatedAt { get; set; } = physicalAssetHistoryItemDto.CreatedAt;

    /// <summary>
    /// The additional note added by the user.
    /// [Source: PhysicalAssetHistory]
    /// </summary>
    public string? Note { get; set; } = physicalAssetHistoryItemDto.Note;

    internal static PhysicalAssetHistoryItem CreateInstance(PhysicalAssetHistoryItemDto physicalAssetHistoryItemDto)
    {
        return physicalAssetHistoryItemDto.PhysicalAssetHistoryItemType switch
        {
            PhysicalAssetHistoryItemType.Created => new PhysicalAssetCreatedHistoryItem(
                (PhysicalAssetCreatedHistoryItemDto)physicalAssetHistoryItemDto),
            PhysicalAssetHistoryItemType.Delivered => new PhysicalAssetDeliveredHistoryItem(
                (PhysicalAssetDeliveredHistoryItemDto)physicalAssetHistoryItemDto),
            PhysicalAssetHistoryItemType.LowVolume => new PhysicalAssetLowVolumeHistoryItem(
                (PhysicalAssetLowVolumeHistoryItemDto)physicalAssetHistoryItemDto),
            PhysicalAssetHistoryItemType.HighVolume => new PhysicalAssetHighVolumeHistoryItem(
                (PhysicalAssetHighVolumeHistoryItemDto)physicalAssetHistoryItemDto),
            PhysicalAssetHistoryItemType.VolumeMeasured => new PhysicalAssetVolumeMeasuredHistoryItem(
                (PhysicalAssetVolumeMeasuredHistoryItemDto)physicalAssetHistoryItemDto),
            PhysicalAssetHistoryItemType.Cleaned => new PhysicalAssetCleanedHistoryItem(
                (PhysicalAssetCleanedHistoryItemDto)physicalAssetHistoryItemDto),
            PhysicalAssetHistoryItemType.Scrapped => new PhysicalAssetScrappedHistoryItem(
                (PhysicalAssetScrappedHistoryItemDto)physicalAssetHistoryItemDto),
            PhysicalAssetHistoryItemType.ScoringLine => new PhysicalAssetScoringLineHistoryItem(
                (PhysicalAssetScoringLineHistoryItemDto)physicalAssetHistoryItemDto),
            PhysicalAssetHistoryItemType.SurfaceAnomaly => new PhysicalAssetSurfaceAnomalyHistoryItem(
                (PhysicalAssetSurfaceAnomalyHistoryItemDto)physicalAssetHistoryItemDto),
            PhysicalAssetHistoryItemType.VolumeTriggeredPrintAnomaly => new PhysicalAssetVolumeTriggeredPrintAnomalyHistoryItem(
                (PhysicalAssetVolumeTriggeredPrintAnomalyHistoryItemDto)physicalAssetHistoryItemDto),
            _ => throw new ArgumentException(
                $"Creating a physical asset history item is not supported for the type '{physicalAssetHistoryItemDto.PhysicalAssetHistoryItemType}'.")
        };
    }
}