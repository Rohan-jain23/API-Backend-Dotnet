using System;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.Exceptions;
using GreenDonut;
using WuH.Ruby.MachineSnapShooter.Client;
using WuH.Ruby.MachineSnapShooter.Client.Enums;
using DataResult = FrameworkAPI.Models.DataResult<WuH.Ruby.MachineSnapShooter.Client.Models.MachineSnapshotResponse>;

namespace FrameworkAPI.DataLoaders;

public class LatestSnapshotCacheDataLoader : CacheDataLoader<string, DataResult>
{
    private readonly ILatestMachineSnapshotCachingService _cachingService;

    public LatestSnapshotCacheDataLoader(ILatestMachineSnapshotCachingService cachingService)
    {
        ArgumentNullException.ThrowIfNull(cachingService);
        _cachingService = cachingService;
    }

    protected override async Task<DataResult> LoadSingleAsync(string key, CancellationToken cancellationToken)
    {
        var response = await _cachingService.GetLatestMachineSnapshot(
            key,
            cancellationToken);

        if (response.HasError)
        {
            var successful = Enum.TryParse(response.Error.ErrorItem?.ToString(), out MachineSnapshotErrorItemType errorType);
            if (successful && errorType is MachineSnapshotErrorItemType.WaitingForFirstMinutelySnapshot)
            {
                return new DataResult(value: null, exception: null);
            }

            return new DataResult(value: null, new InternalServiceException(response.Error));
        }

        return new DataResult(response.Item, exception: null);
    }
}