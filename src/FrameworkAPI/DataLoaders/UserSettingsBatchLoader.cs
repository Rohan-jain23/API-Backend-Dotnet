using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.Exceptions;
using GreenDonut;
using Microsoft.AspNetCore.Http;
using WuH.Ruby.Settings.Client;
using DataResult = FrameworkAPI.Models.DataResult<string>;

namespace FrameworkAPI.DataLoaders;

public class UserSettingsBatchLoader : BatchDataLoader<(string UserId, string SettingId, string? MachineId), DataResult>
{
    private readonly ISettingsService _settingsHttpClient;

    public UserSettingsBatchLoader(
        ISettingsService settingsHttpClient,
        IBatchScheduler batchScheduler,
        DataLoaderOptions? options = null)
        : base(batchScheduler, options)
    {
        ArgumentNullException.ThrowIfNull(settingsHttpClient);
        _settingsHttpClient = settingsHttpClient;
    }

    protected override async Task<IReadOnlyDictionary<(string UserId, string SettingId, string? MachineId), DataResult>>
        LoadBatchAsync(
            IReadOnlyList<(string UserId, string SettingId, string? MachineId)> keys,
            CancellationToken cancellationToken)
    {
        var result = new ConcurrentDictionary<(string UserId, string SettingId, string? MachineId), DataResult>();

        await Parallel.ForEachAsync(keys, cancellationToken, async (key, _) =>
        {
            var userId = key.UserId;
            var settingId = key.SettingId;
            var machineId = key.MachineId;

            // Request data for each key
            var response = await _settingsHttpClient.GetSettingsForUserAndMachine(
                machineId,
                userId,
                settingId,
                cancellationToken);

            // Assign value to each requested key
            if (response.HasError)
            {
                var exception = response.Error.StatusCode == StatusCodes.Status204NoContent
                    ? null
                    : new InternalServiceException(response.Error);
                result.TryAdd(
                    key: (userId, settingId, machineId),
                    value: new DataResult(value: null, exception: exception));

                return;
            }

            result.TryAdd(
                key: (userId, settingId, machineId),
                value: new DataResult(value: response.Item.Value, exception: null));
        });

        return result;
    }
}