using System;
using System.Collections.Generic;

namespace FrameworkAPI.Schema.PhysicalAsset;

public class AniloxPhysicalAssetRequest
{
    public AniloxPhysicalAssetRequest(
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
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(serialNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(manufacturer);

        if (initialUsageCounter is not null)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(initialUsageCounter.Value);
        }

        if (initialTimeUsageCounter is not null)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(initialTimeUsageCounter.Value);
        }

        ArgumentOutOfRangeException.ThrowIfNegative(printWidth);
        ArgumentOutOfRangeException.ThrowIfNegative(outerDiameter);
        ArgumentOutOfRangeException.ThrowIfNegative(screen);
        ArgumentOutOfRangeException.ThrowIfNegative(setVolumeValue);

        if (isSleeve)
        {
            ArgumentNullException.ThrowIfNull(innerDiameter);
            ArgumentOutOfRangeException.ThrowIfNegative(innerDiameter.Value);
        }

        if (setOpticalDensityValue is not null)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(setOpticalDensityValue.Value);
        }

        SerialNumber = serialNumber;
        Manufacturer = manufacturer;
        Description = description;
        DeliveredAt = deliveredAt;
        PreferredUsageLocation = preferredUsageLocation;
        InitialUsageCounter = initialUsageCounter;
        InitialTimeUsageCounter = initialTimeUsageCounter;
        ScanCodes = scanCodes;
        PrintWidth = printWidth;
        IsSleeve = isSleeve;
        InnerDiameter = innerDiameter;
        OuterDiameter = outerDiameter;
        Screen = screen;
        Engraving = engraving;
        SetVolumeValue = setVolumeValue;
        SetOpticalDensityValue = setOpticalDensityValue;
    }

    /// <summary>
    /// Mandatory Serial number of the physical asset to create.
    /// </summary>
    public string SerialNumber { get; set; }

    /// <summary>
    /// Manufacturer of the physical asset to create.
    /// </summary>
    public string Manufacturer { get; set; }

    /// <summary>
    /// Description of the physical asset to create.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Date of the delivery of the physical asset.
    /// </summary>
    public DateTime? DeliveredAt { get; set; }

    /// <summary>
    /// Preferred usage location of the physical asset to create.
    /// </summary>
    public string? PreferredUsageLocation { get; set; }

    /// <summary>
    /// Initial usage counter of the physical asset to create.
    /// </summary>
    public long? InitialUsageCounter { get; set; }

    /// <summary>
    /// Initial time usage counter of the physical asset to create.
    /// </summary>
    public long? InitialTimeUsageCounter { get; set; }

    /// <summary>
    /// List of unique scan codes (e.g. QR codes) identifying the physical asset.
    /// </summary>
    public IEnumerable<string> ScanCodes { get; set; }

    /// <summary>
    /// Flag which signals if the physical asset to create is a sleeve.
    /// </summary>
    public bool IsSleeve { get; set; }

    /// <summary>
    /// Print width of the physical asset to create.
    /// </summary>
    public double PrintWidth { get; set; }

    /// <summary>
    /// Inner diameter of the physical asset to create.
    /// </summary>
    public double? InnerDiameter { get; set; }

    /// <summary>
    /// Outer diameter of the physical asset to create.
    /// </summary>
    public double OuterDiameter { get; set; }

    /// <summary>
    /// Screen of the physical asset to create.
    /// </summary>
    public int Screen { get; set; }

    /// <summary>
    /// Engraving of the physical asset to create.
    /// </summary>
    public string? Engraving { get; set; }

    /// <summary>
    /// Set volume of the physical asset to create.
    /// </summary>
    public double SetVolumeValue { get; set; }

    /// <summary>
    /// Set optical density of the physical asset to create.
    /// </summary>
    public double? SetOpticalDensityValue { get; set; }
}