using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FrameworkAPI.DataLoaders;
using FrameworkAPI.Extensions;
using FrameworkAPI.Models;
using HotChocolate.Fetching;
using Moq;
using WuH.Ruby.Common.Core;
using WuH.Ruby.MachineSnapShooter.Client;
using WuH.Ruby.MachineSnapShooter.Client.Models;
using Xunit;

namespace FrameworkAPI.Test.DataLoaders;

public class MachineTrendByTimeRangeBatchDataLoaderTests
{
    private const string MachineId1 = "EQ00001";
    private const string MachineId2 = "EQ00002";
    private readonly Mock<IMachineSnapshotHttpClient> _machineSnapshotHttpClientMock = new();

    private readonly BatchScheduler _batchScheduler = new();
    private readonly MachineTrendByTimeRangeBatchDataLoader _subject;

    public MachineTrendByTimeRangeBatchDataLoaderTests()
    {
        _subject = new MachineTrendByTimeRangeBatchDataLoader(_machineSnapshotHttpClientMock.Object, _batchScheduler);

        _machineSnapshotHttpClientMock
            .Setup(m => m.GetSnapshotsInTimeRanges(
                It.IsAny<string>(),
                It.IsAny<List<string>>(),
                It.IsAny<List<TimeRange>>(),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((
                string machineId,
                List<string> columnIds,
                List<TimeRange> timeRanges,
                List<Filter> filters,
                CancellationToken cancellationToken) => new InternalItemResponse<MachineSnapshotListResponse>(
                new MachineSnapshotListResponse(
                    new SnapshotMetaDto(machineId, "fakeHash", DateTime.MinValue, []),
                    timeRanges
                        .SelectMany(timeRange => timeRange
                            .Every(TimeSpan.FromMinutes(1))
                            .Select(dateTime => new SnapshotDto(
                                columnIds
                                    .Select(columnId => new SnapshotColumnValueDto(columnId, 10))
                                    .ToList(),
                                dateTime)))
                        .ToList(),
                    [])));
    }

    [Fact]
    public void Equal_Keys_Should_Be_Equal()
    {
        var key1 = (MachineId1, new TimeRange(DateTime.UnixEpoch, DateTime.UnixEpoch.AddDays(1)), "Column1");
        var key2 = (MachineId1, new TimeRange(DateTime.UnixEpoch, DateTime.UnixEpoch.AddDays(1)), "Column1");

        _ = _subject.LoadAsync(key1);
        _ = _subject.LoadAsync(key2);

        key1.Equals(key2).Should().BeTrue();
    }

    [Fact]
    public async Task LoadBatchAsync_With_One_Machine_Multiple_Columns_EqualTimeRanges()
    {
        // Arrange
        var tasks = new List<Task<DataResult<IDictionary<DateTime, object?>>>>
        {
            _subject.LoadAsync(
                (MachineId1, new TimeRange(DateTime.UnixEpoch, DateTime.UnixEpoch.AddDays(1)), "Column1")),
            _subject.LoadAsync(
                (MachineId1, new TimeRange(DateTime.UnixEpoch, DateTime.UnixEpoch.AddDays(1)), "Column2")),
            _subject.LoadAsync(
                (MachineId1, new TimeRange(DateTime.UnixEpoch, DateTime.UnixEpoch.AddDays(1)), "Column3")),
        };

        // Act
        _batchScheduler.BeginDispatch();
        var results = await Task.WhenAll(tasks);

        // Assert
        _machineSnapshotHttpClientMock
            .Verify(
                m => m.GetSnapshotsInTimeRanges(
                    It.IsAny<string>(),
                    It.Is<List<string>>(list => list.SequenceEqual(new List<string> { "Column1", "Column2", "Column3" })),
                    It.Is<List<TimeRange>>(list => list.SequenceEqual(new List<TimeRange> { new(DateTime.UnixEpoch, DateTime.UnixEpoch.AddDays(1)) })),
                    It.IsAny<List<Filter>>(),
                    It.IsAny<CancellationToken>()),
                Times.Once());
        _machineSnapshotHttpClientMock.VerifyNoOtherCalls();
        results.Should().NotBeNull();
        results.Should().HaveCount(3);
        results.Should().AllSatisfy(result => result.Value.Should().NotBeNull());
    }

    [Fact]
    public async Task LoadBatchAsync_With_One_Machine_Multiple_Columns_DifferentTimeRanges()
    {
        // Arrange
        var tasks = new List<Task<DataResult<IDictionary<DateTime, object?>>>>
        {
            _subject.LoadAsync(
                (MachineId1, new TimeRange(DateTime.UnixEpoch, DateTime.UnixEpoch.AddDays(1)), "Column1")),
            _subject.LoadAsync(
                (MachineId1, new TimeRange(DateTime.UnixEpoch.AddDays(2), DateTime.UnixEpoch.AddDays(3)), "Column2")),
            _subject.LoadAsync(
                (MachineId1, new TimeRange(DateTime.UnixEpoch, DateTime.UnixEpoch.AddDays(1)), "Column3")),
        };

        // Act
        _batchScheduler.BeginDispatch();
        var results = await Task.WhenAll(tasks);

        // Assert
        _machineSnapshotHttpClientMock
            .Verify(
                m => m.GetSnapshotsInTimeRanges(
                    MachineId1,
                    It.Is<List<string>>(list => list.SequenceEqual(new List<string> { "Column1", "Column2", "Column3" })),
                    It.Is<List<TimeRange>>(list => list.SequenceEqual(new List<TimeRange>
                    {
                        new(DateTime.UnixEpoch.AddDays(2), DateTime.UnixEpoch.AddDays(3)),
                        new(DateTime.UnixEpoch, DateTime.UnixEpoch.AddDays(1)),
                    })),
                    It.IsAny<List<Filter>>(),
                    It.IsAny<CancellationToken>()),
                Times.Once());
        _machineSnapshotHttpClientMock.VerifyNoOtherCalls();
        results.Should().NotBeNull();
        results.Should().HaveCount(3);
        results.Should().AllSatisfy(result => result.Value.Should().NotBeNull());
    }

    [Fact]
    public async Task LoadBatchAsync_With_Multiple_Machines_Multiple_Columns_DifferentTimeRanges()
    {
        // Arrange
        var tasks = new List<Task<DataResult<IDictionary<DateTime, object?>>>>
        {
            _subject.LoadAsync(
                (MachineId1, new TimeRange(DateTime.UnixEpoch, DateTime.UnixEpoch.AddDays(1)), "Column1")),
            _subject.LoadAsync(
                (MachineId1, new TimeRange(DateTime.UnixEpoch.AddDays(2), DateTime.UnixEpoch.AddDays(3)), "Column2")),
            _subject.LoadAsync(
                (MachineId2, new TimeRange(DateTime.UnixEpoch, DateTime.UnixEpoch.AddDays(1)), "Column1")),
        };

        // Act
        _batchScheduler.BeginDispatch();
        var results = await Task.WhenAll(tasks);

        // Assert
        _machineSnapshotHttpClientMock
            .Verify(
                m => m.GetSnapshotsInTimeRanges(
                    MachineId1,
                    It.Is<List<string>>(list => list.SequenceEqual(new List<string> { "Column1", "Column2" })),
                    It.Is<List<TimeRange>>(list => list.SequenceEqual(new List<TimeRange>
                    {
                        new(DateTime.UnixEpoch, DateTime.UnixEpoch.AddDays(1)),
                        new(DateTime.UnixEpoch.AddDays(2), DateTime.UnixEpoch.AddDays(3)),
                    })),
                    It.IsAny<List<Filter>>(),
                    It.IsAny<CancellationToken>()),
                Times.Once());
        _machineSnapshotHttpClientMock
            .Verify(
                m => m.GetSnapshotsInTimeRanges(
                    MachineId2,
                    It.Is<List<string>>(list => list.SequenceEqual(new List<string> { "Column1" })),
                    It.Is<List<TimeRange>>(list => list.SequenceEqual(new List<TimeRange>
                    {
                        new(DateTime.UnixEpoch, DateTime.UnixEpoch.AddDays(1)),
                    })),
                    It.IsAny<List<Filter>>(),
                    It.IsAny<CancellationToken>()),
                Times.Once());
        _machineSnapshotHttpClientMock.VerifyNoOtherCalls();
        results.Should().NotBeNull();
        results.Should().HaveCount(3);
        results.Should().AllSatisfy(result => result.Value.Should().NotBeNull());
    }

    [Fact]
    public async Task LoadBatchAsync_With_Multiple_Machines_One_Machine_Throws()
    {
        // Arrange
        _machineSnapshotHttpClientMock
            .Setup(m => m.GetSnapshotsInTimeRanges(
                MachineId2,
                It.IsAny<List<string>>(),
                It.IsAny<List<TimeRange>>(),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((
                string machineId,
                List<string> columnIds,
                List<TimeRange> timeRanges,
                List<Filter> filters,
                CancellationToken cancellationToken) =>
            new InternalItemResponse<MachineSnapshotListResponse>(400, "Error"));

        var tasks = new List<Task<DataResult<IDictionary<DateTime, object?>>>>
        {
            _subject.LoadAsync(
                (MachineId1, new TimeRange(DateTime.UnixEpoch, DateTime.UnixEpoch.AddDays(1)), "Column1")),
            _subject.LoadAsync(
                (MachineId1, new TimeRange(DateTime.UnixEpoch.AddDays(2), DateTime.UnixEpoch.AddDays(3)), "Column2")),
            _subject.LoadAsync(
                (MachineId2, new TimeRange(DateTime.UnixEpoch, DateTime.UnixEpoch.AddDays(1)), "Column1")),
        };

        // Act
        _batchScheduler.BeginDispatch();
        var results = await Task.WhenAll(tasks);

        // Assert
        _machineSnapshotHttpClientMock
            .Verify(
                m => m.GetSnapshotsInTimeRanges(
                    MachineId1,
                    It.Is<List<string>>(list => list.SequenceEqual(new List<string> { "Column1", "Column2" })),
                    It.Is<List<TimeRange>>(list => list.SequenceEqual(new List<TimeRange>
                    {
                        new(DateTime.UnixEpoch, DateTime.UnixEpoch.AddDays(1)),
                        new(DateTime.UnixEpoch.AddDays(2), DateTime.UnixEpoch.AddDays(3)),
                    })),
                    It.IsAny<List<Filter>>(),
                    It.IsAny<CancellationToken>()),
                Times.Once());
        _machineSnapshotHttpClientMock
            .Verify(
                m => m.GetSnapshotsInTimeRanges(
                    MachineId2,
                    It.Is<List<string>>(list => list.SequenceEqual(new List<string> { "Column1" })),
                    It.Is<List<TimeRange>>(list => list.SequenceEqual(new List<TimeRange>
                    {
                        new(DateTime.UnixEpoch, DateTime.UnixEpoch.AddDays(1)),
                    })),
                    It.IsAny<List<Filter>>(),
                    It.IsAny<CancellationToken>()),
                Times.Once());
        _machineSnapshotHttpClientMock.VerifyNoOtherCalls();
        results.Should().NotBeNull();
        results.Should().HaveCount(3);
        results[0].Exception.Should().BeNull();
        results[1].Exception.Should().BeNull();
        results[2].Exception.Should().NotBeNull();
    }

}