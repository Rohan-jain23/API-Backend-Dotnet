using System;
using System.Threading;
using System.Threading.Tasks;
using GreenDonut;
using WuH.Ruby.OpcUaForwarder.Client;
using DataResult = FrameworkAPI.Models.DataResult<WuH.Ruby.ProcessDataReader.Client.ProcessData>;

namespace FrameworkAPI.DataLoaders;

public class LatestProcessDataCacheDataLoader(IProcessDataCachingService cachingService) : CacheDataLoader<(string machineId, string path), DataResult>
{
    private readonly IProcessDataCachingService _cachingService = cachingService;

    protected override async Task<DataResult> LoadSingleAsync((string machineId, string path) request, CancellationToken cancellationToken)
    {
        // Request data for grouped Keys
        var response = await _cachingService.GetLatestProcessData(
            request.machineId,
            request.path,
            cancellationToken);

        if (response is null)
        {
            return new DataResult(null, new Exception($"Machine {request.machineId} did not return any latest process data"));
        }

        // Assign Value to each requested key and return
        return new DataResult(response, null);
    }
}