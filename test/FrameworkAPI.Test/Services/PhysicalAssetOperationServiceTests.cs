using System;
using System.Threading.Tasks;
using FluentAssertions;
using FrameworkAPI.Exceptions;
using FrameworkAPI.Schema.PhysicalAsset.Operation;
using FrameworkAPI.Services;
using Microsoft.AspNetCore.Http;
using Moq;
using PhysicalAssetDataHandler.Client.Models.Dtos.Operation;
using PhysicalAssetDataHandler.Client.Models.Enums;
using PhysicalAssetDataHandler.Client.QueueWrappers;
using WuH.Ruby.Common.Core;
using Xunit;
using Client = PhysicalAssetDataHandler.Client.Models.Messages.PhysicalAssetInformation.Operation;

namespace FrameworkAPI.Test.Services;

public class PhysicalAssetOperationServiceTests
{
    private const string UserId = "dab52044-e92b-4769-a89c-3ceb44c33448";

    private readonly PhysicalAssetOperationService _physicalAssetOperationService;
    private readonly Mock<IPhysicalAssetQueueWrapper> _physicalAssetQueueWrapperMock = new();

    public PhysicalAssetOperationServiceTests()
    {
        _physicalAssetOperationService = new PhysicalAssetOperationService(_physicalAssetQueueWrapperMock.Object);
    }

