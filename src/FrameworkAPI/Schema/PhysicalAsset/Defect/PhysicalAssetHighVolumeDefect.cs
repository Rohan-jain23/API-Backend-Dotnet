using System;
using FrameworkAPI.Schema.PhysicalAsset.CapabilityTest;
using PhysicalAssetDataHandler.Client.Models.Dtos.Defect;

namespace FrameworkAPI.Schema.PhysicalAsset.Defect;

/// <summary>
/// A history item for a detected high volume of the physical asset.
/// </summary>
/// <param name="physicalAssetHighVolumeDefectDto">The physical asset high volume defect dto.</param>
public class PhysicalAssetHighVolumeDefect(PhysicalAssetHighVolumeDefectDto physicalAssetHighVolumeDefectDto) : PhysicalAssetDefect(physicalAssetHighVolumeDefectDto)
{
    /// <summary>
    /// Set value when the high volume was detected.
    /// [Source: PhysicalAssetDefect]
    /// </summary>
    public double SetValue { get; set; } = physicalAssetHighVolumeDefectDto.SetValue;

    /// <summary>
    /// Measured value when the high volume was detected.
    /// [Source: PhysicalAssetDefect]
    /// </summary>
    public double MeasuredValue { get; set; } = physicalAssetHighVolumeDefectDto.MeasuredValue;

    /// <summary>
    /// Upper limit value the measured volume is not allowed to be above.
    /// [Source: PhysicalAssetHistory]
    /// </summary>
    public double? UpperLimitValue { get; set; } = physicalAssetHighVolumeDefectDto.UpperLimitValue ?? throw new ArgumentNullException(nameof(physicalAssetHighVolumeDefectDto.UpperLimitValue));

    /// <summary>
    /// Lower limit value the measured volume is not allowed to be below.
    /// [Source: PhysicalAssetHistory]
    /// </summary>
    public double LowerLimitValue { get; set; } = physicalAssetHighVolumeDefectDto.LowerLimitValue;

    /// <summary>
    /// The unit of the set and measured value.
    /// [Source: PhysicalAssetDefect]
    /// </summary>
    public string Unit { get; set; } = physicalAssetHighVolumeDefectDto.Unit;

    /// <summary>
    /// The specification used to check the test result.
    /// [Source: PhysicalAssetHistory]
    /// </summary>
    public VolumeCapabilityTestSpecification VolumeCapabilityTestSpecification { get; set; } = new(physicalAssetHighVolumeDefectDto.VolumeCapabilityTestSpecificationDto);
}