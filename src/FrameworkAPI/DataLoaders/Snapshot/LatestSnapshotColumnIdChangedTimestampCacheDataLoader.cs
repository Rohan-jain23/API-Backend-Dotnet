using System;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.Services.Interfaces;
using GreenDonut;
using DataResult = FrameworkAPI.Models.DataResult<System.DateTime?>;

namespace FrameworkAPI.DataLoaders;

public class
    LatestSnapshotColumnIdChangedTimestampCacheDataLoader : CacheDataLoader<(string MachineId, string ColumnId),
        DataResult>
{
    private readonly ISnapshotColumnIdChangedTimestampCachingService _snapshotColumnIdChangedTimestampCachingService;

    public LatestSnapshotColumnIdChangedTimestampCacheDataLoader(
        ISnapshotColumnIdChangedTimestampCachingService snapshotColumnIdChangedTimestampCachingService,
        DataLoaderOptions? options = null)
        : base(options)
    {
        ArgumentNullException.ThrowIfNull(snapshotColumnIdChangedTimestampCachingService);
        _snapshotColumnIdChangedTimestampCachingService = snapshotColumnIdChangedTimestampCachingService;
    }

    protected override async Task<DataResult> LoadSingleAsync(
        (string MachineId, string ColumnId) key, CancellationToken cancellationToken)
    {
        try
        {
            var changedTimestamp =
                await _snapshotColumnIdChangedTimestampCachingService.Get(
                    key.MachineId, key.ColumnId, cancellationToken);

            return new DataResult(changedTimestamp, exception: null);
        }
        catch (Exception ex)
        {
            return new DataResult(value: null, ex);
        }
    }
}