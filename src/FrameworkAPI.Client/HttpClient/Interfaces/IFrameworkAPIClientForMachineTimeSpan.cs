using System.Threading;
using System.Threading.Tasks;
using WuH.Ruby.Common.Core;

namespace WuH.Ruby.FrameworkAPI.Client;

public interface IFrameworkAPIClientForMachineTimeSpan
{
    Task<InternalItemResponse<RawMaterialConsumptionByMaterial>> GetExtrusionRawMaterialConsumptionByMaterial(
        string machineId,
        TimeRange timeRange,
        CancellationToken cancellationToken);
}