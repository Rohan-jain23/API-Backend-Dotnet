using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.Exceptions;
using FrameworkAPI.Schema.ProductGroup;
using FrameworkAPI.Services.Interfaces;
using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Types;

namespace FrameworkAPI.Mutations;

[ExtendObjectType("Mutation")]
public class ProductGroupsMutation
{
    [Authorize(Roles = ["go-general"])]
    [Error(typeof(ParameterInvalidException))]
    [Error(typeof(InternalServiceException))]
    [UseMutationConvention(PayloadFieldName = "changedProductGroup")]
    public async Task<PaperSackProductGroup> ProductGroupChangeOverallNote(
        [GlobalState] string userId,
        [Service] IProductGroupService productGroupService,
        ProductGroupChangeOverallNoteRequest productGroupChangeOverallNoteRequest)
    {
        var updatedPaperSackProductGroup = await productGroupService.UpdatePaperSackProductGroupNote(
            productGroupChangeOverallNoteRequest.PaperSackProductGroupId,
            productGroupChangeOverallNoteRequest.Note,
            userId,
            CancellationToken.None);
        return updatedPaperSackProductGroup;
    }

    [Authorize(Roles = ["go-general"])]
    [Error(typeof(ParameterInvalidException))]
    [Error(typeof(InternalServiceException))]
    [UseMutationConvention(PayloadFieldName = "changedProductGroup")]
    public async Task<PaperSackProductGroup> ProductGroupChangeMachineNote(
        [GlobalState] string userId,
        [Service] IProductGroupService productGroupService,
        ProductGroupChangeMachineNoteRequest productGroupChangeMachineNoteRequest)
    {
        var updatedPaperSackProductGroup = await productGroupService.UpdatePaperSackProductGroupMachineNote(
                productGroupChangeMachineNoteRequest.PaperSackProductGroupId,
                productGroupChangeMachineNoteRequest.MachineId,
                productGroupChangeMachineNoteRequest.Note,
                userId,
                CancellationToken.None);
        return updatedPaperSackProductGroup;
    }

    [Authorize(Roles = ["go-general"])]
    [Error(typeof(ParameterInvalidException))]
    [Error(typeof(InternalServiceException))]
    [UseMutationConvention(PayloadFieldName = "changedProductGroup")]
    public async Task<PaperSackProductGroup> ProductGroupChangeMachineTargetSpeed(
        [GlobalState] string userId,
        [Service] IProductGroupService productGroupService,
        ProductGroupChangeMachineTargetSpeedRequest productGroupChangeMachineTargetSpeedRequest)
    {
        var updatedPaperSackProductGroup = await productGroupService.UpdatePaperSackProductGroupMachineTargetSpeed(
            productGroupChangeMachineTargetSpeedRequest.PaperSackProductGroupId,
            productGroupChangeMachineTargetSpeedRequest.MachineId,
            productGroupChangeMachineTargetSpeedRequest.TargetSpeed,
            userId,
            CancellationToken.None);
        return updatedPaperSackProductGroup;
    }
}