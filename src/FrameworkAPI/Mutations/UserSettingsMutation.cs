using System;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.Exceptions;
using FrameworkAPI.Helpers;
using FrameworkAPI.Models.Settings;
using FrameworkAPI.Schema.Misc;
using FrameworkAPI.Schema.Settings;
using FrameworkAPI.Services.Settings;
using HotChocolate;
using HotChocolate.Types;
using Microsoft.AspNetCore.Http;
using WuH.Ruby.Common.Core;

namespace FrameworkAPI.Mutations;

/// <summary>
/// Updates user specific values like language or the preferred unit system.
/// </summary>
[ExtendObjectType("Mutation")]
public class UserSettingsMutation
{
    /// <summary>
    /// Mutation to change language of a user.
    /// <param name="userId">The user id.</param>
    /// <param name="userSettingsService">The user settings service.</param>
    /// <param name="languageTag">The preferred user language.</param>
    /// </summary>
    [Error(typeof(ParameterInvalidException))]
    [Error(typeof(UserIdNotFoundException))]
    [Error(typeof(InternalServiceException))]
    [UseMutationConvention(PayloadFieldName = "languageTag")]
    public async Task<string> UserSettingsChangeLanguage(
        [GlobalState] string userId,
        [Service] IUserSettingsService userSettingsService,
        string languageTag)
    {
        if (!ValidLanguageTagHelper.IsLanguageTagValid(languageTag))
        {
            throw new ParameterInvalidException($"Language tag '{languageTag}' is not valid .");
        }

        await userSettingsService.Change(
            userId,
            machineId: null,
            UserSettingIds.Language,
            languageTag,
            CancellationToken.None);

        return languageTag;
    }

    /// <summary>
    /// Mutation to change preferred unit system of a user.
    /// <param name="userId">The user id.</param>
    /// <param name="userSettingsService">The user settings service.</param>
    /// <param name="unitRepresentation">The preferred user unit system.</param>
    /// </summary>
    [Error(typeof(UserIdNotFoundException))]
    [Error(typeof(InternalServiceException))]
    public async Task<UnitRepresentation> UserSettingsChangeUnitSystem(
        [GlobalState] string userId,
        [Service] IUserSettingsService userSettingsService,
        UnitRepresentation unitRepresentation)
    {
        await userSettingsService.Change(
            userId,
            machineId: null,
            UserSettingIds.IsUnitRepresentationInSi,
            (unitRepresentation == UnitRepresentation.Si).ToString(),
            CancellationToken.None);

        return unitRepresentation;
    }

    /// <summary>
    /// Mutation to change the favorite dashboard of a user for extrusion department.
    /// <param name="userId">The user id.</param>
    /// <param name="userSettingsService">The user settings service.</param>
    /// <param name="dashboardSettingsService">The dashboard settings service.</param>
    /// <param name="dashboardId">The database id of the new favorite dashboard.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// </summary>
    [Error(typeof(UserIdNotFoundException))]
    [Error(typeof(IdNotFoundException))]
    [Error(typeof(UnauthorizedAccessException))]
    [Error(typeof(InternalServiceException))]
    [UseMutationConvention(PayloadFieldName = "dashboardId")]
    public async Task<string> UserSettingsChangeFavoriteDashboardExtrusion(
        [GlobalState] string userId,
        [Service] IUserSettingsService userSettingsService,
        [Service] IDashboardSettingsService dashboardSettingsService,
        string dashboardId,
        CancellationToken cancellationToken)
    => await UserSettingsChangeFavoriteDashboard(
        userId,
        dashboardId,
        MachineDepartment.Extrusion,
        userSettingsService,
        dashboardSettingsService,
        cancellationToken);

    /// <summary>
    /// Mutation to change the favorite dashboard of a user for printing department.
    /// <param name="userId">The user id.</param>
    /// <param name="userSettingsService">The user settings service.</param>
    /// <param name="dashboardSettingsService">The dashboard settings service.</param>
    /// <param name="dashboardId">The database id of the new favorite dashboard.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// </summary>
    [Error(typeof(UserIdNotFoundException))]
    [Error(typeof(IdNotFoundException))]
    [Error(typeof(UnauthorizedAccessException))]
    [Error(typeof(InternalServiceException))]
    [UseMutationConvention(PayloadFieldName = "dashboardId")]
    public async Task<string> UserSettingsChangeFavoriteDashboardPrinting(
        [GlobalState] string userId,
        [Service] IUserSettingsService userSettingsService,
        [Service] IDashboardSettingsService dashboardSettingsService,
        string dashboardId,
        CancellationToken cancellationToken)
    => await UserSettingsChangeFavoriteDashboard(
        userId,
        dashboardId,
        MachineDepartment.Printing,
        userSettingsService,
        dashboardSettingsService,
        cancellationToken);

