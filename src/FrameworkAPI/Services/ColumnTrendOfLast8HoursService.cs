using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.DataLoaders;
using FrameworkAPI.Models;
using FrameworkAPI.Schema.Misc;
using FrameworkAPI.Services.Interfaces;
using DataResult = FrameworkAPI.Models.DataResult<
    System.Collections.Generic.IEnumerable<FrameworkAPI.Schema.Misc.NumericTrendElement>>;

namespace FrameworkAPI.Services;

public class ColumnTrendOfLast8HoursService(
    IMachineTimeService machineTimeService, IMachineSnapshotService machineSnapshotService) : IColumnTrendService
{
    private readonly IMachineTimeService _machineTimeService = machineTimeService;
    private readonly IMachineSnapshotService _machineSnapshotService = machineSnapshotService;

    public async Task<DataResult<IEnumerable<NumericTrendElement>>> GetLatest(
        LatestMachineTrendCacheDataLoader latestMachineTrendCacheDataLoader,
        string columnId,
        string machineId,
        CancellationToken cancellationToken)
    {
        var (columnTrend, exception) = await _machineSnapshotService.GetLatestColumnTrend(
            latestMachineTrendCacheDataLoader, columnId, machineId, cancellationToken);

        if (exception is not null)
        {
            return new DataResult(value: null, exception);
        }

        if (columnTrend is null)
        {
            return new DataResult(value: null, exception: null);
        }

        var columnTrendOrException = await GetInternal(columnTrend, endTime: null, machineId, cancellationToken);
        return columnTrendOrException;
    }

    public async Task<DataResult<IEnumerable<NumericTrendElement>>> Get(
        MachineTrendByTimeRangeBatchDataLoader machineTrendByTimeRangeBatchDataLoader,
        string columnId,
        DateTime endTime,
        string machineId,
        CancellationToken cancellationToken)
    {
        var (columnTrend, exception) = await _machineSnapshotService.GetNumericColumnTrendOfLast8Hours(
            machineTrendByTimeRangeBatchDataLoader, columnId, endTime, machineId, cancellationToken);

        if (exception is not null)
        {
            return new DataResult(value: null, exception);
        }

        if (columnTrend is null)
        {
            return new DataResult(value: null, exception: null);
        }

        var columnTrendOrException = await GetInternal(columnTrend, endTime, machineId, cancellationToken);
        return columnTrendOrException;
    }

    private async Task<DataResult<IEnumerable<NumericTrendElement>>> GetInternal(
        IDictionary<DateTime, double?> columnTrend,
        DateTime? endTime,
        string machineId,
        CancellationToken cancellationToken)
    {
        var trendTimeSpanInMinutes = (int)Constants.MachineTrend.TrendTimeSpan.TotalMinutes;

        if (!columnTrend.Any())
        {
            // Fill the whole column trend with a null value for each minute
            DateTime to;

            if (endTime is null)
            {
                var (machineTime, exception) = await _machineTimeService.Get(machineId, cancellationToken);

                if (exception is not null)
                {
                    return new DataResult(value: null, exception);
                }

                to = machineTime!.Value;
            }
            else
            {
                to = endTime.Value;
            }

            // Reset seconds and milliseconds
            to = new DateTime(to.Year, to.Month, to.Day, to.Hour, to.Minute, second: 0, DateTimeKind.Utc);

            for (var i = 0; i < trendTimeSpanInMinutes; i++)
            {
                var dateTime = to.Subtract(TimeSpan.FromMinutes(i));
                columnTrend.Add(dateTime, value: null);
            }
        }
        else if (columnTrend.Count < trendTimeSpanInMinutes)
        {
            // Fill everything up to the first column trend element (exclusive) with a null value for each minute.
            // Gaps after existing column trend elements should never exist because the machine snapShooter returns
            // snapshots only after all snapshots have been created based on the historical data
            var columnTrendElementsToAddCount = trendTimeSpanInMinutes - columnTrend.Count;
            var firstColumnTrendElement = columnTrend.First();
            var lastDateTime = firstColumnTrendElement.Key;

            for (var i = 0; i < columnTrendElementsToAddCount; i++)
            {
                var dateTime = lastDateTime.Subtract(TimeSpan.FromMinutes(i + 1));
                columnTrend.Add(dateTime, value: null);
            }
        }

        return new DataResult(columnTrend.Select(kvp => new NumericTrendElement(kvp.Key, kvp.Value)), exception: null);
    }
}