using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.DataLoaders;
using FrameworkAPI.Models;
using FrameworkAPI.Schema.Machine;
using FrameworkAPI.Schema.Machine.ActualProcessValues;
using FrameworkAPI.Services;
using FrameworkAPI.Services.Interfaces;
using FrameworkAPI.Test.Services.Helpers;
using FrameworkAPI.Test.TestHelpers;
using HotChocolate;
using HotChocolate.Execution;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Snapshooter.Xunit;
using WuH.Ruby.Common.Core;
using WuH.Ruby.MachineDataHandler.Client;
using WuH.Ruby.MachineSnapShooter.Client;
using WuH.Ruby.MachineSnapShooter.Client.Models;
using WuH.Ruby.MachineSnapShooter.Client.Queue;
using WuH.Ruby.MetaDataHandler.Client;
using WuH.Ruby.OpcUaForwarder.Client;
using WuH.Ruby.ProcessDataReader.Client;
using Xunit;
using VariableUnits = WuH.Ruby.MetaDataHandler.Client.VariableUnits;

namespace FrameworkAPI.Test.Queries.MachineQuery;

public class ExtrusionMachineQueryIntegrationTests
{
    private readonly Mock<IProcessDataService> _processDataServiceMock = new();
    private readonly Mock<IProcessDataReaderHttpClient> _processDataReaderHttpClientMock = new();
    private readonly Mock<IMetaDataHandlerHttpClient> _metaDataHandlerHttpClientMock = new();
    private readonly Mock<IMachineSnapshotHttpClient> _machineSnapshotHttpClientMock = new();
    private readonly Mock<IMachineSnapshotQueueWrapper> _machineSnapshotQueueWrapperMock = new();
    private readonly Mock<IMachineCachingService> _machineCachingServiceMock = new();
    private readonly Mock<IProcessDataCachingService> _processDataCachingService = new();
    private readonly Mock<ILatestMachineSnapshotCachingService> _latestMachineSnapshotCachingServiceMock = new();
    private readonly Mock<ILogger<MachineTrendCachingService>> _loggerMock = new();
    private readonly string _machineId = "EQ00001";
    private readonly DateTime _timestamp = DateTime.UnixEpoch.AddHours(2);
    private readonly VariableUnits _variableUnits = new()
    {
        Si = new VariableUnits.UnitWithCoefficient
        {
            Unit = "mm"
        }
    };

