using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using WuH.Ruby.MachineSnapShooter.Client.Models;

namespace FrameworkAPI.Extensions;

public static class SnapshotDtoExtensions
{
    public static IReadOnlyDictionary<string, double?> GetMachineTrendElement(this SnapshotDto snapshotDto)
    {
        return Constants.MachineTrend.TrendingSnapshotColumnIds.ToImmutableDictionary<string, string, double?>(
            columnId => columnId,
            columnId =>
            {
                var columnValue = snapshotDto.ColumnValues.Find(c => c.Id == columnId)?.Value;
                return columnValue is not null ? Convert.ToDouble(columnValue) : null;
            });
    }
}