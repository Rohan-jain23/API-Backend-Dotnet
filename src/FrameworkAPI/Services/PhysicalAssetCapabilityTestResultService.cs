using System.Threading.Tasks;
using FrameworkAPI.Exceptions;
using FrameworkAPI.Schema.PhysicalAsset.CapabilityTest;
using FrameworkAPI.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using PhysicalAssetDataHandler.Client.QueueWrappers;
using CreateAniloxCapabilityTestResultRequest =
    FrameworkAPI.Schema.PhysicalAsset.CapabilityTest.CreateAniloxCapabilityTestResultRequest;
using CreateVolumeCapabilityTestResultRequest =
    FrameworkAPI.Schema.PhysicalAsset.CapabilityTest.CreateVolumeCapabilityTestResultRequest;
using Messages = PhysicalAssetDataHandler.Client.Models.Messages;

namespace FrameworkAPI.Services;

public class PhysicalAssetCapabilityTestResultService(IPhysicalAssetQueueWrapper physicalAssetQueueWrapper)
    : IPhysicalAssetCapabilityTestResultService
{
    public async Task<AniloxCapabilityTestResult> CreateAniloxCapabilityTestResult(
        CreateAniloxCapabilityTestResultRequest createAniloxCapabilityTestResultRequest,
        string userId)
    {
        var createAniloxCapabilityTestResultRequestMessage =
            new Messages.PhysicalAssetInformation.CapabilityTest.CreateAniloxCapabilityTestResultRequest(
                userId: userId,
                createAniloxCapabilityTestResultRequest.PhysicalAssetId,
                createAniloxCapabilityTestResultRequest.TestDateTime,
                createAniloxCapabilityTestResultRequest.Note,
                createAniloxCapabilityTestResultRequest.AniloxCapabilityErrorType,
                createAniloxCapabilityTestResultRequest.StartPositionOnAnilox,
                createAniloxCapabilityTestResultRequest.EndPositionOnAnilox);

        var response = await physicalAssetQueueWrapper
            .SendCreateAniloxCapabilityTestResultAndWaitForReply(createAniloxCapabilityTestResultRequestMessage);

        if (!response.HasError)
        {
            return new AniloxCapabilityTestResult(response.Item);
        }

        if (response.Error.StatusCode is StatusCodes.Status400BadRequest)
        {
            throw new ParameterInvalidException(response.Error.ErrorMessage);
        }

        throw new InternalServiceException(response.Error);
    }

    public async Task<VolumeCapabilityTestResult> CreateVolumeCapabilityTestResult(
        CreateVolumeCapabilityTestResultRequest createVolumeCapabilityTestResultRequest,
        string userId)
    {
        var createVolumeCapabilityTestResultRequestMessage =
            new Messages.PhysicalAssetInformation.CapabilityTest.CreateVolumeCapabilityTestResultRequest(
                userId,
                createVolumeCapabilityTestResultRequest.PhysicalAssetId,
                createVolumeCapabilityTestResultRequest.TestDateTime,
                createVolumeCapabilityTestResultRequest.Note,
                createVolumeCapabilityTestResultRequest.Volume);

        var response = await physicalAssetQueueWrapper
            .SendCreateVolumeCapabilityTestResultAndWaitForReply(createVolumeCapabilityTestResultRequestMessage);

        if (!response.HasError)
        {
            return new VolumeCapabilityTestResult(response.Item);
        }

        if (response.Error.StatusCode is StatusCodes.Status400BadRequest)
        {
            throw new ParameterInvalidException(response.Error.ErrorMessage);
        }

        throw new InternalServiceException(response.Error);
    }
}