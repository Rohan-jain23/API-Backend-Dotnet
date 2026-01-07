using System;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.DataLoaders;

namespace FrameworkAPI.Services.Settings;

public interface IUserSettingsService
{
    Task<string?> GetString(
        UserSettingsBatchLoader userSettingsBatchLoader,
        string userId,
        string? machineId,
        string settingId,
        string? fallbackValue = null,
        CancellationToken cancellationToken = default);

    Task<object?> GetAndParse(
        UserSettingsBatchLoader userSettingsBatchLoader,
        string userId,
        string? machineId,
        string settingId,
        Func<string, object?> parseValueFunc,
        object? fallbackValue = null,
        CancellationToken cancellationToken = default);

    Task<string?> Change(
        string userId,
        string? machineId,
        string settingId,
        string? value,
        CancellationToken cancellationToken = default);
}