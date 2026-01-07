using System;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.DataLoaders;
using FrameworkAPI.Schema.Misc;
using SettingsModels = WuH.Ruby.Settings.Client.Models;

namespace FrameworkAPI.Schema.Settings.DashboardSettings;

public class DashboardSettings
{
    public DashboardSettings(SettingsModels.DashboardSettings dashboardSettings)
    {
        if (!Enum.TryParse<MachineDepartment>(dashboardSettings.Department, out var department))
        {
            throw new ArgumentOutOfRangeException(nameof(dashboardSettings.Department));
        }

        DashboardId = dashboardSettings.DashboardId;
        Department = department;
        IsPublic = dashboardSettings.IsPublic;
        CanOnlyBeEditedByCreator = dashboardSettings.CanOnlyBeEditedByCreator;
        FriendlyName = dashboardSettings.FriendlyName;
        Widget1 = dashboardSettings.WidgetSettings1 is null ? null : new DashboardWidgetSettings(dashboardSettings.WidgetSettings1);
        Widget2 = dashboardSettings.WidgetSettings2 is null ? null : new DashboardWidgetSettings(dashboardSettings.WidgetSettings2);
        Widget3 = dashboardSettings.WidgetSettings3 is null ? null : new DashboardWidgetSettings(dashboardSettings.WidgetSettings3);
        Widget4 = dashboardSettings.WidgetSettings4 is null ? null : new DashboardWidgetSettings(dashboardSettings.WidgetSettings4);
        Widget5 = dashboardSettings.WidgetSettings5 is null ? null : new DashboardWidgetSettings(dashboardSettings.WidgetSettings5);
        Widget6 = dashboardSettings.WidgetSettings6 is null ? null : new DashboardWidgetSettings(dashboardSettings.WidgetSettings6);
        CreatedDate = dashboardSettings.CreatedDate;
        CreatorUserId = dashboardSettings.CreatorUserId;
        LastEditedDate = dashboardSettings.ModifiedDate;
        LastEditorUserId = dashboardSettings.ModifiedUserId;
    }

    /// <summary>
    /// Unique Identifier of this dashboard that was given by the database.
    /// If this is null, the setting is not yet inserted into the database.
    /// </summary>
    public string DashboardId { get; set; }

    /// <summary>
    /// The WuH department this dashboards is dedicated for.
    /// </summary>
    public MachineDepartment Department { get; set; }

    /// <summary>
    /// Is true, if all users can see this dashboard.
    /// Is false, if only the creator can see it.
    /// </summary>
    public bool IsPublic { get; set; }

    /// <summary>
    /// The friendly name of this dashboard which was given by the user.
    /// There should not be two public dashboards with the same friendly name.
    /// </summary>
    public string FriendlyName { get; set; }

    /// <summary>
    /// The timestamp at which this dashboard was created.
    /// </summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// The unique ID of the user that created this dashboard.
    /// </summary>
    public string CreatorUserId { get; set; }

    /// <summary>
    /// The full name of the user that created this dashboard.
    /// [Source: Supervisor]
    /// </summary>
    public async Task<string?> CreatorFullName(
        UserNameCacheDataLoader cacheDataLoader,
        CancellationToken cancellationToken)
    {
        var (name, exception) = await cacheDataLoader.LoadAsync(CreatorUserId, cancellationToken);

        if (exception is not null)
        {
            throw exception;
        }

        return name;
    }

    /// <summary>
    /// The timestamp at which this dashboard was edited the last time.
    /// Is null, if this dashboard was never edited.
    /// </summary>
    public DateTime? LastEditedDate { get; set; }

    /// <summary>
    /// The unique ID of the user that edited this dashboard the last time.
    /// Is null, if this dashboard was never edited.
    /// </summary>
    public string? LastEditorUserId { get; set; }

    /// <summary>
    /// The full name of the user that edited this dashboard the last time.
    /// Is null, if this dashboard was never edited.
    /// [Source: Supervisor]
    /// </summary>
    public async Task<string?> LastEditorFullName(
        UserNameCacheDataLoader cacheDataLoader,
        CancellationToken cancellationToken)
    {
        if (LastEditorUserId is null)
        {
            return string.Empty;
        }

        var (name, exception) = await cacheDataLoader.LoadAsync(LastEditorUserId, cancellationToken);

        if (exception is not null)
        {
            throw exception;
        }

        return name;
    }

    /// <summary>
    /// Is true, if this dashboard can only be edited by the creator.
    /// This setting can be used to prevent unwanted changes of a dashboard (for example in show cases).
    /// As we don't have an admin interface to manage dashboards that were created by deleted users, this setting will always be false by default.
    /// For show cases at trade fairs etc, it can be set in database.
    /// </summary>
    public bool CanOnlyBeEditedByCreator { get; set; } = false;

    /// <summary>
    /// Settings for the 1st widget.
    /// If this is null, this widget is blank.
    /// </summary>
    public DashboardWidgetSettings? Widget1 { get; set; }

    /// <summary>
    /// Settings for the 2nd widget.
    /// If this is null, this widget is blank.
    /// </summary>
    public DashboardWidgetSettings? Widget2 { get; set; }

    /// <summary>
    /// Settings for the 3rd widget.
    /// If this is null, this widget is blank.
    /// </summary>
    public DashboardWidgetSettings? Widget3 { get; set; }

    /// <summary>
    /// Settings for the 4th widget.
    /// If this is null, this widget is blank.
    /// </summary>
    public DashboardWidgetSettings? Widget4 { get; set; }

    /// <summary>
    /// Settings for the 5th widget.
    /// If this is null, this widget is blank.
    /// </summary>
    public DashboardWidgetSettings? Widget5 { get; set; }

    /// <summary>
    /// Settings for the 6th widget.
    /// If this is null, this widget is blank.
    /// </summary>
    public DashboardWidgetSettings? Widget6 { get; set; }
}