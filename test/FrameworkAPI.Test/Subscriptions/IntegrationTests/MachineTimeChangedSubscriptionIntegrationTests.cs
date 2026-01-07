using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FrameworkAPI.Services;
using FrameworkAPI.Services.Interfaces;
using FrameworkAPI.Subscriptions;
using FrameworkAPI.Test.TestHelpers;
using HotChocolate.Execution;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Reactive.Testing;
using Moq;
using WuH.Ruby.Common.Core;
using WuH.Ruby.MachineDataHandler.Client;
using WuH.Ruby.MachineSnapShooter.Client;
using WuH.Ruby.MachineSnapShooter.Client.Models;
using WuH.Ruby.OpcUaForwarder.Client;
using WuH.Ruby.ProcessDataReader.Client;
using Xunit;

namespace FrameworkAPI.Test.Subscriptions.IntegrationTests;

public class MachineTimeChangedSubscriptionIntegrationTests : ReactiveTest
{
    private const string MachineId = "EQ12345";

    private readonly Mock<IMachineCachingService> _machineCachingServiceMock = new();
    private readonly Mock<ILatestMachineSnapshotCachingService> _latestMachineSnapshotCachingServiceMock = new();
    private readonly Mock<IProcessDataReaderHttpClient> _processDataReaderHttpClientMock = new();
    private readonly Mock<IProcessDataQueueWrapper> _processDataQueueWrapperMock = new();

    private Func<string, DateTime?, Task>? _callback;

    public MachineTimeChangedSubscriptionIntegrationTests()
    {
        _processDataReaderHttpClientMock
            .Setup(m => m.GetLastReceivedOpcUaServerTime(It.IsAny<CancellationToken>(), MachineId))
            .ReturnsAsync(new InternalItemResponse<DateTime>(DateTime.UnixEpoch));

        _processDataQueueWrapperMock
            .Setup(m => m.SubscribeForOpcUaServerTime(
                MachineId, It.IsAny<Func<string, DateTime?, Task>>()
            ))
            .Callback((string _, Func<string, DateTime?, Task> callback) =>
            {
                _callback = callback;
            });
    }

    [Fact]
    public async Task Subscribe_To_OpcUa_Machine_Time_Changed()
    {
        // Arrange
        _machineCachingServiceMock
            .Setup(m => m.GetMachine(MachineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Machine());

        var snapshotMetaDto = new SnapshotMetaDto(MachineId, "fakeHash", DateTime.MinValue, new List<SnapshotColumnUnitDto>());
        var machineSnapshotResponse = new MachineSnapshotResponse(snapshotMetaDto, new SnapshotDto(
            [new("ColumnId", 4.11)],
            DateTime.UnixEpoch));

        _latestMachineSnapshotCachingServiceMock
            .Setup(m => m.GetLatestMachineSnapshot(MachineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<MachineSnapshotResponse>(machineSnapshotResponse));

        var scheduler = new TestScheduler();
        using var _ =
            scheduler.ScheduleAsync(state: DateTime.UnixEpoch.AddSeconds(1), dueTime: TimeSpan.FromSeconds(1),
                SetupDateTimes);

        var executor = await InitializeExecutor();

        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                @"subscription {
                    machineTimeChanged(machineId: ""EQ12345"")
                }")
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        var responseStream = (IResponseStream)result;
        var taskCompletionSource = new TaskCompletionSource();

        var dateTimes = new List<DateTime>();

        // Act
        responseStream
            .ReadResultsAsync()
            .ToObservable()
            .Subscribe(queryResult =>
            {
                dateTimes.Add(ParseDateTime(queryResult));

                if (dateTimes.Count == 2)
                {
                    taskCompletionSource.SetResult();
                }
            });

        scheduler.AdvanceTo(TimeSpan.FromSeconds(1).Ticks);

        // Assert
        // Wait for the date time to be received
        await taskCompletionSource.Task.WaitAsync(timeout: TimeSpan.FromSeconds(1));

        dateTimes.Should().BeEquivalentTo(new List<DateTime> { DateTime.UnixEpoch, DateTime.UnixEpoch.AddSeconds(1) });
    }

    private async Task<IRequestExecutor> InitializeExecutor()
    {
        var opcUaServerTimeCachingService = new OpcUaServerTimeCachingService(
            _processDataReaderHttpClientMock.Object,
            _processDataQueueWrapperMock.Object);

        var machineService =
            new MachineService(_machineCachingServiceMock.Object, new Mock<ILogger<MachineService>>().Object);

        var machineTimeService = new MachineTimeService(
            opcUaServerTimeCachingService,
            _latestMachineSnapshotCachingServiceMock.Object,
            new Mock<ILogger<MachineTimeService>>().Object);

        // Arrange
        return await new ServiceCollection()
            .AddSingleton<IMachineService>(machineService)
            .AddSingleton<IMachineTimeService>(machineTimeService)
            .AddLogging()
            .AddGraphQLServer()
            .AddAuthorization()
            .ModifyOptions(t => t.StrictValidation = false)
            .AddSubscriptionType(q => q.Name("Subscription"))
            .AddType<MachineTimeChangedSubscription>()
            .BuildRequestExecutorAsync();
    }

    private async Task<IDisposable> SetupDateTimes(IScheduler _, DateTime dateTime, CancellationToken cancellationToken)
    {
        await _callback!(MachineId, dateTime);

        return Disposable.Empty;
    }

    private static DateTime ParseDateTime(IQueryResult result)
    {
        return DateTimeOffset.Parse(result.Data!["machineTimeChanged"]!.ToString()!).DateTime;
    }
}