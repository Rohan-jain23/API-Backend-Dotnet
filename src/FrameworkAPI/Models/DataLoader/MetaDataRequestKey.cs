using System;

namespace FrameworkAPI.Models.DataLoader;

public class MetaDataRequestKey(string machineId, string key, MetaDataRequestType metaDataRequestType)
    : IEquatable<MetaDataRequestKey>
{
    public string MachineId { get; } = machineId;
    public string Key { get; } = key;
    public MetaDataRequestType MetaDataRequestType { get; } = metaDataRequestType;

    public bool Equals(MetaDataRequestKey? other)
    {
        if (ReferenceEquals(null, other))
            return false;
        if (ReferenceEquals(this, other))
            return true;

        return MachineId == other.MachineId && Key == other.Key && MetaDataRequestType == other.MetaDataRequestType;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
            return false;
        if (ReferenceEquals(this, obj))
            return true;
        if (obj.GetType() != GetType())
            return false;

        return Equals((MetaDataRequestKey)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(MachineId, Key, MetaDataRequestType);
    }
}