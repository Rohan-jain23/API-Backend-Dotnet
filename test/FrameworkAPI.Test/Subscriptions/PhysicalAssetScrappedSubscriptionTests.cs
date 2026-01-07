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

public class PhysicalAssetScrappedSubscriptionTests : ReactiveTest
{
    private const string PhysicalAssetId = "657c41c4435e848c718bb0c6";
    private const string AnotherPhysicalAssetId = "657c41c4435e848c718bb0c6";

    private readonly Mock<ISchedulerProvider> _schedulerProviderMock = new();
    private readonly Mock<IPhysicalAssetQueueWrapper> _physicalAssetQueueWrapperMock = new();
    private readonly PhysicalAssetScrappedSubscription _physicalAssetScrappedSubscription = new();

    private Func<PhysicalAssetScrappedEvent, Task>? _capturedSubscribeToPhysicalAssetScrappedEventCallback;

    private record ScheduleSetupPhysicalAssetState(string PhysicalAssetId, PhysicalAssetType PhysicalAssetType);

    public PhysicalAssetScrappedSubscriptionTests()
    {
        _physicalAssetQueueWrapperMock
            .Setup(m => m.SubscribeToPhysicalAssetScrappedEvent(It.IsAny<Func<PhysicalAssetScrappedEvent, Task>>()))
            .Callback<Func<PhysicalAssetScrappedEvent, Task>>(callback =>
            {
                _capturedSubscribeToPhysicalAssetScrappedEventCallback = callback;
            });
    }

    [Theory]
    [InlineData(null)]
    [InlineData(PhysicalAssetType.Anilox)]
    public void WhenPhysicalAssetScrapped_Should_Emit_Published_Values_When_PhysicalAsset_Scrapped(
        PhysicalAssetType? physicalAssetTypeFilter)
    {
        // Arrange
        var scheduler = new TestScheduler();
        scheduler.Schedule(
            state: new ScheduleSetupPhysicalAssetState(PhysicalAssetId, PhysicalAssetType.Anilox),
            TimeSpan.FromSeconds(9),
            SetupPhysicalAssetScrappedEvent);
        scheduler.Schedule(
            state: new ScheduleSetupPhysicalAssetState(PhysicalAssetId, PhysicalAssetType.Anilox),
            TimeSpan.FromMinutes(5),
            SetupPhysicalAssetScrappedEvent);

        _schedulerProviderMock
            .Setup(x => x.GetScheduler())
            .Returns(scheduler);

        // Act
        var result = scheduler.Start(
            () => _physicalAssetScrappedSubscription.WhenPhysicalAssetScrapped(
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
            SetupPhysicalAssetScrappedEvent);
        scheduler.Schedule(
            state: new ScheduleSetupPhysicalAssetState(AnotherPhysicalAssetId, secondScheduledPhysicalAssetType),
            TimeSpan.FromMinutes(2),
            SetupPhysicalAssetScrappedEvent);
        scheduler.Schedule(
            state: new ScheduleSetupPhysicalAssetState(PhysicalAssetId, firstScheduledPhysicalAssetType),
            TimeSpan.FromSeconds(3),
            SetupPhysicalAssetScrappedEvent);
        scheduler.Schedule(
            state: new ScheduleSetupPhysicalAssetState(AnotherPhysicalAssetId, secondScheduledPhysicalAssetType),
            TimeSpan.FromMinutes(4),
            SetupPhysicalAssetScrappedEvent);

        _schedulerProviderMock
            .Setup(x => x.GetScheduler())
            .Returns(scheduler);

        // Act
        var result = scheduler.Start(
            () => _physicalAssetScrappedSubscription.WhenPhysicalAssetScrapped(
                physicalAssetTypeFilter,
                _physicalAssetQueueWrapperMock.Object),
            created: 0,
            subscribed: 0,
            disposed: TimeSpan.FromMinutes(5).Ticks);

        // Assert
        result.Messages.Should().HaveCount(expectedTrigger);
    }

    private async Task<IDisposable> SetupPhysicalAssetScrappedEvent(
        IScheduler _, ScheduleSetupPhysicalAssetState state)
    {
        await _capturedSubscribeToPhysicalAssetScrappedEventCallback!(
            new PhysicalAssetScrappedEvent(state.PhysicalAssetId, state.PhysicalAssetType, DateTime.UtcNow));

        return Disposable.Empty;
    }
}