using System;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.DataLoaders;
using FrameworkAPI.Schema.Machine.ActualProcessValues;
using FrameworkAPI.Schema.Misc;

namespace FrameworkAPI.Services.Interfaces;

public interface IExtrusionProfileService
{
    Task<ExtrusionThicknessProfile?> GetMostRelevantProfile(
        ProcessDataByTimestampBatchDataLoader processDataByTimestampBatchDataLoader,
        MachineMetaDataBatchDataLoader machineMetaDataBatchDataLoader,
        LatestProcessDataCacheDataLoader latestProcessDataCacheDataLoader,
        string machineId,
        MachineFamily machineFamily,
        DateTime? timestamp,
        CancellationToken cancellationToken);

    Task<ExtrusionThicknessProfile?> GetProfile(
        ProcessDataByTimestampBatchDataLoader processDataByTimestampBatchDataLoader,
        MachineMetaDataBatchDataLoader machineMetaDataBatchDataLoader,
        LatestProcessDataCacheDataLoader latestProcessDataCacheDataLoader,
        ExtrusionThicknessMeasurementType profileType,
        string machineId,
        MachineFamily machineFamily,
        DateTime? timestamp,
        CancellationToken cancellationToken);
}