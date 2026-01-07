using System;
using System.Collections.Generic;

namespace FrameworkAPI.Schema.PhysicalAsset;

/// <summary>
/// A request to update a physical asset of type anilox. Always send all properties, no partial updates are supported.
/// </summary>
public class UpdateAniloxPhysicalAssetRequest : AniloxPhysicalAssetRequest
{
    public UpdateAniloxPhysicalAssetRequest(
        string physicalAssetId,
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
        double? setOpticalDensityValue)
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
        ArgumentException.ThrowIfNullOrWhiteSpace(physicalAssetId);

        PhysicalAssetId = physicalAssetId;
    }

    /// <summary>
    /// Id of the physical asset to update.
    /// </summary>
    public string PhysicalAssetId { get; set; }
}