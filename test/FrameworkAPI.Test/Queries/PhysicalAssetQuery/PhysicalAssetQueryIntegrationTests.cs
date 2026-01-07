using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.DataLoaders;
using FrameworkAPI.Schema.PhysicalAsset;
using FrameworkAPI.Schema.PhysicalAsset.CapabilityTest;
using FrameworkAPI.Schema.PhysicalAsset.Defect;
using FrameworkAPI.Schema.PhysicalAsset.History;
using FrameworkAPI.Services;
using FrameworkAPI.Services.Interfaces;
using FrameworkAPI.Test.Services.Helpers;
using FrameworkAPI.Test.TestHelpers;
using HotChocolate;
using HotChocolate.Execution;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PhysicalAssetDataHandler.Client.Extensions;
using PhysicalAssetDataHandler.Client.HttpClients;
using PhysicalAssetDataHandler.Client.Models;
using PhysicalAssetDataHandler.Client.Models.Dtos;
using PhysicalAssetDataHandler.Client.Models.Dtos.CapabilityTest;
using PhysicalAssetDataHandler.Client.Models.Dtos.Defect;
using PhysicalAssetDataHandler.Client.Models.Dtos.History;
using PhysicalAssetDataHandler.Client.Models.Enums;
using PhysicalAssetDataHandler.Client.QueueWrappers;
using Snapshooter.Xunit;
using WuH.Ruby.Common.Core;
using WuH.Ruby.LicenceManager.Client;
using WuH.Ruby.MachineDataHandler.Client;
using Xunit;
using DateTime = System.DateTime;

namespace FrameworkAPI.Test.Queries.PhysicalAssetQuery;

public class PhysicalAssetQueryIntegrationTests
{
    private readonly Mock<IPhysicalAssetSettingsHttpClient> _physicalAssetSettingsHttpClientMock = new();
    private readonly Mock<IPhysicalAssetHttpClient> _physicalAssetHttpClientMock = new();
    private readonly Mock<ICapabilityTestSpecificationHttpClient> _capabilityTestSpecificationHttpClientMock = new();
    private readonly Mock<IPhysicalAssetQueueWrapper> _physicalAssetQueueWrapperMock = new();
    private readonly Mock<IMachineCachingService> _machineCachingServiceMock = new();
    private readonly Mock<ILicenceManagerCachingService> _licenceManagerCachingServiceMock = new();
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

