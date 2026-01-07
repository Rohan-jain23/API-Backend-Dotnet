using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.DataLoaders;

namespace FrameworkAPI.Services.Settings;

public interface IGlobalSettingsService
{
    Task<string?> GetString(
        GlobalSettingsBatchLoader globalSettingsBatchLoader,
        string? machineId,
        string settingId,
        string? fallbackValue = null,
        CancellationToken cancellationToken = default);

    Task<bool?> GetBoolean(
        GlobalSettingsBatchLoader globalSettingsBatchLoader,
        string? machineId,
        string settingId,
        bool? fallbackValue = null,
        CancellationToken cancellationToken = default);

    Task<string?> Change(
        string? machineId, string settingId, string? value, CancellationToken cancellationToken = default);
}