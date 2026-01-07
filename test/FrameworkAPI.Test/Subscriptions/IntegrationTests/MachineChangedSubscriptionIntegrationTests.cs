using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FrameworkAPI.Schema.Machine;
using FrameworkAPI.Services;
using FrameworkAPI.Services.Interfaces;
using FrameworkAPI.Subscriptions;
using FrameworkAPI.Test.TestHelpers;
using HotChocolate;
using HotChocolate.Execution;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Reactive.Testing;
using Moq;
using Snapshooter.Xunit;
using WuH.Ruby.AlarmDataHandler.Client;
using WuH.Ruby.Common.Core;
using WuH.Ruby.KpiDataHandler.Client;
using WuH.Ruby.MachineDataHandler.Client;
using WuH.Ruby.MachineSnapShooter.Client;
using WuH.Ruby.MachineSnapShooter.Client.Models;
using WuH.Ruby.ProductionPeriodsDataHandler.Client;
using Xunit;
using Machine = WuH.Ruby.MachineDataHandler.Client.Machine;
using MachineFamily = FrameworkAPI.Schema.Misc.MachineFamily;

namespace FrameworkAPI.Test.Subscriptions.IntegrationTests;

public class MachineChangedSubscriptionIntegrationTests : ReactiveTest
{
    private const string MachineId = "EQ12345";

    private readonly TimeSpan _defaultTestTimeout = TimeSpan.FromSeconds(5);
    private readonly Mock<ILatestMachineSnapshotCachingService> _latestMachineSnapshotCachingServiceMock = new();
    private readonly Mock<IAlarmDataHandlerCachingService> _alarmDataHandlerCachingServiceMock = new();
    private readonly Mock<IKpiChangesQueueWrapper> _kpiChangesQueueWrapperMock = new();
    private readonly Mock<IMachineSnapshotHttpClient> _machineSnapshotHttpClientMock = new();
    private readonly Mock<IMachineCachingService> _machineCachingServiceMock = new();
    private readonly Mock<ISchedulerProvider> _schedulerProviderMock = new();

