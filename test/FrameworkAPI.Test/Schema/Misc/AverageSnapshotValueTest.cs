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

public class AverageSnapshotValueTest
{
    private readonly Mock<SnapshotArithmeticMeanBatchDataLoader> _arithmeticMeanBatchDataLoader = new(
        new Mock<IMachineSnapshotHttpClient>().Object,
        new Mock<IBatchScheduler>().Object,
        new DataLoaderOptions());

    private readonly Mock<LatestSnapshotCacheDataLoader> _latestSnapshotCacheDataLoader = new(
        new Mock<ILatestMachineSnapshotCachingService>().Object);
    private readonly Mock<IMachineSnapshotService> _machineSnapshotServiceMock = new();
    private readonly Mock<IMachineTimeService> _machineTimeServiceMock = new();

    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    [Fact]
    public async Task AverageSnapshotValue_Returns_Value()
    {
        //Arrange
        var numericValue = new AverageSnapshotValue(
            "FakeColumnId",
            "FakeMachineId",
            DateTime.MinValue, DateTime.MinValue);
        MockGetAverageColumnValue(11.1, null);
        MockMachineTime("FakeMachineId", DateTime.MinValue);

        //Act
        var result = await numericValue.Value(
            _arithmeticMeanBatchDataLoader.Object,
            _machineSnapshotServiceMock.Object,
            _machineTimeServiceMock.Object,
            It.IsAny<CancellationToken>());

        //Assert
        result.Should().Be(11.1);
    }

    [Fact]
    public async Task AverageSnapshotValue_Returns_Unit()
    {
        //Arrange
        var numericValue = new AverageSnapshotValue(
            "FakeColumnId",
            "FakeMachineId",
            DateTime.MinValue, DateTime.MinValue);
        MockGetLatestColumnUnit("m", null);

        //Act
        var result = await numericValue.Unit(
            _latestSnapshotCacheDataLoader.Object,
            _machineSnapshotServiceMock.Object,
            It.IsAny<CancellationToken>());

        //Assert
        result.Should().Be("m");
    }

    [Fact]
    public async Task AverageSnapshotValue_Get_Unit_Throws_Exception()
    {
        //Arrange
        var numericValue = new AverageSnapshotValue(
            "FakeColumnId",
            "FakeMachineId",
            DateTime.MinValue, DateTime.MinValue);
        MockGetLatestColumnUnit(null, new Exception());

        //Act + Assert
        await Assert.ThrowsAsync<Exception>(async () => await numericValue.Unit(
            _latestSnapshotCacheDataLoader.Object,
            _machineSnapshotServiceMock.Object,
            It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task AverageSnapshotValue_Get_Value_Throws_Exception()
    {
        //Arrange
        var numericValue = new AverageSnapshotValue(
            "FakeColumnId",
            "FakeMachineId",
            DateTime.MinValue, DateTime.MinValue.AddDays(2));
        MockGetAverageColumnValue(null, new Exception());

        //Act + Assert
        await Assert.ThrowsAsync<Exception>(async () => await numericValue.Value(
            _arithmeticMeanBatchDataLoader.Object,
            _machineSnapshotServiceMock.Object,
            _machineTimeServiceMock.Object,
            It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task AverageSnapshotValue_Returns_Value_When_EndDate_Is_Null()
    {
        //Arrange
        var numericValue = new AverageSnapshotValue(
            "FakeColumnId",
            "FakeMachineId",
            DateTime.MinValue,
            null);
        MockGetAverageColumnValue(11.1, null);
        MockMachineTime("FakeMachineId", DateTime.MinValue);

        //Act
        var result = await numericValue.Value(
            _arithmeticMeanBatchDataLoader.Object,
            _machineSnapshotServiceMock.Object,
            _machineTimeServiceMock.Object,
            It.IsAny<CancellationToken>());

        //Assert
        result.Should().Be(11.1);
    }

    private void MockGetAverageColumnValue(double? value, Exception? exception)
    {
        _machineSnapshotServiceMock
            .Setup(x => x.GetArithmeticMean(
                _arithmeticMeanBatchDataLoader.Object,
                "FakeMachineId",
                "FakeColumnId",
                It.IsAny<List<WuH.Ruby.Common.Core.TimeRange>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((
                    SnapshotArithmeticMeanBatchDataLoader _,
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

    private void MockMachineTime(string machineId, DateTime dateTime)
    {
        _machineTimeServiceMock
            .Setup(m => m.Get(machineId, _cancellationToken))
            .ReturnsAsync(new DataResult<DateTime?>(dateTime, exception: null));
    }
}