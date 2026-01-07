using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.DataLoaders;
using FrameworkAPI.Extensions;
using FrameworkAPI.Models;
using FrameworkAPI.Services.Interfaces;
using HotChocolate;
using Microsoft.AspNetCore.Http;

namespace FrameworkAPI.Schema.Misc;

/// <summary>
/// The values of a numeric variable (for example a set value) can be changed during production of a job/roll/time-span,
/// In these cases it is not always easy to determine just one single value for the variable that stands for the whole job/roll/time-span
/// This data model provides more information, so the user might get an indication if the value was changed during the job/roll/time-span
/// This is only supported for values that can be derived from MachineSnapshots.
/// </summary>
public class NumericSnapshotValuesDuringProduction(string columnId, DateTime? endTime, string machineId, IEnumerable<TimeRange>? timeRanges, DateTime? machineQueryTimestamp)
{
    private readonly string _columnId = columnId;
    private readonly DateTime? _endTime = endTime;
    private readonly string _machineId = machineId;
    private readonly IEnumerable<TimeRange>? _timeRanges = timeRanges;
    private readonly DateTime? _machineQueryTimestamp = machineQueryTimestamp;

    /// <summary>
    /// The value on the end of the job/roll/time-span in SI unit.
    /// If the job/roll/time-span is still active, this is the current live-value.
    /// </summary>
    public async Task<double?> LastValue(
        LatestSnapshotCacheDataLoader latestDataLoader,
        SnapshotByTimestampBatchDataLoader timestampDataLoader,
        [Service] IMachineSnapshotService service,
        CancellationToken cancellationToken)
    {
        var (value, exception) = _endTime is null
            ? await service.GetLatestColumnValue(latestDataLoader, _columnId, _machineId, cancellationToken)
            : await service.GetColumnValue(
                timestampDataLoader, _columnId, _endTime.Value, _machineId, cancellationToken);

        if (exception is not null)
        {
            throw exception;
        }

        return value?.ColumnValue is not null
            ? Convert.ToDouble(value.ColumnValue)
            : null;
    }

    /// <summary>
    /// If the job/roll was queried via the machine query, this returns the value at the query timestamp.
    /// If the query timestamp is 'null' and the job/roll is active, this is the live-value.
    /// Otherwise (a completed job/roll/time-span is queried via another query), this value is 'null'.
    /// </summary>
    public async Task<double?> ValueAtQueryTimestamp(
        LatestSnapshotCacheDataLoader latestDataLoader,
        SnapshotByTimestampBatchDataLoader snapshotByTimestampBatchDataLoader,
        [Service] IMachineSnapshotService service,
        CancellationToken cancellationToken)
    {
        DataResult<SnapshotValue> result;
        if (_machineQueryTimestamp is null)
        {
            if (_endTime is null)
                result = await service.GetLatestColumnValue(latestDataLoader, _columnId, _machineId, cancellationToken);
            else
                result = new DataResult<SnapshotValue>(value: null, exception: null);
        }
        else
        {
            result = await service.GetColumnValue(
                snapshotByTimestampBatchDataLoader,
                _columnId,
                _machineQueryTimestamp.Value,
                _machineId,
                cancellationToken);
        }

        if (result.Exception is not null)
            throw result.Exception;

        return result.Value?.ColumnValue is not null
            ? Convert.ToDouble(result.Value?.ColumnValue)
            : null;
    }

    /// <summary>
    /// The value with the longest duration during the job/roll/time-span in SI unit.
    /// Because of performance reasons, it is not possible to subscribe to this property (-> returns 'null').
    /// </summary>
    public async Task<double?> ValueWithLongestDuration(
        SnapshotValuesWithLongestDurationBatchDataLoader dataLoader,
        [Service] IHttpContextAccessor context,
        [Service] IMachineSnapshotService service,
        CancellationToken cancellationToken)
    {
        if (_timeRanges is null || context.HttpContext.IsSubscriptionOrNull())
        {
            return null;
        }

        var dataResult = await service.GetValueWithLongestDuration(
            dataLoader,
            _machineId,
            _columnId,
            _timeRanges.Select(timeRange => (WuH.Ruby.Common.Core.TimeRange)timeRange).ToList(),
            cancellationToken);

        if (dataResult.Exception is not null)
        {
            throw dataResult.Exception;
        }

        return dataResult.Value is not null
            ? Convert.ToDouble(dataResult.Value)
            : null;
    }

    /// <summary>
    /// All distinct values that were present during the job/roll/time-span in SI units.
    /// Because of performance reasons, it is not possible to subscribe to this property (-> returns 'null').
    /// </summary>
    public async Task<IEnumerable<double?>?> DistinctValues(
        SnapshotDistinctValuesBatchDataLoader dataLoader,
        [Service] IHttpContextAccessor context,
        [Service] IMachineSnapshotService service,
        CancellationToken cancellationToken,
        int limit = 100)
    {
        if (_timeRanges is null || context.HttpContext.IsSubscriptionOrNull())
        {
            return new List<double?>();
        }

        var dataResult = await service.GetDistinct(
            dataLoader,
            _machineId,
            _columnId,
            _timeRanges.Select(timeRange => (WuH.Ruby.Common.Core.TimeRange)timeRange).ToList(),
            limit,
            cancellationToken
        );

        if (dataResult.Exception is not null)
        {
            throw dataResult.Exception;
        }

        return dataResult.Value?.Select<object?, double?>(source => source is null ? null : Convert.ToDouble(source));
    }

