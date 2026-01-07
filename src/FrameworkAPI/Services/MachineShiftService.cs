using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.DataLoaders;
using FrameworkAPI.Exceptions;
using FrameworkAPI.Schema.Misc;
using FrameworkAPI.Services.Interfaces;
using Microsoft.Extensions.Logging;
using WuH.Ruby.Settings.Client;

namespace FrameworkAPI.Services;

public class MachineShiftService(
    IShiftSettingsService shiftSettingsService,
    ILogger<MachineShiftService> logger) : IMachineShiftService
{
    public async Task<List<MachineShift>?> GetMachineShifts(
        ProductionPeriodByTimestampCacheDataLoader productionPeriodsCacheDataLoader,
        UserNameCacheDataLoader userNameCacheDataLoader,
        string machineId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        var shiftsResponse = await shiftSettingsService.GetShiftsInTimeRange(
            machineId,
            new WuH.Ruby.Common.Core.TimeRange(from, to),
            "en-US",
            cancellationToken);

        if (shiftsResponse.HasError)
        {
            logger.LogWarning($"{machineId}: Could not get machine shifts. ErrorMessage: {shiftsResponse.Error.ErrorMessage}");
            throw new InternalServiceException(shiftsResponse.Error);
        }

        var results = new List<MachineShift>();
        foreach (var shift in shiftsResponse.Item)
        {
            var operatorName = await GetOperatorName(productionPeriodsCacheDataLoader, userNameCacheDataLoader, machineId, shift, cancellationToken);
            results.Add(new MachineShift(shift.DisplayName, shift.StartTime, shift.EndTime, operatorName));
        }
        return results;
    }

    private async Task<string?> GetOperatorName(
        ProductionPeriodByTimestampCacheDataLoader productionPeriodsCacheDataLoader,
        UserNameCacheDataLoader userNameCacheDataLoader,
        string machineId,
        ActualShift shift,
        CancellationToken cancellationToken)
    {
        var productionPeriodResult = await productionPeriodsCacheDataLoader.LoadAsync((machineId, shift.StartTime.AddMilliseconds(1)), cancellationToken);
        if (productionPeriodResult.Exception is not null)
            throw productionPeriodResult.Exception;

        var firstOperator = productionPeriodResult.Value!.Operators.FirstOrDefault();
        if (firstOperator is null)
            return null;
        if (!Guid.TryParse(firstOperator, out var firstOperatorId))
            return firstOperator;

        var (name, exception) = await userNameCacheDataLoader.LoadAsync(firstOperator, cancellationToken);

        if (exception is not null)
        {
            logger.LogWarning($"{machineId}: Could not resolve name for Guid '{firstOperatorId}'. ErrorMessage: {exception.Message}");
            throw exception;
        }

        return name;
    }
}