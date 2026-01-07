using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FrameworkAPI.Exceptions;
using FrameworkAPI.Models;
using FrameworkAPI.Models.Events;
using FrameworkAPI.Services;
using FrameworkAPI.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Threading;
using Moq;
using WuH.Ruby.Common.Core;
using WuH.Ruby.MachineSnapShooter.Client;
using WuH.Ruby.MachineSnapShooter.Client.Models;
using Xunit;

namespace FrameworkAPI.Test.Services;

public class MachineTimeServiceTests
{
    public static IEnumerable<object?[]> GetMachineTimeFromCachesTestData()
    {
        yield return new object?[]
        {
            new DateTime(2020, 01, 01, 12, 30, 00, DateTimeKind.Utc),
            new DateTime(2021, 01, 01, 12, 30, 00, DateTimeKind.Utc),
            new DateTime(2021, 01, 01, 12, 30, 00, DateTimeKind.Utc)
        };
        yield return new object?[]
        {
            new DateTime(2021, 01, 01, 12, 30, 00, DateTimeKind.Utc),
            new DateTime(2020, 01, 01, 12, 30, 00, DateTimeKind.Utc),
            new DateTime(2021, 01, 01, 12, 30, 00, DateTimeKind.Utc)
        };
        yield return new object?[]
        {
            null,
            new DateTime(2020, 01, 01, 12, 30, 00, DateTimeKind.Utc),
            new DateTime(2020, 01, 01, 12, 30, 00, DateTimeKind.Utc)
        };
        yield return new object?[]
        {
            new DateTime(2020, 01, 01, 12, 30, 00, DateTimeKind.Utc),
            null,
            new DateTime(2020, 01, 01, 12, 30, 00, DateTimeKind.Utc)
        };
    }

    private const string MachineId = "EQ12345";

    private readonly MachineTimeService _machineTimeService;
    private readonly Mock<IOpcUaServerTimeCachingService> _opcUaServerTimeCachingServiceMock;
    private readonly Mock<ILatestMachineSnapshotCachingService> _latestMachineSnapshotCachingServiceMock;

    public MachineTimeServiceTests()
    {
        _opcUaServerTimeCachingServiceMock = new Mock<IOpcUaServerTimeCachingService>();
        _opcUaServerTimeCachingServiceMock
            .SetupAdd(m =>
                m.CacheChanged += It.IsAny<AsyncEventHandler<MachineTimeChangedEventArgs>>());

        _latestMachineSnapshotCachingServiceMock = new Mock<ILatestMachineSnapshotCachingService>();
        _latestMachineSnapshotCachingServiceMock
            .SetupAdd(m =>
                m.CacheChanged += It.IsAny<EventHandler<LiveSnapshotEventArgs>>());

        _machineTimeService = new MachineTimeService(
            _opcUaServerTimeCachingServiceMock.Object,
            _latestMachineSnapshotCachingServiceMock.Object,
            new Mock<ILogger<MachineTimeService>>().Object);
    }

    [Fact]
    public void Constructor_Register_To_CacheChanged_Events()
    {
        // Assert
        _opcUaServerTimeCachingServiceMock
            .VerifyAdd(m =>
                m.CacheChanged += It.IsAny<AsyncEventHandler<MachineTimeChangedEventArgs>>(), Times.Once);
        _latestMachineSnapshotCachingServiceMock
            .VerifyAdd(m =>
                m.CacheChanged += It.IsAny<EventHandler<LiveSnapshotEventArgs>>(), Times.Once);
    }

    [Fact]
    public void Dispose_Unregister_To_CacheChanged_Events()
    {
        // Act
        _machineTimeService.Dispose();

        // Assert
        _opcUaServerTimeCachingServiceMock
            .VerifyRemove(m =>
                m.CacheChanged -= It.IsAny<AsyncEventHandler<MachineTimeChangedEventArgs>>(), Times.Once);
        _latestMachineSnapshotCachingServiceMock
            .VerifyRemove(m => m.CacheChanged -= It.IsAny<EventHandler<LiveSnapshotEventArgs>>(),
                Times.Once);
    }

