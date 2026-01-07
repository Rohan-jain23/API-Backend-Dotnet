using System.Collections.Generic;

namespace WuH.Ruby.FrameworkAPI.Client;

public class RawMaterialConsumptionByMaterial : Dictionary<string, (double Consumption, string Unit)>
{
    public IEnumerable<string> Materials
    {
        get { return Keys; }
    }
    public IEnumerable<(double Consumption, string Unit)> RawMaterialConsumptions
    {
        get { return Values; }
    }

    public new IEnumerator<(string Material, (double Consumption, string Unit) RawMaterialConsumption)> GetEnumerator()
    {
        foreach (var key in Keys)
        {
            yield return (key, this[key]);
        }
    }
}