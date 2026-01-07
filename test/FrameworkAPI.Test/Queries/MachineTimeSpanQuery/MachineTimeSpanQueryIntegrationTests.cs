using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.DataLoaders;
using FrameworkAPI.Models.Settings;
using FrameworkAPI.Schema.MachineTimeSpan;
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
using WuH.Ruby.MachineDataHandler.Client;
using WuH.Ruby.MachineSnapShooter.Client;
using WuH.Ruby.MachineSnapShooter.Client.Models;
using WuH.Ruby.Settings.Client;
using Xunit;

namespace FrameworkAPI.Test.Queries.MachineTimeSpanQuery;

public class MachineTimeSpanQueryIntegrationTests
{
    private const string ArbitraryMachineId = "EQ12345";
    private const string UserId = "mockUser";

    private readonly Machine _arbitraryMachine = new()
    {
        MachineId = ArbitraryMachineId,
        BusinessUnit = BusinessUnit.Extrusion,
        MachineFamily = MachineFamily.BlowFilm.ToString()
    };

    private readonly ValueByColumnId<GroupedSumByIdentifier> _arbitraryGroupedSumByIdentifierColumnId = new()
    {
        {
            "Extrusion.ExtruderA.Settings.Component1.MaterialName",
            new()
            {
                { "ArbitraryMaterialName1", 50 },
                { "ArbitraryMaterialName2", 250 },
                { "ArbitraryMaterialName3", 1000 }
            }
        },
        {
            "Extrusion.ExtruderB.Settings.Component1.MaterialName",
            new()
            {
                { "ArbitraryMaterialName1", 50 },
                { "ArbitraryMaterialName2", 250 },
                { "ArbitraryMaterialName3", 1000 }
            }
        }
    };

    private readonly ValueByColumnId<double?> _arbitraryDoubleByIdentifierColumnId = new()
    {
        {
            "Extrusion.ProducedLength.GoodProduction",
            11.1
        }
    };

    private readonly Mock<IMachineCachingService> _machineCachingServiceMock = new();
    private readonly Mock<IMachineSnapshotHttpClient> _machineSnapshotHttpClientMock = new();
    private readonly Mock<IAlarmDataHandlerHttpClient> _alarmDataHandlerHttpClientMock = new();
    private readonly Mock<ISettingsService> _settingsServiceMock = new();
    private readonly Mock<ILatestMachineSnapshotCachingService> _latestMachineSnapshotCachingServiceMock = new();
    private readonly AlarmService _alarmService;

