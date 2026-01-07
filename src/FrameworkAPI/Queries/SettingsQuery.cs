using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.Exceptions;
using FrameworkAPI.Schema.Misc;
using FrameworkAPI.Schema.Settings;
using FrameworkAPI.Schema.Settings.DashboardSettings;
using FrameworkAPI.Services.Settings;
using HotChocolate;
using HotChocolate.Types;

namespace FrameworkAPI.Queries;

/// <summary>
/// GraphQL query class for settings entity.
/// </summary>
[ExtendObjectType("Query")]
public class SettingsQuery
{
    /// <summary>
    /// Query to get settings of the logged-in user.
    /// </summary>
    public UserSettings? GetUserSettings([GlobalState] string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new UserIdNotFoundException();
        }

        return new UserSettings(userId);
    }

    /// <summary>
    /// Query to get settings of the whole RUBY instance.
    /// </summary>
    public GlobalSettings? GetGlobalSettings()
    {
        return new GlobalSettings();
    }

    /// <summary>
    /// Query to get settings of the configured RUBY dashboards that are visible for the logged-in user.
    /// <param name="dashboardSettingsService">The dashboard settings service.</param>
    /// <param name="userId">The userId from the bearer token.</param>
    /// <param name="machineDepartmentFilter">If set, only dashboards of this machine department are returned.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// </summary>
    public async Task<List<DashboardSettings>> GetDashboardSettings(
        [Service] IDashboardSettingsService dashboardSettingsService,
        [GlobalState] string userId,
        MachineDepartment? machineDepartmentFilter,
        CancellationToken cancellationToken)
    => await dashboardSettingsService.GetDashboardSettingsForUser(userId, machineDepartmentFilter, cancellationToken);

    /// <summary>
    /// Query to get settings of a RUBY dashboard.
    /// This is only successful if the dashboard is visible for the logged-in user.
    /// <param name="dashboardSettingsService">The dashboard settings service.</param>
    /// <param name="userId">The userId from the bearer token.</param>
    /// <param name="dashboardId">The database id of the dashboard.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// </summary>
    public async Task<DashboardSettings> GetDashboardSettingsById(
        [Service] IDashboardSettingsService dashboardSettingsService,
        [GlobalState] string userId,
        string dashboardId,
        CancellationToken cancellationToken)
    => await dashboardSettingsService.GetDashboardSettingsByIdForUser(userId, dashboardId, cancellationToken);
}