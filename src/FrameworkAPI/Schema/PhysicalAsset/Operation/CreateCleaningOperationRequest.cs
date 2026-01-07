using System;
using PhysicalAssetDataHandler.Client.Models.Enums;

namespace FrameworkAPI.Schema.PhysicalAsset.Operation;

/// <summary>
/// A request to create a physical asset cleaning operation.
/// </summary>
public class CreateCleaningOperationRequest(
    string physicalAssetId,
    string? note,
    DateTime startDateTime,
    CleaningOperationType cleaningOperationType,
    bool resetVolumeDefects)
    : CreateOperationRequest(physicalAssetId, note)
{
    /// <summary>
    /// Date the physical asset was cleaned.
    /// </summary>
    public DateTime StartDateTime { get; set; } = startDateTime;

    /// <summary>
    /// Type of the cleaning operation.
    /// </summary>
    public CleaningOperationType CleaningOperationType { get; set; } = cleaningOperationType;

    /// <summary>
    /// The flag which signals if the cleaning resets past volume defects.
    /// </summary>
    public bool ResetVolumeDefects { get; set; } = resetVolumeDefects;
}