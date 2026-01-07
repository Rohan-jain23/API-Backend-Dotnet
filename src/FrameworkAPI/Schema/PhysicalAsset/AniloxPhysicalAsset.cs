using FrameworkAPI.Extensions;
using FrameworkAPI.Schema.Misc;
using PhysicalAssetDataHandler.Client.Models.Dtos;

namespace FrameworkAPI.Schema.PhysicalAsset;

/// <summary>
/// An anilox physical asset.
/// </summary>
/// <param name="aniloxPhysicalAssetDto">The anilox physical asset dto.</param>
public class AniloxPhysicalAsset(AniloxPhysicalAssetDto aniloxPhysicalAssetDto) : PhysicalAsset(aniloxPhysicalAssetDto)
{

    /// <summary>
    /// Specifies whether the anilox physical asset is a sleeve or a roll.
    /// [Source: AniloxPhysicalAsset]
    /// </summary>
    public bool? IsSleeve { get; set; } = aniloxPhysicalAssetDto.IsSleeve;

    /// <summary>
    /// Print width of the anilox physical asset.
    /// [Source: AniloxPhysicalAsset]
    /// </summary>
    public ValueWithUnit<double> PrintWidth { get; set; } = aniloxPhysicalAssetDto.PrintWidth.ToSchema();

    /// <summary>
    /// Inner diameter of the anilox physical asset.
    /// [Source: AniloxPhysicalAsset]
    /// </summary>
    public ValueWithUnit<double>? InnerDiameter { get; set; } = aniloxPhysicalAssetDto.InnerDiameter?.ToSchema();

    /// <summary>
    /// Outer diameter of the anilox physical asset.
    /// [Source: AniloxPhysicalAsset]
    /// </summary>
    public ValueWithUnit<double> OuterDiameter { get; set; } = aniloxPhysicalAssetDto.OuterDiameter.ToSchema();

    /// <summary>
    /// Screen of the anilox physical asset.
    /// [Source: AniloxPhysicalAsset]
    /// </summary>
    public ValueWithUnit<int> Screen { get; set; } = aniloxPhysicalAssetDto.Screen.ToSchema();

    /// <summary>
    /// Engraving of the anilox physical asset.
    /// [Source: AniloxPhysicalAsset]
    /// </summary>
    public string? Engraving { get; set; } = aniloxPhysicalAssetDto.Engraving;

    /// <summary>
    /// Optical density of the anilox physical asset with last measure and set value to test against.
    /// [Source: AniloxPhysicalAsset]
    /// </summary>
    public TestableValueWithUnit<double> OpticalDensity { get; set; } = aniloxPhysicalAssetDto.OpticalDensity.ToSchema();

    /// <summary>
    /// Volume of the anilox physical asset with last measure and set value to test against.
    /// [Source: AniloxPhysicalAsset]
    /// </summary>
    public TestableValueWithUnit<double> Volume { get; set; } = aniloxPhysicalAssetDto.Volume.ToSchema();
}