using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.Exceptions;
using GreenDonut;
using Microsoft.Extensions.Logging;
using WuH.Ruby.Common.Core;
using WuH.Ruby.MachineSnapShooter.Client;
using WuH.Ruby.MachineSnapShooter.Client.Models;
using DataResult = FrameworkAPI.Models.DataResult<WuH.Ruby.MachineSnapShooter.Client.Models.SnapshotDto>;

namespace FrameworkAPI.DataLoaders;

public class
    SnapshotByTimestampBatchDataLoader : BatchDataLoader<(string MachineId, DateTime Timestamp), DataResult>
{
    private readonly IMachineSnapshotHttpClient _client;
    private readonly ILogger<SnapshotByTimestampBatchDataLoader> _logger;

    public SnapshotByTimestampBatchDataLoader(
        IMachineSnapshotHttpClient client,
        IBatchScheduler batchScheduler,
        ILogger<SnapshotByTimestampBatchDataLoader> logger,
        DataLoaderOptions? options = null)
        : base(batchScheduler, options)
    {
        ArgumentNullException.ThrowIfNull(client);
        _client = client;
        _logger = logger;
    }

    protected override async Task<IReadOnlyDictionary<(string MachineId, DateTime Timestamp), DataResult>>
        LoadBatchAsync(IReadOnlyList<(string MachineId, DateTime Timestamp)> keys, CancellationToken cancellationToken)
    {
        var machineIdToTaskDictionary =
            new Dictionary<string, Task<InternalItemResponse<MachineSnapshotForTimestampListResponse>>>();

        foreach (var group in keys.GroupBy(key => key.MachineId))
        {
            // Group requested keys
            var machineId = group.Key;
            var timestamps = group.Select(keyValuePair => keyValuePair.Timestamp).ToList();

            // Request data for grouped keys
            machineIdToTaskDictionary.Add(
                machineId, GetSnapshotsForTimestampListResponse(machineId, timestamps, cancellationToken));
        }

        await Task.WhenAll(machineIdToTaskDictionary.Values);

        var keyToSnapshotDtoDictionary = new Dictionary<(string MachineId, DateTime Timestamp), DataResult>();

        // Assign value to each requested key
        foreach (var key in keys)
        {
            // We don't really have to await the task here again (because we are calling "Task.WhenAll(...)" above)
            // but the static analyzer will raise a warning (false positive) otherwise and awaiting here is fairly cheap
            var response = await machineIdToTaskDictionary[key.MachineId];

            if (response.HasError)
            {
                keyToSnapshotDtoDictionary.Add(
                    key, new DataResult(value: null, new InternalServiceException(response.Error)));
                continue;
            }

            var snapshotDto = response.Item.Data.Single(snapshotForTimestampDto =>
                snapshotForTimestampDto.RequestedTimestamp == key.Timestamp).Snapshot;
            keyToSnapshotDtoDictionary.Add(key, new DataResult(snapshotDto, exception: null));
        }

        return keyToSnapshotDtoDictionary;
    }

    private async Task<InternalItemResponse<MachineSnapshotForTimestampListResponse>>
        GetSnapshotsForTimestampListResponse(
            string machineId, IEnumerable<DateTime> timestamps, CancellationToken cancellationToken)
    {
        var timestampsAsList = timestamps.ToList();
        var response = await _client.GetSnapshotsForTimestamps(
            machineId,
            timestampsAsList,
            null,
            cancellationToken);

        if (!response.HasError && response.Item.Data.Count != timestampsAsList.Count)
        {
            _logger.LogWarning(
                $"{machineId}: Requested {timestampsAsList.Count} timestamps from SnapShooter but {response.Item.Data.Count} were returned.");
        }

        return response;
    }
}