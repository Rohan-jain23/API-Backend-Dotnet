using System.Collections.Generic;
using System.Linq;
using PhysicalAssetDataHandler.Client.Models.Dtos;

namespace FrameworkAPI.Schema.PhysicalAsset;

/// <summary>
/// A plate physical asset.
/// </summary>
/// <param name="platePhysicalAssetDto">The plate physical asset dto.</param>
public class PlatePhysicalAsset(PlatePhysicalAssetDto platePhysicalAssetDto) : PhysicalAsset(platePhysicalAssetDto)
{

    /// <summary>
    /// Surface of the plate physical asset.
    /// [Source: PlatePhysicalAsset]
    /// </summary>
    public IEnumerable<PlateSurfacePoint>? Surface { get; set; } = platePhysicalAssetDto.Surface?.Select(plateSurfacePointDto =>
            new PlateSurfacePoint(plateSurfacePointDto));
}