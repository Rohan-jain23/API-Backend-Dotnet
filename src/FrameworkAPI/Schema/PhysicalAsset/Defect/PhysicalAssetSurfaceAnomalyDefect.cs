using FrameworkAPI.Schema.PhysicalAsset.CapabilityTest;
using PhysicalAssetDataHandler.Client.Models.Dtos.Defect;

namespace FrameworkAPI.Schema.PhysicalAsset.Defect;

/// <summary>
/// A defect for a detected a surface anomaly from start to end position of the physical asset.
/// </summary>
/// <param name="physicalAssetSurfaceAnomalyDefectDto">The physical asset surface anomaly defect dto.</param>
public class PhysicalAssetSurfaceAnomalyDefect(
    PhysicalAssetSurfaceAnomalyDefectDto physicalAssetSurfaceAnomalyDefectDto)
    : PhysicalAssetDefect(physicalAssetSurfaceAnomalyDefectDto)
{
    /// <summary>
    /// The start position on the physical asset.
    /// [Source: PhysicalAssetDefect]
    /// </summary>
    public double StartPosition { get; set; } = physicalAssetSurfaceAnomalyDefectDto.StartPosition;

    /// <summary>
    /// The end position on the physical asset.
    /// [Source: PhysicalAssetDefect]
    /// </summary>
    public double EndPosition { get; set; } = physicalAssetSurfaceAnomalyDefectDto.EndPosition;

    /// <summary>
    /// The unit of the set and measured value.
    /// [Source: PhysicalAssetDefect]
    /// </summary>
    public string Unit { get; set; } = physicalAssetSurfaceAnomalyDefectDto.Unit;

    /// <summary>
    /// The specification used to check the test result.
    /// [Source: PhysicalAssetHistory]
    /// </summary>
    public AniloxCapabilityTestSpecification AniloxCapabilityTestSpecification { get; set; } = new(physicalAssetSurfaceAnomalyDefectDto.AniloxCapabilityTestSpecificationDto);
}