using FrameworkAPI.Schema.PhysicalAsset.CapabilityTest;
using PhysicalAssetDataHandler.Client.Models.Dtos.Defect;

namespace FrameworkAPI.Schema.PhysicalAsset.Defect;

/// <summary>
/// A history item for a detected low volume of the physical asset.
/// </summary>
/// <param name="physicalAssetLowVolumeDefectDto">The physical asset low volume defect dto.</param>
public class PhysicalAssetLowVolumeDefect(PhysicalAssetLowVolumeDefectDto physicalAssetLowVolumeDefectDto) : PhysicalAssetDefect(physicalAssetLowVolumeDefectDto)
{
    /// <summary>
    /// Set value when the low volume was detected.
    /// [Source: PhysicalAssetDefect]
    /// </summary>
    public double SetValue { get; set; } = physicalAssetLowVolumeDefectDto.SetValue;

    /// <summary>
    /// Measured value when the low volume was detected.
    /// [Source: PhysicalAssetDefect]
    /// </summary>
    public double MeasuredValue { get; set; } = physicalAssetLowVolumeDefectDto.MeasuredValue;

    /// <summary>
    /// Upper limit value the measured volume is not allowed to be above.
    /// [Source: PhysicalAssetHistory]
    /// </summary>
    public double? UpperLimitValue { get; set; } = physicalAssetLowVolumeDefectDto.UpperLimitValue;

    /// <summary>
    /// Lower limit value the measured volume is not allowed to be below.
    /// [Source: PhysicalAssetHistory]
    /// </summary>
    public double LowerLimitValue { get; set; } = physicalAssetLowVolumeDefectDto.LowerLimitValue;

    /// <summary>
    /// The unit of the set and measured value.
    /// [Source: PhysicalAssetDefect]
    /// </summary>
    public string Unit { get; set; } = physicalAssetLowVolumeDefectDto.Unit;

    /// <summary>
    /// The specification used to check the test result.
    /// [Source: PhysicalAssetHistory]
    /// </summary>
    public VolumeCapabilityTestSpecification VolumeCapabilityTestSpecification { get; set; } = new(physicalAssetLowVolumeDefectDto.VolumeCapabilityTestSpecificationDto);
}