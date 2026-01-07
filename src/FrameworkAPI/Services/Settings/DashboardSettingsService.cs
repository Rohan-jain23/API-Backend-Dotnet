using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.Exceptions;
using FrameworkAPI.Helpers;
using FrameworkAPI.Schema.Misc;
using FrameworkAPI.Schema.Settings.DashboardSettings;
using Microsoft.AspNetCore.Http;
using WuH.Ruby.Settings.Client;

namespace FrameworkAPI.Services.Settings;

public class DashboardSettingsService(ISettingsService settingsHttpClient) : IDashboardSettingsService
{
    private readonly ISettingsService _settingsHttpClient = settingsHttpClient;

    public async Task<DashboardSettings> CreateDashboard(
        string userId,
        CreateOrEditConfiguredDashboardRequest createDashboardRequest,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(createDashboardRequest.DashboardId))
        {
            throw new ParameterInvalidException($"{nameof(createDashboardRequest.DashboardId)} can not be set.");
        }

        var createdDashboard = await AddOrUpdate(userId, createDashboardRequest, cancellationToken);
        return createdDashboard;
    }

    public async Task<DashboardSettings> EditDashboard(
        string userId,
        CreateOrEditConfiguredDashboardRequest editDashboardRequest,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new UserIdNotFoundException();
        }

        if (string.IsNullOrWhiteSpace(editDashboardRequest.DashboardId))
        {
            throw new ParameterInvalidException($"{nameof(editDashboardRequest.DashboardId)} is null or empty.");
        }

        var requestedDashboard = await GetDashboardSettingsById(editDashboardRequest.DashboardId, cancellationToken);
        if (requestedDashboard.CreatorUserId != userId && requestedDashboard.CanOnlyBeEditedByCreator)
        {
            throw new UnauthorizedAccessException($"Dashboard '{editDashboardRequest.DashboardId}' can not be edited by user '{userId}'.");
        }

        return await AddOrUpdate(userId, editDashboardRequest, cancellationToken);
    }

    public async Task<string> DeleteDashboard(string userId, string dashboardId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new UserIdNotFoundException();
        }

        if (string.IsNullOrWhiteSpace(dashboardId))
        {
            throw new ParameterInvalidException($"{nameof(dashboardId)} is null or empty.");
        }

        var visibleDashboards = await GetDashboardSettingsForUser(userId, machineDepartmentFilter: null, cancellationToken);
        var userDashboards = visibleDashboards.Where(dashboard => dashboard.CreatorUserId == userId);
        var requestedDashboard = visibleDashboards.FirstOrDefault(dashboard => dashboard.DashboardId == dashboardId);

        if (requestedDashboard is null)
        {
            throw new IdNotFoundException($"Dashboard '{dashboardId}' does not exist or is not visible to user '{userId}'.");
        }

        if (requestedDashboard.CreatorUserId != userId)
        {
            throw new UnauthorizedAccessException($"Dashboard '{dashboardId}' can only be deleted by creator.");
        }

        if (userDashboards.Count() == 1)
        {
            throw new InternalServiceException($"Could not delete dashboard '{dashboardId}' because it is the last dashboard for user '{userId}'.", StatusCodes.Status403Forbidden);
        }

        var response = await _settingsHttpClient.DeleteDashboardSettingsById(dashboardId, cancellationToken);
        if (response.HasError)
        {
            throw new InternalServiceException(response.Error);
        }

        return dashboardId;
    }

    public async Task<DashboardSettings> GetDashboardSettingsById(string dashboardId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dashboardId))
        {
            throw new ParameterInvalidException($"{nameof(dashboardId)} is null or empty.");
        }

        var response = await _settingsHttpClient.GetDashboardSettingsById(dashboardId, cancellationToken);

        if (response.HasError && response.Error.StatusCode == StatusCodes.Status204NoContent)
        {
            throw new IdNotFoundException();
        }

        if (response.HasError)
        {
            throw new InternalServiceException(response.Error);
        }

        return new DashboardSettings(response.Item);
    }

    public async Task<DashboardSettings> GetDashboardSettingsByIdForUser(string userId, string dashboardId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new UserIdNotFoundException();
        }

        var dashboard = await GetDashboardSettingsById(dashboardId, cancellationToken);

        if (!dashboard.IsPublic && dashboard.CreatorUserId != userId)
        {
            throw new UnauthorizedAccessException($"User '{userId}' does not have access to the requested dashboard '{dashboardId}'.");
        }

        return dashboard;
    }

    public async Task<List<DashboardSettings>> GetDashboardSettingsForUser(string userId, MachineDepartment? machineDepartmentFilter, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new UserIdNotFoundException();
        }

        var dashboards = await GetAllDashboardSettings(machineDepartmentFilter, cancellationToken);

        return dashboards
            .Where(dashboard => dashboard.CreatorUserId == userId || dashboard.IsPublic)
            .ToList();
    }

    internal async Task<DashboardSettings> AddOrUpdate(string userId, CreateOrEditConfiguredDashboardRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new UserIdNotFoundException();
        }

        var response = await _settingsHttpClient.AddOrUpdateDashboardSettings(
            request.DashboardId,
            request.Department.ToString(),
            userId,
            request.IsPublic,
            canOnlyBeEditedByCreator: true,
            request.FriendlyName,
            request.Widget1?.MapToInternalDashboardWidgetSettings(),
            request.Widget2?.MapToInternalDashboardWidgetSettings(),
            request.Widget3?.MapToInternalDashboardWidgetSettings(),
            request.Widget4?.MapToInternalDashboardWidgetSettings(),
            request.Widget5?.MapToInternalDashboardWidgetSettings(),
            request.Widget6?.MapToInternalDashboardWidgetSettings(),
            cancellationToken);

        if (response.HasError)
        {
            throw new InternalServiceException(response.Error);
        }

        return new DashboardSettings(response.Item);
    }

    internal async Task<List<DashboardSettings>> GetAllDashboardSettings(MachineDepartment? machineDepartmentFilter, CancellationToken cancellationToken)
    {
        var response = await _settingsHttpClient.GetAllDashboardSettings(cancellationToken);

        if (response.HasError && response.Error.StatusCode == 204)
        {
            return [];
        }

        if (response.HasError)
        {
            throw new InternalServiceException(response.Error);
        }

        return response.Items
            .Where(settings => string.IsNullOrWhiteSpace(machineDepartmentFilter?.ToString()) || settings.Department == machineDepartmentFilter?.ToString())
            .Select(settings => new DashboardSettings(settings)).ToList();
    }
}