using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.DataLoaders;
using FrameworkAPI.Exceptions;
using FrameworkAPI.Models.Settings;
using FrameworkAPI.Schema.Machine;
using FrameworkAPI.Schema.ProducedJob;
using FrameworkAPI.Services;
using FrameworkAPI.Services.Interfaces;
using FrameworkAPI.Services.Settings;
using FrameworkAPI.Test.Services.Helpers;
using FrameworkAPI.Test.TestHelpers;
using HotChocolate;
using HotChocolate.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Snapshooter.Xunit;
using WuH.Ruby.AlarmDataHandler.Client;
using WuH.Ruby.Common.Core;
using WuH.Ruby.LicenceManager.Client;
using WuH.Ruby.Common.Queue;
using WuH.Ruby.KpiDataHandler.Client;
using WuH.Ruby.MachineSnapShooter.Client;
using WuH.Ruby.MachineSnapShooter.Client.Models;
using WuH.Ruby.MachineSnapShooter.Client.Queue;
using WuH.Ruby.OpcUaForwarder.Client;
using WuH.Ruby.ProcessDataReader.Client;
using WuH.Ruby.ProductionPeriodsDataHandler.Client;
using WuH.Ruby.Settings.Client;
using Xunit;
using MachineDataHandler = WuH.Ruby.MachineDataHandler.Client;
using MachineFamily = FrameworkAPI.Schema.Misc.MachineFamily;

namespace FrameworkAPI.Test.Queries.MachineQuery;

public class MachineQueryIntegrationTests
{
    private const string MachineId = "EQ00001";
    private const string UserId = "mockUser";

    private readonly Mock<IProductionPeriodsDataHandlerHttpClient> _productionPeriodsDataHandlerHttpClientMock = new();
    private readonly Mock<IProcessDataReaderHttpClient> _processDataReaderHttpClientMock = new();
    private readonly Mock<MachineDataHandler.IMachineCachingService> _machineCachingServiceMock = new();
    private readonly Mock<IMachineSnapshotHttpClient> _machineSnapshotHttpClient = new();
    private readonly Mock<ILatestMachineSnapshotCachingService> _latestMachineSnapshotCachingServiceMock = new();
    private readonly Mock<IProductionPeriodChangesQueueWrapper> _productionPeriodChangesQueueWrapper = new();
    private readonly Mock<IAlarmDataHandlerHttpClient> _alarmDataHandlerHttpClientMock = new();
    private readonly Mock<IAlarmDataHandlerCachingService> _alarmDataHandlerCachingServiceMock = new();
    private readonly Mock<ISettingsService> _settingsServiceMock = new();
    private readonly Mock<ILicenceManagerCachingService> _licenceManagerCachingServiceMock = new();

