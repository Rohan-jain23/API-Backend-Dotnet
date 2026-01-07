using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.DataLoaders;
using FrameworkAPI.Schema.Misc;

namespace FrameworkAPI.Services.Interfaces;

public interface IAlarmService
{
    Task<List<MachineAlarm>?> GetAlarmsByMachineIdAndTime(
        string machineId,
        DateTime from,
        DateTime to,
        int skip,
        int take,
        bool sortDescending,
        string? alarmCodeFilterRegex,
        bool onlyPrimalAlarms,
        string languageTag,
        CancellationToken cancellationToken);

    Task<long?> GetAlarmCount(
        string machineId,
        DateTime? from,
        DateTime? to,
        string? alarmCodeFilterRegex,
        bool? onlyPrimalAlarms,
        CancellationToken cancellationToken);

    Task<List<MachineAlarm>?> GetActiveAlarms(
        ActiveAlarmsCacheDataLoader activeAlarmsCacheDataLoader,
        string machineId,
        int skip,
        int take,
        bool sortDescending,
        string? alarmCodeFilterRegex,
        string languageTag,
        CancellationToken cancellationToken);

    Task<long?> GetActiveAlarmsCount(
        ActiveAlarmsCacheDataLoader activeAlarmsCacheDataLoader,
        string machineId,
        string? alarmCodeFilterRegex,
        CancellationToken cancellationToken);

    Task<MachineAlarm?> GetActivePrimalAlarm(
        ActiveAlarmsCacheDataLoader activeAlarmsCacheDataLoader,
        string machineId,
        string languageTag,
        CancellationToken cancellationToken);
}