using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.DataLoaders;
using FrameworkAPI.Services.Interfaces;
using HotChocolate;

namespace FrameworkAPI.Schema.Misc;

public class SummedSnapshotValue(string columnId, string machineId, IEnumerable<TimeRange>? timeRanges)
{
    private readonly string _columnId = columnId;
    private readonly string _machineId = machineId;
    private readonly IEnumerable<TimeRange>? _timeRanges = timeRanges;

    /// <summary>
    /// The summed up value within this time span.
    /// </summary>
    public async Task<double?> Value(
        SnapshotSumBatchDataLoader dataLoader,
        [Service] IMachineSnapshotService service,
        CancellationToken cancellationToken)
    {
        if (_timeRanges == null)
        {
            return null;
        }

        var result = await service.GetSum(
            dataLoader,
            _machineId,
            _columnId,
            _timeRanges.Select(timeRange => (WuH.Ruby.Common.Core.TimeRange)timeRange).ToList(),
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