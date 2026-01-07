using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Threading;
using FrameworkAPI.Models;
using FrameworkAPI.Models.Events;
using FrameworkAPI.Services.Interfaces;
using FrameworkAPI.Subscriptions;
using Microsoft.Reactive.Testing;
using Moq;
using Xunit;

namespace FrameworkAPI.Test.Subscriptions;

public class MachineTimeChangedSubscriptionTests : ReactiveTest
{
    private const string MachineId = "EQ12345";

    private readonly Mock<IMachineService> _machineServiceMock;
    private readonly Mock<IMachineTimeService> _machineTimeServiceMock;
    private readonly MachineTimeChangedSubscription _machineTimeChangedSubscription;

    public MachineTimeChangedSubscriptionTests()
    {
        _machineTimeChangedSubscription = new MachineTimeChangedSubscription();
        _machineServiceMock = new Mock<IMachineService>();
        _machineTimeServiceMock = new Mock<IMachineTimeService>();
    }

    [Fact]
    public void WhenMachineTimeChanged_Should_Emit_Initial_And_Published_Values()
    {
        // Arrange
        var start = DateTime.UtcNow;

        _machineServiceMock
            .Setup(m => m.DoesMachineExist(MachineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _machineTimeServiceMock
            .SetupSequence(m => m.Get(MachineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DataResult<DateTime?>(start, null));

        var scheduledMachineTimes = new[]
        {
            start.AddMinutes(1),
            start.AddMinutes(2),
            start.AddMinutes(3)
        };

        var scheduler = new TestScheduler();

        for (var i = 0; i < scheduledMachineTimes.Length; i++)
        {
            scheduler.Schedule(
                state: (MachineId, scheduledMachineTimes[i]),
                dueTime: TimeSpan.FromSeconds(i + 1),
                SetupMachineSnapshotChangedEvent);
        }

        // Act
        var result = scheduler.Start(
            () => _machineTimeChangedSubscription.WhenMachineTimeChanged(
                MachineId,
                _machineServiceMock.Object,
                _machineTimeServiceMock.Object),
            created: 0,
            subscribed: 0,
            disposed: TimeSpan.FromSeconds(10).Ticks);

        // Assert
        var expected = new List<Recorded<Notification<DateTime?>>>
        {
            OnNext(1, (DateTime?)start)
        };
        expected = expected.Concat(scheduledMachineTimes.Select((machineTime, index) =>
            OnNext(TimeSpan.FromSeconds(index + 1).Ticks, (DateTime?)machineTime))).ToList();
        ReactiveAssert.AreElementsEqual(expected, result.Messages);
    }

    private IDisposable SetupMachineSnapshotChangedEvent(
        IScheduler _, (string MachineId, DateTime MachineTime) parameters)
    {
        _machineTimeServiceMock.Raise
        (
            m => m.MachineTimeChanged += null,
            _machineTimeServiceMock,
            new MachineTimeChangedEventArgs(parameters.MachineId, parameters.MachineTime));

        return Disposable.Empty;
    }
}