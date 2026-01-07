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
/// Generic classes need to be described via a "GraphQLDescription" attribute,
/// because the summary will not appear in the GraphQL documentation on the normal way.
/// </summary>
/// <typeparam name="T">Data type of the variable.</typeparam>
[GraphQLDescription(
    "The values of a variable/sub-entity (for example a string set value, or (in case of rolls) the corresponding produced job) can be changed during production of a job/roll/etc." +
    "In these cases it is not always easy to determine just one single value for the variable that stands for the whole job/roll/etc." +
    "This data model provides more information, so the user might get an indication if the value was changed during the job/roll/etc." +
    "This is only supported for values that can be derived from MachineSnapshots.")]
public class SnapshotValuesDuringProduction<T>(
    string columnId,
    DateTime? endTime,
    string machineId,
    IEnumerable<TimeRange>? timeRanges,
    DateTime? machineQueryTimestamp,
    Func<object?, T?>? optionalMapper = null)
{
    [GraphQLDescription("The value on the end of the job/roll/etc in SI unit." +
                        "If the job/roll/etc is still active, this is the current live-value.")]
    public async Task<T?> LastValue(
        LatestSnapshotCacheDataLoader latestDataLoader,
        SnapshotByTimestampBatchDataLoader timestampDataLoader,
        [Service] IMachineSnapshotService service,
        CancellationToken ct)
    {
        var (unmappedValue, exception) = endTime is null
            ? await service.GetLatestColumnValue(latestDataLoader, columnId, machineId, ct)
            : await service.GetColumnValue(timestampDataLoader, columnId, endTime.Value, machineId, ct);

        if (exception is not null)
        {
            throw exception;
        }

        return optionalMapper is null ? (T?)unmappedValue!.ColumnValue : optionalMapper(unmappedValue!.ColumnValue);
    }

    /// <summary>
    /// If the job/roll was queried via the machine query, this returns the value at the query timestamp.
    /// If the query timestamp is 'null' and the job/roll is active, this is the live-value.
    /// Otherwise (a completed job/roll/time-span is queried via another query), this value is 'null'.
    /// </summary>
    public async Task<T?> ValueAtQueryTimestamp(
        LatestSnapshotCacheDataLoader latestDataLoader,
        SnapshotByTimestampBatchDataLoader snapshotByTimestampBatchDataLoader,
        [Service] IMachineSnapshotService service,
        CancellationToken cancellationToken)
    {
        DataResult<SnapshotValue> result;
        if (machineQueryTimestamp is null)
        {
            if (endTime is null)
                result = await service.GetLatestColumnValue(latestDataLoader, columnId, machineId, cancellationToken);
            else
                result = new DataResult<SnapshotValue>(value: null, exception: null);
        }
        else
        {
            result = await service.GetColumnValue(
                snapshotByTimestampBatchDataLoader,
                columnId,
                machineQueryTimestamp.Value,
                machineId,
                cancellationToken);
        }

        if (result.Exception is not null)
            throw result.Exception;

        return optionalMapper is null ? (T?)result.Value?.ColumnValue : optionalMapper(result.Value?.ColumnValue);
    }

    /// <summary>
    /// See GraphQL description.
    /// </summary>
    [GraphQLDescription("The value with the longest duration during the job/roll/etc in SI unit. " +
                        "Because of performance reasons, it is not possible to subscribe to this property (-> returns 'null').")]
    public async Task<T?> ValueWithLongestDuration(
        SnapshotValuesWithLongestDurationBatchDataLoader dataLoader,
        [Service] IHttpContextAccessor context,
        [Service] IMachineSnapshotService service,
        CancellationToken cancellationToken)
    {
        if (timeRanges is null || context.HttpContext.IsSubscriptionOrNull())
        {
            return default;
        }

        var dataResult = await service.GetValueWithLongestDuration(
            dataLoader,
            machineId,
            columnId,
            timeRanges.Select(timeRange => (WuH.Ruby.Common.Core.TimeRange)timeRange).ToList(),
            cancellationToken);

        if (dataResult.Exception is not null)
        {
            throw dataResult.Exception;
        }

        return optionalMapper is null
            ? (T?)dataResult.Value
            : optionalMapper(dataResult.Value);
    }

    /// <summary>
    /// See GraphQL description.
    /// </summary>
    [GraphQLDescription("All distinct values in SI unit that were active during the job/roll/etc." +
                        "Because of performance reasons, it is not possible to subscribe to this property (-> returns 'null').")]
    public async Task<IEnumerable<T?>?> DistinctValues(
        SnapshotDistinctValuesBatchDataLoader dataLoader,
        [Service] IHttpContextAccessor context,
        [Service] IMachineSnapshotService service,
        CancellationToken cancellationToken,
        int limit = 100)
    {
        if (timeRanges is null || context.HttpContext.IsSubscriptionOrNull())
        {
            return new List<T?>();
        }

        var dataResult = await service.GetDistinct(
            dataLoader,
            machineId,
            columnId,
            timeRanges.Select(timeRange => (WuH.Ruby.Common.Core.TimeRange)timeRange).ToList(),
            limit,
            cancellationToken
        );

        if (dataResult.Exception is not null)
        {
            throw dataResult.Exception;
        }

        return optionalMapper is null
            ? dataResult.Value?.ConvertAll(distinctValue => (T?)distinctValue)
            : dataResult.Value?.ConvertAll(distinctValue => optionalMapper(distinctValue));
    }
}