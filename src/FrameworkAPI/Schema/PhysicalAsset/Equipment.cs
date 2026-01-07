using PhysicalAssetDataHandler.Client.Models.Dtos;
using PhysicalAssetDataHandler.Client.Models.Enums;

namespace FrameworkAPI.Schema.PhysicalAsset;

/// <summary>
/// An equipment an physical asset can equip.
/// </summary>
/// <param name="equipmentDto">The equipment dto.</param>
public class Equipment(EquipmentDto equipmentDto)
{
    /// <summary>
    /// Type of the equipmentType.
    /// [Source: Equipment]
    /// </summary>
    public EquipmentType EquipmentType { get; set; } = equipmentDto.EquipmentType;

    /// <summary>
    /// Unique identifier of the equipment.
    /// [Source: Equipment]
    /// </summary>
    public string EquipmentId { get; set; } = equipmentDto.EquipmentId;

    /// <summary>
    /// Description of the equipment.
    /// [Source: Equipment]
    /// </summary>
    public string? Description { get; set; } = equipmentDto.Description;
}