using System;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.DataLoaders;
using FrameworkAPI.Services.Interfaces;
using HotChocolate;
using WuH.Ruby.MachineSnapShooter.Client;
using WuH.Ruby.ProductionPeriodsDataHandler.Client;

namespace FrameworkAPI.Schema.Misc;

/// <summary>
/// Production status of the machine.
/// Each machine type defines its own status and assigns it to a generic status category.
/// The production status might be changed automatically by RUBY or a user.
/// </summary>
public class ProductionStatus(string machineId, DateTime? queryTimestamp)
{
    private readonly string _machineId = machineId;
    private readonly DateTime? _queryTimestamp = queryTimestamp;

    /// <summary>
    /// Unique id for this status.
    /// </summary>
    public async Task<int?> Id(
        LatestSnapshotCacheDataLoader latestSnapshotDataLoaderDataLoader,
        SnapshotByTimestampBatchDataLoader snapshotByTimestampDataLoader,
        [Service] IMachineSnapshotService service,
        CancellationToken ct)
    {
        var (snapshotValue, exception) = _queryTimestamp is null
            ? await service.GetLatestColumnValue(
                latestSnapshotDataLoaderDataLoader,
                SnapshotColumnIds.ProductionStatusId,
                _machineId,
                ct)
            : await service.GetColumnValue(
                snapshotByTimestampDataLoader,
                SnapshotColumnIds.ProductionStatusId,
                _queryTimestamp.Value,
                _machineId,
                ct);

        if (exception is not null)
        {
            throw exception;
        }

        if (snapshotValue!.IsCreatedByVirtualTime is null || snapshotValue.IsCreatedByVirtualTime.Value)
        {
            return -11;
        }

        // When the machine is in SnapShooter state WaitingForFirstMinutelySnapshot the columnValue is null.
        return snapshotValue.ColumnValue is null
            ? -11
            : Convert.ToInt32(snapshotValue.ColumnValue);
    }

    /// <summary>
    /// Generic status category which this status is assigned to.
    /// </summary>
    public async Task<ProductionStatusCategory?> Category(
        LatestSnapshotCacheDataLoader latestSnapshotDataLoaderDataLoader,
        SnapshotByTimestampBatchDataLoader snapshotByTimestampDataLoader,
        [Service] IMachineSnapshotService service,
        CancellationToken ct)
    {
        var (snapshotValue, exception) = _queryTimestamp is null
            ? await service.GetLatestColumnValue(
                latestSnapshotDataLoaderDataLoader,
                SnapshotColumnIds.ProductionStatusCategory,
                _machineId,
                ct)
            : await service.GetColumnValue(
                snapshotByTimestampDataLoader,
                SnapshotColumnIds.ProductionStatusCategory,
                _queryTimestamp.Value,
                _machineId,
                ct);

        if (exception is not null)
        {
            throw exception;
        }

        if (snapshotValue is null)
        {
            throw new NullReferenceException("Production status id can't be null.");
        }

        if (snapshotValue.IsCreatedByVirtualTime is null || snapshotValue.IsCreatedByVirtualTime.Value)
        {
            return ProductionStatusCategory.Offline;
        }

        // When the machine is in SnapShooter state WaitingForFirstMinutelySnapshot the columnValue is null.
        return snapshotValue.ColumnValue is null
            ? ProductionStatusCategory.Offline
            : MapStatusCategory(Enum.Parse<ProductionStatusCategoryForPublic>((string)snapshotValue.ColumnValue));
    }

    /// <summary>
    /// The timestamp the machine went into this production status.
    /// </summary>
    public async Task<DateTime?> StartTime(
        LatestSnapshotColumnIdChangedTimestampCacheDataLoader latestSnapshotColumnIdChangedTimestampDataLoader,
        SnapshotColumnIdChangedTimestampCacheDataLoader snapshotColumnIdChangedTimestampDataLoader,
        [Service] IMachineSnapshotService service,
        CancellationToken ct)
    {
        var (changedTimestamp, exception) = _queryTimestamp is null
            ? await service.GetLatestColumnChangedTimestamp(
                latestSnapshotColumnIdChangedTimestampDataLoader,
                SnapshotColumnIds.ProductionStatusCategory,
                _machineId,
                ct)
            : await service.GetColumnChangedTimestamp(
                snapshotColumnIdChangedTimestampDataLoader,
                SnapshotColumnIds.ProductionStatusCategory,
                _queryTimestamp.Value,
                _machineId,
                ct);

        if (exception is not null)
        {
            throw exception;
        }

        return changedTimestamp;
    }

    /// <summary>
    /// The timestamp the machine went into another production status
    /// (this is 'null' for the current production status).
    /// </summary>
    [GraphQLIgnore]
    public DateTime? EndTime { get; set; }

    private static ProductionStatusCategory MapStatusCategory(
        ProductionStatusCategoryForPublic productionStatusCategory)
    {
        return productionStatusCategory switch
        {
            ProductionStatusCategoryForPublic.DownTime => ProductionStatusCategory.DownTime,
            ProductionStatusCategoryForPublic.ScheduledNonProduction => ProductionStatusCategory.ScheduledNonProduction,
            ProductionStatusCategoryForPublic.Production => ProductionStatusCategory.Production,
            ProductionStatusCategoryForPublic.Scrap => ProductionStatusCategory.Scrap,
            ProductionStatusCategoryForPublic.Setup => ProductionStatusCategory.Setup,
            ProductionStatusCategoryForPublic.Offline => ProductionStatusCategory.Offline,
            ProductionStatusCategoryForPublic.InvalidData => ProductionStatusCategory.InvalidData,
            _ => throw new ArgumentException($"Can't map production status category: {productionStatusCategory}")
        };
    }
}