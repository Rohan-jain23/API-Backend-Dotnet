using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FrameworkAPI.DataLoaders;
using FrameworkAPI.Models;
using FrameworkAPI.Schema.Misc;
using FrameworkAPI.Services.Interfaces;
using GreenDonut;
using Moq;
using WuH.Ruby.MachineSnapShooter.Client;
using Xunit;

namespace FrameworkAPI.Test.Schema.Misc;

public class SummedSnapshotValueTests
{

    private readonly Mock<SnapshotSumBatchDataLoader> _sumBatchDataLoaderMock = new(
        new Mock<IMachineSnapshotHttpClient>().Object,
        new Mock<IBatchScheduler>().Object,
        new DataLoaderOptions());

    private readonly Mock<LatestSnapshotCacheDataLoader> _latestSnapshotCacheDataLoader = new(
        new Mock<ILatestMachineSnapshotCachingService>().Object);
    private readonly Mock<IMachineSnapshotService> _machineSnapshotServiceMock = new();

    [Fact]
    public async Task SummedSnapshotValue_Returns_Value()
    {
        //Arrange
        var numericValue = new SummedSnapshotValue(
            "FakeColumnId",
            "FakeMachineId",
            [new TimeRange(DateTime.MinValue, DateTime.MinValue)]);
        MockGetSummedColumnValue(11.1, null);

        //Act
        var result = await numericValue.Value(
            _sumBatchDataLoaderMock.Object,
            _machineSnapshotServiceMock.Object,
            It.IsAny<CancellationToken>());

        //Assert
        result.Should().Be(11.1, null);
    }

    [Fact]
    public async Task SummedSnapshotValue_Returns_Unit()
    {
        //Arrange
        var numericValue = new SummedSnapshotValue(
            "FakeColumnId",
            "FakeMachineId",
            [new TimeRange(DateTime.MinValue, DateTime.MinValue)]);
        MockGetLatestColumnUnit("m", null);

        //Act
        var result = await numericValue.Unit(
            _latestSnapshotCacheDataLoader.Object,
            _machineSnapshotServiceMock.Object,
            It.IsAny<CancellationToken>());

        //Assert
        result.Should().Be("m", null);
    }

    [Fact]
    public async Task SummedSnapshotValue_Get_Unit_Throws_Exception()
    {
        //Arrange
        var numericValue = new SummedSnapshotValue(
            "FakeColumnId",
            "FakeMachineId",
            [new TimeRange(DateTime.MinValue, DateTime.MinValue)]);
        MockGetLatestColumnUnit(null, new Exception());

        //Act + Assert
        await Assert.ThrowsAsync<Exception>(async () => await numericValue.Unit(
            _latestSnapshotCacheDataLoader.Object,
            _machineSnapshotServiceMock.Object,
            It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task SummedSnapshotValue_Get_Value_Throws_Exception()
    {
        //Arrange
        var numericValue = new SummedSnapshotValue(
            "FakeColumnId",
            "FakeMachineId",
            [new TimeRange(DateTime.MinValue, DateTime.MinValue)]);
        MockGetSummedColumnValue(null, new Exception());

        //Act + Assert
        await Assert.ThrowsAsync<Exception>(async () => await numericValue.Value(
            _sumBatchDataLoaderMock.Object,
            _machineSnapshotServiceMock.Object,
            It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task SummedSnapshotValue_Returns_Null_When_TimeRange_Is_Null()
    {
        //Arrange
        var numericValue = new SummedSnapshotValue(
            "FakeColumnId",
            "FakeMachineId",
            null);

        //Act
        var result = await numericValue.Value(
            _sumBatchDataLoaderMock.Object,
            _machineSnapshotServiceMock.Object,
            It.IsAny<CancellationToken>());

        //Assert
        result.Should().BeNull();
    }

    private void MockGetSummedColumnValue(double? value, Exception? exception)
    {
        _machineSnapshotServiceMock
            .Setup(x => x.GetSum(
                _sumBatchDataLoaderMock.Object,
                "FakeMachineId",
                "FakeColumnId",
                It.IsAny<List<WuH.Ruby.Common.Core.TimeRange>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((
                    SnapshotSumBatchDataLoader _,
                    string columnId,
                    string machineId,
                    List<WuH.Ruby.Common.Core.TimeRange> _,
                    CancellationToken _)
                => new DataResult<double?>(value, exception));
    }

    private void MockGetLatestColumnUnit(string? value, Exception? exception)
    {
        _machineSnapshotServiceMock
            .Setup(x => x.GetLatestColumnUnit(
                _latestSnapshotCacheDataLoader.Object,
                "FakeColumnId",
                "FakeMachineId",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((
                    LatestSnapshotCacheDataLoader _,
                    string columnId,
                    string machineId,
                    CancellationToken _)
                => new DataResult<string>(value, exception));
    }

}