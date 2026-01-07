using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FrameworkAPI.DataLoaders;
using FrameworkAPI.Extensions;
using FrameworkAPI.Schema.PhysicalAsset.Defect;
using FrameworkAPI.Schema.PhysicalAsset.History;
using FrameworkAPI.Services.Interfaces;
using HotChocolate;
using HotChocolate.Types;
using PhysicalAssetDataHandler.Client.Models.Dtos;
using PhysicalAssetDataHandler.Client.Models.Enums;

namespace FrameworkAPI.Schema.PhysicalAsset;

/// <summary>
/// Generic interface for physical assets.
/// </summary>
/// <param name="physicalAssetDto">The physical asset dto.</param>
[InterfaceType]
public abstract class PhysicalAsset(PhysicalAssetDto physicalAssetDto)
{
    /// <summary>
    /// Type of the physical asset.
    /// [Source: PhysicalAsset]
    /// </summary>
    public PhysicalAssetType PhysicalAssetType { get; set; } = physicalAssetDto.PhysicalAssetType;

    /// <summary>
    /// Unique identifier of the physical asset.
    /// [Source: PhysicalAsset]
    /// </summary>
    public string PhysicalAssetId { get; set; } = physicalAssetDto.PhysicalAssetId;

    /// <summary>
    /// Date of the creation of the physical asset.
    /// [Source: PhysicalAsset]
    /// </summary>
    public DateTime CreatedAt { get; set; } = physicalAssetDto.CreatedAt;

    /// <summary>
    /// Date of the last change to the metadata, tests or operations of the physical asset.
    /// [Source: PhysicalAsset]
    /// </summary>
    public DateTime LastChange { get; set; } = physicalAssetDto.LastChange;

    /// <summary>
    /// Serial number of the physical asset.
    /// [Source: PhysicalAsset]
    /// </summary>
    public string SerialNumber { get; set; } = physicalAssetDto.SerialNumber;

    /// <summary>
    /// Manufacturer of the physical asset.
    /// [Source: PhysicalAsset]
    /// </summary>
    public string? Manufacturer { get; set; } = physicalAssetDto.Manufacturer;

    /// <summary>
    /// Description of the physical asset.
    /// [Source: PhysicalAsset]
    /// </summary>
    public string? Description { get; set; } = physicalAssetDto.Description;

    /// <summary>
    /// Date of the delivery of the physical asset.
    /// [Source: PhysicalAsset]
    /// </summary>
    public DateTime? DeliveredAt { get; set; } = physicalAssetDto.DeliveredAt;

    /// <summary>
    /// Preferred usage location (like: "EQ12345" or "MIRAFLEX AM Dualport") of the physical asset.
    /// [Source: PhysicalAsset]
    /// </summary>
    public string? PreferredUsageLocation { get; set; } = physicalAssetDto.PreferredUsageLocation;

    /// <summary>
    /// Initial usage counter of the physical asset (if already tracked before by other systems).
    /// [Source: PhysicalAsset]
    /// </summary>
    public long? InitialUsageCounter { get; set; } = physicalAssetDto.InitialUsageCounter;

    /// <summary>
    /// Initial time usage counter of the physical asset (if already tracked before by other systems).
    /// [Source: PhysicalAsset]
    /// </summary>
    public long? InitialTimeUsageCounter { get; set; } = physicalAssetDto.InitialTimeUsageCounter;

    /// <summary>
    /// List of unique scan codes (e.g. QR codes) identifying the physical asset.
    /// [Source: PhysicalAsset]
    /// </summary>
    public IEnumerable<string> ScanCodes { get; set; } = physicalAssetDto.ScanCodes;

    /// <summary>
    /// Tracked output that the physical asset has produced.
    /// [Source: PhysicalAsset]
    /// </summary>
    public PhysicalAssetUsageCounter UsageCounter { get; set; } = new(physicalAssetDto.UsageCounter);

    /// <summary>
    /// Tracked time for which the physical asset was used.
    /// [Source: PhysicalAsset]
    /// </summary>
    public PhysicalAssetTimeUsageCounter TimeUsageCounter { get; set; } = new(physicalAssetDto.TimeUsageCounter);

    /// <summary>
    /// Last type of cleaning when the physical asset was last cleaned.
    /// [Source: PhysicalAsset]
    /// </summary>
    public TrackedValue<CleaningOperationType>? LastCleaning { get; set; } = physicalAssetDto.LastCleaning?.ToSchema();

    /// <summary>
    /// Last consumed material when the physical asset was last used e.g. color.
    /// [Source: PhysicalAsset]
    /// </summary>
    public TrackedValue<string>? LastConsumedMaterial { get; set; } = physicalAssetDto.LastConsumedMaterial?.ToSchema();

    /// <summary>
    /// Equipment the physical asset is currently equipped by e.g. machine.
    /// [Source: PhysicalAsset]
    /// </summary>
    public Equipment? EquippedBy { get; set; } = physicalAssetDto.EquippedBy is null ? null : new Equipment(physicalAssetDto.EquippedBy);

    /// <summary>
    /// The history of the physical asset.
    /// [Source: PhysicalAsset]
    /// </summary>
    public Task<IEnumerable<PhysicalAssetHistoryItem>?> History(
        PhysicalAssetHistoryBatchDataLoader physicalAssetHistoryBatchDataLoader,
        [Service] IPhysicalAssetService physicalAssetService)
        => physicalAssetService.GetHistory(physicalAssetHistoryBatchDataLoader, PhysicalAssetId);

    /// <summary>
    /// The defects of the physical asset.
    /// [Source: PhysicalAsset]
    /// </summary>
    public Task<IEnumerable<PhysicalAssetDefect>?> Defects(
        PhysicalAssetDefectsBatchDataLoader physicalAssetDefectsBatchDataLoader,
        [Service] IPhysicalAssetService physicalAssetService)
        => physicalAssetService.GetDefects(physicalAssetDefectsBatchDataLoader, PhysicalAssetId);

    internal static PhysicalAsset CreateInstance(PhysicalAssetDto physicalAssetDto)
    {
        return physicalAssetDto.PhysicalAssetType switch
        {
            PhysicalAssetType.Anilox => new AniloxPhysicalAsset((AniloxPhysicalAssetDto)physicalAssetDto),
            PhysicalAssetType.Plate => new PlatePhysicalAsset((PlatePhysicalAssetDto)physicalAssetDto),
            _ => throw new ArgumentException(
                $"Creating a physical asset is not supported for the type '{physicalAssetDto.PhysicalAssetType}'.")
        };
    }
}