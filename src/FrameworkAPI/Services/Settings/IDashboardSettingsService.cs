using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.Schema.Misc;
using FrameworkAPI.Schema.Settings.DashboardSettings;

namespace FrameworkAPI.Services.Settings;

public interface IDashboardSettingsService
{
    Task<DashboardSettings> CreateDashboard(string userId, CreateOrEditConfiguredDashboardRequest createDashboardRequest, CancellationToken cancellationToken);

    Task<DashboardSettings> EditDashboard(string userId, CreateOrEditConfiguredDashboardRequest editDashboardRequest, CancellationToken cancellationToken);

    Task<string> DeleteDashboard(string userId, string dashboardId, CancellationToken cancellationToken);

    Task<DashboardSettings> GetDashboardSettingsById(string dashboardId, CancellationToken cancellationToken);

    Task<DashboardSettings> GetDashboardSettingsByIdForUser(string userId, string dashboardId, CancellationToken cancellationToken);

    Task<List<DashboardSettings>> GetDashboardSettingsForUser(string userId, MachineDepartment? machineDepartmentFilter, CancellationToken cancellationToken);
}