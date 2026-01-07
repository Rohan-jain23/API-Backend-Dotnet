using System;
using HotChocolate.Types;
using PhysicalAssetDataHandler.Client.Models.Dtos.Defect;
using PhysicalAssetDataHandler.Client.Models.Enums;

namespace FrameworkAPI.Schema.PhysicalAsset.Defect;

/// <summary>
/// Generic interface for physical asset defects.
/// </summary>
/// <param name="physicalAssetDefectDto">The physical asset defect dto.</param>
[InterfaceType]
public abstract class PhysicalAssetDefect(PhysicalAssetDefectDto physicalAssetDefectDto)
{

    /// <summary>
    /// Type of the physical asset defect.
    /// [Source: PhysicalAssetDefect]
    /// </summary>
    public PhysicalAssetDefectType PhysicalAssetDefectType { get; set; } = physicalAssetDefectDto.PhysicalAssetDefectType;

    /// <summary>
    /// Generated unique id of the source resulting into the defect.
    /// [Source: PhysicalAssetDefect]
    /// </summary>
    public string SourceId { get; set; } = physicalAssetDefectDto.SourceId;

    /// <summary>
    /// Date on which the defect event occurred (is within the lifetime of the physical asset).
    /// [Source: PhysicalAssetDefect]
    /// </summary>
    public DateTime CreatedAt { get; set; } = physicalAssetDefectDto.CreatedAt;

    /// <summary>
    /// The additional note added by the user.
    /// [Source: PhysicalAssetDefect]
    /// </summary>
    public string? Note { get; set; } = physicalAssetDefectDto.Note;

    internal static PhysicalAssetDefect CreateInstance(PhysicalAssetDefectDto physicalAssetDefectDto)
    {
        return physicalAssetDefectDto.PhysicalAssetDefectType switch
        {
            PhysicalAssetDefectType.LowVolume => new PhysicalAssetLowVolumeDefect(
                (PhysicalAssetLowVolumeDefectDto)physicalAssetDefectDto),
            PhysicalAssetDefectType.HighVolume => new PhysicalAssetHighVolumeDefect(
                (PhysicalAssetHighVolumeDefectDto)physicalAssetDefectDto),
            PhysicalAssetDefectType.ScoringLine => new PhysicalAssetScoringLineDefect(
                (PhysicalAssetScoringLineDefectDto)physicalAssetDefectDto),
            PhysicalAssetDefectType.SurfaceAnomaly => new PhysicalAssetSurfaceAnomalyDefect(
                (PhysicalAssetSurfaceAnomalyDefectDto)physicalAssetDefectDto),
            PhysicalAssetDefectType.VolumeTriggeredPrintAnomaly => new PhysicalAssetVolumeTriggeredPrintAnomalyDefect(
                (PhysicalAssetVolumeTriggeredPrintAnomalyDefectDto)physicalAssetDefectDto),
            _ => throw new ArgumentException(
                $"Creating a physical asset defect is not supported for the type '{physicalAssetDefectDto.PhysicalAssetDefectType}'.")
        };
    }
}