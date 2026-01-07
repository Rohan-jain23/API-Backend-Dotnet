using System;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.DataLoaders;
using FrameworkAPI.Models.Settings;
using FrameworkAPI.Schema.Misc;
using FrameworkAPI.Services.Settings;
using HotChocolate;

namespace FrameworkAPI.Schema.Settings;

/// <summary>
/// Settings for the logged-in user.
/// </summary>
public class UserSettings(string userId)
{
    private readonly string _userId = userId;

    /// <summary>
    /// Language tag (like 'en-US') of the logged-in user.
    /// </summary>
    public async Task<string?> LanguageTag(
        UserSettingsBatchLoader userSettingsBatchLoader,
        [Service] IUserSettingsService userSettingsService,
        CancellationToken cancellationToken) =>
        await userSettingsService.GetString(
            userSettingsBatchLoader,
            _userId,
            machineId: null,
            UserSettingIds.Language,
            cancellationToken: cancellationToken);

    /// <summary>
    /// Unit representation system of the logged-in user.
    /// </summary>
    public async Task<UnitRepresentation?> UnitRepresentation(
        UserSettingsBatchLoader userSettingsBatchLoader,
        [Service] IUserSettingsService userSettingsService,
        CancellationToken cancellationToken) =>
        (await userSettingsService.GetAndParse(
            userSettingsBatchLoader,
            _userId,
            machineId: null,
            UserSettingIds.IsUnitRepresentationInSi,
            value =>
            {
                if (bool.TryParse(value, out var valueAsBool))
                {
                    return valueAsBool ? Settings.UnitRepresentation.Si : Settings.UnitRepresentation.NonSi;
                }

                return null;
            },
            cancellationToken: cancellationToken)) as UnitRepresentation?;

    /// <summary>
    /// The database id of the default extrusion dashboard of the logged-in user.
    /// </summary>
    public async Task<string?> FavoriteDashboardDatabaseIdExtrusion(
        UserSettingsBatchLoader userSettingsBatchLoader,
        [Service] IUserSettingsService userSettingsService,
        CancellationToken cancellationToken) =>
        await userSettingsService.GetString(
            userSettingsBatchLoader,
            _userId,
            machineId: null,
            UserSettingIds.FavoriteDashboardDatabaseIdExtrusion,
            null,
            cancellationToken);

    /// <summary>
    /// The database id of the default printing dashboard of the logged-in user.
    /// </summary>
    public async Task<string?> FavoriteDashboardDatabaseIdPrinting(
        UserSettingsBatchLoader userSettingsBatchLoader,
        [Service] IUserSettingsService userSettingsService,
        CancellationToken cancellationToken) =>
        await userSettingsService.GetString(
            userSettingsBatchLoader,
            _userId,
            machineId: null,
            UserSettingIds.FavoriteDashboardDatabaseIdPrinting,
            null,
            cancellationToken);

    /// <summary>
    /// The database id of the default papersack dashboard of the logged-in user.
    /// </summary>
    public async Task<string?> FavoriteDashboardDatabaseIdPaperSack(
        UserSettingsBatchLoader userSettingsBatchLoader,
        [Service] IUserSettingsService userSettingsService,
        CancellationToken cancellationToken) =>
        await userSettingsService.GetString(
            userSettingsBatchLoader,
            _userId,
            machineId: null,
            UserSettingIds.FavoriteDashboardDatabaseIdPaperSack,
            null,
            cancellationToken);

    /// <summary>
    /// The database id of the default other dashboard of the logged-in user.
    /// </summary>
    public async Task<string?> FavoriteDashboardDatabaseIdOther(
        UserSettingsBatchLoader userSettingsBatchLoader,
        [Service] IUserSettingsService userSettingsService,
        CancellationToken cancellationToken) =>
        await userSettingsService.GetString(
            userSettingsBatchLoader,
            _userId,
            machineId: null,
            UserSettingIds.FavoriteDashboardDatabaseIdOther,
            null,
            cancellationToken);

    /// <summary>
    /// The option the logged-in user chose on machine department filter.
    /// </summary>
    public async Task<MachineDepartment?> SelectedMachineDepartment(
        UserSettingsBatchLoader userSettingsBatchLoader,
        [Service] IUserSettingsService userSettingsService,
        CancellationToken cancellationToken) =>
        (await userSettingsService.GetAndParse(
            userSettingsBatchLoader,
            _userId,
            machineId: null,
            UserSettingIds.SelectedMachineDepartment,
            value =>
                Enum.TryParse(value, out MachineDepartment parsedValue) ? parsedValue : null,
            cancellationToken: cancellationToken)) as MachineDepartment?;

    /// <summary>
    /// The option the logged-in user chose on printing machine family filter.
    /// </summary>
    public async Task<MachineFamily?> SelectedPrintingMachineFamily(
        UserSettingsBatchLoader userSettingsBatchLoader,
        [Service] IUserSettingsService userSettingsService,
        CancellationToken cancellationToken) =>
        (await userSettingsService.GetAndParse(
            userSettingsBatchLoader,
            _userId,
            machineId: null,
            UserSettingIds.SelectedPrintingMachineFamily,
            value =>
                Enum.TryParse(value, out MachineFamily parsedValue) ? parsedValue : null,
            cancellationToken: cancellationToken)) as MachineFamily?;

    /// <summary>
    /// The option the logged-in user chose on extrusion machine family filter.
    /// </summary>
    public async Task<MachineFamily?> SelectedExtrusionMachineFamily(
        UserSettingsBatchLoader userSettingsBatchLoader,
        [Service] IUserSettingsService userSettingsService,
        CancellationToken cancellationToken) =>
        (await userSettingsService.GetAndParse(
            userSettingsBatchLoader,
            _userId,
            machineId: null,
            UserSettingIds.SelectedExtrusionMachineFamily,
            value =>
                Enum.TryParse(value, out MachineFamily parsedValue) ? parsedValue : null,
            cancellationToken: cancellationToken)) as MachineFamily?;

    /// <summary>
    /// The option the logged-in user chose on paper sack machine family filter.
    /// </summary>
    public async Task<MachineFamily?> SelectedPaperSackMachineFamily(
        UserSettingsBatchLoader userSettingsBatchLoader,
        [Service] IUserSettingsService userSettingsService,
        CancellationToken cancellationToken) =>
        (await userSettingsService.GetAndParse(
            userSettingsBatchLoader,
            _userId,
            machineId: null,
            UserSettingIds.SelectedPaperSackMachineFamily,
            value =>
                Enum.TryParse(value, out MachineFamily parsedValue) ? parsedValue : null,
            cancellationToken: cancellationToken)) as MachineFamily?;

    /// <summary>
    /// The option the logged-in user chose on other machine family filter.
    /// </summary>
    public async Task<MachineFamily?> SelectedOtherMachineFamily(
        UserSettingsBatchLoader userSettingsBatchLoader,
        [Service] IUserSettingsService userSettingsService,
        CancellationToken cancellationToken) =>
        (await userSettingsService.GetAndParse(
            userSettingsBatchLoader,
            _userId,
            machineId: null,
            UserSettingIds.SelectedOtherMachineFamily,
            value =>
                Enum.TryParse(value, out MachineFamily parsedValue) ? parsedValue : null,
            cancellationToken: cancellationToken)) as MachineFamily?;

    /// <summary>
    /// User settings which are set individually for each machine.
    /// </summary>
    /// <param name="machineId">Unique machine identifier (usually WuH equipment number, like: "EQ12345").</param>
    /// <returns></returns>
    [GraphQLIgnore]
    public UserSettingsPerMachine PerMachine(string machineId) => new(machineId);
}