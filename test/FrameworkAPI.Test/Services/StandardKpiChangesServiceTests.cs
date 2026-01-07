using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using FluentAssertions;
using FrameworkAPI.Services;
using Microsoft.Reactive.Testing;
using Microsoft.VisualStudio.Threading;
using Moq;
using WuH.Ruby.KpiDataHandler.Client;
using Xunit;

namespace FrameworkAPI.Test.Services;

public class StandardKpiChangesServiceTests
{

    private const string MachineId = "EQ00001";

    private readonly Mock<IKpiChangesQueueWrapper> _kpiChangesQueueWrapperMock = new();

    private readonly StandardKpiChangesService _subject;

    private readonly TestScheduler _scheduler = new();

    private int _unsubscribeCount = 0;
    private int _subscribeCount = 0;

    // ReSharper disable once NotAccessedField.Local
#pragma warning disable IDE0052
    private Func<string, string, StandardJobKpis, Task>? _callback;
#pragma warning restore IDE0052

    public StandardKpiChangesServiceTests()
    {
        _subject = new StandardKpiChangesService(_kpiChangesQueueWrapperMock.Object);

        _kpiChangesQueueWrapperMock
            .Setup(m => m.SubscribeForKpiChangesOfActiveJob(MachineId,
                It.IsAny<Func<string, string, StandardJobKpis, Task>>()))
            .Returns((string machineId, Func<string, string, StandardJobKpis, Task> callback) =>
            {
                _callback = callback;
                _subscribeCount++;
                return Disposable.Create(() => _unsubscribeCount++);
            });
    }

    [Fact]
    public void WhenMachineStandardKpisChanged_ShouldEmitOnCallback_AndUnsubscribeCorrectly()
    {
        // Arrange
        Dictionary<long, string> messagesForConsumer = [];
        IDisposable? subscription = null;
        _scheduler.Schedule(TimeSpan.FromMilliseconds(0), () => subscription = _subject.WhenMachineStandardKpisChanged(MachineId).Subscribe(s => messagesForConsumer.Add(_scheduler.Clock, s)));
        _scheduler.Schedule(TimeSpan.FromMilliseconds(100), PublishMachineId(MachineId));
        _scheduler.Schedule(TimeSpan.FromMilliseconds(200), () =>
        {
            subscription?.Dispose();
            subscription = null;
        });
        _scheduler.Schedule(TimeSpan.FromMilliseconds(300), () => subscription = _subject.WhenMachineStandardKpisChanged(MachineId).Subscribe(s => messagesForConsumer.Add(_scheduler.Clock, s)));
        _scheduler.Schedule(TimeSpan.FromMilliseconds(400), PublishMachineId(MachineId));
        _scheduler.Schedule(TimeSpan.FromMilliseconds(500), () =>
        {
            subscription?.Dispose();
            subscription = null;
        });

        // Act
        _scheduler.AdvanceTo(TimeSpan.FromMilliseconds(600).Ticks);

        // Assert
        messagesForConsumer.Should().Equal(
            new Dictionary<long, string>()
            {
                { TimeSpan.FromMilliseconds(100).Ticks, MachineId },
                { TimeSpan.FromMilliseconds(400).Ticks, MachineId },
            });
        _unsubscribeCount.Should().Be(2);
    }

    [Fact]
    public void WhenMachineStandardKpisChanged_ShouldEmitOnCallback_AndSubscribeCorrectlyForTwoConsumersAtTheSameTime()
    {
        // Arrange
        Dictionary<long, string> messagesForConsumer1 = [];
        Dictionary<long, string> messagesForConsumer2 = [];
        IDisposable? subscription1 = null;
        IDisposable? subscription2 = null;
        _scheduler.Schedule(TimeSpan.FromMilliseconds(0), () => subscription1 = _subject.WhenMachineStandardKpisChanged(MachineId).Subscribe(s => messagesForConsumer1.Add(_scheduler.Clock, s)));
        _scheduler.Schedule(TimeSpan.FromMilliseconds(100), PublishMachineId(MachineId));
        _scheduler.Schedule(TimeSpan.FromMilliseconds(200), () => subscription2 = _subject.WhenMachineStandardKpisChanged(MachineId).Subscribe(s => messagesForConsumer2.Add(_scheduler.Clock, s)));
        _scheduler.Schedule(TimeSpan.FromMilliseconds(300), PublishMachineId(MachineId));
        _scheduler.Schedule(TimeSpan.FromMilliseconds(400), () =>
        {
            subscription1?.Dispose();
            subscription1 = null;
        });
        _scheduler.Schedule(TimeSpan.FromMilliseconds(500), PublishMachineId(MachineId));
        _scheduler.Schedule(TimeSpan.FromMilliseconds(600), () =>
        {
            subscription2?.Dispose();
            subscription2 = null;
        });

        // Act
        _scheduler.AdvanceTo(TimeSpan.FromMilliseconds(600).Ticks);

        // Assert
        messagesForConsumer1.Should().Equal(
            new Dictionary<long, string>()
            {
                { TimeSpan.FromMilliseconds(100).Ticks, MachineId },
                { TimeSpan.FromMilliseconds(300).Ticks, MachineId },
            });
        messagesForConsumer2.Should().Equal(
            new Dictionary<long, string>()
            {
                { TimeSpan.FromMilliseconds(300).Ticks, MachineId },
                { TimeSpan.FromMilliseconds(500).Ticks, MachineId },
            });
        _subscribeCount.Should().Be(1);
        _unsubscribeCount.Should().Be(1);
    }

    private Action PublishMachineId(string machineId) => () =>
    {
        if (_callback == null) throw new InvalidOperationException();
        var disposable = new CancellationDisposable();
        _ = _callback(machineId, machineId, new StandardJobKpis())
            .WithCancellation(disposable.Token);
    };

}