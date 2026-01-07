using System;
using FrameworkAPI.Schema.PhysicalAsset.CapabilityTest;
using PhysicalAssetDataHandler.Client.Models.Dtos.History;

namespace FrameworkAPI.Schema.PhysicalAsset.History;

/// <summary>
/// A history item for a detected high volume of the physical asset.
/// </summary>
/// <param name="physicalAssetHighVolumeHistoryItemDto">The physical asset high volume history item dto.</param>
public class PhysicalAssetHighVolumeHistoryItem(PhysicalAssetHighVolumeHistoryItemDto physicalAssetHighVolumeHistoryItemDto) : PhysicalAssetHistoryItem(physicalAssetHighVolumeHistoryItemDto)
{
    /// <summary>
    /// Set value when the high volume was detected.
    /// [Source: PhysicalAssetHistory]
    /// </summary>
    public double SetValue { get; set; } = physicalAssetHighVolumeHistoryItemDto.SetValue;

    /// <summary>
    /// Measured value when the high volume was detected.
    /// [Source: PhysicalAssetHistory]
    /// </summary>
    public double MeasuredValue { get; set; } = physicalAssetHighVolumeHistoryItemDto.MeasuredValue;

    /// <summary>
    /// Upper limit value the measured volume is not allowed to be above.
    /// [Source: PhysicalAssetHistory]
    /// </summary>
    public double? UpperLimitValue { get; set; } = physicalAssetHighVolumeHistoryItemDto.UpperLimitValue ?? throw new ArgumentNullException(nameof(physicalAssetHighVolumeHistoryItemDto.UpperLimitValue));

    /// <summary>
    /// Lower limit value the measured volume is not allowed to be below.
    /// [Source: PhysicalAssetHistory]
    /// </summary>
    public double LowerLimitValue { get; set; } = physicalAssetHighVolumeHistoryItemDto.LowerLimitValue;

    /// <summary>
    /// The unit of the set and measured value.
    /// [Source: PhysicalAssetHistory]
    /// </summary>
    public string Unit { get; set; } = physicalAssetHighVolumeHistoryItemDto.Unit;

    /// <summary>
    /// The specification used to check the test result.
    /// [Source: PhysicalAssetHistory]
    /// </summary>
    public VolumeCapabilityTestSpecification VolumeCapabilityTestSpecification { get; set; } = new(physicalAssetHighVolumeHistoryItemDto.VolumeCapabilityTestSpecificationDto);
}