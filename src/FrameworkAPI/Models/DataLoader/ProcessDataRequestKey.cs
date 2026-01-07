using System;

namespace FrameworkAPI.Models.DataLoader;

public class ProcessDataRequestKey(string machineId, string key, ProcessDataRequestType processDataRequestType, DateTime? timestamp = null)
    : IEquatable<ProcessDataRequestKey>
{
    public string MachineId { get; } = machineId;
    public string Key { get; } = key;
    public ProcessDataRequestType ProcessDataRequestType { get; } = processDataRequestType;
    public DateTime? Timestamp { get; } = timestamp;

    public bool Equals(ProcessDataRequestKey? other)
    {
        if (ReferenceEquals(null, other))
            return false;
        if (ReferenceEquals(this, other))
            return true;

        return MachineId == other.MachineId
                && Key == other.Key
                && ProcessDataRequestType == other.ProcessDataRequestType
                && Timestamp == other.Timestamp;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
            return false;
        if (ReferenceEquals(this, obj))
            return true;
        if (obj.GetType() != GetType())
            return false;

        return Equals((ProcessDataRequestKey)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(MachineId, Key, ProcessDataRequestType, Timestamp);
    }
}