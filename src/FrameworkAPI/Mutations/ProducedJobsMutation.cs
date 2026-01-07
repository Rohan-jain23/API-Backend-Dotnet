using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.Exceptions;
using FrameworkAPI.Schema.ProducedJob;
using FrameworkAPI.Services.Interfaces;
using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Types;

namespace FrameworkAPI.Mutations;

[ExtendObjectType("Mutation")]
public class ProducedJobsMutation
{

    [Authorize(Roles = ["go-general"])]
    [Error(typeof(ParameterInvalidException))]
    [Error(typeof(InternalServiceException))]
    [UseMutationConvention(PayloadFieldName = "changedProducedJob")]
    public async Task<ProducedJob> ProducedJobChangeMachineTargetSpeed(
        [GlobalState] string userId,
        [Service] IProducedJobService producedJobService,
        ProducedJobUpdateTargetSpeedRequest targetSpeedRequest
        )
    {
        return await producedJobService.UpdateProducedJobMachineTargetSpeed(
            targetSpeedRequest.TargetSpeed,
            targetSpeedRequest.MachineId,
            targetSpeedRequest.AssociatedJob,
            userId,
            CancellationToken.None);
    }

    [Authorize(Roles = ["go-general"])]
    [Error(typeof(ParameterInvalidException))]
    [Error(typeof(InternalServiceException))]
    [UseMutationConvention(PayloadFieldName = "changedProducedJob")]
    public async Task<ProducedJob> ProducedJobChangeMachineTargetSetupTimeInMin(
        [GlobalState] string userId,
        [Service] IProducedJobService producedJobService,
        ProducedJobUpdateTargetSetupTimeInMinRequest setupTimeRequest
    )
    {
        return await producedJobService.UpdateProducedJobTargetSetupTimeInMin(
            setupTimeRequest.TargetSetupTimeInMin,
            setupTimeRequest.MachineId,
            setupTimeRequest.AssociatedJob,
            userId,
            CancellationToken.None);
    }

    [Authorize(Roles = ["go-general"])]
    [Error(typeof(ParameterInvalidException))]
    [Error(typeof(InternalServiceException))]
    [UseMutationConvention(PayloadFieldName = "changedProducedJob")]
    public async Task<ProducedJob> ProducedJobChangeMachineTargetDownTimeInMin(
        [GlobalState] string userId,
        [Service] IProducedJobService producedJobService,
        ProducedJobUpdateTargetDownTimeInMinRequest downTimeRequest
    )
    {
        return await producedJobService.UpdateProducedJobTargetDownTimeInMin(
            downTimeRequest.TargetDownTimeInMin,
            downTimeRequest.MachineId,
            downTimeRequest.AssociatedJob,
            userId,
            CancellationToken.None);
    }

    [Authorize(Roles = ["go-general"])]
    [Error(typeof(ParameterInvalidException))]
    [Error(typeof(InternalServiceException))]
    [UseMutationConvention(PayloadFieldName = "changedProducedJob")]
    public async Task<ProducedJob> ProducedJobChangeMachineTargetScrapCountDuringProduction(
        [GlobalState] string userId,
        [Service] IProducedJobService producedJobService,
        ProducedJobUpdateTargetScrapCountDuringProductionRequest scrapCountDuringProductionRequest
    )
    {
        return await producedJobService.UpdateProducedJobTargetScrapCountDuringProduction(
            scrapCountDuringProductionRequest.TargetScrapCountDuringProduction,
            scrapCountDuringProductionRequest.MachineId,
            scrapCountDuringProductionRequest.AssociatedJob,
            userId,
            CancellationToken.None);
    }
}