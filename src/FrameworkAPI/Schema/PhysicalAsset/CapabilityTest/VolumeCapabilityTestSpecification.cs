using PhysicalAssetDataHandler.Client.Models.Dtos.CapabilityTest;

namespace FrameworkAPI.Schema.PhysicalAsset.CapabilityTest;

/// <summary>
/// An volume capability test specification.
/// </summary>
/// <param name="volumeCapabilityTestSpecificationDto">The volume capability test specification dto.</param>
public class VolumeCapabilityTestSpecification(
    VolumeCapabilityTestSpecificationDto volumeCapabilityTestSpecificationDto) : CapabilityTestSpecification(volumeCapabilityTestSpecificationDto)
{
    /// <summary>
    /// The flag which signals if the deviation values are relative.
    /// [Source: VolumeCapabilityTestSpecification]
    /// </summary>
    public bool IsRelative { get; set; } = volumeCapabilityTestSpecificationDto.IsRelative;

    /// <summary>
    /// The upper deviation limit of the measured volume.
    /// [Source: VolumeCapabilityTestSpecification]
    /// </summary>
    public double? VolumeDeviationUpperLimit { get; set; } = volumeCapabilityTestSpecificationDto.VolumeDeviationUpperLimit;

    /// <summary>
    /// The lower deviation limit of the measured volume.
    /// [Source: VolumeCapabilityTestSpecification]
    /// </summary>
    public double? VolumeDeviationLowerLimit { get; set; } = volumeCapabilityTestSpecificationDto.VolumeDeviationLowerLimit;
}