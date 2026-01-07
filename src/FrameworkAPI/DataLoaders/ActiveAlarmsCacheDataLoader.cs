using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.Exceptions;
using GreenDonut;
using WuH.Ruby.AlarmDataHandler.Client;
using DataResult = FrameworkAPI.Models.DataResult<System.Collections.Generic.List<WuH.Ruby.AlarmDataHandler.Client.Alarm>>;

namespace FrameworkAPI.DataLoaders;

public class ActiveAlarmsCacheDataLoader(IAlarmDataHandlerCachingService alarmDataHandlerCachingService) : CacheDataLoader<string, DataResult>
{
    private readonly IAlarmDataHandlerCachingService _alarmDataHandlerCachingService = alarmDataHandlerCachingService;

    protected override async Task<DataResult> LoadSingleAsync(string machineId, CancellationToken cancellationToken)
    {
        var response = await _alarmDataHandlerCachingService.GetActiveAlarms(machineId, cancellationToken);

        if (response.HasError && response.Error.StatusCode != 204)
        {
            return new DataResult(null, new InternalServiceException(response.Error));
        }
        else if (response.HasError && response.Error.StatusCode == 204)
        {
            return new DataResult([], null);
        }

        return new DataResult(response.Items, null);
    }
}