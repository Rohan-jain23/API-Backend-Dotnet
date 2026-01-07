using System;
using PhysicalAssetDataHandler.Client.Models.Dtos.Operation;
using PhysicalAssetDataHandler.Client.Models.Enums;

namespace FrameworkAPI.Schema.PhysicalAsset.Operation;

/// <summary>
/// Generic interface for operations.
/// </summary>
/// <param name="operationDto">The operation dto.</param>
public abstract class Operation(OperationDto operationDto)
{

    /// <summary>
    /// Type of the operation.
    /// [Source: Operation]
    /// </summary>
    public OperationType OperationType { get; set; } = operationDto.OperationType;

    /// <summary>
    /// Unique identifier of the equipment physical asset mapping the operation is associated with.
    /// [Source: Operation]
    /// </summary>
    public string EquipmentPhysicalAssetMappingId { get; set; } = operationDto.EquipmentPhysicalAssetMappingId;

    /// <summary>
    /// Unique identifier of the physical asset the operation is associated with.
    /// [Source: Operation]
    /// </summary>
    public string PhysicalAssetId { get; set; } = operationDto.PhysicalAssetId;

    /// <summary>
    /// Date the operation was started.
    /// [Source: Operation]
    /// </summary>
    public DateTime StartDateTime { get; set; } = operationDto.StartDateTime;

    /// <summary>
    /// Date the operation was ended.
    /// [Source: Operation]
    /// </summary>
    public DateTime? EndDateTime { get; set; } = operationDto.EndDateTime;

    /// <summary>
    /// Id of the user who carried out the operation.
    /// [Source: Operation]
    /// </summary>
    public string OperatorUserId { get; set; } = operationDto.OperatorUserId;

    /// <summary>
    /// The additional note for this operation.
    /// [Source: Operation]
    /// </summary>
    public string? Note { get; set; } = operationDto.Note;
}