using System;
using System.Threading.Tasks;
using FluentAssertions;
using FrameworkAPI.Exceptions;
using FrameworkAPI.Schema.PhysicalAsset.CapabilityTest;
using FrameworkAPI.Services;
using Microsoft.AspNetCore.Http;
using Moq;
using PhysicalAssetDataHandler.Client.Models.Dtos.CapabilityTest;
using PhysicalAssetDataHandler.Client.Models.Enums;
using PhysicalAssetDataHandler.Client.QueueWrappers;
using WuH.Ruby.Common.Core;
using Xunit;
using Client = PhysicalAssetDataHandler.Client.Models.Messages.PhysicalAssetInformation.CapabilityTest;

namespace FrameworkAPI.Test.Services;

public class PhysicalAssetCapabilityTestResultServiceTests
{
    private const string UserId = "dab52044-e92b-4769-a89c-3ceb44c33448";
    private readonly PhysicalAssetCapabilityTestResultService _physicalAssetCapabilityTestResultService;
    private readonly Mock<IPhysicalAssetQueueWrapper> _physicalAssetQueueWrapperMock = new();

    public PhysicalAssetCapabilityTestResultServiceTests()
    {
        _physicalAssetCapabilityTestResultService =
            new PhysicalAssetCapabilityTestResultService(_physicalAssetQueueWrapperMock.Object);
    }

