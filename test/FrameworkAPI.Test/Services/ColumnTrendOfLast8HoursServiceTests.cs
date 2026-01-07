using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FrameworkAPI.DataLoaders;
using FrameworkAPI.Models;
using FrameworkAPI.Schema.Misc;
using FrameworkAPI.Services;
using FrameworkAPI.Services.Interfaces;
using GreenDonut;
using Moq;
using WuH.Ruby.MachineSnapShooter.Client;
using Xunit;

namespace FrameworkAPI.Test.Services;

public class ColumnTrendOfLast8HoursServiceTests
{
    private const string MachineId = "EQ00001";

    private readonly int _trendTimeSpanInMinutes = (int)Constants.MachineTrend.TrendTimeSpan.TotalMinutes;

    private readonly Mock<IMachineSnapshotHttpClient> _machineSnapshotHttpClientMock = new();
    private readonly Mock<IMachineTrendCachingService> _machineTrendCachingServiceMock = new();
    private readonly Mock<IMachineTimeService> _machineTimeServiceMock = new();
    private readonly Mock<IMachineSnapshotService> _machineSnapshotServiceMock = new();
    private readonly ColumnTrendOfLast8HoursService _subject;

    public static TheoryData<DateTime?> GetLatestAndGetTestData => new()
    {
        null,
        new DateTime(year: 2023, month: 1, day: 15, hour: 0, minute: 0, second: 0, DateTimeKind.Utc)
    };

    public ColumnTrendOfLast8HoursServiceTests()
    {
        _subject = new ColumnTrendOfLast8HoursService(_machineTimeServiceMock.Object, _machineSnapshotServiceMock.Object);
    }

    [Theory]
    [MemberData(nameof(GetLatestAndGetTestData))]
    public async Task GetLatest_And_Get_Should_Not_Modify_Column_Trend_When_Column_Trend_Is_Not_Empty_And_Has_No_Gaps(
        DateTime? endTime)
    {
        // Arrange
        var columnTrendAsDictionary = new SortedDictionary<DateTime, double?>();
        var machineTime = DateTime.UtcNow;

        // Reset seconds and milliseconds
        var roundedMachineTime = new DateTime(
            machineTime.Year,
            machineTime.Month,
            machineTime.Day,
            machineTime.Hour,
            machineTime.Minute,
            second: 0,
            DateTimeKind.Utc);

        // Fill the whole trend with null values
        for (var i = 0; i < _trendTimeSpanInMinutes; i++)
        {
            columnTrendAsDictionary.Add(roundedMachineTime.Subtract(TimeSpan.FromMinutes(i)), value: i);
        }

        // Arrange / Act / Assert
        var columnTrendOrException = await MockAndCallGetLatestOrGet(endTime, machineTime, columnTrendAsDictionary);

        // Assert
        var expectedColumnTrend = columnTrendAsDictionary.Select(kvp => new NumericTrendElement(kvp.Key, kvp.Value));

        var columnTrend = columnTrendOrException.Value!.ToList();
        columnTrend.Should().BeEquivalentTo(expectedColumnTrend);
    }

    [Theory]
    [MemberData(nameof(GetLatestAndGetTestData))]
    public async Task GetLatest_And_Get_Should_Fill_Gaps_Correctly_When_Column_Trend_Is_Empty(DateTime? endTime)
    {
        // Arrange
        var columnTrendAsDictionary = new SortedDictionary<DateTime, double?>();
        var machineTime = DateTime.UtcNow;

        // Reset seconds and milliseconds
        var roundedMachineTime = new DateTime(
            machineTime.Year,
            machineTime.Month,
            machineTime.Day,
            machineTime.Hour,
            machineTime.Minute,
            second: 0,
            DateTimeKind.Utc);

        // Arrange / Act / Assert
        var columnTrendOrException = await MockAndCallGetLatestOrGet(endTime, machineTime, columnTrendAsDictionary);

        // Assert
        var columnTrend = columnTrendOrException.Value!.ToList();
        columnTrend.Should().HaveCount(_trendTimeSpanInMinutes);

        for (var i = 0; i < _trendTimeSpanInMinutes; i++)
        {
            var columnTrendElement1 = columnTrend.ElementAt(i);
            columnTrendElement1.Value.Should().BeNull();

            if (i == _trendTimeSpanInMinutes - 1)
            {
                columnTrendElement1.Time.Should().Be(endTime ?? roundedMachineTime);
                continue;
            }

            var columnTrendElement2 = columnTrend.ElementAt(i + 1);
            columnTrendElement2.Time.Should().Be(columnTrendElement1.Time.AddMinutes(1));
        }
    }

