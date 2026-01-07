using System.Threading.Tasks;
using FrameworkAPI.Exceptions;
using FrameworkAPI.Services.Interfaces;
using FrameworkAPI.Schema.PhysicalAsset.Operation;
using Microsoft.AspNetCore.Http;
using PhysicalAssetDataHandler.Client.QueueWrappers;
using CreateCleaningOperationRequest = FrameworkAPI.Schema.PhysicalAsset.Operation.CreateCleaningOperationRequest;
using Messages = PhysicalAssetDataHandler.Client.Models.Messages;

namespace FrameworkAPI.Services;

public class PhysicalAssetOperationService(IPhysicalAssetQueueWrapper physicalAssetQueueWrapper) : IPhysicalAssetOperationService
{
    public async Task<CleaningOperation> CreateCleaningOperation(
        CreateCleaningOperationRequest createCleaningOperationRequest, string userId)
    {
        var createCleaningOperationRequestMessage = new Messages.PhysicalAssetInformation.Operation.CreateCleaningOperationRequest(
            userId,
            createCleaningOperationRequest.PhysicalAssetId,
            createCleaningOperationRequest.Note,
            createCleaningOperationRequest.StartDateTime,
            createCleaningOperationRequest.CleaningOperationType,
            createCleaningOperationRequest.ResetVolumeDefects);

        var response = await physicalAssetQueueWrapper
            .SendCreateCleaningOperationAndWaitForReply(createCleaningOperationRequestMessage);

        if (!response.HasError)
        {
            return new CleaningOperation(response.Item);
        }

        if (response.Error.StatusCode is StatusCodes.Status400BadRequest or StatusCodes.Status409Conflict)
        {
            throw new ParameterInvalidException(response.Error.ErrorMessage);
        }

        throw new InternalServiceException(response.Error);
    }

    public async Task<ScrappingOperation> CreateScrappingOperation(
        CreateScrappingOperationRequest createScrappingOperationRequest, string userId)
    {
        var createScrappingOperationRequestMessage = new Messages.PhysicalAssetInformation.Operation.CreateScrappingOperationRequest(
            userId,
            createScrappingOperationRequest.PhysicalAssetId,
            createScrappingOperationRequest.Note,
            createScrappingOperationRequest.ScrapDateTime);

        var response = await physicalAssetQueueWrapper
            .SendCreateScrappingOperationAndWaitForReply(createScrappingOperationRequestMessage);

        if (!response.HasError)
        {
            return new ScrappingOperation(response.Item);
        }

        if (response.Error.StatusCode is StatusCodes.Status400BadRequest or StatusCodes.Status409Conflict)
        {
            throw new ParameterInvalidException(response.Error.ErrorMessage);
        }

        throw new InternalServiceException(response.Error);
    }

    public async Task<RefurbishingOperation> CreateRefurbishingAniloxOperation(
        CreateRefurbishingAniloxOperationRequest createRefurbishingAniloxOperationRequest, string userId)
    {
        var createScrappingOperationRequestMessage = new Messages.PhysicalAssetInformation.Operation.CreateRefurbishingAniloxOperationRequest(
            userId,
            createRefurbishingAniloxOperationRequest.PhysicalAssetId,
            createRefurbishingAniloxOperationRequest.Note,
            createRefurbishingAniloxOperationRequest.RefurbishedDateTime,
            createRefurbishingAniloxOperationRequest.SerialNumberOverwrite,
            createRefurbishingAniloxOperationRequest.ManufacturerOverwrite,
            createRefurbishingAniloxOperationRequest.Screen,
            createRefurbishingAniloxOperationRequest.Engraving,
            createRefurbishingAniloxOperationRequest.SetVolumeValue,
            createRefurbishingAniloxOperationRequest.MeasuredVolumeValue);

        var response = await physicalAssetQueueWrapper
            .SendCreateRefurbishingAniloxOperationAndWaitForReply(createScrappingOperationRequestMessage);

        if (!response.HasError)
        {
            return new RefurbishingOperation(response.Item);
        }

        if (response.Error.StatusCode is StatusCodes.Status400BadRequest or StatusCodes.Status409Conflict)
        {
            throw new ParameterInvalidException(response.Error.ErrorMessage);
        }

        throw new InternalServiceException(response.Error);
    }
}