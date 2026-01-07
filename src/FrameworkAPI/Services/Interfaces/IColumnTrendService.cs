using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.DataLoaders;
using FrameworkAPI.Models;
using FrameworkAPI.Schema.Misc;

namespace FrameworkAPI.Services.Interfaces;

public interface IColumnTrendService
{
    Task<DataResult<IEnumerable<NumericTrendElement>>> GetLatest(
        LatestMachineTrendCacheDataLoader latestMachineTrendCacheDataLoader,
        string columnId,
        string machineId,
        CancellationToken cancellationToken);

    Task<DataResult<IEnumerable<NumericTrendElement>>> Get(
        MachineTrendByTimeRangeBatchDataLoader machineTrendByTimeRangeBatchDataLoader,
        string columnId,
        DateTime endTime,
        string machineId,
        CancellationToken cancellationToken);
}