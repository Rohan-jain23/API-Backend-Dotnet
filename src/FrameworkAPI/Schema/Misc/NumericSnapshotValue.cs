using System;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.DataLoaders;
using FrameworkAPI.Services.Interfaces;
using HotChocolate;

namespace FrameworkAPI.Schema.Misc;

/// <summary>
/// The value of a numeric variable.
/// This is only supported for values that can be derived from MachineSnapshots.
/// </summary>
public class NumericSnapshotValue(string columnId, DateTime? queryTimestamp, string machineId)
{
    private readonly string _columnId = columnId;
    private readonly DateTime? _queryTimestamp = queryTimestamp;
    private readonly string _machineId = machineId;

    /// <summary>
    /// The last known value of the variable before the query timestamp
    /// (if the query timestamp is 'null', this is the live-value)
    /// </summary>
    public async Task<double?> LastValue(
        LatestSnapshotCacheDataLoader latestSnapshotCacheDataLoader,
        SnapshotByTimestampBatchDataLoader snapshotByTimestampBatchDataLoader,
        [Service] IMachineSnapshotService service,
        CancellationToken cancellationToken)
    {
        var snapshotResult = _queryTimestamp is null
            ? await service.GetLatestColumnValue(latestSnapshotCacheDataLoader, _columnId, _machineId, cancellationToken)
            : await service.GetColumnValue(snapshotByTimestampBatchDataLoader, _columnId, _queryTimestamp.Value, _machineId, cancellationToken);

        if (snapshotResult.Exception is not null)
        {
            throw snapshotResult.Exception;
        }

        return snapshotResult.Value?.ColumnValue is null ? null : Convert.ToDouble(snapshotResult.Value.ColumnValue);
    }

    /// <summary>
    /// The unit of the last value
    /// (if the unit needs to be translated, the corresponding i18n tag is provided here; for example 'label.items').
    /// </summary>
    public async Task<string?> Unit(
        LatestSnapshotCacheDataLoader latestSnapshotCacheDataLoader,
        [Service] IMachineSnapshotService service,
        CancellationToken cancellationToken)
    {
        var unitResult = await service.GetLatestColumnUnit(latestSnapshotCacheDataLoader, _columnId, _machineId, cancellationToken);

        if (unitResult.Exception is not null)
        {
            throw unitResult.Exception;
        }

        return unitResult.Value;
    }
}