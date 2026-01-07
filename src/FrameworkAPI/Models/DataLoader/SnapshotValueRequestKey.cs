using System;
using System.Collections.Generic;
using System.Linq;
using WuH.Ruby.Common.Core;

namespace FrameworkAPI.Models.DataLoader;

/// <summary>
/// This object is used as input by BatchDataLoaders and represents a single snapshot value request
/// </summary>
public class SnapshotValueRequestKey(string machineId, string columnId, List<TimeRange> timeRanges)
    : IEquatable<SnapshotValueRequestKey>
{
    public string MachineId { get; } = machineId;
    public string ColumnId { get; } = columnId;
    public List<TimeRange> TimeRanges { get; } = timeRanges;

    public bool Equals(SnapshotValueRequestKey? other)
    {
        if (ReferenceEquals(null, other))
            return false;
        if (ReferenceEquals(this, other))
            return true;

        return MachineId == other.MachineId && ColumnId == other.ColumnId && TimeRanges.SequenceEqual(other.TimeRanges);
    }
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
            return false;
        if (ReferenceEquals(this, obj))
            return true;
        if (obj.GetType() != GetType())
            return false;

        return Equals((SnapshotValueRequestKey)obj);
    }
    public override int GetHashCode()
    {
        return HashCode.Combine(MachineId, ColumnId, TimeRanges);
    }
}