using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.DataLoaders;
using FrameworkAPI.Models.Settings;
using FrameworkAPI.Services.Settings;
using HotChocolate;

namespace FrameworkAPI.Schema.Settings;

/// <summary>
/// Settings for the whole RUBY instance.
/// </summary>
public class GlobalSettings
{
    /// <summary>
    /// Friendly name of the RUBY instance.
    /// [Source: Setting in Admin]
    /// </summary>
    public async Task<string?> RubyFriendlyName(
        GlobalSettingsBatchLoader globalSettingsBatchLoader,
        [Service] IGlobalSettingsService globalSettingsService,
        CancellationToken cancellationToken) =>
        await globalSettingsService.GetString(
            globalSettingsBatchLoader,
            machineId: null,
            GlobalSettingIds.RubyFriendlyName,
            cancellationToken: cancellationToken);

    /// <summary>
    /// Time zone of the RUBY instance and (if everything is set-up properly) of the connected machines.
    /// [Source: Setting in Admin]
    /// </summary>
    public async Task<string?> RubyTimeZone(
        GlobalSettingsBatchLoader globalSettingsBatchLoader,
        [Service] IGlobalSettingsService globalSettingsService,
        CancellationToken cancellationToken) =>
        await globalSettingsService.GetString(
            globalSettingsBatchLoader,
            machineId: null,
            GlobalSettingIds.RubyTimeZoneInfoTimeZone,
            cancellationToken: cancellationToken);

    /// <summary>
    /// Flag if the time zone is in daylight saving time.
    /// [Source: Setting in Admin]
    /// </summary>
    public async Task<bool?> RubyTimeZoneIsDayLightSavingTime(
        GlobalSettingsBatchLoader globalSettingsBatchLoader,
        [Service] IGlobalSettingsService globalSettingsService,
        CancellationToken cancellationToken) =>
        await globalSettingsService.GetBoolean(
            globalSettingsBatchLoader,
            settingId: GlobalSettingIds.RubyTimeZoneInfoDayLightSavingTime,
            machineId: null,
            cancellationToken: cancellationToken);

    /// <summary>
    /// NTP server to sync the time.
    /// [Source: Setting in Admin]
    /// </summary>
    public async Task<string?> RubyTimeZoneIpAddressWithPort(
        GlobalSettingsBatchLoader globalSettingsBatchLoader,
        [Service] IGlobalSettingsService globalSettingsService,
        CancellationToken cancellationToken) =>
        await globalSettingsService.GetString(
            globalSettingsBatchLoader,
            machineId: null,
            GlobalSettingIds.RubyTimeZoneInfoIpAddressWithPort,
            cancellationToken: cancellationToken);

    /// <summary>
    /// Is the connection to the RUBY Cloud allowed?
    /// [Source: Setting in Admin]
    /// </summary>
    public async Task<bool?> IsRubyCloudEnabled(
        GlobalSettingsBatchLoader globalSettingsBatchLoader,
        [Service] IGlobalSettingsService globalSettingsService,
        CancellationToken cancellationToken) =>
        await globalSettingsService.GetBoolean(
            globalSettingsBatchLoader,
            machineId: null,
            GlobalSettingIds.IsRubyCloudEnabled,
            cancellationToken: cancellationToken);

    /// <summary>
    /// Is user behavior tracking (Matomo) enabled?
    /// </summary>
    public async Task<bool?> IsUserBehaviorTrackingEnabled(
        GlobalSettingsBatchLoader globalSettingsBatchLoader,
        [Service] IGlobalSettingsService globalSettingsService,
        CancellationToken cancellationToken) =>
        await globalSettingsService.GetBoolean(
            globalSettingsBatchLoader,
            machineId: null,
            GlobalSettingIds.IsUserBehaviorTrackingEnabled,
            fallbackValue: true,
            cancellationToken);

    /// <summary>
    /// URL of the Matomo server.
    /// [Source: Hard-coded in backend]
    /// </summary>
    public async Task<string?> UserBehaviorTrackingUrl(
        GlobalSettingsBatchLoader globalSettingsBatchLoader,
        [Service] IGlobalSettingsService globalSettingsService,
        CancellationToken cancellationToken) =>
        await globalSettingsService.GetString(
            globalSettingsBatchLoader,
            machineId: null,
            GlobalSettingIds.UserBehaviorTrackingUrl,
            fallbackValue: "https://matomo.wh-ruby.cloud/",
            cancellationToken);

    /// <summary>
    /// Global settings which are set individually for each machine.
    /// </summary>
    /// <param name="machineId">Unique machine identifier (usually WuH equipment number, like: "EQ12345").</param>
    /// <returns></returns>
    [GraphQLIgnore]
    public GlobalSettingsPerMachine PerMachine(string machineId) => new(machineId);
}