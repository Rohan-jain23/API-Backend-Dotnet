using System.Threading.Tasks;
using FrameworkAPI.Attributes;
using FrameworkAPI.Exceptions;
using FrameworkAPI.Schema.PhysicalAsset;
using FrameworkAPI.Schema.PhysicalAsset.CapabilityTest;
using FrameworkAPI.Schema.PhysicalAsset.Operation;
using FrameworkAPI.Services.Interfaces;
using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Types;

namespace FrameworkAPI.Mutations;

[ExtendObjectType("Mutation")]
public class PhysicalAssetsMutation
{
    [Authorize(Roles = ["go-general"])]
    [LicenceCheck(Licence = Constants.LicensesApplications.Anilox)]
    [Error(typeof(ParameterInvalidException))]
    [Error(typeof(InternalServiceException))]
    [UseMutationConvention(PayloadFieldName = "updatePhysicalAssetSettings")]
    public async Task<PhysicalAssetSettings> PhysicalAssetUpdateSettings(
        [GlobalState] string userId,
        [Service] IPhysicalAssetService physicalAssetService,
        UpdatePhysicalAssetSettingsRequest updatePhysicalAssetSettingsRequest)
    {
        var updatePhysicalAssetSettings =
            await physicalAssetService.UpdatePhysicalAssetSettings(updatePhysicalAssetSettingsRequest, userId);
        return updatePhysicalAssetSettings;
    }

    [Authorize(Roles = ["go-general"])]
    [Error(typeof(ParameterInvalidException))]
    [Error(typeof(InternalServiceException))]
    [UseMutationConvention(PayloadFieldName = "createdAniloxPhysicalAsset")]
    public async Task<AniloxPhysicalAsset> PhysicalAssetsCreateAnilox(
        [GlobalState] string userId,
        [Service] IPhysicalAssetService physicalAssetService,
        CreateAniloxPhysicalAssetRequest createAniloxPhysicalAssetRequest)
    {
        var createdAniloxPhysicalAsset =
            await physicalAssetService.CreateAniloxPhysicalAsset(createAniloxPhysicalAssetRequest, userId);
        return createdAniloxPhysicalAsset;
    }

    [Authorize(Roles = ["go-general"])]
    [LicenceCheck(Licence = Constants.LicensesApplications.Anilox)]
    [Error(typeof(ParameterInvalidException))]
    [Error(typeof(InternalServiceException))]
    [UseMutationConvention(PayloadFieldName = "updatedAniloxPhysicalAsset")]
    public async Task<AniloxPhysicalAsset> PhysicalAssetsUpdateAnilox(
        [GlobalState] string userId,
        [Service] IPhysicalAssetService physicalAssetService,
        UpdateAniloxPhysicalAssetRequest updateAniloxPhysicalAssetRequest)
    {
        var updatedAniloxPhysicalAsset =
            await physicalAssetService.UpdateAniloxPhysicalAsset(updateAniloxPhysicalAssetRequest, userId);
        return updatedAniloxPhysicalAsset;
    }

    [Authorize(Roles = ["go-general"])]
    [LicenceCheck(Licence = Constants.LicensesApplications.Anilox)]
    [Error(typeof(ParameterInvalidException))]
    [Error(typeof(InternalServiceException))]
    [UseMutationConvention(PayloadFieldName = "createdAniloxCapabilityTestResult")]
    public async Task<AniloxCapabilityTestResult> PhysicalAssetsCreateAniloxCapabilityTestResult(
        [GlobalState] string userId,
        [Service] IPhysicalAssetCapabilityTestResultService physicalAssetCapabilityTestResultService,
        CreateAniloxCapabilityTestResultRequest createAniloxCapabilityTestResultRequest)
    {
        var aniloxCapabilityTestResult = await physicalAssetCapabilityTestResultService
            .CreateAniloxCapabilityTestResult(createAniloxCapabilityTestResultRequest, userId);
        return aniloxCapabilityTestResult;
    }

    [Authorize(Roles = ["go-general"])]
    [LicenceCheck(Licence = Constants.LicensesApplications.Anilox)]
    [Error(typeof(ParameterInvalidException))]
    [Error(typeof(InternalServiceException))]
    [UseMutationConvention(PayloadFieldName = "createdVolumeCapabilityTestResult")]
    public async Task<VolumeCapabilityTestResult> PhysicalAssetsCreateVolumeCapabilityTestResult(
        [GlobalState] string userId,
        [Service] IPhysicalAssetCapabilityTestResultService physicalAssetCapabilityTestResultService,
        CreateVolumeCapabilityTestResultRequest createVolumeCapabilityTestResultRequest)
    {
        var volumeCapabilityTestResult = await physicalAssetCapabilityTestResultService
            .CreateVolumeCapabilityTestResult(createVolumeCapabilityTestResultRequest, userId);
        return volumeCapabilityTestResult;
    }

    [Authorize(Roles = ["go-general"])]
    [LicenceCheck(Licence = Constants.LicensesApplications.Anilox)]
    [Error(typeof(ParameterInvalidException))]
    [Error(typeof(InternalServiceException))]
    [UseMutationConvention(PayloadFieldName = "createdCleaningOperationResult")]
    public async Task<CleaningOperation> PhysicalAssetsCreateCleaningOperation(
        [GlobalState] string userId,
        [Service] IPhysicalAssetOperationService physicalAssetOperationService,
        CreateCleaningOperationRequest createCleaningOperationRequest)
    {
        var cleaningOperation = await physicalAssetOperationService
            .CreateCleaningOperation(createCleaningOperationRequest, userId);
        return cleaningOperation;
    }

    [Authorize(Roles = ["go-general"])]
    [LicenceCheck(Licence = Constants.LicensesApplications.Anilox)]
    [Error(typeof(ParameterInvalidException))]
    [Error(typeof(InternalServiceException))]
    [UseMutationConvention(PayloadFieldName = "createdScrappingOperationResult")]
    public async Task<ScrappingOperation> PhysicalAssetsCreateScrappingOperation(
        [GlobalState] string userId,
        [Service] IPhysicalAssetOperationService physicalAssetOperationService,
        CreateScrappingOperationRequest createScrappingOperationRequest)
    {
        var scrappingOperation = await physicalAssetOperationService
            .CreateScrappingOperation(createScrappingOperationRequest, userId);
        return scrappingOperation;
    }

    [Authorize(Roles = ["go-general"])]
    [LicenceCheck(Licence = Constants.LicensesApplications.Anilox)]
    [Error(typeof(ParameterInvalidException))]
    [Error(typeof(InternalServiceException))]
    [UseMutationConvention(PayloadFieldName = "createdRefurbishingAniloxOperationResult")]
    public async Task<RefurbishingOperation> PhysicalAssetsCreateRefurbishingAniloxOperation(
        [GlobalState] string userId,
        [Service] IPhysicalAssetOperationService physicalAssetOperationService,
        CreateRefurbishingAniloxOperationRequest createRefurbishingAniloxOperationRequest)
    {
        var refurbishingOperation = await physicalAssetOperationService
            .CreateRefurbishingAniloxOperation(createRefurbishingAniloxOperationRequest, userId);
        return refurbishingOperation;
    }
}