    [Fact]
    public async Task GetMachine_With_MachineId_Should_Return_The_Correct_Response()
    {
        // Arrange
        var executor = await InitializeExecutor();

        var machine = MachineMock.GenerateCastFilm(MachineId);

        _machineCachingServiceMock
            .Setup(m => m.GetMachine(MachineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(machine);

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    machine(machineId: ""{MachineId}"") {{
                        machineId
                    }}
                }}")
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        _machineCachingServiceMock.VerifyAll();
        _machineCachingServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetMachine_With_MachineId_Should_Return_An_Error_Response_If_MachineId_Does_Not_Exist()
    {
        // Arrange
        var executor = await InitializeExecutor();

        _machineCachingServiceMock
            .Setup(m => m.GetMachine(MachineId, It.IsAny<CancellationToken>()))
            .Throws(new InternalServiceException());

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    machine(machineId: ""{MachineId}"") {{
                        machineId
                    }}
                }}")
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);
        // Assert
        result.ToJson().MatchSnapshot();

        _machineCachingServiceMock.VerifyAll();
        _machineCachingServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetMachine_By_TimeStamp_Should_Use_Query_Timestamp_As_Time()
    {
        // Arrange
        var executor = await InitializeExecutor();

        var machine = MachineMock.GenerateCastFilm(MachineId);

        _machineCachingServiceMock
            .Setup(m => m.GetMachine(MachineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(machine);

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    machine(machineId: ""{MachineId}"", timestamp: ""1970-01-01T07:00:00.000Z"") {{
                        time
                    }}
                }}")
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        _machineCachingServiceMock.VerifyAll();
        _machineCachingServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetMachine_With_All_Attributes_Should_Return_The_Correct_Response()
    {
        // Arrange
        using var fakeLocalTimeZone = new FakeLocalTimeZone(TimeZoneInfo.FindSystemTimeZoneById("US/Eastern"));

        var executor = await InitializeExecutor();

        var machine = MachineMock.GenerateCastFilm(MachineId);

        _machineCachingServiceMock
            .Setup(m => m.GetMachine(MachineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(machine);

        _processDataReaderHttpClientMock
            .Setup(m => m.GetLastReceivedOpcUaServerTime(It.IsAny<CancellationToken>(), MachineId))
            .ReturnsAsync(new InternalItemResponse<DateTime>(DateTime.UnixEpoch));

        _latestMachineSnapshotCachingServiceMock
            .Setup(m => m.GetLatestMachineSnapshot(MachineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<MachineSnapshotResponse>(
                new MachineSnapshotResponse(
                    new SnapshotMetaDto(MachineId, "fakeHash", DateTime.MinValue, []),
                    new SnapshotDto(
                        [],
                        snapshotTime: DateTime.UnixEpoch))));

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    machine(machineId: ""{MachineId}"") {{
                        time
                        name
                        machineType
                        machineId
                        machineFamily
                        department
                    }}
                }}")
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        _machineCachingServiceMock.VerifyAll();
        _machineCachingServiceMock.VerifyNoOtherCalls();

        _processDataReaderHttpClientMock.VerifyAll();
        _processDataReaderHttpClientMock.VerifyNoOtherCalls();

        _latestMachineSnapshotCachingServiceMock.VerifyAdd(m =>
            m.CacheChanged += It.IsAny<EventHandler<LiveSnapshotEventArgs>>());
        _latestMachineSnapshotCachingServiceMock.VerifyAll();
        _latestMachineSnapshotCachingServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetMachine_With_ProductionStatus()
    {
        // Arrange
        var executor = await InitializeExecutor();

        var machine = MachineMock.GenerateCastFilm(MachineId);

        _machineCachingServiceMock
            .Setup(m => m.GetMachine(MachineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(machine);

        var snapshotMetaDto = new SnapshotMetaDto(MachineId, "fakeHash", DateTime.MinValue, []);
        var snapshotDto = new SnapshotDto(
            [
                new SnapshotColumnValueDto(SnapshotColumnIds.ProductionStatusId, 111),
                new SnapshotColumnValueDto(SnapshotColumnIds.ProductionStatusCategory, "Offline")
            ],
            DateTime.UtcNow);
        var valueChangedTimestamp = new DateTime(2023, 3, 15, 8, 15, 0, DateTimeKind.Utc);

        var machineSnapshotResponse = new MachineSnapshotResponse(snapshotMetaDto, snapshotDto);

        _latestMachineSnapshotCachingServiceMock
            .Setup(m => m.GetLatestMachineSnapshot(MachineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<MachineSnapshotResponse>(machineSnapshotResponse));

        _machineSnapshotHttpClient.Setup(m =>
                m.GetLatestSnapshotColumnValueChangedTimestamp(
                    MachineId, SnapshotColumnIds.ProductionStatusCategory,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new InternalItemResponse<SnapshotColumnValueChangedResponse>(
                    new SnapshotColumnValueChangedResponse(valueChangedTimestamp, 111, DateTime.UnixEpoch)));

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    machine(machineId: ""{MachineId}"") {{
                        machineId
                        productionStatus {{
                        category
                        id
                        startTime
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

        _latestMachineSnapshotCachingServiceMock.VerifyAdd(m =>
            m.CacheChanged += It.IsAny<EventHandler<LiveSnapshotEventArgs>>());
        _latestMachineSnapshotCachingServiceMock.VerifyAll();
        _latestMachineSnapshotCachingServiceMock.VerifyNoOtherCalls();

        _machineSnapshotHttpClient.VerifyAll();
        _machineSnapshotHttpClient.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetMachine_With_ProductionStatus_By_TimeStamp_Should_Use_Query_Timestamp_As_Time()
    {
        // Arrange
        var executor = await InitializeExecutor();

        var machine = MachineMock.GenerateCastFilm(MachineId);

        _machineCachingServiceMock
            .Setup(m => m.GetMachine(MachineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(machine);

        var snapshotTimestamp = new DateTime(1970, 1, 1, 7, 0, 0, DateTimeKind.Utc);
        var snapshotMetaDto = new SnapshotMetaDto(MachineId, "fakeHash", DateTime.MinValue, []);
        var snapshotDto = new SnapshotDto(
            [
                new SnapshotColumnValueDto(SnapshotColumnIds.ProductionStatusId, 111),
                new SnapshotColumnValueDto(SnapshotColumnIds.ProductionStatusCategory, "Offline")
            ],
            snapshotTimestamp);
        var snapshotForTimestampDto = new SnapshotForTimestampDto(snapshotDto, snapshotTimestamp);
        var machineSnapshotForTimestampListResponse = new MachineSnapshotForTimestampListResponse(snapshotMetaDto,
            [snapshotForTimestampDto]);
        var snapshotColumnValueChangedResponse =
            new SnapshotColumnValueChangedResponse(new DateTime(2023, 3, 15, 8, 15, 0, DateTimeKind.Utc), 111, DateTime.UnixEpoch);

        _machineSnapshotHttpClient
            .Setup(m => m.GetSnapshotsForTimestamps(
                MachineId,
                It.IsAny<List<DateTime>>(),
                It.IsAny<List<string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new InternalItemResponse<MachineSnapshotForTimestampListResponse>(
                    machineSnapshotForTimestampListResponse));

        _machineSnapshotHttpClient
            .Setup(m => m.GetSnapshotColumnValueChangedTimestampAfterValueTimestamp(
                MachineId, SnapshotColumnIds.ProductionStatusCategory, snapshotTimestamp, It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new InternalItemResponse<SnapshotColumnValueChangedResponse>(snapshotColumnValueChangedResponse));

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    machine(machineId: ""{MachineId}"", timestamp: ""{snapshotTimestamp.ToString("O", CultureInfo.InvariantCulture)}"") {{
                        machineId
                        productionStatus {{
                        category
                        id
                        startTime
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

        _latestMachineSnapshotCachingServiceMock.VerifyAdd(m =>
            m.CacheChanged += It.IsAny<EventHandler<LiveSnapshotEventArgs>>());
        _latestMachineSnapshotCachingServiceMock.VerifyAll();
        _latestMachineSnapshotCachingServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetMachine_With_ProducedJob()
    {
        // Arrange
        var executor = await InitializeExecutor();

        var machine = MachineMock.GenerateCastFilm(MachineId);

        _machineCachingServiceMock
            .Setup(m => m.GetMachine(MachineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(machine);

        var job = new JobInfo
        {
            MachineId = MachineId,
            ProductId = "FakeProductId",
            JobId = "FakeJobId",
            StartTime = DateTime.UnixEpoch
        };

        _productionPeriodsDataHandlerHttpClientMock
            .Setup(m => m.GetLatestJobs(It.IsAny<CancellationToken>(), new List<string> { MachineId }, 0, 1, null))
            .ReturnsAsync(new InternalListResponse<JobInfo>([job]));

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    machine(machineId: ""{MachineId}"") {{
                        machineId
                        producedJob {{
                        uniqueId
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

        _productionPeriodsDataHandlerHttpClientMock.VerifyAll();
        _productionPeriodsDataHandlerHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetMachine_With_Features()
    {
        // Arrange
        var executor = await InitializeExecutor();

        var machine = new MachineDataHandler.Machine
        {
            MachineId = MachineId,
            MachineType = "Extrusion",
            MachineFamily = MachineFamily.CastFilm.ToString(),
            BusinessUnit = MachineDataHandler.BusinessUnit.Extrusion,
            Features = new List<MachineDataHandler.MachineFeature>()
            {
                new() { Name = "ProcessData", FeatureVersion = 1 },
                new() { Name = "ProductionPeriods", FeatureVersion = 2 },
                new() { Name = "AlarmHandling", FeatureVersion = 3 },
                new() { Name = "Material", FeatureVersion = 4 },
                new() { Name = "Messaging", FeatureVersion = 5 },
                new() { Name = "Check", FeatureVersion = 6 },
                new() { Name = "DefectCheck", FeatureVersion = 7 },
                new() { Name = "BarcodeCheck", FeatureVersion = 8 },
                new() { Name = "PDFCheck", FeatureVersion = 9 },
                new() { Name = "PDFCheckPackageTransfer", FeatureVersion = 10 },
                new() { Name = "BrowserOverlay", FeatureVersion = 11 },
                new() { Name = "Flow", FeatureVersion = 12 },
                new() { Name = "RgbLabCheck", FeatureVersion = 13 }
            }
        };

        _machineCachingServiceMock
            .Setup(m => m.GetMachine(MachineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(machine);

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    machine(machineId: ""{MachineId}"") {{
                        machineId
                        features {{
                        alarmHandlingFeatureVersion
                        barcodeCheckFeatureVersion
                        browserOverlayFeatureVersion
                        checkFeatureVersion
                        defectCheckFeatureVersion
                        flowFeatureVersion
                        hasAlarmHandlingFeature
                        hasBarcodeCheckFeature
                        hasBrowserOverlayFeature
                        hasCheckFeature
                        hasDefectCheckFeature
                        hasFlowFeature
                        hasMaterialFeature
                        hasMessagingFeature
                        hasPdfCheckFeature
                        hasPdfCheckPackageTransferFeature
                        hasProcessDataFeature
                        hasProductionPeriodsFeature
                        hasRgbLabCheckFeature
                        materialFeatureVersion
                        messagingFeatureVersion
                        pdfCheckFeatureVersion
                        pdfCheckPackageTransferFeatureVersion
                        processDataFeatureVersion
                        productionPeriodsFeatureVersion
                        rgbLabCheckFeatureVersion
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
    }

    [Fact]
    public async Task GetMachine_Without_Features()
    {
        // Arrange
        var executor = await InitializeExecutor();

        var machine = new MachineDataHandler.Machine
        {
            MachineId = MachineId,
            MachineType = "Extrusion",
            MachineFamily = MachineFamily.CastFilm.ToString(),
            BusinessUnit = MachineDataHandler.BusinessUnit.Extrusion,
            Features = new List<MachineDataHandler.MachineFeature>()
        };

        _machineCachingServiceMock
            .Setup(m => m.GetMachine(MachineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(machine);

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    machine(machineId: ""{MachineId}"") {{
                        machineId
                        features {{
                        alarmHandlingFeatureVersion
                        barcodeCheckFeatureVersion
                        browserOverlayFeatureVersion
                        checkFeatureVersion
                        defectCheckFeatureVersion
                        flowFeatureVersion
                        hasAlarmHandlingFeature
                        hasBarcodeCheckFeature
                        hasBrowserOverlayFeature
                        hasCheckFeature
                        hasDefectCheckFeature
                        hasFlowFeature
                        hasMaterialFeature
                        hasMessagingFeature
                        hasPdfCheckFeature
                        hasPdfCheckPackageTransferFeature
                        hasProcessDataFeature
                        hasProductionPeriodsFeature
                        hasRgbLabCheckFeature
                        materialFeatureVersion
                        messagingFeatureVersion
                        pdfCheckFeatureVersion
                        pdfCheckPackageTransferFeatureVersion
                        processDataFeatureVersion
                        productionPeriodsFeatureVersion
                        rgbLabCheckFeatureVersion
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
    }

    [Fact]
    public async Task GetMachine_With_RubyLicences()
    {
        // Arrange
        var executor = await InitializeExecutor();

        var testDataTime = new DateTime(2020, 04, 01, 12, 00, 00, 00, DateTimeKind.Utc);
        var machine = MachineMock.GenerateCastFilm(MachineId);

        _machineCachingServiceMock
            .Setup(m => m.GetMachine(MachineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(machine);

        _licenceManagerCachingServiceMock
            .Setup(m => m.GetAllDetailedLicenceValidity(machine.MachineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<Dictionary<string, LicenceValidationInfo>>(new Dictionary<string, LicenceValidationInfo>
            {
                { Constants.LicensesApplications.Anilox, new LicenceValidationInfo { ActivationDate = testDataTime.AddDays(-3), ExpiryDate = testDataTime.AddDays(3), IsValid = true } },
                { Constants.LicensesApplications.Track, new LicenceValidationInfo { ActivationDate = testDataTime.AddDays(21), ExpiryDate = testDataTime.AddDays(25), IsValid = false } }
            }));

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    machine(machineId: ""{MachineId}"") {{
                        licenses {{
                          expiryDateOfAniloxLicense
                          expiryDateOfCheckLicense
                          expiryDateOfConnect4FlowLicense
                          expiryDateOfGoLicense
                          expiryDateOfTrackLicense
                          hasValidAniloxLicense
                          hasValidCheckLicense
                          hasValidConnect4FlowLicense
                          hasValidGoLicense
                          hasValidTrackLicense
                        }}
                    }}
                }}")
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        _licenceManagerCachingServiceMock.VerifyAll();
        _licenceManagerCachingServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Get_Active_Machine_Alarms()
    {
        // Arrange
        var executor = await InitializeExecutor();
        var setting = new Setting(UserSettingIds.Language, "de-de");

        var machine = MachineMock.GenerateCastFilm(MachineId);

        _machineCachingServiceMock
            .Setup(m => m.GetMachine(MachineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(machine);

        _settingsServiceMock.Setup(mock => mock.GetSettingsForUserAndMachine(
            null,
            UserId,
            It.IsAny<string>(),
            It.IsAny<CancellationToken>(),
            false
        )).ReturnsAsync(new InternalItemResponse<Setting>(setting));

        var alarms = new List<Alarm>
        {
            new Alarm
            {
                Id = "1",
                MachineId = MachineId,
                AlarmCode = "440-41",
                StartTimestamp = default,
                EndTimestamp = null,
                AlarmNumber = 0,
            },
            new Alarm
            {
                Id = "2",
                MachineId = MachineId,
                AlarmCode = "440-42",
                StartTimestamp = default,
                EndTimestamp = DateTime.UnixEpoch.AddDays(1),
                AlarmNumber = 0,
            }
        };
        _alarmDataHandlerCachingServiceMock.Setup(mock => mock.GetActiveAlarms(
            MachineId,
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(new InternalListResponse<Alarm>(alarms));

        var query =
            $@"{{
                machine(machineId: ""{MachineId}"")
                {{
                    activeMachineAlarms(take: 10){{
                        id
                    }}
                }}
              }}";

        var request = QueryRequestBuilder
            .New()
            .SetQuery(query)
            .AddGlobalState("userId", UserId)
            .AddRoleClaims("go-general")
            .Create();

        // Act
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        _alarmDataHandlerHttpClientMock.VerifyAll();
        _alarmDataHandlerHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Get_Active_Machine_Alarms_But_There_Are_No_Alarms_Returned()
    {
        // Arrange
        var executor = await InitializeExecutor();
        var setting = new Setting(UserSettingIds.Language, "de-de");

        var machine = MachineMock.GenerateCastFilm(MachineId);

        _machineCachingServiceMock
            .Setup(m => m.GetMachine(MachineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(machine);

        _settingsServiceMock.Setup(mock => mock.GetSettingsForUserAndMachine(
            null,
            UserId,
            It.IsAny<string>(),
            It.IsAny<CancellationToken>(),
            false
        )).ReturnsAsync(new InternalItemResponse<Setting>(setting));

        _alarmDataHandlerCachingServiceMock.Setup(mock => mock.GetActiveAlarms(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(new InternalListResponse<Alarm>([]));

        var query =
            $@"{{
                machine(machineId: ""{MachineId}"")
                {{
                    activeMachineAlarms(take: 10){{
                        id
                    }}
                }}
              }}";

        var request = QueryRequestBuilder
            .New()
            .SetQuery(query)
            .AddGlobalState("userId", UserId)
            .AddRoleClaims("go-general")
            .Create();

        // Act
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        _alarmDataHandlerHttpClientMock.VerifyAll();
        _alarmDataHandlerHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Get_Active_Machine_Alarms_But_There_Is_A_Exception_In_AlarmDataHandler_DataResult()
    {
        // Arrange
        var executor = await InitializeExecutor();
        var setting = new Setting(UserSettingIds.Language, "de-de");

        var machine = MachineMock.GenerateCastFilm(MachineId);

        _machineCachingServiceMock
            .Setup(m => m.GetMachine(MachineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(machine);

        _settingsServiceMock.Setup(mock => mock.GetSettingsForUserAndMachine(
            null,
            UserId,
            It.IsAny<string>(),
            It.IsAny<CancellationToken>(),
            false
        )).ReturnsAsync(new InternalItemResponse<Setting>(setting));

        _alarmDataHandlerCachingServiceMock.Setup(mock => mock.GetActiveAlarms(
            MachineId,
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(new InternalListResponse<Alarm>(StatusCodes.Status503ServiceUnavailable, new Exception("Test")));

        var query =
            $@"{{
                machine(machineId: ""{MachineId}"")
                {{
                    activeMachineAlarms(take: 10){{
                        id
                    }}
                }}
              }}";

        var request = QueryRequestBuilder
            .New()
            .SetQuery(query)
            .AddGlobalState("userId", UserId)
            .AddRoleClaims("go-general")
            .Create();

        // Act
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        _alarmDataHandlerHttpClientMock.VerifyAll();
        _alarmDataHandlerHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Get_Active_Primal_Machine_Alarm()
    {
        // Arrange
        var executor = await InitializeExecutor();
        var setting = new Setting(UserSettingIds.Language, "de-de");

        var machine = MachineMock.GenerateCastFilm(MachineId);

        _machineCachingServiceMock
            .Setup(m => m.GetMachine(MachineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(machine);

        _settingsServiceMock.Setup(mock => mock.GetSettingsForUserAndMachine(
            null,
            UserId,
            It.IsAny<string>(),
            It.IsAny<CancellationToken>(),
            false
        )).ReturnsAsync(new InternalItemResponse<Setting>(setting));

        var alarms = new List<Alarm>
        {
            new Alarm
            {
                Id = "1",
                MachineId = MachineId,
                AlarmCode = "440-41",
                StartTimestamp = default,
                EndTimestamp = null,
                AlarmNumber = 0,
                IsPrimal = true
            },
            new Alarm
            {
                Id = "2",
                MachineId = MachineId,
                AlarmCode = "440-42",
                StartTimestamp = default,
                EndTimestamp = DateTime.UnixEpoch.AddDays(1),
                AlarmNumber = 0,
            }
        };
        _alarmDataHandlerCachingServiceMock.Setup(mock => mock.GetActiveAlarms(
            MachineId,
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(new InternalListResponse<Alarm>(alarms));

        var query =
            $@"{{
                machine(machineId: ""{MachineId}"")
                {{
                    activePrimalMachineAlarm(){{
                        id
                    }}
                }}
              }}";

        var request = QueryRequestBuilder
            .New()
            .SetQuery(query)
            .AddGlobalState("userId", UserId)
            .AddRoleClaims("go-general")
            .Create();

        // Act
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        _alarmDataHandlerHttpClientMock.VerifyAll();
        _alarmDataHandlerHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Get_Active_Primal_Machine_Alarm_Which_Is_The_Newest()
    {
        // Arrange
        var executor = await InitializeExecutor();
        var setting = new Setting(UserSettingIds.Language, "de-de");

        var machine = MachineMock.GenerateCastFilm(MachineId);

        _machineCachingServiceMock
            .Setup(m => m.GetMachine(MachineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(machine);

        _settingsServiceMock.Setup(mock => mock.GetSettingsForUserAndMachine(
            null,
            UserId,
            It.IsAny<string>(),
            It.IsAny<CancellationToken>(),
            false
        )).ReturnsAsync(new InternalItemResponse<Setting>(setting));

        var alarms = new List<Alarm>
        {
            new Alarm
            {
                Id = "1",
                MachineId = MachineId,
                AlarmCode = "440-41",
                StartTimestamp = DateTime.UnixEpoch.AddDays(5),
                EndTimestamp = null,
                AlarmNumber = 0,
                IsPrimal = true
            },
            new Alarm
            {
                Id = "2",
                MachineId = MachineId,
                AlarmCode = "440-42",
                StartTimestamp = default,
                EndTimestamp = DateTime.UnixEpoch.AddDays(1),
                AlarmNumber = 0,
            },
            new Alarm
            {
                Id = "3",
                MachineId = MachineId,
                AlarmCode = "440-43",
                StartTimestamp = DateTime.UnixEpoch.AddDays(1),
                EndTimestamp = null,
                AlarmNumber = 0,
                IsPrimal = true
            }
        };
        _alarmDataHandlerCachingServiceMock.Setup(mock => mock.GetActiveAlarms(
            MachineId,
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(new InternalListResponse<Alarm>(alarms));

        var query =
            $@"{{
                machine(machineId: ""{MachineId}"")
                {{
                    activePrimalMachineAlarm(){{
                        id
                    }}
                }}
              }}";

        var request = QueryRequestBuilder
            .New()
            .SetQuery(query)
            .AddGlobalState("userId", UserId)
            .AddRoleClaims("go-general")
            .Create();

        // Act
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        _alarmDataHandlerHttpClientMock.VerifyAll();
        _alarmDataHandlerHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Get_Active_Primal_Machine_Alarms_But_There_Are_No_Alarms_Returned()
    {
        // Arrange
        var executor = await InitializeExecutor();
        var setting = new Setting(UserSettingIds.Language, "de-de");

        var machine = MachineMock.GenerateCastFilm(MachineId);

        _machineCachingServiceMock
            .Setup(m => m.GetMachine(MachineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(machine);

        _settingsServiceMock.Setup(mock => mock.GetSettingsForUserAndMachine(
            null,
            UserId,
            It.IsAny<string>(),
            It.IsAny<CancellationToken>(),
            false
        )).ReturnsAsync(new InternalItemResponse<Setting>(setting));

        _alarmDataHandlerCachingServiceMock.Setup(mock => mock.GetActiveAlarms(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(new InternalListResponse<Alarm>([]));

        var query =
            $@"{{
                machine(machineId: ""{MachineId}"")
                {{
                    activePrimalMachineAlarm(){{
                        id
                    }}
                }}
              }}";

        var request = QueryRequestBuilder
            .New()
            .SetQuery(query)
            .AddGlobalState("userId", UserId)
            .AddRoleClaims("go-general")
            .Create();

        // Act
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        _alarmDataHandlerHttpClientMock.VerifyAll();
        _alarmDataHandlerHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Get_Active_Primal_Machine_Alarms_But_There_Is_A_Exception_In_AlarmDataHandler_DataResult()
    {
        // Arrange
        var executor = await InitializeExecutor();
        var setting = new Setting(UserSettingIds.Language, "de-de");

        var machine = MachineMock.GenerateCastFilm(MachineId);

        _machineCachingServiceMock
            .Setup(m => m.GetMachine(MachineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(machine);

        _settingsServiceMock.Setup(mock => mock.GetSettingsForUserAndMachine(
            null,
            UserId,
            It.IsAny<string>(),
            It.IsAny<CancellationToken>(),
            false
        )).ReturnsAsync(new InternalItemResponse<Setting>(setting));

        _alarmDataHandlerCachingServiceMock.Setup(mock => mock.GetActiveAlarms(
            MachineId,
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(new InternalListResponse<Alarm>(StatusCodes.Status503ServiceUnavailable, new Exception("Test")));

        var query =
            $@"{{
                machine(machineId: ""{MachineId}"")
                {{
                    activePrimalMachineAlarm(){{
                        id
                    }}
                }}
              }}";

        var request = QueryRequestBuilder
            .New()
            .SetQuery(query)
            .AddGlobalState("userId", UserId)
            .AddRoleClaims("go-general")
            .Create();

        // Act
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        _alarmDataHandlerHttpClientMock.VerifyAll();
        _alarmDataHandlerHttpClientMock.VerifyNoOtherCalls();
    }

    private async Task<IRequestExecutor> InitializeExecutor()
    {
        var machineService = new MachineService(
            _machineCachingServiceMock.Object, new Mock<ILogger<MachineService>>().Object);
        var machineSnapshotQueueWrapper = new MachineSnapshotQueueWrapper(
            new Mock<ILogger<MachineSnapshotQueueWrapper>>().Object,
            new Mock<IQueueService>().Object);

        var snapshotColumnIdChangedTimestampService = new SnapshotColumnValueChangedTimestampCachingService
            (_latestMachineSnapshotCachingServiceMock.Object,
            _machineSnapshotHttpClient.Object,
            machineSnapshotQueueWrapper,
            new Mock<ILogger<SnapshotColumnValueChangedTimestampCachingService>>().Object);

        var machineTimeService = new MachineTimeService(
            new OpcUaServerTimeCachingService(
                _processDataReaderHttpClientMock.Object, new Mock<IProcessDataQueueWrapper>().Object),
            _latestMachineSnapshotCachingServiceMock.Object,
            new Mock<ILogger<MachineTimeService>>().Object);
        var jobInfoCachingService = new JobInfoCachingService(
            machineTimeService,
            _productionPeriodsDataHandlerHttpClientMock.Object,
            _productionPeriodChangesQueueWrapper.Object,
            new Mock<ILogger<JobInfoCachingService>>().Object);
        var producedJobService = new ProducedJobService(
            jobInfoCachingService,
            _productionPeriodsDataHandlerHttpClientMock.Object,
            machineService,
            new Mock<ILogger<ProducedJobService>>().Object,
            new Mock<IKpiEventQueueWrapper>().Object);
        var alarmService = new AlarmService(_alarmDataHandlerHttpClientMock.Object);
        var userSettingsService = new UserSettingsService(_settingsServiceMock.Object);
        var userSettingsBatchLoader = new UserSettingsBatchLoader(_settingsServiceMock.Object, new DelayedBatchScheduler());
        var alarmCacheDataLoader = new ActiveAlarmsCacheDataLoader(_alarmDataHandlerCachingServiceMock.Object);
        var licenceService = new LicenceService(_machineCachingServiceMock.Object, _licenceManagerCachingServiceMock.Object);

        return await new ServiceCollection()
            .AddSingleton(userSettingsBatchLoader)
            .AddSingleton(_latestMachineSnapshotCachingServiceMock.Object)
            .AddSingleton(_machineSnapshotHttpClient.Object)
            .AddSingleton(_alarmDataHandlerHttpClientMock.Object)
            .AddSingleton(_alarmDataHandlerCachingServiceMock)
            .AddSingleton(alarmCacheDataLoader)
            .AddSingleton<IOpcUaServerTimeCachingService>(new OpcUaServerTimeCachingService(
                _processDataReaderHttpClientMock.Object, new Mock<IProcessDataQueueWrapper>().Object))
            .AddSingleton<IMachineService>(machineService)
            .AddSingleton<ISnapshotColumnIdChangedTimestampCachingService>(snapshotColumnIdChangedTimestampService)
            .AddSingleton<IProducedJobService>(producedJobService)
            .AddSingleton<IMachineTimeService>(machineTimeService)
            .AddSingleton<IAlarmService>(alarmService)
            .AddSingleton<IMachineSnapshotService>(new MachineSnapshotService())
            .AddSingleton<IUserSettingsService>(userSettingsService)
            .AddSingleton<ILicenceService>(licenceService)
            .AddLogging()
            .AddGraphQLServer()
            .AddAuthorization()
            .ModifyOptions(t => t.StrictValidation = false)
            .AddQueryType(q => q.Name("Query"))
            .AddType<FrameworkAPI.Queries.MachineQuery>()
            .AddType<ExtrusionMachine>()
            .AddType<ProducedJob>()
            .AddType<ExtrusionProducedJob>()
            .AddSorting()
            .AddFiltering()
            .BuildRequestExecutorAsync();
    }
}