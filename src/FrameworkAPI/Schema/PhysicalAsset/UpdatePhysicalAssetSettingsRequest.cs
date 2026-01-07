using System;

namespace FrameworkAPI.Schema.PhysicalAsset;

/// <summary>
/// A request to update a physical asset settings.
/// </summary>
public class UpdatePhysicalAssetSettingsRequest
{
    public UpdatePhysicalAssetSettingsRequest(
        int aniloxCleaningIntervalInMeter)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(aniloxCleaningIntervalInMeter);

        AniloxCleaningIntervalInMeter = aniloxCleaningIntervalInMeter;
    }

    /// <summary>
    /// The interval the physical asset should get cleaned.
    /// </summary>
    public int AniloxCleaningIntervalInMeter { get; set; }
}