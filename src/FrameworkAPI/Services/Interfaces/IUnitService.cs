using WuH.Ruby.MetaDataHandler.Client;

namespace FrameworkAPI.Services.Interfaces;

public interface IUnitService
{
    double CalculateSiValue(double value, ProcessVariableMetaData? machineMetadata);

    string? GetSiUnit(ProcessVariableMetaData? machineMetadata);
}