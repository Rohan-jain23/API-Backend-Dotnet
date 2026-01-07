using System;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using FluentAssertions;
using FrameworkAPI.Services.Interfaces;
using FrameworkAPI.Subscriptions;
using Microsoft.Reactive.Testing;
using Moq;
using PhysicalAssetDataHandler.Client.Models.Enums;
using PhysicalAssetDataHandler.Client.Models.Messages.PhysicalAssetInformation;
using PhysicalAssetDataHandler.Client.QueueWrappers;
using Xunit;

namespace FrameworkAPI.Test.Subscriptions;

public class PhysicalAssetChangedSubscriptionTests : ReactiveTest
{
    private const string PhysicalAssetId = "657c41c4435e848c718bb0c6";
    private const string AnotherPhysicalAssetId = "657c41c4435e848c718bb0c6";

    private readonly Mock<ISchedulerProvider> _schedulerProviderMock = new();
    private readonly Mock<IPhysicalAssetQueueWrapper> _physicalAssetQueueWrapperMock = new();
    private readonly PhysicalAssetChangedSubscription _physicalAssetChangedSubscription = new();

    private Func<PhysicalAssetCreatedEvent, Task>? _capturedSubscribeToPhysicalAssetCreatedEventCallback;
    private Func<PhysicalAssetUpdatedEvent, Task>? _capturedSubscribeToPhysicalAssetUpdatedEventCallback;

    private record ScheduleSetupPhysicalAssetState(string PhysicalAssetId, PhysicalAssetType PhysicalAssetType);

    public PhysicalAssetChangedSubscriptionTests()
    {
        _physicalAssetQueueWrapperMock
            .Setup(m => m.SubscribeToPhysicalAssetCreatedEvent(It.IsAny<Func<PhysicalAssetCreatedEvent, Task>>()))
            .Callback<Func<PhysicalAssetCreatedEvent, Task>>(callback =>
            {
                _capturedSubscribeToPhysicalAssetCreatedEventCallback = callback;
            });

        _physicalAssetQueueWrapperMock
            .Setup(m => m.SubscribeToPhysicalAssetUpdatedEvent(It.IsAny<Func<PhysicalAssetUpdatedEvent, Task>>()))
            .Callback<Func<PhysicalAssetUpdatedEvent, Task>>(callback =>
            {
                _capturedSubscribeToPhysicalAssetUpdatedEventCallback = callback;
            });
    }

    [Theory]
    [InlineData(null)]
    [InlineData(PhysicalAssetType.Anilox)]
    public void WhenPhysicalAssetChanged_Should_Emit_Published_Values_When_PhysicalAsset_Created(
        PhysicalAssetType? physicalAssetTypeFilter)
    {
        // Arrange
        var scheduler = new TestScheduler();
        scheduler.Schedule(
            state: new ScheduleSetupPhysicalAssetState(PhysicalAssetId, PhysicalAssetType.Anilox),
            TimeSpan.FromSeconds(9),
            SetupPhysicalAssetCreatedEvent);
        scheduler.Schedule(
            state: new ScheduleSetupPhysicalAssetState(PhysicalAssetId, PhysicalAssetType.Anilox),
            TimeSpan.FromMinutes(5),
            SetupPhysicalAssetCreatedEvent);

        _schedulerProviderMock
            .Setup(x => x.GetScheduler())
            .Returns(scheduler);

        // Act
        var result = scheduler.Start(
            () => _physicalAssetChangedSubscription.WhenPhysicalAssetChanged(
                physicalAssetTypeFilter,
                _physicalAssetQueueWrapperMock.Object),
            created: 0,
            subscribed: 0,
            disposed: TimeSpan.FromMinutes(10).Ticks);

        // Assert
        var expectedTrigger = new[]
        {
            OnNext(TimeSpan.FromSeconds(9).Ticks, PhysicalAssetId),
            OnNext(TimeSpan.FromMinutes(5).Ticks, PhysicalAssetId)
        };
        ReactiveAssert.AreElementsEqual(expectedTrigger, result.Messages);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(PhysicalAssetType.Anilox)]
    public void WhenPhysicalAssetChanged_Should_Emit_Published_Values_When_PhysicalAsset_Changed(
        PhysicalAssetType? physicalAssetTypeFilter)
    {
        // Arrange
        var scheduler = new TestScheduler();
        scheduler.Schedule(
            state: new ScheduleSetupPhysicalAssetState(PhysicalAssetId, PhysicalAssetType.Anilox),
            TimeSpan.FromSeconds(4),
            SetupPhysicalAssetUpdatedEvent);
        scheduler.Schedule(
            state: new ScheduleSetupPhysicalAssetState(PhysicalAssetId, PhysicalAssetType.Anilox),
            TimeSpan.FromMinutes(8),
            SetupPhysicalAssetUpdatedEvent);

        _schedulerProviderMock
            .Setup(x => x.GetScheduler())
            .Returns(scheduler);

        // Act
        var result = scheduler.Start(
            () => _physicalAssetChangedSubscription.WhenPhysicalAssetChanged(
                physicalAssetTypeFilter,
                _physicalAssetQueueWrapperMock.Object),
            created: 0,
            subscribed: 0,
            disposed: TimeSpan.FromMinutes(10).Ticks);

        // Assert
        var expectedTrigger = new[]
        {
            OnNext(TimeSpan.FromSeconds(4).Ticks, PhysicalAssetId),
            OnNext(TimeSpan.FromMinutes(8).Ticks, PhysicalAssetId)
        };
        ReactiveAssert.AreElementsEqual(expectedTrigger, result.Messages);
    }

