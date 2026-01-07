using System;
using System.Collections.Generic;
using System.Linq;
using FrameworkAPI.Exceptions;
using FrameworkAPI.Models;
using FrameworkAPI.Models.DataLoader;
using WuH.Ruby.Common.Core;
using WuH.Ruby.MetaDataHandler.Client;

namespace FrameworkAPI.Helpers;

internal static class MachineMetaDataBatchHelper
{
    public static IEnumerable<BatchType> GroupRequestKeysIntoBatches<BatchType>(IEnumerable<MetaDataRequestKey> keys) where BatchType : MetaDataRequestBatch
    {
        var batches = new List<BatchType>();

        foreach (var key in keys)
        {
            var batch = batches.SingleOrDefault(batch =>
            {
                return batch switch
                {
                    MetaDataRequestBatch processDataRequestBatch => processDataRequestBatch.CanKeyBeGroupedToBatch(key),
                    _ => batch.CanKeyBeGroupedToBatch(key)
                };
            });

            if (batch is null)
            {
                var newBatch = (BatchType?)Activator.CreateInstance(typeof(BatchType), key) ?? throw new Exception("KeyType must be equal to corresponding BatchType");
                batches.Add(newBatch);
                continue;
            }

            if (!batch.Keys.Contains(key))
            {
                batch.Keys.Add(new(key.MachineId, key.Key, key.MetaDataRequestType));
            }
        }
        return batches;
    }

    public static IReadOnlyDictionary<MetaDataRequestKey, DataResult<ProcessVariableMetaDataResponseItem>> AssignClientResponsesToEachRequestKey(
        IEnumerable<MetaDataRequestKey> keys,
        IReadOnlyCollection<(MetaDataRequestBatch batch, InternalListResponse<ProcessVariableMetaDataResponseItem> clientResponse)> clientResponseByBatches)
    {
        var keyToColumnValueDtoDictionary = new Dictionary<MetaDataRequestKey, DataResult<ProcessVariableMetaDataResponseItem>>();

        foreach (var key in keys)
        {
            var (_, clientResponse) = clientResponseByBatches.First(responseByBatch => responseByBatch.batch.IsKeyPartOfBatch(key));

            if (clientResponse.HasError)
            {
                keyToColumnValueDtoDictionary.Add(key,
                    new DataResult<ProcessVariableMetaDataResponseItem>(value: null, new InternalServiceException(clientResponse.Error)));
                continue;
            }
            if (!clientResponse.Items.Any() || clientResponse.Items.Any(item => item is null))
            {
                keyToColumnValueDtoDictionary.Add(key,
                    new DataResult<ProcessVariableMetaDataResponseItem>(value: null, new NullReferenceException("Response value from ProcessMetaDataHandler was empty")));
                continue;
            }

            var responseForKey = clientResponse.Items.FirstOrDefault(
                response =>
                    (key.MetaDataRequestType == MetaDataRequestType.LastPartOfPath && response.Path.EndsWith(key.Key))
                    || (key.MetaDataRequestType == MetaDataRequestType.VariableIdentifier && response.Data.VariableIdentifier.Equals(key.Key)));

            if (responseForKey is null)
            {
                keyToColumnValueDtoDictionary.Add(key,
                    new DataResult<ProcessVariableMetaDataResponseItem>(value: null, new NullReferenceException($"ProcessMetaDataHandler did not return response for '{key.Key}' on machine '{key.MachineId}'")));
                continue;
            }

            keyToColumnValueDtoDictionary.Add(key, new DataResult<ProcessVariableMetaDataResponseItem>(responseForKey, exception: null));
        }

        return keyToColumnValueDtoDictionary;
    }
}