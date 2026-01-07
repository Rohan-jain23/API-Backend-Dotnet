using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using FrameworkAPI.Extensions;
using WuH.Ruby.Common.Core;
using WuH.Ruby.MachineSnapShooter.Client.Models;

namespace FrameworkAPI.Models;

public class MachineTrendCache(string machineId)
{
    private readonly ConcurrentDictionary<DateTime, ImmutableDictionary<string, double?>> _cache = new();
    public string MachineId => machineId;

    public bool IsEmpty => _cache.IsEmpty;

    public DateTime? LatestDateTime => _cache.IsEmpty
        ? null
        : _cache
            .Keys
            .Max();

    public IReadOnlyDictionary<DateTime, IReadOnlyDictionary<string, double?>?> Get(TimeRange timeRange)
    {
        var machineTrend = new SortedDictionary<DateTime, IReadOnlyDictionary<string, double?>?>();
        for (var date = timeRange.From; date <= timeRange.To; date = date.AddMinutes(1))
        {
            var value = _cache.GetValueOrDefault(date);
            machineTrend.Add(date, value);
        }
        return machineTrend;
    }

    public void UpdateCacheValues(IEnumerable<SnapshotDto> snapshots)
    {
        foreach (var snapshot in snapshots)
        {
            var trendElement = snapshot.GetMachineTrendElement();
            _cache.AddOrUpdate(
                snapshot.SnapshotTime,
                _ => trendElement.ToImmutableDictionary(),
                (_, currentValues) => currentValues.SetItems(trendElement));
        }
    }

    public void DeleteOldSnapshotsFromCache(TimeRange validTimeRange)
    {
        var oldKeys = _cache
            .Keys
            .Where(it => it < validTimeRange.From || it > validTimeRange.To);

        foreach (var oldKey in oldKeys)
        {
            _cache.TryRemove(oldKey, out _);
        }
    }
}