    /// <summary>
    /// Mutation to change the favorite dashboard of a user for papersack department.
    /// <param name="userId">The user id.</param>
    /// <param name="userSettingsService">The user settings service.</param>
    /// <param name="dashboardSettingsService">The dashboard settings service.</param>
    /// <param name="dashboardId">The database id of the new favorite dashboard.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// </summary>
    [Error(typeof(UserIdNotFoundException))]
    [Error(typeof(IdNotFoundException))]
    [Error(typeof(UnauthorizedAccessException))]
    [Error(typeof(InternalServiceException))]
    [UseMutationConvention(PayloadFieldName = "dashboardId")]
    public async Task<string> UserSettingsChangeFavoriteDashboardPaperSack(
        [GlobalState] string userId,
        [Service] IUserSettingsService userSettingsService,
        [Service] IDashboardSettingsService dashboardSettingsService,
        string dashboardId,
        CancellationToken cancellationToken)
    => await UserSettingsChangeFavoriteDashboard(
        userId,
        dashboardId,
        MachineDepartment.PaperSack,
        userSettingsService,
        dashboardSettingsService,
        cancellationToken);

    /// <summary>
    /// Mutation to change the favorite dashboard of a user for other departments.
    /// <param name="userId">The user id.</param>
    /// <param name="userSettingsService">The user settings service.</param>
    /// <param name="dashboardSettingsService">The dashboard settings service.</param>
    /// <param name="dashboardId">The database id of the new favorite dashboard.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// </summary>
    [Error(typeof(UserIdNotFoundException))]
    [Error(typeof(IdNotFoundException))]
    [Error(typeof(UnauthorizedAccessException))]
    [Error(typeof(InternalServiceException))]
    [UseMutationConvention(PayloadFieldName = "dashboardId")]
    public async Task<string> UserSettingsChangeFavoriteDashboardOther(
        [GlobalState] string userId,
        [Service] IUserSettingsService userSettingsService,
        [Service] IDashboardSettingsService dashboardSettingsService,
        string dashboardId,
        CancellationToken cancellationToken)
    => await UserSettingsChangeFavoriteDashboard(
        userId,
        dashboardId,
        MachineDepartment.Other,
        userSettingsService,
        dashboardSettingsService,
        cancellationToken);

    /// <summary>
    /// Mutation to change preferred machine department of a user.
    /// <param name="userId">The user id.</param>
    /// <param name="userSettingsService">The user settings service.</param>
    /// <param name="machineDepartment">The preferred machine department.</param>
    /// </summary>
    [Error(typeof(UserIdNotFoundException))]
    [Error(typeof(InternalServiceException))]
    public async Task<MachineDepartment?> UserSettingsChangeMachineDepartment(
        [GlobalState] string userId,
        [Service] IUserSettingsService userSettingsService,
        MachineDepartment? machineDepartment)
    {
        await userSettingsService.Change(
            userId,
            machineId: null,
            UserSettingIds.SelectedMachineDepartment,
            machineDepartment?.ToString(),
            CancellationToken.None);

        return machineDepartment;
    }

    /// <summary>
    /// Mutation to change preferred machine family for printing business unit of a user.
    /// <param name="userId">The user id.</param>
    /// <param name="userSettingsService">The user settings service.</param>
    /// <param name="printingMachineFamily">The preferred machine family.</param>
    /// </summary>
    [Error(typeof(UserIdNotFoundException))]
    [Error(typeof(InternalServiceException))]
    [UseMutationConvention(PayloadFieldName = "printingMachineFamily")]
    public async Task<MachineFamily?> UserSettingsChangePrintingMachineFamily(
        [GlobalState] string userId,
        [Service] IUserSettingsService userSettingsService,
        MachineFamily? printingMachineFamily)
    {
        return await UserSettingsChangeMachineFamily(
            UserSettingIds.SelectedPrintingMachineFamily,
            userId,
            userSettingsService,
            printingMachineFamily);
    }

