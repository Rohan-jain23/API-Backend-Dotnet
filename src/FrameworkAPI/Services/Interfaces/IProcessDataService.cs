using System;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.DataLoaders;
using FrameworkAPI.Models;
using WuH.Ruby.MetaDataHandler.Client;
using WuH.Ruby.ProcessDataReader.Client;

namespace FrameworkAPI.Services.Interfaces;

public interface IProcessDataService
{
    Task<DataResult<(ProcessData processData, ProcessVariableMetaDataResponseItem metaData)?>> GetProcessDataByVariableIdentifier(
        ProcessDataByTimestampBatchDataLoader processDataByTimestampBatchDataLoader,
        MachineMetaDataBatchDataLoader machineMetaDataBatchDataLoader,
        LatestProcessDataCacheDataLoader latestProcessDataCacheDataLoader,
        string machineId,
        string identifier,
        DateTime? timestamp,
        CancellationToken cancellationToken);

    Task<DataResult<(ProcessData processData, ProcessVariableMetaDataResponseItem metaData)?>> GetProcessDataByLastPartOfPath(
        ProcessDataByTimestampBatchDataLoader processDataByTimestampBatchDataLoader,
        MachineMetaDataBatchDataLoader machineMetaDataBatchDataLoader,
        LatestProcessDataCacheDataLoader latestProcessDataCacheDataLoader,
        string machineId,
        string identifier,
        DateTime? timestamp,
        CancellationToken cancellationToken);
}