using System;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.DataLoaders;
using FrameworkAPI.Models.DataLoader;
using FrameworkAPI.Services.Interfaces;
using DataResult = FrameworkAPI.Models.DataResult<
    (WuH.Ruby.ProcessDataReader.Client.ProcessData processData,
    WuH.Ruby.MetaDataHandler.Client.ProcessVariableMetaDataResponseItem metaData)?>;

namespace FrameworkAPI.Services;

public class ProcessDataService : IProcessDataService
{
    public async Task<DataResult> GetProcessDataByVariableIdentifier(
        ProcessDataByTimestampBatchDataLoader processDataByTimestampBatchDataLoader,
        MachineMetaDataBatchDataLoader machineMetaDataBatchDataLoader,
        LatestProcessDataCacheDataLoader latestProcessDataCacheDataLoader,
        string machineId,
        string identifier,
        DateTime? timestamp,
        CancellationToken cancellationToken)
    {
        return await GetProcessData(
            processDataByTimestampBatchDataLoader,
            machineMetaDataBatchDataLoader,
            latestProcessDataCacheDataLoader,
            machineId,
            identifier,
            MetaDataRequestType.VariableIdentifier,
            timestamp,
            cancellationToken);
    }

    public async Task<DataResult> GetProcessDataByLastPartOfPath(
        ProcessDataByTimestampBatchDataLoader processDataByTimestampBatchDataLoader,
        MachineMetaDataBatchDataLoader machineMetaDataBatchDataLoader,
        LatestProcessDataCacheDataLoader latestProcessDataCacheDataLoader,
        string machineId,
        string lastPartOfPath,
        DateTime? timestamp,
        CancellationToken cancellationToken)
    {
        return await GetProcessData(
            processDataByTimestampBatchDataLoader,
            machineMetaDataBatchDataLoader,
            latestProcessDataCacheDataLoader,
            machineId,
            lastPartOfPath,
            MetaDataRequestType.LastPartOfPath,
            timestamp,
            cancellationToken);
    }

    private static async Task<DataResult> GetProcessData(
        ProcessDataByTimestampBatchDataLoader processDataByTimestampBatchDataLoader,
        MachineMetaDataBatchDataLoader machineMetaDataBatchDataLoader,
        LatestProcessDataCacheDataLoader latestProcessDataCacheDataLoader,
        string machineId,
        string identifier,
        MetaDataRequestType requestType,
        DateTime? timestamp,
        CancellationToken cancellationToken
    )
    {
        var metaData = await machineMetaDataBatchDataLoader.LoadAsync(new MetaDataRequestKey(machineId, identifier, requestType), cancellationToken);
        if (metaData.Exception is not null)
        {
            return new DataResult(value: null, metaData.Exception);
        }

        var requestKey = (machineId, metaData.Value!.Path, timestamp);
        if (timestamp is null)
        {
            var processCacheResult = await latestProcessDataCacheDataLoader.LoadAsync((requestKey.machineId, requestKey.Path), cancellationToken);
            if (processCacheResult.Exception is not null)
            {
                return new DataResult(value: null, processCacheResult.Exception);
            }

            return new DataResult((processCacheResult.Value!, metaData.Value), exception: null);
        }

        var dataResult = await processDataByTimestampBatchDataLoader.LoadAsync(new ProcessDataRequestKey(
            requestKey.machineId,
            requestKey.Path,
            ProcessDataRequestType.Path,
            timestamp), cancellationToken);

        if (dataResult.Exception is not null)
        {
            return new DataResult(value: null, dataResult.Exception);
        }

        return new DataResult((dataResult.Value!, metaData.Value), exception: null);
    }
}