    [Fact]
    public async Task
        CreateCleaningOperation_Returns_CreatedCleaningOperation_When_QueueWrapper_Replies_With_Created_CleaningOperation()
    {
        // Arrange
        var createCleaningOperationRequest = new CreateCleaningOperationRequest(
            physicalAssetId: "65577c04e51181aa1bdcc90f",
            note: null,
            startDateTime: DateTime.UtcNow,
            CleaningOperationType.UltrasonicCleaning,
            resetVolumeDefects: true);

        var createCleaningOperationRequestMessage = new Client.CreateCleaningOperationRequest(
            UserId,
            createCleaningOperationRequest.PhysicalAssetId,
            createCleaningOperationRequest.Note,
            createCleaningOperationRequest.StartDateTime,
            createCleaningOperationRequest.CleaningOperationType,
            createCleaningOperationRequest.ResetVolumeDefects);

        var expectedCleaningOperationDto = new CleaningOperationDto(
            equipmentPhysicalAssetMappingId: "65b3693056068ff18b8d1a7f",
            createCleaningOperationRequest.PhysicalAssetId,
            createCleaningOperationRequest.StartDateTime,
            endDateTime: createCleaningOperationRequest.StartDateTime,
            operatorUserId: UserId,
            createCleaningOperationRequest.Note,
            createCleaningOperationRequest.CleaningOperationType,
            createCleaningOperationRequest.ResetVolumeDefects);

        Client.CreateCleaningOperationRequest? capturedCreateCleaningOperationRequestMessage = null;
        _physicalAssetQueueWrapperMock
            .Setup(m => m.SendCreateCleaningOperationAndWaitForReply(It.IsAny<Client.CreateCleaningOperationRequest>()))
            .Callback<Client.CreateCleaningOperationRequest>(
                request => capturedCreateCleaningOperationRequestMessage = request)
            .ReturnsAsync(new InternalItemResponse<CleaningOperationDto>(expectedCleaningOperationDto))
            .Verifiable(Times.Once);

        // Act
        var createdCleaningOperationDto = await _physicalAssetOperationService.CreateCleaningOperation(
            createCleaningOperationRequest, UserId);

        // Assert
        createdCleaningOperationDto.Should().BeEquivalentTo(expectedCleaningOperationDto);

        capturedCreateCleaningOperationRequestMessage
            .Should().BeEquivalentTo(createCleaningOperationRequestMessage);

        _physicalAssetQueueWrapperMock.VerifyAll();
        _physicalAssetQueueWrapperMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task
        CreateCleaningOperation_Throws_InternalServiceException_When_QueueWrapper_Replies_With_Error()
    {
        // Arrange
        var createCleaningOperationRequest = new CreateCleaningOperationRequest(
            physicalAssetId: "65577c04e51181aa1bdcc90f",
            note: null,
            startDateTime: DateTime.UtcNow,
            CleaningOperationType.UltrasonicCleaning,
            resetVolumeDefects: true);

        _physicalAssetQueueWrapperMock
            .Setup(m => m.SendCreateCleaningOperationAndWaitForReply(It.IsAny<Client.CreateCleaningOperationRequest>()))
            .ReturnsAsync(new InternalItemResponse<CleaningOperationDto>(
                StatusCodes.Status500InternalServerError, "Internal error"))
            .Verifiable(Times.Once);

        // Act
        var createCleaningOperationAction = () => _physicalAssetOperationService.CreateCleaningOperation(
            createCleaningOperationRequest, UserId);

        // Assert
        await createCleaningOperationAction.Should().ThrowAsync<InternalServiceException>();

        _physicalAssetQueueWrapperMock.VerifyAll();
        _physicalAssetQueueWrapperMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task
        CreateCleaningOperation_Throws_ParameterInvalidException_When_QueueWrapper_Replies_With_BadRequest_Error()
    {
        // Arrange
        var createCleaningOperationRequest = new CreateCleaningOperationRequest(
            physicalAssetId: "65577c04e51181aa1bdcc90f",
            note: null,
            startDateTime: DateTime.UtcNow,
            CleaningOperationType.UltrasonicCleaning,
            resetVolumeDefects: true);

        _physicalAssetQueueWrapperMock
            .Setup(m => m.SendCreateCleaningOperationAndWaitForReply(It.IsAny<Client.CreateCleaningOperationRequest>()))
            .ReturnsAsync(
                new InternalItemResponse<CleaningOperationDto>(StatusCodes.Status400BadRequest, "Bad request"))
            .Verifiable(Times.Once);

        // Act
        var createCleaningOperationAction = () => _physicalAssetOperationService.CreateCleaningOperation(
            createCleaningOperationRequest, UserId);

        // Assert
        await createCleaningOperationAction.Should().ThrowAsync<ParameterInvalidException>();

        _physicalAssetQueueWrapperMock.VerifyAll();
        _physicalAssetQueueWrapperMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task
        CreateScrappingOperation_Returns_CreatedScrappingOperation_When_QueueWrapper_Replies_With_Created_ScrappingOperation()
    {
        // Arrange
        var createScrappingOperationRequest = new CreateScrappingOperationRequest(
            physicalAssetId: "65577c04e51181aa1bdcc90f",
            note: null,
            scrapDateTime: DateTime.UtcNow);

        var createScrappingOperationRequestMessage = new Client.CreateScrappingOperationRequest(
            UserId,
            createScrappingOperationRequest.PhysicalAssetId,
            createScrappingOperationRequest.Note,
            createScrappingOperationRequest.ScrapDateTime);

        var expectedScrappingOperationDto = new ScrappingOperationDto(
            equipmentPhysicalAssetMappingId: "65b3693056068ff18b8d1a7f",
            createScrappingOperationRequest.PhysicalAssetId,
            createScrappingOperationRequest.ScrapDateTime,
            endDateTime: createScrappingOperationRequest.ScrapDateTime,
            operatorUserId: UserId,
            createScrappingOperationRequest.Note);

        Client.CreateScrappingOperationRequest? capturedCreateScrappingOperationRequestMessage = null;
        _physicalAssetQueueWrapperMock
            .Setup(m => m.SendCreateScrappingOperationAndWaitForReply(It.IsAny<Client.CreateScrappingOperationRequest>()))
            .Callback<Client.CreateScrappingOperationRequest>(
                request => capturedCreateScrappingOperationRequestMessage = request)
            .ReturnsAsync(new InternalItemResponse<ScrappingOperationDto>(expectedScrappingOperationDto))
            .Verifiable(Times.Once);

        // Act
        var createdScrappingOperationDto = await _physicalAssetOperationService.CreateScrappingOperation(
            createScrappingOperationRequest, UserId);

        // Assert
        createdScrappingOperationDto.Should().BeEquivalentTo(expectedScrappingOperationDto);

        capturedCreateScrappingOperationRequestMessage
            .Should().BeEquivalentTo(createScrappingOperationRequestMessage);

        _physicalAssetQueueWrapperMock.VerifyAll();
        _physicalAssetQueueWrapperMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task
        CreateScrappingOperation_Throws_InternalServiceException_When_QueueWrapper_Replies_With_Error()
    {
        // Arrange
        var createScrappingOperationRequest = new CreateScrappingOperationRequest(
            physicalAssetId: "65577c04e51181aa1bdcc90f",
            note: null,
            scrapDateTime: DateTime.UtcNow);

        _physicalAssetQueueWrapperMock
            .Setup(m => m.SendCreateScrappingOperationAndWaitForReply(It.IsAny<Client.CreateScrappingOperationRequest>()))
            .ReturnsAsync(new InternalItemResponse<ScrappingOperationDto>(
                StatusCodes.Status500InternalServerError, "Internal error"))
            .Verifiable(Times.Once);

        // Act
        var createScrappingOperationAction = () => _physicalAssetOperationService.CreateScrappingOperation(
            createScrappingOperationRequest, UserId);

        // Assert
        await createScrappingOperationAction.Should().ThrowAsync<InternalServiceException>();

        _physicalAssetQueueWrapperMock.VerifyAll();
        _physicalAssetQueueWrapperMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task
        CreateScrappingOperation_Throws_ParameterInvalidException_When_QueueWrapper_Replies_With_BadRequest_Error()
    {
        // Arrange
        var createScrappingOperationRequest = new CreateScrappingOperationRequest(
            physicalAssetId: "65577c04e51181aa1bdcc90f",
            note: null,
            scrapDateTime: DateTime.UtcNow);

        _physicalAssetQueueWrapperMock
            .Setup(m => m.SendCreateScrappingOperationAndWaitForReply(It.IsAny<Client.CreateScrappingOperationRequest>()))
            .ReturnsAsync(
                new InternalItemResponse<ScrappingOperationDto>(StatusCodes.Status400BadRequest, "Bad request"))
            .Verifiable(Times.Once);

        // Act
        var createScrappingOperationAction = () => _physicalAssetOperationService.CreateScrappingOperation(
            createScrappingOperationRequest, UserId);

        // Assert
        await createScrappingOperationAction.Should().ThrowAsync<ParameterInvalidException>();

        _physicalAssetQueueWrapperMock.VerifyAll();
        _physicalAssetQueueWrapperMock.VerifyNoOtherCalls();
    }
}