using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FrameworkAPI.Interceptors;
using FrameworkAPI.Mutations;
using FrameworkAPI.Services;
using FrameworkAPI.Services.Interfaces;
using FrameworkAPI.Test.TestHelpers;
using HotChocolate;
using HotChocolate.Execution;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using PhysicalAssetDataHandler.Client.HttpClients;
using PhysicalAssetDataHandler.Client.Models;
using PhysicalAssetDataHandler.Client.Models.Dtos;
using PhysicalAssetDataHandler.Client.Models.Dtos.CapabilityTest;
using PhysicalAssetDataHandler.Client.Models.Dtos.Operation;
using PhysicalAssetDataHandler.Client.Models.Enums;
using PhysicalAssetDataHandler.Client.Models.Messages;
using PhysicalAssetDataHandler.Client.QueueWrappers;
using Snapshooter.Xunit;
using WuH.Ruby.Common.Core;
using WuH.Ruby.LicenceManager.Client;
using WuH.Ruby.MachineDataHandler.Client;
using Xunit;
using PhysicalAssetInformation = PhysicalAssetDataHandler.Client.Models.Messages.PhysicalAssetInformation;

namespace FrameworkAPI.Test.Mutations;

public class PhysicalAssetsMutationIntegrationTests
{
    private const string UserId = "test-user-id";
    private const string PhysicalAssetId = "65577c04e51181aa1bdcc90f";
    private const string SerialNumber = "123456789";
    private const string Manufacturer = "WuH";
    private const string Engraving = "HEX 60°";
    private const double PrintWidth = 12.5;
    private const double SetVolumeValue = 50.5;
    private const double OuterDiameter = 10.0;
    private const int Screen = 30;

    private readonly Mock<IPhysicalAssetSettingsHttpClient> _physicalAssetSettingsHttpClientMock = new();
    private readonly Mock<IPhysicalAssetHttpClient> _physicalAssetHttpClientMock = new();
    private readonly Mock<IPhysicalAssetQueueWrapper> _physicalAssetQueueWrapperMock = new();
    private readonly Mock<IMachineCachingService> _machineCachingServiceMock = new();
    private readonly Mock<ILicenceManagerCachingService> _licenceManagerCachingServiceMock = new();

