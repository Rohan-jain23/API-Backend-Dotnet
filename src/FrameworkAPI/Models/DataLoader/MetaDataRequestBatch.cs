using System.Collections.Generic;

namespace FrameworkAPI.Models.DataLoader;

public class MetaDataRequestBatch(MetaDataRequestKey key)
{
    public string MachineId { get; set; } = key.MachineId;

    public List<MetaDataRequestKey> Keys { get; set; } = [key];

    public bool CanKeyBeGroupedToBatch(MetaDataRequestKey key)
    {
        return MachineId == key.MachineId;
    }

    public bool IsKeyPartOfBatch(MetaDataRequestKey key)
    {
        return Keys.Contains(key)
               && MachineId.Equals(key.MachineId);
    }
}