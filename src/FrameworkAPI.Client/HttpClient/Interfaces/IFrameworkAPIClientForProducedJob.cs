using System.Threading;
using System.Threading.Tasks;
using WuH.Ruby.Common.Core;
using WuH.Ruby.FrameworkAPI.Client.GraphQL;

namespace WuH.Ruby.FrameworkAPI.Client;

public interface IFrameworkAPIClientForProducedJob
{
    Task<InternalItemResponse<RawMaterialConsumptionByMaterial>> GetExtrusionRawMaterialConsumptionByMaterial(
        string machineId,
        string jobId,
        CancellationToken cancellationToken);

    Task<InternalListResponse<IGenerateTrackProductionHistoryByJob_ProducedJob_TrackProductionHistory>> GetTrackProductionHistory(
        string machineId,
        string jobId,
        CancellationToken cancellationToken);
}