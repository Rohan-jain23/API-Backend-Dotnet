using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.Exceptions;
using GreenDonut;
using Microsoft.AspNetCore.Http;
using WuH.Ruby.Settings.Client;
using DataResult = FrameworkAPI.Models.DataResult<string>;

namespace FrameworkAPI.DataLoaders;

public class GlobalSettingsBatchLoader : BatchDataLoader<(string SettingId, string? MachineId), DataResult>
{
    private readonly ISettingsService _settingsHttpClient;

    public GlobalSettingsBatchLoader(
        ISettingsService settingsHttpClient,
        IBatchScheduler batchScheduler,
        DataLoaderOptions? options = null)
        : base(batchScheduler, options)
    {
        ArgumentNullException.ThrowIfNull(settingsHttpClient);
        _settingsHttpClient = settingsHttpClient;
    }

    protected override async Task<IReadOnlyDictionary<(string SettingId, string? MachineId), DataResult>>
        LoadBatchAsync(
            IReadOnlyList<(string SettingId, string? MachineId)> keys,
            CancellationToken cancellationToken)
    {
        // Group requested keys
        var machineGroups = keys
            .GroupBy(key => key.MachineId ?? "")
            .ToDictionary(
                group => group.Key,
                group => group.Select(x => x.SettingId).ToList());

        var result = new ConcurrentDictionary<(string SettingId, string? MachineId), DataResult>();

        // Request data for grouped keys
        await Parallel.ForEachAsync(machineGroups, cancellationToken, async (machineGroup, _) =>
        {
            var machineId = string.IsNullOrWhiteSpace(machineGroup.Key) ? null : machineGroup.Key;
            var settingIds = machineGroup.Value;

            var response = await _settingsHttpClient.GetGlobalSettings(
                machineId: machineId,
                settingIds.ToList(),
                cancellationToken);

            // Assign value to each requested key
            if (response.HasError)
            {
                if (response.Error.StatusCode == StatusCodes.Status204NoContent)
                {
                    foreach (var settingId in settingIds)
                    {
                        result.TryAdd(key: (settingId, machineId), value: new DataResult(value: null, exception: null));
                    }
                }

                var exception = new InternalServiceException(response.Error);

                foreach (var settingId in settingIds)
                {
                    result.TryAdd(key: (settingId, machineId), value: new DataResult(value: null, exception));
                }

                return;
            }

            foreach (var settingId in settingIds)
            {
                var value = response.Items.FirstOrDefault(i => i.SettingId == settingId)?.Value;
                result.TryAdd(
                    key: (settingId, machineId),
                    value: new DataResult(value, exception: null));
            }
        });

        return result;
    }
}