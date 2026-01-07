using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FrameworkAPI.DataLoaders;
using FrameworkAPI.Exceptions;
using FrameworkAPI.Extensions;
using FrameworkAPI.Models;
using FrameworkAPI.Services;
using FrameworkAPI.Test.Services.Helpers;
using Microsoft.Extensions.Logging;
using Moq;
using WuH.Ruby.Common.Core;
using WuH.Ruby.MachineSnapShooter.Client;
using WuH.Ruby.MachineSnapShooter.Client.Enums;
using WuH.Ruby.MachineSnapShooter.Client.Models;
using Xunit;

namespace FrameworkAPI.Test.Services;

public class MachineSnapshotServiceTests
{
    private const string ColumnId = "Speed";
    private const string ColumnUnit = "km/h";
    private const string MachineId = "EQ00001";
    private const int Limit = 100;
    private readonly DateTime _timestampMinValue = DateTime.MinValue;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly ValueByColumnId<double?> _arbitraryNumericValueByColumnId = new() { { ColumnId, 50 } };
    private readonly ValueByColumnId<double?> _emptyArbitraryNumericValueByColumnId = new() { { ColumnId, null } };
    private static readonly WuH.Ruby.Common.Core.TimeRange ArbitraryTimeRange = new(new DateTime(2002, 5, 10), new DateTime(2002, 5, 10));
    private readonly List<WuH.Ruby.Common.Core.TimeRange> _arbitraryTimeRanges = [ArbitraryTimeRange];
    private readonly List<TimeRange> _arbitraryFrameworkApiTimeRanges = [ArbitraryTimeRange];
    private readonly GroupAssignment _groupAssignment = new("keyColumnId", "valueColumnId");
    private readonly ValueByColumnId<GroupedSumByIdentifier> _arbitraryGroupedSums = new() { { "keyColumnId", new GroupedSumByIdentifier { { "Abc", 50.0 } } } };
    private readonly ValueByColumnId<GroupedSumByIdentifier> _emptyArbitraryGroupedSums = new() { { "keyColumnId", new GroupedSumByIdentifier() } };
    private readonly MachineSnapshotService _subject = new();
    private readonly SnapshotByTimestampBatchDataLoader _snapshotByTimestampBatchDataLoader;
    private readonly LatestSnapshotCacheDataLoader _latestSnapshotCacheDataLoader;
    private readonly SnapshotValuesWithLongestDurationBatchDataLoader _valuesWithLongestDurationBatchDataLoader;
    private readonly SnapshotDistinctValuesBatchDataLoader _distinctValuesBatchLoader;
    private readonly MachineTrendByTimeRangeBatchDataLoader _machineTrendByTimeRangeBatchDataLoader;
    private readonly SnapshotMinBatchDataLoader _minBatchDataLoader;
    private readonly SnapshotMaxBatchDataLoader _maxBatchDataLoader;
    private readonly SnapshotSumBatchDataLoader _sumBatchDataLoader;
    private readonly SnapshotGroupedSumBatchDataLoader _groupedSumBatchDataLoader;
    private readonly SnapshotArithmeticMeanBatchDataLoader _arithmeticMeanBatchDataLoader;
    private readonly SnapshotMedianValuesBatchDataLoader _medianValuesBatchDataLoader;
    private readonly SnapshotStandardDeviationBatchDataLoader _standardDeviationBatchDataLoader;
    private readonly Mock<IMachineSnapshotHttpClient> _machineSnapshotsHttpClientMock = new();
    private readonly Mock<ILatestMachineSnapshotCachingService> _latestMachineSnapshotCachingServiceMock = new();

    public MachineSnapshotServiceTests()
    {
        _snapshotByTimestampBatchDataLoader = new SnapshotByTimestampBatchDataLoader(
            _machineSnapshotsHttpClientMock.Object,
            new DelayedBatchScheduler(),
            new Mock<ILogger<SnapshotByTimestampBatchDataLoader>>().Object);
        _minBatchDataLoader = new SnapshotMinBatchDataLoader(
            _machineSnapshotsHttpClientMock.Object,
            new DelayedBatchScheduler());
        _maxBatchDataLoader = new SnapshotMaxBatchDataLoader(
            _machineSnapshotsHttpClientMock.Object,
            new DelayedBatchScheduler());
        _sumBatchDataLoader = new SnapshotSumBatchDataLoader(
            _machineSnapshotsHttpClientMock.Object,
            new DelayedBatchScheduler());
        _groupedSumBatchDataLoader = new SnapshotGroupedSumBatchDataLoader(
            _machineSnapshotsHttpClientMock.Object,
            new DelayedBatchScheduler());
        _valuesWithLongestDurationBatchDataLoader = new SnapshotValuesWithLongestDurationBatchDataLoader(
            _machineSnapshotsHttpClientMock.Object,
            new DelayedBatchScheduler());
        _distinctValuesBatchLoader = new SnapshotDistinctValuesBatchDataLoader(
            _machineSnapshotsHttpClientMock.Object,
            new DelayedBatchScheduler());
        _arithmeticMeanBatchDataLoader = new SnapshotArithmeticMeanBatchDataLoader(
            _machineSnapshotsHttpClientMock.Object,
            new DelayedBatchScheduler());
        _latestSnapshotCacheDataLoader =
            new LatestSnapshotCacheDataLoader(_latestMachineSnapshotCachingServiceMock.Object);
        _medianValuesBatchDataLoader = new SnapshotMedianValuesBatchDataLoader(
            _machineSnapshotsHttpClientMock.Object,
            new DelayedBatchScheduler()
        );
        _standardDeviationBatchDataLoader = new SnapshotStandardDeviationBatchDataLoader(
            _machineSnapshotsHttpClientMock.Object,
            new DelayedBatchScheduler());
        _machineTrendByTimeRangeBatchDataLoader = new MachineTrendByTimeRangeBatchDataLoader(
            _machineSnapshotsHttpClientMock.Object,
            new DelayedBatchScheduler());
    }

