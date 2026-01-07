using System.Threading;
using System.Threading.Tasks;
using WuH.Ruby.Common.Core;
using WuH.Ruby.FrameworkAPI.Client.GraphQL;

namespace WuH.Ruby.FrameworkAPI.Client;

public class FrameworkAPIClientForMutations(IFrameworkAPIGraphQLClient graphQLClient) : IFrameworkAPIClientForMutations
{
    public async Task<InternalResponse> ExecuteProducedJobChangeMachineTargetDownTimeInMin(
        string associatedJob,
        string machineId,
        double targetDownTimeInMin,
        CancellationToken cancellationToken)
    {
        var operationResult = await graphQLClient.GenerateProducedJobChangeMachineTargetDownTimeInMin.ExecuteAsync(associatedJob, machineId, targetDownTimeInMin, cancellationToken);
        return operationResult.ToInternalResponse();
    }

    public async Task<InternalResponse> ExecuteProducedJobChangeMachineTargetScrapCountDuringProduction(
        string associatedJob,
        string machineId,
        double targetScrapCountDuringProduction,
        CancellationToken cancellationToken)
    {
        var operationResult = await graphQLClient.GenerateProducedJobChangeMachineTargetScrapCountDuringProduction.ExecuteAsync(associatedJob, machineId, targetScrapCountDuringProduction, cancellationToken);
        return operationResult.ToInternalResponse();
    }

    public async Task<InternalResponse> ExecuteProducedJobChangeMachineTargetSetupTimeInMin(
        string associatedJob,
        string machineId,
        double targetSetupTimeInMin,
        CancellationToken cancellationToken)
    {
        var operationResult = await graphQLClient.GenerateProducedJobChangeMachineTargetSetupTimeInMin.ExecuteAsync(associatedJob, machineId, targetSetupTimeInMin, cancellationToken);
        return operationResult.ToInternalResponse();
    }

    public async Task<InternalResponse> ExecuteProducedJobChangeMachineTargetSpeed(
        string associatedJob,
        string machineId,
        double targetSpeed,
        CancellationToken cancellationToken)
    {
        var operationResult = await graphQLClient.GenerateProducedJobChangeMachineTargetSpeed.ExecuteAsync(associatedJob, machineId, targetSpeed, cancellationToken);
        return operationResult.ToInternalResponse();
    }

    public async Task<InternalResponse> ExecuteProductGroupChangeMachineNote(
        string machineId,
        string productGroupId,
        string note,
        CancellationToken cancellationToken)
    {
        var operationResult = await graphQLClient.GenerateProductGroupChangeMachineNote.ExecuteAsync(machineId, productGroupId, note, cancellationToken);
        return operationResult.ToInternalResponse();
    }

    public async Task<InternalResponse> ExecuteProductGroupChangeMachineTargetSpeedMutation(
        string machineId,
        string productGroupId,
        double targetSpeed,
        CancellationToken cancellationToken)
    {
        var operationResult = await graphQLClient.GenerateProductGroupChangeMachineTargetSpeedMutation.ExecuteAsync(machineId, productGroupId, targetSpeed, cancellationToken);
        return operationResult.ToInternalResponse();
    }

    public async Task<InternalResponse> ExecuteProductGroupChangeOverallNote(
        string productGroupId,
        string note,
        CancellationToken cancellationToken)
    {
        var operationResult = await graphQLClient.GenerateProductGroupChangeOverallNote.ExecuteAsync(productGroupId, note, cancellationToken);
        return operationResult.ToInternalResponse();
    }
}