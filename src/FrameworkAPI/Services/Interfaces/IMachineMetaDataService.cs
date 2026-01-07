using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.DataLoaders;
using WuH.Ruby.MetaDataHandler.Client;

namespace FrameworkAPI.Services.Interfaces;

public interface IMachineMetaDataService
{
    Task<ProcessVariableMetaData?> GetMachineMetadata(
        MachineMetaDataBatchDataLoader machineMetaDataBatchDataLoader,
        string machineId,
        string variableIdentifier,
        CancellationToken cancellationToken = default);
}