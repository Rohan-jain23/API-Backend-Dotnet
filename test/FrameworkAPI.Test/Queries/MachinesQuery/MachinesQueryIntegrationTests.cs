using System;
using System.Collections.Generic;
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
using WuH.Ruby.MachineSnapShooter.Client;
using WuH.Ruby.MachineSnapShooter.Client.Models;
using WuH.Ruby.OpcUaForwarder.Client;
using WuH.Ruby.ProcessDataReader.Client;
using Xunit;
using MachineDataHandler = WuH.Ruby.MachineDataHandler.Client;

namespace FrameworkAPI.Test.Queries.MachinesQuery;

public class MachinesQueryIntegrationTests
{
    private const string MachineId1 = "EQ00001";
    private const string MachineId2 = "EQ00002";

    private readonly Mock<IProcessDataReaderHttpClient> _processDataReaderHttpClientMock = new();
    private readonly Mock<MachineDataHandler.IMachineCachingService> _machineCachingServiceMock = new();
    private readonly Mock<ILatestMachineSnapshotCachingService> _latestMachineSnapshotCachingServiceMock = new();

    [Fact]
    public async Task GetMachines_With_MachineId_Should_Return_The_Correct_Response()
    {
        // Arrange
        var executor = await InitializeExecutor();

        var machines = new List<MachineDataHandler.Machine>
            { MachineMock.GenerateCastFilm(MachineId1), MachineMock.GenerateCastFilm(MachineId2) };

        _machineCachingServiceMock
            .Setup(m => m.GetMachinesAsInternalListResponse(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalListResponse<MachineDataHandler.Machine>(machines));

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                @"{
                    machines {
                    machineId
                    }
                }")
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        _machineCachingServiceMock.VerifyAll();
        _machineCachingServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetMachines_With_Filter_Where_MachineId_Contains_01_Should_Return_The_Correct_Response()
    {
        // Arrange
        var executor = await InitializeExecutor();

        var machines = new List<MachineDataHandler.Machine>
            { MachineMock.GenerateCastFilm(MachineId1), MachineMock.GenerateCastFilm(MachineId2) };

        _machineCachingServiceMock
            .Setup(m => m.GetMachinesAsInternalListResponse(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalListResponse<MachineDataHandler.Machine>(machines));

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                @"{
                    machines(where: { machineId: { contains: ""01"" } }) {
                    machineId
                    }
                }")
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        _machineCachingServiceMock.VerifyAll();
        _machineCachingServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetMachines_With_All_Requested_Properties_Should_Return_The_Correct_Response()
    {
        // Arrange
        var executor = await InitializeExecutor();

        var machines = new List<MachineDataHandler.Machine>
            { MachineMock.GenerateCastFilm(MachineId1), MachineMock.GenerateCastFilm(MachineId2) };

        _machineCachingServiceMock
            .Setup(m => m.GetMachinesAsInternalListResponse(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalListResponse<MachineDataHandler.Machine>(machines));

        var machineIdsAndTimes = new[]
        {
            (Id: MachineId1, Time: DateTime.UnixEpoch),
            (Id: MachineId2, Time: DateTime.UnixEpoch.AddHours(1))
        };

        foreach (var (id, time) in machineIdsAndTimes)
        {
            _processDataReaderHttpClientMock
                .Setup(m => m.GetLastReceivedOpcUaServerTime(It.IsAny<CancellationToken>(), id))
                .ReturnsAsync(new InternalItemResponse<DateTime>(time));
            _latestMachineSnapshotCachingServiceMock
                .Setup(m => m.GetLatestMachineSnapshot(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new InternalItemResponse<MachineSnapshotResponse>(
                    new MachineSnapshotResponse(
                        new SnapshotMetaDto(id, "fakeHash", DateTime.MinValue, []),
                        new SnapshotDto(
                            [],
                            snapshotTime: time))));
        }

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                @"{
                    machines {
                    time
                    name
                    machineType
                    machineId
                    machineFamily
                    department
                    }
                }")
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
    public async Task GetMachines_On_Extrusion_Machine_With_Requested_MachineId_Should_Return_The_Correct_Response()
    {
        // Arrange
        var executor = await InitializeExecutor();

        var machines = new List<MachineDataHandler.Machine>
            { MachineMock.GenerateCastFilm(MachineId1), MachineMock.GenerateFlexoPrint(MachineId2) };

        _machineCachingServiceMock
            .Setup(m => m.GetMachinesAsInternalListResponse(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalListResponse<MachineDataHandler.Machine>(machines));

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                @"{
                    machines {
                    ... on ExtrusionMachine {
                        machineFamily
                        machineId
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
    }

    [Fact]
    public async Task
        GetMachines_With_Filter_Where_MachineId_Contains_NOTAVAILABLE_Should_Return_An_Empty_List_Response()
    {
        // Arrange
        var executor = await InitializeExecutor();

        var machines = new List<MachineDataHandler.Machine>
            { MachineMock.GenerateCastFilm(MachineId1), MachineMock.GenerateCastFilm(MachineId2) };

        _machineCachingServiceMock
            .Setup(m => m.GetMachinesAsInternalListResponse(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalListResponse<MachineDataHandler.Machine>(machines));

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
            @"{
                 machines(where: { machineId: { contains: ""NOTAVAILABLE"" } }) {
                   machineId
                 }
              }")
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        _machineCachingServiceMock.VerifyAll();
        _machineCachingServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetMachines_With_MachineId_Returns_Empty_List_Returns_Empty_Result()
    {
        // Arrange
        var executor = await InitializeExecutor();

        _machineCachingServiceMock
            .Setup(m => m.GetMachinesAsInternalListResponse(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalListResponse<MachineDataHandler.Machine>([]));

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                    @"{
                        machines {
                        machineId
                        }
                    }")
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);
        // Assert
        result.ToJson().MatchSnapshot();

        _machineCachingServiceMock.VerifyAll();
        _machineCachingServiceMock.VerifyNoOtherCalls();
    }

    private async Task<IRequestExecutor> InitializeExecutor()
    {
        return await new ServiceCollection()
            .AddSingleton<IMachineService>(new MachineService(
                _machineCachingServiceMock.Object, new Mock<ILogger<MachineService>>().Object))
            .AddSingleton<IMachineTimeService>(new MachineTimeService(
                new OpcUaServerTimeCachingService(
                    _processDataReaderHttpClientMock.Object, new Mock<IProcessDataQueueWrapper>().Object),
                _latestMachineSnapshotCachingServiceMock.Object,
                new Mock<ILogger<MachineTimeService>>().Object))
            .AddLogging()
            .AddGraphQLServer()
            .AddAuthorization()
            .ModifyOptions(t => t.StrictValidation = false)
            .AddQueryType(q => q.Name("Query"))
            .AddType<FrameworkAPI.Queries.MachineQuery>()
            .AddType<ExtrusionMachine>()
            .AddType<PrintingMachine>()
            .AddSorting()
            .AddFiltering()
            .BuildRequestExecutorAsync();
    }
}