using System.Collections.Generic;
using System.Linq;
using WuH.Ruby.Common.Core;

namespace FrameworkAPI.Models.DataLoader;

/// <summary>
/// This object is used by BatchDataLoaders for grouping single SnapshotValue requests (keys)
/// </summary>
public class SnapshotValueRequestBatch
{
    public string MachineId { get; }
    public List<string> ColumnIds { get; }
    public List<TimeRange> TimeRanges { get; }

    public SnapshotValueRequestBatch(SnapshotValueRequestKey key)
    {
        MachineId = key.MachineId;
        ColumnIds = new List<string> { key.ColumnId };
        TimeRanges = key.TimeRanges;
    }

    public bool CanKeyBeGroupedToBatch(SnapshotValueRequestKey key)
    {
        return MachineId == key.MachineId && TimeRanges.SequenceEqual(key.TimeRanges);
    }

    public bool IsKeyPartOfBatch(SnapshotValueRequestKey key)
    {
        return ColumnIds.Contains(key.ColumnId)
               && MachineId.Equals(key.MachineId)
               && TimeRanges.SequenceEqual(key.TimeRanges);
    }
}