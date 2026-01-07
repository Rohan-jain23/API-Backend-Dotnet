using System;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.Exceptions;
using GreenDonut;
using WuH.Ruby.MachineSnapShooter.Client;
using DataResult = FrameworkAPI.Models.DataResult<System.DateTime?>;

namespace FrameworkAPI.DataLoaders;

public class
    SnapshotColumnIdChangedTimestampCacheDataLoader : CacheDataLoader<(string MachineId, string ColumnId, DateTime ValueTimestamp), DataResult>
{
    private readonly IMachineSnapshotHttpClient _machineSnapshotHttpClient;

    public SnapshotColumnIdChangedTimestampCacheDataLoader(
        IMachineSnapshotHttpClient machineSnapshotHttpClient,
        DataLoaderOptions? options = null)
        : base(options)
    {
        ArgumentNullException.ThrowIfNull(machineSnapshotHttpClient);
        _machineSnapshotHttpClient = machineSnapshotHttpClient;
    }

    protected override async Task<DataResult> LoadSingleAsync(
        (string MachineId, string ColumnId, DateTime ValueTimestamp) key, CancellationToken cancellationToken)
    {
        var response = await _machineSnapshotHttpClient.GetSnapshotColumnValueChangedTimestampAfterValueTimestamp(
            key.MachineId, key.ColumnId, key.ValueTimestamp, cancellationToken);

        if (response.HasError)
        {
            return new DataResult(value: null, new InternalServiceException(response.Error));
        }

        return new DataResult(value: response.Item?.ValueEqualSince, exception: null);
    }
}