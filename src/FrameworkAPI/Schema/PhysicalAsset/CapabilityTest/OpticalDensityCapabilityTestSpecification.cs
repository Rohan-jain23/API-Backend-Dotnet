using PhysicalAssetDataHandler.Client.Models.Dtos.CapabilityTest;

namespace FrameworkAPI.Schema.PhysicalAsset.CapabilityTest;

/// <summary>
/// An optical density capability test specification.
/// </summary>
/// <param name="opticalDensityCapabilityTestSpecificationDto">The optical density capability test specification dto.</param>
public class OpticalDensityCapabilityTestSpecification(
    OpticalDensityCapabilityTestSpecificationDto opticalDensityCapabilityTestSpecificationDto) : CapabilityTestSpecification(opticalDensityCapabilityTestSpecificationDto)
{

    /// <summary>
    /// The flag which signals if the deviation values are relative.
    /// [Source: OpticalDensityCapabilityTestSpecification]
    /// </summary>
    public bool IsRelative { get; set; } = opticalDensityCapabilityTestSpecificationDto.IsRelative;

    /// <summary>
    /// The upper deviation limit of the measured optical density.
    /// [Source: OpticalDensityCapabilityTestSpecification]
    /// </summary>
    public double? OpticalDensityDeviationUpperLimit { get; set; } =
            opticalDensityCapabilityTestSpecificationDto.OpticalDensityDeviationUpperLimit;

    /// <summary>
    /// The lower deviation limit of the measured optical density.
    /// [Source: OpticalDensityCapabilityTestSpecification]
    /// </summary>
    public double? OpticalDensityDeviationLowerLimit { get; set; } =
            opticalDensityCapabilityTestSpecificationDto.OpticalDensityDeviationLowerLimit;
}