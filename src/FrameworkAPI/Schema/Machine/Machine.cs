using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.DataLoaders;
using FrameworkAPI.Helpers;
using FrameworkAPI.Models.Settings;
using FrameworkAPI.Schema.Misc;
using FrameworkAPI.Services.Interfaces;
using FrameworkAPI.Services.Settings;
using HotChocolate;
using HotChocolate.Types;
using MachineDataHandler = WuH.Ruby.MachineDataHandler.Client;

namespace FrameworkAPI.Schema.Machine;

/// <summary>
/// Generic interface for machine entities of all machine families.
/// </summary>
[InterfaceType]
public abstract class Machine(MachineDataHandler.Machine machine)
{

    /// <summary>
    /// Unique identifier (usually WuH equipment number, like: "EQ12345") of the machine.
    /// [Source: Machine]
    /// </summary>
    public string MachineId { get; set; } = machine.MachineId;

    /// <summary>
    /// Friendly name of the machine.
    /// [Source: Setting in Admin]
    /// </summary>
    public string Name { get; set; } = machine.Name;

    /// <summary>
    /// The WuH department the machine is belonging to.
    /// [Source: Machine]
    /// </summary>
    public MachineDepartment Department { get; set; } = machine.BusinessUnit.MapToSchemaMachineDepartment();

    /// <summary>
    /// Family / generic type of the machine.
    /// [Source: Machine]
    /// </summary>
    public MachineFamily MachineFamily { get; set; } = machine.MachineFamilyEnum.MapToSchemaMachineFamily();

    /// <summary>
    /// Detailed type of the machine.
    /// [Source: Machine]
    /// </summary>
    public string MachineType { get; set; } = machine.MachineType;

    /// <summary>
    /// Features of the machine.
    /// [Source: Machine]
    /// </summary>
    public MachineFeatures Features { get; set; } = new MachineFeatures(machine.Features);

    /// <summary>
    /// Query timestamp is not provided:
    /// Machines OPC-UA server time and latest snapshot time are compared and the latest one is being returned.
    ///
    /// Query timestamp is provided:
    /// Query timestamp is being returned.
    ///
    /// [Source: Machines OPC-UA server time, latest snapshot or query timestamp]
    /// </summary>
    public async Task<DateTime?> Time(
        [Service] IMachineTimeService machineTimeService,
        CancellationToken cancellationToken)
    {
        if (QueryTimestamp is not null)
        {
            return QueryTimestamp;
        }

        var (time, exception) = await machineTimeService.Get(MachineId, cancellationToken);

        if (exception is not null)
        {
            throw exception;
        }

        return time;
    }

    /// <summary>
    /// The current production status of the machine.
    /// [Source: MachineSnapshot]
    /// </summary>
    public ProductionStatus ProductionStatus() => new(MachineId, QueryTimestamp);

    /// <summary>
    /// The job entity that is currently produced on the machine if no timestamp is provided.
    /// Otherwise the job entity that was active at the given timestamp.
    /// </summary>
    public Task<ProducedJob.ProducedJob?> ProducedJob(
        [Service] IProducedJobService service, CancellationToken cancellationToken)
        => service.GetProducedJob(MachineId, Department, MachineFamily, QueryTimestamp, cancellationToken);

    /// <summary>
    /// Status of licenses for RUBY extensions and connection modules.
    /// [Source: LicenseManager]
    /// </summary>
    public Task<RubyLicenses?> Licenses([Service] ILicenceService service, CancellationToken cancellationToken)
        => service.GetMachineLicenses(MachineId, cancellationToken);

    /// <summary>
    /// The currently active machine alarm that was started first after the current problem occurred.
    /// Is null, if no alarm is active on the machine.
    /// [Source: AlarmDataHandler]
    /// </summary>
    public async Task<MachineAlarm?> ActivePrimalMachineAlarm(
        [GlobalState] string userId,
        [Service] IAlarmService alarmService,
        [Service] IUserSettingsService userSettingsService,
        UserSettingsBatchLoader userSettingsBatchLoader,
        ActiveAlarmsCacheDataLoader activeAlarmsCacheDataLoader,
        CancellationToken cancellationToken = default)
    {
        if (QueryTimestamp is not null)
            throw new NotImplementedException("Active alarms can not be queried for a historic machine timestamp.");

        var languageTag = await userSettingsService.GetString(userSettingsBatchLoader, userId, machineId: null, UserSettingIds.Language, cancellationToken: cancellationToken);
        return await alarmService.GetActivePrimalAlarm(activeAlarmsCacheDataLoader, MachineId, languageTag!, cancellationToken);
    }

