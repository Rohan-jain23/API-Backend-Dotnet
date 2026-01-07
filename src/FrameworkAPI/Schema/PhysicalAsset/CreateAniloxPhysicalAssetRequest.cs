using System;
using System.Collections.Generic;

namespace FrameworkAPI.Schema.PhysicalAsset;

/// <summary>
/// A request to create a physical asset of type anilox.
/// </summary>
public class CreateAniloxPhysicalAssetRequest : AniloxPhysicalAssetRequest
{
    public CreateAniloxPhysicalAssetRequest(
        string serialNumber,
        string manufacturer,
        string? description,
        DateTime? deliveredAt,
        string? preferredUsageLocation,
        long? initialUsageCounter,
        long? initialTimeUsageCounter,
        IEnumerable<string> scanCodes,
        double printWidth,
        bool isSleeve,
        double? innerDiameter,
        double outerDiameter,
        int screen,
        string? engraving,
        double setVolumeValue,
        double? setOpticalDensityValue,
        double? measuredVolumeValue)
        : base(
            serialNumber,
            manufacturer,
            description,
            deliveredAt,
            preferredUsageLocation,
            initialUsageCounter,
            initialTimeUsageCounter,
            scanCodes,
            printWidth,
            isSleeve,
            innerDiameter,
            outerDiameter,
            screen,
            engraving,
            setVolumeValue,
            setOpticalDensityValue)
    {
        if (measuredVolumeValue is not null)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(measuredVolumeValue.Value);
        }

        MeasuredVolumeValue = measuredVolumeValue;
    }

    /// <summary>
    /// The last measured volume of the physical asset to create.
    /// </summary>
    public double? MeasuredVolumeValue { get; set; }
}