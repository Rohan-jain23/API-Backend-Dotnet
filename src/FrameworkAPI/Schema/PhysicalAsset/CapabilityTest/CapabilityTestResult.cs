using System;
using HotChocolate.Types;
using PhysicalAssetDataHandler.Client.Models.Dtos.CapabilityTest;
using PhysicalAssetDataHandler.Client.Models.Enums;

namespace FrameworkAPI.Schema.PhysicalAsset.CapabilityTest;

/// <summary>
/// Generic interface for capability test results.
/// </summary>
/// <param name="capabilityTestResultDto">The capability test result dto.</param>
[InterfaceType]
public abstract class CapabilityTestResult(CapabilityTestResultDto capabilityTestResultDto)
{

    /// <summary>
    /// Type of the capability test result.
    /// [Source: CapabilityTestResult]
    /// </summary>
    public readonly CapabilityTestType CapabilityTestType = capabilityTestResultDto.CapabilityTestType;

    /// <summary>
    /// Unique identifier of the capability test result.
    /// [Source: CapabilityTestResult]
    /// </summary>
    public string CapabilityTestResultId { get; set; } = capabilityTestResultDto.CapabilityTestResultId;

    /// <summary>
    /// Unique identifier of the used capability test specification.
    /// [Source: CapabilityTestResult]
    /// </summary>
    public string CapabilityTestSpecificationId { get; set; } = capabilityTestResultDto.CapabilityTestSpecificationId;

    /// <summary>
    /// Unique identifier of the tested physical asset.
    /// [Source: CapabilityTestResult]
    /// </summary>
    public string PhysicalAssetId { get; set; } = capabilityTestResultDto.PhysicalAssetId;

    /// <summary>
    /// Date the physical asset was tested.
    /// [Source: CapabilityTestResult]
    /// </summary>
    public DateTime TestDateTime { get; set; } = capabilityTestResultDto.TestDateTime;

    /// <summary>
    /// Id of the user who tested the physical asset.
    /// [Source: CapabilityTestResult]
    /// </summary>
    public string TesterUserId { get; set; } = capabilityTestResultDto.TesterUserId;

    /// <summary>
    /// The additional note for this test result.
    /// [Source: CapabilityTestResult]
    /// </summary>
    public string? Note { get; set; } = capabilityTestResultDto.Note;

    /// <summary>
    /// Specifies whether the test passed when it was compared against the specification.
    /// [Source: CapabilityTestResult]
    /// </summary>
    public bool IsPassed { get; set; } = capabilityTestResultDto.IsPassed;
}