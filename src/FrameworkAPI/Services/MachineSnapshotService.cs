using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.DataLoaders;
using FrameworkAPI.Models;
using FrameworkAPI.Models.DataLoader;
using FrameworkAPI.Schema.Misc;
using FrameworkAPI.Services.Interfaces;
using GreenDonut;
using WuH.Ruby.MachineSnapShooter.Client.Models;
using TimeRange = WuH.Ruby.Common.Core.TimeRange;

namespace FrameworkAPI.Services;

public class MachineSnapshotService : IMachineSnapshotService
{
    public async Task<DataResult<SnapshotValue>> GetLatestColumnValue(
        LatestSnapshotCacheDataLoader dataLoader,
        string columnId,
        string machineId,
        CancellationToken cancellationToken)
    {
        var (latestSnapshot, exception) = await dataLoader.LoadAsync(machineId, cancellationToken);

        if (exception is not null)
        {
            return new DataResult<SnapshotValue>(value: null, exception);
        }

        // The latest snapshot is for example null when the machine is waiting for first minutely snapshot
        if (latestSnapshot is null)
        {
            return new DataResult<SnapshotValue>(new SnapshotValue(columnId, null), exception: null);
        }

        var snapshotValue = new SnapshotValue(
            columnId,
            latestSnapshot.Data?.ColumnValues
                .Find(snapshotColumnValueDto => snapshotColumnValueDto.Id == columnId)?.Value,
            latestSnapshot.Data?.IsCreatedByVirtualTime);

        return new DataResult<SnapshotValue>(snapshotValue, exception: null);
    }

    public async Task<DataResult<string>> GetLatestColumnUnit(
        LatestSnapshotCacheDataLoader dataLoader,
        string columnId,
        string machineId,
        CancellationToken cancellationToken)
    {
        var (latestSnapshot, exception) = await dataLoader.LoadAsync(machineId, cancellationToken);

        if (exception is not null)
        {
            return new DataResult<string>(value: null, exception);
        }

        if (latestSnapshot is null)
        {
            return new DataResult<string>(value: null, exception: null);
        }

        var unit = latestSnapshot.Meta.ColumnUnits
            .Find(snapshotColumnUnitDto => snapshotColumnUnitDto.Id == columnId)?.Unit ?? string.Empty;

        return new DataResult<string>(unit, exception: null);
    }

    public async Task<DataResult<DateTime?>> GetLatestColumnChangedTimestamp(
        LatestSnapshotColumnIdChangedTimestampCacheDataLoader dataLoader,
        string columnId,
        string machineId,
        CancellationToken cancellationToken)
    {
        var dataResult = await dataLoader.LoadAsync((MachineId: machineId, ColumnId: columnId), cancellationToken);
        return dataResult;
    }

    public async Task<DataResult<SortedDictionary<DateTime, double?>>> GetLatestColumnTrend(
        LatestMachineTrendCacheDataLoader dataLoader,
        string columnId,
        string machineId,
        CancellationToken cancellationToken)
    {
        var (machineTrend, exception) = await dataLoader.LoadAsync(machineId, cancellationToken);

        if (exception is not null)
        {
            return new DataResult<SortedDictionary<DateTime, double?>>(value: null, exception);
        }

        if (machineTrend is null)
        {
            return new DataResult<SortedDictionary<DateTime, double?>>(value: null, exception: null);
        }

        var columnTrend = machineTrend.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value?.GetValueOrDefault(columnId));

        // Sort column trend by date time (ascending)
        var sortedColumnTrend = new SortedDictionary<DateTime, double?>(columnTrend);

