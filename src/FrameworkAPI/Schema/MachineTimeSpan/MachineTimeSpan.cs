using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.DataLoaders;
using FrameworkAPI.Extensions;
using FrameworkAPI.Models.Settings;
using FrameworkAPI.Schema.Misc;
using FrameworkAPI.Services.Interfaces;
using FrameworkAPI.Services.Settings;
using HotChocolate;
using HotChocolate.Types;
using Microsoft.AspNetCore.Http;

namespace FrameworkAPI.Schema.MachineTimeSpan;

/// <summary>
/// Generic interface for machine time span entities of all machine families.
/// </summary>
[InterfaceType]
public abstract class MachineTimeSpan(string machineId, MachineDepartment machineDepartment, DateTime from, DateTime to)
{

    /// <summary>
    /// Unique identifier (usually WuH equipment number, like: "EQ12345") of the machine.
    /// [Source: Machine]
    /// </summary>
    public string MachineId { get; set; } = machineId;

    /// <summary>
    /// The WuH department the machine is belonging to.
    /// [Source: Machine]
    /// </summary>
    internal MachineDepartment Department { get; set; } = machineDepartment;

    /// <summary>
    /// The start timestamp of the query.
    /// [Source: Query]
    /// </summary>
    public DateTime From { get; set; } = from;

    /// <summary>
    /// The end timestamp of the query.
    /// [Source: Query]
    /// </summary>
    public DateTime To { get; set; } = to;

    /// <summary>
    /// The trend of the machines production status.
    /// The trend elements are sorted by the time (ascending).
    /// The trend will contain one element for each minute in the time-span.
    /// [Source: MachineSnapshots]
    /// </summary>
    [GraphQLIgnore]
    public List<ProductionStatusTrendItem>? ProductionStatusTrend { get; set; }

    /// <summary>
    /// Cumulated minutes the machine was in each production status during this time span.
    /// Also, the total times are provided.
    /// [Source: MachineSnapshots]
    /// </summary>
    [GraphQLIgnore]
    public ProductionTimes? ProductionTimes { get; set; }

    /// <summary>
    /// OEE during this time span.
    /// The overall equipment effectiveness (OEE) is a measure that identifies the percentage of production time that is truly productive.
    /// The OEE is calculated from three sub values (Availability, Effectiveness, Quality) which indicate how the productivity was lost.
    /// [Source: MachineSnapshots]
    /// </summary>
    [GraphQLIgnore]
    public OeeValues? OverallEquipmentEffectiveness { get; set; }

    /// <summary>
    /// Alarms which were active on the machine during the query time span.
    /// [Source: AlarmDataHandler]
    /// </summary>
    /// <param name="userId">Id of the logged-in user (automatically resolved)</param>
    /// <param name="userSettingsBatchLoader">Internal batch loader</param>
    /// <param name="alarmService">Used internal service</param>
    /// <param name="userService">Used internal service</param>
    /// <param name="skip">Number of alarms that are skipped (can be used for pagination)</param>
    /// <param name="take">Maximum number of alarms that are returned (can be used for pagination)</param>
    /// <param name="sortDescending">If true, the returned alarms are sorted in descending order (by default the alarms are returned in ascending order)</param>
    /// <param name="alarmCodeFilterRegex">Regular expression on the alarm code field to filter the returned alarms.</param>
    /// <param name="onlyPrimalAlarms">If set, only the alarms are returned that are started first after a problem occurred.</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>List of <see cref="MachineAlarm"/>.</returns>
    public async Task<List<MachineAlarm>?> MachineAlarms(
        [GlobalState] string userId,
        UserSettingsBatchLoader userSettingsBatchLoader,
        [Service] IAlarmService alarmService,
        [Service] IUserSettingsService userService,
        int skip = 0,
        int take = 100,
        bool sortDescending = false,
        string? alarmCodeFilterRegex = null,
        bool onlyPrimalAlarms = false,
        CancellationToken cancellationToken = default)
    {
        var languageTag = await userService.GetString(userSettingsBatchLoader, userId, machineId: null, UserSettingIds.Language, cancellationToken: cancellationToken);
        return await alarmService.GetAlarmsByMachineIdAndTime(MachineId, From, To, skip, take, sortDescending, alarmCodeFilterRegex, onlyPrimalAlarms, languageTag!, cancellationToken);
    }

    /// <summary>
    /// Number of alarms which were active on the machine during the query time span.
    /// [Source: AlarmDataHandler]
    /// </summary>
    /// <param name="alarmService">Internal service.</param>
    /// <param name="alarmCodeFilterRegex">Regular expression on the alarm code field to filter the counted alarms.</param>
    /// <param name="onlyPrimalAlarms">If set, only the alarms are counted that are started first after a problem occurred.</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>Number of machine alarms.</returns>
    public async Task<long?> MachineAlarmCount(
        [Service] IAlarmService alarmService,
        string? alarmCodeFilterRegex = null,
        bool onlyPrimalAlarms = false,
        CancellationToken cancellationToken = default) =>
            await alarmService.GetAlarmCount(MachineId, From, To, alarmCodeFilterRegex, onlyPrimalAlarms, cancellationToken);

    /// <summary>
    /// Returns all shifts during the query time span.
    /// Usually, the machine is operated by different operators during the day.
    /// The period of time one operator is working on the machine is called 'shift'.
    /// Because of performance reasons, it is not possible to subscribe to this property (-> returns 'null').
    /// [Source: Settings + ProductionPeriods (for operators)]
    /// </summary>
    public async Task<List<MachineShift>?> Shifts(
        ProductionPeriodByTimestampCacheDataLoader productionPeriodsCacheDataLoader,
        UserNameCacheDataLoader userNameCacheDataLoader,
        [Service] IMachineShiftService machineShiftService,
        [Service] IHttpContextAccessor context,
        CancellationToken cancellationToken)
    {
        if (context.HttpContext.IsSubscriptionOrNull())
            return null;

        return await machineShiftService.GetMachineShifts(productionPeriodsCacheDataLoader, userNameCacheDataLoader, MachineId, From, To, cancellationToken);
    }

    internal static MachineTimeSpan CreateInstance(string machineId, MachineDepartment machineDepartment, DateTime from, DateTime to)
    {
        return machineDepartment switch
        {
            MachineDepartment.Printing => new PrintingMachineTimeSpan(machineId, machineDepartment, from, to),
            MachineDepartment.PaperSack => new PaperSackMachineTimeSpan(machineId, machineDepartment, from, to),
            MachineDepartment.Extrusion => new ExtrusionMachineTimeSpan(machineId, machineDepartment, from, to),
            _ => new OtherMachineTimeSpan(machineId, machineDepartment, from, to)
        };
    }
}