    /// <summary>
    /// All alarms that are currently active on the machine.
    /// [Source: AlarmDataHandler]
    /// </summary>
    /// <param name="userId">Id of the logged-in user (automatically resolved)</param>
    /// <param name="alarmService">Used internal service</param>
    /// <param name="userSettingsService">Used internal service</param>
    /// <param name="userSettingsBatchLoader">Internal batch loader</param>
    /// <param name="activeAlarmsCacheDataLoader">Internal cache loader</param>
    /// <param name="skip">Number of alarms that are skipped (can be used for pagination)</param>
    /// <param name="take">Maximum number of alarms that are returned (can be used for pagination)</param>
    /// <param name="sortDescending">If true, the returned alarms are sorted in descending order (by default the alarms are returned in ascending order)</param>
    /// <param name="alarmCodeFilterRegex">Regular expression on the alarm code field to filter the returned alarms.</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>List of <see cref="MachineAlarm"/>.</returns>
    public async Task<List<MachineAlarm>?> ActiveMachineAlarms(
        [GlobalState] string userId,
        [Service] IAlarmService alarmService,
        [Service] IUserSettingsService userSettingsService,
        UserSettingsBatchLoader userSettingsBatchLoader,
        ActiveAlarmsCacheDataLoader activeAlarmsCacheDataLoader,
        int skip = 0,
        int take = 10,
        bool sortDescending = false,
        string? alarmCodeFilterRegex = null,
        CancellationToken cancellationToken = default)
    {
        if (QueryTimestamp is not null)
            throw new NotImplementedException("Active alarms can not be queried for a historic machine timestamp.");

        var languageTag = await userSettingsService.GetString(userSettingsBatchLoader, userId, machineId: null, UserSettingIds.Language, cancellationToken: cancellationToken);
        return await alarmService.GetActiveAlarms(
            activeAlarmsCacheDataLoader,
            MachineId,
            skip,
            take,
            sortDescending,
            alarmCodeFilterRegex,
            languageTag!,
            cancellationToken);
    }

    /// <summary>
    /// Number of all alarms that are currently active on the machine.
    /// [Source: AlarmDataHandler]
    /// </summary>
    /// <param name="alarmService">Internal service.</param>
    /// <param name="activeAlarmsCacheDataLoader">Internal cache loader</param>
    /// <param name="alarmCodeFilterRegex">Regular expression on the alarm code field to filter the counted alarms.</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>Number of machine alarms.</returns>
    public async Task<long?> ActiveMachineAlarmCount(
        [Service] IAlarmService alarmService,
        ActiveAlarmsCacheDataLoader activeAlarmsCacheDataLoader,
        string? alarmCodeFilterRegex = null,
        CancellationToken cancellationToken = default)
    {
        if (QueryTimestamp is not null)
            throw new NotImplementedException("Active alarms can not be queried for a historic machine timestamp.");

        return await alarmService.GetActiveAlarmsCount(activeAlarmsCacheDataLoader, MachineId, alarmCodeFilterRegex, cancellationToken);
    }

    /// <summary>
    /// Timestamp to apply when querying the machine.
    /// If this is null, the current status of the machine is queried.
    /// Otherwise, the historic values on this timestamp are returned.
    /// </summary>
    [GraphQLIgnore]
    internal DateTime? QueryTimestamp { get; set; }

    internal static Machine CreateInstance(MachineDataHandler.Machine machine)
    {
        return machine.BusinessUnit switch
        {
            MachineDataHandler.BusinessUnit.Printing => new PrintingMachine(machine),
            MachineDataHandler.BusinessUnit.PaperSack => new PaperSackMachine(machine),
            MachineDataHandler.BusinessUnit.Extrusion => new ExtrusionMachine(machine),
            _ => new OtherMachine(machine)
        };
    }
}