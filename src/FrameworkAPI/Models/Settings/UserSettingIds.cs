namespace FrameworkAPI.Models.Settings;

/// <summary>
/// The settings for user preferences.
/// </summary>
public static class UserSettingIds
{
    /// <summary>
    /// The SettingId for users preferred language in the settings collection.
    /// </summary>
    public const string Language = "Language";

    /// <summary>
    /// The SettingId for users preferred unit system in the settings collection.
    /// </summary>
    public const string IsUnitRepresentationInSi = "IsUnitRepresentationInSi";

    /// <summary>
    /// The SettingId for users favorite extrusion dashboard database id.
    /// </summary>
    public const string FavoriteDashboardDatabaseIdExtrusion = "FavoriteDashboardDatabaseIdExtrusion";

    /// <summary>
    /// The SettingId for users favorite printing dashboard database id.
    /// </summary>
    public const string FavoriteDashboardDatabaseIdPrinting = "FavoriteDashboardDatabaseIdPrinting";

    /// <summary>
    /// The SettingId for users favorite papersack dashboard database id.
    /// </summary>
    public const string FavoriteDashboardDatabaseIdPaperSack = "FavoriteDashboardDatabaseIdPaperSack";

    /// <summary>
    /// The SettingId for users favorite other dashboard database id.
    /// </summary>
    public const string FavoriteDashboardDatabaseIdOther = "FavoriteDashboardDatabaseIdOther";

    /// <summary>
    /// The SettingId for users preferred machine department.
    /// </summary>
    public const string SelectedMachineDepartment = "MachineDepartment";

    /// <summary>
    /// The SettingId for users preferred machine family for printing business unit.
    /// </summary>
    public const string SelectedPrintingMachineFamily = "PrintingMachineFamily";

    /// <summary>
    /// The SettingId for users preferred machine family for extrusion business unit.
    /// </summary>
    public const string SelectedExtrusionMachineFamily = "ExtrusionMachineFamily";

    /// <summary>
    /// The SettingId for users preferred machine family for paper sack business unit.
    /// </summary>
    public const string SelectedPaperSackMachineFamily = "PaperSackMachineFamily";

    /// <summary>
    /// The SettingId for users preferred machine family for other business unit.
    /// </summary>
    public const string SelectedOtherMachineFamily = "OtherMachineFamily";
}