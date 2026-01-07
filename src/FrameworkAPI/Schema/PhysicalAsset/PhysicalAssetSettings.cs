using FrameworkAPI.Extensions;
using FrameworkAPI.Schema.Misc;
using PhysicalAssetDataHandler.Client.Models.Dtos;

namespace FrameworkAPI.Schema.PhysicalAsset;

public class PhysicalAssetSettings(PhysicalAssetSettingsDto physicalAssetSettingsDto)
{
    /// <summary>
    /// The interval the physical asset should get cleaned.
    /// [Source: GlobalSettings]
    /// </summary>
    public ValueWithUnit<int> CleaningInterval { get; set; } = physicalAssetSettingsDto.AniloxCleaningInterval.ToSchema();
}