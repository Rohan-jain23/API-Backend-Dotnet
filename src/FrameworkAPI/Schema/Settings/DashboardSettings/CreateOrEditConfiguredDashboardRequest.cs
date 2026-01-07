using FrameworkAPI.Schema.Misc;

namespace FrameworkAPI.Schema.Settings.DashboardSettings;

/// <summary>
/// Request item to create or edit a dashboard.
/// All widget settings will be overwritten - even if they are null.
/// </summary>
public class CreateOrEditConfiguredDashboardRequest
{
    /// <summary>
    /// The dashboard id of the dashboard to be edited or created.
    /// </summary>
    public string? DashboardId { get; set; }

    /// <summary>
    /// The WuH department this dashboards is dedicated for.
    /// </summary>
    public required MachineDepartment Department { get; set; }

    /// <summary>
    /// Is true, if all users can see this dashboard.
    /// Is false, if only the creator can see it.
    /// </summary>
    public required bool IsPublic { get; set; }

    /// <summary>
    /// The friendly name of this dashboard which was given by the user.
    /// There should not be two public dashboards with the same friendly name.
    /// </summary>
    public required string FriendlyName { get; set; }

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