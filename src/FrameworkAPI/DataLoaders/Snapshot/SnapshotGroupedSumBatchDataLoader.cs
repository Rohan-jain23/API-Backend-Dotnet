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
using WuH.Ruby.MachineSnapShooter.Client.Models;

namespace FrameworkAPI.DataLoaders;

public class SnapshotGroupedSumBatchDataLoader : BatchDataLoader<GroupedSumRequestKey, DataResult<GroupedSumByIdentifier?>>
{
    private readonly IMachineSnapshotHttpClient _machineSnapshotHttpClient;

    public SnapshotGroupedSumBatchDataLoader(
        IMachineSnapshotHttpClient machineSnapshotHttpClient,
        IBatchScheduler batchScheduler,
        DataLoaderOptions? options = null) : base(batchScheduler, options)
    {
        ArgumentNullException.ThrowIfNull(machineSnapshotHttpClient);
        _machineSnapshotHttpClient = machineSnapshotHttpClient;
    }

    protected override async Task<IReadOnlyDictionary<GroupedSumRequestKey, DataResult<GroupedSumByIdentifier?>>> LoadBatchAsync(
        IReadOnlyList<GroupedSumRequestKey> groupedSumRequestKeys,
        CancellationToken cancellationToken)
    {
        // Group requested keys
        var batches = AggregationBatchHelper.GroupRequestKeysIntoBatches(groupedSumRequestKeys);

        // Request data for grouped Keys
        var snapshotClientResponseTasks = batches
            .Select(async group =>
            {
                var machineId = group.MachineId;
                var response = await _machineSnapshotHttpClient.GetGroupedSums(
                    machineId,
                    group.GroupAssignments.ToList(),
                    group.TimeRanges
                        .Select(timeRange => new WuH.Ruby.Common.Core.TimeRange(timeRange.From, timeRange.To)).ToList(),
                    [],
                    cancellationToken);
                return (group, response);
            });

        var snapshotClientResponses = await Task.WhenAll(snapshotClientResponseTasks);

        // Assign Value to each requested key and return
        return AggregationBatchHelper.AssignAvailableClientResponsesToEachRequestKey(groupedSumRequestKeys, snapshotClientResponses);
    }
}