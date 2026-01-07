using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FrameworkAPI.DataLoaders;
using FrameworkAPI.Exceptions;
using FrameworkAPI.Schema.PhysicalAsset;
using FrameworkAPI.Services;
using FrameworkAPI.Test.Services.Helpers;
using Microsoft.AspNetCore.Http;
using Moq;
using PhysicalAssetDataHandler.Client.HttpClients;
using PhysicalAssetDataHandler.Client.Models;
using PhysicalAssetDataHandler.Client.Models.Dtos;
using PhysicalAssetDataHandler.Client.Models.Dtos.CapabilityTest;
using PhysicalAssetDataHandler.Client.Models.Dtos.Defect;
using PhysicalAssetDataHandler.Client.Models.Dtos.History;
using PhysicalAssetDataHandler.Client.Models.Enums;
using PhysicalAssetDataHandler.Client.QueueWrappers;
using WuH.Ruby.Common.Core;
using Xunit;
using Messages = PhysicalAssetDataHandler.Client.Models.Messages;

namespace FrameworkAPI.Test.Services;

public class PhysicalAssetServiceTests
{
    public static IEnumerable<object?[]> GetPhysicalAssetsTestData()
    {
        yield return
        [
            PhysicalAssetsFilter.All,
            null,
            null
        ];
        yield return
        [
            PhysicalAssetsFilter.Utilisable,
            PhysicalAssetType.Anilox,
            null
        ];
        yield return
        [
            PhysicalAssetsFilter.Utilisable,
            PhysicalAssetType.Plate,
            null
        ];
        yield return
        [
            PhysicalAssetsFilter.Scrapped,
            null,
            DateTime.UtcNow
        ];
        yield return
        [
            PhysicalAssetsFilter.Scrapped,
            PhysicalAssetType.Anilox,
            DateTime.UtcNow
        ];
        yield return
        [
            PhysicalAssetsFilter.Utilisable,
            PhysicalAssetType.Plate,
            DateTime.UtcNow
        ];
    }

    private const string UserId = "test-user-id";
    private readonly PhysicalAssetService _physicalAssetService;
    private readonly Mock<IPhysicalAssetSettingsHttpClient> _physicalAssetSettingsHttpClientMock = new();
    private readonly Mock<IPhysicalAssetHttpClient> _physicalAssetHttpClientMock = new();
    private readonly Mock<IPhysicalAssetQueueWrapper> _physicalAssetQueueWrapperMock = new();
    private readonly PhysicalAssetHistoryBatchDataLoader _physicalAssetHistoryBatchDataLoader;
    private readonly PhysicalAssetDefectsBatchDataLoader _physicalAssetDefectsBatchDataLoader;
    private readonly AniloxCapabilityTestSpecificationDto _aniloxCapabilityTestSpecificationDto = new(
        capabilityTestSpecificationId: "AniloxCapabilityTestSpecification_1",
        version: 1,
        description: null,
        createdAt: new DateTime(2020, 01, 01, 00, 00, 00, 00, DateTimeKind.Utc),
        alwaysPass: false);
    private readonly VolumeCapabilityTestSpecificationDto _volumeCapabilityTestSpecificationDto = new(
        capabilityTestSpecificationId: "VolumeCapabilityTestSpecification_1",
        version: 1,
        description: null,
        createdAt: new DateTime(2022, 01, 01, 00, 00, 00, 00, DateTimeKind.Utc),
        isRelative: false,
        volumeDeviationUpperLimit: 10,
        volumeDeviationLowerLimit: 10);

    public PhysicalAssetServiceTests()
    {
        _physicalAssetService = new PhysicalAssetService(
            _physicalAssetSettingsHttpClientMock.Object,
            _physicalAssetHttpClientMock.Object,
            _physicalAssetQueueWrapperMock.Object);

        var delayedBatchScheduler = new DelayedBatchScheduler();
        _physicalAssetHistoryBatchDataLoader =
            new PhysicalAssetHistoryBatchDataLoader(_physicalAssetHttpClientMock.Object, delayedBatchScheduler);
        _physicalAssetDefectsBatchDataLoader =
            new PhysicalAssetDefectsBatchDataLoader(_physicalAssetHttpClientMock.Object, delayedBatchScheduler);
    }

