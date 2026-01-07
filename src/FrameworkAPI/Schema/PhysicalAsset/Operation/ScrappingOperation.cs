using PhysicalAssetDataHandler.Client.Models.Dtos.Operation;

namespace FrameworkAPI.Schema.PhysicalAsset.Operation;

/// <summary>
/// A scrapping operation.
/// </summary>
/// <param name="scrappingOperationDto">The cleaning operation dto.</param>
public class ScrappingOperation(ScrappingOperationDto scrappingOperationDto) : Operation(scrappingOperationDto);