    /// <summary>
    /// The average (arithmetic mean) value during the job/roll/time-span in SI unit.
    /// Because of performance reasons, it is not possible to subscribe to this property (-> returns 'null').
    /// </summary>
    public async Task<double?> AverageValue(
        SnapshotArithmeticMeanBatchDataLoader dataLoader,
        [Service] IHttpContextAccessor context,
        [Service] IMachineSnapshotService service,
        CancellationToken cancellationToken)
    {
        if (_timeRanges is null || context.HttpContext.IsSubscriptionOrNull())
        {
            return null;
        }

        var result = await service.GetArithmeticMean(
            dataLoader,
            _machineId,
            _columnId,
            _timeRanges.Select(timeRange => (WuH.Ruby.Common.Core.TimeRange)timeRange).ToList(),
            cancellationToken
        );

        if (result.Exception is not null)
        {
            throw result.Exception;
        }

        return result.Value;
    }

    /// <summary>
    /// The standard deviation of all values during the job/roll/time-span in SI unit.
    /// Because of performance reasons, it is not possible to subscribe to this property (-> returns 'null').
    /// </summary>
    public async Task<double?> StandardDeviationValue(
        SnapshotStandardDeviationBatchDataLoader dataLoader,
        [Service] IHttpContextAccessor context,
        [Service] IMachineSnapshotService service,
        CancellationToken cancellationToken)
    {
        if (_timeRanges is null || context.HttpContext.IsSubscriptionOrNull())
        {
            return null;
        }

        var result = await service.GetStandardDeviation(
            dataLoader,
            _machineId,
            _columnId,
            _timeRanges.Select(timeRange => (WuH.Ruby.Common.Core.TimeRange)timeRange).ToList(),
            cancellationToken);

        if (result.Exception is not null)
        {
            throw result.Exception;
        }

        return result.Value;
    }

    /// <summary>
    /// The minimum value across all values during the job/roll/time-span in SI unit.
    /// Because of performance reasons, it is not possible to subscribe to this property (-> returns 'null').
    /// </summary>
    public async Task<double?> MinValue(
        SnapshotMinBatchDataLoader dataLoader,
        [Service] IHttpContextAccessor context,
        [Service] IMachineSnapshotService service,
        CancellationToken cancellationToken)
    {
        if (_timeRanges is null || context.HttpContext.IsSubscriptionOrNull())
        {
            return null;
        }

        var result = await service.GetMin(
            dataLoader,
            _machineId,
            _columnId,
            _timeRanges.Select(timeRange => (WuH.Ruby.Common.Core.TimeRange)timeRange).ToList(),
            cancellationToken);

        if (result.Exception is not null)
        {
            throw result.Exception;
        }

        return result.Value;
    }

    /// <summary>
    /// The maximal value across all values during the job/roll/time-span in SI unit.
    /// Because of performance reasons, it is not possible to subscribe to this property (-> returns 'null').
    /// </summary>
    public async Task<double?> MaxValue(
        SnapshotMaxBatchDataLoader dataLoader,
        [Service] IHttpContextAccessor context,
        [Service] IMachineSnapshotService service,
        CancellationToken cancellationToken)
    {
        if (_timeRanges is null || context.HttpContext.IsSubscriptionOrNull())
        {
            return null;
        }

        var result = await service.GetMax(
            dataLoader,
            _machineId,
            _columnId,
            _timeRanges.Select(timeRange => (WuH.Ruby.Common.Core.TimeRange)timeRange).ToList(),
            cancellationToken);

        if (result.Exception is not null)
        {
            throw result.Exception;
        }

        return result.Value;
    }

    /// <summary>
    /// The value with the longest duration during the job/roll/time-span in SI unit.
    /// Because of performance reasons, it is not possible to subscribe to this property (-> returns 'null').
    /// </summary>‚
    public async Task<double?> Median(
        SnapshotMedianValuesBatchDataLoader dataLoader,
        [Service] IHttpContextAccessor context,
        [Service] IMachineSnapshotService service,
        CancellationToken cancellationToken)
    {
        if (_timeRanges is null || context.HttpContext.IsSubscriptionOrNull())
        {
            return null;
        }

        var dataResult = await service.GetMedian(
            dataLoader,
            _machineId,
            _columnId,
            _timeRanges.Select(timeRange => (WuH.Ruby.Common.Core.TimeRange)timeRange).ToList(),
            cancellationToken);

        if (dataResult.Exception is not null)
        {
            throw dataResult.Exception;
        }

        return dataResult.Value;
    }

    /// <summary>
    /// The trend of the values during the job/roll/time-span in SI unit.
    /// It is represented by trend elements each consisting of a time and a value.
    /// The trend elements are sorted by the time (ascending).
    /// The trend will contain one element for each minute in the job/roll/time-span.
    /// If a job or roll with multiple time ranges is queried this trend contains also the data between these time ranges.
    /// Because of performance reasons, it is not possible to subscribe to this property (-> returns 'null').
    /// </summary>
    public async Task<IEnumerable<NumericTrendElement>?> Trend(
        MachineTrendByTimeRangeBatchDataLoader dataLoader,
        [Service] IMachineSnapshotService service,
        [Service] IHttpContextAccessor context,
        CancellationToken cancellationToken)
    {
        if (context.HttpContext.IsSubscriptionOrNull())
            return null;

        return await service.GetNumericColumnTrend(
            dataLoader,
            _columnId,
            _timeRanges?.Select(timeRange => (WuH.Ruby.Common.Core.TimeRange)timeRange).ToList(),
            _machineId,
            cancellationToken);
    }

    /// <summary>
    /// The unit of the values
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
}