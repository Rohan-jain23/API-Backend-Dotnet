using System.Linq;

namespace FrameworkAPI.Models.DataLoader;

/// <summary>
/// This object is used by BatchDataLoaders for grouping single SnapshotValue requests (keys)
/// </summary>
public class SnapshotValueWithLimitRequestBatch(SnapshotValueWithLimitRequestKey key) : SnapshotValueRequestBatch(key)
{
    public int Limit { get; private set; } = key.Limit;

    public bool CanKeyBeGroupedToBatch(SnapshotValueWithLimitRequestKey key)
    {
        return CanKeyBeGroupedToBatch((SnapshotValueRequestKey)key) && Limit.Equals(key.Limit);
    }

    public bool IsKeyPartOfBatch(SnapshotValueWithLimitRequestKey key)
    {
        return ColumnIds.Contains(key.ColumnId)
               && MachineId.Equals(key.MachineId)
               && TimeRanges.SequenceEqual(key.TimeRanges)
               && Limit.Equals(key.Limit);
    }
}