    [Theory]
    [InlineData(AniloxCapabilityErrorType.ScoringLine, 100.0, null)]
    [InlineData(AniloxCapabilityErrorType.Surface, 100.0, 200.0)]
    [InlineData(AniloxCapabilityErrorType.Volume, 100.0, 400.0)]
    [InlineData(AniloxCapabilityErrorType.Other, 250.0, 400.0)]
    public async Task
        CreateAniloxCapabilityTestResult_Returns_CreatedAniloxCapabilityTestResult_When_QueueWrapper_Replies_With_Created_AniloxCapabilityTestResult(
            AniloxCapabilityErrorType aniloxCapabilityErrorType,
            double startPositionOnAnilox,
            double? endPositionOnAnilox)
    {
        // Arrange
        var createAniloxCapabilityTestResultRequest =
            new CreateAniloxCapabilityTestResultRequest(
                physicalAssetId: "65c1ec08e43a3ed4f620c169",
                testDateTime: DateTime.UtcNow,
                note: null,
                aniloxCapabilityErrorType,
                startPositionOnAnilox,
                endPositionOnAnilox);
        var createAniloxCapabilityTestResultRequestMessage =
            new Client.CreateAniloxCapabilityTestResultRequest(
                UserId,
                createAniloxCapabilityTestResultRequest.PhysicalAssetId,
                createAniloxCapabilityTestResultRequest.TestDateTime,
                createAniloxCapabilityTestResultRequest.Note,
                aniloxCapabilityErrorType,
                startPositionOnAnilox,
                endPositionOnAnilox);

        var expectedAniloxCapabilityTestResultDto = new AniloxCapabilityTestResultDto(
            capabilityTestResultId: "65b3693056068ff18b8d1a7f",
            capabilityTestSpecificationId: "65b8b4ed9a37742358b8ef5c",
            createAniloxCapabilityTestResultRequestMessage.PhysicalAssetId,
            createAniloxCapabilityTestResultRequestMessage.TestDateTime,
            testerUserId: createAniloxCapabilityTestResultRequestMessage.UserId,
            createAniloxCapabilityTestResultRequestMessage.Note,
            isPassed: true,
            aniloxCapabilityErrorType,
            startPositionOnAnilox,
            endPositionOnAnilox);

        Client.CreateAniloxCapabilityTestResultRequest?
            capturedCreateAniloxCapabilityTestResultRequestMessage = null;
        _physicalAssetQueueWrapperMock
            .Setup(m => m.SendCreateAniloxCapabilityTestResultAndWaitForReply(
                It.IsAny<Client.CreateAniloxCapabilityTestResultRequest>()))
            .Callback<Client.CreateAniloxCapabilityTestResultRequest>(
                request => capturedCreateAniloxCapabilityTestResultRequestMessage = request)
            .ReturnsAsync(
                new InternalItemResponse<AniloxCapabilityTestResultDto>(expectedAniloxCapabilityTestResultDto))
            .Verifiable(Times.Once);

        // Act
        var createdAniloxCapabilityTestResultDto = await _physicalAssetCapabilityTestResultService
            .CreateAniloxCapabilityTestResult(createAniloxCapabilityTestResultRequest, UserId);

        // Assert
        createdAniloxCapabilityTestResultDto.Should().BeEquivalentTo(expectedAniloxCapabilityTestResultDto);

        capturedCreateAniloxCapabilityTestResultRequestMessage.Should()
            .BeEquivalentTo(createAniloxCapabilityTestResultRequestMessage);

        _physicalAssetQueueWrapperMock.VerifyAll();
        _physicalAssetQueueWrapperMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task
        CreateAniloxCapabilityTestResult_Throws_InternalServiceException_When_QueueWrapper_Replies_With_Error()
    {
        // Arrange
        var createAniloxCapabilityTestResultRequest = new CreateAniloxCapabilityTestResultRequest(
            physicalAssetId: "65c1ec08e43a3ed4f620c169",
            testDateTime: DateTime.UtcNow,
            note: null,
            AniloxCapabilityErrorType.Surface,
            startPositionOnAnilox: 100.0,
            endPositionOnAnilox: 200.0);

        _physicalAssetQueueWrapperMock
            .Setup(m => m.SendCreateAniloxCapabilityTestResultAndWaitForReply(
                It.IsAny<Client.CreateAniloxCapabilityTestResultRequest>()))
            .ReturnsAsync(
                new InternalItemResponse<AniloxCapabilityTestResultDto>(
                    StatusCodes.Status500InternalServerError, "Internal error"))
            .Verifiable(Times.Once);

        // Act
        var createAniloxCapabilityTestResultAction = () =>
            _physicalAssetCapabilityTestResultService.CreateAniloxCapabilityTestResult(
                createAniloxCapabilityTestResultRequest, UserId);

        // Assert
        await createAniloxCapabilityTestResultAction.Should().ThrowAsync<InternalServiceException>();

        _physicalAssetQueueWrapperMock.VerifyAll();
        _physicalAssetQueueWrapperMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task
        CreateAniloxCapabilityTestResult_Throws_ParameterInvalidException_When_QueueWrapper_Replies_With_BadRequest_Error()
    {
        // Arrange
        var createAniloxCapabilityTestResultRequest = new CreateAniloxCapabilityTestResultRequest(
            physicalAssetId: "65c1ec08e43a3ed4f620c169",
            testDateTime: DateTime.UtcNow,
            note: null,
            AniloxCapabilityErrorType.Surface,
            startPositionOnAnilox: 100.0,
            endPositionOnAnilox: 200.0);

        _physicalAssetQueueWrapperMock
            .Setup(m => m.SendCreateAniloxCapabilityTestResultAndWaitForReply(
                It.IsAny<Client.CreateAniloxCapabilityTestResultRequest>()))
            .ReturnsAsync(
                new InternalItemResponse<AniloxCapabilityTestResultDto>(StatusCodes.Status400BadRequest, "Bad request"))
            .Verifiable(Times.Once);

        // Act
        var createAniloxCapabilityTestResultAction = () =>
            _physicalAssetCapabilityTestResultService.CreateAniloxCapabilityTestResult(
                createAniloxCapabilityTestResultRequest, UserId);

        // Assert
        await createAniloxCapabilityTestResultAction.Should().ThrowAsync<ParameterInvalidException>();

        _physicalAssetQueueWrapperMock.VerifyAll();
        _physicalAssetQueueWrapperMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task
        CreateVolumeCapabilityTestResult_Returns_CreatedVolumeCapabilityTestResult_When_QueueWrapper_Replies_With_Created_VolumeCapabilityTestResult()
    {
        // Arrange
        var createVolumeCapabilityTestResultRequest =
            new CreateVolumeCapabilityTestResultRequest(
                physicalAssetId: "65c1ec08e43a3ed4f620c169",
                testDateTime: DateTime.UtcNow,
                note: null,
                volume: 15.0);
        var createVolumeCapabilityTestResultRequestMessage =
            new Client.CreateVolumeCapabilityTestResultRequest(
                UserId,
                createVolumeCapabilityTestResultRequest.PhysicalAssetId,
                createVolumeCapabilityTestResultRequest.TestDateTime,
                createVolumeCapabilityTestResultRequest.Note,
                createVolumeCapabilityTestResultRequest.Volume);

        var expectedVolumeCapabilityTestResultDto = new VolumeCapabilityTestResultDto(
            capabilityTestResultId: "65b3693056068ff18b8d1a7f",
            capabilityTestSpecificationId: "65b8b4ed9a37742358b8ef5c",
            createVolumeCapabilityTestResultRequestMessage.PhysicalAssetId,
            createVolumeCapabilityTestResultRequestMessage.TestDateTime,
            testerUserId: createVolumeCapabilityTestResultRequestMessage.UserId,
            createVolumeCapabilityTestResultRequestMessage.Note,
            isPassed: true,
            createVolumeCapabilityTestResultRequestMessage.Volume);

        Client.CreateVolumeCapabilityTestResultRequest?
            capturedCreateVolumeCapabilityTestResultRequestMessage = null;
        _physicalAssetQueueWrapperMock
            .Setup(m => m.SendCreateVolumeCapabilityTestResultAndWaitForReply(
                It.IsAny<Client.CreateVolumeCapabilityTestResultRequest>()))
            .Callback<Client.CreateVolumeCapabilityTestResultRequest>(
                request => capturedCreateVolumeCapabilityTestResultRequestMessage = request)
            .ReturnsAsync(
                new InternalItemResponse<VolumeCapabilityTestResultDto>(expectedVolumeCapabilityTestResultDto))
            .Verifiable(Times.Once);

        // Act
        var createdVolumeCapabilityTestResultDto = await _physicalAssetCapabilityTestResultService
            .CreateVolumeCapabilityTestResult(createVolumeCapabilityTestResultRequest, UserId);

        // Assert
        createdVolumeCapabilityTestResultDto.Should().BeEquivalentTo(expectedVolumeCapabilityTestResultDto);

        capturedCreateVolumeCapabilityTestResultRequestMessage.Should()
            .BeEquivalentTo(createVolumeCapabilityTestResultRequestMessage);

        _physicalAssetQueueWrapperMock.VerifyAll();
        _physicalAssetQueueWrapperMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task
        CreateVolumeCapabilityTestResult_Throws_InternalServiceException_When_QueueWrapper_Replies_With_Error()
    {
        // Arrange
        var createVolumeCapabilityTestResultRequest = new CreateVolumeCapabilityTestResultRequest(
            physicalAssetId: "65c1ec08e43a3ed4f620c169",
            testDateTime: DateTime.UtcNow,
            note: null,
            volume: 15.0);

        _physicalAssetQueueWrapperMock
            .Setup(m => m.SendCreateVolumeCapabilityTestResultAndWaitForReply(
                It.IsAny<Client.CreateVolumeCapabilityTestResultRequest>()))
            .ReturnsAsync(
                new InternalItemResponse<VolumeCapabilityTestResultDto>(
                    StatusCodes.Status500InternalServerError, "Internal error"))
            .Verifiable(Times.Once);

        // Act
        var createVolumeCapabilityTestResultAction = () =>
            _physicalAssetCapabilityTestResultService.CreateVolumeCapabilityTestResult(
                createVolumeCapabilityTestResultRequest, UserId);

        // Assert
        await createVolumeCapabilityTestResultAction.Should().ThrowAsync<InternalServiceException>();

        _physicalAssetQueueWrapperMock.VerifyAll();
        _physicalAssetQueueWrapperMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task
        CreateVolumeCapabilityTestResult_Throws_ParameterInvalidException_When_QueueWrapper_Replies_With_BadRequest_Error()
    {
        // Arrange
        var createVolumeCapabilityTestResultRequest = new CreateVolumeCapabilityTestResultRequest(
            physicalAssetId: "65c1ec08e43a3ed4f620c169",
            testDateTime: DateTime.UtcNow,
            note: null,
            volume: 15.0);

        _physicalAssetQueueWrapperMock
            .Setup(m => m.SendCreateVolumeCapabilityTestResultAndWaitForReply(
                It.IsAny<Client.CreateVolumeCapabilityTestResultRequest>()))
            .ReturnsAsync(
                new InternalItemResponse<VolumeCapabilityTestResultDto>(
                    StatusCodes.Status400BadRequest, "Bad request"))
            .Verifiable(Times.Once);

        // Act
        var createVolumeCapabilityTestResultAction = () =>
            _physicalAssetCapabilityTestResultService.CreateVolumeCapabilityTestResult(
                createVolumeCapabilityTestResultRequest, UserId);

        // Assert
        await createVolumeCapabilityTestResultAction.Should().ThrowAsync<ParameterInvalidException>();

        _physicalAssetQueueWrapperMock.VerifyAll();
        _physicalAssetQueueWrapperMock.VerifyNoOtherCalls();
    }
}