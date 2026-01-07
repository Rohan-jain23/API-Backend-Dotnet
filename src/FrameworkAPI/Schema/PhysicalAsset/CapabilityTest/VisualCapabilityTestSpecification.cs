using PhysicalAssetDataHandler.Client.Models.Dtos.CapabilityTest;

namespace FrameworkAPI.Schema.PhysicalAsset.CapabilityTest;

/// <summary>
/// A visual capability test specification.
/// </summary>
/// <param name="visualCapabilityTestSpecificationDto">The visual capability test specification dto.</param>
public class VisualCapabilityTestSpecification(
    CapabilityTestSpecificationDto visualCapabilityTestSpecificationDto) : CapabilityTestSpecification(visualCapabilityTestSpecificationDto)
{
}