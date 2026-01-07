using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FrameworkAPI.Exceptions;
using FrameworkAPI.Services;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using Moq;
using PhysicalAssetDataHandler.Client.HttpClients;
using PhysicalAssetDataHandler.Client.Models.Dtos.CapabilityTest;
using PhysicalAssetDataHandler.Client.Models.Enums;
using WuH.Ruby.Common.Core;
using Xunit;

namespace FrameworkAPI.Test.Services;

public class PhysicalAssetCapabilityTestSpecificationServiceTests
{
    private readonly PhysicalAssetCapabilityTestSpecificationService _physicalAssetCapabilityTestSpecificationService;
    private readonly Mock<ICapabilityTestSpecificationHttpClient> _capabilityTestSpecificationHttpClientMock = new();

    public PhysicalAssetCapabilityTestSpecificationServiceTests()
    {
        _physicalAssetCapabilityTestSpecificationService =
            new PhysicalAssetCapabilityTestSpecificationService(_capabilityTestSpecificationHttpClientMock.Object);
    }

    [Fact]
    public async Task GetCurrentCapabilityTestSpecifications_Returns_CapabilityTestSpecifications()
    {
        // Arrange
        var expectedCapabilityTestSpecifications = new List<CapabilityTestSpecificationDto>
        {
            new VolumeCapabilityTestSpecificationDto(
                capabilityTestSpecificationId: ObjectId.GenerateNewId().ToString(),
                version: 1,
                description: "VolumeCapabilityTestSpecificationDto #1",
                createdAt: DateTime.UtcNow,
                isRelative: true,
                volumeDeviationUpperLimit: 5,
                volumeDeviationLowerLimit: 15),
            new VisualCapabilityTestSpecificationDto(
                capabilityTestSpecificationId: ObjectId.GenerateNewId().ToString(),
                version: 1,
                description: "VisualCapabilityTestSpecificationDto #1",
                createdAt: DateTime.UtcNow)
        };

        _capabilityTestSpecificationHttpClientMock
            .Setup(m => m.GetCurrentVersions(CancellationToken.None))
            .ReturnsAsync(
                new InternalListResponse<CapabilityTestSpecificationDto>(expectedCapabilityTestSpecifications))
            .Verifiable(Times.Once);

        // Act
        var capabilityTestSpecifications =
            await _physicalAssetCapabilityTestSpecificationService
                .GetCurrentCapabilityTestSpecifications(CancellationToken.None);

        // Assert
        capabilityTestSpecifications.Should().BeEquivalentTo(expectedCapabilityTestSpecifications);

        _capabilityTestSpecificationHttpClientMock.VerifyAll();
        _capabilityTestSpecificationHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task
        GetCurrentCapabilityTestSpecifications_Returns_Empty_List_When_Response_Contains_NoContent()
    {
        // Arrange
        _capabilityTestSpecificationHttpClientMock
            .Setup(m => m.GetCurrentVersions(CancellationToken.None))
            .ReturnsAsync(
                new InternalListResponse<CapabilityTestSpecificationDto>(StatusCodes.Status204NoContent, "No content"))
            .Verifiable(Times.Once);

        // Act
        var capabilityTestSpecifications = await _physicalAssetCapabilityTestSpecificationService
            .GetCurrentCapabilityTestSpecifications(CancellationToken.None);

        // Assert
        capabilityTestSpecifications.Should().BeEmpty();

        _capabilityTestSpecificationHttpClientMock.VerifyAll();
        _capabilityTestSpecificationHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task
        GetCurrentCapabilityTestSpecifications_Throws_InternalServiceException_When_Response_Contains_Error()
    {
        // Arrange
        _capabilityTestSpecificationHttpClientMock
            .Setup(m => m.GetCurrentVersions(CancellationToken.None))
            .ReturnsAsync(new InternalListResponse<CapabilityTestSpecificationDto>(
                StatusCodes.Status500InternalServerError,
                "Internal error"))
            .Verifiable(Times.Once);

        // Act
        var getCurrentCapabilityTestSpecificationsAction =
            () => _physicalAssetCapabilityTestSpecificationService
                .GetCurrentCapabilityTestSpecifications(CancellationToken.None);

        // Assert
        await getCurrentCapabilityTestSpecificationsAction.Should().ThrowAsync<InternalServiceException>();

        _capabilityTestSpecificationHttpClientMock.VerifyAll();
        _capabilityTestSpecificationHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetCurrentCapabilityTestSpecification_Returns_CapabilityTestSpecification()
    {
        // Arrange
        const CapabilityTestType capabilityTestType = CapabilityTestType.Volume;
        var expectedCapabilityTestSpecification =
            new VolumeCapabilityTestSpecificationDto(
                capabilityTestSpecificationId: ObjectId.GenerateNewId().ToString(),
                version: 1,
                description: "VolumeCapabilityTestSpecificationDto #1",
                createdAt: DateTime.UtcNow,
                isRelative: true,
                volumeDeviationUpperLimit: 5,
                volumeDeviationLowerLimit: 15);

        _capabilityTestSpecificationHttpClientMock
            .Setup(m => m.GetCurrentVersion(capabilityTestType, CancellationToken.None))
            .ReturnsAsync(new InternalItemResponse<CapabilityTestSpecificationDto>(expectedCapabilityTestSpecification))
            .Verifiable(Times.Once);

        // Act
        var capabilityTestSpecification =
            await _physicalAssetCapabilityTestSpecificationService.GetCurrentCapabilityTestSpecification(
                capabilityTestType, CancellationToken.None);

        // Assert
        capabilityTestSpecification.Should().BeEquivalentTo(expectedCapabilityTestSpecification);

        _capabilityTestSpecificationHttpClientMock.VerifyAll();
        _capabilityTestSpecificationHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task
        GetCurrentCapabilityTestSpecification_Throws_InternalServiceException_When_Response_Contains_NoContent()
    {
        // Arrange
        const CapabilityTestType capabilityTestType = CapabilityTestType.Volume;
        _capabilityTestSpecificationHttpClientMock
            .Setup(m => m.GetCurrentVersion(capabilityTestType, CancellationToken.None))
            .ReturnsAsync(
                new InternalItemResponse<CapabilityTestSpecificationDto>(StatusCodes.Status204NoContent, "No content"))
            .Verifiable(Times.Once);

        // Act
        var getCurrentCapabilityTestSpecificationAction =
            () => _physicalAssetCapabilityTestSpecificationService.GetCurrentCapabilityTestSpecification(
                capabilityTestType, CancellationToken.None);

        // Assert
        await getCurrentCapabilityTestSpecificationAction.Should().ThrowAsync<InternalServiceException>();

        _capabilityTestSpecificationHttpClientMock.VerifyAll();
        _capabilityTestSpecificationHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task
        GetCurrentCapabilityTestSpecification_Throws_InternalServiceException_When_Response_Contains_Error()
    {
        // Arrange
        const CapabilityTestType capabilityTestType = CapabilityTestType.Volume;
        _capabilityTestSpecificationHttpClientMock
            .Setup(m => m.GetCurrentVersion(capabilityTestType, CancellationToken.None))
            .ReturnsAsync(new InternalItemResponse<CapabilityTestSpecificationDto>(
                StatusCodes.Status500InternalServerError,
                "Internal error"))
            .Verifiable(Times.Once);

        // Act
        var getCurrentCapabilityTestSpecificationAction =
            () => _physicalAssetCapabilityTestSpecificationService.GetCurrentCapabilityTestSpecification(
                capabilityTestType, CancellationToken.None);

        // Assert
        await getCurrentCapabilityTestSpecificationAction.Should().ThrowAsync<InternalServiceException>();

        _capabilityTestSpecificationHttpClientMock.VerifyAll();
        _capabilityTestSpecificationHttpClientMock.VerifyNoOtherCalls();
    }
}