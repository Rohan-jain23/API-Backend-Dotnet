using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using FrameworkAPI.Services;
using FrameworkAPI.Services.Interfaces;
using FrameworkAPI.Subscriptions;
using Microsoft.Reactive.Testing;
using Moq;
using WuH.Ruby.AlarmDataHandler.Client;
using WuH.Ruby.Common.Core;
using WuH.Ruby.MachineDataHandler.Client;
using WuH.Ruby.MachineSnapShooter.Client;
using WuH.Ruby.MachineSnapShooter.Client.Models;
using Xunit;
using Machine = FrameworkAPI.Schema.Machine.Machine;
using MachineFamily = FrameworkAPI.Schema.Misc.MachineFamily;

namespace FrameworkAPI.Test.Subscriptions;

public class MachineChangedSubscriptionTests : ReactiveTest
{
    private const string MachineId = "EQ12345";

    private readonly Mock<IMachineService> _machineServiceMock = new();
    private readonly Mock<ILatestMachineSnapshotCachingService> _latestMachineSnapshotCachingServiceMock = new();
    private readonly Mock<IAlarmDataHandlerCachingService> _alarmDataHandlerCachingServiceMock = new();
    private readonly Mock<IStandardKpiChangesService> _standardKpiChangesServiceMock = new();
    private readonly Mock<ISchedulerProvider> _schedulerProviderMock = new();
    private readonly MachineChangedSubscription _subject;

