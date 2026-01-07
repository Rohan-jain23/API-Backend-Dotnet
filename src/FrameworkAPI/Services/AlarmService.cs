using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.DataLoaders;
using FrameworkAPI.Schema.Misc;
using FrameworkAPI.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using WuH.Ruby.AlarmDataHandler.Client;

namespace FrameworkAPI.Services;

public class AlarmService(IAlarmDataHandlerHttpClient alarmDataHandlerHttpClient) : IAlarmService
{
    private readonly IAlarmDataHandlerHttpClient _alarmDataHandlerHttpClient = alarmDataHandlerHttpClient;

    public async Task<List<MachineAlarm>?> GetAlarmsByMachineIdAndTime(
        string machineId,
        DateTime from,
        DateTime to,
        int skip,
        int take,
        bool sortDescending,
        string? alarmCodeFilterRegex,
        bool onlyPrimalAlarms,
        string languageTag,
        CancellationToken cancellationToken)
    {
        var alarmsResponse = await _alarmDataHandlerHttpClient.GetAlarms(
            cancellationToken,
            machineId,
            from,
            to,
            take,
            skip,
            !sortDescending,
            alarmCodeFilterRegex,
            onlyPrimalAlarms);

        if (alarmsResponse.HasError && alarmsResponse.Error.StatusCode != StatusCodes.Status204NoContent)
        {
            throw alarmsResponse.Error.Exception;
        }
        else if (alarmsResponse.HasError && alarmsResponse.Error.StatusCode == StatusCodes.Status204NoContent)
        {
            return [];
        }

        return alarmsResponse.Items.Select(alarm => new MachineAlarm(alarm, languageTag)).ToList();
    }

    public async Task<long?> GetAlarmCount(
        string machineId,
        DateTime? from,
        DateTime? to,
        string? alarmCodeFilterRegex,
        bool? onlyPrimalAlarms,
        CancellationToken cancellationToken)
    {
        var response = await _alarmDataHandlerHttpClient.GetAlarmCount(cancellationToken, machineId, from, to, alarmCodeFilterRegex, onlyPrimalAlarms);
        if (response.HasError)
        {
            throw response.Error.Exception;
        }

        return response.Item;
    }

    public async Task<List<MachineAlarm>?> GetActiveAlarms(
        ActiveAlarmsCacheDataLoader activeAlarmsCacheDataLoader,
        string machineId,
        int skip,
        int take,
        bool sortDescending,
        string? alarmCodeFilterRegex,
        string languageTag,
        CancellationToken cancellationToken)
    {
        var response = await activeAlarmsCacheDataLoader.LoadAsync(machineId, cancellationToken);

        if (response.Exception is not null)
        {
            throw response.Exception;
        }

        var activeAlarms = response.Value!;
        if (sortDescending)
            activeAlarms = [.. activeAlarms.OrderByDescending(alarm => alarm.StartTimestamp)];
        else
            activeAlarms = [.. activeAlarms.OrderBy(alarm => alarm.StartTimestamp)];

        return FilterAlarmByRegexWhenFilterIsNotNull(alarmCodeFilterRegex, activeAlarms)
            .Skip(skip)
            .Take(take)
            .Select(internalAlarm => new MachineAlarm(internalAlarm, languageTag))
            .ToList();
    }

    public async Task<long?> GetActiveAlarmsCount(
        ActiveAlarmsCacheDataLoader activeAlarmsCacheDataLoader,
        string machineId,
        string? alarmCodeFilterRegex,
        CancellationToken cancellationToken)
    {
        var response = await activeAlarmsCacheDataLoader.LoadAsync(machineId, cancellationToken);

        if (response.Exception is not null)
        {
            throw response.Exception;
        }

        return FilterAlarmByRegexWhenFilterIsNotNull(alarmCodeFilterRegex, response.Value!).Count;
    }

    public async Task<MachineAlarm?> GetActivePrimalAlarm(
        ActiveAlarmsCacheDataLoader activeAlarmsCacheDataLoader,
        string machineId,
        string languageTag,
        CancellationToken cancellationToken)
    {
        var response = await activeAlarmsCacheDataLoader.LoadAsync(machineId, cancellationToken);

        if (response.Exception is not null)
        {
            throw response.Exception;
        }

        var alarm = response.Value!.FirstOrDefault(alarm => alarm.IsPrimal);
        return alarm is null ? null : new MachineAlarm(alarm, languageTag);
    }

    private static List<Alarm> FilterAlarmByRegexWhenFilterIsNotNull(string? alarmCodeFilterRegex, List<Alarm> activeAlarms)
    {
        if (!string.IsNullOrWhiteSpace(alarmCodeFilterRegex))
        {
            var regex = new Regex(alarmCodeFilterRegex);
            activeAlarms = activeAlarms.Where(alarm => regex.IsMatch(alarm.AlarmCode)).ToList();
        }

        return activeAlarms;
    }
}