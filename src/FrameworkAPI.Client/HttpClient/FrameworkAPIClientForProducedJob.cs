using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WuH.Ruby.Common.Core;
using WuH.Ruby.FrameworkAPI.Client.GraphQL;

namespace WuH.Ruby.FrameworkAPI.Client;

public class FrameworkAPIClientForProducedJob(IFrameworkAPIGraphQLClient graphQLClient) : IFrameworkAPIClientForProducedJob
{
    public async Task<InternalItemResponse<RawMaterialConsumptionByMaterial>> GetExtrusionRawMaterialConsumptionByMaterial(
        string machineId,
        string jobId,
        CancellationToken cancellationToken)
    {
        var operationResult = await graphQLClient.GenerateRawMaterialConsumptionByJob.ExecuteAsync(machineId, jobId, cancellationToken);
        var response = operationResult.ToInternalItemResponse();

        if (response.HasError)
        {
            return new InternalItemResponse<RawMaterialConsumptionByMaterial>(response.Error);
        }

        var producedJob = response.Item.ProducedJob;

        if (producedJob is not IGenerateRawMaterialConsumptionByJob_ProducedJob_ExtrusionProducedJob extrusionProducedJob)
        {
            return new InternalItemResponse<RawMaterialConsumptionByMaterial>(
                statusCode: 500,
                errorMessage: "Produced job is not of extrusion machine");
        }

        var rawMaterialConsumptionByMaterial = extrusionProducedJob.RawMaterialConsumptionByMaterial;

        if (rawMaterialConsumptionByMaterial is null)
        {
            return new InternalItemResponse<RawMaterialConsumptionByMaterial>(
                statusCode: 500,
                errorMessage: "Unexpectedly received null for 'Data.ProducedJob.RawMaterialConsumptionByMaterial' from GraphQl in the operation result.");
        }

        var rawMaterialConsumptionByJob = new RawMaterialConsumptionByMaterial();

        foreach (var rawMaterialConsumption in rawMaterialConsumptionByMaterial)
        {
            var materialName = rawMaterialConsumption.Key;
            var consumption = rawMaterialConsumption.Value;

            rawMaterialConsumptionByJob.Add(materialName, (consumption.Value ?? 0, consumption.Unit ?? ""));
        }

        return new InternalItemResponse<RawMaterialConsumptionByMaterial>(rawMaterialConsumptionByJob);
    }

    public async Task<InternalListResponse<IGenerateTrackProductionHistoryByJob_ProducedJob_TrackProductionHistory>> GetTrackProductionHistory(
        string machineId,
        string jobId,
        CancellationToken cancellationToken)
    {
        var operationResult = await graphQLClient.GenerateTrackProductionHistoryByJob.ExecuteAsync(machineId, jobId, cancellationToken);
        var response = operationResult.ToInternalItemResponse();
        if (response.HasError)
        {
            return new InternalListResponse<IGenerateTrackProductionHistoryByJob_ProducedJob_TrackProductionHistory>(response.Error);
        }

        var producedJob = response.Item.ProducedJob;
        if (producedJob is not IGenerateTrackProductionHistoryByJob_ProducedJob_PaperSackProducedJob paperSackProducedJob)
        {
            return new InternalListResponse<IGenerateTrackProductionHistoryByJob_ProducedJob_TrackProductionHistory>(
                statusCode: 500,
                errorMessage: "Produced job is not of papersack machine");
        }

        var trackProductionHistory = paperSackProducedJob?.TrackProductionHistory;
        if (trackProductionHistory is null)
        {
            return new InternalListResponse<IGenerateTrackProductionHistoryByJob_ProducedJob_TrackProductionHistory>(
                statusCode: 500,
                errorMessage: "Unexpectedly received null for 'Data.ProducedJob.TrackProductionHistory' from GraphQl in the operation result.");
        }

        return new InternalListResponse<IGenerateTrackProductionHistoryByJob_ProducedJob_TrackProductionHistory>(trackProductionHistory.ToList());
    }
}