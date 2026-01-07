using PhysicalAssetDataHandler.Client.Models.Dtos;

namespace FrameworkAPI.Schema.PhysicalAsset;

/// <summary>
/// A point on the surface of the plate physical asset.
/// </summary>
/// <param name="plateSurfacePointDto">The plate surface point dto.</param>
public class PlateSurfacePoint(PlateSurfacePointDto plateSurfacePointDto)
{

    /// <summary>
    /// X coordinate of a point on the surface of the plate physical asset.
    /// [Source: PlatePhysicalAsset]
    /// </summary>
    public int X { get; set; } = plateSurfacePointDto.X;

    /// <summary>
    /// Y coordinate of a point on the surface of the plate physical asset.
    /// [Source: PlatePhysicalAsset]
    /// </summary>
    public int Y { get; set; } = plateSurfacePointDto.Y;

    /// <summary>
    /// Measured value of a point on the surface of the plate physical asset.
    /// [Source: PlatePhysicalAsset]
    /// </summary>
    public double Value { get; set; } = plateSurfacePointDto.Value;
}