    public MachineChangedSubscriptionTests()
    {
        _machineServiceMock
            .Setup(m => m.GetMachine(MachineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Machine.CreateInstance(new WuH.Ruby.MachineDataHandler.Client.Machine
            {
                BusinessUnit = BusinessUnit.Extrusion,
                MachineFamily = MachineFamily.BlowFilm.ToString(),
                MachineId = MachineId
            }));

        _machineServiceMock
            .Setup(m => m.DoesMachineExist(MachineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _latestMachineSnapshotCachingServiceMock
            .Setup(m => m.GetLatestMachineSnapshot(MachineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<MachineSnapshotResponse>(
                new MachineSnapshotResponse(
                    new SnapshotMetaDto(MachineId, "fakeHash", DateTime.MinValue, new List<SnapshotColumnUnitDto>()),
                    new SnapshotDto(new List<SnapshotColumnValueDto>(), DateTime.UnixEpoch))));

        _standardKpiChangesServiceMock
            .Setup(m => m.WhenMachineStandardKpisChanged(MachineId))
            .Returns(Observable.Empty<string>());

        _subject = new MachineChangedSubscription();
    }

    [Fact]
    public void WhenMachineChanged_Should_Emit_Initial_And_Published_Values()
    {
        // Arrange
        var scheduler = new TestScheduler();
        scheduler.Schedule(MachineId, TimeSpan.FromSeconds(1), SetupMachineSnapshotChangedEvent);
        scheduler.Schedule(MachineId, TimeSpan.FromMinutes(5), SetupMachineSnapshotChangedEvent);

        _schedulerProviderMock
            .Setup(x => x.GetScheduler())
            .Returns(scheduler);

        // Act
        var result = scheduler.Start(
            () => _subject.WhenMachineChanged(
                MachineId,
                _machineServiceMock.Object,
                _latestMachineSnapshotCachingServiceMock.Object,
                _alarmDataHandlerCachingServiceMock.Object,
                _standardKpiChangesServiceMock.Object,
                _schedulerProviderMock.Object),
            created: 0,
            subscribed: 0,
            disposed: TimeSpan.FromMinutes(10).Ticks);

        // Assert
        var expectedTrigger = new[]
        {
            OnNext(1, MachineId),
            OnNext((TimeSpan.FromSeconds(1) + TimeSpan.FromSeconds(1)).Ticks, MachineId),
            OnNext((TimeSpan.FromMinutes(5) + TimeSpan.FromSeconds(1)).Ticks, MachineId)
        };
        ReactiveAssert.AreElementsEqual(expectedTrigger, result.Messages);
    }

    [Fact]
    public void WhenMachineChanged_Should_Emit_Initial_And_Published_Values_But_Skip_Messages_When_New_Message_Arrives_Under_1_Second()
    {
        // Arrange
        var scheduler = new TestScheduler();
        scheduler.Schedule(MachineId, TimeSpan.FromSeconds(1), SetupMachineSnapshotChangedEvent);
        scheduler.Schedule(MachineId, TimeSpan.FromMinutes(5), SetupMachineSnapshotChangedEvent);
        scheduler.Schedule(MachineId, TimeSpan.FromMinutes(5) + TimeSpan.FromMilliseconds(900), SetupMachineSnapshotChangedEvent);

        _schedulerProviderMock
            .Setup(x => x.GetScheduler())
            .Returns(scheduler);

        // Act
        var result = scheduler.Start(
            () => _subject.WhenMachineChanged(
                MachineId,
                _machineServiceMock.Object,
                _latestMachineSnapshotCachingServiceMock.Object,
                _alarmDataHandlerCachingServiceMock.Object,
                _standardKpiChangesServiceMock.Object,
                _schedulerProviderMock.Object),
            created: 0,
            subscribed: 0,
            disposed: TimeSpan.FromMinutes(10).Ticks);

        // Assert
        var expectedTrigger = new[]
        {
            OnNext(1, MachineId),
            OnNext((TimeSpan.FromSeconds(1) + TimeSpan.FromSeconds(1)).Ticks, MachineId),
            OnNext((TimeSpan.FromMinutes(5) + TimeSpan.FromMilliseconds(900) + TimeSpan.FromSeconds(1)).Ticks, MachineId)
        };
        ReactiveAssert.AreElementsEqual(expectedTrigger, result.Messages);
    }

    [Fact]
    public void WhenMachineChanged_Should_Emit_All_Values_With_Matching_MachineId()
    {
        // Arrange
        var scheduler = new TestScheduler();
        scheduler.Schedule(MachineId, TimeSpan.FromSeconds(1), SetupMachineSnapshotChangedEvent);
        scheduler.Schedule("OtherMachineId", TimeSpan.FromMinutes(1), SetupMachineSnapshotChangedEvent);
        scheduler.Schedule(MachineId, TimeSpan.FromMinutes(2), SetupMachineSnapshotChangedEvent);
        scheduler.Schedule(MachineId, TimeSpan.FromHours(1), SetupMachineSnapshotChangedEvent);
        scheduler.Schedule("OtherMachineId", TimeSpan.FromHours(2), SetupMachineSnapshotChangedEvent);
        scheduler.Schedule(MachineId, TimeSpan.FromHours(3), SetupMachineSnapshotChangedEvent);

        _schedulerProviderMock
            .Setup(x => x.GetScheduler())
            .Returns(scheduler);

        // Act
        var result = scheduler.Start(
            () => _subject.WhenMachineChanged(
                MachineId,
                _machineServiceMock.Object,
                _latestMachineSnapshotCachingServiceMock.Object,
                _alarmDataHandlerCachingServiceMock.Object,
                _standardKpiChangesServiceMock.Object,
                _schedulerProviderMock.Object),
            created: 0,
            subscribed: 0,
            disposed: TimeSpan.FromHours(5).Ticks);

        // Assert
        var expectedTrigger = new[]
        {
            OnNext(1, MachineId),
            OnNext((TimeSpan.FromSeconds(1) + TimeSpan.FromSeconds(1)).Ticks, MachineId),
            OnNext((TimeSpan.FromMinutes(2) + TimeSpan.FromSeconds(1)).Ticks, MachineId),
            OnNext((TimeSpan.FromHours(1) + TimeSpan.FromSeconds(1)).Ticks, MachineId),
            OnNext((TimeSpan.FromHours(3) + TimeSpan.FromSeconds(1)).Ticks, MachineId)
        };
        ReactiveAssert.AreElementsEqual(expectedTrigger, result.Messages);
    }

    [Fact]
    public void WhenMachineChanged_Should_Add_And_Remove_EventHandler()
    {
        // Arrange
        var scheduler = new TestScheduler();

        _schedulerProviderMock
            .Setup(x => x.GetScheduler())
            .Returns(scheduler);

        // Act
        scheduler.Start(
            () => _subject.WhenMachineChanged(
                MachineId,
                _machineServiceMock.Object,
                _latestMachineSnapshotCachingServiceMock.Object,
                _alarmDataHandlerCachingServiceMock.Object,
                _standardKpiChangesServiceMock.Object,
                _schedulerProviderMock.Object),
            created: 0,
            subscribed: 0,
            disposed: TimeSpan.FromSeconds(10).Ticks);

        // Assert
        _latestMachineSnapshotCachingServiceMock
            .VerifyAdd(m => m.CacheChanged += It.IsAny<EventHandler<LiveSnapshotEventArgs>>(),
                Times.Exactly(1));
        _latestMachineSnapshotCachingServiceMock
            .VerifyRemove(m => m.CacheChanged -= It.IsAny<EventHandler<LiveSnapshotEventArgs>>(),
                Times.Exactly(1));
    }

    [Fact]
    public void WhenMachineChanged_Should_Not_Add_EventHandler_When_MachineId_Does_Not_Exist()
    {
        // Arrange
        const string notExistingMachineId = "NotExistingId";
        var scheduler = new TestScheduler();

        _machineServiceMock
            .Setup(m => m.DoesMachineExist(notExistingMachineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _schedulerProviderMock
            .Setup(x => x.GetScheduler())
            .Returns(scheduler);

        // Act
        scheduler.Start(
            () => _subject.WhenMachineChanged(
                notExistingMachineId,
                _machineServiceMock.Object,
                _latestMachineSnapshotCachingServiceMock.Object,
                _alarmDataHandlerCachingServiceMock.Object,
                _standardKpiChangesServiceMock.Object,
                _schedulerProviderMock.Object),
            created: 0,
            subscribed: 0,
            disposed: TimeSpan.FromSeconds(10).Ticks);

        // Assert   
        _latestMachineSnapshotCachingServiceMock.VerifyAdd(
            m => m.CacheChanged += It.IsAny<EventHandler<LiveSnapshotEventArgs>>(), Times.Never);
    }

    private IDisposable SetupMachineSnapshotChangedEvent(IScheduler _, string machineId)
    {
        var snapshotDto = new SnapshotQueueMessageDto(
            new List<SnapshotColumnValueDto>(), DateTime.MinValue, "fakeHash", snapshotTime: DateTime.UnixEpoch);

        _latestMachineSnapshotCachingServiceMock.Raise(
            latestMachineSnapshotCachingService => latestMachineSnapshotCachingService.CacheChanged += null,
            new LiveSnapshotEventArgs(machineId, snapshotDto, isMinutelySnapshot: true));

        return Disposable.Empty;
    }
}