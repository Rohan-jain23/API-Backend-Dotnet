using System.Threading;
using System.Threading.Tasks;
using WuH.Ruby.Common.Core;

namespace WuH.Ruby.FrameworkAPI.Client;

public interface IFrameworkAPIClientForMutations
{
    Task<InternalResponse> ExecuteProductGroupChangeMachineTargetSpeedMutation(
        string machineId,
        string productGroupId,
        double targetSpeed,
        CancellationToken cancellationToken);
}