    public MachineChangedSubscriptionIntegrationTests()
    {
        _machineCachingServiceMock
            .Setup(m => m.GetMachine(MachineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Machine
            {
                BusinessUnit = BusinessUnit.Extrusion,
                MachineFamily = MachineFamily.BlowFilm.ToString(),
                MachineId = MachineId
            });
    }

    [Fact]
    public async Task Subscribe_To_Machine_Changed()
    {
        // Arrange
        _latestMachineSnapshotCachingServiceMock
            .Setup(m =>
                m.GetLatestMachineSnapshot(
                    MachineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<MachineSnapshotResponse>(
                new MachineSnapshotResponse(
                    new SnapshotMetaDto(MachineId, "fakeHash", DateTime.MinValue, new List<SnapshotColumnUnitDto>()),
                    new SnapshotDto(new List<SnapshotColumnValueDto>(), DateTime.UnixEpoch))));

        var machineIds = new List<string>();

        var scheduler = new TestScheduler();
        scheduler.Schedule(state: MachineId, dueTime: TimeSpan.FromSeconds(1), RaiseMachineSnapshotChangedEvent);

        _schedulerProviderMock
            .Setup(x => x.GetScheduler())
            .Returns(scheduler);

        var executor = await InitializeExecutor();

        var request = QueryRequestBuilder
            .New()
            .SetQuery("subscription { machineChanged(machineId: \"EQ12345\") { machineId } }")
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        result.Should().BeAssignableTo<IResponseStream>();

        var responseStream = (IResponseStream)result;
        var taskCompletionSource = new TaskCompletionSource();

        // Act
        responseStream
            .ReadResultsAsync()
            .ToObservable()
            .Subscribe(queryResult =>
            {
                machineIds.Add(ParseMachineId(queryResult));

                // Assert
                if (machineIds.Count == 2)
                {
                    taskCompletionSource.SetResult();
                }
            });

        scheduler.AdvanceTo(TimeSpan.FromSeconds(2).Ticks);

        // Assert
        // Wait for the two machine changes to be received
        await taskCompletionSource.Task.WaitAsync(timeout: _defaultTestTimeout);
    }

    [Fact]
    public async Task Subscribe_To_Machine_Changed_To_Offline()
    {
        // Arrange
        var snapshotMetaDto = new SnapshotMetaDto(MachineId, "fakeHash", DateTime.MinValue, []);
        var onlineSnapshotDto = new SnapshotDto(
            [
                new SnapshotColumnValueDto(SnapshotColumnIds.ProductionStatusId, 1),
                new SnapshotColumnValueDto(SnapshotColumnIds.ProductionStatusCategory, ProductionStatusCategoryForPublic.Production.ToString())
            ],
            DateTime.UtcNow,
            isCreatedByVirtualTime: false);
        var onlineMachineSnapshotResponse = new MachineSnapshotResponse(snapshotMetaDto, onlineSnapshotDto);

        var offlineSnapshotDto = new SnapshotDto(
            [
                new SnapshotColumnValueDto(SnapshotColumnIds.ProductionStatusId, -11),
                new SnapshotColumnValueDto(SnapshotColumnIds.ProductionStatusCategory, ProductionStatusCategoryForPublic.Offline.ToString())
            ],
            DateTime.UtcNow,
            isCreatedByVirtualTime: true);
        var offlineMachineSnapshotResponse = new MachineSnapshotResponse(snapshotMetaDto, offlineSnapshotDto);

        _latestMachineSnapshotCachingServiceMock
            .SetupSequence(mock =>
                mock.GetLatestMachineSnapshot(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<MachineSnapshotResponse>(onlineMachineSnapshotResponse))
            .ReturnsAsync(new InternalItemResponse<MachineSnapshotResponse>(offlineMachineSnapshotResponse));

        var results = new List<string>();

        var scheduler = new TestScheduler();

        _schedulerProviderMock
            .Setup(x => x.GetScheduler())
            .Returns(scheduler);

        var executor = await InitializeExecutor();
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                @"subscription {
                    machineChanged(machineId: ""EQ12345"") {
                    machineId
                    productionStatus {
                        category
                        id
                    }
                    }
                }")
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        result.Should().BeAssignableTo<IResponseStream>();

        var responseStream = (IResponseStream)result;
        var taskCompletionSource = new TaskCompletionSource();

        // Act
        responseStream
            .ReadResultsAsync()
            .ToObservable()
            .Subscribe(queryResult =>
            {
                results.Add(queryResult.ToJson());

                // Assert
                if (results.Count == 2)
                {
                    taskCompletionSource.SetResult();
                }
            });

        scheduler.Schedule(MachineId, TimeSpan.FromSeconds(1), RaiseMachineSnapshotChangedEvent);

        scheduler.AdvanceTo(TimeSpan.FromSeconds(5).Ticks);

        // Assert
        // Wait for the two machine changes to be received
        await taskCompletionSource.Task.WaitAsync(timeout: _defaultTestTimeout);

        results.Should().MatchSnapshot();
    }

    [Fact]
    public async Task Subscribe_To_StandardKpis_Changed()
    {
        // Arrange
        _latestMachineSnapshotCachingServiceMock
            .Setup(m => m.GetLatestMachineSnapshot(MachineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<MachineSnapshotResponse>(
                new MachineSnapshotResponse(
                    new SnapshotMetaDto(MachineId, "fakeHash", DateTime.MinValue, []),
                    new SnapshotDto(
                        [],
                        DateTime.UnixEpoch))));

        Func<string, string, StandardJobKpis, Task>? registeredCallback = null;
        _kpiChangesQueueWrapperMock
            .Setup(m => m.SubscribeForKpiChangesOfActiveJob(
                MachineId,
                It.IsAny<Func<string, string, StandardJobKpis, Task>>()))
            .Callback<string, Func<string, string, StandardJobKpis, Task>>((_, callback) =>
            {
                registeredCallback = callback;
            });

        var scheduler = new TestScheduler();
        _schedulerProviderMock
            .Setup(x => x.GetScheduler())
            .Returns(scheduler);
        using var _ = scheduler.ScheduleAsync(state: MachineId, dueTime: TimeSpan.FromSeconds(1), async (_, _, _) =>
        {
            await registeredCallback!(MachineId, "Job123", new StandardJobKpis());
        });

        var executor = await InitializeExecutor();

        var request = QueryRequestBuilder
            .New()
            .SetQuery("subscription { machineChanged(machineId: \"EQ12345\") { machineId } }")
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        result.Should().BeAssignableTo<IResponseStream>();

        registeredCallback.Should().NotBeNull();

        var responseStream = (IResponseStream)result;
        var taskCompletionSource = new TaskCompletionSource();

        // Act
        var machineIds = new List<string>();
        responseStream
            .ReadResultsAsync()
            .ToObservable()
            .Subscribe(queryResult =>
            {
                machineIds.Add(ParseMachineId(queryResult));

                // Assert
                if (machineIds.Count == 2)
                {
                    taskCompletionSource.SetResult();
                }
            });

        scheduler.AdvanceTo(TimeSpan.FromSeconds(5).Ticks);

        // Assert
        // Wait for the two machine changes to be received
        await taskCompletionSource.Task.WaitAsync(timeout: _defaultTestTimeout);
    }

    private async Task<IRequestExecutor> InitializeExecutor()
    {
        var machineService =
            new MachineService(_machineCachingServiceMock.Object, new Mock<ILogger<MachineService>>().Object);
        var machineSnapshotService = new MachineSnapshotService();
        var standardKpiChangesService = new StandardKpiChangesService(_kpiChangesQueueWrapperMock.Object);

        return await new ServiceCollection()
            .AddSingleton(_latestMachineSnapshotCachingServiceMock.Object)
            .AddSingleton(_alarmDataHandlerCachingServiceMock.Object)
            .AddSingleton(_machineSnapshotHttpClientMock.Object)
            .AddSingleton(_kpiChangesQueueWrapperMock.Object)
            .AddSingleton(_schedulerProviderMock.Object)
            .AddSingleton<IStandardKpiChangesService>(standardKpiChangesService)
            .AddSingleton<IMachineService>(machineService)
            .AddSingleton<IMachineSnapshotService>(machineSnapshotService)
            .AddLogging()
            .AddGraphQLServer()
            .AddAuthorization()
            .ModifyOptions(t => t.StrictValidation = false)
            .AddType<ExtrusionMachine>()
            .AddSubscriptionType(q => q.Name("Subscription"))
            .AddType<MachineChangedSubscription>()
            .BuildRequestExecutorAsync();
    }

    private IDisposable RaiseMachineSnapshotChangedEvent(IScheduler _, string machineId)
    {
        var snapshotDto = new SnapshotQueueMessageDto(
            new List<SnapshotColumnValueDto>(), DateTime.MinValue, "fakeHash", snapshotTime: DateTime.UnixEpoch);

        _latestMachineSnapshotCachingServiceMock.Raise(
            latestMachineSnapshotCachingService => latestMachineSnapshotCachingService.CacheChanged += null,
            new LiveSnapshotEventArgs(machineId, snapshotDto, isMinutelySnapshot: true));

        return Disposable.Empty;
    }

    private static string ParseMachineId(IQueryResult result)
    {
        var objectResult = result.Data!["machineChanged"] as IReadOnlyDictionary<string, object>;
        var machineId = objectResult!["machineId"].ToString()!;
        return machineId;
    }
}