using System;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.DataLoaders;
using FrameworkAPI.Exceptions;
using WuH.Ruby.Settings.Client;

namespace FrameworkAPI.Services.Settings;

public class UserSettingsService(ISettingsService settingsHttpClient) : IUserSettingsService
{
    private readonly ISettingsService _settingsHttpClient = settingsHttpClient;

    public async Task<string?> GetString(
        UserSettingsBatchLoader userSettingsBatchLoader,
        string userId,
        string? machineId,
        string settingId,
        string? fallbackValue = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new UserIdNotFoundException();
        }

        if (machineId is not null && string.IsNullOrWhiteSpace(machineId))
        {
            throw new ArgumentException($"{nameof(machineId)} \"{machineId}\" is no valid machine id.");
        }

        var (value, exception) =
            await userSettingsBatchLoader.LoadAsync((userId, settingId, machineId), cancellationToken);

        if (exception is not null)
        {
            throw exception;
        }

        return value ?? fallbackValue;
    }

    public async Task<object?> GetAndParse(
        UserSettingsBatchLoader userSettingsBatchLoader,
        string userId,
        string? machineId,
        string settingId,
        Func<string, object?> parseValueFunc,
        object? fallbackValue = null,
        CancellationToken cancellationToken = default)
    {
        var value = await GetString(
            userSettingsBatchLoader, userId, machineId, settingId, cancellationToken: cancellationToken);
        return value is null ? fallbackValue : parseValueFunc(value);
    }

    public async Task<string?> Change(
        string userId, string? machineId, string settingId, string? value, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new UserIdNotFoundException();
        }

        if (string.IsNullOrWhiteSpace(settingId))
        {
            throw new ArgumentException($"{nameof(settingId)} \"{settingId}\" is no valid settings id.");
        }
        if (machineId is not null && string.IsNullOrWhiteSpace(machineId))
        {
            throw new ArgumentException($"{nameof(machineId)} \"{machineId}\" is no valid machine id.");
        }

        var response = await _settingsHttpClient.PostSettingsForUserAndMachine(
            machineId,
            userId,
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