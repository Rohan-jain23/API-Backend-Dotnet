using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.Helpers;
using FrameworkAPI.Models.DataLoader;
using GreenDonut;
using WuH.Ruby.MetaDataHandler.Client;
using DataResult = FrameworkAPI.Models.DataResult<WuH.Ruby.MetaDataHandler.Client.ProcessVariableMetaDataResponseItem>;

namespace FrameworkAPI.DataLoaders;

public class MachineMetaDataBatchDataLoader : BatchDataLoader<MetaDataRequestKey, DataResult>
{
    private readonly IMetaDataHandlerHttpClient _metaDataHandlerHttpClient;

    public MachineMetaDataBatchDataLoader(
        IMetaDataHandlerHttpClient metaDataHandlerHttpClient,
        IBatchScheduler batchScheduler,
        DataLoaderOptions? options = null)
        : base(batchScheduler, options)
    {
        ArgumentNullException.ThrowIfNull(metaDataHandlerHttpClient);
        _metaDataHandlerHttpClient = metaDataHandlerHttpClient;
    }

    protected override async Task<IReadOnlyDictionary<MetaDataRequestKey, DataResult>> LoadBatchAsync(
        IReadOnlyList<MetaDataRequestKey> keys,
        CancellationToken cancellationToken)
    {
        // Group requested keys
        var snapshotClientResponseTasks = keys
        .GroupBy(key => key.MetaDataRequestType)
        .SelectMany(group =>
        {
            var groupKeys = group.ToList();
            var batches = MachineMetaDataBatchHelper.GroupRequestKeysIntoBatches<MetaDataRequestBatch>(groupKeys);

            return batches.Select(async batch =>
            {
                var identifiers = batch.Keys.Select(key => key.Key).ToList();

                if (group.Key == MetaDataRequestType.VariableIdentifier)
                {
                    var variableResponse = await _metaDataHandlerHttpClient.GetProcessVariableMetaDataByIdentifiers(
                        cancellationToken,
                        batch.MachineId,
                        identifiers
                    );

                    return (batch, variableResponse);
                }

                var lastPartOfPathResponse = await _metaDataHandlerHttpClient.GetProcessVariableMetaDataByLastPartOfPathList(
                    cancellationToken,
                    batch.MachineId,
                    identifiers
                );

                return (batch, lastPartOfPathResponse);
            });
        });

        var snapshotClientResponses = await Task.WhenAll(snapshotClientResponseTasks);

        return MachineMetaDataBatchHelper.AssignClientResponsesToEachRequestKey(keys, snapshotClientResponses);
    }
}