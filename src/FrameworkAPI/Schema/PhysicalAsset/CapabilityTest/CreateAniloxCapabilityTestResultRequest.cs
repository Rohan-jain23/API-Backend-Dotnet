using System;
using PhysicalAssetDataHandler.Client.Models.Enums;

namespace FrameworkAPI.Schema.PhysicalAsset.CapabilityTest;

/// <summary>
/// A request to create an anilox capability test result.
/// </summary>
public class CreateAniloxCapabilityTestResultRequest : CreateCapabilityTestResultRequest
{
    public CreateAniloxCapabilityTestResultRequest(
        string physicalAssetId,
        DateTime testDateTime,
        string? note,
        AniloxCapabilityErrorType aniloxCapabilityErrorType,
        double startPositionOnAnilox,
        double? endPositionOnAnilox) : base(physicalAssetId, testDateTime, note)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(startPositionOnAnilox);

        if (endPositionOnAnilox.HasValue)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(endPositionOnAnilox.Value);

            if (startPositionOnAnilox > endPositionOnAnilox.Value)
            {

                throw new ArgumentOutOfRangeException(nameof(startPositionOnAnilox),
                    $"{nameof(startPositionOnAnilox)} '{startPositionOnAnilox}' has to be less than or equal to {nameof(endPositionOnAnilox)} '{endPositionOnAnilox}'");
            }
        }

        AniloxCapabilityErrorType = aniloxCapabilityErrorType;
        StartPositionOnAnilox = startPositionOnAnilox;
        EndPositionOnAnilox = endPositionOnAnilox;
    }

    /// <summary>
    /// The type of the anilox error.
    /// </summary>
    public AniloxCapabilityErrorType AniloxCapabilityErrorType { get; set; }

    /// <summary>
    /// The start position on the anilox.
    /// </summary>
    public double StartPositionOnAnilox { get; set; }

    /// <summary>
    /// The end position on the anilox.
    /// </summary>
    public double? EndPositionOnAnilox { get; set; }
}