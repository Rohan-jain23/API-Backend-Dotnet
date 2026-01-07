namespace FrameworkAPI.Models.Settings;

/// <summary>
/// The settings for user preferences.
/// </summary>
public static class GlobalSettingIds
{
    /// <summary>
    /// The SettingId for the friendly name of the ruby instance.
    /// </summary>
    public const string RubyFriendlyName = "RubyInstance_FriendlyName";

    /// <summary>
    /// The SettingId for the time zone info of the ruby instance.
    /// </summary>
    public const string RubyTimeZoneInfoTimeZone = "TimeInformation_TimeZone";

    /// <summary>
    /// The SettingId of the DayLightSavingTime flag for the time zone info of the ruby instance.
    /// </summary>
    public const string RubyTimeZoneInfoDayLightSavingTime = "TimeInformation_DayLightSavingTime";

    /// <summary>
    /// The SettingId for the time zone server of the ruby instance.
    /// </summary>
    public const string RubyTimeZoneInfoIpAddressWithPort = "TimeInformation_IPAddressWithPort";

    /// <summary>
    /// The SettingId if the ruby cloud is enabled.
    /// </summary>
    public const string IsRubyCloudEnabled = "Cloud_Enabled";

    /// <summary>
    /// The SettingId if the user behavior tracking is enabled.
    /// </summary>
    public const string IsUserBehaviorTrackingEnabled = "UserTracking_Enabled";

    /// <summary>
    /// The SettingId for the tracking url if behavior tracking is enabled.
    /// </summary>
    public const string UserBehaviorTrackingUrl = "UserTracking_URL";
}