using System;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.DataLoaders;
using FrameworkAPI.Exceptions;
using WuH.Ruby.Settings.Client;

namespace FrameworkAPI.Services.Settings;

public class GlobalSettingsService(ISettingsService settingsHttpClient) : IGlobalSettingsService
{
    private readonly ISettingsService _settingsHttpClient = settingsHttpClient;

    public async Task<string?> GetString(
        GlobalSettingsBatchLoader globalSettingsBatchLoader,
        string? machineId,
        string settingId,
        string? fallbackValue = null,
        CancellationToken cancellationToken = default)
    {
        if (machineId is not null && string.IsNullOrWhiteSpace(machineId))
        {
            throw new ArgumentException($"{nameof(machineId)} \"{machineId}\" is no valid machine id.");
        }

        var (value, exception) = await globalSettingsBatchLoader.LoadAsync((settingId, machineId), cancellationToken);

        if (exception is not null)
        {
            throw exception;
        }

        return value ?? fallbackValue;
    }

    public async Task<bool?> GetBoolean(
        GlobalSettingsBatchLoader globalSettingsBatchLoader,
        string? machineId,
        string settingId,
        bool? fallbackValue = null,
        CancellationToken cancellationToken = default)
    {
        if (machineId is not null && string.IsNullOrWhiteSpace(machineId))
        {
            throw new ArgumentException($"{nameof(machineId)} \"{machineId}\" is no valid machine id.");
        }

        var value = await GetString(
            globalSettingsBatchLoader,
            machineId,
            settingId,
            cancellationToken: cancellationToken);

        return bool.TryParse(value, out var valueAsBool) ? valueAsBool : fallbackValue;
    }

    public Task<string?> Change(
        string? machineId, string settingId, string? value, CancellationToken cancellationToken = default)
    {
        if (machineId is not null && string.IsNullOrWhiteSpace(machineId))
        {
            throw new ArgumentException($"{nameof(machineId)} \"{machineId}\" is no valid machine id.");
        }

        return PostSetting(machineId, settingId, value, cancellationToken);
    }

    private async Task<string?> PostSetting(string? machineId, string settingId, string? value, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(settingId))
        {
            throw new ArgumentException($"{nameof(settingId)} \"{settingId}\" is no valid settings id.");
        }

        var response = await _settingsHttpClient.PostGlobalSettings(
            machineId,
            settingId,
            value,
            cancellationToken
        );

        if (response.HasError)
        {
            throw new InternalServiceException(response.Error.ErrorMessage, response.Error.StatusCode);
        }

        return value;
    }
}