    [Fact]
    public async Task Update_PhysicalAsset_Settings_Calls_Client_With_Request_And_Returns_Value()
    {
        // Arrange
        var executor = await InitializeExecutor();

        var machine = new Machine { MachineId = "EQ12345" };
        _machineCachingServiceMock
            .Setup(m => m.GetMachines(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync([machine])
            .Verifiable(Times.Once);
        _licenceManagerCachingServiceMock
            .Setup(m => m.GetDetailedLicenceValidity(machine.MachineId, Constants.LicensesApplications.Anilox, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<LicenceValidationInfo>(new LicenceValidationInfo { IsValid = true }))
            .Verifiable(Times.Once);

        var updatePhysicalAssetSettingsRequest = new UpdatePhysicalAssetSettingsRequest(
            UserId,
            aniloxCleaningIntervalInMeter: 1_000);

        var expectedPhysicalAssetSettingsDto = new PhysicalAssetSettingsDto(
            aniloxCleaningInterval: new ValueWithUnit<int>(updatePhysicalAssetSettingsRequest.AniloxCleaningIntervalInMeter, "m"));

        UpdatePhysicalAssetSettingsRequest? capturedUpdatePhysicalAssetSettingsRequestMessage = null;
        _physicalAssetQueueWrapperMock
            .Setup(m => m.SendUpdatePhysicalAssetSettingsRequestAndWaitForReply(
                It.IsAny<UpdatePhysicalAssetSettingsRequest>()))
            .Callback<UpdatePhysicalAssetSettingsRequest>(
                request => capturedUpdatePhysicalAssetSettingsRequestMessage = request)
            .ReturnsAsync(new InternalItemResponse<PhysicalAssetSettingsDto>(expectedPhysicalAssetSettingsDto))
            .Verifiable(Times.Once);

        var query = PhysicalAssetUpdateSettingsMutation(updatePhysicalAssetSettingsRequest.AniloxCleaningIntervalInMeter);

        var request = QueryRequestBuilder
            .New()
            .SetQuery(query)
            .AddRoleClaims("go-general")
            .AddGlobalState("userId", UserId)
            .Create();

        // Act
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        capturedUpdatePhysicalAssetSettingsRequestMessage.Should().BeEquivalentTo(updatePhysicalAssetSettingsRequest);

        _physicalAssetHttpClientMock.VerifyNoOtherCalls();

        _physicalAssetQueueWrapperMock.VerifyAll();
        _physicalAssetQueueWrapperMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Create_Anilox_Physical_Asset_Calls_Client_With_Request_And_Returns_Value()
    {
        // Arrange
        var executor = await InitializeExecutor();

        var machine = new Machine { MachineId = "EQ12345" };
        _machineCachingServiceMock
            .Setup(m => m.GetMachines(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync([machine])
            .Verifiable(Times.Once);
        _licenceManagerCachingServiceMock
            .Setup(m => m.GetDetailedLicenceValidity(machine.MachineId, Constants.LicensesApplications.Anilox, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<LicenceValidationInfo>(new LicenceValidationInfo { IsValid = true }))
            .Verifiable(Times.Once);

        var utcNow = DateTime.UtcNow;
        var usageCounter = new PhysicalAssetUsageCounterDto(
            current: 0, atLastCleaning: null, sinceLastCleaning: 0, cleaningInterval: 500, cleaningIntervalExceeded: false, unit: "mm");
        var timeUsageCounter = new PhysicalAssetTimeUsageCounterDto(
            current: 0, atLastCleaning: null, sinceLastCleaning: null, unit: "ms");
        var aniloxPhysicalAssetDto = new AniloxPhysicalAssetDto(
            PhysicalAssetId,
            createdAt: utcNow,
            lastChange: utcNow.AddMinutes(5),
            SerialNumber,
            Manufacturer,
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
            equippedBy: null,
            isSleeve: false,
            printWidth: new ValueWithUnit<double>(PrintWidth, "mm"),
            innerDiameter: null,
            outerDiameter: new ValueWithUnit<double>(OuterDiameter, "mm"),
            screen: new ValueWithUnit<int>(Screen, "l/cm"),
            Engraving,
            volume: new TestableValueWithUnit<double>(
                setValue: SetVolumeValue, measuredValue: 98, measuredAt: DateTime.UtcNow, unit: "cm³/m²"),
            opticalDensity: new TestableValueWithUnit<double>(
                setValue: null, measuredValue: null, measuredAt: null, unit: null));
        var createAniloxPhysicalAssetRequest =
            new PhysicalAssetInformation.CreateAniloxPhysicalAssetRequest(
                UserId,
                aniloxPhysicalAssetDto.SerialNumber,
                aniloxPhysicalAssetDto.Manufacturer,
                aniloxPhysicalAssetDto.Description,
                aniloxPhysicalAssetDto.DeliveredAt,
                aniloxPhysicalAssetDto.PreferredUsageLocation,
                aniloxPhysicalAssetDto.InitialUsageCounter,
                aniloxPhysicalAssetDto.InitialTimeUsageCounter,
                aniloxPhysicalAssetDto.ScanCodes,
                aniloxPhysicalAssetDto.PrintWidth.Value,
                aniloxPhysicalAssetDto.IsSleeve,
                aniloxPhysicalAssetDto.InnerDiameter?.Value,
                aniloxPhysicalAssetDto.OuterDiameter.Value,
                aniloxPhysicalAssetDto.Screen.Value,
                aniloxPhysicalAssetDto.Engraving,
                setVolumeValue: aniloxPhysicalAssetDto.Volume.SetValue!.Value,
                aniloxPhysicalAssetDto.OpticalDensity.SetValue,
                measuredVolumeValue: 125.9);

        PhysicalAssetInformation.CreateAniloxPhysicalAssetRequest? capturedCreateAniloxPhysicalAssetRequest = null;
        _physicalAssetQueueWrapperMock
            .Setup(m => m.SendCreateAniloxPhysicalAssetRequestAndWaitForReply(
                It.IsAny<PhysicalAssetInformation.CreateAniloxPhysicalAssetRequest>()))
            .Callback<PhysicalAssetInformation.CreateAniloxPhysicalAssetRequest>(
                request => capturedCreateAniloxPhysicalAssetRequest = request)
            .ReturnsAsync(new InternalItemResponse<AniloxPhysicalAssetDto>(aniloxPhysicalAssetDto))
            .Verifiable(Times.Once);

        var query = PhysicalAssetsCreateAniloxMutation(
            createAniloxPhysicalAssetRequest.SerialNumber,
            createAniloxPhysicalAssetRequest.Manufacturer,
            createAniloxPhysicalAssetRequest.PreferredUsageLocation,
            createAniloxPhysicalAssetRequest.ScanCodes,
            createAniloxPhysicalAssetRequest.PrintWidth,
            createAniloxPhysicalAssetRequest.OuterDiameter,
            createAniloxPhysicalAssetRequest.Screen,
            createAniloxPhysicalAssetRequest.Engraving,
            createAniloxPhysicalAssetRequest.SetVolumeValue,
            createAniloxPhysicalAssetRequest.MeasuredVolumeValue);

        var request = QueryRequestBuilder
            .New()
            .SetQuery(query)
            .AddRoleClaims("go-general")
            .AddGlobalState("userId", UserId)
            .Create();

        // Act
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        capturedCreateAniloxPhysicalAssetRequest.Should().BeEquivalentTo(createAniloxPhysicalAssetRequest);

        _physicalAssetHttpClientMock.VerifyNoOtherCalls();

        _physicalAssetQueueWrapperMock.VerifyAll();
        _physicalAssetQueueWrapperMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Create_Anilox_Physical_Asset_With_Null_Serial_Number_Returns_Error()
    {
        // Arrange
        var executor = await InitializeExecutor();

        var machine = new Machine { MachineId = "EQ12345" };
        _machineCachingServiceMock
            .Setup(m => m.GetMachines(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync([machine])
            .Verifiable(Times.Once);
        _licenceManagerCachingServiceMock
            .Setup(m => m.GetDetailedLicenceValidity(machine.MachineId, Constants.LicensesApplications.Anilox, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<LicenceValidationInfo>(new LicenceValidationInfo { IsValid = true }))
            .Verifiable(Times.Once);

        var query = PhysicalAssetsCreateAniloxMutation(serialNumber: null);

        var request = QueryRequestBuilder
            .New()
            .SetQuery(query)
            .AddRoleClaims("go-general")
            .AddGlobalState("userId", UserId)
            .Create();

        // Act
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        _physicalAssetHttpClientMock.VerifyNoOtherCalls();

        _physicalAssetQueueWrapperMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Create_Anilox_Physical_Asset_With_Already_Existing_Serial_Number_Returns_Error()
    {
        // Arrange
        var executor = await InitializeExecutor();

        var machine = new Machine { MachineId = "EQ12345" };
        _machineCachingServiceMock
            .Setup(m => m.GetMachines(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync([machine])
            .Verifiable(Times.Once);
        _licenceManagerCachingServiceMock
            .Setup(m => m.GetDetailedLicenceValidity(machine.MachineId, Constants.LicensesApplications.Anilox, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<LicenceValidationInfo>(new LicenceValidationInfo { IsValid = true }))
            .Verifiable(Times.Once);

        _physicalAssetQueueWrapperMock
            .Setup(m => m.SendCreateAniloxPhysicalAssetRequestAndWaitForReply(
                It.IsAny<PhysicalAssetInformation.CreateAniloxPhysicalAssetRequest>()))
            .ReturnsAsync(
                new InternalItemResponse<AniloxPhysicalAssetDto>(StatusCodes.Status409Conflict, "Already existing"))
            .Verifiable(Times.Once);

        var query = PhysicalAssetsCreateAniloxMutation(SerialNumber);

        var request = QueryRequestBuilder
            .New()
            .SetQuery(query)
            .AddRoleClaims("go-general")
            .AddGlobalState("userId", UserId)
            .Create();

        // Act
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        _physicalAssetHttpClientMock.VerifyNoOtherCalls();

        _physicalAssetQueueWrapperMock.VerifyAll();
        _physicalAssetQueueWrapperMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Update_Anilox_Physical_Asset_Calls_Client_With_Request_And_Returns_Value()
    {
        // Arrange
        var executor = await InitializeExecutor();

        var machine = new Machine { MachineId = "EQ12345" };
        _machineCachingServiceMock
            .Setup(m => m.GetMachines(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync([machine])
            .Verifiable(Times.Once);
        _licenceManagerCachingServiceMock
            .Setup(m => m.GetDetailedLicenceValidity(machine.MachineId, Constants.LicensesApplications.Anilox, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<LicenceValidationInfo>(new LicenceValidationInfo { IsValid = true }))
            .Verifiable(Times.Once);

        var utcNow = DateTime.UtcNow;
        var usageCounter = new PhysicalAssetUsageCounterDto(
            current: 0, atLastCleaning: null, sinceLastCleaning: 0, cleaningInterval: 500, cleaningIntervalExceeded: false, unit: "mm");
        var timeUsageCounter = new PhysicalAssetTimeUsageCounterDto(
            current: 0, atLastCleaning: null, sinceLastCleaning: null, unit: "ms");
        var aniloxPhysicalAssetDto = new AniloxPhysicalAssetDto(
            PhysicalAssetId,
            createdAt: utcNow,
            lastChange: utcNow.AddMinutes(5),
            SerialNumber,
            Manufacturer,
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
            equippedBy: null,
            isSleeve: false,
            printWidth: new ValueWithUnit<double>(PrintWidth, "mm"),
            innerDiameter: null,
            outerDiameter: new ValueWithUnit<double>(OuterDiameter, "mm"),
            screen: new ValueWithUnit<int>(Screen, "l/cm"),
            Engraving,
            volume: new TestableValueWithUnit<double>(
                setValue: SetVolumeValue, measuredValue: 98, measuredAt: DateTime.UtcNow, unit: "cm³/m²"),
            opticalDensity: new TestableValueWithUnit<double>(
                setValue: null, measuredValue: null, measuredAt: null, unit: null));
        var updateAniloxPhysicalAssetRequest =
            new PhysicalAssetInformation.UpdateAniloxPhysicalAssetRequest(
                UserId,
                aniloxPhysicalAssetDto.PhysicalAssetId,
                aniloxPhysicalAssetDto.SerialNumber,
                aniloxPhysicalAssetDto.Manufacturer,
                aniloxPhysicalAssetDto.Description,
                aniloxPhysicalAssetDto.DeliveredAt,
                aniloxPhysicalAssetDto.PreferredUsageLocation,
                aniloxPhysicalAssetDto.InitialUsageCounter,
                aniloxPhysicalAssetDto.InitialTimeUsageCounter,
                aniloxPhysicalAssetDto.ScanCodes,
                aniloxPhysicalAssetDto.PrintWidth.Value,
                aniloxPhysicalAssetDto.IsSleeve,
                aniloxPhysicalAssetDto.InnerDiameter?.Value,
                aniloxPhysicalAssetDto.OuterDiameter.Value,
                aniloxPhysicalAssetDto.Screen.Value,
                aniloxPhysicalAssetDto.Engraving,
                setVolumeValue: aniloxPhysicalAssetDto.Volume.SetValue!.Value,
                aniloxPhysicalAssetDto.OpticalDensity.SetValue);

        PhysicalAssetInformation.UpdateAniloxPhysicalAssetRequest? capturedUpdateAniloxPhysicalAssetRequest = null;
        _physicalAssetQueueWrapperMock
            .Setup(m => m.SendUpdateAniloxPhysicalAssetRequestAndWaitForReply(
                It.IsAny<PhysicalAssetInformation.UpdateAniloxPhysicalAssetRequest>()))
            .Callback<PhysicalAssetInformation.UpdateAniloxPhysicalAssetRequest>(
                request => capturedUpdateAniloxPhysicalAssetRequest = request)
            .ReturnsAsync(new InternalItemResponse<AniloxPhysicalAssetDto>(aniloxPhysicalAssetDto))
            .Verifiable(Times.Once);

        var query = PhysicalAssetsUpdateAniloxMutation(
            updateAniloxPhysicalAssetRequest.PhysicalAssetId,
            updateAniloxPhysicalAssetRequest.SerialNumber,
            updateAniloxPhysicalAssetRequest.Manufacturer,
            updateAniloxPhysicalAssetRequest.ScanCodes,
            updateAniloxPhysicalAssetRequest.PrintWidth,
            updateAniloxPhysicalAssetRequest.SetVolumeValue,
            updateAniloxPhysicalAssetRequest.OuterDiameter,
            updateAniloxPhysicalAssetRequest.Screen,
            updateAniloxPhysicalAssetRequest.PreferredUsageLocation,
            updateAniloxPhysicalAssetRequest.Engraving);

        var request = QueryRequestBuilder
            .New()
            .SetQuery(query)
            .AddRoleClaims("go-general")
            .AddGlobalState("userId", UserId)
            .Create();

        // Act
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        capturedUpdateAniloxPhysicalAssetRequest.Should().BeEquivalentTo(updateAniloxPhysicalAssetRequest);

        _physicalAssetHttpClientMock.VerifyNoOtherCalls();

        _physicalAssetQueueWrapperMock.VerifyAll();
        _physicalAssetQueueWrapperMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Update_Anilox_Physical_Asset_With_Not_Existing_Id_Returns_Error()
    {
        // Arrange
        var executor = await InitializeExecutor();

        var machine = new Machine { MachineId = "EQ12345" };
        _machineCachingServiceMock
            .Setup(m => m.GetMachines(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync([machine])
            .Verifiable(Times.Once);
        _licenceManagerCachingServiceMock
            .Setup(m => m.GetDetailedLicenceValidity(machine.MachineId, Constants.LicensesApplications.Anilox, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<LicenceValidationInfo>(new LicenceValidationInfo { IsValid = true }))
            .Verifiable(Times.Once);

        _physicalAssetQueueWrapperMock
            .Setup(m => m.SendUpdateAniloxPhysicalAssetRequestAndWaitForReply(
                It.IsAny<PhysicalAssetInformation.UpdateAniloxPhysicalAssetRequest>()))
            .ReturnsAsync(
                new InternalItemResponse<AniloxPhysicalAssetDto>(StatusCodes.Status400BadRequest, "Does not exist"))
            .Verifiable(Times.Once);

        var query = PhysicalAssetsUpdateAniloxMutation(PhysicalAssetId, SerialNumber);

        var request = QueryRequestBuilder
            .New()
            .SetQuery(query)
            .AddRoleClaims("go-general")
            .AddGlobalState("userId", UserId)
            .Create();

        // Act
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        _physicalAssetHttpClientMock.VerifyNoOtherCalls();

        _physicalAssetQueueWrapperMock.VerifyAll();
        _physicalAssetQueueWrapperMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Update_Anilox_Physical_Asset_With_Null_Serial_Number_Returns_Error()
    {
        // Arrange
        var executor = await InitializeExecutor();

        var machine = new Machine { MachineId = "EQ12345" };
        _machineCachingServiceMock
            .Setup(m => m.GetMachines(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync([machine])
            .Verifiable(Times.Once);
        _licenceManagerCachingServiceMock
            .Setup(m => m.GetDetailedLicenceValidity(machine.MachineId, Constants.LicensesApplications.Anilox, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<LicenceValidationInfo>(new LicenceValidationInfo { IsValid = true }))
            .Verifiable(Times.Once);

        var query = PhysicalAssetsUpdateAniloxMutation(PhysicalAssetId, serialNumber: null);

        var request = QueryRequestBuilder
            .New()
            .SetQuery(query)
            .AddRoleClaims("go-general")
            .AddGlobalState("userId", UserId)
            .Create();

        // Act
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        _physicalAssetHttpClientMock.VerifyNoOtherCalls();

        _physicalAssetQueueWrapperMock.VerifyAll();
        _physicalAssetQueueWrapperMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Update_Anilox_Physical_Asset_With_Already_Existing_Serial_Number_On_Other_Asset_Returns_Error()
    {
        // Arrange
        var executor = await InitializeExecutor();

        var machine = new Machine { MachineId = "EQ12345" };
        _machineCachingServiceMock
            .Setup(m => m.GetMachines(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync([machine])
            .Verifiable(Times.Once);
        _licenceManagerCachingServiceMock
            .Setup(m => m.GetDetailedLicenceValidity(machine.MachineId, Constants.LicensesApplications.Anilox, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<LicenceValidationInfo>(new LicenceValidationInfo { IsValid = true }))
            .Verifiable(Times.Once);

        _physicalAssetQueueWrapperMock
            .Setup(m => m.SendUpdateAniloxPhysicalAssetRequestAndWaitForReply(
                It.IsAny<PhysicalAssetInformation.UpdateAniloxPhysicalAssetRequest>()))
            .ReturnsAsync(new InternalItemResponse<AniloxPhysicalAssetDto>(
                StatusCodes.Status409Conflict, "Serial number already exists on other asset"))
            .Verifiable(Times.Once);

        var query = PhysicalAssetsUpdateAniloxMutation(PhysicalAssetId, SerialNumber);

        var request = QueryRequestBuilder
            .New()
            .SetQuery(query)
            .AddRoleClaims("go-general")
            .AddGlobalState("userId", UserId)
            .Create();

        // Act
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        _physicalAssetHttpClientMock.VerifyNoOtherCalls();

        _physicalAssetQueueWrapperMock.VerifyAll();
        _physicalAssetQueueWrapperMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Create_Anilox_Capability_Test_Result_Calls_Client_With_Request_And_Returns_Value()
    {
        // Arrange
        var executor = await InitializeExecutor();

        var machine = new Machine { MachineId = "EQ12345" };
        _machineCachingServiceMock
            .Setup(m => m.GetMachines(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync([machine])
            .Verifiable(Times.Once);
        _licenceManagerCachingServiceMock
            .Setup(m => m.GetDetailedLicenceValidity(machine.MachineId, Constants.LicensesApplications.Anilox, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<LicenceValidationInfo>(new LicenceValidationInfo { IsValid = true }))
            .Verifiable(Times.Once);

        var expectedAniloxCapabilityTestResultDto = new AniloxCapabilityTestResultDto(
            capabilityTestResultId: "65b3693056068ff18b8d1a7f",
            capabilityTestSpecificationId: "65b8b4ed9a37742358b8ef5c",
            PhysicalAssetId,
            testDateTime: new DateTime(year: 2023, month: 2, day: 1, hour: 12, minute: 30, second: 0, DateTimeKind.Utc),
            testerUserId: UserId,
            note: null,
            isPassed: true,
            AniloxCapabilityErrorType.ScoringLine,
            startPositionOnAnilox: 200.0,
            endPositionOnAnilox: null);

        var createAniloxCapabilityTestResultRequest =
            new PhysicalAssetInformation.CapabilityTest.CreateAniloxCapabilityTestResultRequest(
                expectedAniloxCapabilityTestResultDto.TesterUserId,
                expectedAniloxCapabilityTestResultDto.PhysicalAssetId,
                expectedAniloxCapabilityTestResultDto.TestDateTime,
                expectedAniloxCapabilityTestResultDto.Note,
                expectedAniloxCapabilityTestResultDto.AniloxCapabilityErrorType,
                expectedAniloxCapabilityTestResultDto.StartPositionOnAnilox,
                expectedAniloxCapabilityTestResultDto.EndPositionOnAnilox);

        PhysicalAssetInformation.CapabilityTest.CreateAniloxCapabilityTestResultRequest?
            capturedCreateAniloxCapabilityTestResultRequestMessage = null;
        _physicalAssetQueueWrapperMock
            .Setup(m => m.SendCreateAniloxCapabilityTestResultAndWaitForReply(
                It.IsAny<PhysicalAssetInformation.CapabilityTest.CreateAniloxCapabilityTestResultRequest>()))
            .Callback<PhysicalAssetInformation.CapabilityTest.CreateAniloxCapabilityTestResultRequest>(
                request => capturedCreateAniloxCapabilityTestResultRequestMessage = request)
            .ReturnsAsync(
                new InternalItemResponse<AniloxCapabilityTestResultDto>(expectedAniloxCapabilityTestResultDto))
            .Verifiable(Times.Once);

        var query = PhysicalAssetsCreateAniloxCapabilityTestResultMutation(
            PhysicalAssetId,
            expectedAniloxCapabilityTestResultDto.TestDateTime,
            expectedAniloxCapabilityTestResultDto.Note,
            aniloxCapabilityErrorType: "SCORING_LINE",
            expectedAniloxCapabilityTestResultDto.StartPositionOnAnilox,
            expectedAniloxCapabilityTestResultDto.EndPositionOnAnilox);

        var request = QueryRequestBuilder
            .New()
            .SetQuery(query)
            .AddRoleClaims("go-general")
            .AddGlobalState("userId", UserId)
            .Create();

        // Act
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        capturedCreateAniloxCapabilityTestResultRequestMessage.Should()
            .BeEquivalentTo(createAniloxCapabilityTestResultRequest);

        _physicalAssetHttpClientMock.VerifyNoOtherCalls();

        _physicalAssetQueueWrapperMock.VerifyAll();
        _physicalAssetQueueWrapperMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Create_Volume_Capability_Test_Result_Calls_Client_With_Request_And_Returns_Value()
    {
        // Arrange
        var executor = await InitializeExecutor();

        var machine = new Machine { MachineId = "EQ12345" };
        _machineCachingServiceMock
            .Setup(m => m.GetMachines(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync([machine])
            .Verifiable(Times.Once);
        _licenceManagerCachingServiceMock
            .Setup(m => m.GetDetailedLicenceValidity(machine.MachineId, Constants.LicensesApplications.Anilox, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<LicenceValidationInfo>(new LicenceValidationInfo { IsValid = true }))
            .Verifiable(Times.Once);

        var expectedVolumeCapabilityTestResultDto = new VolumeCapabilityTestResultDto(
            capabilityTestResultId: "65b3693056068ff18b8d1a7f",
            capabilityTestSpecificationId: "65b8b4ed9a37742358b8ef5c",
            PhysicalAssetId,
            testDateTime: new DateTime(year: 2023, month: 2, day: 1, hour: 12, minute: 30, second: 0, DateTimeKind.Utc),
            testerUserId: UserId,
            note: null,
            volume: 15.0,
            isPassed: true);

        var createVolumeCapabilityTestResultRequest =
            new PhysicalAssetInformation.CapabilityTest.CreateVolumeCapabilityTestResultRequest(
                expectedVolumeCapabilityTestResultDto.TesterUserId,
                expectedVolumeCapabilityTestResultDto.PhysicalAssetId,
                expectedVolumeCapabilityTestResultDto.TestDateTime,
                expectedVolumeCapabilityTestResultDto.Note,
                expectedVolumeCapabilityTestResultDto.Volume);

        PhysicalAssetInformation.CapabilityTest.CreateVolumeCapabilityTestResultRequest?
            capturedCreateVolumeCapabilityTestResultRequestMessage = null;
        _physicalAssetQueueWrapperMock
            .Setup(m => m.SendCreateVolumeCapabilityTestResultAndWaitForReply(
                It.IsAny<PhysicalAssetInformation.CapabilityTest.CreateVolumeCapabilityTestResultRequest>()))
            .Callback<PhysicalAssetInformation.CapabilityTest.CreateVolumeCapabilityTestResultRequest>(
                request => capturedCreateVolumeCapabilityTestResultRequestMessage = request)
            .ReturnsAsync(
                new InternalItemResponse<VolumeCapabilityTestResultDto>(expectedVolumeCapabilityTestResultDto))
            .Verifiable(Times.Once);

        var query = PhysicalAssetsCreateVolumeCapabilityTestResultMutation(
            PhysicalAssetId,
            expectedVolumeCapabilityTestResultDto.TestDateTime,
            expectedVolumeCapabilityTestResultDto.Note,
            expectedVolumeCapabilityTestResultDto.Volume);

        var request = QueryRequestBuilder
            .New()
            .SetQuery(query)
            .AddRoleClaims("go-general")
            .AddGlobalState("userId", UserId)
            .Create();

        // Act
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        capturedCreateVolumeCapabilityTestResultRequestMessage.Should()
            .BeEquivalentTo(createVolumeCapabilityTestResultRequest);

        _physicalAssetHttpClientMock.VerifyNoOtherCalls();

        _physicalAssetQueueWrapperMock.VerifyAll();
        _physicalAssetQueueWrapperMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Create_Cleaning_Operation_Calls_Client_With_Request_And_Returns_Value()
    {
        // Arrange
        var executor = await InitializeExecutor();

        var machine = new Machine { MachineId = "EQ12345" };
        _machineCachingServiceMock
            .Setup(m => m.GetMachines(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync([machine])
            .Verifiable(Times.Once);
        _licenceManagerCachingServiceMock
            .Setup(m => m.GetDetailedLicenceValidity(machine.MachineId, Constants.LicensesApplications.Anilox, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<LicenceValidationInfo>(new LicenceValidationInfo { IsValid = true }))
            .Verifiable(Times.Once);

        var createCleaningOperationRequest =
            new PhysicalAssetInformation.Operation.CreateCleaningOperationRequest(
                UserId,
                physicalAssetId: "65577c04e51181aa1bdcc90f",
                note: null,
                startDateTime:
                new DateTime(year: 2024, month: 2, day: 1, hour: 12, minute: 30, second: 0, DateTimeKind.Utc),
                CleaningOperationType.UltrasonicCleaning,
                resetVolumeDefects: true);

        var expectedCleaningOperationDto = new CleaningOperationDto(
            equipmentPhysicalAssetMappingId: "65b3693056068ff18b8d1a7f",
            createCleaningOperationRequest.PhysicalAssetId,
            createCleaningOperationRequest.StartDateTime,
            endDateTime: createCleaningOperationRequest.StartDateTime,
            operatorUserId: UserId,
            createCleaningOperationRequest.Note,
            createCleaningOperationRequest.CleaningOperationType,
            createCleaningOperationRequest.ResetVolumeDefects);

        PhysicalAssetInformation.Operation.CreateCleaningOperationRequest?
            capturedCreateCleaningOperationRequestMessage = null;
        _physicalAssetQueueWrapperMock
            .Setup(m => m.SendCreateCleaningOperationAndWaitForReply(
                It.IsAny<PhysicalAssetInformation.Operation.CreateCleaningOperationRequest>()))
            .Callback<PhysicalAssetInformation.Operation.CreateCleaningOperationRequest>(
                request => capturedCreateCleaningOperationRequestMessage = request)
            .ReturnsAsync(new InternalItemResponse<CleaningOperationDto>(expectedCleaningOperationDto))
            .Verifiable(Times.Once);

        var query = PhysicalAssetsCreateCleaningOperationMutation(
            PhysicalAssetId,
            createCleaningOperationRequest.Note,
            createCleaningOperationRequest.StartDateTime,
            cleaningOperationType: "ULTRASONIC_CLEANING",
            createCleaningOperationRequest.ResetVolumeDefects);

        var request = QueryRequestBuilder
            .New()
            .SetQuery(query)
            .AddRoleClaims("go-general")
            .AddGlobalState("userId", UserId)
            .Create();

        // Act
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        capturedCreateCleaningOperationRequestMessage.Should().BeEquivalentTo(createCleaningOperationRequest);

        _physicalAssetHttpClientMock.VerifyNoOtherCalls();

        _physicalAssetQueueWrapperMock.VerifyAll();
        _physicalAssetQueueWrapperMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Create_RefurbishingAnilox_Operation_Calls_Client_With_Request_And_Returns_Value()
    {
        // Arrange
        var executor = await InitializeExecutor();

        var machine = new Machine { MachineId = "EQ12345" };
        _machineCachingServiceMock
            .Setup(m => m.GetMachines(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync([machine])
            .Verifiable(Times.Once);
        _licenceManagerCachingServiceMock
            .Setup(m => m.GetDetailedLicenceValidity(machine.MachineId, Constants.LicensesApplications.Anilox, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<LicenceValidationInfo>(new LicenceValidationInfo { IsValid = true }))
            .Verifiable(Times.Once);

        var createRefurbishingAniloxOperationRequest =
            new PhysicalAssetInformation.Operation.CreateRefurbishingAniloxOperationRequest(
                UserId,
                physicalAssetId: "65577c04e51181aa1bdcc90f",
                note: null,
                refurbishedDateTime: new DateTime(year: 2024, month: 2, day: 1, hour: 12, minute: 30, second: 0, DateTimeKind.Utc),
                serialNumberOverwrite: "EQ34566",
                manufacturerOverwrite: "Zecher",
                screen: 25,
                engraving: "HEX 123TZ",
                setVolumeValue: 50.0,
                measuredVolumeValue: 90.0);

        var expectedRefurbishingOperationDto = new RefurbishingOperationDto(
            equipmentPhysicalAssetMappingId: "65b3693056068ff18b8d1a7f",
            createRefurbishingAniloxOperationRequest.PhysicalAssetId,
            startDateTime: createRefurbishingAniloxOperationRequest.RefurbishedDateTime,
            endDateTime: createRefurbishingAniloxOperationRequest.RefurbishedDateTime,
            createRefurbishingAniloxOperationRequest.UserId,
            createRefurbishingAniloxOperationRequest.Note);

        PhysicalAssetInformation.Operation.CreateRefurbishingAniloxOperationRequest?
            capturedCreateRefurbishingAniloxOperationRequestMessage = null;
        _physicalAssetQueueWrapperMock
            .Setup(m => m.SendCreateRefurbishingAniloxOperationAndWaitForReply(
                It.IsAny<PhysicalAssetInformation.Operation.CreateRefurbishingAniloxOperationRequest>()))
            .Callback<PhysicalAssetInformation.Operation.CreateRefurbishingAniloxOperationRequest>(
                request => capturedCreateRefurbishingAniloxOperationRequestMessage = request)
            .ReturnsAsync(new InternalItemResponse<RefurbishingOperationDto>(expectedRefurbishingOperationDto))
            .Verifiable(Times.Once);

        var query = PhysicalAssetsCreateRefurbishingAniloxOperationMutation(
            PhysicalAssetId,
            createRefurbishingAniloxOperationRequest.RefurbishedDateTime,
            createRefurbishingAniloxOperationRequest.SerialNumberOverwrite,
            createRefurbishingAniloxOperationRequest.ManufacturerOverwrite,
            createRefurbishingAniloxOperationRequest.Screen,
            createRefurbishingAniloxOperationRequest.Engraving,
            createRefurbishingAniloxOperationRequest.SetVolumeValue,
            createRefurbishingAniloxOperationRequest.MeasuredVolumeValue,
            createRefurbishingAniloxOperationRequest.Note);

        var request = QueryRequestBuilder
            .New()
            .SetQuery(query)
            .AddRoleClaims("go-general")
            .AddGlobalState("userId", UserId)
            .Create();

        // Act
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        capturedCreateRefurbishingAniloxOperationRequestMessage.Should().BeEquivalentTo(createRefurbishingAniloxOperationRequest);

        _physicalAssetHttpClientMock.VerifyNoOtherCalls();

        _physicalAssetQueueWrapperMock.VerifyAll();
        _physicalAssetQueueWrapperMock.VerifyNoOtherCalls();
    }

    private async Task<IRequestExecutor> InitializeExecutor()
    {
        var services = new ServiceCollection();

        WuH.Ruby.Common.ProjectTemplate.ServiceCollectionExtensions.AddAuthentication(services);

        var licenceService = new LicenceService(
            _machineCachingServiceMock.Object, _licenceManagerCachingServiceMock.Object);
        var physicalAssetService = new PhysicalAssetService(
            _physicalAssetSettingsHttpClientMock.Object,
            _physicalAssetHttpClientMock.Object,
            _physicalAssetQueueWrapperMock.Object);

        var physicalAssetCapabilityTestResultService =
            new PhysicalAssetCapabilityTestResultService(_physicalAssetQueueWrapperMock.Object);

        var physicalAssetOperationService = new PhysicalAssetOperationService(_physicalAssetQueueWrapperMock.Object);

        return await services
            .AddSingleton<ILicenceService>(licenceService)
            .AddSingleton<IPhysicalAssetService>(physicalAssetService)
            .AddSingleton<IPhysicalAssetCapabilityTestResultService>(physicalAssetCapabilityTestResultService)
            .AddSingleton<IPhysicalAssetOperationService>(physicalAssetOperationService)
            .AddSingleton(new Mock<ILogger<DefaultAuthorizationService>>().Object)
            .AddAuthorization()
            .AddHttpContextAccessor()
            .AddGraphQLServer()
            .AddDefaultTransactionScopeHandler()
            .AddMutationConventions()
            .AddHttpRequestInterceptor<HttpRequestInterceptor>()
            .ModifyOptions(t => t.StrictValidation = false)
            .AddAuthorization()
            .AddMutationType(q => q.Name("Mutation"))
            .AddType<PhysicalAssetsMutation>()
            .AddSorting()
            .AddFiltering()
            .BuildRequestExecutorAsync();
    }

    private static string PhysicalAssetUpdateSettingsMutation(int aniloxCleaningIntervalInMeter)
    {
        return $@"mutation
        {{
            physicalAssetUpdateSettings(input:
                {{
                    updatePhysicalAssetSettingsRequest: {{
                        aniloxCleaningIntervalInMeter: {aniloxCleaningIntervalInMeter}
                    }}
                }})
            {{
                updatePhysicalAssetSettings {{
                    cleaningInterval {{
                        unit
                        value
                    }}
                }}
                errors {{
                    ... on Error {{
                        __typename
                        message
                    }}
                    ... on ParameterInvalidError {{
                        __typename
                        message
                    }}
                    ... on InternalServiceError {{
                        __typename
                        statusCode
                    }}
                }}
            }}
        }}";
    }

    private static string PhysicalAssetsCreateAniloxMutation(
        string? serialNumber,
        string manufacturer = Manufacturer,
        string? preferredUsageLocation = null,
        IEnumerable<string>? scanCodes = null,
        double printWidth = PrintWidth,
        double outerDiameter = OuterDiameter,
        double screen = Screen,
        string? engraving = null,
        double setVolumeValue = SetVolumeValue,
        double? measuredVolumeValue = null)
    {
        return $@"mutation
        {{
            physicalAssetsCreateAnilox(input:
                {{
                    createAniloxPhysicalAssetRequest: {{
                        serialNumber: {(serialNumber is not null ? $"\"{serialNumber}\"" : "null")},
                        manufacturer: {$"\"{manufacturer}\""},
                        preferredUsageLocation: {(preferredUsageLocation is not null ? $"\"{preferredUsageLocation}\"" : "null")},
                        scanCodes: {JsonConvert.SerializeObject(scanCodes ?? ["123456789", "987654321"])}
                        printWidth: {printWidth.ToString("F", CultureInfo.InvariantCulture)},
                        isSleeve: false,
                        outerDiameter: {outerDiameter.ToString("F", CultureInfo.InvariantCulture)},
                        screen: {screen},
                        engraving: {(engraving is not null ? $"\"{engraving}\"" : "null")},
                        setVolumeValue: {setVolumeValue.ToString("F", CultureInfo.InvariantCulture)},
                        measuredVolumeValue: {measuredVolumeValue?.ToString("F", CultureInfo.InvariantCulture) ?? "null"}
                    }}
                }})
            {{
                createdAniloxPhysicalAsset {{
                    serialNumber
                    physicalAssetType
                    physicalAssetId
                    scanCodes
                }}
                errors {{
                    ... on Error {{
                        __typename
                        message
                    }}
                    ... on ParameterInvalidError {{
                        __typename
                        message
                    }}
                    ... on InternalServiceError {{
                        __typename
                        statusCode
                    }}
                }}
            }}
        }}";
    }

    private static string PhysicalAssetsUpdateAniloxMutation(
        string physicalAssetId,
        string? serialNumber,
        string manufacturer = Manufacturer,
        IEnumerable<string>? scanCodes = null,
        double printWidth = PrintWidth,
        double setVolumeValue = SetVolumeValue,
        double outerDiameter = OuterDiameter,
        double screen = Screen,
        string? preferredUsageLocation = null,
        string? engraving = null)
    {
        return $@"mutation
        {{
            physicalAssetsUpdateAnilox(input:
                {{
                    updateAniloxPhysicalAssetRequest: {{
                        physicalAssetId: ""{physicalAssetId}"",
                        serialNumber: {(serialNumber is not null ? $"\"{serialNumber}\"" : "null")},
                        manufacturer: {$"\"{manufacturer}\""},
                        scanCodes: {JsonConvert.SerializeObject(scanCodes ?? ["123456789", "987654321"])}
                        printWidth: {printWidth.ToString("F", CultureInfo.InvariantCulture)},
                        isSleeve: false,
                        outerDiameter: {outerDiameter.ToString("F", CultureInfo.InvariantCulture)},
                        screen: {screen},
                        preferredUsageLocation: {(preferredUsageLocation is not null ? $"\"{preferredUsageLocation}\"" : "null")},
                        engraving: {(engraving is not null ? $"\"{engraving}\"" : "null")},
                        setVolumeValue: {setVolumeValue.ToString("F", CultureInfo.InvariantCulture)}
                    }}
                }})
            {{
                updatedAniloxPhysicalAsset {{
                    serialNumber
                    physicalAssetType
                    physicalAssetId
                    scanCodes
                }}
                errors {{
                    ... on Error {{
                        __typename
                        message
                    }}
                    ... on ParameterInvalidError {{
                        __typename
                        message
                    }}
                    ... on InternalServiceError {{
                        __typename
                        statusCode
                    }}
                }}
            }}
        }}";
    }

    private static string PhysicalAssetsCreateAniloxCapabilityTestResultMutation(
        string physicalAssetId,
        DateTime testDateTime,
        string? note,
        string aniloxCapabilityErrorType,
        double startPositionOnAnilox,
        double? endPositionOnAnilox)
    {
        return $@"mutation
        {{
            physicalAssetsCreateAniloxCapabilityTestResult(input:
                {{
                    createAniloxCapabilityTestResultRequest: {{
                        physicalAssetId: ""{physicalAssetId}"",
                        testDateTime: ""{testDateTime.ToString("o", CultureInfo.InvariantCulture)}"",
                        note: {(note is not null ? $"\"{note}\"" : "null")},
                        aniloxCapabilityErrorType: {aniloxCapabilityErrorType},
                        startPositionOnAnilox: {startPositionOnAnilox.ToString("F", CultureInfo.InvariantCulture)},
                        endPositionOnAnilox: {(endPositionOnAnilox is null ? "null" : endPositionOnAnilox.Value.ToString("F", CultureInfo.InvariantCulture))}
                    }}
                }})
            {{
                createdAniloxCapabilityTestResult {{
                    capabilityTestResultId
                    capabilityTestSpecificationId
                    physicalAssetId
                    testDateTime
                    testerUserId
                    aniloxCapabilityErrorType
                    startPositionOnAnilox
                    endPositionOnAnilox
                }}
                errors {{
                    ... on Error {{
                        __typename
                        message
                    }}
                    ... on ParameterInvalidError {{
                        __typename
                        message
                    }}
                    ... on InternalServiceError {{
                        __typename
                        statusCode
                    }}
                }}
            }}
        }}";
    }

    private static string PhysicalAssetsCreateVolumeCapabilityTestResultMutation(
        string physicalAssetId, DateTime testDateTime, string? note, double volume)
    {
        return $@"mutation
        {{
            physicalAssetsCreateVolumeCapabilityTestResult(input:
                {{
                    createVolumeCapabilityTestResultRequest: {{
                        physicalAssetId: ""{physicalAssetId}"",
                        testDateTime: ""{testDateTime.ToString("o", CultureInfo.InvariantCulture)}"",
                        note: {(note is not null ? $"\"{note}\"" : "null")},
                        volume: {volume.ToString("F", CultureInfo.InvariantCulture)}
                    }}
                }})
            {{
                createdVolumeCapabilityTestResult {{
                    capabilityTestResultId
                    capabilityTestSpecificationId
                    physicalAssetId
                    testDateTime
                    testerUserId
                    volume
                }}
                errors {{
                    ... on Error {{
                        __typename
                        message
                    }}
                    ... on ParameterInvalidError {{
                        __typename
                        message
                    }}
                    ... on InternalServiceError {{
                        __typename
                        statusCode
                    }}
                }}
            }}
        }}";
    }

    private static string PhysicalAssetsCreateCleaningOperationMutation(
        string physicalAssetId,
        string? note,
        DateTime startDateTime,
        string cleaningOperationType,
        bool resetVolumeDefects)
    {
        return $@"mutation
        {{
            physicalAssetsCreateCleaningOperation(input:
                {{
                    createCleaningOperationRequest: {{
                        physicalAssetId: ""{physicalAssetId}"",
                        note: {(note is not null ? $"\"{note}\"" : "null")},
                        startDateTime: ""{startDateTime.ToString("O", CultureInfo.InvariantCulture)}"",
                        cleaningOperationType: {cleaningOperationType},
                        resetVolumeDefects: {resetVolumeDefects.ToString().ToLower()}
                    }}
                }})
            {{
                createdCleaningOperationResult {{
                    operationType
                    equipmentPhysicalAssetMappingId
                    physicalAssetId
                    startDateTime
                    endDateTime
                    operatorUserId
                    note
                    cleaningOperationType
                    resetVolumeDefects
                }}
                errors {{
                    ... on Error {{
                        __typename
                        message
                    }}
                    ... on ParameterInvalidError {{
                        __typename
                        message
                    }}
                    ... on InternalServiceError {{
                        __typename
                        statusCode
                    }}
                }}
            }}
        }}";
    }

    private static string PhysicalAssetsCreateRefurbishingAniloxOperationMutation(
        string physicalAssetId,
        DateTime refurbishedDateTime,
        string? serialNumberOverwrite,
        string? manufacturerOverwrite,
        int screen,
        string? engraving,
        double setVolumeValue,
        double? measuredVolumeValue,
        string? note)
    {
        return $@"mutation
        {{
            physicalAssetsCreateRefurbishingAniloxOperation(input:
                {{
                    createRefurbishingAniloxOperationRequest: {{
                        physicalAssetId: ""{physicalAssetId}"",
                        note: {(note is not null ? $"\"{note}\"" : "null")},
                        refurbishedDateTime: ""{refurbishedDateTime.ToString("O", CultureInfo.InvariantCulture)}"",
                        serialNumberOverwrite: {(serialNumberOverwrite is not null ? $"\"{serialNumberOverwrite}\"" : "null")},
                        manufacturerOverwrite: {(manufacturerOverwrite is not null ? $"\"{manufacturerOverwrite}\"" : "null")},
                        screen: {screen},
                        setVolumeValue: {setVolumeValue.ToString("F", CultureInfo.InvariantCulture)},
                        engraving: {(engraving is not null ? $"\"{engraving}\"" : "null")},
                        measuredVolumeValue: {measuredVolumeValue?.ToString("F", CultureInfo.InvariantCulture) ?? "null"}
                    }}
                }})
            {{
                createdRefurbishingAniloxOperationResult {{
                    operationType
                    equipmentPhysicalAssetMappingId
                    physicalAssetId
                    startDateTime
                    endDateTime
                    operatorUserId
                    note
                }}
                errors {{
                    ... on Error {{
                        __typename
                        message
                    }}
                    ... on ParameterInvalidError {{
                        __typename
                        message
                    }}
                    ... on InternalServiceError {{
                        __typename
                        statusCode
                    }}
                }}
            }}
        }}";
    }
}