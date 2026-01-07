using PhysicalAssetDataHandler.Client.Models.Dtos.CapabilityTest;

namespace FrameworkAPI.Schema.PhysicalAsset.CapabilityTest;

/// <summary>
/// A volume capability test result.
/// </summary>
public class VolumeCapabilityTestResult(VolumeCapabilityTestResultDto volumeCapabilityTestResultDto)
    : CapabilityTestResult(volumeCapabilityTestResultDto)
{
    /// <summary>
    /// The volume measured by the tester.
    /// [Source: VolumeCapabilityTestResult]
    /// </summary>
    public double Volume { get; set; } = volumeCapabilityTestResultDto.Volume;
}