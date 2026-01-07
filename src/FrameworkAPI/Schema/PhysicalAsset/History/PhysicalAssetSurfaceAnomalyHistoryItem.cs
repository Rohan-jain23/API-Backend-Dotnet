using FrameworkAPI.Schema.PhysicalAsset.CapabilityTest;
using PhysicalAssetDataHandler.Client.Models.Dtos.History;

namespace FrameworkAPI.Schema.PhysicalAsset.History;

/// <summary>
/// A history item for a detected surface anomaly from start to end position of the physical asset.
/// </summary>
/// <param name="physicalAssetSurfaceAnomalyHistoryItemDto">The physical asset surface anomaly history item dto.</param>
public class PhysicalAssetSurfaceAnomalyHistoryItem(
    PhysicalAssetSurfaceAnomalyHistoryItemDto physicalAssetSurfaceAnomalyHistoryItemDto) : PhysicalAssetHistoryItem(physicalAssetSurfaceAnomalyHistoryItemDto)
{
    /// <summary>
    /// The start position on the physical asset.
    /// [Source: PhysicalAssetHistory]
    /// </summary>
    public double StartPosition { get; set; } = physicalAssetSurfaceAnomalyHistoryItemDto.StartPosition;

    /// <summary>
    /// The end position on the physical asset.
    /// [Source: PhysicalAssetHistory]
    /// </summary>
    public double EndPosition { get; set; } = physicalAssetSurfaceAnomalyHistoryItemDto.EndPosition;

    /// <summary>
    /// The unit of the set and measured value.
    /// [Source: PhysicalAssetHistory]
    /// </summary>
    public string Unit { get; set; } = physicalAssetSurfaceAnomalyHistoryItemDto.Unit;

    /// <summary>
    /// The specification used to check the test result.
    /// [Source: PhysicalAssetHistory]
    /// </summary>
    public AniloxCapabilityTestSpecification AniloxCapabilityTestSpecification { get; set; } = new(physicalAssetSurfaceAnomalyHistoryItemDto.AniloxCapabilityTestSpecificationDto);
}