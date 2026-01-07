using FrameworkAPI.Schema.PhysicalAsset.CapabilityTest;
using PhysicalAssetDataHandler.Client.Models.Dtos.History;

namespace FrameworkAPI.Schema.PhysicalAsset.History;

/// <summary>
/// A history item for an volume measurement of the physical asset.
/// </summary>
/// <param name="physicalAssetVolumeMeasuredHistoryItemDto">The physical asset high volume history item dto.</param>
public class PhysicalAssetVolumeMeasuredHistoryItem(PhysicalAssetVolumeMeasuredHistoryItemDto physicalAssetVolumeMeasuredHistoryItemDto) : PhysicalAssetHistoryItem(physicalAssetVolumeMeasuredHistoryItemDto)
{
    /// <summary>
    /// Set value when the volume was measured.
    /// [Source: PhysicalAssetHistory]
    /// </summary>
    public double SetValue { get; set; } = physicalAssetVolumeMeasuredHistoryItemDto.SetValue;

    /// <summary>
    /// Measured value when volume was measured.
    /// [Source: PhysicalAssetHistory]
    /// </summary>
    public double MeasuredValue { get; set; } = physicalAssetVolumeMeasuredHistoryItemDto.MeasuredValue;

    /// <summary>
    /// Upper limit value the measured volume is not allowed to be above.
    /// [Source: PhysicalAssetHistory]
    /// </summary>
    public double? UpperLimitValue { get; set; } = physicalAssetVolumeMeasuredHistoryItemDto.UpperLimitValue;

    /// <summary>
    /// Lower limit value the measured volume is not allowed to be below.
    /// [Source: PhysicalAssetHistory]
    /// </summary>
    public double LowerLimitValue { get; set; } = physicalAssetVolumeMeasuredHistoryItemDto.LowerLimitValue;

    /// <summary>
    /// The unit of the set and measured value.
    /// [Source: PhysicalAssetHistory]
    /// </summary>
    public string Unit { get; set; } = physicalAssetVolumeMeasuredHistoryItemDto.Unit;

    /// <summary>
    /// The specification used to check the test result.
    /// [Source: PhysicalAssetHistory]
    /// </summary>
    public VolumeCapabilityTestSpecification VolumeCapabilityTestSpecification { get; set; } = new(physicalAssetVolumeMeasuredHistoryItemDto.VolumeCapabilityTestSpecificationDto);
}