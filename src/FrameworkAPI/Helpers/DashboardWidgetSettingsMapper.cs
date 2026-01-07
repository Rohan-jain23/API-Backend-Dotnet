using FrameworkAPI.Schema.Settings.DashboardSettings;
using SettingsModels = WuH.Ruby.Settings.Client.Models;

namespace FrameworkAPI.Helpers;

public static class DashboardWidgetSettingsMapper
{
    internal static SettingsModels.DashboardWidgetSettings MapToInternalDashboardWidgetSettings(this DashboardWidgetSettings dashboardWidgetSettings)
    => new()
    {
        WidgetCatalogId = dashboardWidgetSettings.WidgetCatalogId,
        MachineIds = dashboardWidgetSettings.MachineIds,
        AdditionalSetting = dashboardWidgetSettings.AdditionalSetting
    };
}