    [Fact]
    public async Task GetExtrusionMachine_For_Primary_Profile_Should_Return_Primary_Profile()
    {
        // Arrange
        var executor = await InitializeExecutor();

        InitializeBlowFilmMachineMock();
        SetupExtrusionProfileMocks(ExtrusionThicknessMeasurementType.Primary);

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    machine(machineId: ""{_machineId}"") {{
                        ... on ExtrusionMachine {{
                            actualProcessValues {{
                                thicknessProfiles{{
                                    primaryProfile {{
                                        type
                                    }}
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
        _processDataServiceMock.VerifyAll();
        _processDataServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetExtrusionMachine_For_MdoProfileA_Should_Return_MdoProfileA()
    {
        // Arrange
        var executor = await InitializeExecutor();

        InitializeBlowFilmMachineMock();
        SetupExtrusionProfileMocks(ExtrusionThicknessMeasurementType.MdoWinderA);

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    machine(machineId: ""{_machineId}"") {{
                        ... on ExtrusionMachine {{
                            actualProcessValues {{
                                thicknessProfiles{{
                                    mdoProfileA {{
                                        type
                                    }}
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
        _processDataServiceMock.VerifyAll();
        _processDataServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetExtrusionMachine_For_MdoProfileB_Should_Return_MdoProfileB()
    {
        // Arrange
        var executor = await InitializeExecutor();

        InitializeBlowFilmMachineMock();
        SetupExtrusionProfileMocks(ExtrusionThicknessMeasurementType.MdoWinderB);

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    machine(machineId: ""{_machineId}"") {{
                        ... on ExtrusionMachine {{
                            actualProcessValues {{
                                thicknessProfiles{{
                                    mdoProfileB {{
                                        type
                                    }}
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
        _processDataServiceMock.VerifyAll();
        _processDataServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetExtrusionMachine_Should_Return_Actual_Process_Values_Of_TwoSigma()
    {
        // Arrange
        var executor = await InitializeExecutor();

        InitializeCastFilmMachineMock();

        var snapshotMetaDto = new SnapshotMetaDto(
            _machineId,
            "fakeHash",
            DateTime.MinValue,
            [new SnapshotColumnUnitDto(SnapshotColumnIds.ExtrusionQualityActualValuesTwoSigma, "%")]);

        var snapshotDto = new SnapshotDto(
            [new SnapshotColumnValueDto(SnapshotColumnIds.ExtrusionQualityActualValuesTwoSigma, 77.7)],
            DateTime.UtcNow);

        var machineSnapshotResponse = new MachineSnapshotResponse(snapshotMetaDto, snapshotDto);

        _latestMachineSnapshotCachingServiceMock
            .Setup(m => m.GetLatestMachineSnapshot(_machineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<MachineSnapshotResponse>(machineSnapshotResponse));

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                   machine(machineId: ""{_machineId}"") {{
                     ... on ExtrusionMachine {{
                       actualProcessValues {{
                         twoSigma {{
                           lastValue
                           unit
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

        _latestMachineSnapshotCachingServiceMock.VerifyAdd(m =>
            m.CacheChanged += It.IsAny<EventHandler<LiveSnapshotEventArgs>>());
        _latestMachineSnapshotCachingServiceMock.VerifyAll();
        _latestMachineSnapshotCachingServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetExtrusionMachine_Should_Return_Actual_Process_Values_Of_Thickness()
    {
        // Arrange
        var executor = await InitializeExecutor();
        const string mockedColumnId = SnapshotColumnIds.ExtrusionFormatActualValuesThickness;

        InitializeCastFilmMachineMock();

        var snapshotMetaDto = new SnapshotMetaDto(
            _machineId,
            "fakeHash",
            DateTime.MinValue,
            [new SnapshotColumnUnitDto(mockedColumnId, "mm")]);

        var snapshotDto = new SnapshotDto(
            [new SnapshotColumnValueDto(mockedColumnId, 1.7)],
            DateTime.UtcNow);

        var machineSnapshotResponse = new MachineSnapshotResponse(snapshotMetaDto, snapshotDto);

        _latestMachineSnapshotCachingServiceMock
            .Setup(m => m.GetLatestMachineSnapshot(_machineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<MachineSnapshotResponse>(machineSnapshotResponse));

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                        machine(machineId: ""{_machineId}"") {{
                            ... on ExtrusionMachine {{
                                actualProcessValues {{
                                    thickness {{
                                        lastValue
                                        unit
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

        _latestMachineSnapshotCachingServiceMock.VerifyAdd(m =>
            m.CacheChanged += It.IsAny<EventHandler<LiveSnapshotEventArgs>>());
        _latestMachineSnapshotCachingServiceMock.VerifyAll();
        _latestMachineSnapshotCachingServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetExtrusionMachine_Should_Return_Actual_Process_Values_Of_Width()
    {
        // Arrange
        var executor = await InitializeExecutor();
        const string mockedColumnId = SnapshotColumnIds.ExtrusionFormatActualValuesWidth;

        InitializeCastFilmMachineMock();

        var snapshotMetaDto = new SnapshotMetaDto(
            _machineId,
            "fakeHash",
            DateTime.MinValue,
            [new SnapshotColumnUnitDto(mockedColumnId, "mm")]);

        var snapshotDto = new SnapshotDto(
            [new SnapshotColumnValueDto(mockedColumnId, 4.11)],
            DateTime.UtcNow);

        var machineSnapshotResponse = new MachineSnapshotResponse(snapshotMetaDto, snapshotDto);

        _latestMachineSnapshotCachingServiceMock
            .Setup(m => m.GetLatestMachineSnapshot(_machineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<MachineSnapshotResponse>(machineSnapshotResponse));

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    machine(machineId: ""{_machineId}"") {{
                        ... on ExtrusionMachine {{
                            actualProcessValues {{
                                width {{
                                    lastValue
                                    unit
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

        _latestMachineSnapshotCachingServiceMock.VerifyAdd(m =>
            m.CacheChanged += It.IsAny<EventHandler<LiveSnapshotEventArgs>>());
        _latestMachineSnapshotCachingServiceMock.VerifyAll();
        _latestMachineSnapshotCachingServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetExtrusionMachine_Should_Return_Actual_Process_Values_Of_RollLengthA()
    {
        // Arrange
        var executor = await InitializeExecutor();
        const string mockedColumnId = SnapshotColumnIds.ExtrusionWindingStationAActualValuesRollLength;

        InitializeCastFilmMachineMock();

        var snapshotMetaDto = new SnapshotMetaDto(
            _machineId,
            "fakeHash",
            DateTime.MinValue,
            [new SnapshotColumnUnitDto(mockedColumnId, "mm")]);

        var snapshotDto = new SnapshotDto(
            [new SnapshotColumnValueDto(mockedColumnId, 1.58)],
            DateTime.UtcNow);

        var machineSnapshotResponse = new MachineSnapshotResponse(snapshotMetaDto, snapshotDto);

        _latestMachineSnapshotCachingServiceMock
            .Setup(m => m.GetLatestMachineSnapshot(_machineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<MachineSnapshotResponse>(machineSnapshotResponse));

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    machine(machineId: ""{_machineId}"") {{
                        ... on ExtrusionMachine {{
                            actualProcessValues {{
                                rollLengthA {{
                                    lastValue
                                    unit
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

        _latestMachineSnapshotCachingServiceMock.VerifyAdd(m =>
            m.CacheChanged += It.IsAny<EventHandler<LiveSnapshotEventArgs>>());
        _latestMachineSnapshotCachingServiceMock.VerifyAll();
        _latestMachineSnapshotCachingServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetExtrusionMachine_Should_Return_Actual_Process_Values_Of_RollLengthB()
    {
        // Arrange
        var executor = await InitializeExecutor();
        const string mockedColumnId = SnapshotColumnIds.ExtrusionWindingStationBActualValuesRollLength;

        InitializeCastFilmMachineMock();
        var snapshotMetaDto = new SnapshotMetaDto(
            _machineId,
            "fakeHash",
            DateTime.MinValue,
            [new SnapshotColumnUnitDto(mockedColumnId, "mm")]);
        var snapshotDto = new SnapshotDto(
            [new SnapshotColumnValueDto(mockedColumnId, 4.50)],
            DateTime.UtcNow);
        var machineSnapshotResponse = new MachineSnapshotResponse(snapshotMetaDto, snapshotDto);

        _latestMachineSnapshotCachingServiceMock
            .Setup(m => m.GetLatestMachineSnapshot(_machineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<MachineSnapshotResponse>(machineSnapshotResponse));

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    machine(machineId: ""{_machineId}"") {{
                        ... on ExtrusionMachine {{
                            actualProcessValues {{
                                rollLengthB {{
                                    lastValue
                                    unit
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

        _latestMachineSnapshotCachingServiceMock.VerifyAdd(m =>
            m.CacheChanged += It.IsAny<EventHandler<LiveSnapshotEventArgs>>());
        _latestMachineSnapshotCachingServiceMock.VerifyAll();
        _latestMachineSnapshotCachingServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetExtrusionMachine_Should_Return_Actual_Process_Values_Of_PowerOutput()
    {
        // Arrange
        var executor = await InitializeExecutor();
        const string mockedColumnId = SnapshotColumnIds.ExtrusionEnergyActualValuesElectricalPowerConsumption;

        InitializeCastFilmMachineMock();

        var snapshotMetaDto = new SnapshotMetaDto(
            _machineId,
            "fakeHash",
            DateTime.MinValue,
            [new SnapshotColumnUnitDto(mockedColumnId, "mw")]);
        var snapshotDto = new SnapshotDto(
            [new SnapshotColumnValueDto(mockedColumnId, (int?)460)],
            DateTime.UtcNow);
        var machineSnapshotResponse = new MachineSnapshotResponse(snapshotMetaDto, snapshotDto);

        _latestMachineSnapshotCachingServiceMock
            .Setup(m => m.GetLatestMachineSnapshot(_machineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<MachineSnapshotResponse>(machineSnapshotResponse));

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    machine(machineId: ""{_machineId}"") {{
                        ... on ExtrusionMachine {{
                            actualProcessValues {{
                                powerOutput {{
                                    lastValue
                                    unit
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

        _latestMachineSnapshotCachingServiceMock.VerifyAdd(m =>
            m.CacheChanged += It.IsAny<EventHandler<LiveSnapshotEventArgs>>());
        _latestMachineSnapshotCachingServiceMock.VerifyAll();
        _latestMachineSnapshotCachingServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetExtrusionMachine_Should_Return_Actual_Process_Values_Of_thicknessProfile()
    {
        // Arrange
        var executor = await InitializeExecutor();

        InitializeBlowFilmMachineMock();
        SetupExtrusionProfileMocks();

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    machine(machineId: ""{_machineId}"", timestamp: ""{_timestamp}"") {{
                        ... on ExtrusionMachine {{
                            actualProcessValues {{
                                thicknessProfiles{{
                                    mostRelevantProfile {{
                                        type
                                        xAxisUnit
                                        meanValue{{
                                            unit
                                            value
                                        }}
                                        twoSigma {{
                                            unit
                                            value
                                        }}
                                        isControllerOn
                                        controlElements {{
                                            key
                                            value
                                        }}
                                        dataPointsCount
                                        dataPoints {{
                                            key
                                            value
                                        }}
                                    }}
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
        _processDataServiceMock.VerifyAll();
        _processDataServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetExtrusionMachine_Should_Return_Actual_Process_Values_Of_thicknessProfile_Without_Date()
    {
        // Arrange
        var executor = await InitializeExecutor();

        InitializeBlowFilmMachineMock();
        SetupExtrusionProfileMocks();

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    machine(machineId: ""{_machineId}"") {{
                        ... on ExtrusionMachine {{
                            actualProcessValues {{
                                thicknessProfiles{{
                                    mostRelevantProfile {{
                                        type
                                        xAxisUnit
                                        meanValue{{
                                            unit
                                            value
                                        }}
                                        twoSigma {{
                                            unit
                                            value
                                        }}
                                        isControllerOn
                                        controlElements {{
                                            key
                                            value
                                        }}
                                        dataPointsCount
                                        dataPoints {{
                                            key
                                            value
                                        }}
                                    }}
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
        _processDataServiceMock.VerifyAll();
        _processDataServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetExtrusionMachine_Actual_Process_Value_Gets_Null_From_ProcessDataService_On_DataPoints()
    {
        // Arrange
        var executor = await InitializeExecutor();

        InitializeBlowFilmMachineMock();
        SetupExtrusionProfileMocks();
        SetupProcessDataServiceMock(Constants.LastPartOfPath.PrimaryProfile, null);

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    machine(machineId: ""{_machineId}"") {{
                        ... on ExtrusionMachine {{
                            actualProcessValues {{
                                thicknessProfiles{{
                                    primaryProfile {{
                                        type
                                        xAxisUnit
                                    }}
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
        _processDataServiceMock.VerifyAll();
        _processDataServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetExtrusionMachine_Actual_Process_Value_With_Timestamp_Gets_Exception_On_Mean_Value()
    {
        // Arrange
        var executor = await InitializeExecutor();

        InitializeCastFilmMachineMock();
        MockDataPoints();
        MockMeanValue(exception: new Exception("Im a test"));

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    machine(machineId: ""{_machineId}"", timestamp: ""{_timestamp}"") {{
                        ... on ExtrusionMachine {{
                            actualProcessValues {{
                                thicknessProfiles{{
                                    primaryProfile {{
                                        meanValue{{
                                            unit
                                            value
                                        }}
                                    }}
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
        _processDataServiceMock.VerifyAll();
        _processDataServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetExtrusionMachine_Actual_Process_Value_With_Timestamp_Gets_Exception_On_DataPoints()
    {
        // Arrange
        var executor = await InitializeExecutor();

        InitializeCastFilmMachineMock();
        MockDataPoints(exception: new Exception("Im a test"));

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    machine(machineId: ""{_machineId}"", timestamp: ""{_timestamp}"") {{
                        ... on ExtrusionMachine {{
                            actualProcessValues {{
                                thicknessProfiles{{
                                    primaryProfile {{
                                        type
                                        xAxisUnit
                                    }}
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
        _processDataServiceMock.VerifyAll();
        _processDataServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetExtrusionMachine_Gets_Null_Value_But_No_Exception()
    {
        // Arrange
        var executor = await InitializeExecutor();
        const string mockedColumnId = SnapshotColumnIds.ExtrusionEnergyActualValuesElectricalPowerConsumption;

        InitializeCastFilmMachineMock();

        var snapshotMetaDto = new SnapshotMetaDto(
            _machineId,
            "fakeHash",
            DateTime.MinValue,
            [new SnapshotColumnUnitDto(mockedColumnId, "mw")]);

        var snapshotDto = new SnapshotDto(
            [new SnapshotColumnValueDto(mockedColumnId, null)],
            DateTime.UtcNow);

        var machineSnapshotResponse = new MachineSnapshotResponse(snapshotMetaDto, snapshotDto);

        _latestMachineSnapshotCachingServiceMock
            .Setup(m => m.GetLatestMachineSnapshot(_machineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<MachineSnapshotResponse>(machineSnapshotResponse));

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    machine(machineId: ""{_machineId}"") {{
                        ... on ExtrusionMachine {{
                            actualProcessValues {{
                                powerOutput {{
                                    lastValue
                                    unit
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

        _latestMachineSnapshotCachingServiceMock.VerifyAdd(m =>
            m.CacheChanged += It.IsAny<EventHandler<LiveSnapshotEventArgs>>());
        _latestMachineSnapshotCachingServiceMock.VerifyAll();
        _latestMachineSnapshotCachingServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetExtrusionMachine_Gets_Exception_From_LatestMachineSnapshot()
    {
        // Arrange
        var executor = await InitializeExecutor();
        InitializeCastFilmMachineMock();

        _latestMachineSnapshotCachingServiceMock
            .Setup(m => m.GetLatestMachineSnapshot(_machineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<MachineSnapshotResponse>(404, new Exception("Something very unexpected happened")));

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    machine(machineId: ""{_machineId}"") {{
                        ... on ExtrusionMachine {{
                            actualProcessValues {{
                                powerOutput {{
                                    lastValue
                                    unit
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

        _latestMachineSnapshotCachingServiceMock.VerifyAdd(m =>
            m.CacheChanged += It.IsAny<EventHandler<LiveSnapshotEventArgs>>());
        _latestMachineSnapshotCachingServiceMock.VerifyAll();
        _latestMachineSnapshotCachingServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetExtrusionMachine_Should_Return_Base_Properties()
    {
        // Arrange
        var executor = await InitializeExecutor();

        InitializeCastFilmMachineMock();

        var snapshotMetaDto = new SnapshotMetaDto(
            _machineId,
            "fakeHash",
            DateTime.MinValue,
            [
                new SnapshotColumnUnitDto(SnapshotColumnIds.ExtrusionThroughput, "kg/h"),
                new SnapshotColumnUnitDto(SnapshotColumnIds.ExtrusionSpeed, "m/min")
            ]);

        var snapshotDto = new SnapshotDto(
            [
                new SnapshotColumnValueDto(SnapshotColumnIds.ExtrusionThroughput, 77.7),
                new SnapshotColumnValueDto(SnapshotColumnIds.ExtrusionSpeed, 88.8)
            ],
            DateTime.UtcNow);

        var machineSnapshotResponse = new MachineSnapshotResponse(snapshotMetaDto, snapshotDto);

        _latestMachineSnapshotCachingServiceMock
            .Setup(m => m.GetLatestMachineSnapshot(_machineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<MachineSnapshotResponse>(machineSnapshotResponse));

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    machine(machineId: ""{_machineId}"") {{
                        ... on ExtrusionMachine {{
                            throughputRate {{
                                value
                                unit
                            }}
                            lineSpeed {{
                                value
                                unit
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

        _latestMachineSnapshotCachingServiceMock.VerifyAdd(m =>
            m.CacheChanged += It.IsAny<EventHandler<LiveSnapshotEventArgs>>());
        _latestMachineSnapshotCachingServiceMock.VerifyAll();
        _latestMachineSnapshotCachingServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetExtrusionMachine_Should_Return_Trend_Of_Base_Properties()
    {
        // Arrange
        var executor = await InitializeExecutor();

        InitializeCastFilmMachineMock();

        var machineTime = new DateTime(year: 2023, month: 1, day: 15, hour: 0, minute: 0, second: 0, DateTimeKind.Utc);

        _processDataReaderHttpClientMock
            .Setup(m => m.GetLastReceivedOpcUaServerTime(It.IsAny<CancellationToken>(), _machineId))
            .ReturnsAsync(new InternalItemResponse<DateTime>(machineTime));

        var snapshotMetaDto = new SnapshotMetaDto(_machineId, "fakeHash", DateTime.MinValue, []);
        var snapshotDtos = new List<SnapshotDto>();

        for (var i = 0; i < (int)Constants.MachineTrend.TrendTimeSpan.TotalMinutes; i++)
        {
            snapshotDtos.Add(new SnapshotDto(
                [
                    new SnapshotColumnValueDto(SnapshotColumnIds.ExtrusionThroughput, i == 0 ? null : i * 1.5),
                    new SnapshotColumnValueDto(SnapshotColumnIds.ExtrusionSpeed, i == 0 ? null : i * 2.5)
                ],
                machineTime.Subtract(TimeSpan.FromMinutes(i))));
        }

        var machineSnapshotResponse = new MachineSnapshotResponse(snapshotMetaDto, snapshotDtos.First());

        _latestMachineSnapshotCachingServiceMock
            .Setup(m => m.GetLatestMachineSnapshot(_machineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<MachineSnapshotResponse>(machineSnapshotResponse));

        var machineSnapshotListResponse = new MachineSnapshotListResponse(snapshotMetaDto, snapshotDtos, []);

        _machineSnapshotHttpClientMock
            .Setup(m => m.GetSnapshotsInTimeRanges(_machineId, It.IsAny<List<string>>(), It.IsAny<List<TimeRange>>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<MachineSnapshotListResponse>(machineSnapshotListResponse));

        _machineSnapshotHttpClientMock
            .Setup(m => m.GetFirstSnapshot(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string machineId, CancellationToken ct) => new InternalItemResponse<MachineSnapshotResponse>(new MachineSnapshotResponse(
                new SnapshotMetaDto(machineId, "hash", DateTime.MinValue, []),
                new SnapshotDto([], DateTime.UnixEpoch.AddDays(-1))
            )));

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    machine(machineId: ""{_machineId}"") {{
                        ... on ExtrusionMachine {{
                            throughputRate {{
                                trendOfLast8Hours {{
                                    time
                                    value
                                }}
                            }}
                            lineSpeed {{
                                trendOfLast8Hours {{
                                    time
                                    value
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

        _processDataReaderHttpClientMock.VerifyAll();
        _processDataReaderHttpClientMock.VerifyNoOtherCalls();

        _latestMachineSnapshotCachingServiceMock.VerifyAdd(m =>
            m.CacheChanged += It.IsAny<EventHandler<LiveSnapshotEventArgs>>());
        _latestMachineSnapshotCachingServiceMock.VerifyAll();
        _latestMachineSnapshotCachingServiceMock.VerifyNoOtherCalls();

        _machineSnapshotHttpClientMock.VerifyAll();
        _machineSnapshotHttpClientMock.VerifyNoOtherCalls();
    }

    private void InitializeBlowFilmMachineMock()
    {
        var machine = MachineMock.GenerateBlowFilm(_machineId);

        _machineCachingServiceMock
            .Setup(m => m.GetMachine(_machineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(machine);
    }

    private void InitializeCastFilmMachineMock()
    {
        var machine = MachineMock.GenerateCastFilm(_machineId);

        _machineCachingServiceMock
            .Setup(m => m.GetMachine(_machineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(machine);
    }

    private void SetupExtrusionProfileMocks(ExtrusionThicknessMeasurementType profileType = ExtrusionThicknessMeasurementType.Primary)
    {
        MockDataPoints(profileType);
        MockMeanValue(profileType);
        MockTwoSigmaValue(profileType);
        MockIsControllerOn(profileType);

        if (profileType == ExtrusionThicknessMeasurementType.Primary)
        {
            MockControlElements();
        }
    }

    private void MockDataPoints(ExtrusionThicknessMeasurementType profileType = ExtrusionThicknessMeasurementType.Primary, List<double>? entryList = null, Exception? exception = null)
    {
        var path = profileType switch
        {
            ExtrusionThicknessMeasurementType.MdoWinderA => Constants.LastPartOfPath.MdoProfileA,
            ExtrusionThicknessMeasurementType.MdoWinderB => Constants.LastPartOfPath.MdoProfileB,
            _ => Constants.LastPartOfPath.PrimaryProfile,
        };

        if (exception is not null)
        {
            SetupProcessDataServiceMock(path, exception: exception);
            return;
        }

        var profileEntries = entryList ?? Enumerable.Range(1, 500).Select(i => (double)i).ToList();

        SetupProcessDataServiceMock(path, JsonConvert.SerializeObject(profileEntries));
    }

    private void MockMeanValue(ExtrusionThicknessMeasurementType profileType = ExtrusionThicknessMeasurementType.Primary, Exception? exception = null)
    {
        var path = profileType switch
        {
            ExtrusionThicknessMeasurementType.MdoWinderA => Constants.LastPartOfPath.MdoProfileAMeanValue,
            ExtrusionThicknessMeasurementType.MdoWinderB => Constants.LastPartOfPath.MdoProfileBMeanValue,
            _ => Constants.LastPartOfPath.PrimaryProfileMeanValue,
        };

        if (exception is not null)
        {
            SetupProcessDataServiceMock(path, exception: exception);
            return;
        }

        SetupProcessDataServiceMock(path, 100.0);
    }

    private void MockTwoSigmaValue(ExtrusionThicknessMeasurementType profileType = ExtrusionThicknessMeasurementType.Primary, Exception? exception = null)
    {
        var path = profileType switch
        {
            ExtrusionThicknessMeasurementType.MdoWinderA => Constants.LastPartOfPath.MdoProfileATwoSigma,
            ExtrusionThicknessMeasurementType.MdoWinderB => Constants.LastPartOfPath.MdoProfileBTwoSigma,
            _ => Constants.LastPartOfPath.PrimaryProfileTwoSigma,
        };

        if (exception is not null)
        {
            SetupProcessDataServiceMock(path, exception: exception);
            return;
        }

        SetupProcessDataServiceMock(path, 200.0);
    }

    private void MockIsControllerOn(
        ExtrusionThicknessMeasurementType profileType = ExtrusionThicknessMeasurementType.Primary,
        bool state = true,
        Exception? exception = null)
    {
        var profileControlPath = Constants.LastPartOfPath.ProfileControl;

        var otherPath = profileType switch
        {
            ExtrusionThicknessMeasurementType.Primary => Constants.LastPartOfPath.ThicknessGauge,
            ExtrusionThicknessMeasurementType.MdoWinderA => Constants.LastPartOfPath.WinderAContactDrive,
            _ => Constants.LastPartOfPath.WinderBContactDrive
        };

        if (exception is not null)
        {
            SetupProcessDataServiceMock(profileControlPath, exception: exception);
            SetupProcessDataServiceMock(otherPath, exception: exception);
            return;
        }

        var profileControlValue = profileType switch
        {
            ExtrusionThicknessMeasurementType.Primary => 1.0,
            ExtrusionThicknessMeasurementType.MdoWinderA => 4.0,
            _ => 4.0
        };

        SetupProcessDataServiceMock(profileControlPath, profileControlValue);
        SetupProcessDataServiceMock(
            otherPath,
             profileType switch
             {
                 ExtrusionThicknessMeasurementType.Primary => state,
                 ExtrusionThicknessMeasurementType.MdoWinderA => state ? 1.0 : 0.0,
                 _ => state ? 1.0 : 0.0
             });
    }

    private void MockControlElements(Exception? exception = null)
    {
        var path = Constants.LastPartOfPath.ControlElements;

        if (exception is not null)
        {
            SetupProcessDataServiceMock(path, exception: exception);
            return;
        }

        var controlElements = new List<double>
        {
            1,
            2,
            3
        };

        SetupProcessDataServiceMock(path, JsonConvert.SerializeObject(controlElements));
    }

    private void SetupProcessDataServiceMock(string path, object? value = null, Exception? exception = null)
    {
        (ProcessData processData, ProcessVariableMetaDataResponseItem metaData)? dataResultValue = exception is not null
                                ? null
                                : new ValueTuple<ProcessData, ProcessVariableMetaDataResponseItem>(
                                    new ProcessData()
                                    {
                                        Value = value,
                                        Timestamp = _timestamp,
                                        Path = path
                                    },
                                    new ProcessVariableMetaDataResponseItem()
                                    {
                                        Data = new ProcessVariableMetaData()
                                        {
                                            Units = _variableUnits,
                                            VariableIdentifier = path
                                        }
                                    }
                                    );

        _processDataServiceMock
            .Setup(mock => mock.GetProcessDataByLastPartOfPath(
                It.IsAny<ProcessDataByTimestampBatchDataLoader>(),
                It.IsAny<MachineMetaDataBatchDataLoader>(),
                It.IsAny<LatestProcessDataCacheDataLoader>(),
                _machineId,
                path,
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DataResult<(ProcessData processData, ProcessVariableMetaDataResponseItem metaData)?>(dataResultValue, exception));
    }

    private async Task<IRequestExecutor> InitializeExecutor()
    {
        var machineTimeService = new MachineTimeService(
            new OpcUaServerTimeCachingService(_processDataReaderHttpClientMock.Object, new Mock<IProcessDataQueueWrapper>().Object),
            _latestMachineSnapshotCachingServiceMock.Object,
            new Mock<ILogger<MachineTimeService>>().Object);
        var processDataService = new ProcessDataService();
        var machineSnapshotService = new MachineSnapshotService();

        return await new ServiceCollection()
            .AddSingleton(_machineSnapshotHttpClientMock.Object)
            .AddSingleton(_metaDataHandlerHttpClientMock.Object)
            .AddSingleton(new MachineMetaDataBatchDataLoader(_metaDataHandlerHttpClientMock.Object, new DelayedBatchScheduler()))
            .AddSingleton(new ProcessDataByTimestampBatchDataLoader(_processDataReaderHttpClientMock.Object, new DelayedBatchScheduler()))
            .AddSingleton(_latestMachineSnapshotCachingServiceMock.Object)
            .AddSingleton(_processDataCachingService.Object)
            .AddSingleton<IExtrusionProfileService>(new ExtrusionProfileService(_processDataServiceMock.Object))
            .AddSingleton<IMachineTrendCachingService>(new MachineTrendCachingService(
                _machineSnapshotHttpClientMock.Object,
                _latestMachineSnapshotCachingServiceMock.Object,
                machineTimeService,
                _machineSnapshotQueueWrapperMock.Object,
                _loggerMock.Object))
            .AddSingleton<IMachineService>(
                new MachineService(_machineCachingServiceMock.Object, new Mock<ILogger<MachineService>>().Object))
            .AddSingleton<IMachineSnapshotService>(machineSnapshotService)
            .AddSingleton<IColumnTrendService>(new ColumnTrendOfLast8HoursService(machineTimeService, machineSnapshotService))
            .AddLogging()
            .AddGraphQLServer()
            .AddAuthorization()
            .ModifyOptions(t => t.StrictValidation = false)
            .AddQueryType(q => q.Name("Query"))
            .AddType<FrameworkAPI.Queries.MachineQuery>()
            .AddType<ExtrusionMachine>()
            .AddType<ExtrusionThicknessProfile>()
            .AddType<ExtrusionThicknessProfiles>()
            .AddSorting()
            .AddFiltering()
            .BuildRequestExecutorAsync();
    }
}