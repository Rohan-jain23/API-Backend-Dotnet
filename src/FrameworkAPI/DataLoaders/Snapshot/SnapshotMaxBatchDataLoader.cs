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

public class SnapshotMaxBatchDataLoader : BatchDataLoader<SnapshotValueRequestKey, DataResult<double?>>
{
    private readonly IMachineSnapshotHttpClient _machineSnapshotHttpClient;

    public SnapshotMaxBatchDataLoader(
        IMachineSnapshotHttpClient machineSnapshotHttpClient,
        IBatchScheduler batchScheduler,
        DataLoaderOptions? options = null) : base(batchScheduler, options)
    {
        ArgumentNullException.ThrowIfNull(machineSnapshotHttpClient);
        _machineSnapshotHttpClient = machineSnapshotHttpClient;
    }

    protected override async Task<IReadOnlyDictionary<SnapshotValueRequestKey, DataResult<double?>>> LoadBatchAsync(
        IReadOnlyList<SnapshotValueRequestKey> keys,
        CancellationToken cancellationToken)
    {
        // Group requested keys
        var batches = AggregationBatchHelper.GroupRequestKeysIntoBatches<SnapshotValueRequestBatch>(keys);

        // Request data for grouped Keys
        var snapshotClientResponseTasks = batches
            .Select(async group =>
            {
                var response = await _machineSnapshotHttpClient.GetMaxValues(
                    group.MachineId,
                    group.ColumnIds.ToList(),
                    group.TimeRanges.Select(range => new WuH.Ruby.Common.Core.TimeRange(range.From, range.To)).ToList(),
                    [],
                    cancellationToken);

                return (group, response);
            });

        var snapshotClientResponses = await Task.WhenAll(snapshotClientResponseTasks);

        // Assign Value to each requested key and return
        return AggregationBatchHelper.AssignClientResponsesToEachRequestKey(keys, snapshotClientResponses);
    }
}