using System.Collections.Generic;
using WuH.Ruby.Common.Core;
using WuH.Ruby.MachineSnapShooter.Client.Models;

namespace FrameworkAPI.Models.DataLoader;

/// <summary>
/// This object is used as input by BatchDataLoaders and represents a single grouped sum value request
/// </summary>
public class GroupedSumRequestKey(string machineId, GroupAssignment groupAssignment, List<TimeRange> timeRanges)
{
    public string MachineId { get; } = machineId;
    public GroupAssignment GroupAssignment { get; } = groupAssignment;
    public List<TimeRange> TimeRanges { get; } = timeRanges;
}