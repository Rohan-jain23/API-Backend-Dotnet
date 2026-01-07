using System;

namespace FrameworkAPI.Schema.PhysicalAsset.Operation;

/// <summary>
/// A request to create a anilox physical asset refurbishing operation.
/// </summary>
public class CreateRefurbishingAniloxOperationRequest : CreateOperationRequest
{
    public CreateRefurbishingAniloxOperationRequest(
        string physicalAssetId,
        DateTime refurbishedDateTime,
        string? note,
        string? serialNumberOverwrite,
        string? manufacturerOverwrite,
        int screen,
        string? engraving,
        double setVolumeValue,
        double? measuredVolumeValue) : base(physicalAssetId, note)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(screen);
        ArgumentOutOfRangeException.ThrowIfNegative(setVolumeValue);

        if (measuredVolumeValue is not null)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(measuredVolumeValue.Value);
        }

        RefurbishedDateTime = refurbishedDateTime;
        SerialNumberOverwrite = serialNumberOverwrite;
        ManufacturerOverwrite = manufacturerOverwrite;
        Screen = screen;
        Engraving = engraving;
        SetVolumeValue = setVolumeValue;
        MeasuredVolumeValue = measuredVolumeValue;
    }

    /// <summary>
    /// Date the physical asset is refurbished.
    /// </summary>
    public DateTime RefurbishedDateTime { get; set; }

    /// <summary>
    /// Optional overwrite for the current serial number.
    /// </summary>
    public string? SerialNumberOverwrite { get; set; }

    /// <summary>
    /// Optional overwrite for the current manufacturer.
    /// </summary>
    public string? ManufacturerOverwrite { get; set; }

    /// <summary>
    /// Screen of the refurbished anilox physical asset.
    /// </summary>
    public int Screen { get; set; }

    /// <summary>
    /// Engraving of the refurbished anilox physical asset.
    /// </summary>
    public string? Engraving { get; set; }

    /// <summary>
    /// Set volume value of the refurbished anilox physical asset.
    /// </summary>
    public double SetVolumeValue { get; set; }

    /// <summary>
    /// The measured volume of the refurbished anilox physical asset.
    /// </summary>
    public double? MeasuredVolumeValue { get; set; }
}