    public MachineTimeSpanQueryIntegrationTests()
    {
        _machineCachingServiceMock
            .Setup(m => m.GetMachine(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(_arbitraryMachine);
        _machineCachingServiceMock
            .Setup(m => m.GetMachinesAsInternalListResponse(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalListResponse<Machine>(new List<Machine> { _arbitraryMachine }));
        _alarmService = new AlarmService(_alarmDataHandlerHttpClientMock.Object);
    }

    [Fact]
    public async Task Get_GetRawMaterialConsumptionByMaterial_With_Existing_Material_Consumption_Columns()
    {
        // Arrange
        _machineSnapshotHttpClientMock
            .Setup(s => s.GetGroupedSums(
                ArbitraryMachineId,
                It.IsAny<List<GroupAssignment>>(),
                It.IsAny<List<TimeRange>>(),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new InternalItemResponse<ValueByColumnId<GroupedSumByIdentifier>>(_arbitraryGroupedSumByIdentifierColumnId)
            );

        var executor = await InitializeExecutor();

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(GetRawMaterialConsumptionByMaterialQuery())
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        _machineSnapshotHttpClientMock.VerifyAll();
        _machineSnapshotHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Get_GetRawMaterialConsumptionByMaterial_With_No_Existing_Material_Consumption_Columns()
    {
        // Arrange
        _machineSnapshotHttpClientMock
            .Setup(s => s.GetGroupedSums(
                ArbitraryMachineId,
                It.IsAny<List<GroupAssignment>>(),
                It.IsAny<List<TimeRange>>(),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new InternalItemResponse<ValueByColumnId<GroupedSumByIdentifier>>(new ValueByColumnId<GroupedSumByIdentifier>())
            );

        var executor = await InitializeExecutor();

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(GetRawMaterialConsumptionByMaterialQuery())
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        _machineSnapshotHttpClientMock.VerifyAll();
        _machineSnapshotHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Get_GetRawMaterialConsumptionByMaterial_With_Invalid_From_And_To()
    {
        // Arrange
        var executor = await InitializeExecutor();

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                GetRawMaterialConsumptionByMaterialQuery(
                    from: DateTime.UnixEpoch.AddDays(1),
                    to: DateTime.UnixEpoch
                ))
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        _machineSnapshotHttpClientMock.VerifyAll();
        _machineSnapshotHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Get_GetRawMaterialConsumptionByMaterial_With_Unknown_Machine_Id()
    {
        // Arrange
        var executor = await InitializeExecutor();

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                GetRawMaterialConsumptionByMaterialQuery(
                    machineId: "ArbitraryId"
                ))
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        _machineSnapshotHttpClientMock.VerifyAll();
        _machineSnapshotHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Get_GoodLength_Returns_Value_And_Unit()
    {
        // Arrange
        _machineSnapshotHttpClientMock
            .Setup(s => s.GetSumValues(
                ArbitraryMachineId,
                It.IsAny<List<string>>(),
                It.IsAny<List<TimeRange>>(),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()
            )).ReturnsAsync(
                new InternalItemResponse<ValueByColumnId<double?>>(_arbitraryDoubleByIdentifierColumnId)
            );
        InitializeMachineSnapshotServices(SnapshotColumnIds.ExtrusionProducedLengthGoodProduction, 11.1);

        var executor = await InitializeExecutor();

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(GetGoodLengthQuery())
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        _machineSnapshotHttpClientMock.VerifyAll();
        _machineSnapshotHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Get_AllAlarms_Via_TimeSpan()
    {
        // Arrange
        var executor = await InitializeExecutor();

        var mockAlarms = new List<Alarm>
        {
            new Alarm
            {
                Id = "1",
                MachineId = ArbitraryMachineId,
                AlarmCode = "606-42",
                StartTimestamp = default,
                EndTimestamp = DateTime.UnixEpoch.AddDays(3),
                AcknowledgeTimestamp = DateTime.UnixEpoch.AddDays(1),
                AlarmLevel = null,
                AlarmNumber = 1,
                AlarmText = null,
                ModuleCode = null,
                ModuleName = null,
                InfoText = null
            },
            new Alarm
            {
                Id = "2",
                MachineId = ArbitraryMachineId,
                AlarmCode = "606-43",
                StartTimestamp = default,
                EndTimestamp = DateTime.UnixEpoch.AddDays(2),
                AcknowledgeTimestamp = DateTime.UnixEpoch.AddDays(2),
                AlarmLevel = null,
                AlarmNumber = 1,
                AlarmText = null,
                ModuleCode = null,
                ModuleName = null,
                InfoText = null
            }
        };

        _alarmDataHandlerHttpClientMock.Setup(mock => mock.GetAlarms(
            It.IsAny<CancellationToken>(),
            ArbitraryMachineId,
            It.IsAny<DateTime?>(),
            It.IsAny<DateTime?>(),
            It.IsAny<int?>(),
            It.IsAny<int?>(),
            It.IsAny<bool?>(),
            It.IsAny<string?>(),
            It.IsAny<bool?>()
        )).ReturnsAsync(new InternalListResponse<Alarm>(mockAlarms));

        CreateUserSettingsMockForLanguage();
        var from = DateTime.UnixEpoch.AddDays(2);
        var to = DateTime.UnixEpoch.AddDays(5);
        var query =
            $@"{{
                machineTimeSpan(from: ""{from:yyyy-MM-ddTHH:mm:ss.fffK}"", machineId: ""{ArbitraryMachineId}"", to: ""{to:yyyy-MM-ddTHH:mm:ss.fffK}"")
                {{
                    machineAlarms {{
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
    public async Task Get_AllAlarms_Via_TimeSpan_But_AlarmDataHandler_Returns_Empty_List()
    {
        // Arrange
        var executor = await InitializeExecutor();

        var mockAlarms = new List<Alarm>();

        _alarmDataHandlerHttpClientMock.Setup(mock => mock.GetAlarms(
            It.IsAny<CancellationToken>(),
            ArbitraryMachineId,
            It.IsAny<DateTime?>(),
            It.IsAny<DateTime?>(),
            It.IsAny<int?>(),
            It.IsAny<int?>(),
            It.IsAny<bool?>(),
            It.IsAny<string?>(),
            It.IsAny<bool?>()
        )).ReturnsAsync(new InternalListResponse<Alarm>(mockAlarms));

        CreateUserSettingsMockForLanguage();
        var from = DateTime.UnixEpoch.AddDays(2);
        var to = DateTime.UnixEpoch.AddDays(5);
        var query =
            $@"{{
                machineTimeSpan(from: ""{from:yyyy-MM-ddTHH:mm:ss.fffK}"", machineId: ""{ArbitraryMachineId}"", to: ""{to:yyyy-MM-ddTHH:mm:ss.fffK}"")
                {{
                    machineAlarms {{
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
    public async Task Get_AllAlarms_Via_TimeSpan_But_AlarmDataHandler_Returns_Exception_In_DataResult()
    {
        // Arrange
        var executor = await InitializeExecutor();

        _alarmDataHandlerHttpClientMock.Setup(mock => mock.GetAlarms(
            It.IsAny<CancellationToken>(),
            ArbitraryMachineId,
            It.IsAny<DateTime?>(),
            It.IsAny<DateTime?>(),
            It.IsAny<int?>(),
            It.IsAny<int?>(),
            It.IsAny<bool?>(),
            It.IsAny<string?>(),
            It.IsAny<bool?>()
        )).ReturnsAsync(new InternalListResponse<Alarm>(StatusCodes.Status500InternalServerError, new Exception("I am a test")));

        CreateUserSettingsMockForLanguage();
        var from = DateTime.UnixEpoch.AddDays(2);
        var to = DateTime.UnixEpoch.AddDays(5);
        var query =
            $@"{{
                machineTimeSpan(from: ""{from:yyyy-MM-ddTHH:mm:ss.fffK}"", machineId: ""{ArbitraryMachineId}"", to: ""{to:yyyy-MM-ddTHH:mm:ss.fffK}"")
                {{
                    machineAlarms {{
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
    public async Task Get_MachineAlarmCount_Via_TimeSpan()
    {
        // Arrange
        var executor = await InitializeExecutor();
        CreateUserSettingsMockForLanguage();
        _alarmDataHandlerHttpClientMock.Setup(mock => mock.GetAlarmCount(
            It.IsAny<CancellationToken>(),
            ArbitraryMachineId,
            It.IsAny<DateTime?>(),
            It.IsAny<DateTime?>(),
            It.IsAny<string?>(),
            It.IsAny<bool?>()
        )).ReturnsAsync(new InternalItemResponse<long>(2));

        var from = DateTime.UnixEpoch.AddDays(2);
        var to = DateTime.UnixEpoch.AddDays(5);
        var query =
            $@"{{
                machineTimeSpan(from: ""{from:yyyy-MM-ddTHH:mm:ss.fffK}"", machineId: ""{ArbitraryMachineId}"", to: ""{to:yyyy-MM-ddTHH:mm:ss.fffK}"")
                {{
                    machineAlarmCount
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
    public async Task Get_MachineAlarmCount_Via_TimeSpan_But_AlarmDataHandler_Returns_Exception()
    {
        // Arrange
        var executor = await InitializeExecutor();
        CreateUserSettingsMockForLanguage();
        _alarmDataHandlerHttpClientMock.Setup(mock => mock.GetAlarmCount(
            It.IsAny<CancellationToken>(),
            ArbitraryMachineId,
            It.IsAny<DateTime?>(),
            It.IsAny<DateTime?>(),
            It.IsAny<string?>(),
            It.IsAny<bool?>()
        )).ReturnsAsync(new InternalItemResponse<long>(StatusCodes.Status500InternalServerError, new Exception("I am a test")));

        var from = DateTime.UnixEpoch.AddDays(2);
        var to = DateTime.UnixEpoch.AddDays(5);
        var query =
            $@"{{
                machineTimeSpan(from: ""{from:yyyy-MM-ddTHH:mm:ss.fffK}"", machineId: ""{ArbitraryMachineId}"", to: ""{to:yyyy-MM-ddTHH:mm:ss.fffK}"")
                {{
                    machineAlarmCount
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

    private void CreateUserSettingsMockForLanguage()
    {
        var setting = new Setting(UserSettingIds.Language, "de-de");

        _settingsServiceMock.Setup(mock => mock.GetSettingsForUserAndMachine(
            null,
            UserId,
            It.IsAny<string>(),
            It.IsAny<CancellationToken>(),
            false
        )).ReturnsAsync(new InternalItemResponse<Setting>(setting));
    }

    private static string GetRawMaterialConsumptionByMaterialQuery(
        string machineId = ArbitraryMachineId,
        DateTime? from = null,
        DateTime? to = null
    )
    {
        var fromDate = from ?? DateTime.UnixEpoch;
        var toDate = to ?? DateTime.UnixEpoch.AddDays(1);

        var iso8601From = fromDate.ToString("yyyy-MM-ddTHH:mm:ss.fffK");
        var iso8601To = toDate.ToString("yyyy-MM-ddTHH:mm:ss.fffK");

        return @$" {{
                machineTimeSpan(from: ""{iso8601From}"", machineId: ""{machineId}"", to: ""{iso8601To}"")
                {{
                    ... on ExtrusionMachineTimeSpan
                    {{
                        __typename
                        rawMaterialConsumptionByMaterial
                        {{
                            key
                            value
                            {{
                                unit
                                value
                            }}
                        }}
                    }}
                }}
            }}
        ";
    }

    private static string GetGoodLengthQuery()
    {
        var fromDate = DateTime.UnixEpoch;
        var toDate = DateTime.UnixEpoch.AddDays(1);

        var iso8601From = fromDate.ToString("yyyy-MM-ddTHH:mm:ss.fffK");
        var iso8601To = toDate.ToString("yyyy-MM-ddTHH:mm:ss.fffK");

        return $@" {{
                machineTimeSpan(from: ""{iso8601From}"", machineId: ""{ArbitraryMachineId}"", to: ""{iso8601To}"")
                {{
                    ... on ExtrusionMachineTimeSpan
                    {{
                        __typename
                        goodLength
                        {{
                            unit
                            value
                        }}
                    }}
                }}
            }}
        ";
    }

    private async Task<IRequestExecutor> InitializeExecutor()
    {
        var machineService = new MachineService(
            _machineCachingServiceMock.Object,
            new Mock<ILogger<MachineService>>().Object);
        var machineSnapshotService = new MachineSnapshotService();
        var materialConsumptionService = new MaterialConsumptionService(machineSnapshotService);
        var userSettingsService = new UserSettingsService(_settingsServiceMock.Object);
        var userSettingsBatchLoader = new UserSettingsBatchLoader(_settingsServiceMock.Object, new DelayedBatchScheduler());
        var latestSnapShotCacheDataLoader =
            new LatestSnapshotCacheDataLoader(_latestMachineSnapshotCachingServiceMock.Object);

        return await new ServiceCollection()
            .AddSingleton(userSettingsBatchLoader)
            .AddSingleton(latestSnapShotCacheDataLoader)
            .AddSingleton(_machineSnapshotHttpClientMock.Object)
            .AddSingleton<IMachineService>(machineService)
            .AddSingleton<IMachineSnapshotService>(machineSnapshotService)
            .AddSingleton<IMaterialConsumptionService>(materialConsumptionService)
            .AddSingleton<IAlarmService>(_alarmService)
            .AddSingleton<IUserSettingsService>(userSettingsService)
            .AddLogging()
            .AddGraphQLServer()
            .AddAuthorization()
            .AddQueryType(q => q.Name("Query"))
            .AddType<FrameworkAPI.Queries.MachineTimeSpanQuery>()
            .AddType<ExtrusionMachineTimeSpan>()
            .AddType<MachineTimeSpan>()
            .BuildRequestExecutorAsync();
    }

    private void InitializeMachineSnapshotServices(string columnId, object? value)
    {
        var snapshotMetaDto = new SnapshotMetaDto(
            ArbitraryMachineId, "fakeHash", DateTime.MinValue, [new SnapshotColumnUnitDto(columnId, $"FakeUnit_{columnId}")]);
        var snapshotDto = new SnapshotDto(
            [new SnapshotColumnValueDto(columnId, value)],
            snapshotTime: DateTime.UtcNow);

        _latestMachineSnapshotCachingServiceMock
            .Setup(m => m.GetLatestMachineSnapshot(ArbitraryMachineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new InternalItemResponse<MachineSnapshotResponse>(
                    new MachineSnapshotResponse(snapshotMetaDto, snapshotDto)));
    }
}