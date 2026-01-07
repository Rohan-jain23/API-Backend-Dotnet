using FrameworkAPI.Services.Interfaces;
using WuH.Ruby.MetaDataHandler.Client;

namespace FrameworkAPI.Services;

public class UnitService : IUnitService
{
    public double CalculateSiValue(double value, ProcessVariableMetaData? machineMetadata)
    {
        if (machineMetadata?.Units is null)
        {
            return value;
        }

        var units = machineMetadata.Units;
        var multiplier = units.Si.Multiplier;
        var offset = units.Si.Offset;

        return value * multiplier + offset;
    }

    public string? GetSiUnit(ProcessVariableMetaData? machineMetadata)
    {
        if (machineMetadata?.Units?.Si.Unit is null)
        {
            return null;
        }

        var siUnit = machineMetadata.Units.Si.Unit;

        if (Constants.Units.SpecialUnitsTranslation.TryGetValue(siUnit, out var translatedSiUnit))
        {
            return translatedSiUnit;
        }

        return siUnit;
    }
}