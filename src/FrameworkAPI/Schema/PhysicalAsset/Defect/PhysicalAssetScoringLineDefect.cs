using FrameworkAPI.Schema.PhysicalAsset.CapabilityTest;
using PhysicalAssetDataHandler.Client.Models.Dtos.Defect;

namespace FrameworkAPI.Schema.PhysicalAsset.Defect;

/// <summary>
/// A defect for a detected scoring line on an position of the physical asset.
/// </summary>
/// <param name="physicalAssetScoringLineDefectDto">The physical asset scoring line defect dto.</param>
public class PhysicalAssetScoringLineDefect(PhysicalAssetScoringLineDefectDto physicalAssetScoringLineDefectDto)
    : PhysicalAssetDefect(physicalAssetScoringLineDefectDto)
{
    /// <summary>
    /// The position on the physical asset.
    /// [Source: PhysicalAssetDefect]
    /// </summary>
    public double Position { get; set; } = physicalAssetScoringLineDefectDto.Position;

    /// <summary>
    /// The unit of the set and measured value.
    /// [Source: PhysicalAssetDefect]
    /// </summary>
    public string Unit { get; set; } = physicalAssetScoringLineDefectDto.Unit;

    /// <summary>
    /// The specification used to check the test result.
    /// [Source: PhysicalAssetHistory]
    /// </summary>
    public AniloxCapabilityTestSpecification AniloxCapabilityTestSpecification { get; set; } = new(physicalAssetScoringLineDefectDto.AniloxCapabilityTestSpecificationDto);
}