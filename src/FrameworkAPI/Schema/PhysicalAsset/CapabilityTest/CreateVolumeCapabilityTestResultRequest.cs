using System;

namespace FrameworkAPI.Schema.PhysicalAsset.CapabilityTest;

/// <summary>
/// A request to create a physical asset test result.
/// </summary>
public class CreateVolumeCapabilityTestResultRequest : CreateCapabilityTestResultRequest
{
    public CreateVolumeCapabilityTestResultRequest(
        string physicalAssetId,
        DateTime testDateTime,
        string? note,
        double volume) : base(physicalAssetId, testDateTime, note)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(volume);

        Volume = volume;
    }

    /// <summary>
    /// The volume measured by the tester.
    /// </summary>
    public double Volume { get; set; }
}