using System;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.DataLoaders;
using FrameworkAPI.Schema.Misc;
using FrameworkAPI.Services.Interfaces;
using HotChocolate;

namespace FrameworkAPI.Schema.Machine.ActualProcessValues;

/// <summary>
/// All profiles of the different thickness measurement systems.
/// These profiles show the current deviation of produced thickness as a profile over the produced width.
/// </summary>
public class ExtrusionThicknessProfiles(DateTime? queryTimestamp, string machineId, MachineFamily machineFamily)
{
    /// <summary>
    /// The profile of the most relevant thickness measurement.
    /// Logic:
    /// - Is 'Primary' if profile control mode is not 'MDO'.
    /// - Is 'MdoWinderA' if profile control mode is 'MDO' and winder A has contact pressure.
    /// - Is 'MdoWinderB' if profile control mode is 'MDO' and winder A has no contact pressure.
    /// </summary>
    public async Task<ExtrusionThicknessProfile?> MostRelevantProfile(
        ProcessDataByTimestampBatchDataLoader processDataByTimestampBatchDataLoader,
        MachineMetaDataBatchDataLoader machineMetaDataBatchDataLoader,
        LatestProcessDataCacheDataLoader latestProcessDataCacheDataLoader,
        [Service] IExtrusionProfileService extrusionProfileService,
        CancellationToken cancellationToken)
        => await extrusionProfileService.GetMostRelevantProfile(
           processDataByTimestampBatchDataLoader,
           machineMetaDataBatchDataLoader,
           latestProcessDataCacheDataLoader,
           machineId,
           machineFamily,
           queryTimestamp,
           cancellationToken);

    /// <summary>
    /// The profile of the primary thickness measurement.
    /// </summary>
    public async Task<ExtrusionThicknessProfile?> PrimaryProfile(
        ProcessDataByTimestampBatchDataLoader processDataByTimestampBatchDataLoader,
        MachineMetaDataBatchDataLoader machineMetaDataBatchDataLoader,
        LatestProcessDataCacheDataLoader latestProcessDataCacheDataLoader,
        [Service] IExtrusionProfileService extrusionProfileService,
        CancellationToken cancellationToken)
        => await extrusionProfileService.GetProfile(
            processDataByTimestampBatchDataLoader,
            machineMetaDataBatchDataLoader,
            latestProcessDataCacheDataLoader,
            ExtrusionThicknessMeasurementType.Primary,
            machineId,
            machineFamily,
            queryTimestamp,
            cancellationToken);

    /// <summary>
    /// The profile of the thickness measurement after the MDO before winding station A.
    /// </summary>
    public async Task<ExtrusionThicknessProfile?> MdoProfileA(
        ProcessDataByTimestampBatchDataLoader processDataByTimestampBatchDataLoader,
        MachineMetaDataBatchDataLoader machineMetaDataBatchDataLoader,
        LatestProcessDataCacheDataLoader latestProcessDataCacheDataLoader,
        [Service] IExtrusionProfileService extrusionProfileService,
        CancellationToken cancellationToken)
        => await extrusionProfileService.GetProfile(
            processDataByTimestampBatchDataLoader,
            machineMetaDataBatchDataLoader,
            latestProcessDataCacheDataLoader,
            ExtrusionThicknessMeasurementType.MdoWinderA,
            machineId,
            machineFamily,
            queryTimestamp,
            cancellationToken);

    /// <summary>
    /// The profile of the thickness measurement after the MDO before winding station B.
    /// </summary>
    public async Task<ExtrusionThicknessProfile?> MdoProfileB(
        ProcessDataByTimestampBatchDataLoader processDataByTimestampBatchDataLoader,
        MachineMetaDataBatchDataLoader machineMetaDataBatchDataLoader,
        LatestProcessDataCacheDataLoader latestProcessDataCacheDataLoader,
        [Service] IExtrusionProfileService extrusionProfileService,
        CancellationToken cancellationToken)
        => await extrusionProfileService.GetProfile(
            processDataByTimestampBatchDataLoader,
            machineMetaDataBatchDataLoader,
            latestProcessDataCacheDataLoader,
            ExtrusionThicknessMeasurementType.MdoWinderB,
            machineId,
            machineFamily,
            queryTimestamp,
            cancellationToken);
}