    /// <summary>
    /// Mutation to change preferred machine family for extrusion business unit of a user.
    /// <param name="userId">The user id.</param>
    /// <param name="userSettingsService">The user settings service.</param>
    /// <param name="extrusionMachineFamily">The preferred machine family.</param>
    /// </summary>
    [Error(typeof(UserIdNotFoundException))]
    [Error(typeof(InternalServiceException))]
    [UseMutationConvention(PayloadFieldName = "extrusionMachineFamily")]
    public async Task<MachineFamily?> UserSettingsChangeExtrusionMachineFamily(
        [GlobalState] string userId,
        [Service] IUserSettingsService userSettingsService,
        MachineFamily? extrusionMachineFamily)
    {
        return await UserSettingsChangeMachineFamily(
            UserSettingIds.SelectedExtrusionMachineFamily,
            userId,
            userSettingsService,
            extrusionMachineFamily);
    }

    /// <summary>
    /// Mutation to change preferred machine family for paper sack business unit of a user.
    /// <param name="userId">The user id.</param>
    /// <param name="userSettingsService">The user settings service.</param>
    /// <param name="paperSackMachineFamily">The preferred machine family.</param>
    /// </summary>
    [Error(typeof(UserIdNotFoundException))]
    [Error(typeof(InternalServiceException))]
    [UseMutationConvention(PayloadFieldName = "paperSackMachineFamily")]
    public async Task<MachineFamily?> UserSettingsChangePaperSackMachineFamily(
        [GlobalState] string userId,
        [Service] IUserSettingsService userSettingsService,
        MachineFamily? paperSackMachineFamily)
    {
        return await UserSettingsChangeMachineFamily(
            UserSettingIds.SelectedPaperSackMachineFamily,
            userId,
            userSettingsService,
            paperSackMachineFamily);
    }

    /// <summary>
    /// Mutation to change preferred machine family for other business unit of a user.
    /// <param name="userId">The user id.</param>
    /// <param name="userSettingsService">The user settings service.</param>
    /// <param name="otherMachineFamily">The preferred machine family.</param>
    /// </summary>
    [Error(typeof(UserIdNotFoundException))]
    [Error(typeof(InternalServiceException))]
    [UseMutationConvention(PayloadFieldName = "otherMachineFamily")]
    public async Task<MachineFamily?> UserSettingsChangeOtherMachineFamily(
        [GlobalState] string userId,
        [Service] IUserSettingsService userSettingsService,
        MachineFamily? otherMachineFamily)
    {
        return await UserSettingsChangeMachineFamily(
            UserSettingIds.SelectedOtherMachineFamily,
            userId,
            userSettingsService,
            otherMachineFamily);
    }

    private static async Task<string> UserSettingsChangeFavoriteDashboard(
        string userId,
        string dashboardId,
        MachineDepartment machineDepartment,
        IUserSettingsService userSettingsService,
        IDashboardSettingsService dashboardSettingsService,
        CancellationToken cancellationToken)
    {
        var requestedDashboard = await dashboardSettingsService.GetDashboardSettingsById(dashboardId, cancellationToken);

        if (requestedDashboard.CreatorUserId != userId && !requestedDashboard.IsPublic)
        {
            throw new UnauthorizedAccessException($"Dashboard '{dashboardId}' is not accessible by user '{userId}'.");
        }

        if (requestedDashboard.Department != machineDepartment)
        {
            throw new InternalServiceException(new InternalError(StatusCodes.Status400BadRequest, $"Requested dashboard is not a {machineDepartment} dashboard."));
        }

        var userSettingId = machineDepartment switch
        {
            MachineDepartment.Extrusion => UserSettingIds.FavoriteDashboardDatabaseIdExtrusion,
            MachineDepartment.PaperSack => UserSettingIds.FavoriteDashboardDatabaseIdPaperSack,
            MachineDepartment.Printing => UserSettingIds.FavoriteDashboardDatabaseIdPrinting,
            MachineDepartment.Other => UserSettingIds.FavoriteDashboardDatabaseIdOther,
            _ => throw new ArgumentOutOfRangeException()
        };

        await userSettingsService.Change(
            userId,
            machineId: null,
            userSettingId,
            dashboardId,
            cancellationToken);

        return dashboardId;
    }

    private static async Task<MachineFamily?> UserSettingsChangeMachineFamily(
        string userSettingId,
        string userId,
        IUserSettingsService userSettingsService,
        MachineFamily? machineFamily)
    {
        await userSettingsService.Change(
            userId,
            machineId: null,
            userSettingId,
            machineFamily?.ToString(),
            CancellationToken.None);

        return machineFamily;
    }
}