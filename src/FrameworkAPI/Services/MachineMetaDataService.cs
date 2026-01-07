using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.DataLoaders;
using FrameworkAPI.Models.DataLoader;
using FrameworkAPI.Services.Interfaces;
using WuH.Ruby.MetaDataHandler.Client;

namespace FrameworkAPI.Services;

public class MachineMetaDataService : IMachineMetaDataService
{
    public async Task<ProcessVariableMetaData?> GetMachineMetadata(
        MachineMetaDataBatchDataLoader machineMetaDataBatchDataLoader,
        string machineId,
        string variableIdentifier,
        CancellationToken cancellationToken = default)
    {
        var (machineMetadata, exception) =
            await machineMetaDataBatchDataLoader.LoadAsync(
                new MetaDataRequestKey(
                    machineId,
                    variableIdentifier,
                    MetaDataRequestType.VariableIdentifier),
                cancellationToken);

        if (exception is not null) throw exception;

        return machineMetadata!.Data;
    }
}