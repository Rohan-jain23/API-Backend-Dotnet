using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.DataLoaders;
using FrameworkAPI.Schema.Misc;
using TimeRange = WuH.Ruby.Common.Core.TimeRange;

namespace FrameworkAPI.Services.Interfaces;

public interface IMaterialConsumptionService
{
    Task<Dictionary<string, NumericValue>?> GetRawMaterialConsumptionByMaterial(
        SnapshotGroupedSumBatchDataLoader dataLoader,
        string machineId,
        IEnumerable<TimeRange> timeRanges,
        CancellationToken cancellationToken);
}