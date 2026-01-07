using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FrameworkAPI.Schema.PhysicalAsset;
using FrameworkAPI.Services;
using FrameworkAPI.Services.Interfaces;
using FrameworkAPI.Subscriptions;
using FrameworkAPI.Test.TestHelpers;
using HotChocolate.Execution;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Reactive.Testing;
using Moq;
using PhysicalAssetDataHandler.Client.Extensions;
using PhysicalAssetDataHandler.Client.HttpClients;
using PhysicalAssetDataHandler.Client.Models;
using PhysicalAssetDataHandler.Client.Models.Dtos;
using PhysicalAssetDataHandler.Client.Models.Enums;
using PhysicalAssetDataHandler.Client.Models.Messages.PhysicalAssetInformation;
using PhysicalAssetDataHandler.Client.QueueWrappers;
using WuH.Ruby.Common.Core;
using WuH.Ruby.LicenceManager.Client;
using WuH.Ruby.MachineDataHandler.Client;
using Xunit;

namespace FrameworkAPI.Test.Subscriptions.IntegrationTests;

public class PhysicalAssetChangedSubscriptionIntegrationTests : ReactiveTest
{
    private const string PhysicalAssetId = "657c41c4435e848c718bb0c6";
    private const string AnotherPhysicalAssetId = "65940d9c730f6ec4cb6188e7";

    private readonly Mock<IMachineCachingService> _machineCachingServiceMock = new();
    private readonly Mock<ILicenceManagerCachingService> _licenceManagerCachingServiceMock = new();
    private readonly Mock<IPhysicalAssetSettingsHttpClient> _physicalAssetSettingsHttpClientMock = new();
    private readonly Mock<IPhysicalAssetHttpClient> _physicalAssetHttpClientMock = new();
    private readonly Mock<IPhysicalAssetQueueWrapper> _physicalAssetQueueWrapperMock = new();
    private readonly Mock<ISchedulerProvider> _schedulerProviderMock = new();

    private Func<PhysicalAssetCreatedEvent, Task>? _capturedSubscribeToPhysicalAssetCreatedEventCallback;
    private Func<PhysicalAssetUpdatedEvent, Task>? _capturedSubscribeToPhysicalAssetUpdatedEventCallback;

    private record ScheduleSetupPhysicalAssetState(string PhysicalAssetId, PhysicalAssetType PhysicalAssetType);

