using System;

namespace FrameworkAPI.Models.Events;

public class MachineTimeChangedEventArgs(string machineId, DateTime? machineTime) : EventArgs
{
    public string MachineId { get; } = machineId;

    public DateTime? MachineTime { get; } = machineTime;
}