    [Fact]
    public async Task GetLatestColumnValue_Returns_DataResult_With_Correct_Value()
    {
        // Arrange
        _latestMachineSnapshotCachingServiceMock
            .Setup(m => m.GetLatestMachineSnapshot(MachineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<MachineSnapshotResponse>(GenerateMachineSnapshotResponse()))
            .Verifiable(Times.Once);

        // Act
        var (latestColumnValue, exception) = await _subject.GetLatestColumnValue(
            _latestSnapshotCacheDataLoader, ColumnId, MachineId, CancellationToken.None);

        // Assert
        latestColumnValue.Should().BeEquivalentTo(new SnapshotValue(ColumnId, columnValue: 100));
        exception.Should().BeNull();

        _latestMachineSnapshotCachingServiceMock.VerifyAll();
        _latestMachineSnapshotCachingServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetLatestColumnValue_Returns_DataResult_With_Exception_If_GetLatestMachineSnapshot_Fails()
    {
        // Arrange
        _latestMachineSnapshotCachingServiceMock
            .Setup(m => m.GetLatestMachineSnapshot(MachineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<MachineSnapshotResponse>(
                (int)HttpStatusCode.InternalServerError, "ErrorMessage"))
            .Verifiable(Times.Once);

        // Act
        var (latestColumnValue, exception) = await _subject.GetLatestColumnValue(
            _latestSnapshotCacheDataLoader, ColumnId, MachineId, CancellationToken.None);

        // Assert
        latestColumnValue.Should().BeNull();
        exception.Should().BeOfType<InternalServiceException>();
        exception!.Message.Should().Be("ErrorMessage");

        _latestMachineSnapshotCachingServiceMock.VerifyAll();
        _latestMachineSnapshotCachingServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetLatestColumnValue_Returns_DataResult_With_Null_If_No_Exception_And_No_Value_Provided()
    {
        // Arrange
        _latestMachineSnapshotCachingServiceMock
            .Setup(m => m.GetLatestMachineSnapshot(MachineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<MachineSnapshotResponse>(
                new InternalError(
                    (int)HttpStatusCode.InternalServerError,
                    MachineSnapshotErrorItemType.WaitingForFirstMinutelySnapshot.ToString(),
                    MachineSnapshotErrorItemType.WaitingForFirstMinutelySnapshot)))
            .Verifiable(Times.Once);

        // Act
        var (latestColumnValue, exception) = await _subject.GetLatestColumnValue(
            _latestSnapshotCacheDataLoader, ColumnId, MachineId, CancellationToken.None);

        // Assert
        latestColumnValue!.ColumnId.Should().Be(ColumnId);
        latestColumnValue.ColumnValue.Should().BeNull();
        exception.Should().BeNull();

        _latestMachineSnapshotCachingServiceMock.VerifyAll();
        _latestMachineSnapshotCachingServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetLatestColumnUnit_Returns_DataResult_With_Correct_Value()
    {
        // Arrange
        _latestMachineSnapshotCachingServiceMock
            .Setup(m => m.GetLatestMachineSnapshot(MachineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<MachineSnapshotResponse>(GenerateMachineSnapshotResponse()))
            .Verifiable(Times.Once);

        // Act
        var (latestColumnUnit, exception) = await _subject.GetLatestColumnUnit(
            _latestSnapshotCacheDataLoader, ColumnId, MachineId, CancellationToken.None);

        // Assert
        latestColumnUnit.Should().Be(ColumnUnit);
        exception.Should().BeNull();

        _latestMachineSnapshotCachingServiceMock.VerifyAll();
        _latestMachineSnapshotCachingServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetLatestColumnUnit_Returns_DataResult_With_Null_If_No_Exception_And_No_Value_Provided()
    {
        // Arrange
        _latestMachineSnapshotCachingServiceMock
            .Setup(m => m.GetLatestMachineSnapshot(MachineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<MachineSnapshotResponse>(
                new InternalError(
                    (int)HttpStatusCode.InternalServerError,
                    MachineSnapshotErrorItemType.WaitingForFirstMinutelySnapshot.ToString(),
                    MachineSnapshotErrorItemType.WaitingForFirstMinutelySnapshot)))
            .Verifiable(Times.Once);

        // Act
        var (latestColumnUnit, exception) = await _subject.GetLatestColumnUnit(
            _latestSnapshotCacheDataLoader, ColumnId, MachineId, CancellationToken.None);

        // Assert
        latestColumnUnit.Should().BeNull();
        exception.Should().BeNull();

        _latestMachineSnapshotCachingServiceMock.VerifyAll();
        _latestMachineSnapshotCachingServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetLatestColumnUnit_Returns_DataResult_With_Exception_If_GetLatestColumnUnit_Fails()
    {
        // Arrange
        _latestMachineSnapshotCachingServiceMock
            .Setup(m => m.GetLatestMachineSnapshot(MachineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<MachineSnapshotResponse>(
                (int)HttpStatusCode.InternalServerError, "ErrorMessage"))
            .Verifiable(Times.Once);

        // Act
        var (latestColumnUnit, exception) = await _subject.GetLatestColumnUnit(
            _latestSnapshotCacheDataLoader, ColumnId, MachineId, CancellationToken.None);

        // Assert
        latestColumnUnit.Should().BeNull();
        exception.Should().BeOfType<InternalServiceException>();
        exception!.Message.Should().Be("ErrorMessage");

        _latestMachineSnapshotCachingServiceMock.VerifyAll();
        _latestMachineSnapshotCachingServiceMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(null)]
    [InlineData(50)]
    public async Task GetValueWithLongestDuration_Returns_DataResult_With_Correct_Values(object? value)
    {
        // Arrange
        var mockedValuesWithLongestDuration = new ValueByColumnId<object?> { { ColumnId, value } };

        _machineSnapshotsHttpClientMock
            .Setup(m => m.GetValuesWithLongestDuration(
                MachineId, new List<string> { ColumnId }, _arbitraryTimeRanges, It.IsAny<List<Filter>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<ValueByColumnId<object?>>(mockedValuesWithLongestDuration))
            .Verifiable(Times.Once);

        // Act
        var (valueWithLongestDuration, exception) = await _subject.GetValueWithLongestDuration(
            _valuesWithLongestDurationBatchDataLoader, MachineId, ColumnId, _arbitraryFrameworkApiTimeRanges, CancellationToken.None);

        // Assert
        valueWithLongestDuration.Should().BeEquivalentTo(mockedValuesWithLongestDuration.Single().Value);
        exception.Should().BeNull();

        _machineSnapshotsHttpClientMock.VerifyAll();
        _machineSnapshotsHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task
        GetValueWithLongestDuration_Returns_DataResult_With_Exception_If_GetValuesWithLongestDuration_Fails()
    {
        // Arrange
        _machineSnapshotsHttpClientMock
            .Setup(m => m.GetValuesWithLongestDuration(
                MachineId,
                new List<string> { ColumnId },
                _arbitraryTimeRanges,
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<ValueByColumnId<object?>>(
                (int)HttpStatusCode.InternalServerError, "ErrorMessage"))
            .Verifiable(Times.Once);

        // Act
        var (valueWithLongestDuration, exception) = await _subject.GetValueWithLongestDuration(
            _valuesWithLongestDurationBatchDataLoader, MachineId, ColumnId, _arbitraryFrameworkApiTimeRanges, CancellationToken.None);

        // Assert
        exception!.Message.Should().Be("ErrorMessage");
        valueWithLongestDuration.Should().BeNull();
    }

    [Fact]
    public async Task GetMin_Returning_The_Correct_Value()
    {
        // Arrange
        _machineSnapshotsHttpClientMock.Setup(m => m.GetMinValues(
            It.IsAny<string>(),
            It.IsAny<List<string>>(),
            It.IsAny<List<WuH.Ruby.Common.Core.TimeRange>>(),
            It.IsAny<List<Filter>>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(new InternalItemResponse<ValueByColumnId<double?>>(_arbitraryNumericValueByColumnId));

        // Act‚‚
        var result = await _subject.GetMin(_minBatchDataLoader, MachineId, ColumnId, _arbitraryFrameworkApiTimeRanges, _cancellationTokenSource.Token);

        // Assert
        result.Exception.Should().BeNull();
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task GetMin_Gets_Empty_List_From_HttpClient_GetMinValues()
    {
        // Arrange
        _machineSnapshotsHttpClientMock.Setup(m => m.GetMinValues(
            It.IsAny<string>(),
            It.IsAny<List<string>>(),
            It.IsAny<List<WuH.Ruby.Common.Core.TimeRange>>(),
            It.IsAny<List<Filter>>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(new InternalItemResponse<ValueByColumnId<double?>>(_emptyArbitraryNumericValueByColumnId));

        // Act
        var result = await _subject.GetMin(_minBatchDataLoader, MachineId, ColumnId, _arbitraryFrameworkApiTimeRanges, _cancellationTokenSource.Token);

        // Assert
        result.Exception.Should().BeNull();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task GetMin_Gets_MultipleMachines()
    {
        // Arrange
        _machineSnapshotsHttpClientMock.Setup(m => m.GetMinValues(
            It.IsAny<string>(),
            It.IsAny<List<string>>(),
            It.IsAny<List<WuH.Ruby.Common.Core.TimeRange>>(),
            It.IsAny<List<Filter>>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(new InternalItemResponse<ValueByColumnId<double?>>(_emptyArbitraryNumericValueByColumnId));

        // Act
        var result = await _subject.GetMin(_minBatchDataLoader, MachineId, ColumnId, _arbitraryFrameworkApiTimeRanges, _cancellationTokenSource.Token);

        // Assert
        result.Exception.Should().BeNull();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task GetMax_Returning_The_Correct_Value()
    {
        // Arrange
        _machineSnapshotsHttpClientMock.Setup(m => m.GetMaxValues(
            It.IsAny<string>(),
            It.IsAny<List<string>>(),
            It.IsAny<List<WuH.Ruby.Common.Core.TimeRange>>(),
            It.IsAny<List<Filter>>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(new InternalItemResponse<ValueByColumnId<double?>>(_arbitraryNumericValueByColumnId));

        // Act‚‚
        var result = await _subject.GetMax(_maxBatchDataLoader, MachineId, ColumnId, _arbitraryFrameworkApiTimeRanges, _cancellationTokenSource.Token);

        // Assert
        result.Exception.Should().BeNull();
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task GetMax_Gets_Empty_List_From_HttpClient_GetMaxValues()
    {
        // Arrange
        _machineSnapshotsHttpClientMock.Setup(m => m.GetMaxValues(
            It.IsAny<string>(),
            It.IsAny<List<string>>(),
            It.IsAny<List<WuH.Ruby.Common.Core.TimeRange>>(),
            It.IsAny<List<Filter>>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(new InternalItemResponse<ValueByColumnId<double?>>(_emptyArbitraryNumericValueByColumnId));

        // Act
        var result = await _subject.GetMax(_maxBatchDataLoader, MachineId, ColumnId, _arbitraryFrameworkApiTimeRanges, _cancellationTokenSource.Token);

        // Assert
        result.Exception.Should().BeNull();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task GetMax_Gets_MultipleMachines()
    {
        // Arrange
        _machineSnapshotsHttpClientMock.Setup(m => m.GetMaxValues(
            It.IsAny<string>(),
            It.IsAny<List<string>>(),
            It.IsAny<List<WuH.Ruby.Common.Core.TimeRange>>(),
            It.IsAny<List<Filter>>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(new InternalItemResponse<ValueByColumnId<double?>>(_emptyArbitraryNumericValueByColumnId));

        // Act
        await _subject.GetMax(_maxBatchDataLoader, MachineId, ColumnId, _arbitraryFrameworkApiTimeRanges, _cancellationTokenSource.Token);

        // Assert
        _machineSnapshotsHttpClientMock.VerifyAll();
        _machineSnapshotsHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetSum_Returning_The_Correct_Value()
    {
        // Arrange
        _machineSnapshotsHttpClientMock.Setup(m => m.GetSumValues(
            It.IsAny<string>(),
            It.IsAny<List<string>>(),
            It.IsAny<List<WuH.Ruby.Common.Core.TimeRange>>(),
            It.IsAny<List<Filter>>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(new InternalItemResponse<ValueByColumnId<double?>>(_arbitraryNumericValueByColumnId));

        // Act
        var result = await _subject.GetSum(_sumBatchDataLoader, MachineId, ColumnId, _arbitraryFrameworkApiTimeRanges, _cancellationTokenSource.Token);

        // Assert
        result.Exception.Should().BeNull();
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task GetSum_Gets_Empty_List_From_HttpClient_GetSumValues()
    {
        // Arrange
        _machineSnapshotsHttpClientMock.Setup(m => m.GetSumValues(
            It.IsAny<string>(),
            It.IsAny<List<string>>(),
            It.IsAny<List<WuH.Ruby.Common.Core.TimeRange>>(),
            It.IsAny<List<Filter>>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(new InternalItemResponse<ValueByColumnId<double?>>(_emptyArbitraryNumericValueByColumnId));

        // Act
        var result = await _subject.GetSum(_sumBatchDataLoader, MachineId, ColumnId, _arbitraryFrameworkApiTimeRanges, _cancellationTokenSource.Token);

        // Assert
        result.Exception.Should().BeNull();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task GetSum_Gets_MultipleMachines()
    {
        // Arrange
        _machineSnapshotsHttpClientMock.Setup(m => m.GetSumValues(
            It.IsAny<string>(),
            It.IsAny<List<string>>(),
            It.IsAny<List<WuH.Ruby.Common.Core.TimeRange>>(),
            It.IsAny<List<Filter>>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(new InternalItemResponse<ValueByColumnId<double?>>(_emptyArbitraryNumericValueByColumnId));

        // Act
        await _subject.GetSum(_sumBatchDataLoader, MachineId, ColumnId, _arbitraryFrameworkApiTimeRanges, _cancellationTokenSource.Token);

        // Assert
        _machineSnapshotsHttpClientMock.VerifyAll();
        _machineSnapshotsHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetGroupedSums_Returning_The_Correct_Value()
    {
        // Arrange
        _machineSnapshotsHttpClientMock.Setup(m => m.GetGroupedSums(
            It.IsAny<string>(),
            It.IsAny<List<GroupAssignment>>(),
            It.IsAny<List<WuH.Ruby.Common.Core.TimeRange>>(),
            It.IsAny<List<Filter>>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(new InternalItemResponse<ValueByColumnId<GroupedSumByIdentifier>>(_arbitraryGroupedSums));

        // Act
        var result = await _subject.GetGroupedSum(_groupedSumBatchDataLoader, MachineId, _groupAssignment, _arbitraryFrameworkApiTimeRanges, _cancellationTokenSource.Token);
        // Assert
        result.Exception.Should().BeNull();
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task GetGroupedSums_Ignores_Undefined_Key_Column_Ids()
    {
        // Arrange
        _machineSnapshotsHttpClientMock.Setup(m => m.GetGroupedSums(
            It.IsAny<string>(),
            It.IsAny<List<GroupAssignment>>(),
            It.IsAny<List<WuH.Ruby.Common.Core.TimeRange>>(),
            It.IsAny<List<Filter>>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(new InternalItemResponse<ValueByColumnId<GroupedSumByIdentifier>>(_arbitraryGroupedSums));

        // Act
        var result = await _subject.GetGroupedSum(_groupedSumBatchDataLoader, MachineId, new GroupAssignment("UnknownKeyColumnId", _groupAssignment.ValueColumnId), _arbitraryFrameworkApiTimeRanges, _cancellationTokenSource.Token);

        // Assert
        result.Exception.Should().BeNull();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task GetGroupedSums_Ignores_Undefined_Value_Column_Ids()
    {
        // Arrange
        _machineSnapshotsHttpClientMock.Setup(m => m.GetGroupedSums(
            It.IsAny<string>(),
            new List<GroupAssignment> { _groupAssignment },
            It.IsAny<List<WuH.Ruby.Common.Core.TimeRange>>(),
            It.IsAny<List<Filter>>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(new InternalItemResponse<ValueByColumnId<GroupedSumByIdentifier>>(_arbitraryGroupedSums));

        // Act
        var result = await _subject.GetGroupedSum(_groupedSumBatchDataLoader, MachineId, new GroupAssignment(_groupAssignment.KeyColumnId, "UnknownValueColumnId"), _arbitraryFrameworkApiTimeRanges, _cancellationTokenSource.Token);

        // Assert
        result.Exception.Should().BeNull();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task GetGroupedSums_Ignores_Undefined_Value_Column_Ids_And_Returns_Value()
    {
        // Arrange
        _machineSnapshotsHttpClientMock.Setup(m => m.GetGroupedSums(
            It.IsAny<string>(),
            It.IsAny<List<GroupAssignment>>(),
            It.IsAny<List<WuH.Ruby.Common.Core.TimeRange>>(),
            It.IsAny<List<Filter>>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(new InternalItemResponse<ValueByColumnId<GroupedSumByIdentifier>>(_arbitraryGroupedSums));

        // Act
        var result = await _subject.GetGroupedSum(_groupedSumBatchDataLoader, MachineId, new GroupAssignment("UnknownColumnId", _groupAssignment.ValueColumnId), _arbitraryFrameworkApiTimeRanges, _cancellationTokenSource.Token);

        // Assert
        result.Exception.Should().BeNull();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task GetGroupedSums_Gets_Empty_List_From_HttpClient_GetGroupedSums()
    {
        // Arrange
        _machineSnapshotsHttpClientMock.Setup(m => m.GetGroupedSums(
            It.IsAny<string>(),
            It.IsAny<List<GroupAssignment>>(),
            It.IsAny<List<WuH.Ruby.Common.Core.TimeRange>>(),
            It.IsAny<List<Filter>>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(new InternalItemResponse<ValueByColumnId<GroupedSumByIdentifier>>(_emptyArbitraryGroupedSums));

        // Act
        var result = await _subject.GetGroupedSum(_groupedSumBatchDataLoader, MachineId, _groupAssignment, _arbitraryFrameworkApiTimeRanges, _cancellationTokenSource.Token);

        // Assert
        result.Exception.Should().BeNull();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task GetGroupedSums_Gets_MultipleMachines()
    {
        // Arrange
        _machineSnapshotsHttpClientMock.Setup(m => m.GetGroupedSums(
            It.IsAny<string>(),
            It.IsAny<List<GroupAssignment>>(),
            It.IsAny<List<WuH.Ruby.Common.Core.TimeRange>>(),
            It.IsAny<List<Filter>>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(new InternalItemResponse<ValueByColumnId<GroupedSumByIdentifier>>(_emptyArbitraryGroupedSums));

        // Act
        await _subject.GetGroupedSum(_groupedSumBatchDataLoader, MachineId, new GroupAssignment(ColumnId, "SomeValue"), _arbitraryFrameworkApiTimeRanges, _cancellationTokenSource.Token);

        // Assert
        _machineSnapshotsHttpClientMock.VerifyAll();
        _machineSnapshotsHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetArithmeticMean_Returning_The_Correct_Value()
    {
        // Arrange
        _machineSnapshotsHttpClientMock.Setup(m => m.GetArithmeticMeans(
            It.IsAny<string>(),
            It.IsAny<List<string>>(),
            It.IsAny<List<WuH.Ruby.Common.Core.TimeRange>>(),
            It.IsAny<List<Filter>>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(new InternalItemResponse<ValueByColumnId<double?>>(_arbitraryNumericValueByColumnId));

        // Act‚‚
        var result = await _subject.GetArithmeticMean(_arithmeticMeanBatchDataLoader, MachineId, ColumnId, _arbitraryFrameworkApiTimeRanges, _cancellationTokenSource.Token);

        // Assert
        result.Exception.Should().BeNull();
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task GetArithmeticMean_Gets_Empty_List_From_HttpClient_GetArithmeticMeans()
    {
        // Arrange
        _machineSnapshotsHttpClientMock.Setup(m => m.GetArithmeticMeans(
            It.IsAny<string>(),
            It.IsAny<List<string>>(),
            It.IsAny<List<WuH.Ruby.Common.Core.TimeRange>>(),
            It.IsAny<List<Filter>>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(new InternalItemResponse<ValueByColumnId<double?>>(_emptyArbitraryNumericValueByColumnId));

        // Act
        var result = await _subject.GetArithmeticMean(_arithmeticMeanBatchDataLoader, MachineId, ColumnId, _arbitraryFrameworkApiTimeRanges, _cancellationTokenSource.Token);

        // Assert
        result.Exception.Should().BeNull();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task GetArithmeticMean_Gets_MultipleMachines()
    {
        // Arrange
        _machineSnapshotsHttpClientMock.Setup(m => m.GetArithmeticMeans(
            It.IsAny<string>(),
            It.IsAny<List<string>>(),
            It.IsAny<List<WuH.Ruby.Common.Core.TimeRange>>(),
            It.IsAny<List<Filter>>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(new InternalItemResponse<ValueByColumnId<double?>>(_emptyArbitraryNumericValueByColumnId));

        // Act
        var result = await _subject.GetArithmeticMean(_arithmeticMeanBatchDataLoader, MachineId, ColumnId, _arbitraryFrameworkApiTimeRanges, _cancellationTokenSource.Token);

        // Assert
        result.Exception.Should().BeNull();
        result.Value.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData(50)]
    public async Task GetDistinct_Returns_DataResult_With_Correct_Values(object? value)
    {
        // Arrange
        var mockDistinctValues = new ValueByColumnId<List<object?>>
            { { ColumnId, [value] } };

        _machineSnapshotsHttpClientMock
            .Setup(m => m.GetDistinctValues(
                MachineId, new List<string> { ColumnId }, _arbitraryTimeRanges, Limit, It.IsAny<List<Filter>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<ValueByColumnId<List<object?>>>(mockDistinctValues))
            .Verifiable(Times.Once);

        // Act
        var (distinctValues, exception) = await _subject.GetDistinct(
            _distinctValuesBatchLoader, MachineId, ColumnId, _arbitraryFrameworkApiTimeRanges, Limit, CancellationToken.None);

        // Assert
        distinctValues.Should().BeEquivalentTo(mockDistinctValues.First().Value);
        exception.Should().BeNull();

        _machineSnapshotsHttpClientMock.VerifyAll();
        _machineSnapshotsHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetDistinct_Returns_DataResult_With_Exception_If_GetDistinctValues_Fails()
    {
        // Arrange
        _machineSnapshotsHttpClientMock
            .Setup(m => m.GetDistinctValues(
                MachineId, new List<string> { ColumnId }, _arbitraryTimeRanges, Limit, It.IsAny<List<Filter>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<ValueByColumnId<List<object?>>>(
                (int)HttpStatusCode.InternalServerError, "ErrorMessage"))
            .Verifiable(Times.Once);

        // Act
        var (distinctValues, exception) = await _subject.GetDistinct(
            _distinctValuesBatchLoader, MachineId, ColumnId, _arbitraryFrameworkApiTimeRanges, Limit, CancellationToken.None);

        // Assert
        distinctValues.Should().BeNull();
        exception.Should().BeOfType<InternalServiceException>();
        exception!.Message.Should().Be("ErrorMessage");

        _machineSnapshotsHttpClientMock.VerifyAll();
        _machineSnapshotsHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetMedian_Returning_The_Correct_Value()
    {
        // Arrange
        _machineSnapshotsHttpClientMock.Setup(m => m.GetMedianValues(
            It.IsAny<string>(),
            It.IsAny<List<string>>(),
            It.IsAny<List<WuH.Ruby.Common.Core.TimeRange>>(),
            It.IsAny<List<Filter>>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(new InternalItemResponse<ValueByColumnId<double?>>(_arbitraryNumericValueByColumnId));

        // Act‚‚
        var result = await _subject.GetMedian(_medianValuesBatchDataLoader, MachineId, ColumnId, _arbitraryFrameworkApiTimeRanges, _cancellationTokenSource.Token);

        // Assert
        result.Exception.Should().BeNull();
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task GetMedian_Gets_Empty_List_From_HttpClient_GetMedianValues()
    {
        // Arrange
        _machineSnapshotsHttpClientMock.Setup(m => m.GetMedianValues(
            It.IsAny<string>(),
            It.IsAny<List<string>>(),
            It.IsAny<List<WuH.Ruby.Common.Core.TimeRange>>(),
            It.IsAny<List<Filter>>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(new InternalItemResponse<ValueByColumnId<double?>>(_emptyArbitraryNumericValueByColumnId));

        // Act
        var result = await _subject.GetMedian(_medianValuesBatchDataLoader, MachineId, ColumnId, _arbitraryFrameworkApiTimeRanges, _cancellationTokenSource.Token);

        // Assert
        result.Exception.Should().BeNull();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task GetMedian_Gets_MultipleMachines()
    {
        // Arrange
        _machineSnapshotsHttpClientMock.Setup(m => m.GetMedianValues(
            It.IsAny<string>(),
            It.IsAny<List<string>>(),
            It.IsAny<List<WuH.Ruby.Common.Core.TimeRange>>(),
            It.IsAny<List<Filter>>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(new InternalItemResponse<ValueByColumnId<double?>>(_emptyArbitraryNumericValueByColumnId));

        // Act
        await _subject.GetMedian(_medianValuesBatchDataLoader, MachineId, ColumnId, _arbitraryFrameworkApiTimeRanges, _cancellationTokenSource.Token);

        // Assert
        _machineSnapshotsHttpClientMock.VerifyAll();
        _machineSnapshotsHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetStandardDeviation_Returning_The_Correct_Value()
    {
        // Arrange
        _machineSnapshotsHttpClientMock.Setup(m => m.GetStandardDeviations(
            It.IsAny<string>(),
            It.IsAny<List<string>>(),
            It.IsAny<List<WuH.Ruby.Common.Core.TimeRange>>(),
            It.IsAny<List<Filter>>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(new InternalItemResponse<ValueByColumnId<double?>>(_arbitraryNumericValueByColumnId));

        // Act‚‚
        var result = await _subject.GetStandardDeviation(_standardDeviationBatchDataLoader, MachineId, ColumnId, _arbitraryFrameworkApiTimeRanges, _cancellationTokenSource.Token);

        // Assert
        result.Exception.Should().BeNull();
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task GetStandardDeviation_Gets_Empty_List_From_HttpClient_GetStandardDeviations()
    {
        // Arrange
        _machineSnapshotsHttpClientMock.Setup(m => m.GetStandardDeviations(
            It.IsAny<string>(),
            It.IsAny<List<string>>(),
            It.IsAny<List<WuH.Ruby.Common.Core.TimeRange>>(),
            It.IsAny<List<Filter>>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(new InternalItemResponse<ValueByColumnId<double?>>(_emptyArbitraryNumericValueByColumnId));

        // Act
        var result = await _subject.GetStandardDeviation(_standardDeviationBatchDataLoader, MachineId, ColumnId, _arbitraryFrameworkApiTimeRanges, _cancellationTokenSource.Token);

        // Assert
        result.Exception.Should().BeNull();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task GetStandardDeviation_Gets_MultipleMachines()
    {
        // Arrange
        _machineSnapshotsHttpClientMock.Setup(m => m.GetStandardDeviations(
            It.IsAny<string>(),
            It.IsAny<List<string>>(),
            It.IsAny<List<WuH.Ruby.Common.Core.TimeRange>>(),
            It.IsAny<List<Filter>>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(new InternalItemResponse<ValueByColumnId<double?>>(_emptyArbitraryNumericValueByColumnId));

        // Act
        await _subject.GetStandardDeviation(_standardDeviationBatchDataLoader, MachineId, ColumnId, _arbitraryFrameworkApiTimeRanges, _cancellationTokenSource.Token);

        // Assert
        _machineSnapshotsHttpClientMock.VerifyAll();
        _machineSnapshotsHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetColumnValue_Returns_DataResult_With_Correct_Value()
    {
        // Arrange
        _machineSnapshotsHttpClientMock
            .Setup(m => m.GetSnapshotsForTimestamps(
                MachineId,
                new List<DateTime> { _timestampMinValue },
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<MachineSnapshotForTimestampListResponse>(
                GenerateMachineSnapshotForTimestampListResponse(new List<DateTime> { _timestampMinValue })))
            .Verifiable(Times.Once);

        // Act
        var (columnValue, exception) = await _subject.GetColumnValue(
            _snapshotByTimestampBatchDataLoader,
            ColumnId,
            _timestampMinValue,
            MachineId,
            CancellationToken.None);

        // Assert
        columnValue.Should().BeEquivalentTo(new SnapshotValue(ColumnId, columnValue: 100));
        exception.Should().BeNull();

        _machineSnapshotsHttpClientMock.VerifyAll();
        _machineSnapshotsHttpClientMock.VerifyNoOtherCalls();
    }

    // TODO: I expected the method to return an exception here
    [Fact]
    public async Task
        GetColumnValue_Returns_DataResult_With_Exception_If_GetSnapshotsForTimestamps_Returns_Not_Correct_Amount_Of_Data()
    {
        // Arrange
        _machineSnapshotsHttpClientMock
            .Setup(m => m.GetSnapshotsForTimestamps(
                MachineId, new List<DateTime> { _timestampMinValue }, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<MachineSnapshotForTimestampListResponse>(
                GenerateMachineSnapshotForTimestampListResponse(new List<DateTime>
                    { DateTime.MinValue, DateTime.MaxValue })))
            .Verifiable(Times.Once);

        // Act
        var (columnValue, exception) = await _subject.GetColumnValue(
            _snapshotByTimestampBatchDataLoader,
            ColumnId,
            _timestampMinValue,
            MachineId,
            CancellationToken.None);

        // Assert
        columnValue.Should().BeEquivalentTo(new SnapshotValue(ColumnId, columnValue: 100));
        exception.Should().BeNull();

        _machineSnapshotsHttpClientMock.VerifyAll();
        _machineSnapshotsHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetColumnValue_Returns_DataResult_With_Exception_If_GetSnapshotsForTimestamps_Fails()
    {
        // Arrange
        _machineSnapshotsHttpClientMock
            .Setup(m => m.GetSnapshotsForTimestamps(
                MachineId, new List<DateTime> { _timestampMinValue }, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<MachineSnapshotForTimestampListResponse>(
                (int)HttpStatusCode.InternalServerError, "ErrorMessage"))
            .Verifiable(Times.Once);

        // Act
        var (columnValue, exception) = await _subject.GetColumnValue(
            _snapshotByTimestampBatchDataLoader, ColumnId, _timestampMinValue, MachineId, CancellationToken.None);

        // Assert
        columnValue.Should().BeNull();
        exception.Should().BeOfType<InternalServiceException>();
        exception!.Message.Should().Be("ErrorMessage");

        _machineSnapshotsHttpClientMock.VerifyAll();
        _machineSnapshotsHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetNumericColumnTrend_Success()
    {
        // Arrange
        var timeRanges = new List<TimeRange>
        {
            new(DateTime.UnixEpoch, DateTime.UnixEpoch.AddDays(1))
        };
        _machineSnapshotsHttpClientMock
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

        // Act
        var result = (await _subject.GetNumericColumnTrend(
            _machineTrendByTimeRangeBatchDataLoader, ColumnId, timeRanges, MachineId, CancellationToken.None)).ToList();

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(60 * 24 + 1);
        result.Should().AllSatisfy(el => el.Value.Should().Be(10));

        _machineSnapshotsHttpClientMock.VerifyAll();
        _machineSnapshotsHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetNumericColumnTrend_Throws_If_GetSnapshotsInTimeRanges_Has_Error_Fails()
    {
        // Arrange
        var timeRanges = new List<TimeRange>
        {
            new(DateTime.UnixEpoch, DateTime.UnixEpoch.AddDays(1))
        };
        _machineSnapshotsHttpClientMock
            .Setup(m => m.GetSnapshotsInTimeRanges(
                MachineId, new List<string> { ColumnId }, timeRanges, new List<Filter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<MachineSnapshotListResponse>(
                (int)HttpStatusCode.InternalServerError, "ErrorMessage"))
            .Verifiable(Times.Once);

        // Act
        var act = async () => await _subject.GetNumericColumnTrend(
            _machineTrendByTimeRangeBatchDataLoader, ColumnId, timeRanges, MachineId, CancellationToken.None);

        // Assert
        (await act.Should().ThrowAsync<Exception>()).WithMessage("ErrorMessage");

        _machineSnapshotsHttpClientMock.VerifyAll();
        _machineSnapshotsHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetNumericColumnTrend_Returns_Empty_List_When_No_TimeRanges_Given()
    {
        // Arrange
        var timeRanges = new List<TimeRange>();

        // Act
        var result = await _subject.GetNumericColumnTrend(
            _machineTrendByTimeRangeBatchDataLoader, ColumnId, timeRanges, MachineId, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetNumericColumnTrend_Returns_Empty_List_When_No_TimeRanges_Given_Is_Null()
    {
        // Arrange
        // Act
        var result = await _subject.GetNumericColumnTrend(
            _machineTrendByTimeRangeBatchDataLoader, ColumnId, null, MachineId, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    private static MachineSnapshotResponse GenerateMachineSnapshotResponse()
    {
        var snapshotMetaDto =
            new SnapshotMetaDto(MachineId, "fakeHash", DateTime.MinValue, [new SnapshotColumnUnitDto(ColumnId, ColumnUnit)]);
        var snapshotDto = new SnapshotDto(
            [new SnapshotColumnValueDto(ColumnId, value: 100)], DateTime.MaxValue);

        return new MachineSnapshotResponse(snapshotMetaDto, snapshotDto);
    }

    private static MachineSnapshotForTimestampListResponse GenerateMachineSnapshotForTimestampListResponse(
        IList<DateTime> timestamps)
    {
        var snapshotColumnUnitDto = new SnapshotColumnUnitDto(ColumnId, ColumnUnit);
        var snapshotMetaDto = new SnapshotMetaDto(MachineId, "fakeHash", DateTime.MinValue, [snapshotColumnUnitDto]);
        var snapshotForTimestampDtos = timestamps
            .Select(timestamp => new SnapshotForTimestampDto(
                new SnapshotDto(
                    [new SnapshotColumnValueDto(ColumnId, timestamps.IndexOf(timestamp) + 100)],
                    timestamp),
                timestamp))
            .ToList();

        return new MachineSnapshotForTimestampListResponse(snapshotMetaDto, snapshotForTimestampDtos);
    }
}