    [Theory]
    [MemberData(nameof(GetPhysicalAssetsTestData))]
    public async Task GetPhysicalAssets_Returns_PhysicalAssets(PhysicalAssetsFilter physicalAssetsFilter, PhysicalAssetType? physicalAssetTypeFilter, DateTime? lastChangeFilter)
    {
        // Arrange
        var expectedPhysicalAssets = new List<PhysicalAssetDto>();
        var usageCounter = new PhysicalAssetUsageCounterDto(
            current: 0, atLastCleaning: null, sinceLastCleaning: 0, cleaningInterval: 500, cleaningIntervalExceeded: false, unit: "mm");
        var timeUsageCounter = new PhysicalAssetTimeUsageCounterDto(
            current: 0, atLastCleaning: null, sinceLastCleaning: null, unit: "ms");

        var utcNow = DateTime.UtcNow;
        if (physicalAssetTypeFilter is null or PhysicalAssetType.Anilox)
        {
            expectedPhysicalAssets.Add(
                new AniloxPhysicalAssetDto(
                    physicalAssetId: "AniloxPhysicalAssetDto #1",
                    createdAt: utcNow,
                    lastChange: utcNow.AddMinutes(5),
                    serialNumber: "0123456789",
                    manufacturer: "Zecher",
                    description: null,
                    deliveredAt: null,
                    preferredUsageLocation: "EQ12345",
                    initialUsageCounter: null,
                    initialTimeUsageCounter: null,
                    scanCodes: ["123456789", "987654321"],
                    usageCounter,
                    timeUsageCounter,
                    lastCleaning: null,
                    lastConsumedMaterial: null,
                    equippedBy: new EquipmentDto(EquipmentType.Machine, equipmentId: "EQ12345", description: "Machine 12345"),
                    isSleeve: false,
                    printWidth: new ValueWithUnit<double>(3000, "mm"),
                    innerDiameter: null,
                    outerDiameter: new ValueWithUnit<double>(1000, "mm"),
                    screen: new ValueWithUnit<int>(12, "l/cm"),
                    engraving: null,
                    opticalDensity: new PhysicalAssetDataHandler.Client.Models.TestableValueWithUnit<double>(
                        setValue: 100.0, measuredValue: 98, measuredAt: DateTime.MinValue, unit: null),
                    volume: new PhysicalAssetDataHandler.Client.Models.TestableValueWithUnit<double>(
                        setValue: 5.0, measuredValue: null, measuredAt: null, unit: "cm³/m²")));
        }

        if (physicalAssetTypeFilter is null or PhysicalAssetType.Plate)
        {
            expectedPhysicalAssets.Add(
                new PlatePhysicalAssetDto(
                    physicalAssetId: "PlatePhysicalAssetDto #1",
                    createdAt: utcNow,
                    lastChange: utcNow.AddMinutes(5),
                    serialNumber: "9876543210",
                    manufacturer: "Zecher",
                    description: null,
                    deliveredAt: null,
                    preferredUsageLocation: "EQ12345",
                    initialUsageCounter: 1000,
                    initialTimeUsageCounter: null,
                    scanCodes: [],
                    usageCounter,
                    timeUsageCounter,
                    lastCleaning: null,
                    lastConsumedMaterial: null,
                    equippedBy: null,
                    surface: null));
        }

        _physicalAssetHttpClientMock
            .Setup(m => m.GetPhysicalAssets(physicalAssetsFilter, physicalAssetTypeFilter, lastChangeFilter,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalListResponse<PhysicalAssetDto>(expectedPhysicalAssets))
            .Verifiable(Times.Once);

        // Act
        var physicalAssets = await _physicalAssetService.GetAllPhysicalAssets(
            physicalAssetsFilter, physicalAssetTypeFilter, lastChangeFilter, CancellationToken.None);

        // Assert
        physicalAssets.Should().BeEquivalentTo(expectedPhysicalAssets);

        _physicalAssetHttpClientMock.VerifyAll();
        _physicalAssetHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetPhysicalAssets_Returns_Empty_List_When_Response_Contains_NoContent()
    {
        // Arrange
        _physicalAssetHttpClientMock
            .Setup(m => m.GetPhysicalAssets(PhysicalAssetsFilter.Utilisable, null, null, CancellationToken.None))
            .ReturnsAsync(new InternalListResponse<PhysicalAssetDto>(StatusCodes.Status204NoContent, "No content"));

        // Act
        var physicalAssets = await _physicalAssetService.GetAllPhysicalAssets(
            physicalAssetsFilter: PhysicalAssetsFilter.Utilisable,
            physicalAssetTypeFilter: null,
            lastChangeFilter: null,
            CancellationToken.None);

        // Assert
        physicalAssets.Should().BeEmpty();

        _physicalAssetHttpClientMock.VerifyAll();
        _physicalAssetHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetPhysicalAssets_Throws_InternalServiceException_When_Response_Contains_Error()
    {
        // Arrange
        _physicalAssetHttpClientMock
            .Setup(m => m.GetPhysicalAssets(PhysicalAssetsFilter.Utilisable, null, null, CancellationToken.None))
            .ReturnsAsync(new InternalListResponse<PhysicalAssetDto>(StatusCodes.Status500InternalServerError,
                "Internal error"))
            .Verifiable(Times.Once);

        // Act
        var getAllPhysicalAssetsAction = () => _physicalAssetService.GetAllPhysicalAssets(
            physicalAssetsFilter: PhysicalAssetsFilter.Utilisable,
            physicalAssetTypeFilter: null,
            lastChangeFilter: null,
            CancellationToken.None);

        // Assert
        await getAllPhysicalAssetsAction.Should().ThrowAsync<InternalServiceException>();

        _physicalAssetHttpClientMock.VerifyAll();
        _physicalAssetHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetPhysicalAsset_Returns_PhysicalAsset()
    {
        // Arrange
        const string physicalAssetId = "AniloxPhysicalAssetDto #1";
        var utcNow = DateTime.UtcNow;
        var usageCounter = new PhysicalAssetUsageCounterDto(
            current: 0, atLastCleaning: null, sinceLastCleaning: 0, cleaningInterval: 500, cleaningIntervalExceeded: false, unit: "mm");
        var timeUsageCounter = new PhysicalAssetTimeUsageCounterDto(
            current: 0, atLastCleaning: null, sinceLastCleaning: null, unit: "ms");
        var expectedPhysicalAssetDto =
            new AniloxPhysicalAssetDto(
                physicalAssetId,
                createdAt: utcNow,
                lastChange: utcNow.AddMinutes(5),
                serialNumber: "0123456789",
                manufacturer: "Zecher",
                description: null,
                deliveredAt: null,
                preferredUsageLocation: "EQ12345",
                initialUsageCounter: null,
                initialTimeUsageCounter: null,
                scanCodes: ["123456789", "987654321"],
                usageCounter,
                timeUsageCounter,
                lastCleaning: null,
                lastConsumedMaterial: null,
                isSleeve: false,
                equippedBy: null,
                printWidth: new ValueWithUnit<double>(3000, "mm"),
                innerDiameter: null,
                outerDiameter: new ValueWithUnit<double>(1000, "mm"),
                screen: new ValueWithUnit<int>(12, "l/cm"),
                engraving: null,
                opticalDensity: new PhysicalAssetDataHandler.Client.Models.TestableValueWithUnit<double>(
                    setValue: 100.0, measuredValue: 98, measuredAt: DateTime.MinValue, unit: null),
                volume: new PhysicalAssetDataHandler.Client.Models.TestableValueWithUnit<double>(
                    setValue: 5.0, measuredValue: null, measuredAt: null, unit: "cm³/m²"));

        _physicalAssetHttpClientMock
            .Setup(m => m.GetPhysicalAsset(physicalAssetId, CancellationToken.None))
            .ReturnsAsync(new InternalItemResponse<PhysicalAssetDto>(expectedPhysicalAssetDto))
            .Verifiable(Times.Once);

        // Act
        var physicalAsset = await _physicalAssetService.GetPhysicalAsset(physicalAssetId, CancellationToken.None);

        // Assert
        physicalAsset.Should().BeEquivalentTo(
            expectedPhysicalAssetDto,
            options => options
                .Excluding(value => value.PrintWidth)
                .Excluding(value => value.OuterDiameter)
                .Excluding(value => value.Screen));
        var aniloxPhysicalAsset = physicalAsset.As<AniloxPhysicalAsset>();

        await AssertValueWithUnit(aniloxPhysicalAsset.PrintWidth, expectedPhysicalAssetDto.PrintWidth);
        await AssertValueWithUnit(aniloxPhysicalAsset.OuterDiameter, expectedPhysicalAssetDto.OuterDiameter);
        await AssertValueWithUnit(aniloxPhysicalAsset.Screen, expectedPhysicalAssetDto.Screen);

        _physicalAssetHttpClientMock.VerifyAll();
        _physicalAssetHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetPhysicalAsset_Throws_InternalServiceException_When_Response_Contains_NoContent()
    {
        // Arrange
        const string physicalAssetId = "AniloxPhysicalAssetDto #1";
        _physicalAssetHttpClientMock
            .Setup(m => m.GetPhysicalAsset(physicalAssetId, CancellationToken.None))
            .ReturnsAsync(new InternalItemResponse<PhysicalAssetDto>(StatusCodes.Status204NoContent, "No content"))
            .Verifiable(Times.Once);

        // Act
        var getPhysicalAssetAction =
            () => _physicalAssetService.GetPhysicalAsset(physicalAssetId, CancellationToken.None);

        // Assert
        await getPhysicalAssetAction.Should().ThrowAsync<InternalServiceException>();

        _physicalAssetHttpClientMock.VerifyAll();
        _physicalAssetHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetPhysicalAsset_Throws_InternalServiceException_When_Response_Contains_Error()
    {
        // Arrange
        const string physicalAssetId = "AniloxPhysicalAssetDto #1";
        _physicalAssetHttpClientMock
            .Setup(m => m.GetPhysicalAsset(physicalAssetId, CancellationToken.None))
            .ReturnsAsync(new InternalItemResponse<PhysicalAssetDto>(StatusCodes.Status500InternalServerError,
                "Internal error"))
            .Verifiable(Times.Once);

        // Act
        var getPhysicalAssetAction =
            () => _physicalAssetService.GetPhysicalAsset(physicalAssetId, CancellationToken.None);

        // Assert
        await getPhysicalAssetAction.Should().ThrowAsync<InternalServiceException>();

        _physicalAssetHttpClientMock.VerifyAll();
        _physicalAssetHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetHistory_Returns_PhysicalAssetHistoryItems()
    {
        // Arrange
        const string physicalAssetId = "AniloxPhysicalAssetDto #1";
        const string volumeUnit = "cm³/m²";
        var utcNow = DateTime.UtcNow;
        var expectedPhysicalAssetHistory = new List<PhysicalAssetHistoryItemDto>
        {
              new PhysicalAssetScrappedHistoryItemDto(
                    sourceId: "scrapped_EquipmentPhysicalAssetMapping_2",
                    note: null,
                    createdAt: utcNow.AddMinutes(35)),
                new PhysicalAssetSurfaceAnomalyHistoryItemDto(
                    sourceId: "surfaceAnomaly_CapabilityTestResult_1",
                    note: "Found before job #12345",
                    createdAt: utcNow.AddMinutes(25),
                    startPosition: 10.0,
                    endPosition: 20.0,
                    unit: "mm",
                    aniloxCapabilityTestSpecificationDto: _aniloxCapabilityTestSpecificationDto),
                new PhysicalAssetScoringLineHistoryItemDto(
                    sourceId: "scoringLine_CapabilityTestResult_1",
                    note: null,
                    createdAt: utcNow.AddMinutes(25),
                    position: 40.0,
                    unit: "mm",
                    aniloxCapabilityTestSpecificationDto: _aniloxCapabilityTestSpecificationDto),
                new PhysicalAssetCleanedHistoryItemDto(
                    sourceId: "cleaned_EquipmentPhysicalAssetMapping_1",
                    note: "Additional note",
                    createdAt: utcNow.AddMinutes(20),
                    CleaningOperationType.ChemicalCleaning,
                    resetVolumeDefects: true),
                new PhysicalAssetHighVolumeHistoryItemDto(
                    sourceId: "highVolume_CapabilityTestResult_1",
                    note: null,
                    createdAt: utcNow.AddMinutes(15),
                    setValue: 5.0,
                    measuredValue: 10.0,
                    upperLimitValue: _volumeCapabilityTestSpecificationDto.VolumeDeviationUpperLimit!.Value,
                    lowerLimitValue: _volumeCapabilityTestSpecificationDto.VolumeDeviationLowerLimit,
                    volumeUnit,
                    volumeCapabilityTestSpecificationDto: _volumeCapabilityTestSpecificationDto),
                new PhysicalAssetLowVolumeHistoryItemDto(
                    sourceId: "lowVolume_CapabilityTestResult_1",
                    note: null,
                    createdAt: utcNow.AddMinutes(10),
                    setValue: 5.0,
                    measuredValue: 4.0,
                    upperLimitValue: _volumeCapabilityTestSpecificationDto.VolumeDeviationUpperLimit!.Value,
                    lowerLimitValue: _volumeCapabilityTestSpecificationDto.VolumeDeviationLowerLimit,
                    volumeUnit,
                    volumeCapabilityTestSpecificationDto: _volumeCapabilityTestSpecificationDto),
                new PhysicalAssetDeliveredHistoryItemDto(
                    sourceId: $"delivered_PhysicalAsset_{physicalAssetId}",
                    note: null,
                    createdAt: utcNow.AddMinutes(5)),
                new PhysicalAssetCreatedHistoryItemDto(
                    sourceId: $"created_PhysicalAsset_{physicalAssetId}",
                    note: null,
                    createdAt: utcNow)
        };

        _physicalAssetHttpClientMock
            .Setup(m => m.GetPhysicalAssetHistory(physicalAssetId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalListResponse<PhysicalAssetHistoryItemDto>(expectedPhysicalAssetHistory))
            .Verifiable(Times.Once);

        // Act
        var physicalAsset = await _physicalAssetService.GetHistory(
            _physicalAssetHistoryBatchDataLoader, physicalAssetId);

        // Assert
        physicalAsset.Should().BeEquivalentTo(expectedPhysicalAssetHistory);

        _physicalAssetHttpClientMock.VerifyAll();
        _physicalAssetHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetHistory_Returns_Null_When_Response_Contains_No_Content()
    {
        // Arrange
        const string physicalAssetId = "AniloxPhysicalAssetDto #1";

        _physicalAssetHttpClientMock
            .Setup(m => m.GetPhysicalAssetHistory(physicalAssetId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalListResponse<PhysicalAssetHistoryItemDto>(
                StatusCodes.Status204NoContent, "No content"))
            .Verifiable(Times.Once);

        // Act
        var physicalAsset = await _physicalAssetService.GetHistory(
            _physicalAssetHistoryBatchDataLoader, physicalAssetId);

        // Assert
        physicalAsset.Should().BeNull();

        _physicalAssetHttpClientMock.VerifyAll();
        _physicalAssetHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetHistory_Throws_InternalServiceException_When_Response_Contains_Error()
    {
        // Arrange
        const string physicalAssetId = "AniloxPhysicalAssetDto #1";

        _physicalAssetHttpClientMock
            .Setup(m => m.GetPhysicalAssetHistory(physicalAssetId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalListResponse<PhysicalAssetHistoryItemDto>(
                StatusCodes.Status500InternalServerError, "Internal error"))
            .Verifiable(Times.Once);

        // Act
        var getHistoryAction = () => _physicalAssetService.GetHistory(
            _physicalAssetHistoryBatchDataLoader, physicalAssetId);

        // Assert
        await getHistoryAction.Should().ThrowAsync<InternalServiceException>();

        _physicalAssetHttpClientMock.VerifyAll();
        _physicalAssetHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetDefects_Returns_PhysicalAssetDefects()
    {
        // Arrange
        const string physicalAssetId = "AniloxPhysicalAssetDto #1";
        const string volumeUnit = "cm³/m²";
        const string positionUnit = "mm";
        var utcNow = DateTime.UtcNow;
        var expectedPhysicalAssetDefects = new List<PhysicalAssetDefectDto>
        {
            new PhysicalAssetLowVolumeDefectDto(
                sourceId: "PhysicalAssetLowVolumeHistoryItemDto_1",
                note: null,
                createdAt: utcNow.AddMinutes(5),
                setValue: 5.0,
                measuredValue: 4.0,
                upperLimitValue: _volumeCapabilityTestSpecificationDto.VolumeDeviationUpperLimit!.Value,
                lowerLimitValue: _volumeCapabilityTestSpecificationDto.VolumeDeviationLowerLimit,
                volumeUnit,
                volumeCapabilityTestSpecificationDto: _volumeCapabilityTestSpecificationDto),
            new PhysicalAssetHighVolumeDefectDto(
                sourceId: "highVolume_CapabilityTestResult_1",
                note: "note for highVolume_CapabilityTestResult_1",
                createdAt: utcNow.AddMinutes(10),
                setValue: 5.0,
                measuredValue: 10.0,
                upperLimitValue: _volumeCapabilityTestSpecificationDto.VolumeDeviationUpperLimit!.Value,
                lowerLimitValue: _volumeCapabilityTestSpecificationDto.VolumeDeviationLowerLimit,
                volumeUnit,
                volumeCapabilityTestSpecificationDto: _volumeCapabilityTestSpecificationDto),
            new PhysicalAssetScoringLineDefectDto(
                sourceId: "scoringLine_CapabilityTestResult_1",
                note: "note for scoringLine_CapabilityTestResult_1",
                createdAt: utcNow.AddMinutes(15),
                position: 50.0,
                positionUnit,
                aniloxCapabilityTestSpecificationDto: _aniloxCapabilityTestSpecificationDto),
            new PhysicalAssetSurfaceAnomalyDefectDto(
                sourceId: "surfaceAnomaly_CapabilityTestResult_1",
                note: null,
                createdAt: utcNow.AddMinutes(20),
                startPosition: 20.0,
                endPosition: 30.0,
                positionUnit,
                aniloxCapabilityTestSpecificationDto: _aniloxCapabilityTestSpecificationDto)
        };

        _physicalAssetHttpClientMock
            .Setup(m => m.GetPhysicalAssetDefects(physicalAssetId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalListResponse<PhysicalAssetDefectDto>(expectedPhysicalAssetDefects))
            .Verifiable(Times.Once);

        // Act
        var defects = await _physicalAssetService.GetDefects(_physicalAssetDefectsBatchDataLoader, physicalAssetId);

        // Assert
        defects.Should().BeEquivalentTo(expectedPhysicalAssetDefects);

        _physicalAssetHttpClientMock.VerifyAll();
        _physicalAssetHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetDefects_Returns_Null_When_Response_Contains_No_Content()
    {
        // Arrange
        const string physicalAssetId = "AniloxPhysicalAssetDto #1";

        _physicalAssetHttpClientMock
            .Setup(m => m.GetPhysicalAssetDefects(physicalAssetId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new InternalListResponse<PhysicalAssetDefectDto>(StatusCodes.Status204NoContent, "No content"))
            .Verifiable(Times.Once);

        // Act
        var defects = await _physicalAssetService.GetDefects(_physicalAssetDefectsBatchDataLoader, physicalAssetId);

        // Assert
        defects.Should().BeNull();

        _physicalAssetHttpClientMock.VerifyAll();
        _physicalAssetHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetDefects_Throws_InternalServiceException_When_Response_Contains_Error()
    {
        // Arrange
        const string physicalAssetId = "AniloxPhysicalAssetDto #1";

        _physicalAssetHttpClientMock
            .Setup(m => m.GetPhysicalAssetDefects(physicalAssetId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalListResponse<PhysicalAssetDefectDto>(
                StatusCodes.Status500InternalServerError, "Internal error"))
            .Verifiable(Times.Once);

        // Act
        var getDefectsAction = () => _physicalAssetService.GetDefects(
            _physicalAssetDefectsBatchDataLoader, physicalAssetId);

        // Assert
        await getDefectsAction.Should().ThrowAsync<InternalServiceException>();

        _physicalAssetHttpClientMock.VerifyAll();
        _physicalAssetHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpdatePhysicalAssetSettings_Returns_PhysicalAssetSettings_When_QueueWrapper_Replies_With_Updated_PhysicalAssetSettings()
    {
        // Arrange
        var updatePhysicalAssetSettingsRequest = new UpdatePhysicalAssetSettingsRequest(aniloxCleaningIntervalInMeter: 2_000);
        var expectedPhysicalAssetSettingsDto = new PhysicalAssetSettingsDto(
            aniloxCleaningInterval: new ValueWithUnit<int>(updatePhysicalAssetSettingsRequest.AniloxCleaningIntervalInMeter, "m"));

        _physicalAssetQueueWrapperMock
            .Setup(m => m.SendUpdatePhysicalAssetSettingsRequestAndWaitForReply(It.IsAny<Messages.UpdatePhysicalAssetSettingsRequest>()))
            .ReturnsAsync(new InternalItemResponse<PhysicalAssetSettingsDto>(expectedPhysicalAssetSettingsDto));

        // Act
        var physicalAssetSettings = await _physicalAssetService.UpdatePhysicalAssetSettings(updatePhysicalAssetSettingsRequest, UserId);
        var value = await physicalAssetSettings.CleaningInterval.Value(CancellationToken.None);
        var unit = await physicalAssetSettings.CleaningInterval.Unit(CancellationToken.None);

        // Assert
        value.Should().Be(expectedPhysicalAssetSettingsDto.AniloxCleaningInterval.Value);
        unit.Should().BeEquivalentTo(expectedPhysicalAssetSettingsDto.AniloxCleaningInterval.Unit);

        _physicalAssetHttpClientMock.VerifyNoOtherCalls();

        _physicalAssetQueueWrapperMock.VerifyAll();
        _physicalAssetQueueWrapperMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpdatePhysicalAssetSettings_Throws_InternalServiceException_When_QueueWrapper_Replies_With_Error()
    {
        // Arrange
        var updatePhysicalAssetSettingsRequest = new UpdatePhysicalAssetSettingsRequest(aniloxCleaningIntervalInMeter: 2_000);

        _physicalAssetQueueWrapperMock
            .Setup(m => m.SendUpdatePhysicalAssetSettingsRequestAndWaitForReply(It.IsAny<Messages.UpdatePhysicalAssetSettingsRequest>()))
            .ReturnsAsync(new InternalItemResponse<PhysicalAssetSettingsDto>(StatusCodes.Status500InternalServerError, "Internal error"));

        // Act
        var updatePhysicalAssetSettingsAction = () =>
            _physicalAssetService.UpdatePhysicalAssetSettings(updatePhysicalAssetSettingsRequest, UserId);

        // Assert
        await updatePhysicalAssetSettingsAction.Should().ThrowAsync<InternalServiceException>();

        _physicalAssetHttpClientMock.VerifyNoOtherCalls();

        _physicalAssetQueueWrapperMock.VerifyAll();
        _physicalAssetQueueWrapperMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(StatusCodes.Status400BadRequest)]
    [InlineData(StatusCodes.Status409Conflict)]
    public async Task UpdatePhysicalAssetSettings_Throws_ParameterInvalidException_When_QueueWrapper_Replies_With_Specific_Error(int statusCode)
    {
        // Arrange
        var updatePhysicalAssetSettingsRequest = new UpdatePhysicalAssetSettingsRequest(aniloxCleaningIntervalInMeter: 2_000);

        _physicalAssetQueueWrapperMock
            .Setup(m => m.SendUpdatePhysicalAssetSettingsRequestAndWaitForReply(It.IsAny<Messages.UpdatePhysicalAssetSettingsRequest>()))
            .ReturnsAsync(new InternalItemResponse<PhysicalAssetSettingsDto>(statusCode, "Error"));

        // Act
        var updatePhysicalAssetSettingsAction = () =>
            _physicalAssetService.UpdatePhysicalAssetSettings(updatePhysicalAssetSettingsRequest, UserId);

        // Assert
        await updatePhysicalAssetSettingsAction.Should().ThrowAsync<ParameterInvalidException>();

        _physicalAssetHttpClientMock.VerifyNoOtherCalls();

        _physicalAssetQueueWrapperMock.VerifyAll();
        _physicalAssetQueueWrapperMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateAniloxPhysicalAsset_Returns_CreatedAniloxPhysicalAsset_When_QueueWrapper_Replies_With_Created_AniloxPhysicalAsset()
    {
        // Arrange
        var createAniloxPhysicalAssetRequest = new CreateAniloxPhysicalAssetRequest(
            serialNumber: "TZ123456789",
            manufacturer: "WuH",
            description: "Sample Anilox",
            deliveredAt: DateTime.MinValue,
            preferredUsageLocation: "EQ12345",
            initialUsageCounter: null,
            initialTimeUsageCounter: null,
            scanCodes: ["123456789", "987654321"],
            printWidth: 120.5,
            isSleeve: true,
            innerDiameter: 12.4,
            outerDiameter: 17.9,
            screen: 12469,
            engraving: "HEX 60",
            setVolumeValue: 40,
            setOpticalDensityValue: null,
            measuredVolumeValue: null);

        var createAniloxPhysicalAssetRequestMessage =
            new Messages.PhysicalAssetInformation.CreateAniloxPhysicalAssetRequest(
                UserId,
                createAniloxPhysicalAssetRequest.SerialNumber,
                createAniloxPhysicalAssetRequest.Manufacturer,
                createAniloxPhysicalAssetRequest.Description,
                createAniloxPhysicalAssetRequest.DeliveredAt,
                createAniloxPhysicalAssetRequest.PreferredUsageLocation,
                createAniloxPhysicalAssetRequest.InitialUsageCounter,
                createAniloxPhysicalAssetRequest.InitialTimeUsageCounter,
                createAniloxPhysicalAssetRequest.ScanCodes,
                createAniloxPhysicalAssetRequest.PrintWidth,
                createAniloxPhysicalAssetRequest.IsSleeve,
                createAniloxPhysicalAssetRequest.InnerDiameter,
                createAniloxPhysicalAssetRequest.OuterDiameter,
                createAniloxPhysicalAssetRequest.Screen,
                createAniloxPhysicalAssetRequest.Engraving,
                createAniloxPhysicalAssetRequest.SetVolumeValue,
                createAniloxPhysicalAssetRequest.SetOpticalDensityValue,
                createAniloxPhysicalAssetRequest.MeasuredVolumeValue);

        var utcNow = DateTime.UtcNow;
        var usageCounter = new PhysicalAssetUsageCounterDto(
            current: 0, atLastCleaning: null, sinceLastCleaning: 0, cleaningInterval: 500, cleaningIntervalExceeded: false, unit: "mm");
        var timeUsageCounter = new PhysicalAssetTimeUsageCounterDto(
            current: 0, atLastCleaning: null, sinceLastCleaning: null, unit: "ms");
        var expectedAniloxPhysicalAssetDto = new AniloxPhysicalAssetDto(
            physicalAssetId: "physicalAssetId",
            createdAt: utcNow,
            lastChange: utcNow.AddMinutes(5),
            serialNumber: "0123456789",
            manufacturer: "Zecher",
            description: null,
            deliveredAt: null,
            preferredUsageLocation: "EQ12345",
            initialUsageCounter: null,
            initialTimeUsageCounter: null,
            scanCodes: ["123456789", "987654321"],
            usageCounter,
            timeUsageCounter,
            lastCleaning: null,
            lastConsumedMaterial: null,
            isSleeve: false,
            equippedBy: null,
            printWidth: new ValueWithUnit<double>(3000, "mm"),
            innerDiameter: null,
            outerDiameter: new ValueWithUnit<double>(1000, "mm"),
            screen: new ValueWithUnit<int>(12, "l/cm"),
            engraving: null,
            opticalDensity: new PhysicalAssetDataHandler.Client.Models.TestableValueWithUnit<double>(
                setValue: 100.0, measuredValue: 98, measuredAt: DateTime.MinValue, unit: null),
            volume: new PhysicalAssetDataHandler.Client.Models.TestableValueWithUnit<double>(
                setValue: 5.0, measuredValue: null, measuredAt: null, unit: "cm³/m²"));

        Messages.PhysicalAssetInformation.CreateAniloxPhysicalAssetRequest?
            capturedCreateAniloxPhysicalAssetRequestMessage = null;
        _physicalAssetQueueWrapperMock
            .Setup(m => m.SendCreateAniloxPhysicalAssetRequestAndWaitForReply(
                It.IsAny<Messages.PhysicalAssetInformation.CreateAniloxPhysicalAssetRequest>()))
            .Callback<Messages.PhysicalAssetInformation.CreateAniloxPhysicalAssetRequest>(
                request => capturedCreateAniloxPhysicalAssetRequestMessage = request)
            .ReturnsAsync(
                new InternalItemResponse<AniloxPhysicalAssetDto>(expectedAniloxPhysicalAssetDto));

        // Act
        var createdAniloxPhysicalAsset =
            await _physicalAssetService.CreateAniloxPhysicalAsset(createAniloxPhysicalAssetRequest, UserId);

        // Assert
        createdAniloxPhysicalAsset.Should().BeEquivalentTo(
            expectedAniloxPhysicalAssetDto,
            options => options
                .Excluding(value => value.PrintWidth)
                .Excluding(value => value.OuterDiameter)
                .Excluding(value => value.Screen));
        await AssertValueWithUnit(createdAniloxPhysicalAsset.PrintWidth, expectedAniloxPhysicalAssetDto.PrintWidth);
        await AssertValueWithUnit(createdAniloxPhysicalAsset.OuterDiameter, expectedAniloxPhysicalAssetDto.OuterDiameter);
        await AssertValueWithUnit(createdAniloxPhysicalAsset.Screen, expectedAniloxPhysicalAssetDto.Screen);

        capturedCreateAniloxPhysicalAssetRequestMessage.Should()
            .BeEquivalentTo(createAniloxPhysicalAssetRequestMessage);

        _physicalAssetHttpClientMock.VerifyNoOtherCalls();

        _physicalAssetQueueWrapperMock.VerifyAll();
        _physicalAssetQueueWrapperMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateAniloxPhysicalAsset_Throws_InternalServiceException_When_QueueWrapper_Replies_With_Error()
    {
        // Arrange
        var createAniloxPhysicalAssetRequest = new CreateAniloxPhysicalAssetRequest(
            serialNumber: "TZ123456789",
            manufacturer: "WuH",
            description: "Sample Anilox",
            deliveredAt: DateTime.MinValue,
            preferredUsageLocation: "EQ12345",
            initialUsageCounter: null,
            initialTimeUsageCounter: null,
            scanCodes: ["123456789", "987654321"],
            printWidth: 120.5,
            isSleeve: true,
            innerDiameter: 12.4,
            outerDiameter: 17.9,
            screen: 12469,
            engraving: "HEX 60",
            setVolumeValue: 40,
            setOpticalDensityValue: null,
            measuredVolumeValue: null);

        _physicalAssetQueueWrapperMock
            .Setup(m => m.SendCreateAniloxPhysicalAssetRequestAndWaitForReply(
                It.IsAny<Messages.PhysicalAssetInformation.CreateAniloxPhysicalAssetRequest>()))
            .ReturnsAsync(
                new InternalItemResponse<AniloxPhysicalAssetDto>(StatusCodes.Status500InternalServerError,
                    "Internal error"));

        // Act
        var createAniloxPhysicalAssetAction = () =>
            _physicalAssetService.CreateAniloxPhysicalAsset(createAniloxPhysicalAssetRequest, UserId);

        // Assert
        await createAniloxPhysicalAssetAction.Should().ThrowAsync<InternalServiceException>();

        _physicalAssetHttpClientMock.VerifyNoOtherCalls();

        _physicalAssetQueueWrapperMock.VerifyAll();
        _physicalAssetQueueWrapperMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(StatusCodes.Status400BadRequest)]
    [InlineData(StatusCodes.Status409Conflict)]
    public async Task CreateAniloxPhysicalAsset_Throws_ParameterInvalidException_When_QueueWrapper_Replies_With_Specific_Error(int statusCode)
    {
        // Arrange
        var createAniloxPhysicalAssetRequest = new CreateAniloxPhysicalAssetRequest(
            serialNumber: "TZ123456789",
            manufacturer: "WuH",
            description: "Sample Anilox",
            deliveredAt: DateTime.MinValue,
            preferredUsageLocation: "EQ12345",
            initialUsageCounter: null,
            initialTimeUsageCounter: null,
            scanCodes: ["123456789", "987654321"],
            printWidth: 120.5,
            isSleeve: true,
            innerDiameter: 12.4,
            outerDiameter: 17.9,
            screen: 12469,
            engraving: "HEX 60",
            setVolumeValue: 40,
            setOpticalDensityValue: null,
            measuredVolumeValue: null);

        _physicalAssetQueueWrapperMock
            .Setup(m => m.SendCreateAniloxPhysicalAssetRequestAndWaitForReply(
                It.IsAny<Messages.PhysicalAssetInformation.CreateAniloxPhysicalAssetRequest>()))
            .ReturnsAsync(new InternalItemResponse<AniloxPhysicalAssetDto>(statusCode, "Error"));

        // Act
        var createAniloxPhysicalAssetAction = () =>
            _physicalAssetService.CreateAniloxPhysicalAsset(createAniloxPhysicalAssetRequest, UserId);

        // Assert
        await createAniloxPhysicalAssetAction.Should().ThrowAsync<ParameterInvalidException>();

        _physicalAssetHttpClientMock.VerifyNoOtherCalls();

        _physicalAssetQueueWrapperMock.VerifyAll();
        _physicalAssetQueueWrapperMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpdateAniloxPhysicalAsset_SendUpdateAniloxPhysicalAssetRequest_ReturnsUpdatedAniloxPhysicalAsset()
    {
        // Arrange
        var updateAniloxPhysicalAssetRequest = new UpdateAniloxPhysicalAssetRequest(
            "physicalAssetId",
            serialNumber: "TZ123456789",
            manufacturer: "WuH",
            description: "Sample Anilox",
            deliveredAt: DateTime.MinValue,
            preferredUsageLocation: "EQ12345",
            initialUsageCounter: null,
            initialTimeUsageCounter: null,
            scanCodes: ["123456789", "987654321"],
            printWidth: 120.5,
            isSleeve: true,
            innerDiameter: 12.4,
            outerDiameter: 17.9,
            screen: 12469,
            engraving: "HEX 60",
            setVolumeValue: 40,
            setOpticalDensityValue: null
        );

        var updateAniloxPhysicalAssetRequestMessage =
            new Messages.PhysicalAssetInformation.UpdateAniloxPhysicalAssetRequest(
                UserId,
                updateAniloxPhysicalAssetRequest.PhysicalAssetId,
                updateAniloxPhysicalAssetRequest.SerialNumber,
                updateAniloxPhysicalAssetRequest.Manufacturer,
                updateAniloxPhysicalAssetRequest.Description,
                updateAniloxPhysicalAssetRequest.DeliveredAt,
                updateAniloxPhysicalAssetRequest.PreferredUsageLocation,
                updateAniloxPhysicalAssetRequest.InitialUsageCounter,
                updateAniloxPhysicalAssetRequest.InitialTimeUsageCounter,
                updateAniloxPhysicalAssetRequest.ScanCodes,
                updateAniloxPhysicalAssetRequest.PrintWidth,
                updateAniloxPhysicalAssetRequest.IsSleeve,
                updateAniloxPhysicalAssetRequest.InnerDiameter,
                updateAniloxPhysicalAssetRequest.OuterDiameter,
                updateAniloxPhysicalAssetRequest.Screen,
                updateAniloxPhysicalAssetRequest.Engraving,
                updateAniloxPhysicalAssetRequest.SetVolumeValue,
                updateAniloxPhysicalAssetRequest.SetOpticalDensityValue
            );

        var utcNow = DateTime.UtcNow;
        var usageCounter = new PhysicalAssetUsageCounterDto(
            current: 0, atLastCleaning: null, sinceLastCleaning: 0, cleaningInterval: 500, cleaningIntervalExceeded: false, unit: "mm");
        var timeUsageCounter = new PhysicalAssetTimeUsageCounterDto(
            current: 0, atLastCleaning: null, sinceLastCleaning: null, unit: "ms");
        var expectedAniloxPhysicalAssetDto = new AniloxPhysicalAssetDto(
            physicalAssetId: "physicalAssetId",
            createdAt: utcNow,
            lastChange: utcNow.AddMinutes(5),
            serialNumber: "0123456789",
            manufacturer: "Zecher",
            description: null,
            deliveredAt: null,
            preferredUsageLocation: "EQ12345",
            initialUsageCounter: null,
            initialTimeUsageCounter: null,
            scanCodes: ["123456789", "987654321"],
            usageCounter,
            timeUsageCounter,
            lastCleaning: null,
            lastConsumedMaterial: null,
            isSleeve: false,
            equippedBy: null,
            printWidth: new ValueWithUnit<double>(3000, "mm"),
            innerDiameter: null,
            outerDiameter: new ValueWithUnit<double>(1000, "mm"),
            screen: new ValueWithUnit<int>(12, "l/cm"),
            engraving: null,
            opticalDensity: new PhysicalAssetDataHandler.Client.Models.TestableValueWithUnit<double>(
                setValue: 100.0, measuredValue: 98, measuredAt: DateTime.MinValue, unit: null),
            volume: new PhysicalAssetDataHandler.Client.Models.TestableValueWithUnit<double>(
                setValue: 5.0, measuredValue: null, measuredAt: null, unit: "cm³/m²"));

        Messages.PhysicalAssetInformation.UpdateAniloxPhysicalAssetRequest?
            capturedUpdateAniloxPhysicalAssetRequestMessage = null;
        _physicalAssetQueueWrapperMock
            .Setup(m => m.SendUpdateAniloxPhysicalAssetRequestAndWaitForReply(
                It.IsAny<Messages.PhysicalAssetInformation.UpdateAniloxPhysicalAssetRequest>()))
            .Callback<Messages.PhysicalAssetInformation.UpdateAniloxPhysicalAssetRequest>(
                request => capturedUpdateAniloxPhysicalAssetRequestMessage = request)
            .ReturnsAsync(
                new InternalItemResponse<AniloxPhysicalAssetDto>(expectedAniloxPhysicalAssetDto));

        // Act
        var updatedAniloxPhysicalAsset =
            await _physicalAssetService.UpdateAniloxPhysicalAsset(updateAniloxPhysicalAssetRequest, UserId);

        // Assert
        updatedAniloxPhysicalAsset.Should().BeEquivalentTo(
            expectedAniloxPhysicalAssetDto,
            options => options
                .Excluding(value => value.PrintWidth)
                .Excluding(value => value.OuterDiameter)
                .Excluding(value => value.Screen));
        await AssertValueWithUnit(updatedAniloxPhysicalAsset.PrintWidth, expectedAniloxPhysicalAssetDto.PrintWidth);
        await AssertValueWithUnit(updatedAniloxPhysicalAsset.OuterDiameter, expectedAniloxPhysicalAssetDto.OuterDiameter);
        await AssertValueWithUnit(updatedAniloxPhysicalAsset.Screen, expectedAniloxPhysicalAssetDto.Screen);

        capturedUpdateAniloxPhysicalAssetRequestMessage.Should()
            .BeEquivalentTo(updateAniloxPhysicalAssetRequestMessage);

        _physicalAssetHttpClientMock.VerifyNoOtherCalls();

        _physicalAssetQueueWrapperMock.VerifyAll();
        _physicalAssetQueueWrapperMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpdateAniloxPhysicalAsset_Throws_InternalServiceException_When_UpdateAniloxPhysicalAssetRequest_Replies_With_Error()
    {
        // Arrange
        var updateAniloxPhysicalAssetRequest = new UpdateAniloxPhysicalAssetRequest(
            "physicalAssetId",
            serialNumber: "TZ123456789",
            manufacturer: "WuH",
            description: "Sample Anilox",
            deliveredAt: DateTime.MinValue,
            preferredUsageLocation: "EQ12345",
            initialUsageCounter: null,
            initialTimeUsageCounter: null,
            scanCodes: ["123456789", "987654321"],
            printWidth: 120.5,
            isSleeve: true,
            innerDiameter: 12.4,
            outerDiameter: 17.9,
            screen: 12469,
            engraving: "HEX 60",
            setVolumeValue: 40,
            setOpticalDensityValue: null
        );

        _physicalAssetQueueWrapperMock
            .Setup(m => m.SendUpdateAniloxPhysicalAssetRequestAndWaitForReply(
                It.IsAny<Messages.PhysicalAssetInformation.UpdateAniloxPhysicalAssetRequest>()))
            .ReturnsAsync(
                new InternalItemResponse<AniloxPhysicalAssetDto>(StatusCodes.Status500InternalServerError,
                    "Internal error"));

        // Act
        var updateAniloxPhysicalAssetAction = () =>
            _physicalAssetService.UpdateAniloxPhysicalAsset(updateAniloxPhysicalAssetRequest, UserId);

        // Assert
        await updateAniloxPhysicalAssetAction.Should().ThrowAsync<InternalServiceException>();

        _physicalAssetHttpClientMock.VerifyNoOtherCalls();

        _physicalAssetQueueWrapperMock.VerifyAll();
        _physicalAssetQueueWrapperMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(StatusCodes.Status400BadRequest)]
    [InlineData(StatusCodes.Status409Conflict)]
    public async Task UpdateAniloxPhysicalAsset_Throws_ParameterInvalidException_When_UpdateAniloxPhysicalAssetRequest_Replies_With_Specific_Error(int statusCode)
    {
        // Arrange
        var updateAniloxPhysicalAssetRequest = new UpdateAniloxPhysicalAssetRequest(
            "physicalAssetId",
            serialNumber: "TZ123456789",
            manufacturer: "WuH",
            description: "Sample Anilox",
            deliveredAt: DateTime.MinValue,
            preferredUsageLocation: "EQ12345",
            initialUsageCounter: null,
            initialTimeUsageCounter: null,
            scanCodes: ["123456789", "987654321"],
            printWidth: 120.5,
            isSleeve: true,
            innerDiameter: 12.4,
            outerDiameter: 17.9,
            screen: 12469,
            engraving: "HEX 60",
            setVolumeValue: 40,
            setOpticalDensityValue: null
        );

        _physicalAssetQueueWrapperMock
            .Setup(m => m.SendUpdateAniloxPhysicalAssetRequestAndWaitForReply(
                It.IsAny<Messages.PhysicalAssetInformation.UpdateAniloxPhysicalAssetRequest>()))
            .ReturnsAsync(new InternalItemResponse<AniloxPhysicalAssetDto>(statusCode, "Error"));

        // Act
        var updateAniloxPhysicalAssetAction = () =>
            _physicalAssetService.UpdateAniloxPhysicalAsset(updateAniloxPhysicalAssetRequest, UserId);

        // Assert
        await updateAniloxPhysicalAssetAction.Should().ThrowAsync<ParameterInvalidException>();

        _physicalAssetHttpClientMock.VerifyNoOtherCalls();

        _physicalAssetQueueWrapperMock.VerifyAll();
        _physicalAssetQueueWrapperMock.VerifyNoOtherCalls();
    }

    private static async Task AssertValueWithUnit<T>(
        FrameworkAPI.Schema.Misc.ValueWithUnit<T> framework, ValueWithUnit<T> client)
        where T : struct, IComparable, IComparable<T>, IConvertible, IEquatable<T>, IFormattable
    {
        var value = await framework.Value(CancellationToken.None);
        value.Should().BeEquivalentTo(client.Value);

        var unit = await framework.Unit(CancellationToken.None);
        unit.Should().BeEquivalentTo(client.Unit);
    }
}