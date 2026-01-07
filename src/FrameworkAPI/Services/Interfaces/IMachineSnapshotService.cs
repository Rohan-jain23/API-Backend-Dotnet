using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.DataLoaders;
using FrameworkAPI.Models;
using FrameworkAPI.Schema.Misc;
using WuH.Ruby.MachineSnapShooter.Client.Models;
using TimeRange = WuH.Ruby.Common.Core.TimeRange;

namespace FrameworkAPI.Services.Interfaces;

public interface IMachineSnapshotService
{
    Task<DataResult<SnapshotValue>> GetLatestColumnValue(
        LatestSnapshotCacheDataLoader dataLoader,
        string columnId,
        string machineId,
        CancellationToken cancellationToken);

    Task<DataResult<string>> GetLatestColumnUnit(
        LatestSnapshotCacheDataLoader dataLoader,
        string columnId,
        string machineId,
        CancellationToken cancellationToken);

    Task<DataResult<DateTime?>> GetLatestColumnChangedTimestamp(
        LatestSnapshotColumnIdChangedTimestampCacheDataLoader dataLoader,
        string columnId,
        string machineId,
        CancellationToken cancellationToken);

    Task<DataResult<SortedDictionary<DateTime, double?>>> GetLatestColumnTrend(
        LatestMachineTrendCacheDataLoader dataLoader,
        string columnId,
        string machineId,
        CancellationToken cancellationToken);

    Task<DataResult<object?>> GetValueWithLongestDuration(
        SnapshotValuesWithLongestDurationBatchDataLoader dataLoader,
        string machineId,
        string columnId,
        List<TimeRange> timeRanges,
        CancellationToken cancellationToken);

    Task<DataResult<List<object?>>> GetDistinct(
        SnapshotDistinctValuesBatchDataLoader dataLoader,
        string machineId,
        string columnId,
        List<TimeRange> timeRanges,
        int limit,
        CancellationToken cancellationToken);

    Task<DataResult<double?>> GetMin(
        SnapshotMinBatchDataLoader dataLoader,
        string machineId,
        string columnId,
        List<TimeRange> timeRanges,
        CancellationToken cancellationToken);

    Task<DataResult<double?>> GetMax(
        SnapshotMaxBatchDataLoader dataLoader,
        string machineId,
        string columnId,
        List<TimeRange> timeRanges,
        CancellationToken cancellationToken);

    Task<DataResult<double?>> GetSum(
        SnapshotSumBatchDataLoader dataLoader,
        string machineId,
        string columnId,
        List<TimeRange> timeRanges,
        CancellationToken cancellationToken);

    Task<DataResult<GroupedSumByIdentifier>> GetGroupedSum(
        SnapshotGroupedSumBatchDataLoader dataLoader,
        string machineId,
        GroupAssignment groupAssignment,
        List<TimeRange> timeRanges,
        CancellationToken cancellationToken);

    Task<DataResult<double?>> GetArithmeticMean(
        SnapshotArithmeticMeanBatchDataLoader dataLoader,
        string machineId,
        string columnId,
        List<TimeRange> timeRanges,
        CancellationToken cancellationToken);

    Task<DataResult<double?>> GetMedian(
        SnapshotMedianValuesBatchDataLoader dataLoader,
        string machineId,
        string columnId,
        List<TimeRange> timeRanges,
        CancellationToken cancellationToken);

    Task<DataResult<double?>> GetStandardDeviation(
        SnapshotStandardDeviationBatchDataLoader dataLoader,
        string machineId,
        string columnId,
        List<TimeRange> timeRanges,
        CancellationToken cancellationToken);

    Task<DataResult<SnapshotValue>> GetColumnValue(
        SnapshotByTimestampBatchDataLoader dataLoader,
        string columnId,
        DateTime timestamp,
        string machineId,
        CancellationToken cancellationToken);

    Task<DataResult<SortedDictionary<DateTime, double?>>> GetNumericColumnTrendOfLast8Hours(
        MachineTrendByTimeRangeBatchDataLoader dataLoader,
        string columnId,
        DateTime endTime,
        string machineId,
        CancellationToken cancellationToken);

    Task<IEnumerable<NumericTrendElement>> GetNumericColumnTrend(
        MachineTrendByTimeRangeBatchDataLoader dataLoader,
        string columnId,
        List<TimeRange>? timeRanges,
        string machineId,
        CancellationToken cancellationToken);

    Task<DataResult<DateTime?>> GetColumnChangedTimestamp(
        SnapshotColumnIdChangedTimestampCacheDataLoader dataLoader,
        string columnId,
        DateTime endTime,
        string machineId,
        CancellationToken cancellationToken);
}