using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FrameworkAPI.DataLoaders;
using FrameworkAPI.Exceptions;
using FrameworkAPI.Models;
using FrameworkAPI.Schema.Misc;
using FrameworkAPI.Services.Interfaces;
using GreenDonut;
using Microsoft.Extensions.Logging;
using Moq;
using WuH.Ruby.MachineSnapShooter.Client;
using Xunit;

namespace FrameworkAPI.Test.Schema.Misc;

public class SnapshotValuesDuringProductionTests
{
    private readonly Mock<LatestSnapshotCacheDataLoader> _latestSnapshotCacheDataLoaderMock = new(new Mock<ILatestMachineSnapshotCachingService>().Object);
    private readonly Mock<SnapshotByTimestampBatchDataLoader> _snapshotByTimestampBatchDataLoaderMock = new(
        new Mock<IMachineSnapshotHttpClient>().Object,
        new Mock<IBatchScheduler>().Object,
        new Mock<ILogger<SnapshotByTimestampBatchDataLoader>>().Object,
        new DataLoaderOptions());
    private readonly Mock<IMachineSnapshotService> _machineSnapshotServiceMock = new();

    [Fact]
    public async Task ValueAtQueryTimestamp_Returns_Latest_Snapshot_When_QueryTimestamp_Is_Null_And_The_Job_Is_Active()
    {
        // Arrange
        var value = new SnapshotValuesDuringProduction<string>(
            "FakeColumnId",
            endTime: null,
            machineId: "FakeMachineId",
            timeRanges: null,
            machineQueryTimestamp: null);
        MockGetLatestColumnValue("Hello World!");

        // Act
        var valueAtQueryTimestamp = await value.ValueAtQueryTimestamp(
            _latestSnapshotCacheDataLoaderMock.Object,
            _snapshotByTimestampBatchDataLoaderMock.Object,
            _machineSnapshotServiceMock.Object,
            It.IsAny<CancellationToken>());

        // Assert
        valueAtQueryTimestamp.Should().Be("Hello World!");

    }

    [Fact]
    public async Task ValueAtQueryTimestamp_Returns_Null_When_QueryTimestamp_Is_Null_And_The_Job_Is_Not_Active()
    {
        // Arrange
        var value = new SnapshotValuesDuringProduction<bool?>(
            "FakeColumnId",
            endTime: DateTime.UnixEpoch,
            machineId: "FakeMachineId",
            timeRanges: null,
            machineQueryTimestamp: null);
        MockGetLatestColumnValue(true);

        // Act
        var valueAtQueryTimestamp = await value.ValueAtQueryTimestamp(
            _latestSnapshotCacheDataLoaderMock.Object,
            _snapshotByTimestampBatchDataLoaderMock.Object,
            _machineSnapshotServiceMock.Object,
            It.IsAny<CancellationToken>());

        // Assert
        valueAtQueryTimestamp.Should().Be(null);
    }

    [Fact]
    public async Task ValueAtQueryTimestamp_Returns_Value_For_Timestamp_When_QueryTimestamp_Is_Set()
    {
        // Arrange
        var value = new SnapshotValuesDuringProduction<int>(
            "FakeColumnId",
            endTime: null,
            machineId: "FakeMachineId",
            timeRanges: null,
            machineQueryTimestamp: DateTime.UnixEpoch);
        MockGetColumnValue(22);

        // Act
        var valueAtQueryTimestamp = await value.ValueAtQueryTimestamp(
            _latestSnapshotCacheDataLoaderMock.Object,
            _snapshotByTimestampBatchDataLoaderMock.Object,
            _machineSnapshotServiceMock.Object,
            It.IsAny<CancellationToken>());

        // Assert
        valueAtQueryTimestamp.Should().Be(22);
    }

    [Fact]
    public async Task ValueAtQueryTimestamp_Throws_When_Exception_Is_Returned()
    {
        // Arrange
        var value = new SnapshotValuesDuringProduction<string>(
            "FakeColumnId",
            endTime: null,
            machineId: "FakeMachineId",
            timeRanges: null,
            machineQueryTimestamp: DateTime.UnixEpoch);

        _machineSnapshotServiceMock
            .Setup(x => x.GetColumnValue(
                _snapshotByTimestampBatchDataLoaderMock.Object,
                "FakeColumnId",
                It.IsAny<DateTime>(),
                "FakeMachineId",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DataResult<SnapshotValue>(null, new InternalServiceException()));

        // Act
        var action = () => value.ValueAtQueryTimestamp(
            _latestSnapshotCacheDataLoaderMock.Object,
            _snapshotByTimestampBatchDataLoaderMock.Object,
            _machineSnapshotServiceMock.Object,
            It.IsAny<CancellationToken>());

        // Assert
        await action.Should().ThrowAsync<InternalServiceException>();
    }

    private void MockGetLatestColumnValue(object? value)
    {
        _machineSnapshotServiceMock
            .Setup(x => x.GetLatestColumnValue(
                _latestSnapshotCacheDataLoaderMock.Object,
                "FakeColumnId",
                "FakeMachineId",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((
                LatestSnapshotCacheDataLoader _,
                string columnId,
                string machineId,
                CancellationToken _)
                    => new DataResult<SnapshotValue>(new SnapshotValue(columnId, value), null));
    }

    private void MockGetColumnValue(object? value)
    {
        _machineSnapshotServiceMock
            .Setup(x => x.GetColumnValue(
                _snapshotByTimestampBatchDataLoaderMock.Object,
                "FakeColumnId",
                It.IsAny<DateTime>(),
                "FakeMachineId",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((
                SnapshotByTimestampBatchDataLoader _,
                string columnId,
                DateTime timestamp,
                string machineId,
                CancellationToken _)
                    => new DataResult<SnapshotValue>(new SnapshotValue(columnId, value), null));
    }
}