    [Fact]
    public async Task GetPhysicalAssetSettings_Should_Return_Physical_Asset_Settings()
    {
        // Arrange
        var executor = await InitializeExecutor();

        _physicalAssetSettingsHttpClientMock
            .Setup(mock => mock.GetPhysicalAssetSettings(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<PhysicalAssetSettingsDto>(new PhysicalAssetSettingsDto(
                aniloxCleaningInterval: new ValueWithUnit<int>(500_000, "m"))));

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                @"{
                    physicalAssetSettings {
                      cleaningInterval {
                        unit
                        value
                      }
                    }
                }")
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        _physicalAssetSettingsHttpClientMock.VerifyAll();
        _physicalAssetSettingsHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetPhysicalAssets_Without_Filter_Should_Return_Physical_Assets()
    {
        // Arrange
        var executor = await InitializeExecutor();

        var createdAt = new DateTime(year: 2018, month: 1, day: 15, hour: 12, minute: 15, second: 3, DateTimeKind.Utc);
        var lastChange = new DateTime(year: 2019, month: 1, day: 15, hour: 12, minute: 15, second: 3, DateTimeKind.Utc);
        var usageCounter = new PhysicalAssetUsageCounterDto(
            current: 0, atLastCleaning: null, sinceLastCleaning: 0, cleaningInterval: 500, cleaningIntervalExceeded: false, unit: "mm");
        var timeUsageCounter = new PhysicalAssetTimeUsageCounterDto(
            current: 0, atLastCleaning: null, sinceLastCleaning: null, unit: "ms");

        var physicalAssets = new List<PhysicalAssetDto>
        {
            new AniloxPhysicalAssetDto(
                physicalAssetId: "AniloxPhysicalAssetDto #1",
                createdAt,
                lastChange,
                serialNumber: "9876543210",
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
                equippedBy: new EquipmentDto(EquipmentType.Machine, equipmentId: "EQ12345", description: null),
                printWidth: new ValueWithUnit<double>(10_000, "mm"),
                innerDiameter: null,
                outerDiameter: new ValueWithUnit<double>(1_000.0, "mm"),
                screen: new ValueWithUnit<int>(30, "l/cm"),
                engraving: null,
                opticalDensity: new PhysicalAssetDataHandler.Client.Models.TestableValueWithUnit<double>(
                    setValue: null, measuredValue: null, measuredAt: null, unit: null),
                volume: new PhysicalAssetDataHandler.Client.Models.TestableValueWithUnit<double>(
                    setValue: 5.0, measuredValue: null, measuredAt: null, unit: "cm³/m²")),
            new PlatePhysicalAssetDto(
                physicalAssetId: "PlatePhysicalAssetDto #1",
                createdAt,
                lastChange,
                serialNumber: "9876543210",
                manufacturer: "Zecher",
                description: null,
                deliveredAt: null,
                preferredUsageLocation: "EQ12345",
                initialUsageCounter: null,
                initialTimeUsageCounter: null,
                scanCodes: [],
                usageCounter,
                timeUsageCounter,
                lastCleaning: null,
                lastConsumedMaterial: null,
                equippedBy: null,
                surface: null)
        };

        var machine = new Machine { MachineId = "EQ12345" };
        _machineCachingServiceMock
            .Setup(m => m.GetMachines(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync([machine])
            .Verifiable(Times.Once);
        _licenceManagerCachingServiceMock
            .Setup(m => m.GetDetailedLicenceValidity(machine.MachineId, Constants.LicensesApplications.Anilox, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<LicenceValidationInfo>(new LicenceValidationInfo { IsValid = true }))
            .Verifiable(Times.Once);

        _physicalAssetHttpClientMock
            .Setup(m => m.GetPhysicalAssets(PhysicalAssetsFilter.Utilisable, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalListResponse<PhysicalAssetDto>(physicalAssets))
            .Verifiable(Times.Once);

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                @"{
                    physicalAssets {
                      physicalAssetId
                      physicalAssetType
                      serialNumber,
                      scanCodes,
                      usageCounter {
                        current
                        atLastCleaning
                        unit
                      }
                      timeUsageCounter {
                        current
                        atLastCleaning
                        unit
                      }
                      equippedBy {
                        equipmentType
                        equipmentId
                        description
                      }
                    }
                }")
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        _machineCachingServiceMock.VerifyAll();
        _machineCachingServiceMock.VerifyNoOtherCalls();

        _licenceManagerCachingServiceMock.VerifyAll();
        _licenceManagerCachingServiceMock.VerifyNoOtherCalls();

        _physicalAssetHttpClientMock.VerifyAll();
        _physicalAssetHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetPhysicalAssets_With_Filter_But_Null_Value_Should_Return_Physical_Assets()
    {
        // Arrange
        var executor = await InitializeExecutor();

        var createdAt = new DateTime(year: 2018, month: 1, day: 15, hour: 12, minute: 15, second: 3, DateTimeKind.Utc);
        var lastChange = new DateTime(year: 2019, month: 1, day: 15, hour: 12, minute: 15, second: 3, DateTimeKind.Utc);
        var usageCounter = new PhysicalAssetUsageCounterDto(
            current: 0, atLastCleaning: null, sinceLastCleaning: 0, cleaningInterval: 500, cleaningIntervalExceeded: false, unit: "mm");
        var timeUsageCounter = new PhysicalAssetTimeUsageCounterDto(
            current: 0, atLastCleaning: null, sinceLastCleaning: null, unit: "ms");
        var physicalAssets = new List<PhysicalAssetDto>
        {
            new AniloxPhysicalAssetDto(
                physicalAssetId: "AniloxPhysicalAssetDto #1",
                createdAt,
                lastChange,
                serialNumber: "9876543210",
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
                printWidth: new ValueWithUnit<double>(10_000, "mm"),
                innerDiameter: null,
                outerDiameter: new ValueWithUnit<double>(1_000.0, "mm"),
                screen: new ValueWithUnit<int>(30, "l/cm"),
                engraving: null,
                opticalDensity: new PhysicalAssetDataHandler.Client.Models.TestableValueWithUnit<double>(
                    setValue: null, measuredValue: null, measuredAt: null, unit: null),
                volume: new PhysicalAssetDataHandler.Client.Models.TestableValueWithUnit<double>(
                    setValue: 5.0, measuredValue: null, measuredAt: null, unit: "cm³/m²"))
        };

        var machine = new Machine { MachineId = "EQ12345" };
        _machineCachingServiceMock
            .Setup(m => m.GetMachines(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync([machine])
            .Verifiable(Times.Once);
        _licenceManagerCachingServiceMock
            .Setup(m => m.GetDetailedLicenceValidity(machine.MachineId, Constants.LicensesApplications.Anilox, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<LicenceValidationInfo>(new LicenceValidationInfo { IsValid = true }))
            .Verifiable(Times.Once);

        _physicalAssetHttpClientMock
            .Setup(m => m.GetPhysicalAssets(PhysicalAssetsFilter.Utilisable, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalListResponse<PhysicalAssetDto>(physicalAssets))
            .Verifiable(Times.Once);

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                @"{
                    physicalAssets(
                    physicalAssetTypeFilter: null,
                    lastChangeFilter: null) {
                        physicalAssetId
                        physicalAssetType
                        serialNumber
                    }
                }")
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        _machineCachingServiceMock.VerifyAll();
        _machineCachingServiceMock.VerifyNoOtherCalls();

        _licenceManagerCachingServiceMock.VerifyAll();
        _licenceManagerCachingServiceMock.VerifyNoOtherCalls();

        _physicalAssetHttpClientMock.VerifyAll();
        _physicalAssetHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetPhysicalAssets_With_Filter_Should_Return_Physical_Assets()
    {
        // Arrange
        var executor = await InitializeExecutor();

        var createdAt = new DateTime(year: 2018, month: 1, day: 15, hour: 12, minute: 15, second: 3, DateTimeKind.Utc);
        var lastChange = new DateTime(year: 2019, month: 1, day: 15, hour: 12, minute: 15, second: 3, DateTimeKind.Utc);
        var usageCounter = new PhysicalAssetUsageCounterDto(
            current: 0, atLastCleaning: null, sinceLastCleaning: 0, cleaningInterval: 500, cleaningIntervalExceeded: false, unit: "mm");
        var timeUsageCounter = new PhysicalAssetTimeUsageCounterDto(
            current: 0, atLastCleaning: null, sinceLastCleaning: null, unit: "ms");
        var physicalAssets = new List<PhysicalAssetDto>
        {
            new AniloxPhysicalAssetDto(
                physicalAssetId: "AniloxPhysicalAssetDto #1",
                createdAt,
                lastChange,
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
                printWidth: new ValueWithUnit<double>(10_000, "mm"),
                innerDiameter: null,
                outerDiameter: new ValueWithUnit<double>(1_000.0, "mm"),
                screen: new ValueWithUnit<int>(30, "l/cm"),
                engraving: null,
                opticalDensity: new PhysicalAssetDataHandler.Client.Models.TestableValueWithUnit<double>(
                    setValue: 100.0, measuredValue: 98, measuredAt: DateTime.MinValue, unit: null),
                volume: new PhysicalAssetDataHandler.Client.Models.TestableValueWithUnit<double>(
                    setValue: 5.0, measuredValue: null, measuredAt: null, unit: "cm³/m²")),
            new AniloxPhysicalAssetDto(
                physicalAssetId: "AniloxPhysicalAssetDto #1",
                createdAt,
                lastChange,
                serialNumber: "9876543210",
                manufacturer: "Zecher",
                description: null,
                deliveredAt: null,
                preferredUsageLocation: "EQ12345",
                initialUsageCounter: null,
                initialTimeUsageCounter: null,
                scanCodes: [],
                usageCounter,
                timeUsageCounter,
                lastCleaning: null,
                lastConsumedMaterial: null,
                isSleeve: false,
                equippedBy: null,
                printWidth: new ValueWithUnit<double>(10_000, "mm"),
                innerDiameter: null,
                outerDiameter: new ValueWithUnit<double>(1_000.0, "mm"),
                screen: new ValueWithUnit<int>(30, "l/cm"),
                engraving: null,
                opticalDensity: new PhysicalAssetDataHandler.Client.Models.TestableValueWithUnit<double>(
                    setValue: 100.0, measuredValue: 98, measuredAt: DateTime.MinValue, unit: null),
                volume: new PhysicalAssetDataHandler.Client.Models.TestableValueWithUnit<double>(
                    setValue: 5.0, measuredValue: null, measuredAt: null, unit: "cm³/m²"))
        };

        var machine = new Machine { MachineId = "EQ12345" };
        _machineCachingServiceMock
            .Setup(m => m.GetMachines(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync([machine])
            .Verifiable(Times.Once);
        _licenceManagerCachingServiceMock
            .Setup(m => m.GetDetailedLicenceValidity(machine.MachineId, Constants.LicensesApplications.Anilox, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<LicenceValidationInfo>(new LicenceValidationInfo { IsValid = true }))
            .Verifiable(Times.Once);

        _physicalAssetHttpClientMock
            .Setup(m => m.GetPhysicalAssets(PhysicalAssetsFilter.Utilisable, PhysicalAssetType.Anilox, lastChange,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalListResponse<PhysicalAssetDto>(physicalAssets))
            .Verifiable(Times.Once);

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    physicalAssets(
                        physicalAssetsFilter: {PhysicalAssetsFilter.Utilisable.GetEnumMemberValue().ToUpper()},
                        physicalAssetTypeFilter: {PhysicalAssetType.Anilox.GetEnumMemberValue().ToUpper()},
                        lastChangeFilter: ""{lastChange.ToString("O", CultureInfo.InvariantCulture)}"") {{
                        physicalAssetId
                        physicalAssetType
                        serialNumber
                        }}
                }}")
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        _machineCachingServiceMock.VerifyAll();
        _machineCachingServiceMock.VerifyNoOtherCalls();

        _licenceManagerCachingServiceMock.VerifyAll();
        _licenceManagerCachingServiceMock.VerifyNoOtherCalls();

        _physicalAssetHttpClientMock.VerifyAll();
        _physicalAssetHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetPhysicalAssets_With_History_Should_Return_Physical_Asset_Defects()
    {
        // Arrange
        var executor = await InitializeExecutor();

        const string volumeUnit = "cm³/m²";
        var usageCounter = new PhysicalAssetUsageCounterDto(
            current: 0, atLastCleaning: null, sinceLastCleaning: 0, cleaningInterval: 500, cleaningIntervalExceeded: false, unit: "mm");
        var timeUsageCounter = new PhysicalAssetTimeUsageCounterDto(
            current: 0, atLastCleaning: null, sinceLastCleaning: null, unit: "ms");
        var physicalAssets = new List<PhysicalAssetDto>
        {
            new AniloxPhysicalAssetDto(
                physicalAssetId: "AniloxPhysicalAssetDto #1",
                createdAt: new DateTime(year: 2018, month: 1, day: 15, hour: 12, minute: 15, second: 3, DateTimeKind.Utc),
                lastChange: new DateTime(year: 2019, month: 1, day: 15, hour: 12, minute: 15, second: 3, DateTimeKind.Utc),
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
                printWidth: new ValueWithUnit<double>(10_000, "mm"),
                innerDiameter: null,
                outerDiameter: new ValueWithUnit<double>(1_000.0, "mm"),
                screen: new ValueWithUnit<int>(30, "l/cm"),
                engraving: null,
                opticalDensity: new PhysicalAssetDataHandler.Client.Models.TestableValueWithUnit<double>(
                    setValue: 100.0, measuredValue: 98, measuredAt: DateTime.MinValue, unit: null),
                volume: new PhysicalAssetDataHandler.Client.Models.TestableValueWithUnit<double>(
                    setValue: 5.0, measuredValue: null, measuredAt: null, volumeUnit)),
            new AniloxPhysicalAssetDto(
                physicalAssetId: "AniloxPhysicalAssetDto #2",
                createdAt: new DateTime(year: 2019, month: 1, day: 15, hour: 12, minute: 15, second: 3,
                    DateTimeKind.Utc),
                lastChange: new DateTime(year: 2020, month: 1, day: 15, hour: 12, minute: 15, second: 3,
                    DateTimeKind.Utc),
                serialNumber: "9876543210",
                manufacturer: "Zecher",
                description: null,
                deliveredAt: null,
                preferredUsageLocation: "EQ12345",
                initialUsageCounter: null,
                initialTimeUsageCounter: null,
                scanCodes: [],
                usageCounter,
                timeUsageCounter,
                lastCleaning: null,
                lastConsumedMaterial: null,
                isSleeve: false,
                equippedBy: null,
                printWidth: new ValueWithUnit<double>(10_000, "mm"),
                innerDiameter: null,
                outerDiameter: new ValueWithUnit<double>(1_000.0, "mm"),
                screen: new ValueWithUnit<int>(30, "l/cm"),
                engraving: null,
                opticalDensity: new PhysicalAssetDataHandler.Client.Models.TestableValueWithUnit<double>(
                    setValue: 100.0, measuredValue: 98, measuredAt: DateTime.MinValue, unit: null),
                volume: new PhysicalAssetDataHandler.Client.Models.TestableValueWithUnit<double>(
                    setValue: 5.0, measuredValue: null, measuredAt: null, volumeUnit))
        };
        var expectedPhysicalAssetHistories = physicalAssets.ToDictionary(
            physicalAsset => physicalAsset.PhysicalAssetId.ToString(),
            physicalAsset => new PhysicalAssetHistoryItemDto[]
            {
                new PhysicalAssetScrappedHistoryItemDto(
                    sourceId: "scrapped_EquipmentPhysicalAssetMapping_2",
                    note: "Not possible to repair anymore",
                    createdAt: physicalAsset.LastChange.AddMinutes(45)),
                new PhysicalAssetSurfaceAnomalyHistoryItemDto(
                    sourceId: "surfaceAnomaly_CapabilityTestResult_1",
                    note: null,
                    createdAt: physicalAsset.LastChange.AddMinutes(40),
                    startPosition: 10.0,
                    endPosition: 20.0,
                    unit: "mm",
                    aniloxCapabilityTestSpecificationDto: _aniloxCapabilityTestSpecificationDto),
                new PhysicalAssetScoringLineHistoryItemDto(
                    sourceId: "scoringLine_CapabilityTestResult_1",
                    note: null,
                    createdAt: physicalAsset.LastChange.AddMinutes(35),
                    position: 40.0,
                    unit: "mm",
                    aniloxCapabilityTestSpecificationDto: _aniloxCapabilityTestSpecificationDto),
                new PhysicalAssetVolumeTriggeredPrintAnomalyHistoryItemDto(
                    sourceId: "volumeTriggeredPrintAnomaly_CapabilityTestResult_1",
                    note: "Detected on Job #54321",
                    createdAt: physicalAsset.LastChange.AddMinutes(30),
                    startPosition: 1.0,
                    endPosition: 100.0,
                    unit: "mm",
                    aniloxCapabilityTestSpecificationDto: _aniloxCapabilityTestSpecificationDto),
                new PhysicalAssetCleanedHistoryItemDto(
                    sourceId: "cleaned_EquipmentPhysicalAssetMapping_1",
                    note: "Cleaned with care",
                    createdAt: physicalAsset.LastChange.AddMinutes(25),
                    CleaningOperationType.LaserCleaning,
                    resetVolumeDefects: false),
                new PhysicalAssetHighVolumeHistoryItemDto(
                    sourceId: "highVolume_CapabilityTestResult_1",
                    note: null,
                    createdAt: physicalAsset.LastChange.AddMinutes(20),
                    setValue: 5.0,
                    measuredValue: 10.0,
                    upperLimitValue: _volumeCapabilityTestSpecificationDto.VolumeDeviationUpperLimit!.Value,
                    lowerLimitValue: _volumeCapabilityTestSpecificationDto.VolumeDeviationLowerLimit,
                    volumeUnit,
                    volumeCapabilityTestSpecificationDto: _volumeCapabilityTestSpecificationDto),
                new PhysicalAssetLowVolumeHistoryItemDto(
                    sourceId: "lowVolume_CapabilityTestResult_1",
                    note: null,
                    createdAt: physicalAsset.LastChange.AddMinutes(15),
                    setValue: 5.0,
                    measuredValue: 4.0,
                    upperLimitValue: _volumeCapabilityTestSpecificationDto.VolumeDeviationUpperLimit!.Value,
                    lowerLimitValue: _volumeCapabilityTestSpecificationDto.VolumeDeviationLowerLimit,
                    volumeUnit,
                    volumeCapabilityTestSpecificationDto: _volumeCapabilityTestSpecificationDto),
                new PhysicalAssetVolumeMeasuredHistoryItemDto(
                    sourceId: "volumeMeasured_CapabilityTestResult_1",
                    note: null,
                    createdAt: physicalAsset.LastChange.AddMinutes(10),
                    setValue: 5.0,
                    measuredValue: 7.0,
                    upperLimitValue: _volumeCapabilityTestSpecificationDto.VolumeDeviationUpperLimit!.Value,
                    lowerLimitValue: _volumeCapabilityTestSpecificationDto.VolumeDeviationLowerLimit,
                    volumeUnit,
                    volumeCapabilityTestSpecificationDto: _volumeCapabilityTestSpecificationDto),
                new PhysicalAssetDeliveredHistoryItemDto(
                    sourceId: $"delivered_PhysicalAsset_{physicalAsset.PhysicalAssetId}",
                    note: null,
                    createdAt: physicalAsset.LastChange.AddMinutes(5)),
                new PhysicalAssetCreatedHistoryItemDto(
                    sourceId: $"created_PhysicalAsset_{physicalAsset.PhysicalAssetId}",
                    note: null,
                    createdAt: physicalAsset.LastChange)
            }.AsEnumerable());

        var machine = new Machine { MachineId = "EQ12345" };
        _machineCachingServiceMock
            .Setup(m => m.GetMachines(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync([machine])
            .Verifiable(Times.Once);
        _licenceManagerCachingServiceMock
            .Setup(m => m.GetDetailedLicenceValidity(machine.MachineId, Constants.LicensesApplications.Anilox, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<LicenceValidationInfo>(new LicenceValidationInfo { IsValid = true }))
            .Verifiable(Times.Once);

        _physicalAssetHttpClientMock
            .Setup(m => m.GetPhysicalAssets(PhysicalAssetsFilter.Utilisable, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalListResponse<PhysicalAssetDto>(physicalAssets))
            .Verifiable(Times.Once);
        _physicalAssetHttpClientMock
            .Setup(m => m.GetPhysicalAssetsHistory(It.IsAny<IEnumerable<string>>(), It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<IDictionary<string, IEnumerable<PhysicalAssetHistoryItemDto>>>(
                expectedPhysicalAssetHistories))
            .Verifiable(Times.Once);

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                @"{
                    physicalAssets { 
                    history {
                        physicalAssetHistoryItemType
                        sourceId
                        createdAt
                        note
                        ... on PhysicalAssetCreatedHistoryItem {
                          physicalAssetHistoryItemType
                          sourceId
                          createdAt
                          note
                        }
                        ... on PhysicalAssetDeliveredHistoryItem {
                          physicalAssetHistoryItemType
                          sourceId
                          createdAt
                          note
                        }
                        ... on PhysicalAssetHighVolumeHistoryItem {
                          physicalAssetHistoryItemType
                          sourceId
                          createdAt
                          setValue
                          measuredValue
                          upperLimitValue
                          lowerLimitValue
                          unit
                          note
                          volumeCapabilityTestSpecification {
                            capabilityTestSpecificationId
                            capabilityTestType
                            createdAt
                            description
                            isRelative
                            version
                            volumeDeviationLowerLimit
                            volumeDeviationUpperLimit
                          }
                        }
                        ... on PhysicalAssetLowVolumeHistoryItem {
                          physicalAssetHistoryItemType
                          sourceId
                          createdAt
                          setValue
                          measuredValue
                          upperLimitValue
                          lowerLimitValue
                          unit
                          note
                          volumeCapabilityTestSpecification {
                            capabilityTestSpecificationId
                            capabilityTestType
                            createdAt
                            description
                            isRelative
                            version
                            volumeDeviationLowerLimit
                            volumeDeviationUpperLimit
                          }
                        }
                        ... on PhysicalAssetVolumeMeasuredHistoryItem {
                            physicalAssetHistoryItemType
                            sourceId
                            createdAt
                            setValue
                            measuredValue
                            upperLimitValue
                            lowerLimitValue
                            unit
                            note
                            volumeCapabilityTestSpecification {
                              capabilityTestSpecificationId
                              capabilityTestType
                              createdAt
                              description
                              isRelative
                              version
                              volumeDeviationLowerLimit
                              volumeDeviationUpperLimit
                            }
                        }
                        ... on PhysicalAssetCleanedHistoryItem {
                          physicalAssetHistoryItemType
                          sourceId
                          createdAt
                          cleaningOperationType
                          resetVolumeDefects
                          note
                        }
                        ... on PhysicalAssetScrappedHistoryItem {
                          physicalAssetHistoryItemType
                          sourceId
                          createdAt
                          note
                        }
                        ... on PhysicalAssetScoringLineHistoryItem {
                          physicalAssetHistoryItemType
                          sourceId
                          createdAt
                          position
                          unit
                          note
                          aniloxCapabilityTestSpecification {
                            alwaysPass
                            capabilityTestSpecificationId
                            capabilityTestType
                            createdAt
                            description
                            version
                          }
                        }
                        ... on PhysicalAssetSurfaceAnomalyHistoryItem {
                          physicalAssetHistoryItemType
                          sourceId
                          createdAt
                          startPosition
                          endPosition
                          unit
                          note
                          aniloxCapabilityTestSpecification {
                            alwaysPass
                            capabilityTestSpecificationId
                            capabilityTestType
                            createdAt
                            description
                            version
                          }
                        }
                        ... on PhysicalAssetVolumeTriggeredPrintAnomalyHistoryItem {
                          physicalAssetHistoryItemType
                          sourceId
                          createdAt
                          startPosition
                          endPosition
                          unit
                          note
                          aniloxCapabilityTestSpecification {
                            alwaysPass
                            capabilityTestSpecificationId
                            capabilityTestType
                            createdAt
                            description
                            version
                          }
                        }
                      }
                    }
                }")
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        _machineCachingServiceMock.VerifyAll();
        _machineCachingServiceMock.VerifyNoOtherCalls();

        _licenceManagerCachingServiceMock.VerifyAll();
        _licenceManagerCachingServiceMock.VerifyNoOtherCalls();

        _physicalAssetHttpClientMock.VerifyAll();
        _physicalAssetHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetPhysicalAssets_With_Defects_Should_Return_Physical_Asset_Defects()
    {
        // Arrange
        var executor = await InitializeExecutor();

        const string volumeUnit = "cm³/m²";
        const string positionUnit = "mm";
        var usageCounter = new PhysicalAssetUsageCounterDto(
            current: 0, atLastCleaning: null, sinceLastCleaning: 0, cleaningInterval: 500, cleaningIntervalExceeded: false, unit: positionUnit);
        var timeUsageCounter = new PhysicalAssetTimeUsageCounterDto(
            current: 0, atLastCleaning: null, sinceLastCleaning: null, unit: "ms");
        var physicalAssets = new List<PhysicalAssetDto>
        {
            new AniloxPhysicalAssetDto(
                physicalAssetId: "AniloxPhysicalAssetDto #1",
                createdAt: new DateTime(year: 2018, month: 1, day: 15, hour: 12, minute: 15, second: 3,
                    DateTimeKind.Utc),
                lastChange: new DateTime(year: 2019, month: 1, day: 15, hour: 12, minute: 15, second: 3,
                    DateTimeKind.Utc),
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
                printWidth: new ValueWithUnit<double>(10_000, positionUnit),
                innerDiameter: null,
                outerDiameter: new ValueWithUnit<double>(1_000.0, positionUnit),
                screen: new ValueWithUnit<int>(30, "l/cm"),
                engraving: null,
                opticalDensity: new PhysicalAssetDataHandler.Client.Models.TestableValueWithUnit<double>(
                    setValue: 100.0, measuredValue: 98, measuredAt: DateTime.MinValue, unit: null),
                volume: new PhysicalAssetDataHandler.Client.Models.TestableValueWithUnit<double>(
                    setValue: 5.0, measuredValue: null, measuredAt: null, volumeUnit)),
            new AniloxPhysicalAssetDto(
                physicalAssetId: "AniloxPhysicalAssetDto #2",
                createdAt: new DateTime(year: 2019, month: 1, day: 15, hour: 12, minute: 15, second: 3,
                    DateTimeKind.Utc),
                lastChange: new DateTime(year: 2020, month: 1, day: 15, hour: 12, minute: 15, second: 3,
                    DateTimeKind.Utc),
                serialNumber: "9876543210",
                manufacturer: "Zecher",
                description: null,
                deliveredAt: null,
                preferredUsageLocation: "EQ12345",
                initialUsageCounter: null,
                initialTimeUsageCounter: null,
                scanCodes: [],
                usageCounter,
                timeUsageCounter,
                lastCleaning: null,
                lastConsumedMaterial: null,
                isSleeve: false,
                equippedBy: null,
                printWidth: new ValueWithUnit<double>(10_000, positionUnit),
                innerDiameter: null,
                outerDiameter: new ValueWithUnit<double>(1_000.0, positionUnit),
                screen: new ValueWithUnit<int>(30, "l/cm"),
                engraving: null,
                opticalDensity: new PhysicalAssetDataHandler.Client.Models.TestableValueWithUnit<double>(
                    setValue: 100.0, measuredValue: 98, measuredAt: DateTime.MinValue, unit: null),
                volume: new PhysicalAssetDataHandler.Client.Models.TestableValueWithUnit<double>(
                    setValue: 5.0, measuredValue: null, measuredAt: null, volumeUnit))
        };
        var expectedPhysicalAssetDefects = physicalAssets.ToDictionary(
            physicalAsset => physicalAsset.PhysicalAssetId.ToString(),
            physicalAsset => new PhysicalAssetDefectDto[]
            {
                new PhysicalAssetLowVolumeDefectDto(
                    sourceId: "lowVolume_CapabilityTestResult_1",
                    note: "note for lowVolume_CapabilityTestResult_1",
                    createdAt: physicalAsset.LastChange.AddMinutes(5),
                    setValue: 5.0,
                    measuredValue: 4.0,
                    upperLimitValue: _volumeCapabilityTestSpecificationDto.VolumeDeviationUpperLimit!.Value,
                    lowerLimitValue: _volumeCapabilityTestSpecificationDto.VolumeDeviationLowerLimit,
                    volumeUnit,
                    volumeCapabilityTestSpecificationDto: _volumeCapabilityTestSpecificationDto),
                new PhysicalAssetHighVolumeDefectDto(
                    sourceId: "highVolume_CapabilityTestResult_1",
                    note: "note for highVolume_CapabilityTestResult_1",
                    createdAt: physicalAsset.LastChange.AddMinutes(10),
                    setValue: 5.0,
                    measuredValue: 10.0,
                    upperLimitValue: _volumeCapabilityTestSpecificationDto.VolumeDeviationUpperLimit!.Value,
                    lowerLimitValue: _volumeCapabilityTestSpecificationDto.VolumeDeviationLowerLimit,
                    volumeUnit,
                    volumeCapabilityTestSpecificationDto: _volumeCapabilityTestSpecificationDto),
                new PhysicalAssetScoringLineDefectDto(
                    sourceId: "scoringLine_CapabilityTestResult_1",
                    note: "note for scoringLine_CapabilityTestResult_1",
                    createdAt: physicalAsset.LastChange.AddMinutes(15),
                    position: 50.0,
                    positionUnit,
                    aniloxCapabilityTestSpecificationDto: _aniloxCapabilityTestSpecificationDto),
                new PhysicalAssetSurfaceAnomalyDefectDto(
                    sourceId: "surfaceAnomaly_CapabilityTestResult_1",
                    note: "note for surfaceAnomaly_CapabilityTestResult_1",
                    createdAt: physicalAsset.LastChange.AddMinutes(20),
                    startPosition: 20.0,
                    endPosition: 30.0,
                    positionUnit,
                    aniloxCapabilityTestSpecificationDto: _aniloxCapabilityTestSpecificationDto)
            }.AsEnumerable());

        var machine = new Machine { MachineId = "EQ12345" };
        _machineCachingServiceMock
            .Setup(m => m.GetMachines(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync([machine])
            .Verifiable(Times.Once);
        _licenceManagerCachingServiceMock
            .Setup(m => m.GetDetailedLicenceValidity(machine.MachineId, Constants.LicensesApplications.Anilox, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<LicenceValidationInfo>(new LicenceValidationInfo { IsValid = true }))
            .Verifiable(Times.Once);

        _physicalAssetHttpClientMock
            .Setup(m => m.GetPhysicalAssets(PhysicalAssetsFilter.Utilisable, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalListResponse<PhysicalAssetDto>(physicalAssets))
            .Verifiable(Times.Once);
        _physicalAssetHttpClientMock
            .Setup(m => m.GetPhysicalAssetsDefects(It.IsAny<IEnumerable<string>>(), It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<IDictionary<string, IEnumerable<PhysicalAssetDefectDto>>>(
                expectedPhysicalAssetDefects))
            .Verifiable(Times.Once);

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                @"{
                    physicalAssets { 
                    defects {
                        physicalAssetDefectType
                          sourceId
                          createdAt
                        ... on PhysicalAssetLowVolumeDefect {
                          physicalAssetDefectType
                          sourceId
                          createdAt
                          setValue
                          measuredValue
                          upperLimitValue
                          lowerLimitValue
                          unit
                          volumeCapabilityTestSpecification {
                            capabilityTestSpecificationId
                            capabilityTestType
                            createdAt
                            description
                            isRelative
                            version
                            volumeDeviationLowerLimit
                            volumeDeviationUpperLimit
                          }
                        }
                        ... on PhysicalAssetHighVolumeDefect {
                          physicalAssetDefectType
                          sourceId
                          createdAt
                          setValue
                          measuredValue
                          upperLimitValue
                          lowerLimitValue
                          unit
                          volumeCapabilityTestSpecification {
                            capabilityTestSpecificationId
                            capabilityTestType
                            createdAt
                            description
                            isRelative
                            version
                            volumeDeviationLowerLimit
                            volumeDeviationUpperLimit
                          }
                        }
                        ... on PhysicalAssetScoringLineDefect {
                          physicalAssetDefectType
                          sourceId
                          createdAt
                          position
                          unit
                          aniloxCapabilityTestSpecification {
                            alwaysPass
                            capabilityTestSpecificationId
                            capabilityTestType
                            createdAt
                            description
                            version
                          }
                        }
                        ... on PhysicalAssetSurfaceAnomalyDefect {
                          physicalAssetDefectType
                          sourceId
                          createdAt
                          startPosition
                          endPosition
                          unit
                          aniloxCapabilityTestSpecification {
                            alwaysPass
                            capabilityTestSpecificationId
                            capabilityTestType
                            createdAt
                            description
                            version
                          }
                        }
                        ... on PhysicalAssetVolumeTriggeredPrintAnomalyDefect {
                          physicalAssetDefectType
                          sourceId
                          createdAt
                          startPosition
                          endPosition
                          unit
                          aniloxCapabilityTestSpecification {
                            alwaysPass
                            capabilityTestSpecificationId
                            capabilityTestType
                            createdAt
                            description
                            version
                          }
                        }
                      }
                    }
                }")
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        _machineCachingServiceMock.VerifyAll();
        _machineCachingServiceMock.VerifyNoOtherCalls();

        _licenceManagerCachingServiceMock.VerifyAll();
        _licenceManagerCachingServiceMock.VerifyNoOtherCalls();

        _physicalAssetHttpClientMock.VerifyAll();
        _physicalAssetHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetPhysicalAsset_Should_Return_Physical_Asset()
    {
        // Arrange
        var executor = await InitializeExecutor();

        const string physicalAssetId = "AniloxPhysicalAssetDto #1";
        var createdAt = new DateTime(year: 2017, month: 1, day: 15, hour: 12, minute: 15, second: 3, DateTimeKind.Utc);
        var lastChange = new DateTime(year: 2019, month: 1, day: 15, hour: 12, minute: 15, second: 3, DateTimeKind.Utc);
        var usageCounter = new PhysicalAssetUsageCounterDto(
            current: 0, atLastCleaning: null, sinceLastCleaning: 0, cleaningInterval: 500, cleaningIntervalExceeded: false, unit: "mm");
        var timeUsageCounter = new PhysicalAssetTimeUsageCounterDto(
            current: 0, atLastCleaning: null, sinceLastCleaning: null, unit: "ms");
        var physicalAsset = new AniloxPhysicalAssetDto(
            physicalAssetId: "AniloxPhysicalAssetDto #1",
            createdAt,
            lastChange,
            serialNumber: "9876543210",
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
            equippedBy: new EquipmentDto(EquipmentType.Machine, equipmentId: "EQ12345", description: "Machine 12345"),
            printWidth: new ValueWithUnit<double>(10_000, "mm"),
            innerDiameter: null,
            outerDiameter: new ValueWithUnit<double>(1_000.0, "mm"),
            screen: new ValueWithUnit<int>(30, "l/cm"),
            engraving: null,
            opticalDensity: new PhysicalAssetDataHandler.Client.Models.TestableValueWithUnit<double>(
                setValue: null, measuredValue: null, measuredAt: null, unit: null),
            volume: new PhysicalAssetDataHandler.Client.Models.TestableValueWithUnit<double>(
                setValue: 5.0, measuredValue: null, measuredAt: null, unit: "cm³/m²"));

        var machine = new Machine { MachineId = "EQ12345" };
        _machineCachingServiceMock
            .Setup(m => m.GetMachines(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync([machine])
            .Verifiable(Times.Once);
        _licenceManagerCachingServiceMock
            .Setup(m => m.GetDetailedLicenceValidity(machine.MachineId, Constants.LicensesApplications.Anilox, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<LicenceValidationInfo>(new LicenceValidationInfo { IsValid = true }))
            .Verifiable(Times.Once);

        _physicalAssetHttpClientMock
            .Setup(m => m.GetPhysicalAsset(physicalAssetId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<PhysicalAssetDto>(physicalAsset))
            .Verifiable(Times.Once);

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    physicalAsset(physicalAssetId: ""{physicalAssetId}"") {{
                      physicalAssetId
                      physicalAssetType
                      serialNumber,
                      scanCodes,
                      usageCounter {{
                        current
                        atLastCleaning
                        unit
                      }}
                      timeUsageCounter {{
                        current
                        atLastCleaning
                        unit
                      }}
                      equippedBy {{
                        equipmentType
                        equipmentId
                        description
                      }}
                    }}
                }}")
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        _machineCachingServiceMock.VerifyAll();
        _machineCachingServiceMock.VerifyNoOtherCalls();

        _licenceManagerCachingServiceMock.VerifyAll();
        _licenceManagerCachingServiceMock.VerifyNoOtherCalls();

        _physicalAssetHttpClientMock.VerifyAll();
        _physicalAssetHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetPhysicalAsset_Should_Return_Anilox_Physical_Asset()
    {
        // Arrange
        var executor = await InitializeExecutor();

        const string physicalAssetId = "AniloxPhysicalAssetDto #1";
        var createdAt = new DateTime(year: 2017, month: 1, day: 15, hour: 12, minute: 15, second: 3, DateTimeKind.Utc);
        var lastChange = new DateTime(year: 2019, month: 1, day: 15, hour: 12, minute: 15, second: 3, DateTimeKind.Utc);
        var measuredAt = new DateTime(year: 2018, month: 1, day: 15, hour: 12, minute: 15, second: 3, DateTimeKind.Utc);
        var usageCounter = new PhysicalAssetUsageCounterDto(
            current: 200, atLastCleaning: 75, sinceLastCleaning: 125, cleaningInterval: 100, cleaningIntervalExceeded: true, unit: "mm");
        var timeUsageCounter = new PhysicalAssetTimeUsageCounterDto(
            current: 100, atLastCleaning: 60, sinceLastCleaning: 40, unit: "ms");
        var physicalAsset = new AniloxPhysicalAssetDto(
            physicalAssetId,
            createdAt,
            lastChange,
            serialNumber: "0123456789",
            manufacturer: "Zecher",
            description: null,
            deliveredAt: null,
            preferredUsageLocation: "EQ12345",
            initialUsageCounter: null,
            initialTimeUsageCounter: null,
            scanCodes: ["123456789"],
            usageCounter,
            timeUsageCounter,
            lastCleaning: null,
            lastConsumedMaterial: null,
            isSleeve: false,
            equippedBy: null,
            printWidth: new ValueWithUnit<double>(10_000, "mm"),
            innerDiameter: null,
            outerDiameter: new ValueWithUnit<double>(1_000.0, "mm"),
            screen: new ValueWithUnit<int>(300, "L/cm"),
            engraving: "HEX 60°",
            opticalDensity: new PhysicalAssetDataHandler.Client.Models.TestableValueWithUnit<double>(
                setValue: null, measuredValue: null, measuredAt: measuredAt, unit: null),
            volume: new PhysicalAssetDataHandler.Client.Models.TestableValueWithUnit<double>(
                setValue: 5.0, measuredValue: null, measuredAt: measuredAt, unit: "cm³/m²"));

        var machine = new Machine { MachineId = "EQ12345" };
        _machineCachingServiceMock
            .Setup(m => m.GetMachines(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync([machine])
            .Verifiable(Times.Once);
        _licenceManagerCachingServiceMock
            .Setup(m => m.GetDetailedLicenceValidity(machine.MachineId, Constants.LicensesApplications.Anilox, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<LicenceValidationInfo>(new LicenceValidationInfo { IsValid = true }))
            .Verifiable(Times.Once);

        _physicalAssetHttpClientMock
            .Setup(m => m.GetPhysicalAsset(physicalAssetId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<PhysicalAssetDto>(physicalAsset))
            .Verifiable(Times.Once);

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    physicalAsset(physicalAssetId: ""{physicalAssetId}"") {{
                        physicalAssetId
                        physicalAssetType
                        serialNumber
                        scanCodes,
                        ... on AniloxPhysicalAsset {{
                        engraving
                        screen {{
                            value
                            unit
                        }}
                        opticalDensity {{
                            setValue
                            measuredValue
                            measuredAt
                            unit
                        }}
                        volume {{
                            setValue
                            measuredValue
                            measuredAt
                            unit
                        }}
                        usageCounter {{
                            atLastCleaning
                            current
                            unit
                            cleaningInterval
                            cleaningIntervalExceeded
                            sinceLastCleaning
                        }}
                        timeUsageCounter {{
                            atLastCleaning
                            current
                            unit
                            sinceLastCleaning
                        }}
                      }}
                    }}
                }}")
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        _machineCachingServiceMock.VerifyAll();
        _machineCachingServiceMock.VerifyNoOtherCalls();

        _licenceManagerCachingServiceMock.VerifyAll();
        _licenceManagerCachingServiceMock.VerifyNoOtherCalls();

        _physicalAssetHttpClientMock.VerifyAll();
        _physicalAssetHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetPhysicalAssetHistory_Should_Return_History()
    {
        // Arrange
        var executor = await InitializeExecutor();

        const string physicalAssetId = "AniloxPhysicalAssetDto #1";
        const string volumeUnit = "cm³/m²";
        var createdAt = new DateTime(year: 2017, month: 1, day: 15, hour: 12, minute: 15, second: 3, DateTimeKind.Utc);
        var lastChange = new DateTime(year: 2019, month: 1, day: 15, hour: 12, minute: 15, second: 3, DateTimeKind.Utc);
        var measuredAt = new DateTime(year: 2018, month: 1, day: 15, hour: 12, minute: 15, second: 3, DateTimeKind.Utc);
        var usageCounter = new PhysicalAssetUsageCounterDto(
            current: 0, atLastCleaning: null, sinceLastCleaning: 0, cleaningInterval: 500, cleaningIntervalExceeded: false, unit: "mm");
        var timeUsageCounter = new PhysicalAssetTimeUsageCounterDto(
            current: 0, atLastCleaning: null, sinceLastCleaning: null, unit: "ms");

        var physicalAsset = new AniloxPhysicalAssetDto(
            physicalAssetId,
            createdAt,
            lastChange,
            serialNumber: "9876543210",
            manufacturer: "Zecher",
            description: null,
            deliveredAt: null,
            preferredUsageLocation: "EQ12345",
            initialUsageCounter: null,
            initialTimeUsageCounter: null,
            scanCodes: [],
            usageCounter,
            timeUsageCounter,
            lastCleaning: null,
            lastConsumedMaterial: null,
            isSleeve: false,
            equippedBy: null,
            printWidth: new ValueWithUnit<double>(10_000, "mm"),
            innerDiameter: null,
            outerDiameter: new ValueWithUnit<double>(1_000.0, "mm"),
            screen: new ValueWithUnit<int>(300, "l/cm"),
            engraving: null,
            opticalDensity: new PhysicalAssetDataHandler.Client.Models.TestableValueWithUnit<double>(
                setValue: 100.0, measuredValue: 98, measuredAt, unit: null),
            volume: new PhysicalAssetDataHandler.Client.Models.TestableValueWithUnit<double>(
                setValue: 5.0, measuredValue: null, measuredAt: null, volumeUnit));

        var expectedPhysicalAssetHistory = new List<PhysicalAssetHistoryItemDto>
        {
            new PhysicalAssetScrappedHistoryItemDto(
                sourceId: "scrapped_EquipmentPhysicalAssetMapping_2",
                note: "Not possible to repair anymore",
                createdAt: physicalAsset.LastChange.AddMinutes(45)),
            new PhysicalAssetSurfaceAnomalyHistoryItemDto(
                sourceId: "surfaceAnomaly_CapabilityTestResult_1",
                note: null,
                createdAt: physicalAsset.LastChange.AddMinutes(40),
                startPosition: 10.0,
                endPosition: 20.0,
                unit: "mm",
                aniloxCapabilityTestSpecificationDto: _aniloxCapabilityTestSpecificationDto),
            new PhysicalAssetScoringLineHistoryItemDto(
                sourceId: "scoringLine_CapabilityTestResult_1",
                note: null,
                createdAt: physicalAsset.LastChange.AddMinutes(35),
                position: 40.0,
                unit: "mm",
                aniloxCapabilityTestSpecificationDto: _aniloxCapabilityTestSpecificationDto),
            new PhysicalAssetVolumeTriggeredPrintAnomalyHistoryItemDto(
                sourceId: "volumeTriggeredPrintAnomaly_CapabilityTestResult_1",
                note: "Detected on Job #54321",
                createdAt: physicalAsset.LastChange.AddMinutes(30),
                startPosition: 1.0,
                endPosition: 100.0,
                unit: "mm",
                aniloxCapabilityTestSpecificationDto: _aniloxCapabilityTestSpecificationDto),
            new PhysicalAssetCleanedHistoryItemDto(
                sourceId: "cleaned_EquipmentPhysicalAssetMapping_1",
                note: "Cleaned with care",
                createdAt: physicalAsset.LastChange.AddMinutes(25),
                CleaningOperationType.LaserCleaning,
                resetVolumeDefects: false),
            new PhysicalAssetHighVolumeHistoryItemDto(
                sourceId: "highVolume_CapabilityTestResult_1",
                note: null,
                createdAt: physicalAsset.LastChange.AddMinutes(20),
                setValue: 5.0,
                measuredValue: 10.0,
                upperLimitValue: _volumeCapabilityTestSpecificationDto.VolumeDeviationUpperLimit!.Value,
                lowerLimitValue: _volumeCapabilityTestSpecificationDto.VolumeDeviationLowerLimit,
                volumeUnit,
                volumeCapabilityTestSpecificationDto: _volumeCapabilityTestSpecificationDto),
            new PhysicalAssetLowVolumeHistoryItemDto(
                sourceId: "lowVolume_CapabilityTestResult_1",
                note: null,
                createdAt: physicalAsset.LastChange.AddMinutes(15),
                setValue: 5.0,
                measuredValue: 4.0,
                upperLimitValue: _volumeCapabilityTestSpecificationDto.VolumeDeviationUpperLimit!.Value,
                lowerLimitValue: _volumeCapabilityTestSpecificationDto.VolumeDeviationLowerLimit,
                volumeUnit,
                volumeCapabilityTestSpecificationDto: _volumeCapabilityTestSpecificationDto),
            new PhysicalAssetVolumeMeasuredHistoryItemDto(
                sourceId: "volumeMeasured_CapabilityTestResult_1",
                note: null,
                createdAt: physicalAsset.LastChange.AddMinutes(10),
                setValue: 5.0,
                measuredValue: 7.0,
                upperLimitValue: _volumeCapabilityTestSpecificationDto.VolumeDeviationUpperLimit!.Value,
                lowerLimitValue: _volumeCapabilityTestSpecificationDto.VolumeDeviationLowerLimit,
                volumeUnit,
                volumeCapabilityTestSpecificationDto: _volumeCapabilityTestSpecificationDto),
            new PhysicalAssetDeliveredHistoryItemDto(
                sourceId: $"delivered_PhysicalAsset_{physicalAsset.PhysicalAssetId}",
                note: null,
                createdAt: physicalAsset.LastChange.AddMinutes(5)),
            new PhysicalAssetCreatedHistoryItemDto(
                sourceId: $"created_PhysicalAsset_{physicalAsset.PhysicalAssetId}",
                note: null,
                createdAt: physicalAsset.LastChange)
        };

        var machine = new Machine { MachineId = "EQ12345" };
        _machineCachingServiceMock
            .Setup(m => m.GetMachines(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync([machine])
            .Verifiable(Times.Once);
        _licenceManagerCachingServiceMock
            .Setup(m => m.GetDetailedLicenceValidity(machine.MachineId, Constants.LicensesApplications.Anilox, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<LicenceValidationInfo>(new LicenceValidationInfo { IsValid = true }))
            .Verifiable(Times.Once);

        _physicalAssetHttpClientMock
            .Setup(m => m.GetPhysicalAsset(physicalAssetId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<PhysicalAssetDto>(physicalAsset))
            .Verifiable(Times.Once);
        _physicalAssetHttpClientMock
            .Setup(m => m.GetPhysicalAssetHistory(physicalAssetId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalListResponse<PhysicalAssetHistoryItemDto>(expectedPhysicalAssetHistory))
            .Verifiable(Times.Once);

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    physicalAsset(physicalAssetId: ""{physicalAssetId}"") {{
                        history {{
                        physicalAssetHistoryItemType
                        sourceId
                        createdAt
                        note
                        ... on PhysicalAssetCreatedHistoryItem {{
                            physicalAssetHistoryItemType
                            sourceId
                            createdAt
                            note
                        }}
                        ... on PhysicalAssetDeliveredHistoryItem {{
                            physicalAssetHistoryItemType
                            sourceId
                            createdAt
                            note
                        }}
                        ... on PhysicalAssetHighVolumeHistoryItem {{
                            physicalAssetHistoryItemType
                            sourceId
                            createdAt
                            setValue
                            measuredValue
                            upperLimitValue
                            lowerLimitValue
                            unit
                            note
                            volumeCapabilityTestSpecification {{
                              capabilityTestSpecificationId
                              capabilityTestType
                              createdAt
                              description
                              isRelative
                              version
                              volumeDeviationLowerLimit
                              volumeDeviationUpperLimit
                            }}
                        }}
                        ... on PhysicalAssetLowVolumeHistoryItem {{
                            physicalAssetHistoryItemType
                            sourceId
                            createdAt
                            setValue
                            measuredValue
                            upperLimitValue
                            lowerLimitValue
                            unit
                            note
                            volumeCapabilityTestSpecification {{
                              capabilityTestSpecificationId
                              capabilityTestType
                              createdAt
                              description
                              isRelative
                              version
                              volumeDeviationLowerLimit
                              volumeDeviationUpperLimit
                            }}
                        }}
                        ... on PhysicalAssetVolumeMeasuredHistoryItem {{
                            physicalAssetHistoryItemType
                            sourceId
                            createdAt
                            setValue
                            measuredValue
                            upperLimitValue
                            lowerLimitValue
                            unit
                            note
                            volumeCapabilityTestSpecification {{
                              capabilityTestSpecificationId
                              capabilityTestType
                              createdAt
                              description
                              isRelative
                              version
                              volumeDeviationLowerLimit
                              volumeDeviationUpperLimit
                            }}
                        }}
                        ... on PhysicalAssetCleanedHistoryItem {{
                            physicalAssetHistoryItemType
                            sourceId
                            createdAt
                            cleaningOperationType
                            resetVolumeDefects
                            note
                        }}
                        ... on PhysicalAssetScrappedHistoryItem {{
                            physicalAssetHistoryItemType
                            sourceId
                            createdAt
                            note
                        }}
                        ... on PhysicalAssetScoringLineHistoryItem {{
                            physicalAssetHistoryItemType
                            sourceId
                            createdAt
                            position
                            unit
                            note
                            aniloxCapabilityTestSpecification {{
                              alwaysPass
                              capabilityTestSpecificationId
                              capabilityTestType
                              createdAt
                              description
                              version
                            }}
                        }}
                        ... on PhysicalAssetSurfaceAnomalyHistoryItem {{
                            physicalAssetHistoryItemType
                            sourceId
                            createdAt
                            startPosition
                            endPosition
                            unit
                            note
                            aniloxCapabilityTestSpecification {{
                              alwaysPass
                              capabilityTestSpecificationId
                              capabilityTestType
                              createdAt
                              description
                              version
                            }}
                        }}
                        ... on PhysicalAssetVolumeTriggeredPrintAnomalyHistoryItem {{
                            physicalAssetHistoryItemType
                            sourceId
                            createdAt
                            startPosition
                            endPosition
                            unit
                            note
                            aniloxCapabilityTestSpecification {{
                              alwaysPass
                              capabilityTestSpecificationId
                              capabilityTestType
                              createdAt
                              description
                              version
                            }}
                        }}
                      }}
                    }}
                }}")
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        _machineCachingServiceMock.VerifyAll();
        _machineCachingServiceMock.VerifyNoOtherCalls();

        _licenceManagerCachingServiceMock.VerifyAll();
        _licenceManagerCachingServiceMock.VerifyNoOtherCalls();

        _physicalAssetHttpClientMock.VerifyAll();
        _physicalAssetHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetPhysicalAssetDefects_Should_Return_Defects()
    {
        // Arrange
        var executor = await InitializeExecutor();

        const string physicalAssetId = "AniloxPhysicalAssetDto #1";
        const string volumeUnit = "cm³/m²";
        const string positionUnit = "mm";
        var createdAt = new DateTime(year: 2017, month: 1, day: 15, hour: 12, minute: 15, second: 3, DateTimeKind.Utc);
        var lastChange = new DateTime(year: 2019, month: 1, day: 15, hour: 12, minute: 15, second: 3, DateTimeKind.Utc);
        var measuredAt = new DateTime(year: 2018, month: 1, day: 15, hour: 12, minute: 15, second: 3, DateTimeKind.Utc);
        var usageCounter = new PhysicalAssetUsageCounterDto(
            current: 0, atLastCleaning: null, sinceLastCleaning: 0, cleaningInterval: 500, cleaningIntervalExceeded: false, unit: "mm");
        var timeUsageCounter = new PhysicalAssetTimeUsageCounterDto(current: 0, atLastCleaning: null, sinceLastCleaning: null, unit: "ms");
        var physicalAsset = new AniloxPhysicalAssetDto(
            physicalAssetId,
            createdAt,
            lastChange,
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
            printWidth: new ValueWithUnit<double>(10_000, "mm"),
            innerDiameter: null,
            outerDiameter: new ValueWithUnit<double>(1_000.0, "mm"),
            screen: new ValueWithUnit<int>(300, "l/cm"),
            engraving: null,
            opticalDensity: new PhysicalAssetDataHandler.Client.Models.TestableValueWithUnit<double>(
                setValue: 100.0, measuredValue: 98, measuredAt, unit: null),
            volume: new PhysicalAssetDataHandler.Client.Models.TestableValueWithUnit<double>(
                setValue: 5.0, measuredValue: null, measuredAt: null, volumeUnit));

        var expectedPhysicalAssetDefects = new List<PhysicalAssetDefectDto>
        {
            new PhysicalAssetLowVolumeDefectDto(
                sourceId: "lowVolume_CapabilityTestResult_1",
                note: "note for lowVolume_CapabilityTestResult_1",
                createdAt: lastChange.AddMinutes(5),
                setValue: 5.0,
                measuredValue: 4.0,
                upperLimitValue: _volumeCapabilityTestSpecificationDto.VolumeDeviationUpperLimit!.Value,
                lowerLimitValue: _volumeCapabilityTestSpecificationDto.VolumeDeviationLowerLimit,
                volumeUnit,
                volumeCapabilityTestSpecificationDto: _volumeCapabilityTestSpecificationDto),
            new PhysicalAssetHighVolumeDefectDto(
                sourceId: "highVolume_CapabilityTestResult_1",
                note: "note for highVolume_CapabilityTestResult_1",
                createdAt: physicalAsset.LastChange.AddMinutes(10),
                setValue: 5.0,
                measuredValue: 10.0,
                upperLimitValue: _volumeCapabilityTestSpecificationDto.VolumeDeviationUpperLimit!.Value,
                lowerLimitValue: _volumeCapabilityTestSpecificationDto.VolumeDeviationLowerLimit,
                volumeUnit,
                volumeCapabilityTestSpecificationDto: _volumeCapabilityTestSpecificationDto),
            new PhysicalAssetScoringLineDefectDto(
                sourceId: "scoringLine_CapabilityTestResult_1",
                note: "note for scoringLine_CapabilityTestResult_1",
                createdAt: physicalAsset.LastChange.AddMinutes(15),
                position: 50.0,
                positionUnit,
                aniloxCapabilityTestSpecificationDto: _aniloxCapabilityTestSpecificationDto),
            new PhysicalAssetSurfaceAnomalyDefectDto(
                sourceId: "surfaceAnomaly_CapabilityTestResult_1",
                note: "note for surfaceAnomaly_CapabilityTestResult_1",
                createdAt: physicalAsset.LastChange.AddMinutes(20),
                startPosition: 20.0,
                endPosition: 30.0,
                positionUnit,
                aniloxCapabilityTestSpecificationDto: _aniloxCapabilityTestSpecificationDto),
            new PhysicalAssetVolumeTriggeredPrintAnomalyDefectDto(
                sourceId: "volumeTriggeredPrintAnomaly_CapabilityTestResult_1",
                note: null,
                createdAt: physicalAsset.LastChange.AddMinutes(20),
                startPosition: 20.0,
                endPosition: 30.0,
                positionUnit,
                aniloxCapabilityTestSpecificationDto: _aniloxCapabilityTestSpecificationDto)
        };

        var machine = new Machine { MachineId = "EQ12345" };
        _machineCachingServiceMock
            .Setup(m => m.GetMachines(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync([machine])
            .Verifiable(Times.Once);
        _licenceManagerCachingServiceMock
            .Setup(m => m.GetDetailedLicenceValidity(machine.MachineId, Constants.LicensesApplications.Anilox, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<LicenceValidationInfo>(new LicenceValidationInfo { IsValid = true }))
            .Verifiable(Times.Once);

        _physicalAssetHttpClientMock
            .Setup(m => m.GetPhysicalAsset(physicalAssetId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<PhysicalAssetDto>(physicalAsset))
            .Verifiable(Times.Once);
        _physicalAssetHttpClientMock
            .Setup(m => m.GetPhysicalAssetDefects(physicalAssetId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalListResponse<PhysicalAssetDefectDto>(expectedPhysicalAssetDefects))
            .Verifiable(Times.Once);

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    physicalAsset(physicalAssetId: ""{physicalAssetId}"") {{
                      defects {{
                        physicalAssetDefectType
                        sourceId
                        createdAt
                        ... on PhysicalAssetLowVolumeDefect {{
                          physicalAssetDefectType
                          sourceId
                          createdAt
                          setValue
                          measuredValue
                          upperLimitValue
                          lowerLimitValue
                          unit
                          volumeCapabilityTestSpecification {{
                            capabilityTestSpecificationId
                            capabilityTestType
                            createdAt
                            description
                            isRelative
                            version
                            volumeDeviationLowerLimit
                            volumeDeviationUpperLimit
                          }}
                        }}
                        ... on PhysicalAssetHighVolumeDefect {{
                          physicalAssetDefectType
                          sourceId
                          createdAt
                          setValue
                          measuredValue
                          upperLimitValue
                          lowerLimitValue
                          unit
                          volumeCapabilityTestSpecification {{
                            capabilityTestSpecificationId
                            capabilityTestType
                            createdAt
                            description
                            isRelative
                            version
                            volumeDeviationLowerLimit
                            volumeDeviationUpperLimit
                          }}
                        }}
                        ... on PhysicalAssetScoringLineDefect {{
                          physicalAssetDefectType
                          sourceId
                          createdAt
                          position
                          unit
                          aniloxCapabilityTestSpecification {{
                            alwaysPass
                            capabilityTestSpecificationId
                            capabilityTestType
                            createdAt
                            description
                            version
                          }}
                        }}
                        ... on PhysicalAssetSurfaceAnomalyDefect {{
                          physicalAssetDefectType
                          sourceId
                          createdAt
                          startPosition
                          endPosition
                          unit
                          aniloxCapabilityTestSpecification {{
                            alwaysPass
                            capabilityTestSpecificationId
                            capabilityTestType
                            createdAt
                            description
                            version
                          }}
                        }}
                        ... on PhysicalAssetVolumeTriggeredPrintAnomalyDefect {{
                          physicalAssetDefectType
                          sourceId
                          createdAt
                          startPosition
                          endPosition
                          unit
                          aniloxCapabilityTestSpecification {{
                            alwaysPass
                            capabilityTestSpecificationId
                            capabilityTestType
                            createdAt
                            description
                            version
                          }}
                        }}
                      }}
                    }}
                }}")
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);
        // Assert
        result.ToJson().MatchSnapshot();

        _machineCachingServiceMock.VerifyAll();
        _machineCachingServiceMock.VerifyNoOtherCalls();

        _licenceManagerCachingServiceMock.VerifyAll();
        _licenceManagerCachingServiceMock.VerifyNoOtherCalls();

        _physicalAssetHttpClientMock.VerifyAll();
        _physicalAssetHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task
        GetPhysicalAssetCapabilityTestSpecifications_Should_Return_Capability_Test_Specifications()
    {
        // Arrange
        var executor = await InitializeExecutor();

        var createdAt =
            new DateTime(year: 2019, month: 1, day: 15, hour: 12, minute: 15, second: 3, DateTimeKind.Utc);
        var expectedCapabilityTestSpecifications = new List<CapabilityTestSpecificationDto>
        {
            new VolumeCapabilityTestSpecificationDto(
                capabilityTestSpecificationId: "VolumeCapabilityTestSpecificationDto #1",
                version: 1,
                description: "VolumeCapabilityTestSpecification #1",
                createdAt,
                isRelative: true,
                volumeDeviationUpperLimit: 5,
                volumeDeviationLowerLimit: 15),
            new VisualCapabilityTestSpecificationDto(
                capabilityTestSpecificationId: "VisualCapabilityTestSpecificationDto #1",
                version: 2,
                description: "VisualCapabilityTestSpecificationDto #1",
                createdAt)
        };

        var machine = new Machine { MachineId = "EQ12345" };
        _machineCachingServiceMock
            .Setup(m => m.GetMachines(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync([machine])
            .Verifiable(Times.Once);
        _licenceManagerCachingServiceMock
            .Setup(m => m.GetDetailedLicenceValidity(machine.MachineId, Constants.LicensesApplications.Anilox, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<LicenceValidationInfo>(new LicenceValidationInfo { IsValid = true }))
            .Verifiable(Times.Once);

        _capabilityTestSpecificationHttpClientMock
            .Setup(m => m.GetCurrentVersions(It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new InternalListResponse<CapabilityTestSpecificationDto>(expectedCapabilityTestSpecifications))
            .Verifiable(Times.Once);

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                @"{
                    physicalAssetCapabilityTestSpecifications {
                      capabilityTestType
                      capabilityTestSpecificationId
                      version
                    }
                }")
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        _machineCachingServiceMock.VerifyAll();
        _machineCachingServiceMock.VerifyNoOtherCalls();

        _licenceManagerCachingServiceMock.VerifyAll();
        _licenceManagerCachingServiceMock.VerifyNoOtherCalls();

        _capabilityTestSpecificationHttpClientMock.VerifyAll();
        _capabilityTestSpecificationHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetPhysicalAssetCapabilityTestSpecification_Should_Return_Capability_Test_Specification()
    {
        // Arrange
        var executor = await InitializeExecutor();

        var createdAt =
            new DateTime(year: 2019, month: 1, day: 15, hour: 12, minute: 15, second: 3, DateTimeKind.Utc);
        var expectedCapabilityTestSpecification = new VisualCapabilityTestSpecificationDto(
            capabilityTestSpecificationId: "VisualCapabilityTestSpecificationDto #1",
            version: 1,
            description: "VisualCapabilityTestSpecificationDto #1",
            createdAt);

        var machine = new Machine { MachineId = "EQ12345" };
        _machineCachingServiceMock
            .Setup(m => m.GetMachines(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync([machine])
            .Verifiable(Times.Once);
        _licenceManagerCachingServiceMock
            .Setup(m => m.GetDetailedLicenceValidity(machine.MachineId, Constants.LicensesApplications.Anilox, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<LicenceValidationInfo>(new LicenceValidationInfo { IsValid = true }))
            .Verifiable(Times.Once);

        _capabilityTestSpecificationHttpClientMock
            .Setup(m => m.GetCurrentVersion(
                CapabilityTestType.Visual, It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new InternalItemResponse<CapabilityTestSpecificationDto>(expectedCapabilityTestSpecification))
            .Verifiable(Times.Once);

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                @"{
                    physicalAssetCapabilityTestSpecification(capabilityTestType: VISUAL) {
                      capabilityTestType
                      capabilityTestSpecificationId
                      version
                    }
                }")
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        _machineCachingServiceMock.VerifyAll();
        _machineCachingServiceMock.VerifyNoOtherCalls();

        _licenceManagerCachingServiceMock.VerifyAll();
        _licenceManagerCachingServiceMock.VerifyNoOtherCalls();

        _capabilityTestSpecificationHttpClientMock.VerifyAll();
        _capabilityTestSpecificationHttpClientMock.VerifyNoOtherCalls();
    }

    private async Task<IRequestExecutor> InitializeExecutor()
    {
        var physicalAssetService = new PhysicalAssetService(
            _physicalAssetSettingsHttpClientMock.Object,
            _physicalAssetHttpClientMock.Object,
            _physicalAssetQueueWrapperMock.Object);
        var physicalAssetCapabilityTestSpecificationService = new PhysicalAssetCapabilityTestSpecificationService(
            _capabilityTestSpecificationHttpClientMock.Object);

        var licenceService = new LicenceService(
            _machineCachingServiceMock.Object, _licenceManagerCachingServiceMock.Object);

        var delayedBatchScheduler = new DelayedBatchScheduler();
        var physicalAssetHistoryBatchDataLoader =
            new PhysicalAssetHistoryBatchDataLoader(_physicalAssetHttpClientMock.Object, delayedBatchScheduler);
        var physicalAssetDefectsBatchDataLoader =
            new PhysicalAssetDefectsBatchDataLoader(_physicalAssetHttpClientMock.Object, delayedBatchScheduler);

        return await new ServiceCollection()
            .AddSingleton<ILicenceService>(licenceService)
            .AddSingleton<IPhysicalAssetService>(physicalAssetService)
            .AddSingleton<IPhysicalAssetCapabilityTestSpecificationService>(physicalAssetCapabilityTestSpecificationService)
            .AddSingleton(physicalAssetHistoryBatchDataLoader)
            .AddSingleton(physicalAssetDefectsBatchDataLoader)
            .AddLogging()
            .AddGraphQLServer()
            .AddAuthorization()
            .ModifyOptions(t => t.StrictValidation = false)
            .AddQueryType(q => q.Name("Query"))
            .AddType<FrameworkAPI.Queries.PhysicalAssetQuery>()
            .AddType<PhysicalAssetSettings>()
            .AddType<PhysicalAsset>()
            .AddType<AniloxPhysicalAsset>()
            .AddType<PlatePhysicalAsset>()
            .AddType<Equipment>()
            .AddType<CapabilityTestSpecification>()
            .AddType<VolumeCapabilityTestSpecification>()
            .AddType<OpticalDensityCapabilityTestSpecification>()
            .AddType<AniloxCapabilityTestSpecification>()
            .AddType<VisualCapabilityTestSpecification>()
            .AddType<PhysicalAssetCreatedHistoryItem>()
            .AddType<PhysicalAssetDeliveredHistoryItem>()
            .AddType<PhysicalAssetHighVolumeHistoryItem>()
            .AddType<PhysicalAssetLowVolumeHistoryItem>()
            .AddType<PhysicalAssetVolumeMeasuredHistoryItem>()
            .AddType<PhysicalAssetCleanedHistoryItem>()
            .AddType<PhysicalAssetScrappedHistoryItem>()
            .AddType<PhysicalAssetScoringLineHistoryItem>()
            .AddType<PhysicalAssetSurfaceAnomalyHistoryItem>()
            .AddType<PhysicalAssetVolumeTriggeredPrintAnomalyHistoryItem>()
            .AddType<PhysicalAssetHighVolumeDefect>()
            .AddType<PhysicalAssetLowVolumeDefect>()
            .AddType<PhysicalAssetScoringLineDefect>()
            .AddType<PhysicalAssetSurfaceAnomalyDefect>()
            .AddType<PhysicalAssetVolumeTriggeredPrintAnomalyDefect>()
            .AddSorting()
            .AddFiltering()
            .BuildRequestExecutorAsync();
    }
}