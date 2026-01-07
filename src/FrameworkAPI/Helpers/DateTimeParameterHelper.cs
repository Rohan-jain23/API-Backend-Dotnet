using System;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.Services.Interfaces;
using HotChocolate;

namespace FrameworkAPI.Helpers;

public static class DateTimeParameterHelper
{
    public static async Task<DateTime> GetValidTimeForRequest(
        [Service] IMachineTimeService machineTimeService,
        DateTime? dateTime,
        string machineId,
        CancellationToken cancellationToken)
    {
        DateTime validTimeForRequest;
        if (dateTime is null || dateTime.Value.Equals(DateTime.MinValue))
        {
            var (dateTimeResult, exception) = await machineTimeService.Get(machineId, cancellationToken);
            if (exception is not null)
            {
                throw exception;
            }

            validTimeForRequest = dateTimeResult ?? DateTime.UtcNow;
        }
        else
        {
            validTimeForRequest = dateTime.Value;
        }

        return validTimeForRequest;
    }
}