    [Theory]
    [InlineData(null, PhysicalAssetType.Anilox, PhysicalAssetType.Anilox, 4)]
    [InlineData(null, PhysicalAssetType.Plate, PhysicalAssetType.Plate, 4)]
    [InlineData(PhysicalAssetType.Plate, PhysicalAssetType.Plate, PhysicalAssetType.Plate, 4)]
    [InlineData(PhysicalAssetType.Anilox, PhysicalAssetType.Plate, PhysicalAssetType.Plate, 0)]
    [InlineData(PhysicalAssetType.Anilox, PhysicalAssetType.Anilox, PhysicalAssetType.Plate, 2)]
    [InlineData(PhysicalAssetType.Anilox, PhysicalAssetType.Anilox, PhysicalAssetType.Anilox, 4)]
    public void WhenPhysicalAssetChanged_Should_Emit_Published_Values_When_PhysicalAssetType_Matched(
        PhysicalAssetType? physicalAssetTypeFilter,
        PhysicalAssetType firstScheduledPhysicalAssetType,
        PhysicalAssetType secondScheduledPhysicalAssetType,
        int expectedTrigger)
    {
        // Arrange
        var scheduler = new TestScheduler();
        scheduler.Schedule(
            state: new ScheduleSetupPhysicalAssetState(PhysicalAssetId, firstScheduledPhysicalAssetType),
            TimeSpan.FromSeconds(1),
            SetupPhysicalAssetCreatedEvent);
        scheduler.Schedule(
            state: new ScheduleSetupPhysicalAssetState(AnotherPhysicalAssetId, secondScheduledPhysicalAssetType),
            TimeSpan.FromMinutes(2),
            SetupPhysicalAssetCreatedEvent);
        scheduler.Schedule(
            state: new ScheduleSetupPhysicalAssetState(PhysicalAssetId, firstScheduledPhysicalAssetType),
            TimeSpan.FromSeconds(3),
            SetupPhysicalAssetUpdatedEvent);
        scheduler.Schedule(
            state: new ScheduleSetupPhysicalAssetState(AnotherPhysicalAssetId, secondScheduledPhysicalAssetType),
            TimeSpan.FromMinutes(4),
            SetupPhysicalAssetUpdatedEvent);

        _schedulerProviderMock
            .Setup(x => x.GetScheduler())
            .Returns(scheduler);

        // Act
        var result = scheduler.Start(
            () => _physicalAssetChangedSubscription.WhenPhysicalAssetChanged(
                physicalAssetTypeFilter,
                _physicalAssetQueueWrapperMock.Object),
            created: 0,
            subscribed: 0,
            disposed: TimeSpan.FromMinutes(5).Ticks);

        // Assert
        result.Messages.Should().HaveCount(expectedTrigger);
    }

    private async Task<IDisposable> SetupPhysicalAssetCreatedEvent(
        IScheduler _, ScheduleSetupPhysicalAssetState state)
    {
        await _capturedSubscribeToPhysicalAssetCreatedEventCallback!(
            new PhysicalAssetCreatedEvent(state.PhysicalAssetId, state.PhysicalAssetType, DateTime.UtcNow));

        return Disposable.Empty;
    }

    private async Task<IDisposable> SetupPhysicalAssetUpdatedEvent(
        IScheduler _, ScheduleSetupPhysicalAssetState state)
    {
        await _capturedSubscribeToPhysicalAssetUpdatedEventCallback!(
            new PhysicalAssetUpdatedEvent(state.PhysicalAssetId, state.PhysicalAssetType, DateTime.UtcNow));

        return Disposable.Empty;
    }
}