        return new DataResult<SortedDictionary<DateTime, double?>>(sortedColumnTrend, exception: null);
    }

    public async Task<DataResult<object?>> GetValueWithLongestDuration(
        SnapshotValuesWithLongestDurationBatchDataLoader dataLoader,
        string machineId,
        string columnId,
        List<TimeRange> timeRanges,
        CancellationToken cancellationToken)
    {
        var dataResult = await dataLoader.LoadAsync(new SnapshotValueRequestKey(machineId, columnId, timeRanges), cancellationToken);

        return dataResult.Exception is null
            ? new DataResult<object?>(dataResult.Value, exception: null)
            : new DataResult<object?>(value: null, dataResult.Exception);
    }

    public async Task<DataResult<List<object?>>> GetDistinct(
        SnapshotDistinctValuesBatchDataLoader dataLoader,
        string machineId,
        string columnId,
        List<TimeRange> timeRanges,
        int limit,
        CancellationToken cancellationToken)
    {
        var dataResult = await dataLoader.LoadAsync(new SnapshotValueWithLimitRequestKey(machineId, columnId, timeRanges, limit), cancellationToken);

        return dataResult.Exception is null
            ? new DataResult<List<object?>>(dataResult.Value, exception: null)
            : new DataResult<List<object?>>(value: null, dataResult.Exception);
    }

    public async Task<DataResult<double?>> GetMin(
        SnapshotMinBatchDataLoader dataLoader,
        string machineId,
        string columnId,
        List<TimeRange> timeRanges,
        CancellationToken cancellationToken)
    {
        var dataLoaderResult = await dataLoader.LoadAsync(new SnapshotValueRequestKey(machineId, columnId, timeRanges), cancellationToken);

        return dataLoaderResult.Exception is null
            ? new DataResult<double?>(dataLoaderResult.Value, exception: null)
            : new DataResult<double?>(null, dataLoaderResult.Exception);
    }

    public async Task<DataResult<double?>> GetMax(
        SnapshotMaxBatchDataLoader dataLoader,
        string machineId,
        string columnId,
        List<TimeRange> timeRanges,
        CancellationToken cancellationToken)
    {
        var dataLoaderResult = await dataLoader.LoadAsync(new SnapshotValueRequestKey(machineId, columnId, timeRanges), cancellationToken);

        return dataLoaderResult.Exception is null
            ? new DataResult<double?>(dataLoaderResult.Value, exception: null)
            : new DataResult<double?>(null, dataLoaderResult.Exception);
    }

    public async Task<DataResult<double?>> GetSum(
        SnapshotSumBatchDataLoader dataLoader,
        string machineId,
        string columnId,
        List<TimeRange> timeRanges,
        CancellationToken cancellationToken)
    {
        var dataLoaderResult = await dataLoader.LoadAsync(new SnapshotValueRequestKey(machineId, columnId, timeRanges), cancellationToken);

        return dataLoaderResult.Exception is null
            ? new DataResult<double?>(dataLoaderResult.Value, exception: null)
            : new DataResult<double?>(null, dataLoaderResult.Exception);
    }

    public async Task<DataResult<GroupedSumByIdentifier>> GetGroupedSum(
        SnapshotGroupedSumBatchDataLoader dataLoader,
        string machineId,
        GroupAssignment groupAssignment,
        List<TimeRange> timeRanges,
        CancellationToken cancellationToken)
    {
        var dataLoaderResult = await dataLoader.LoadAsync(new GroupedSumRequestKey(machineId, groupAssignment, timeRanges), cancellationToken);
        return dataLoaderResult.Exception is null
            ? new DataResult<GroupedSumByIdentifier>(dataLoaderResult.Value, exception: null)
            : new DataResult<GroupedSumByIdentifier>(null, dataLoaderResult.Exception);
    }

    public async Task<DataResult<double?>> GetArithmeticMean(
        SnapshotArithmeticMeanBatchDataLoader dataLoader,
        string machineId,
        string columnId,
        List<TimeRange> timeRanges,
        CancellationToken cancellationToken)
    {
        var dataLoaderResult = await dataLoader.LoadAsync(new SnapshotValueRequestKey(machineId, columnId, timeRanges), cancellationToken);

        return dataLoaderResult.Exception is null
            ? new DataResult<double?>(dataLoaderResult.Value, exception: null)
            : new DataResult<double?>(null, dataLoaderResult.Exception);
    }

    public async Task<DataResult<double?>> GetMedian(
        SnapshotMedianValuesBatchDataLoader dataLoader,
        string machineId,
        string columnId,
        List<TimeRange> timeRanges,
        CancellationToken cancellationToken)
    {
        var dataLoaderResult = await dataLoader.LoadAsync(new SnapshotValueRequestKey(machineId, columnId, timeRanges), cancellationToken);

        return dataLoaderResult.Exception is null
            ? new DataResult<double?>(dataLoaderResult.Value, exception: null)
            : new DataResult<double?>(null, dataLoaderResult.Exception);
    }

    public async Task<DataResult<double?>> GetStandardDeviation(
        SnapshotStandardDeviationBatchDataLoader dataLoader,
        string machineId,
        string columnId,
        List<TimeRange> timeRanges,
        CancellationToken cancellationToken)
    {
        var dataLoaderResult = await dataLoader.LoadAsync(new SnapshotValueRequestKey(machineId, columnId, timeRanges), cancellationToken);

        return dataLoaderResult.Exception is null
            ? new DataResult<double?>(dataLoaderResult.Value, exception: null)
            : new DataResult<double?>(null, dataLoaderResult.Exception);
    }

    public async Task<DataResult<SnapshotValue>> GetColumnValue(
        SnapshotByTimestampBatchDataLoader dataLoader,
        string columnId,
        DateTime timestamp,
        string machineId,
        CancellationToken cancellationToken)
    {
        var (snapshotDtoByTimestamp, exception) = await dataLoader.LoadAsync((machineId, timestamp), cancellationToken);

        if (exception is not null)
        {
            return new DataResult<SnapshotValue>(value: null, exception);
        }

        var snapshotValue = new SnapshotValue(
            columnId,
            snapshotDtoByTimestamp?.ColumnValues
                .Find(snapshotColumnValueDto => snapshotColumnValueDto.Id == columnId)?.Value,
            snapshotDtoByTimestamp?.IsCreatedByVirtualTime ?? false);

        return new DataResult<SnapshotValue>(snapshotValue, exception: null);
    }

    public async Task<DataResult<SortedDictionary<DateTime, double?>>> GetNumericColumnTrendOfLast8Hours(
        MachineTrendByTimeRangeBatchDataLoader dataLoader,
        string columnId,
        DateTime endTime,
        string machineId,
        CancellationToken cancellationToken)
    {
        var to = endTime;
        var from = endTime.Subtract(TimeSpan.FromHours(8));

        var (machineTrend, exception) = await dataLoader.LoadAsync((machineId, new TimeRange(from, to), columnId), cancellationToken);

        if (exception is not null)
        {
            return new DataResult<SortedDictionary<DateTime, double?>>(value: null, exception);
        }

        if (machineTrend is null)
        {
            return new DataResult<SortedDictionary<DateTime, double?>>(value: null, exception: null);
        }

        var columnTrend = machineTrend.ToDictionary<KeyValuePair<DateTime, object?>, DateTime, double?>(
            kvp => kvp.Key,
            kvp => kvp.Value is not null ? Convert.ToDouble(kvp.Value) : null);

        // Sort column trend by date time (ascending)
        var sortedColumnTrend = new SortedDictionary<DateTime, double?>(columnTrend);

        return new DataResult<SortedDictionary<DateTime, double?>>(sortedColumnTrend, exception: null);
    }

    public async Task<IEnumerable<NumericTrendElement>> GetNumericColumnTrend(
        MachineTrendByTimeRangeBatchDataLoader dataLoader,
        string columnId,
        List<TimeRange>? timeRanges,
        string machineId,
        CancellationToken cancellationToken)
    {
        if (timeRanges is null || timeRanges.Count == 0)
        {
            return [];
        }

        var results = await timeRanges.ToObservable()
            .SelectMany(timeRange => Observable.FromAsync(
                async ctFrmAsync => await dataLoader.LoadAsync(
                    ctFrmAsync,
                    (machineId, timeRange, columnId))))
            .SelectMany(result => result)
            .ToList()
            .ToTask(cancellationToken);

        if (results.FirstOrDefault(result => result.Exception is not null) is { } errorResult)
            throw errorResult.Exception!;

        var trend = results
            .SelectMany(result => result.Value?
                .Select(kvp => new NumericTrendElement(
                    kvp.Key,
                    kvp.Value is not null ? Convert.ToDouble(kvp.Value) : null)) ?? [])
            .DistinctBy(element => element.Time)
            .OrderByDescending(element => element.Time)
            .ToList();

        return trend;
    }

    public async Task<DataResult<DateTime?>> GetColumnChangedTimestamp(
        SnapshotColumnIdChangedTimestampCacheDataLoader dataLoader,
        string columnId,
        DateTime endTime,
        string machineId,
        CancellationToken cancellationToken)
    {
        var dataResult = await dataLoader.LoadAsync(
            (MachineId: machineId, ColumnId: columnId, endTime), cancellationToken);
        return dataResult;
    }
}