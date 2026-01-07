using System.Threading;
using System.Threading.Tasks;
using WuH.Ruby.Common.Core;
using WuH.Ruby.FrameworkAPI.Client.GraphQL;

namespace WuH.Ruby.FrameworkAPI.Client;

public class FrameworkAPIClientForMachineTimeSpan(IFrameworkAPIGraphQLClient graphQLClient) : IFrameworkAPIClientForMachineTimeSpan
{
    public async Task<InternalItemResponse<RawMaterialConsumptionByMaterial>> GetExtrusionRawMaterialConsumptionByMaterial(
        string machineId,
        TimeRange timeRange,
        CancellationToken cancellationToken)
    {
        var operationResult = await graphQLClient.GenerateRawMaterialConsumptionByTimeSpan.ExecuteAsync(
            timeRange.From,
            timeRange.To,
            machineId,
            cancellationToken);
        var response = operationResult.ToInternalItemResponse();

        if (response.HasError)
        {
            return new InternalItemResponse<RawMaterialConsumptionByMaterial>(response.Error);
        }

        var machineTimeSpan = response.Item.MachineTimeSpan;

        if (machineTimeSpan is not IGenerateRawMaterialConsumptionByTimeSpan_MachineTimeSpan_ExtrusionMachineTimeSpan
            extrusionMachineTimeSpan)
        {
            return new InternalItemResponse<RawMaterialConsumptionByMaterial>(
                statusCode: 500,
                errorMessage: "Time Span is not of extrusion machine");
        }

        var rawMaterialConsumptionByMaterial = extrusionMachineTimeSpan.RawMaterialConsumptionByMaterial;

        if (rawMaterialConsumptionByMaterial is null)
        {
            return new InternalItemResponse<RawMaterialConsumptionByMaterial>(
                statusCode: 500,
                errorMessage: "Unexpectedly received null for 'Data.MachineTimeSpan.RawMaterialConsumptionByMaterial' in from GraphQl in the operation result.");
        }

        var rawMaterialConsumptionByTimeSpan = new RawMaterialConsumptionByMaterial();

        foreach (var rawMaterialConsumption in rawMaterialConsumptionByMaterial)
        {
            var materialName = rawMaterialConsumption.Key;
            var consumption = rawMaterialConsumption.Value;

            rawMaterialConsumptionByTimeSpan.Add(materialName, (consumption.Value ?? 0, consumption.Unit ?? ""));
        }

        return new InternalItemResponse<RawMaterialConsumptionByMaterial>(rawMaterialConsumptionByTimeSpan);
    }
}