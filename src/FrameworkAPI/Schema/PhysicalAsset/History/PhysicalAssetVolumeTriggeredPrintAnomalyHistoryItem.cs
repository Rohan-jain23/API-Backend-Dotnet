using FrameworkAPI.Schema.PhysicalAsset.CapabilityTest;
using PhysicalAssetDataHandler.Client.Models.Dtos.History;

namespace FrameworkAPI.Schema.PhysicalAsset.History;

/// <summary>
/// A history item for a detected volume triggered print anomaly from start to end position of the physical asset.
/// </summary>
/// <param name="physicalAssetVolumeTriggeredPrintAnomalyHistoryItemDto">The physical asset volume triggered print anomaly history item dto.</param>
public class PhysicalAssetVolumeTriggeredPrintAnomalyHistoryItem(
    PhysicalAssetVolumeTriggeredPrintAnomalyHistoryItemDto physicalAssetVolumeTriggeredPrintAnomalyHistoryItemDto)
    : PhysicalAssetHistoryItem(physicalAssetVolumeTriggeredPrintAnomalyHistoryItemDto)
{
    /// <summary>
    /// The start position on the physical asset.
    /// [Source: PhysicalAssetHistory]
    /// </summary>
    public double StartPosition { get; set; } = physicalAssetVolumeTriggeredPrintAnomalyHistoryItemDto.StartPosition;

    /// <summary>
    /// The end position on the physical asset.
    /// [Source: PhysicalAssetHistory]
    /// </summary>
    public double EndPosition { get; set; } = physicalAssetVolumeTriggeredPrintAnomalyHistoryItemDto.EndPosition;

    /// <summary>
    /// The unit of the set and measured value.
    /// [Source: PhysicalAssetHistory]
    /// </summary>
    public string Unit { get; set; } = physicalAssetVolumeTriggeredPrintAnomalyHistoryItemDto.Unit;

    /// <summary>
    /// The specification used to check the test result.
    /// [Source: PhysicalAssetHistory]
    /// </summary>
    public AniloxCapabilityTestSpecification AniloxCapabilityTestSpecification { get; set; } = new(physicalAssetVolumeTriggeredPrintAnomalyHistoryItemDto.AniloxCapabilityTestSpecificationDto);
}