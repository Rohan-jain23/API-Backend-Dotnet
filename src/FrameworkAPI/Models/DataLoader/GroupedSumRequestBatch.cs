using System.Collections.Generic;
using System.Linq;
using WuH.Ruby.Common.Core;
using WuH.Ruby.MachineSnapShooter.Client.Models;

namespace FrameworkAPI.Models.DataLoader;

/// <summary>
/// This object is used by BatchDataLoaders for grouping single GroupedSum requests (keys)
/// </summary>
public class GroupedSumRequestBatch(GroupedSumRequestKey groupedSumRequestKey)
{
    public string MachineId { get; } = groupedSumRequestKey.MachineId;
    public List<GroupAssignment> GroupAssignments { get; } = [groupedSumRequestKey.GroupAssignment];
    public List<TimeRange> TimeRanges { get; } = groupedSumRequestKey.TimeRanges;

    public bool CanKeyBeGroupedToBatch(GroupedSumRequestKey groupedSumRequestKey)
    {
        return MachineId == groupedSumRequestKey.MachineId && TimeRanges.SequenceEqual(groupedSumRequestKey.TimeRanges);
    }

    public bool IsKeyPartOfBatch(GroupedSumRequestKey groupedSumRequestKey)
    {
        return GroupAssignments.Contains(groupedSumRequestKey.GroupAssignment)
               && MachineId.Equals(groupedSumRequestKey.MachineId)
               && TimeRanges.SequenceEqual(groupedSumRequestKey.TimeRanges);
    }
}