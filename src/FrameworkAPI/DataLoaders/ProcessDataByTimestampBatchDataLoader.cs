using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.Exceptions;
using FrameworkAPI.Models.DataLoader;
using GreenDonut;
using WuH.Ruby.Common.Core;
using WuH.Ruby.ProcessDataReader.Client;
using DataResult = FrameworkAPI.Models.DataResult<WuH.Ruby.ProcessDataReader.Client.ProcessData>;

namespace FrameworkAPI.DataLoaders;

public class ProcessDataByTimestampBatchDataLoader(
    IProcessDataReaderHttpClient processDataReaderHttpClient,
    IBatchScheduler batchScheduler,
    DataLoaderOptions? options = null) : BatchDataLoader<ProcessDataRequestKey, DataResult>(batchScheduler, options)
{
    private readonly IProcessDataReaderHttpClient _processDataReaderHttpClient = processDataReaderHttpClient;

    protected override async Task<IReadOnlyDictionary<ProcessDataRequestKey, DataResult>> LoadBatchAsync(
        IReadOnlyList<ProcessDataRequestKey> keys,
        CancellationToken cancellationToken)
    {
        var processDataResponseByGroup = new Dictionary<(string MachineId, ProcessDataRequestType RequestType, DateTime? Timestamp), InternalListResponse<ProcessData>>();
        var dataResultByKey = new Dictionary<ProcessDataRequestKey, DataResult>();

        // Create response tasks for groups
        var tasks = keys.GroupBy(key => new { key.MachineId, key.ProcessDataRequestType, key.Timestamp })
            .Select(async group =>
            {
                var machineId = group.Key.MachineId;
                var requestType = group.Key.ProcessDataRequestType;
                var timestamp = group.Key.Timestamp;
                var groupIdentifiers = group.Select(request => request.Key).ToList();

                if (group.Key.ProcessDataRequestType == ProcessDataRequestType.VariableIdentifier)
                {
                    var variableResponse = await _processDataReaderHttpClient.GetDataForOneTimestampByIdentifier(cancellationToken, machineId, groupIdentifiers, timestamp);
                    processDataResponseByGroup.Add((machineId, requestType, timestamp), variableResponse);
                    return;
                }

                var pathResponse = await _processDataReaderHttpClient.GetDataForOneTimestampByPath(cancellationToken, machineId, groupIdentifiers, timestamp);
                processDataResponseByGroup.Add((machineId, requestType, timestamp), pathResponse);
            });

        await Task.WhenAll(tasks);

        // Assign value to each requested key
        foreach (var key in keys)
        {
            var response = processDataResponseByGroup[(key.MachineId, key.ProcessDataRequestType, key.Timestamp)];

            if (response.HasError)
            {
                dataResultByKey.Add(key, new DataResult(value: null, new InternalServiceException(response.Error)));
                continue;
            }

            if (response.Items.Any(item => item is null))
            {
                dataResultByKey.Add(key, new DataResult(value: null, new NullReferenceException("Response item was null")));
                continue;
            }

            ProcessData? processData;
            if (key.ProcessDataRequestType == ProcessDataRequestType.Path)
            {
                processData = response.Items.Find(processData => processData.Path == key.Key);
                dataResultByKey.Add(key, new DataResult(processData, exception: null));
                continue;
            }

            processData = response.Items.Find(processData => processData.VariableIdentifier == key.Key);
            dataResultByKey.Add(key, new DataResult(processData, exception: null));
        }

        return dataResultByKey;
    }
}