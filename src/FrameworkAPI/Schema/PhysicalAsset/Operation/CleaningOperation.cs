using PhysicalAssetDataHandler.Client.Models.Dtos.Operation;
using PhysicalAssetDataHandler.Client.Models.Enums;

namespace FrameworkAPI.Schema.PhysicalAsset.Operation;

/// <summary>
/// A cleaning operation.
/// </summary>
/// <param name="cleaningOperationDto">The cleaning operation dto.</param>
public class CleaningOperation(CleaningOperationDto cleaningOperationDto) : Operation(cleaningOperationDto)
{
    /// <summary>
    /// Type of the cleaning.
    /// [Source: CleaningOperation]
    /// </summary>
    public CleaningOperationType CleaningOperationType { get; set; } = cleaningOperationDto.CleaningOperationType;

    /// <summary>
    /// The flag which signals if the cleaning resets past volume defects.
    /// [Source: CleaningOperation]
    /// </summary>
    public bool ResetVolumeDefects { get; set; } = cleaningOperationDto.ResetVolumeDefects;
}