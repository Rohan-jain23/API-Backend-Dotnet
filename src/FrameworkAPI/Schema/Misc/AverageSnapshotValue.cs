using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.DataLoaders;
using FrameworkAPI.Helpers;
using FrameworkAPI.Services.Interfaces;
using HotChocolate;

namespace FrameworkAPI.Schema.Misc;

public class AverageSnapshotValue(string columnId, string machineId, DateTime startTime, DateTime? endTime)
{
    private readonly string _columnId = columnId;
    private readonly string _machineId = machineId;
    private readonly DateTime _startTime = startTime;
    private readonly DateTime? _endTime = endTime;

    /// <summary>
    /// The average (arithmetic mean) value during the job/roll/time-span in SI unit.
    /// Because of performance reasons, it is not possible to subscribe to this property (-> returns 'null').
    /// </summary>
    public async Task<double?> Value(
        SnapshotArithmeticMeanBatchDataLoader dataLoader,
        [Service] IMachineSnapshotService machineSnapshotService,
        [Service] IMachineTimeService machineTimeService,
        CancellationToken cancellationToken)
    {
        var validEndTime = await DateTimeParameterHelper.GetValidTimeForRequest(
            machineTimeService,
            _endTime,
            _machineId,
            cancellationToken);

        var timeRanges = new List<WuH.Ruby.Common.Core.TimeRange>
        {
            new(_startTime, validEndTime)
        };

        var result = await machineSnapshotService.GetArithmeticMean(
            dataLoader,
            _machineId,
            _columnId,
            timeRanges,
            cancellationToken);

        if (result.Exception is not null)
        {
            throw result.Exception;
        }

        return result.Value;
    }

    /// <summary>
    /// The unit of the value.
    /// </summary>
    public async Task<string?> Unit(
        LatestSnapshotCacheDataLoader latestSnapshotCacheDataLoader,
        [Service] IMachineSnapshotService service,
        CancellationToken cancellationToken)
    {
        var unitResult = await service.GetLatestColumnUnit(latestSnapshotCacheDataLoader, _columnId, _machineId, cancellationToken);

        if (unitResult.Exception is not null)
        {
            throw unitResult.Exception;
        }

        return unitResult.Value;
    }
}