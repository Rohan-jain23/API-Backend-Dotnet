using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.Schema.Machine;
using FrameworkAPI.Services;
using FrameworkAPI.Services.Interfaces;
using FrameworkAPI.Test.TestHelpers;
using HotChocolate;
using HotChocolate.Execution;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Snapshooter.Xunit;
using WuH.Ruby.Common.Core;
using WuH.Ruby.MachineDataHandler.Client;
using WuH.Ruby.MachineSnapShooter.Client;
using WuH.Ruby.MachineSnapShooter.Client.Models;
using WuH.Ruby.MachineSnapShooter.Client.Queue;
using WuH.Ruby.OpcUaForwarder.Client;
using WuH.Ruby.ProcessDataReader.Client;
using Xunit;

namespace FrameworkAPI.Test.Queries.MachineQuery;

public class PrintingMachineQueryIntegrationTests
{
    private const string MachineId = "EQ00001";

    private readonly Mock<IProcessDataReaderHttpClient> _processDataReaderHttpClientMock = new();
    private readonly Mock<IMachineSnapshotHttpClient> _machineSnapshotHttpClientMock = new();
    private readonly Mock<IMachineCachingService> _machineCachingServiceMock = new();
    private readonly Mock<IMachineSnapshotQueueWrapper> _machineSnapshotQueueWrapperMock = new();
    private readonly Mock<ILatestMachineSnapshotCachingService> _latestMachineSnapshotCachingServiceMock = new();
    private readonly Mock<ILogger<MachineTrendCachingService>> _loggerMock = new();

    [Fact]
    public async Task GetPrintingMachine_Should_Return_Speed()
    {
        // Arrange
        var executor = await InitializeExecutor();

        InitializeFlexoPrintMachineMock();

        var snapshotMetaDto = new SnapshotMetaDto(
            MachineId,
            "fakeHash",
            DateTime.MinValue,
            new List<SnapshotColumnUnitDto>
            {
                new(SnapshotColumnIds.PrintingSpeed, "m/min")
            });
        var snapshotDto = new SnapshotDto(
            new List<SnapshotColumnValueDto>
            {
                new(SnapshotColumnIds.PrintingSpeed, 77.7)
            },
            DateTime.UtcNow);
        var machineSnapshotResponse = new MachineSnapshotResponse(snapshotMetaDto, snapshotDto);

        _latestMachineSnapshotCachingServiceMock
            .Setup(m => m.GetLatestMachineSnapshot(MachineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<MachineSnapshotResponse>(machineSnapshotResponse));

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    machine(machineId: ""{MachineId}"") {{
                        ... on PrintingMachine {{
                        speed {{
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
    public async Task GetPrintingMachine_Should_Return_Trend_Of_Speed()
    {
        // Arrange
        var executor = await InitializeExecutor();

        InitializeFlexoPrintMachineMock();

        var machineTime = new DateTime(year: 2023, month: 1, day: 15, hour: 0, minute: 0, second: 0, DateTimeKind.Utc);

        _processDataReaderHttpClientMock
            .Setup(m => m.GetLastReceivedOpcUaServerTime(It.IsAny<CancellationToken>(), MachineId))
            .ReturnsAsync(new InternalItemResponse<DateTime>(machineTime));

        var snapshotMetaDto = new SnapshotMetaDto(MachineId, "fakeHash", DateTime.MinValue, new List<SnapshotColumnUnitDto>());
        var snapshotDtos = new List<SnapshotDto>();

        for (var i = 0; i < (int)Constants.MachineTrend.TrendTimeSpan.TotalMinutes; i++)
        {
            snapshotDtos.Add(new SnapshotDto(
                new List<SnapshotColumnValueDto>
                {
                    new(SnapshotColumnIds.PrintingSpeed, i == 0 ? null : i * 1.5)
                },
                machineTime.Subtract(TimeSpan.FromMinutes(i))));
        }

        var machineSnapshotResponse = new MachineSnapshotResponse(snapshotMetaDto, snapshotDtos.First());

        _latestMachineSnapshotCachingServiceMock
            .Setup(m => m.GetLatestMachineSnapshot(MachineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<MachineSnapshotResponse>(machineSnapshotResponse));

        var machineSnapshotListResponse = new MachineSnapshotListResponse(snapshotMetaDto, snapshotDtos, []);
        _machineSnapshotHttpClientMock
            .Setup(m => m.GetSnapshotsInTimeRanges(MachineId, It.IsAny<List<string>>(), It.IsAny<List<TimeRange>>(), null, It.IsAny<CancellationToken>()))
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
                    machine(machineId: ""{MachineId}"") {{
                        ... on PrintingMachine {{
                        speed {{
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

    private void InitializeFlexoPrintMachineMock()
    {
        var machine = MachineMock.GenerateFlexoPrint(MachineId);

        _machineCachingServiceMock
            .Setup(m => m.GetMachine(MachineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(machine);
    }

    private async Task<IRequestExecutor> InitializeExecutor()
    {
        var machineTimeService = new MachineTimeService(
            new OpcUaServerTimeCachingService(
                _processDataReaderHttpClientMock.Object, new Mock<IProcessDataQueueWrapper>().Object),
            _latestMachineSnapshotCachingServiceMock.Object,
            new Mock<ILogger<MachineTimeService>>().Object);
        var machineSnapshotService = new MachineSnapshotService();

        return await new ServiceCollection()
            .AddSingleton(_machineSnapshotHttpClientMock.Object)
            .AddSingleton(_latestMachineSnapshotCachingServiceMock.Object)
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
            .AddType<PrintingMachine>()
            .AddSorting()
            .AddFiltering()
            .BuildRequestExecutorAsync();
    }
}