    [Theory]
    [MemberData(nameof(GetMachineTimeFromCachesTestData))]
    public async Task Get_MachineTime_From_Caches_Return_DataResult_With_MachineTime(
        DateTime? opcUaServerTime, DateTime snapshotTime, DateTime? expectedMachineTime)
    {
        // Arrange
        _opcUaServerTimeCachingServiceMock
            .Setup(m =>
                m.Get(MachineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DataResult<DateTime?>(opcUaServerTime, exception: null));

        _latestMachineSnapshotCachingServiceMock
            .Setup(m =>
                m.GetLatestMachineSnapshot(MachineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<MachineSnapshotResponse>(
                new MachineSnapshotResponse(
                    new SnapshotMetaDto(MachineId, "fakeHash", DateTime.MinValue, []),
                    new SnapshotDto([], snapshotTime))));

        // Act
        var result = await _machineTimeService.Get(MachineId, CancellationToken.None);

        // Assert
        result.Value.Should().Be(expectedMachineTime);
        result.Exception.Should().BeNull();

        _opcUaServerTimeCachingServiceMock.VerifyAll();
        _opcUaServerTimeCachingServiceMock.VerifyNoOtherCalls();

        _latestMachineSnapshotCachingServiceMock.VerifyAll();
        _latestMachineSnapshotCachingServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Get_MachineTime_From_OpcUaCache_Failed_Return_DataResult_With_MachineTime_From_Snapshot()
    {
        // Arrange
        var snapshotTime = new DateTime(2020, 01, 01, 12, 30, 00, DateTimeKind.Utc);

        _opcUaServerTimeCachingServiceMock
            .Setup(m => m.Get(MachineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DataResult<DateTime?>(
                value: null, exception: new Exception("Internal Exception")));

        _latestMachineSnapshotCachingServiceMock
            .Setup(m =>
                m.GetLatestMachineSnapshot(MachineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<MachineSnapshotResponse>(
                new MachineSnapshotResponse(
                    new SnapshotMetaDto(MachineId, "fakeHash", DateTime.MinValue, new List<SnapshotColumnUnitDto>()),
                    new SnapshotDto(new List<SnapshotColumnValueDto>(), snapshotTime))));

        // Act
        var result = await _machineTimeService.Get(MachineId, CancellationToken.None);

        // Assert
        result.Value.Should().Be(snapshotTime);
        result.Exception.Should().BeNull();
    }

    [Fact]
    public async Task Get_MachineTime_From_LastSnapshotCache_Failed_Return_DataResult_With_MachineTime_From_OpcUa()
    {
        // Arrange
        var upcUaMachineTime = new DateTime(2020, 01, 01, 12, 30, 00, DateTimeKind.Utc);

        _opcUaServerTimeCachingServiceMock
            .Setup(m => m.Get(MachineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DataResult<DateTime?>(
                value: upcUaMachineTime, exception: null));

        _latestMachineSnapshotCachingServiceMock
            .Setup(m =>
                m.GetLatestMachineSnapshot(MachineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<MachineSnapshotResponse>(
                new InternalError((int)HttpStatusCode.InternalServerError, "Internal Error")));

        // Act
        var result = await _machineTimeService.Get(MachineId, CancellationToken.None);

        // Assert
        result.Value.Should().Be(upcUaMachineTime);
        result.Exception.Should().BeNull();
    }

    [Fact]
    public async Task Get_MachineTime_From_Caches_Failed_Return_DataResult_With_InternalServiceException()
    {
        // Arrange
        _opcUaServerTimeCachingServiceMock
            .Setup(m => m.Get(MachineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DataResult<DateTime?>(
                value: null, exception: new Exception("Internal Exception")));

        _latestMachineSnapshotCachingServiceMock
            .Setup(m =>
                m.GetLatestMachineSnapshot(MachineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<MachineSnapshotResponse>(
                new InternalError((int)HttpStatusCode.InternalServerError, "Internal Error")));

        // Act
        var result = await _machineTimeService.Get(MachineId, CancellationToken.None);

        // Assert
        result.Value.Should().BeNull();
        result.Exception.Should().BeOfType<InternalServiceException>();
    }

    [Fact]
    public async Task OpcUaServerTimeCachingServiceCacheChanged_With_Value_Triggers_MachineTimeChanged()
    {
        // Arrange
        var expectedMachineTime =
            new DateTime(2020, 01, 01, 12, 30, 00, DateTimeKind.Utc);

        DateTime? triggeredMachineTime = null;

        _machineTimeService.MachineTimeChanged += (_, args) =>
        {
            triggeredMachineTime = args.MachineTime;
            return Task.CompletedTask;
        };

        _opcUaServerTimeCachingServiceMock
            .Setup(m => m.Get(MachineId, CancellationToken.None))
            .ReturnsAsync(new DataResult<DateTime?>(value: expectedMachineTime, exception: null));
        _latestMachineSnapshotCachingServiceMock
            .Setup(m =>
                m.GetLatestMachineSnapshot(MachineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<MachineSnapshotResponse>(
                new InternalError((int)HttpStatusCode.InternalServerError, "Internal Error")));

        var machineSnapshotChangedEventArgs = new MachineTimeChangedEventArgs(MachineId, expectedMachineTime);

        // Act
        _opcUaServerTimeCachingServiceMock
            .Raise(m => m.CacheChanged += null, machineSnapshotChangedEventArgs);
        var result = await _machineTimeService.Get(MachineId, CancellationToken.None);

        // Assert
        triggeredMachineTime.Should().Be(expectedMachineTime);

        result.Value.Should().Be(expectedMachineTime);
        result.Exception.Should().BeNull();
    }

    [Fact]
    public void
        OpcUaServerTimeCachingServiceCacheChanged_Handling_Failed_To_Get_Value_MachineTimeChanged_Not_Triggered()
    {
        // Arrange
        var machineTime =
            new DateTime(2020, 01, 01, 12, 30, 00, DateTimeKind.Utc);

        bool? triggered = false;
        _machineTimeService.MachineTimeChanged += (_, _) =>
        {
            triggered = true;
            return Task.CompletedTask;
        };

        _opcUaServerTimeCachingServiceMock
            .Setup(m => m.Get(MachineId, CancellationToken.None))
            .ReturnsAsync(new DataResult<DateTime?>(value: null, exception: new Exception("Exception")));
        _latestMachineSnapshotCachingServiceMock
            .Setup(m =>
                m.GetLatestMachineSnapshot(MachineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<MachineSnapshotResponse>(
                new InternalError((int)HttpStatusCode.InternalServerError, "Internal Error")));

        var machineSnapshotChangedEventArgs = new MachineTimeChangedEventArgs(MachineId, machineTime);

        // Act
        _opcUaServerTimeCachingServiceMock
            .Raise(m => m.CacheChanged += null, machineSnapshotChangedEventArgs);

        // Assert
        triggered.Should().BeFalse();
    }

    [Fact]
    public async Task LatestMachineSnapshotCachingServiceCacheChanged_With_Value_Triggers_MachineTimeChanged()
    {
        // Arrange
        var expectedMachineTime =
            new DateTime(2020, 01, 01, 12, 30, 00, DateTimeKind.Utc);
        var snapshotDto = new SnapshotQueueMessageDto(
            new List<SnapshotColumnValueDto>(), DateTime.MinValue, "fakeHash", expectedMachineTime);

        DateTime? triggeredMachineTime = null;

        _machineTimeService.MachineTimeChanged += (_, args) =>
        {
            triggeredMachineTime = args.MachineTime;
            return Task.CompletedTask;
        };

        _opcUaServerTimeCachingServiceMock
            .Setup(m => m.Get(MachineId, CancellationToken.None))
            .ReturnsAsync(new DataResult<DateTime?>(value: null, exception: null));
        _latestMachineSnapshotCachingServiceMock
            .Setup(m =>
                m.GetLatestMachineSnapshot(MachineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<MachineSnapshotResponse>(
                new MachineSnapshotResponse(
                    new SnapshotMetaDto(MachineId, "fakeHash", DateTime.MinValue, new List<SnapshotColumnUnitDto>()),
                    snapshotDto)));

        var machineSnapshotChangedEventArgs =
            new LiveSnapshotEventArgs(MachineId, snapshotDto, isMinutelySnapshot: true);

        // Act
        _latestMachineSnapshotCachingServiceMock
            .Raise(m => m.CacheChanged += null, machineSnapshotChangedEventArgs);
        var result = await _machineTimeService.Get(MachineId, CancellationToken.None);

        // Assert
        triggeredMachineTime.Should().Be(expectedMachineTime);

        result.Value.Should().Be(expectedMachineTime);
        result.Exception.Should().BeNull();
    }

    [Fact]
    public void
        LatestMachineSnapshotCachingServiceCacheChanged_Handling_Failed_To_Get_Value_MachineTimeChanged_Not_Triggered()
    {
        // Arrange
        var snapshotDto = new SnapshotQueueMessageDto(new List<SnapshotColumnValueDto>(), DateTime.MinValue, "fakeHash", DateTime.UtcNow);

        bool? triggered = false;
        _machineTimeService.MachineTimeChanged += (_, _) =>
        {
            triggered = true;
            return Task.CompletedTask;
        };

        _opcUaServerTimeCachingServiceMock
            .Setup(m => m.Get(MachineId, CancellationToken.None))
            .ReturnsAsync(new DataResult<DateTime?>(value: null, exception: new Exception("Exception")));
        _latestMachineSnapshotCachingServiceMock
            .Setup(m =>
                m.GetLatestMachineSnapshot(MachineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<MachineSnapshotResponse>(
                new InternalError((int)HttpStatusCode.InternalServerError, "Internal Error")));

        var machineSnapshotChangedEventArgs =
            new LiveSnapshotEventArgs(MachineId, snapshotDto, isMinutelySnapshot: true);

        // Act
        _latestMachineSnapshotCachingServiceMock
            .Raise(m => m.CacheChanged += null, machineSnapshotChangedEventArgs);

        // Assert
        triggered.Should().BeFalse();
    }
}