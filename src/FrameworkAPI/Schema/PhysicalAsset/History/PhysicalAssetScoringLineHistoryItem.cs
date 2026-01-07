using FrameworkAPI.Schema.PhysicalAsset.CapabilityTest;
using PhysicalAssetDataHandler.Client.Models.Dtos.History;

namespace FrameworkAPI.Schema.PhysicalAsset.History;

/// <summary>
/// A history item for a detected scoring line on an position of the physical asset.
/// </summary>
/// <param name="physicalAssetScoringLineDefectDto">The physical asset scoring line point history item dto.</param>
public class PhysicalAssetScoringLineHistoryItem(
    PhysicalAssetScoringLineHistoryItemDto physicalAssetScoringLineDefectDto) : PhysicalAssetHistoryItem(physicalAssetScoringLineDefectDto)
{
    /// <summary>
    /// The position on the physical asset.
    /// [Source: PhysicalAssetHistory]
    /// </summary>
    public double Position { get; set; } = physicalAssetScoringLineDefectDto.Position;

    /// <summary>
    /// The unit of the set and measured value.
    /// [Source: PhysicalAssetHistory]
    /// </summary>
    public string Unit { get; set; } = physicalAssetScoringLineDefectDto.Unit;

    /// <summary>
    /// The specification used to check the test result.
    /// [Source: PhysicalAssetHistory]
    /// </summary>
    public AniloxCapabilityTestSpecification AniloxCapabilityTestSpecification { get; set; } = new(physicalAssetScoringLineDefectDto.AniloxCapabilityTestSpecificationDto);
}