using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.Exceptions;
using FrameworkAPI.Models.Settings;
using FrameworkAPI.Services.Settings;
using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Types;

namespace FrameworkAPI.Mutations;

/// <summary>
/// Updates user specific values like language or the preferred unit system.
/// </summary>
[ExtendObjectType("Mutation")]
public class GlobalSettingsMutation
{
    /// <summary>
    /// Mutation to change friendly name of the ruby instance.
    /// <param name="globalSettingsService">The user settings service.</param>
    /// <param name="rubyFriendlyName">The ruby friendly name.</param>
    /// </summary>
    [Authorize(Roles = ["admin"])]
    [Error(typeof(ParameterInvalidException))]
    [Error(typeof(InternalServiceException))]
    [UseMutationConvention(PayloadFieldName = "rubyFriendlyName")]
    public async Task<string?> GlobalSettingsChangeRubyFriendlyName(
        [Service] IGlobalSettingsService globalSettingsService,
        string? rubyFriendlyName)
    {
        await globalSettingsService.Change(
            machineId: null,
            GlobalSettingIds.RubyFriendlyName,
            rubyFriendlyName,
            CancellationToken.None);

        return rubyFriendlyName;
    }

    /// <summary>
    /// Mutation to change time zone of the ruby instance.
    /// <param name="globalSettingsService">The user settings service.</param>
    /// <param name="timeZone">The time zone.</param>
    /// </summary>
    [Authorize(Roles = ["admin"])]
    [Error(typeof(ParameterInvalidException))]
    [Error(typeof(InternalServiceException))]
    [UseMutationConvention(PayloadFieldName = "timeZone")]
    public async Task<string> GlobalSettingsChangeTimeZone(
        [Service] IGlobalSettingsService globalSettingsService,
        string timeZone)
    {
        await globalSettingsService.Change(
            machineId: null,
            GlobalSettingIds.RubyTimeZoneInfoTimeZone,
            timeZone,
            CancellationToken.None);

        return timeZone;
    }

    /// <summary>
    /// Mutation to change the ntp server of the ruby instance.
    /// <param name="globalSettingsService">The user settings service.</param>
    /// <param name="timeZoneIoAddressWithPort">The ntp server with ip and port.</param>
    /// </summary>
    [Authorize(Roles = ["admin"])]
    [Error(typeof(ParameterInvalidException))]
    [Error(typeof(InternalServiceException))]
    [UseMutationConvention(PayloadFieldName = "timeZoneIoAddressWithPort")]
    public async Task<string?> GlobalSettingsChangeTimeZoneIpAddressWithPort(
        [Service] IGlobalSettingsService globalSettingsService,
        string? timeZoneIoAddressWithPort)
    {
        await globalSettingsService.Change(
            machineId: null,
            GlobalSettingIds.RubyTimeZoneInfoIpAddressWithPort,
            timeZoneIoAddressWithPort,
            CancellationToken.None);

        return timeZoneIoAddressWithPort;
    }

    /// <summary>
    /// Mutation to change the flag which signals if the cloud is enabled.
    /// <param name="globalSettingsService">The user settings service.</param>
    /// <param name="cloudEnabled">The flag which signals if the cloud is enabled.</param>
    /// </summary>
    [Authorize(Roles = ["admin"])]
    [Error(typeof(ParameterInvalidException))]
    [Error(typeof(InternalServiceException))]
    [UseMutationConvention(PayloadFieldName = "cloudEnabled")]
    public async Task<bool> GlobalSettingsChangeRubyCloudEnabled(
        [Service] IGlobalSettingsService globalSettingsService,
        bool cloudEnabled)
    {
        await globalSettingsService.Change(
            machineId: null,
            GlobalSettingIds.IsRubyCloudEnabled,
            cloudEnabled.ToString(),
            CancellationToken.None);

        return cloudEnabled;
    }

    /// <summary>
    /// Mutation to change the flag which signals if the user behavior tracking is enabled.
    /// <param name="globalSettingsService">The user settings service.</param>
    /// <param name="userBehaviorTrackingEnabled">The flag which signals if the user behavior tracking is enabled.</param>
    /// </summary>
    [Authorize(Roles = ["go-general"])]
    [Error(typeof(ParameterInvalidException))]
    [Error(typeof(InternalServiceException))]
    [UseMutationConvention(PayloadFieldName = "userBehaviorTrackingEnabled")]
    public async Task<bool> GlobalSettingsChangeRubyUserBehaviorTrackingEnabled(
        [Service] IGlobalSettingsService globalSettingsService,
        bool userBehaviorTrackingEnabled)
    {
        await globalSettingsService.Change(
            machineId: null,
            GlobalSettingIds.IsUserBehaviorTrackingEnabled,
            userBehaviorTrackingEnabled.ToString(),
            CancellationToken.None);

        return userBehaviorTrackingEnabled;
    }

    /// <summary>
    /// Mutation to change the url tracking is send to if enabled.
    /// <param name="globalSettingsService">The user settings service.</param>
    /// <param name="userBehaviorTrackingUrl">The url tracking data is send to.</param>
    /// </summary>
    [Authorize(Roles = ["go-general"])]
    [Error(typeof(ParameterInvalidException))]
    [Error(typeof(InternalServiceException))]
    [UseMutationConvention(PayloadFieldName = "userBehaviorTrackingUrl")]
    public async Task<string?> GlobalSettingsChangeUserBehaviorTrackingUrl(
        [Service] IGlobalSettingsService globalSettingsService,
        string? userBehaviorTrackingUrl)
    {
        await globalSettingsService.Change(
            machineId: null,
            GlobalSettingIds.UserBehaviorTrackingUrl,
            userBehaviorTrackingUrl,
            CancellationToken.None);

        return userBehaviorTrackingUrl;
    }
}