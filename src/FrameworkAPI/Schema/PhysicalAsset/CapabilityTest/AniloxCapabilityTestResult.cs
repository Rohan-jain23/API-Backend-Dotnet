using PhysicalAssetDataHandler.Client.Models.Dtos.CapabilityTest;
using PhysicalAssetDataHandler.Client.Models.Enums;

namespace FrameworkAPI.Schema.PhysicalAsset.CapabilityTest;

/// <summary>
/// An anilox capability test result.
/// </summary>
public class AniloxCapabilityTestResult(AniloxCapabilityTestResultDto aniloxCapabilityTestResultDto)
    : CapabilityTestResult(aniloxCapabilityTestResultDto)
{
    /// <summary>
    /// The type of the anilox error.
    /// [Source: AniloxCapabilityTestResult]
    /// </summary>
    public AniloxCapabilityErrorType AniloxCapabilityErrorType { get; set; } =
        aniloxCapabilityTestResultDto.AniloxCapabilityErrorType;

    /// <summary>
    /// The start position on the anilox.
    /// [Source: AniloxCapabilityTestResult]
    /// </summary>
    public double StartPositionOnAnilox { get; set; } = aniloxCapabilityTestResultDto.StartPositionOnAnilox;

    /// <summary>
    /// The end position on the anilox.
    /// [Source: AniloxCapabilityTestResult]
    /// </summary>
    public double? EndPositionOnAnilox { get; set; } = aniloxCapabilityTestResultDto.EndPositionOnAnilox;
}