using System.Collections.Generic;
using SettingsModels = WuH.Ruby.Settings.Client.Models;

namespace FrameworkAPI.Schema.Settings.DashboardSettings;

public class DashboardWidgetSettings
{
    /// <summary>
    /// Reference to the entry in the frontend's widget catalog.
    /// This is not a unique identifier for the widget instance:
    /// Widgets with the same WidgetCatalogId can be configured on the same or other dashboards but with different machine(s).
    /// </summary>
    public string WidgetCatalogId { get; set; } = string.Empty;

    /// <summary>
    /// List with unique machine identifiers (usually WuH equipment number, like: "EQ12345") of all machines that should be shown on this widget.
    /// </summary>
    public List<string> MachineIds { get; set; } = [];

    /// <summary>
    /// An optional free text that can be used for specific widget settings.
    /// </summary>
    public string? AdditionalSetting { get; set; }

    public DashboardWidgetSettings(SettingsModels.DashboardWidgetSettings dashboardWidgetSettings)
    {
        WidgetCatalogId = dashboardWidgetSettings.WidgetCatalogId;
        MachineIds = dashboardWidgetSettings.MachineIds;
        AdditionalSetting = dashboardWidgetSettings.AdditionalSetting;
    }

    // We need this empty constructor, because the class is present in other GraphQl Types as property
    public DashboardWidgetSettings() { }
}