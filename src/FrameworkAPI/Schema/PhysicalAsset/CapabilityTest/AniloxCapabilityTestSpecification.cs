using PhysicalAssetDataHandler.Client.Models.Dtos.CapabilityTest;

namespace FrameworkAPI.Schema.PhysicalAsset.CapabilityTest;

/// <summary>
/// An anilox capability test specification.
/// </summary>
/// <param name="aniloxCapabilityTestSpecificationDto">The anilox capability test specification dto.</param>
public class AniloxCapabilityTestSpecification(
    AniloxCapabilityTestSpecificationDto aniloxCapabilityTestSpecificationDto) : CapabilityTestSpecification(aniloxCapabilityTestSpecificationDto)
{
    /// <summary>
    /// The flag which signals if errors are always passing regardless of the test result.
    /// [Source: AniloxCapabilityTestSpecification]
    /// </summary>
    public bool AlwaysPass { get; set; } = aniloxCapabilityTestSpecificationDto.AlwaysPass;
}