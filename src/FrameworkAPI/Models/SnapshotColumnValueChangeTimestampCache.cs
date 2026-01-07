using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace FrameworkAPI.Models;

public class SnapshotColumnValueChangeTimestampCache
{
    private record TimestampAndValue(DateTime Timestamp, object? Value);
    private record LiveAndLastFullMinute(TimestampAndValue Live, TimestampAndValue LastFullMinute, DateTime LastMinutelySnapshotTime);

    public string MachineId { get; }

    private readonly ConcurrentDictionary<string, LiveAndLastFullMinute?> _cache = new();

    public SnapshotColumnValueChangeTimestampCache(string machineId)
    {
        MachineId = machineId;
    }

    public ICollection<string> ColumnIds { get => _cache.Keys; }

    public bool TryGetValue(string columnId, [NotNullWhen(returnValue: true)] out DateTime? lastChangeTimestamp)
    {
        if (!_cache.TryGetValue(columnId, out var liveAndLastFullMinute))
        {
            lastChangeTimestamp = null;
            return false;
        }

        if (liveAndLastFullMinute is null)
        {
            lastChangeTimestamp = null;
            return false;
        }

        lastChangeTimestamp = liveAndLastFullMinute.Live.Timestamp;
        return true;
    }

    public DateTime InitializeValueForColumnId(string columnId, object? value, DateTime changedTimestamp, DateTime latestMinutelySnapshotTime)
    {
        var updatedColumnIdValue = new TimestampAndValue(changedTimestamp, value);
        var updatedLiveAndLastFullMinute = new LiveAndLastFullMinute(updatedColumnIdValue, updatedColumnIdValue, latestMinutelySnapshotTime);

        _cache.AddOrUpdate(
            columnId,
            _ => updatedLiveAndLastFullMinute,
            (_, _) => updatedLiveAndLastFullMinute);

        return updatedLiveAndLastFullMinute.Live.Timestamp;
    }

    public void UpdateLiveValueForColumnId(string columnId, object? value, DateTime snapshotTime, bool isMinutelySnapshot)
    {
        var updatedColumnIdValue = new TimestampAndValue(snapshotTime, value);
        _cache.AddOrUpdate(
            columnId,
            _ => null,
            (_, cachedValue) =>
            {
                if (cachedValue is null) return null;

                (var cachedLive, var cachedLastFullMinute, var cachedLastSnapshotTime) = cachedValue;

                if (cachedLastSnapshotTime > snapshotTime) return cachedValue;
                // Reset cache when there was a gap. Check for 2 minutes because live messages might come shortly before the next full minute message
                if (snapshotTime - cachedLastSnapshotTime >= TimeSpan.FromMinutes(2)) return null;

                if (isMinutelySnapshot)
                {
                    return !Equals(value, cachedLastFullMinute.Value)
                        ? new LiveAndLastFullMinute(updatedColumnIdValue, updatedColumnIdValue, snapshotTime)
                        : new LiveAndLastFullMinute(cachedLastFullMinute, cachedLastFullMinute, snapshotTime);
                }

                // If the live value changes within a minute back to the value we had at the last full minute change, 
                // we ignore the short spike and change back the timestamp of the last full minute change.
                // We do this because snapshot data is stored minutely and we want the cache to be consistent.
                if (Equals(value, cachedLive.Value)) return cachedValue;

                var newLiveChanged = Equals(value, cachedLastFullMinute.Value)
                    ? cachedLastFullMinute
                    : updatedColumnIdValue;

                return new LiveAndLastFullMinute(newLiveChanged, cachedLastFullMinute, cachedLastSnapshotTime);

            });
    }

    public void Clear() => _cache.Clear();
}