using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FrameworkAPI.Exceptions;
using FrameworkAPI.Extensions;
using FrameworkAPI.Models;
using FrameworkAPI.Services;
using FrameworkAPI.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using WuH.Ruby.Common.Core;
using WuH.Ruby.MachineSnapShooter.Client;
using WuH.Ruby.MachineSnapShooter.Client.Enums;
using WuH.Ruby.MachineSnapShooter.Client.Models;
using WuH.Ruby.MachineSnapShooter.Client.Queue;
using Xunit;

namespace FrameworkAPI.Test.Services;

public class MachineTrendCachingServiceTests
{
    private const string MachineId = "EQ00001";
    private const string Machine2Id = "EQ00002";
    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    private readonly MachineTrendCachingService _subject;
    private readonly Mock<IMachineSnapshotHttpClient> _machineSnapshotHttpClientMock = new();
    private readonly Mock<ILatestMachineSnapshotCachingService> _latestMachineSnapshotCachingServiceMock = new();
    private readonly Mock<IMachineTimeService> _machineTimeServiceMock = new();
    private readonly Mock<IMachineSnapshotQueueWrapper> _machineSnapshotQueueWrapperMock = new();
    private readonly Mock<ILogger<MachineTrendCachingService>> _loggerMock = new();
    private Func<HistoricSnapshotChangeMessage, Task>? _triggerHistoricSnapshotChange;

    public MachineTrendCachingServiceTests()
    {
        _machineSnapshotQueueWrapperMock
            .Setup(m =>
                m.SubscribeForHistoricSnapshotChangeMessage(It.IsAny<Func<HistoricSnapshotChangeMessage, Task>>()))
            .Callback((Func<HistoricSnapshotChangeMessage, Task> handler) => _triggerHistoricSnapshotChange = handler);

        _subject = new MachineTrendCachingService(
            _machineSnapshotHttpClientMock.Object,
            _latestMachineSnapshotCachingServiceMock.Object,
            _machineTimeServiceMock.Object,
            _machineSnapshotQueueWrapperMock.Object,
            _loggerMock.Object);

        _machineSnapshotHttpClientMock
            .Setup(m => m.GetFirstSnapshot(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string machineId, CancellationToken ct) => new InternalItemResponse<MachineSnapshotResponse>(new MachineSnapshotResponse(
                new SnapshotMetaDto(machineId, "hash", DateTime.MinValue, []),
                new SnapshotDto([], DateTime.UnixEpoch.AddDays(-1))
            )));
    }

