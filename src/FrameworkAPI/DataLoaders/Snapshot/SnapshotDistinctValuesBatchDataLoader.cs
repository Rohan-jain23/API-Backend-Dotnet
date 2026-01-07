using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.Helpers;
using FrameworkAPI.Models;
using FrameworkAPI.Models.DataLoader;
using GreenDonut;
using WuH.Ruby.MachineSnapShooter.Client;

namespace FrameworkAPI.DataLoaders;

public class SnapshotDistinctValuesBatchDataLoader : BatchDataLoader<SnapshotValueWithLimitRequestKey, DataResult<List<object?>?>>
{
    private readonly IMachineSnapshotHttpClient _machineSnapshotHttpClient;

    public SnapshotDistinctValuesBatchDataLoader(
        IMachineSnapshotHttpClient machineSnapshotHttpClient,
        IBatchScheduler batchScheduler,
        DataLoaderOptions? options = null)
        : base(batchScheduler, options)
    {
        ArgumentNullException.ThrowIfNull(machineSnapshotHttpClient);
        _machineSnapshotHttpClient = machineSnapshotHttpClient;
    }

    protected override async Task<IReadOnlyDictionary<SnapshotValueWithLimitRequestKey, DataResult<List<object?>?>>> LoadBatchAsync(
        IReadOnlyList<SnapshotValueWithLimitRequestKey> keys,
        CancellationToken cancellationToken)
    {
        // Group requested keys
        var batches = AggregationBatchHelper.GroupRequestKeysIntoBatches<SnapshotValueWithLimitRequestBatch>(keys);

        // Request data for grouped Keys
        var snapshotClientResponseTasks = batches
            .Select(async batch =>
            {
                var machineId = batch.MachineId;
                var response = await _machineSnapshotHttpClient.GetDistinctValues(
                    machineId,
                    batch.ColumnIds.ToList(),
                    batch.TimeRanges
                        .Select(timeRange => new WuH.Ruby.Common.Core.TimeRange(timeRange.From, timeRange.To)).ToList(),
                    batch.Limit,
                    [],
                    cancellationToken);
                return (group: batch, response);
            });

        var snapshotClientResponse = await Task.WhenAll(snapshotClientResponseTasks);

        // Assign value to each requested key
        return AggregationBatchHelper.AssignClientResponsesToEachRequestKey(keys, snapshotClientResponse);
    }
}