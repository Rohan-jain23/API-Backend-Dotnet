using FrameworkAPI.Schema.PhysicalAsset.CapabilityTest;
using PhysicalAssetDataHandler.Client.Models.Dtos.Defect;

namespace FrameworkAPI.Schema.PhysicalAsset.Defect;

/// <summary>
/// A defect for a detected a volume triggered print anomaly from start to end position of the physical asset.
/// </summary>
/// <param name="physicalAssetVolumeTriggeredPrintAnomalyDefectDto">The physical asset volume triggered print anomaly defect dto.</param>
public class PhysicalAssetVolumeTriggeredPrintAnomalyDefect(
    PhysicalAssetVolumeTriggeredPrintAnomalyDefectDto physicalAssetVolumeTriggeredPrintAnomalyDefectDto)
    : PhysicalAssetDefect(physicalAssetVolumeTriggeredPrintAnomalyDefectDto)
{
    /// <summary>
    /// The start position on the physical asset.
    /// [Source: PhysicalAssetDefect]
    /// </summary>
    public double StartPosition { get; set; } = physicalAssetVolumeTriggeredPrintAnomalyDefectDto.StartPosition;

    /// <summary>
    /// The end position on the physical asset.
    /// [Source: PhysicalAssetDefect]
    /// </summary>
    public double EndPosition { get; set; } = physicalAssetVolumeTriggeredPrintAnomalyDefectDto.EndPosition;

    /// <summary>
    /// The unit of the set and measured value.
    /// [Source: PhysicalAssetDefect]
    /// </summary>
    public string Unit { get; set; } = physicalAssetVolumeTriggeredPrintAnomalyDefectDto.Unit;

    /// <summary>
    /// The specification used to check the test result.
    /// [Source: PhysicalAssetHistory]
    /// </summary>
    public AniloxCapabilityTestSpecification AniloxCapabilityTestSpecification { get; set; } = new(physicalAssetVolumeTriggeredPrintAnomalyDefectDto.AniloxCapabilityTestSpecificationDto);
}