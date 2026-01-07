using System.Threading.Tasks;
using FrameworkAPI.Schema.PhysicalAsset.Operation;

namespace FrameworkAPI.Services.Interfaces;

public interface IPhysicalAssetOperationService
{
    Task<CleaningOperation> CreateCleaningOperation(
        CreateCleaningOperationRequest createCleaningOperationRequest, string userId);

    Task<ScrappingOperation> CreateScrappingOperation(
        CreateScrappingOperationRequest createScrappingOperationRequest, string userId);

    Task<RefurbishingOperation> CreateRefurbishingAniloxOperation(
        CreateRefurbishingAniloxOperationRequest createRefurbishingAniloxOperationRequest, string userId);
}