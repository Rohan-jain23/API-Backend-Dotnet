using PhysicalAssetDataHandler.Client.Models.Dtos.Operation;

namespace FrameworkAPI.Schema.PhysicalAsset.Operation;

/// <summary>
/// A refurbishing operation.
/// </summary>
/// <param name="refurbishingOperationDto">The refurbishing operation dto.</param>
public class RefurbishingOperation(RefurbishingOperationDto refurbishingOperationDto) : Operation(refurbishingOperationDto);