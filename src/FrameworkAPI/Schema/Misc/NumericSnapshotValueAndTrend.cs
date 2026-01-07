using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.DataLoaders;
using FrameworkAPI.Services.Interfaces;
using HotChocolate;

namespace FrameworkAPI.Schema.Misc;

/// <summary>
/// The value of a numeric variable and the trend of the last 8-hours.
/// This is only supported for values that can be derived from MachineSnapshots.
/// </summary>
public class NumericSnapshotValueAndTrend(string columnId, DateTime? queryTimestamp, string machineId)
{
    private readonly string _columnId = columnId;
    private readonly DateTime? _queryTimestamp = queryTimestamp;
    private readonly string _machineId = machineId;

    /// <summary>
    /// The numeric value in SI unit
    /// (is 'null', if this data is currently not available).
    /// </summary>
    public async Task<double?> Value(
        LatestSnapshotCacheDataLoader latestDataLoader,
        SnapshotByTimestampBatchDataLoader timestampDataLoader,
        [Service] IMachineSnapshotService service,
        CancellationToken cancellationToken)
    {
        var (value, exception) = _queryTimestamp is null
            ? await service.GetLatestColumnValue(latestDataLoader, _columnId, _machineId, cancellationToken)
            : await service.GetColumnValue(
                timestampDataLoader, _columnId, _queryTimestamp.Value, _machineId, cancellationToken);

        if (exception is not null)
        {
            throw exception;
        }

        return value is null ? null : Convert.ToDouble(value.ColumnValue);
    }

    /// <summary>
    /// The unit of the value
    /// (if the unit needs to be translated, the corresponding i18n tag is provided here; for example 'label.items').
    /// </summary>
    public async Task<string?> Unit(
        LatestSnapshotCacheDataLoader dataLoader,
        [Service] IMachineSnapshotService service,
        CancellationToken cancellationToken)
    {
        var (unit, exception) = await service.GetLatestColumnUnit(dataLoader, _columnId, _machineId, cancellationToken);

        if (exception is not null)
        {
            throw exception;
        }

        return unit;
    }

    /// <summary>
    /// The trend of the last 8 hours. It is represented by trend elements each consisting of a time and a value. The
    /// trend elements are sorted by the time (ascending) and 481 (8 hours * 60 minutes + 1 minute) trend elements will
    /// always be returned (if no error occurred). The whole trend will be filled with a null value for each minute if
    /// there are no snapshots. Everything up to the first trend element (exclusive) will be filled with a null value
    /// for each minute if there are less than 481 snapshots. Gaps after existing trend elements should never exist
    /// because the MachineSnapShooter returns snapshots only after all snapshots have been created based on the
    /// historical data.
    ///
    /// Trend interval:
    /// - Query timestamp is not provided
    ///   - Start: Machine time - 8 hours
    ///   - End: Machine time
    /// - Query timestamp is provided
    ///   - Start: Query timestamp - 8 hours
    ///   - End: Query timestamp
    /// </summary>
    public async Task<IEnumerable<NumericTrendElement>?> TrendOfLast8Hours(
        LatestMachineTrendCacheDataLoader latestMachineTrendCacheDataLoader,
        MachineTrendByTimeRangeBatchDataLoader machineTrendByTimeRangeBatchDataLoader,
        [Service] IColumnTrendService columnTrendService,
        CancellationToken cancellationToken)
    {
        var (columnTrend, exception) = _queryTimestamp is null
            ? await columnTrendService.GetLatest(
                latestMachineTrendCacheDataLoader, _columnId, _machineId, cancellationToken)
            : await columnTrendService.Get(
                machineTrendByTimeRangeBatchDataLoader, _columnId, _queryTimestamp.Value, _machineId, cancellationToken);

        if (exception is not null)
        {
            throw exception;
        }

        return columnTrend;
    }
}