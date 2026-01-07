using System;
using System.Collections.Generic;
using System.Linq;
using FrameworkAPI.Exceptions;
using FrameworkAPI.Models;
using FrameworkAPI.Models.DataLoader;
using WuH.Ruby.Common.Core;
using WuH.Ruby.MachineSnapShooter.Client.Models;

namespace FrameworkAPI.Helpers;

public abstract class AggregationBatchHelper
{
    public static IEnumerable<BatchType> GroupRequestKeysIntoBatches<BatchType>(IEnumerable<SnapshotValueRequestKey> keys) where BatchType : SnapshotValueRequestBatch
    {
        var batches = new List<BatchType>();

        foreach (var key in keys)
        {
            var batch = batches.SingleOrDefault(batch =>
            {
                return batch switch
                {
                    SnapshotValueWithLimitRequestBatch snapshotValueWithLimitRequestBatch => snapshotValueWithLimitRequestBatch.CanKeyBeGroupedToBatch((SnapshotValueWithLimitRequestKey)key),
                    _ => batch.CanKeyBeGroupedToBatch(key)
                };
            });

            if (batch is null)
            {
                var newBatch = (BatchType?)Activator.CreateInstance(typeof(BatchType), key) ?? throw new Exception("KeyType must be equal to corresponding BatchType");
                batches.Add(newBatch);
                continue;
            }

            if (!batch.ColumnIds.Contains(key.ColumnId))
            {
                batch.ColumnIds.Add(key.ColumnId);
            }
        }
        return batches;
    }
    public static IEnumerable<GroupedSumRequestBatch> GroupRequestKeysIntoBatches(IEnumerable<GroupedSumRequestKey> groupedSumRequestKeys)
    {
        var batches = new List<GroupedSumRequestBatch>();

        foreach (var groupedSumRequest in groupedSumRequestKeys)
        {
            var batch = batches.SingleOrDefault(batch => batch.CanKeyBeGroupedToBatch(groupedSumRequest));

            if (batch is null)
            {
                var newBatch = (GroupedSumRequestBatch?)Activator.CreateInstance(typeof(GroupedSumRequestBatch), groupedSumRequest) ?? throw new Exception("KeyType must be equal to corresponding BatchType (GroupedSumRequestBatch)");
                batches.Add(newBatch);
                continue;
            }

            if (!batch.GroupAssignments.Contains(groupedSumRequest.GroupAssignment))
            {
                batch.GroupAssignments.Add(groupedSumRequest.GroupAssignment);
            }
        }
        return batches;
    }

    public static IReadOnlyDictionary<SnapshotValueRequestKey, DataResult<ResultType?>> AssignClientResponsesToEachRequestKey<ResultType>(
        IEnumerable<SnapshotValueRequestKey> keys,
        IReadOnlyCollection<(SnapshotValueRequestBatch batch, InternalItemResponse<ValueByColumnId<ResultType>> clientResponse)> clientResponseByBatches)
    {
        var keyToColumnValueDtoDictionary = new Dictionary<SnapshotValueRequestKey, DataResult<ResultType?>>();

        foreach (var key in keys)
        {
            var (batch, response) = clientResponseByBatches
                .Single(clientResponseByBatch => clientResponseByBatch.batch.IsKeyPartOfBatch(key));

            if (response.HasError)
            {
                keyToColumnValueDtoDictionary.Add(key,
                    new DataResult<ResultType?>(default, new InternalServiceException(response.Error)));
                continue;
            }

            if (!response.Item.ContainsKey(key.ColumnId))
            {
                keyToColumnValueDtoDictionary.Add(key,
                    new DataResult<ResultType?>(default, new ColumnDoesNotExistForMachineException(key.ColumnId, batch.MachineId)));
                continue;
            }

            keyToColumnValueDtoDictionary.Add(
                key,
                new DataResult<ResultType?>(
                    response.Item[key.ColumnId],
                    exception: null));
        }

        return keyToColumnValueDtoDictionary;
    }

    public static IReadOnlyDictionary<GroupedSumRequestKey, DataResult<ResultType?>> AssignAvailableClientResponsesToEachRequestKey<ResultType>(
        IEnumerable<GroupedSumRequestKey> groupedSumRequestKeys,
        IReadOnlyCollection<(GroupedSumRequestBatch batch, InternalItemResponse<ValueByColumnId<ResultType>> clientResponse)> clientResponseByBatches)
    {
        var keyToColumnValueDtoDictionary = new Dictionary<GroupedSumRequestKey, DataResult<ResultType?>>();

        foreach (var groupedSumRequestKey in groupedSumRequestKeys)
        {
            var response = clientResponseByBatches
                .Single(clientResponseByBatch => clientResponseByBatch.batch.IsKeyPartOfBatch(groupedSumRequestKey)).clientResponse;

            // Special cases in grouped sums (because/for material consumption): Not all requests have a result.
            // For group assignments which contain a non existing key or value column no result is returned.
            // CASE 1: A non existing valueColumId paired with an existing keyColumID will result in a null response
            if (response is null)
            {
                keyToColumnValueDtoDictionary.Add(groupedSumRequestKey,
                    new DataResult<ResultType?>(default, null));
                continue;
            }

            if (response.HasError)
            {
                keyToColumnValueDtoDictionary.Add(groupedSumRequestKey,
                    new DataResult<ResultType?>(default, new InternalServiceException(response.Error)));
                continue;
            }
            // CASE 2: A non existing keyColumID will result in a null response
            if (!response.Item.ContainsKey(groupedSumRequestKey.GroupAssignment.KeyColumnId))
            {
                keyToColumnValueDtoDictionary.Add(groupedSumRequestKey,
                    new DataResult<ResultType?>(default, null));
                continue;
            }

            keyToColumnValueDtoDictionary.Add(
                groupedSumRequestKey,
                new DataResult<ResultType?>(
                    response.Item[groupedSumRequestKey.GroupAssignment.KeyColumnId],
                    exception: null));
        }

        return keyToColumnValueDtoDictionary;
    }

    public static IReadOnlyDictionary<SnapshotValueWithLimitRequestKey, DataResult<ResultType?>> AssignClientResponsesToEachRequestKey<ResultType>(
        IEnumerable<SnapshotValueWithLimitRequestKey> keys,
        IReadOnlyCollection<(SnapshotValueWithLimitRequestBatch batch, InternalItemResponse<ValueByColumnId<ResultType>> clientResponse)> clientResponseByBatches)
    {
        var keyToColumnValueDtoDictionary = new Dictionary<SnapshotValueWithLimitRequestKey, DataResult<ResultType?>>();

        foreach (var key in keys)
        {
            var response = clientResponseByBatches
                .Single(clientResponseByBatch => clientResponseByBatch.batch.IsKeyPartOfBatch(key)).clientResponse;

            if (response.HasError)
            {
                keyToColumnValueDtoDictionary.Add(key,
                    new DataResult<ResultType?>(default, new InternalServiceException(response.Error)));
                continue;
            }

            keyToColumnValueDtoDictionary.Add(
                key,
                new DataResult<ResultType?>(
                    response.Item[key.ColumnId],
                    exception: null));
        }

        return keyToColumnValueDtoDictionary;
    }
}