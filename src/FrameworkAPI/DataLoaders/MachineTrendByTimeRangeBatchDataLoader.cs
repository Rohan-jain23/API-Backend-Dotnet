using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.Extensions;
using FrameworkAPI.Models;
using GreenDonut;
using WuH.Ruby.Common.Core;
using WuH.Ruby.MachineSnapShooter.Client;

namespace FrameworkAPI.DataLoaders;

public class MachineTrendByTimeRangeBatchDataLoader :
    BatchDataLoader<(string MachineId, TimeRange timeRange, string columnId), DataResult<IDictionary<DateTime, object?>>>
{
    private readonly IMachineSnapshotHttpClient _machineSnapshotHttpClient;

    public MachineTrendByTimeRangeBatchDataLoader(
        IMachineSnapshotHttpClient machineSnapshotHttpClient,
        IBatchScheduler batchScheduler,
        DataLoaderOptions? options = null)
        : base(batchScheduler, options)
    {
        ArgumentNullException.ThrowIfNull(machineSnapshotHttpClient);
        _machineSnapshotHttpClient = machineSnapshotHttpClient;
    }

    protected override async Task<IReadOnlyDictionary<(string MachineId, TimeRange timeRange, string columnId), DataResult<IDictionary<DateTime, object?>>>>
        LoadBatchAsync(
            IReadOnlyList<(string MachineId, TimeRange timeRange, string columnId)> keys, CancellationToken cancellationToken)
    {
        var machineGroups = keys
            .GroupBy(key => key.MachineId)
            .ToDictionary(
                machineGrouping => machineGrouping.Key,
                machineGrouping => LoadBatchForMachineAsync(
                machineGrouping.Key,
                machineGrouping
                    .Select(key => (key.timeRange, key.columnId))
                    .ToImmutableList(),
                cancellationToken));

        await Task.WhenAll(machineGroups.Values);

        return keys.ToDictionary(
            key => key,
            key =>
            {
                var machineTrendResponse = machineGroups[key.MachineId].Result;
                if (machineTrendResponse.HasError)
                {
                    return new DataResult<IDictionary<DateTime, object?>>(
                        null,
                        machineTrendResponse.Error.Exception ??
                            new Exception($"Failed to load trend data. {machineTrendResponse.Error.ErrorMessage}"));
                }

                return new DataResult<IDictionary<DateTime, object?>>(
                    machineTrendResponse.Item[key.columnId]
                        .Where(val => key.timeRange.From <= val.Key && key.timeRange.To >= val.Key)
                        .ToDictionary(val => val.Key, val => val.Value),
                    null);
            });
    }

    private async Task<InternalItemResponse<IDictionary<string, Dictionary<DateTime, object?>>>>
        LoadBatchForMachineAsync(string machineId, IReadOnlyList<(TimeRange timeRange, string columnId)> keys, CancellationToken cancellationToken)
    {
        var timeRanges = keys
            .Select(key => key.timeRange)
            .Flatten()
            .ToList();

        var columnIds = keys
            .Select(key => key.columnId)
            .Distinct()
            .ToList();

        var response = await _machineSnapshotHttpClient.GetSnapshotsInTimeRanges(machineId, columnIds, timeRanges, [], cancellationToken);

        if (response.HasError)
            return new InternalItemResponse<IDictionary<string, Dictionary<DateTime, object?>>>(response.Error);

        var result = columnIds
            .ToDictionary(
                columnId => columnId,
                columnId => response.Item.Data.ToDictionary(
                    snapshot => snapshot.SnapshotTime,
                    snapshot => snapshot.ColumnValues.FirstOrDefault(colVal => colVal.Id == columnId)?.Value));

        return new InternalItemResponse<IDictionary<string, Dictionary<DateTime, object?>>>(result);
    }
}