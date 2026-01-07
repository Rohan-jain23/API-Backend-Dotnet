using System;
using System.Collections.Generic;
using WuH.Ruby.Common.Core;

namespace FrameworkAPI.Models.DataLoader;

/// <summary>
/// This object is used as input by BatchDataLoaders and represents a single snapshot value request
/// </summary>
public class SnapshotValueWithLimitRequestKey(
    string machineId,
    string columnId,
    List<TimeRange> timeRanges,
    int limit) : SnapshotValueRequestKey(machineId, columnId, timeRanges), IEquatable<SnapshotValueWithLimitRequestKey>
{
    public int Limit { get; } = limit;

    public bool Equals(SnapshotValueWithLimitRequestKey? other)
    {
        if (ReferenceEquals(null, other))
            return false;
        if (ReferenceEquals(this, other))
            return true;

        return base.Equals(other) && Limit == other.Limit;
    }
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
            return false;
        if (ReferenceEquals(this, obj))
            return true;
        if (obj.GetType() != GetType())
            return false;

        return Equals((SnapshotValueWithLimitRequestKey)obj);
    }
    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), Limit);
    }
}