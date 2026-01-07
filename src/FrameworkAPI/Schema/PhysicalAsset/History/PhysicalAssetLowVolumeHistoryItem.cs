using FrameworkAPI.Schema.PhysicalAsset.CapabilityTest;
using PhysicalAssetDataHandler.Client.Models.Dtos.History;

namespace FrameworkAPI.Schema.PhysicalAsset.History;

/// <summary>
/// A history item for a detected low volume of the physical asset.
/// </summary>
/// <param name="physicalAssetLowVolumeHistoryItemDto">The physical asset low volume history item dto.</param>
public class PhysicalAssetLowVolumeHistoryItem(PhysicalAssetLowVolumeHistoryItemDto physicalAssetLowVolumeHistoryItemDto) : PhysicalAssetHistoryItem(physicalAssetLowVolumeHistoryItemDto)
{
    /// <summary>
    /// Set value when the low volume was detected.
    /// [Source: PhysicalAssetHistory]
    /// </summary>
    public double SetValue { get; set; } = physicalAssetLowVolumeHistoryItemDto.SetValue;

    /// <summary>
    /// Measured value when the low volume was detected.
    /// [Source: PhysicalAssetHistory]
    /// </summary>
    public double MeasuredValue { get; set; } = physicalAssetLowVolumeHistoryItemDto.MeasuredValue;

    /// <summary>
    /// Upper limit value the measured volume is not allowed to be above.
    /// [Source: PhysicalAssetHistory]
    /// </summary>
    public double? UpperLimitValue { get; set; } = physicalAssetLowVolumeHistoryItemDto.UpperLimitValue;

    /// <summary>
    /// Lower limit value the measured volume is not allowed to be below.
    /// [Source: PhysicalAssetHistory]
    /// </summary>
    public double LowerLimitValue { get; set; } = physicalAssetLowVolumeHistoryItemDto.LowerLimitValue;

    /// <summary>
    /// The unit of the set and measured value.
    /// [Source: PhysicalAssetHistory]
    /// </summary>
    public string Unit { get; set; } = physicalAssetLowVolumeHistoryItemDto.Unit;

    /// <summary>
    /// The specification used to check the test result.
    /// [Source: PhysicalAssetHistory]
    /// </summary>
    public VolumeCapabilityTestSpecification VolumeCapabilityTestSpecification { get; set; } = new(physicalAssetLowVolumeHistoryItemDto.VolumeCapabilityTestSpecificationDto);
}