    [Fact]
    public async Task Get_Returns_MachineTrend()
    {
        // Arrange
        var to = DateTime.UnixEpoch;
        var from = to.Subtract(Constants.MachineTrend.TrendTimeSpan.Subtract(TimeSpan.FromMinutes(1)));

        _machineTimeServiceMock
            .Setup(m => m.Get(MachineId, _cancellationToken))
            .ReturnsAsync(new DataResult<DateTime?>(to, exception: null));

        var machineSnapshotListResponse = CreateMachineSnapshotListResponse(MachineId, to);

        _machineSnapshotHttpClientMock
            .Setup(m => m.GetSnapshotsInTimeRanges(
                MachineId,
                It.Is<List<string>>(list => list.SequenceEqual(Constants.MachineTrend.TrendingSnapshotColumnIds)),
                It.Is<List<TimeRange>>(list => list.Count == 1 && list[0].From == from && list[0].To == to),
                null,
                _cancellationToken))
            .ReturnsAsync(new InternalItemResponse<MachineSnapshotListResponse>(machineSnapshotListResponse));

        // Act
        var machineTrend = await _subject.Get(MachineId, _cancellationToken);

        var expectedMachineTrend =
            new SortedDictionary<DateTime, IReadOnlyDictionary<string, double?>>(machineSnapshotListResponse.Data.ToDictionary(
                snapshotDto => snapshotDto.SnapshotTime, snapshotDto => snapshotDto.GetMachineTrendElement()));
        machineTrend.Should().BeEquivalentTo(expectedMachineTrend);

        _machineTimeServiceMock.VerifyAll();
        _machineTimeServiceMock.VerifyNoOtherCalls();

        _machineSnapshotHttpClientMock.VerifyAll();
        _machineSnapshotHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Get_Returns_MachineTrend_With_Only_One_Hour_Of_Data()
    {
        // Arrange
        var to = DateTime.UnixEpoch;
        var from = to.Subtract(Constants.MachineTrend.TrendTimeSpan.Subtract(TimeSpan.FromMinutes(1)));
        var firstSnapshotTimestamp = DateTime.UnixEpoch.Subtract(TimeSpan.FromHours(1).Subtract(TimeSpan.FromMinutes(1)));

        _machineTimeServiceMock
            .Setup(m => m.Get(MachineId, _cancellationToken))
            .ReturnsAsync(new DataResult<DateTime?>(to, exception: null));

        _machineSnapshotHttpClientMock
            .Setup(m => m.GetFirstSnapshot(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string machineId, CancellationToken ct) => new InternalItemResponse<MachineSnapshotResponse>(new MachineSnapshotResponse(
                new SnapshotMetaDto(machineId, "hash", DateTime.MinValue, []),
                new SnapshotDto([], firstSnapshotTimestamp))));

        var machineSnapshotListResponse = CreateMachineSnapshotListResponse(MachineId, to);

        _machineSnapshotHttpClientMock
            .Setup(m => m.GetSnapshotsInTimeRanges(
                MachineId,
                It.Is<List<string>>(list => list.SequenceEqual(Constants.MachineTrend.TrendingSnapshotColumnIds)),
                It.Is<List<TimeRange>>(list => list.Count == 1 && list[0].From == firstSnapshotTimestamp && list[0].To == to),
                null,
                _cancellationToken))
            .ReturnsAsync(new InternalItemResponse<MachineSnapshotListResponse>(machineSnapshotListResponse));

        // Act
        var machineTrend = await _subject.Get(MachineId, _cancellationToken);

        var expectedMachineTrend =
            new SortedDictionary<DateTime, IReadOnlyDictionary<string, double?>>(machineSnapshotListResponse.Data.ToDictionary(
                snapshotDto => snapshotDto.SnapshotTime, snapshotDto => snapshotDto.GetMachineTrendElement()));
        machineTrend.Should().BeEquivalentTo(expectedMachineTrend);

        _machineTimeServiceMock.VerifyAll();
        _machineTimeServiceMock.VerifyNoOtherCalls();

        _machineSnapshotHttpClientMock.VerifyAll();
        _machineSnapshotHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Get_Returns_Cached_MachineTrend()
    {
        // Arrange
        var to = DateTime.UnixEpoch;
        var from = to.Subtract(Constants.MachineTrend.TrendTimeSpan.Subtract(TimeSpan.FromMinutes(1)));

        _machineTimeServiceMock
            .Setup(m => m.Get(MachineId, _cancellationToken))
            .ReturnsAsync(new DataResult<DateTime?>(to, exception: null));

        var machineSnapshotListResponse = CreateMachineSnapshotListResponse(MachineId, to);

        _machineSnapshotHttpClientMock
            .Setup(m => m.GetSnapshotsInTimeRanges(
                MachineId,
                It.Is<List<string>>(list => list.SequenceEqual(Constants.MachineTrend.TrendingSnapshotColumnIds)),
                It.Is<List<TimeRange>>(list => list.Count == 1 && list[0].From == from && list[0].To == to),
                null,
                _cancellationToken))
            .ReturnsAsync(new InternalItemResponse<MachineSnapshotListResponse>(machineSnapshotListResponse));

        // Request data and add it to the cache
        var machineTrend = await _subject.Get(MachineId, _cancellationToken);

        var expectedMachineTrend =
            new SortedDictionary<DateTime, IReadOnlyDictionary<string, double?>>(machineSnapshotListResponse.Data.ToDictionary(
                snapshotDto => snapshotDto.SnapshotTime, snapshotDto => snapshotDto.GetMachineTrendElement()));
        machineTrend.Should().BeEquivalentTo(expectedMachineTrend);

        // Act
        machineTrend = await _subject.Get(MachineId, _cancellationToken);

        // Assert
        machineTrend.Should().BeEquivalentTo(expectedMachineTrend);

        _machineTimeServiceMock.Verify(m => m.Get(MachineId, _cancellationToken), Times.Exactly(2));
        _machineTimeServiceMock.VerifyNoOtherCalls();

        _machineSnapshotHttpClientMock.Verify(
            m => m.GetSnapshotsInTimeRanges(MachineId, It.IsAny<List<string>>(), It.IsAny<List<TimeRange>>(), null, _cancellationToken), Times.Once);
        _machineSnapshotHttpClientMock.Verify(
            m => m.GetFirstSnapshot(MachineId, _cancellationToken), Times.Once);
        _machineSnapshotHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Get_Throws_Exception_When_Get_Of_MachineTimeService_Returns_Exception()
    {
        // Arrange
        _machineTimeServiceMock
            .Setup(m => m.Get(MachineId, _cancellationToken))
            .ReturnsAsync(new DataResult<DateTime?>(
                value: null,
                new InternalServiceException("Internal Service Exception", (int)HttpStatusCode.InternalServerError)));

        var getAction = () => _subject.Get(MachineId, _cancellationToken);

        // Act / Assert
        await getAction.Should().ThrowAsync<InternalServiceException>();

        _machineTimeServiceMock.VerifyAll();
        _machineTimeServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Get_Throws_Exception_When_GetSnapshotsInTimeRange_Returns_Error()
    {
        // Arrange
        var to = DateTime.UnixEpoch;
        var from = to.Subtract(Constants.MachineTrend.TrendTimeSpan.Subtract(TimeSpan.FromMinutes(1)));

        _machineTimeServiceMock
            .Setup(m => m.Get(MachineId, _cancellationToken))
            .ReturnsAsync(new DataResult<DateTime?>(to, exception: null));

        _machineSnapshotHttpClientMock
            .Setup(m => m.GetSnapshotsInTimeRanges(
                MachineId,
                It.Is<List<string>>(list => list.SequenceEqual(Constants.MachineTrend.TrendingSnapshotColumnIds)),
                It.Is<List<TimeRange>>(list => list.Count == 1 && list[0].From == from && list[0].To == to),
                null,
                _cancellationToken))
            .ReturnsAsync(new InternalItemResponse<MachineSnapshotListResponse>(
                (int)HttpStatusCode.InternalServerError, "Error"));

        var getAction = () => _subject.Get(MachineId, _cancellationToken);

        // Act / Assert
        await getAction.Should().ThrowAsync<InternalServiceException>();

        _machineTimeServiceMock.VerifyAll();
        _machineTimeServiceMock.VerifyNoOtherCalls();

        _machineSnapshotHttpClientMock.VerifyAll();
        _machineSnapshotHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Get_Returns_Null_When_GetSnapshotsInTimeRange_Returns_Error_WaitingForFirstMinutelySnapshot()
    {
        // Arrange
        var to = DateTime.UnixEpoch;
        var from = to.Subtract(Constants.MachineTrend.TrendTimeSpan.Subtract(TimeSpan.FromMinutes(1)));

        _machineTimeServiceMock
            .Setup(m => m.Get(MachineId, _cancellationToken))
            .ReturnsAsync(new DataResult<DateTime?>(to, exception: null));

        _machineSnapshotHttpClientMock
            .Setup(m => m.GetSnapshotsInTimeRanges(
                MachineId,
                It.Is<List<string>>(list => list.SequenceEqual(Constants.MachineTrend.TrendingSnapshotColumnIds)),
                It.Is<List<TimeRange>>(list => list.Count == 1 && list[0].From == from && list[0].To == to),
                null,
                _cancellationToken))
            .ReturnsAsync(new InternalItemResponse<MachineSnapshotListResponse>(
                (int)HttpStatusCode.InternalServerError,
                MachineSnapshotErrorItemType.WaitingForFirstMinutelySnapshot.ToString(),
                MachineSnapshotErrorItemType.WaitingForFirstMinutelySnapshot));

        // Act
        var result = await _subject.Get(MachineId, _cancellationToken);

        // Assert
        result.Should().BeNull();

        _machineTimeServiceMock.VerifyAll();
        _machineTimeServiceMock.VerifyNoOtherCalls();

        _machineSnapshotHttpClientMock.VerifyAll();
        _machineSnapshotHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Get_Returns_Null_When_GetFirstSnapshot_Returns_Error_WaitingForFirstMinutelySnapshot()
    {
        // Arrange
        var to = DateTime.UnixEpoch;
        var from = to.Subtract(Constants.MachineTrend.TrendTimeSpan.Subtract(TimeSpan.FromMinutes(1)));

        _machineTimeServiceMock
            .Setup(m => m.Get(MachineId, _cancellationToken))
            .ReturnsAsync(new DataResult<DateTime?>(to, exception: null));

        _machineSnapshotHttpClientMock
            .Setup(m => m.GetFirstSnapshot(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string machineId, CancellationToken ct) => new InternalItemResponse<MachineSnapshotResponse>(
                (int)HttpStatusCode.InternalServerError,
                MachineSnapshotErrorItemType.WaitingForFirstMinutelySnapshot.ToString(),
                MachineSnapshotErrorItemType.WaitingForFirstMinutelySnapshot));

        // Act
        var result = await _subject.Get(MachineId, _cancellationToken);

        // Assert
        result.Should().BeNull();

        _machineTimeServiceMock.VerifyAll();
        _machineTimeServiceMock.VerifyNoOtherCalls();

        _machineSnapshotHttpClientMock.VerifyAll();
        _machineSnapshotHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Get_Throws_Exception_When_GetFirstSnapshot_Returns_Error()
    {
        // Arrange
        var to = DateTime.UnixEpoch;
        var from = to.Subtract(Constants.MachineTrend.TrendTimeSpan.Subtract(TimeSpan.FromMinutes(1)));

        _machineTimeServiceMock
            .Setup(m => m.Get(MachineId, _cancellationToken))
            .ReturnsAsync(new DataResult<DateTime?>(to, exception: null));

        _machineSnapshotHttpClientMock
            .Setup(m => m.GetFirstSnapshot(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string machineId, CancellationToken ct) => new InternalItemResponse<MachineSnapshotResponse>(
                (int)HttpStatusCode.InternalServerError, "Error"));

        var getAction = () => _subject.Get(MachineId, _cancellationToken);

        // Act / Assert
        await getAction.Should().ThrowAsync<InternalServiceException>();

        _machineTimeServiceMock.VerifyAll();
        _machineTimeServiceMock.VerifyNoOtherCalls();

        _machineSnapshotHttpClientMock.VerifyAll();
        _machineSnapshotHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Cache_Gets_Updated_When_Snapshot_Cache_Changed_And_Snapshot_Is_Not_Null()
    {
        // Arrange
        var to = DateTime.UnixEpoch;
        var from = to.Subtract(Constants.MachineTrend.TrendTimeSpan.Subtract(TimeSpan.FromMinutes(1)));

        _machineTimeServiceMock
            .SetupSequence(m => m.Get(MachineId, _cancellationToken))
            .ReturnsAsync(new DataResult<DateTime?>(to, exception: null))
            .ReturnsAsync(new DataResult<DateTime?>(to.AddMinutes(1), exception: null));

        var machineSnapshotListResponse = CreateMachineSnapshotListResponse(MachineId, to);

        _machineSnapshotHttpClientMock
            .Setup(m => m.GetSnapshotsInTimeRanges(
                MachineId,
                It.Is<List<string>>(list => list.SequenceEqual(Constants.MachineTrend.TrendingSnapshotColumnIds)),
                It.Is<List<TimeRange>>(list => list.Count == 1 && list[0].From == from && list[0].To == to),
                null,
                _cancellationToken))
            .ReturnsAsync(new InternalItemResponse<MachineSnapshotListResponse>(machineSnapshotListResponse));

        // Request data and add it to the cache
        // We are calling "Clone()" here, because "Get(...)" returns a reference and updating the cache
        // would also affect this reference otherwise
        var machineTrend = (await _subject.Get(MachineId, _cancellationToken)).Clone();

        var latestSnapshotDto = machineSnapshotListResponse.Data.First();
        var updatedSnapshotDto = new SnapshotQueueMessageDto(
            latestSnapshotDto.ColumnValues,
            DateTime.MinValue,
            machineSnapshotListResponse.Meta.SchemaHash,
            latestSnapshotDto.SnapshotTime.AddMinutes(1),
            latestSnapshotDto.IsCreatedByVirtualTime);

        var machineSnapshotChangedEventArgs =
            new LiveSnapshotEventArgs(MachineId, updatedSnapshotDto, isMinutelySnapshot: true);

        // Act

        // Trigger a cache update
        _latestMachineSnapshotCachingServiceMock.Raise(m => m.CacheChanged += null, machineSnapshotChangedEventArgs);

        // Assert
        var updatedMachineTrend = await _subject.Get(MachineId, _cancellationToken);
        updatedMachineTrend.Should().HaveSameCount(machineTrend);
        updatedMachineTrend.Should().NotBeEquivalentTo(machineTrend);
        updatedMachineTrend!.Last().Key.Should().Be(updatedSnapshotDto.SnapshotTime);

        _latestMachineSnapshotCachingServiceMock.VerifyAdd(
            m => m.CacheChanged += It.IsAny<EventHandler<LiveSnapshotEventArgs>>(), Times.Once);
        _latestMachineSnapshotCachingServiceMock.VerifyNoOtherCalls();

        _machineTimeServiceMock.VerifyAll();
        _machineTimeServiceMock.VerifyNoOtherCalls();

        _machineSnapshotHttpClientMock.VerifyAll();
        _machineSnapshotHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Cache_Gets_Updated_Historically()
    {
        // Arrange
        var to = DateTime.UnixEpoch;
        var from = to.Subtract(Constants.MachineTrend.TrendTimeSpan.Subtract(TimeSpan.FromMinutes(1)));

        _machineTimeServiceMock
            .Setup(m => m.Get(MachineId, _cancellationToken))
            .ReturnsAsync(new DataResult<DateTime?>(to, exception: null));

        var machineSnapshotListResponse = CreateMachineSnapshotListResponse(MachineId, to);

        _machineSnapshotHttpClientMock
            .Setup(m => m.GetSnapshotsInTimeRanges(
                MachineId,
                It.Is<List<string>>(list => list.SequenceEqual(Constants.MachineTrend.TrendingSnapshotColumnIds)),
                It.Is<List<TimeRange>>(list => list.Count == 1 && list[0].From == from && list[0].To == to),
                null,
                _cancellationToken))
            .ReturnsAsync(new InternalItemResponse<MachineSnapshotListResponse>(machineSnapshotListResponse));

        _machineSnapshotHttpClientMock
            .Setup(m => m.GetSnapshotsInTimeRanges(
                MachineId,
                It.Is<List<string>>(list => list.SequenceEqual(new List<string> { SnapshotColumnIds.ExtrusionThroughput })),
                It.Is<List<TimeRange>>(list => list.Count == 1 && list[0].From == to && list[0].To == to),
                null,
                _cancellationToken))
            .ReturnsAsync(new InternalItemResponse<MachineSnapshotListResponse>(new MachineSnapshotListResponse(
                new SnapshotMetaDto(MachineId, "fakeHash", DateTime.MinValue, []),
                [new SnapshotDto([new SnapshotColumnValueDto(SnapshotColumnIds.ExtrusionThroughput, 4)], to)],
                [])));

        // Request data and add it to the cache
        // We are calling "Clone()" here, because "Get(...)" returns a reference and updating the cache
        // would also affect this reference otherwise
        var machineTrend = (await _subject.Get(MachineId, _cancellationToken)).Clone();
        (machineTrend?[to]?[SnapshotColumnIds.ExtrusionThroughput]).Should().Be(null);

        // Act

        if (_triggerHistoricSnapshotChange != null)
        {
            await _triggerHistoricSnapshotChange.Invoke(new HistoricSnapshotChangeMessage(
                MachineId,
                "fakeHash",
                DateTime.MinValue,
                to,
                to,
                [SnapshotColumnIds.ExtrusionThroughput]));
        }

        // Assert
        var updatedMachineTrend = await _subject.Get(MachineId, _cancellationToken);
        updatedMachineTrend.Should().HaveSameCount(machineTrend);
        updatedMachineTrend.Should().NotBeEquivalentTo(machineTrend);
        (updatedMachineTrend?[to]?[SnapshotColumnIds.ExtrusionThroughput]).Should().Be(4);

        _latestMachineSnapshotCachingServiceMock.VerifyAdd(
            m => m.CacheChanged += It.IsAny<EventHandler<LiveSnapshotEventArgs>>(), Times.Once);
        _latestMachineSnapshotCachingServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Cache_Should_Continue_To_Update_When_One_Machine_Throws_Error()
    {
        // Arrange
        var to = DateTime.UnixEpoch;

        _machineTimeServiceMock
            .SetupSequence(m => m.Get(MachineId, _cancellationToken))
            .ReturnsAsync(new DataResult<DateTime?>(to, null))
            .ReturnsAsync(new DataResult<DateTime?>(to.AddMinutes(20), null));

        _machineTimeServiceMock
            .SetupSequence(m => m.Get(Machine2Id, _cancellationToken))
            .ReturnsAsync(new DataResult<DateTime?>(to, null))
            .ReturnsAsync(new DataResult<DateTime?>(to.AddMinutes(20), null));

        var fistSnapshotListResponse1 = CreateMachineSnapshotListResponse(MachineId, to);
        var secondSnapshotListResponse1 = CreateMachineSnapshotListResponse(MachineId, to.AddMinutes(20));
        var fistSnapshotListResponse2 = CreateMachineSnapshotListResponse(Machine2Id, to);
        var secondSnapshotListResponse2 = CreateMachineSnapshotListResponse(Machine2Id, to.AddMinutes(19));

        _machineSnapshotHttpClientMock
            .SetupSequence(m =>
                m.GetSnapshotsInTimeRanges(MachineId, It.IsAny<List<string>>(), It.IsAny<List<TimeRange>>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<MachineSnapshotListResponse>(fistSnapshotListResponse1))
            .ReturnsAsync(new InternalItemResponse<MachineSnapshotListResponse>(500, "Error"))
            .ReturnsAsync(new InternalItemResponse<MachineSnapshotListResponse>(secondSnapshotListResponse1));

        _machineSnapshotHttpClientMock
            .SetupSequence(m =>
                m.GetSnapshotsInTimeRanges(Machine2Id, It.IsAny<List<string>>(), It.IsAny<List<TimeRange>>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<MachineSnapshotListResponse>(fistSnapshotListResponse2))
            .ReturnsAsync(new InternalItemResponse<MachineSnapshotListResponse>(secondSnapshotListResponse2));

        // Request data and add it to the cache
        // We are calling "Clone()" here, because "Get(...)" returns a reference and updating the cache
        // would also affect this reference otherwise
        var machineTrend1 = (await _subject.Get(MachineId, _cancellationToken)).Clone();
        var machineTrend2 = (await _subject.Get(Machine2Id, _cancellationToken)).Clone();

        SnapshotQueueMessageDto ChangeTime(SnapshotQueueMessageDto dto, DateTime time)
        {
            return new SnapshotQueueMessageDto(
                dto.ColumnValues,
                DateTime.MinValue,
                dto.SchemaHash,
                time,
                dto.IsCreatedByVirtualTime);
        }

        var latestSnapshotDto1 = SnapshotQueueMessageDto.FromSnapshotDto(fistSnapshotListResponse1.Data.First(), DateTime.MinValue, fistSnapshotListResponse1.Meta.SchemaHash);
        var updatedSnapshotDto1 = ChangeTime(latestSnapshotDto1, latestSnapshotDto1.SnapshotTime.AddMinutes(19));
        var machineSnapshotChangedEventArgs1 = new LiveSnapshotEventArgs(MachineId, updatedSnapshotDto1, true);
        var secondUpdatedSnapshotDto1 = ChangeTime(latestSnapshotDto1, latestSnapshotDto1.SnapshotTime.AddMinutes(20));
        var secondMachineSnapshotChangedEventArgs1 =
            new LiveSnapshotEventArgs(MachineId, secondUpdatedSnapshotDto1, true);

        var latestSnapshotDto2 = fistSnapshotListResponse2.Data.First();
        var updatedSnapshotDto2 = ChangeTime(latestSnapshotDto1, latestSnapshotDto2.SnapshotTime.AddMinutes(19));
        var machineSnapshotChangedEventArgs2 = new LiveSnapshotEventArgs(Machine2Id, updatedSnapshotDto2, true);
        var secondUpdatedSnapshotDto2 = ChangeTime(latestSnapshotDto1, latestSnapshotDto1.SnapshotTime.AddMinutes(20));
        var secondMachineSnapshotChangedEventArgs2 =
            new LiveSnapshotEventArgs(MachineId, secondUpdatedSnapshotDto2, true);

        // Act

        // Trigger a cache update
        _latestMachineSnapshotCachingServiceMock.Raise(m => m.CacheChanged += null, machineSnapshotChangedEventArgs1);
        _latestMachineSnapshotCachingServiceMock.Raise(m => m.CacheChanged += null, machineSnapshotChangedEventArgs2);
        _latestMachineSnapshotCachingServiceMock.Raise(m => m.CacheChanged += null,
            secondMachineSnapshotChangedEventArgs1);
        _latestMachineSnapshotCachingServiceMock.Raise(m => m.CacheChanged += null,
            secondMachineSnapshotChangedEventArgs2);

        // Assert
        var updatedMachineTrend1 = await _subject.Get(MachineId, _cancellationToken);
        updatedMachineTrend1.Should().HaveSameCount(machineTrend1);
        updatedMachineTrend1.Should().NotBeEquivalentTo(machineTrend1);
        updatedMachineTrend1!.Last().Key.Should().Be(secondUpdatedSnapshotDto2.SnapshotTime);
        var updatedMachineTrend2 = await _subject.Get(Machine2Id, _cancellationToken);
        updatedMachineTrend2.Should().HaveSameCount(machineTrend2);
        updatedMachineTrend2.Should().NotBeEquivalentTo(machineTrend2);
        updatedMachineTrend2!.Last().Key.Should().Be(secondUpdatedSnapshotDto2.SnapshotTime);

        _latestMachineSnapshotCachingServiceMock.VerifyAdd(
            m => m.CacheChanged += It.IsAny<EventHandler<LiveSnapshotEventArgs>>(), Times.Once);
        _latestMachineSnapshotCachingServiceMock.VerifyNoOtherCalls();

        _machineTimeServiceMock.VerifyAll();
        _machineTimeServiceMock.VerifyNoOtherCalls();

        _machineSnapshotHttpClientMock.VerifyAll();
        _machineSnapshotHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Cache_Gets_Cleared_When_Snapshot_Cache_Changed_And_Snapshot_Is_Null()
    {
        // Arrange
        var to = DateTime.UnixEpoch;
        var from = to.Subtract(Constants.MachineTrend.TrendTimeSpan.Subtract(TimeSpan.FromMinutes(1)));

        _machineTimeServiceMock
            .Setup(m => m.Get(MachineId, _cancellationToken))
            .ReturnsAsync(new DataResult<DateTime?>(to, exception: null));

        var machineSnapshotListResponse = CreateMachineSnapshotListResponse(MachineId, to);

        _machineSnapshotHttpClientMock
            .Setup(m => m.GetSnapshotsInTimeRanges(
                MachineId,
                It.Is<List<string>>(list => list.SequenceEqual(Constants.MachineTrend.TrendingSnapshotColumnIds)),
                It.Is<List<TimeRange>>(list => list.Count == 1 && list[0].From == from && list[0].To == to),
                null,
                _cancellationToken))
            .ReturnsAsync(new InternalItemResponse<MachineSnapshotListResponse>(machineSnapshotListResponse));

        // Request data and add it to the cache
        await _subject.Get(MachineId, _cancellationToken);

        var machineSnapshotChangedEventArgs =
            new LiveSnapshotEventArgs(MachineId, snapshotQueueMessageDto: null, isMinutelySnapshot: false);

        // Act

        // Trigger a cache clearing
        _latestMachineSnapshotCachingServiceMock.Raise(m => m.CacheChanged += null, machineSnapshotChangedEventArgs);

        // Assert
        await _subject.Get(MachineId, _cancellationToken);

        _latestMachineSnapshotCachingServiceMock.VerifyAdd(
            m => m.CacheChanged += It.IsAny<EventHandler<LiveSnapshotEventArgs>>(), Times.Once);
        _latestMachineSnapshotCachingServiceMock.VerifyNoOtherCalls();

        _machineTimeServiceMock.Verify(m => m.Get(MachineId, _cancellationToken), Times.Exactly(2));
        _machineTimeServiceMock.VerifyNoOtherCalls();

        _machineSnapshotHttpClientMock.Verify(
            m => m.GetSnapshotsInTimeRanges(MachineId, It.IsAny<List<string>>(), It.IsAny<List<TimeRange>>(), null, _cancellationToken), Times.Exactly(2));
        _machineSnapshotHttpClientMock.Verify(
            m => m.GetFirstSnapshot(MachineId, _cancellationToken), Times.Exactly(2));
        _machineSnapshotHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Cache_Does_Not_Get_Updated_When_Snapshot_Cache_Changed_And_MachineId_Key_Is_Not_Found()
    {
        // Arrange
        var to = DateTime.UnixEpoch;
        var from = to.Subtract(Constants.MachineTrend.TrendTimeSpan.Subtract(TimeSpan.FromMinutes(1)));

        _machineTimeServiceMock
            .Setup(m => m.Get(MachineId, _cancellationToken))
            .ReturnsAsync(new DataResult<DateTime?>(to, exception: null));

        var machineSnapshotListResponse = CreateMachineSnapshotListResponse(MachineId, to);

        _machineSnapshotHttpClientMock
            .Setup(m => m.GetSnapshotsInTimeRanges(
                MachineId,
                It.Is<List<string>>(list => list.SequenceEqual(Constants.MachineTrend.TrendingSnapshotColumnIds)),
                It.Is<List<TimeRange>>(list => list.Count == 1 && list[0].From == from && list[0].To == to),
                null,
                _cancellationToken))
            .ReturnsAsync(new InternalItemResponse<MachineSnapshotListResponse>(machineSnapshotListResponse));

        var latestSnapshotDto = machineSnapshotListResponse.Data.First();
        var updatedSnapshotDto = new SnapshotQueueMessageDto(
            latestSnapshotDto.ColumnValues,
            DateTime.MinValue,
            machineSnapshotListResponse.Meta.SchemaHash,
            latestSnapshotDto.SnapshotTime.Add(TimeSpan.FromMinutes(1)),
            latestSnapshotDto.IsCreatedByVirtualTime);

        var machineSnapshotChangedEventArgs =
            new LiveSnapshotEventArgs(MachineId, updatedSnapshotDto, isMinutelySnapshot: true);

        // Act

        // Trigger a cache update
        _latestMachineSnapshotCachingServiceMock.Raise(m => m.CacheChanged += null, machineSnapshotChangedEventArgs);

        // Assert
        var machineTrend = await _subject.Get(MachineId, _cancellationToken);

        var expectedMachineTrend =
            new SortedDictionary<DateTime, IReadOnlyDictionary<string, double?>>(machineSnapshotListResponse.Data.ToDictionary(
                snapshotDto => snapshotDto.SnapshotTime, snapshotDto => snapshotDto.GetMachineTrendElement()));
        machineTrend.Should().BeEquivalentTo(expectedMachineTrend);

        _latestMachineSnapshotCachingServiceMock.VerifyAdd(
            m => m.CacheChanged += It.IsAny<EventHandler<LiveSnapshotEventArgs>>(), Times.Once);
        _latestMachineSnapshotCachingServiceMock.VerifyNoOtherCalls();

        _machineTimeServiceMock.VerifyAll();
        _machineTimeServiceMock.VerifyNoOtherCalls();

        _machineSnapshotHttpClientMock.VerifyAll();
        _machineSnapshotHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Cache_Should_Restore_On_Second_Get_After_Empty_Snapshot_Clears_Cache_And_First_Get_Fails()
    {
        // Arrange
        var to = DateTime.UnixEpoch;
        var from = to.Subtract(Constants.MachineTrend.TrendTimeSpan.Subtract(TimeSpan.FromMinutes(1)));

        _machineTimeServiceMock
            .SetupSequence(m => m.Get(MachineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DataResult<DateTime?>(to, null))
            .ReturnsAsync(new DataResult<DateTime?>(to.AddMinutes(1), null))
            .ReturnsAsync(new DataResult<DateTime?>(to.AddMinutes(2), null));

        var firstMachineSnapshotListResponse = CreateMachineSnapshotListResponse(MachineId, to);
        var secondMachineSnapshotListResponse = CreateMachineSnapshotListResponse(MachineId, to.AddMinutes(2));

        _machineSnapshotHttpClientMock
            .SetupSequence(m => m.GetSnapshotsInTimeRanges(MachineId, It.IsAny<List<string>>(), It.IsAny<List<TimeRange>>(), null, _cancellationToken))
            .ReturnsAsync(new InternalItemResponse<MachineSnapshotListResponse>(firstMachineSnapshotListResponse))
            .ReturnsAsync(new InternalItemResponse<MachineSnapshotListResponse>(500, "Error"))
            .ReturnsAsync(new InternalItemResponse<MachineSnapshotListResponse>(secondMachineSnapshotListResponse));

        var machineSnapshotChangedEventArgs = new LiveSnapshotEventArgs(MachineId, null, true);

        // Act

        // Trigger a cache update
        await _subject.Get(MachineId, _cancellationToken);
        _latestMachineSnapshotCachingServiceMock.Raise(m => m.CacheChanged -= null, machineSnapshotChangedEventArgs);

        // Assert
        var get = async () => await _subject.Get(MachineId, _cancellationToken);
        await get.Should().ThrowAsync<InternalServiceException>();

        var machineTrendSuccess = await _subject.Get(MachineId, _cancellationToken);
        var expectedMachineTrendSuccess =
            new SortedDictionary<DateTime, IReadOnlyDictionary<string, double?>>(
                secondMachineSnapshotListResponse.Data.ToDictionary(
                    snapshotDto => snapshotDto.SnapshotTime, snapshotDto => snapshotDto.GetMachineTrendElement()));
        machineTrendSuccess.Should().BeEquivalentTo(expectedMachineTrendSuccess);

        _latestMachineSnapshotCachingServiceMock.VerifyAdd(
            m => m.CacheChanged += It.IsAny<EventHandler<LiveSnapshotEventArgs>>(), Times.Once);
        _latestMachineSnapshotCachingServiceMock.VerifyNoOtherCalls();

        _machineTimeServiceMock.VerifyAll();
        _machineTimeServiceMock.VerifyNoOtherCalls();

        _machineSnapshotHttpClientMock.VerifyAll();
        _machineSnapshotHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Cache_Does_Not_Get_Updated_When_Snapshot_Cache_Changed_And_Snapshot_Is_Not_Minutely()
    {
        // Arrange
        var to = DateTime.UnixEpoch;
        var from = to.Subtract(Constants.MachineTrend.TrendTimeSpan.Subtract(TimeSpan.FromMinutes(1)));

        _machineTimeServiceMock
            .Setup(m => m.Get(MachineId, _cancellationToken))
            .ReturnsAsync(new DataResult<DateTime?>(to, exception: null));

        var machineSnapshotListResponse = CreateMachineSnapshotListResponse(MachineId, to);

        _machineSnapshotHttpClientMock
            .Setup(m => m.GetSnapshotsInTimeRanges(
                MachineId,
                It.Is<List<string>>(list => list.SequenceEqual(Constants.MachineTrend.TrendingSnapshotColumnIds)),
                It.Is<List<TimeRange>>(list => list.Count == 1 && list[0].From == from && list[0].To == to),
                null,
                _cancellationToken))
            .ReturnsAsync(new InternalItemResponse<MachineSnapshotListResponse>(machineSnapshotListResponse));

        // Request data and add it to the cache
        var machineTrend = await _subject.Get(MachineId, _cancellationToken);

        var expectedMachineTrend =
            new SortedDictionary<DateTime, IReadOnlyDictionary<string, double?>>(machineSnapshotListResponse.Data.ToDictionary(
                snapshotDto => snapshotDto.SnapshotTime, snapshotDto => snapshotDto.GetMachineTrendElement()));
        machineTrend.Should().BeEquivalentTo(expectedMachineTrend);

        var latestSnapshotDto = machineSnapshotListResponse.Data.First();
        var updatedSnapshotDto = new SnapshotQueueMessageDto(
            latestSnapshotDto.ColumnValues,
            DateTime.MinValue,
            machineSnapshotListResponse.Meta.SchemaHash,
            latestSnapshotDto.SnapshotTime.Add(TimeSpan.FromMinutes(1)),
            latestSnapshotDto.IsCreatedByVirtualTime);

        var machineSnapshotChangedEventArgs =
            new LiveSnapshotEventArgs(MachineId, updatedSnapshotDto, isMinutelySnapshot: false);

        // Act

        // Trigger a cache update
        _latestMachineSnapshotCachingServiceMock.Raise(m => m.CacheChanged += null, machineSnapshotChangedEventArgs);

        // Assert
        machineTrend = await _subject.Get(MachineId, _cancellationToken);
        machineTrend.Should().BeEquivalentTo(expectedMachineTrend);

        _latestMachineSnapshotCachingServiceMock.VerifyAdd(
            m => m.CacheChanged += It.IsAny<EventHandler<LiveSnapshotEventArgs>>(), Times.Once);
        _latestMachineSnapshotCachingServiceMock.VerifyNoOtherCalls();

        _machineTimeServiceMock.VerifyAll();
        _machineTimeServiceMock.VerifyNoOtherCalls();

        _machineSnapshotHttpClientMock.VerifyAll();
        _machineSnapshotHttpClientMock.VerifyNoOtherCalls();
    }

    private static MachineSnapshotListResponse CreateMachineSnapshotListResponse(string machineId, DateTime to, TimeSpan? existingTimeSpan = null)
    {
        var snapshotMetaDto = new SnapshotMetaDto(machineId, "fakeHash", DateTime.MinValue, []);
        var snapshotDtos = new List<SnapshotDto>();

        var timeSpan = existingTimeSpan is null ? Constants.MachineTrend.TrendTimeSpan : existingTimeSpan;
        for (var i = 0; i < (int)timeSpan.Value.TotalMinutes; i++)
        {
            snapshotDtos.Add(new SnapshotDto(
                [
                    new(SnapshotColumnIds.ExtrusionThroughput, i == 0 ? null : i * 1.5)
                ],
                to.Subtract(TimeSpan.FromMinutes(i))));
        }

        var machineSnapshotListResponse = new MachineSnapshotListResponse(snapshotMetaDto, snapshotDtos, []);
        return machineSnapshotListResponse;
    }
}