    public PhysicalAssetChangedSubscriptionIntegrationTests()
    {
        var utcNow = DateTime.UtcNow;
        var usageCounter = new PhysicalAssetUsageCounterDto(
            current: 0, atLastCleaning: null, sinceLastCleaning: 0, cleaningInterval: 500, cleaningIntervalExceeded: false, unit: "mm");
        var timeUsageCounter = new PhysicalAssetTimeUsageCounterDto(
            current: 0, atLastCleaning: null, sinceLastCleaning: null, unit: "ms");
        _physicalAssetHttpClientMock
            .Setup(m => m.GetPhysicalAsset(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string physicalAssetId, CancellationToken _) =>
                new InternalItemResponse<PhysicalAssetDto>(
                    new AniloxPhysicalAssetDto(
                        physicalAssetId,
                        createdAt: utcNow,
                        lastChange: utcNow.AddMinutes(5),
                        serialNumber: "0123456789",
                        manufacturer: "Zecher",
                        description: null,
                        deliveredAt: null,
                        preferredUsageLocation: "EQ12345",
                        initialUsageCounter: null,
                        initialTimeUsageCounter: null,
                        scanCodes: ["123456789", "987654321"],
                        usageCounter,
                        timeUsageCounter,
                        lastCleaning: null,
                        lastConsumedMaterial: null,
                        equippedBy: null,
                        isSleeve: false,
                        printWidth: new ValueWithUnit<double>(3000, "mm"),
                        innerDiameter: null,
                        outerDiameter: new ValueWithUnit<double>(1000, "mm"),
                        screen: new ValueWithUnit<int>(30, "l/cm"),
                        engraving: null,
                        opticalDensity: new PhysicalAssetDataHandler.Client.Models.TestableValueWithUnit<double>(
                            setValue: 100.0, measuredValue: 98, measuredAt: DateTime.MinValue, unit: null),
                        volume: new PhysicalAssetDataHandler.Client.Models.TestableValueWithUnit<double>(
                            setValue: null, measuredValue: null, measuredAt: null, unit: "cm³/m²"))));

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
    public async Task WhenPhysicalAssetCreatedOrChanged_Should_Emit_Published_Values_When_PhysicalAsset_Created(
        PhysicalAssetType? physicalAssetTypeFilter)
    {
        // Arrange
        var machine = new Machine { MachineId = "EQ12345" };
        _machineCachingServiceMock
            .Setup(m => m.GetMachines(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync([machine]);
        _licenceManagerCachingServiceMock
            .Setup(m => m.GetDetailedLicenceValidity(machine.MachineId, Constants.LicensesApplications.Anilox, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<LicenceValidationInfo>(new LicenceValidationInfo { IsValid = true }));

        var scheduler = new TestScheduler();
        scheduler.Schedule(
            state: new ScheduleSetupPhysicalAssetState(PhysicalAssetId, PhysicalAssetType.Anilox),
            dueTime: TimeSpan.FromSeconds(1),
            SetupPhysicalAssetCreatedEvent);
        scheduler.Schedule(
            state: new ScheduleSetupPhysicalAssetState(AnotherPhysicalAssetId, PhysicalAssetType.Anilox),
            dueTime: TimeSpan.FromSeconds(4),
            SetupPhysicalAssetCreatedEvent);

        _schedulerProviderMock
            .Setup(x => x.GetScheduler())
            .Returns(scheduler);

        var executor = await InitializeExecutor();

        var query = GetSubscriptionQuery(physicalAssetTypeFilter);
        var request = QueryRequestBuilder
            .New()
            .SetQuery(query)
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        result.Should().BeAssignableTo<IResponseStream>();

        _capturedSubscribeToPhysicalAssetCreatedEventCallback.Should().NotBeNull();

        var responseStream = (IResponseStream)result;
        var taskCompletionSource = new TaskCompletionSource();

        // Act
        var physicalAssetIds = new List<string>();
        responseStream
            .ReadResultsAsync()
            .ToObservable()
            .Subscribe(queryResult =>
            {
                physicalAssetIds.Add(ParsePhysicalAssetId(queryResult));

                // Assert
                if (physicalAssetIds.Count == 2)
                {
                    taskCompletionSource.SetResult();
                }
            });

        scheduler.AdvanceTo(TimeSpan.FromSeconds(5).Ticks);

        // Assert
        // Wait for the two machine changes to be received
        await taskCompletionSource.Task.WaitAsync(timeout: TimeSpan.FromSeconds(1));

        physicalAssetIds.Should().HaveCount(2);
        physicalAssetIds.Should().Contain(PhysicalAssetId);
        physicalAssetIds.Should().Contain(AnotherPhysicalAssetId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(PhysicalAssetType.Anilox)]
    public async Task PhysicalAssetUpdated_Should_Emit_Published_Values_When_PhysicalAsset_Updated(
        PhysicalAssetType? physicalAssetTypeFilter)
    {
        // Arrange
        var machine = new Machine { MachineId = "EQ12345" };
        _machineCachingServiceMock
            .Setup(m => m.GetMachines(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync([machine]);
        _licenceManagerCachingServiceMock
            .Setup(m => m.GetDetailedLicenceValidity(machine.MachineId, Constants.LicensesApplications.Anilox, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<LicenceValidationInfo>(new LicenceValidationInfo { IsValid = true }));

        var scheduler = new TestScheduler();
        scheduler.Schedule(
            state: new ScheduleSetupPhysicalAssetState(PhysicalAssetId, PhysicalAssetType.Anilox),
            dueTime: TimeSpan.FromSeconds(1),
            SetupPhysicalAssetUpdatedEvent);
        scheduler.Schedule(
            state: new ScheduleSetupPhysicalAssetState(AnotherPhysicalAssetId, PhysicalAssetType.Anilox),
            dueTime: TimeSpan.FromSeconds(3),
            SetupPhysicalAssetUpdatedEvent);

        _schedulerProviderMock
            .Setup(x => x.GetScheduler())
            .Returns(scheduler);

        var executor = await InitializeExecutor();

        var query = GetSubscriptionQuery(physicalAssetTypeFilter);

        var request = QueryRequestBuilder
            .New()
            .SetQuery(query)
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        result.Should().BeAssignableTo<IResponseStream>();

        _capturedSubscribeToPhysicalAssetCreatedEventCallback.Should().NotBeNull();

        var responseStream = (IResponseStream)result;
        var taskCompletionSource = new TaskCompletionSource();

        // Act
        var physicalAssetIds = new List<string>();
        responseStream
            .ReadResultsAsync()
            .ToObservable()
            .Subscribe(queryResult =>
            {
                physicalAssetIds.Add(ParsePhysicalAssetId(queryResult));

                // Assert
                if (physicalAssetIds.Count == 2)
                {
                    taskCompletionSource.SetResult();
                }
            });

        scheduler.AdvanceTo(TimeSpan.FromSeconds(4).Ticks);

        // Assert
        // Wait for the two machine changes to be received
        await taskCompletionSource.Task.WaitAsync(timeout: TimeSpan.FromSeconds(1));

        physicalAssetIds.Should().HaveCount(2);
        physicalAssetIds.Should().Contain(PhysicalAssetId);
        physicalAssetIds.Should().Contain(AnotherPhysicalAssetId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(PhysicalAssetType.Anilox)]
    public async Task PhysicalAssetUpdated_Should_Emit_Published_Values_When_PhysicalAsset_Created_And_Updated(
        PhysicalAssetType? physicalAssetTypeFilter)
    {
        // Arrange
        var machine = new Machine { MachineId = "EQ12345" };
        _machineCachingServiceMock
            .Setup(m => m.GetMachines(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync([machine]);
        _licenceManagerCachingServiceMock
            .Setup(m => m.GetDetailedLicenceValidity(machine.MachineId, Constants.LicensesApplications.Anilox, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<LicenceValidationInfo>(new LicenceValidationInfo { IsValid = true }));

        var scheduler = new TestScheduler();
        scheduler.Schedule(
            state: new ScheduleSetupPhysicalAssetState(PhysicalAssetId, PhysicalAssetType.Anilox),
            dueTime: TimeSpan.FromSeconds(1),
            SetupPhysicalAssetCreatedEvent);
        scheduler.Schedule(
            state: new ScheduleSetupPhysicalAssetState(PhysicalAssetId, PhysicalAssetType.Anilox),
            dueTime: TimeSpan.FromSeconds(5),
            SetupPhysicalAssetUpdatedEvent);

        _schedulerProviderMock
            .Setup(x => x.GetScheduler())
            .Returns(scheduler);

        var executor = await InitializeExecutor();

        var query = GetSubscriptionQuery(physicalAssetTypeFilter);

        var request = QueryRequestBuilder
            .New()
            .SetQuery(query)
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        result.Should().BeAssignableTo<IResponseStream>();

        result.Should().BeAssignableTo<IResponseStream>();

        _capturedSubscribeToPhysicalAssetCreatedEventCallback.Should().NotBeNull();

        var responseStream = (IResponseStream)result;
        var taskCompletionSource = new TaskCompletionSource();

        // Act
        var physicalAssetIds = new List<string>();
        responseStream
            .ReadResultsAsync()
            .ToObservable()
            .Subscribe(queryResult =>
            {
                physicalAssetIds.Add(ParsePhysicalAssetId(queryResult));

                // Assert
                if (physicalAssetIds.Count == 2)
                {
                    taskCompletionSource.SetResult();
                }
            });

        scheduler.AdvanceTo(TimeSpan.FromSeconds(6).Ticks);

        // Assert
        // Wait for the two machine changes to be received
        await taskCompletionSource.Task.WaitAsync(timeout: TimeSpan.FromSeconds(1));
    }

    private static string GetSubscriptionQuery(PhysicalAssetType? physicalAssetTypeFilter)
    {
        return
            $"subscription {{ physicalAssetChanged(physicalAssetTypeFilter: {physicalAssetTypeFilter?.GetEnumMemberValue().ToUpper() ?? "null"}) {{ physicalAssetId }} }}";
    }

    private async Task<IRequestExecutor> InitializeExecutor()
    {
        var licenceService = new LicenceService(
            _machineCachingServiceMock.Object, _licenceManagerCachingServiceMock.Object);
        var physicalAssetService = new PhysicalAssetService(
                _physicalAssetSettingsHttpClientMock.Object,
                _physicalAssetHttpClientMock.Object,
                _physicalAssetQueueWrapperMock.Object);

        // Arrange
        return await new ServiceCollection()
            .AddSingleton(_physicalAssetQueueWrapperMock.Object)
            .AddSingleton(_schedulerProviderMock.Object)
            .AddSingleton<ILicenceService>(licenceService)
            .AddSingleton<IPhysicalAssetService>(physicalAssetService)
            .AddLogging()
            .AddGraphQLServer()
            .AddAuthorization()
            .ModifyOptions(t => t.StrictValidation = false)
            .AddType<AniloxPhysicalAsset>()
            .AddSubscriptionType(q => q.Name("Subscription"))
            .AddType<PhysicalAssetChangedSubscription>()
            .BuildRequestExecutorAsync();
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

    private static string ParsePhysicalAssetId(IQueryResult result)
    {
        var objectResult = result.Data!["physicalAssetChanged"] as IReadOnlyDictionary<string, object>;
        var physicalAssetId = objectResult!["physicalAssetId"].ToString()!;
        return physicalAssetId;
    }
}