using System;
using HotChocolate.Types;
using PhysicalAssetDataHandler.Client.Models.Dtos.CapabilityTest;
using PhysicalAssetDataHandler.Client.Models.Enums;

namespace FrameworkAPI.Schema.PhysicalAsset.CapabilityTest;

/// <summary>
/// Generic interface for capability test specifications.
/// </summary>
/// <param name="capabilityTestSpecificationDto">The capability test specification dto.</param>
[InterfaceType]
public abstract class CapabilityTestSpecification(CapabilityTestSpecificationDto capabilityTestSpecificationDto)
{

    /// <summary>
    /// Type of the capability test specification.
    /// [Source: CapabilityTestSpecification]
    /// </summary>
    public CapabilityTestType CapabilityTestType { get; set; } = capabilityTestSpecificationDto.CapabilityTestType;

    /// <summary>
    /// Unique identifier of the capability test specification.
    /// [Source: CapabilityTestSpecification]
    /// </summary>
    public string CapabilityTestSpecificationId { get; set; } = capabilityTestSpecificationDto.CapabilityTestSpecificationId;

    /// <summary>
    /// Version of the capability test specification.
    /// [Source: CapabilityTestSpecification]
    /// </summary>
    public int Version { get; set; } = capabilityTestSpecificationDto.Version;

    /// <summary>
    /// Description of the capability test specification.
    /// [Source: CapabilityTestSpecification]
    /// </summary>
    public string? Description { get; set; } = capabilityTestSpecificationDto.Description;

    /// <summary>
    /// Date of the creation of the capability test specification.
    /// [Source: CapabilityTestSpecification]
    /// </summary>
    public DateTime CreatedAt { get; set; } = capabilityTestSpecificationDto.CreatedAt;

    internal static CapabilityTestSpecification CreateInstance(
        CapabilityTestSpecificationDto capabilityTestSpecificationDto)
    {
        return capabilityTestSpecificationDto.CapabilityTestType switch
        {
            CapabilityTestType.Volume => new VolumeCapabilityTestSpecification(
                (VolumeCapabilityTestSpecificationDto)capabilityTestSpecificationDto),
            CapabilityTestType.OpticalDensity => new OpticalDensityCapabilityTestSpecification(
                (OpticalDensityCapabilityTestSpecificationDto)capabilityTestSpecificationDto),
            CapabilityTestType.Anilox => new AniloxCapabilityTestSpecification(
                (AniloxCapabilityTestSpecificationDto)capabilityTestSpecificationDto),
            CapabilityTestType.Visual => new VisualCapabilityTestSpecification(
                (VisualCapabilityTestSpecificationDto)capabilityTestSpecificationDto),
            _ => throw new ArgumentException(
                $"Creating a capability test specification is not supported for the type '{capabilityTestSpecificationDto.CapabilityTestType}'.")
        };
    }
}