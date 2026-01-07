using System;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.Exceptions;
using FrameworkAPI.Schema.Settings.DashboardSettings;
using FrameworkAPI.Services.Settings;
using HotChocolate;
using HotChocolate.Types;

namespace FrameworkAPI.Mutations;

/// <summary>
/// Manages configured dashboards.
/// </summary>
[ExtendObjectType("Mutation")]
public class DashboardSettingsMutation
{
    /// <summary>
    /// Mutation to create a dashboard.
    /// <param name="userId">The user id of the creator.</param>
    /// <param name="dashboardSettingsService">The dashboard settings service.</param>
    /// <param name="createDashboardRequest">Dashboard settings.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// </summary>
    [Error(typeof(ParameterInvalidException))]
    [Error(typeof(UserIdNotFoundException))]
    [Error(typeof(InternalServiceException))]
    [UseMutationConvention(PayloadFieldName = "dashboardSettings")]
    public async Task<DashboardSettings> DashboardSettingsCreateDashboard(
        [GlobalState] string userId,
        [Service] IDashboardSettingsService dashboardSettingsService,
        CreateOrEditConfiguredDashboardRequest createDashboardRequest,
        CancellationToken cancellationToken)
    => await dashboardSettingsService.CreateDashboard(userId, createDashboardRequest, cancellationToken);

    /// <summary>
    /// Mutation to edit a dashboard.
    /// <param name="userId">The user id of the editor.</param>
    /// <param name="dashboardSettingsService">The dashboard settings service.</param>
    /// <param name="editDashboardRequest">Dashboard settings.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// </summary>
    [Error(typeof(ParameterInvalidException))]
    [Error(typeof(UserIdNotFoundException))]
    [Error(typeof(IdNotFoundException))]
    [Error(typeof(InternalServiceException))]
    [UseMutationConvention(PayloadFieldName = "dashboardSettings")]
    public async Task<DashboardSettings> DashboardSettingsEditDashboard(
        [GlobalState] string userId,
        [Service] IDashboardSettingsService dashboardSettingsService,
        CreateOrEditConfiguredDashboardRequest editDashboardRequest,
        CancellationToken cancellationToken)
    => await dashboardSettingsService.EditDashboard(userId, editDashboardRequest, cancellationToken);

    /// <summary>
    /// Mutation to delete a dashboard.
    /// <param name="userId">The user id of the deleter.</param>
    /// <param name="dashboardSettingsService">The dashboard settings service.</param>
    /// <param name="dashboardId">Id of the dashboard.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// </summary>
    [Error(typeof(UserIdNotFoundException))]
    [Error(typeof(IdNotFoundException))]
    [Error(typeof(UnauthorizedAccessException))]
    [Error(typeof(ParameterInvalidException))]
    [Error(typeof(InternalServiceException))]
    [UseMutationConvention(PayloadFieldName = "dashboardId")]
    public async Task<string> DashboardSettingsDeleteDashboard(
        [GlobalState] string userId,
        [Service] IDashboardSettingsService dashboardSettingsService,
        string dashboardId,
        CancellationToken cancellationToken)
    => await dashboardSettingsService.DeleteDashboard(userId, dashboardId, cancellationToken);
}