    [Theory]
    [MemberData(nameof(GetLatestAndGetTestData))]
    public async Task GetLatest_And_Get_Should_Fill_Gaps_Correctly_When_Column_Trend_Is_Not_Empty_And_Has_Gaps(
        DateTime? endTime)
    {
        // Arrange
        var columnTrendAsDictionary = new SortedDictionary<DateTime, double?>();
        var machineTime = DateTime.UtcNow;

        // Reset seconds and milliseconds
        var roundedMachineTime = new DateTime(
            machineTime.Year,
            machineTime.Month,
            machineTime.Day,
            machineTime.Hour,
            machineTime.Minute,
            second: 0,
            DateTimeKind.Utc);

        // Fill half of the trend with non-null values
        for (var i = 0; i < _trendTimeSpanInMinutes / 2; i++)
        {
            columnTrendAsDictionary.Add(roundedMachineTime.Subtract(TimeSpan.FromMinutes(i)), value: i);
        }

        // Arrange / Act / Assert
        var columnTrendOrException = await MockAndCallGetLatestOrGet(endTime, machineTime, columnTrendAsDictionary);

        // Assert
        var columnTrend = columnTrendOrException.Value!.ToList();
        columnTrend.Should().HaveCount(_trendTimeSpanInMinutes);

        for (var i = 0; i < _trendTimeSpanInMinutes; i++)
        {
            var columnTrendElement1 = columnTrend.ElementAt(i);

            if (i <= _trendTimeSpanInMinutes / 2)
            {
                // The first half of the trend should contain null values
                columnTrendElement1.Value.Should().BeNull();
            }
            else
            {
                // The last half of the trend should contain non-null values
                columnTrendElement1.Value.Should().NotBeNull();
            }

            if (i == _trendTimeSpanInMinutes - 1)
            {
                columnTrendElement1.Time.Should().Be(roundedMachineTime);
                continue;
            }

            var columnTrendElement2 = columnTrend.ElementAt(i + 1);
            columnTrendElement2.Time.Should().Be(columnTrendElement1.Time.AddMinutes(1));
        }
    }

    [Theory]
    [MemberData(nameof(GetLatestAndGetTestData))]
    public async Task GetLatest_Should_Return_Null_DataResult_When_GetLatestColumnTrend_Returns_Null(
        DateTime? endTime)
    {
        // Arrange
        var machineTime = DateTime.UtcNow;

        // Arrange / Act / Assert
        var columnTrendOrException = await MockAndCallGetLatestOrGet(endTime, machineTime, columnTrend: null);

        // Assert
        columnTrendOrException.Value.Should().BeNull();
        columnTrendOrException.Exception.Should().BeNull();
    }

    private async Task<DataResult<IEnumerable<NumericTrendElement>>> MockAndCallGetLatestOrGet(
        DateTime? endTime, DateTime? machineTime, SortedDictionary<DateTime, double?>? columnTrend)
    {
        // Arrange
        const string columnId = SnapshotColumnIds.ExtrusionThroughput;

        DataResult<IEnumerable<NumericTrendElement>> columnTrendOrException;

        if (endTime is null)
        {
            var latestMachineTrendCacheDataLoader =
                new LatestMachineTrendCacheDataLoader(_machineTrendCachingServiceMock.Object);

            _machineSnapshotServiceMock
                .Setup(m => m.GetLatestColumnTrend(
                    latestMachineTrendCacheDataLoader, columnId, MachineId, CancellationToken.None))
                .ReturnsAsync(new DataResult<SortedDictionary<DateTime, double?>>(columnTrend, exception: null));

            if (columnTrend is not null && !columnTrend.Any())
            {
                _machineTimeServiceMock
                    .Setup(m => m.Get(MachineId, CancellationToken.None))
                    .ReturnsAsync(new DataResult<DateTime?>(machineTime, exception: null));
            }

            // Act
            columnTrendOrException =
                await _subject.GetLatest(latestMachineTrendCacheDataLoader, columnId, MachineId,
                    CancellationToken.None);
        }
        else
        {
            var machineTrendByTimeSpanBatchDataLoader = new MachineTrendByTimeRangeBatchDataLoader(
                _machineSnapshotHttpClientMock.Object, new Mock<IBatchScheduler>().Object);

            _machineSnapshotServiceMock
                .Setup(m => m.GetNumericColumnTrendOfLast8Hours(
                    machineTrendByTimeSpanBatchDataLoader, columnId, endTime.Value, MachineId, CancellationToken.None))
                .ReturnsAsync(new DataResult<SortedDictionary<DateTime, double?>>(columnTrend, exception: null));

            // Act
            columnTrendOrException = await _subject.Get(
                machineTrendByTimeSpanBatchDataLoader, columnId, endTime.Value, MachineId, CancellationToken.None);
        }

        // Assert
        _machineSnapshotServiceMock.VerifyAll();
        _machineSnapshotServiceMock.VerifyNoOtherCalls();

        _machineTimeServiceMock.VerifyAll();
        _machineTimeServiceMock.VerifyNoOtherCalls();

        return columnTrendOrException;
    }
}