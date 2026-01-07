using FrameworkAPI.Schema.Misc;
using MachineDataHandler = WuH.Ruby.MachineDataHandler.Client;

namespace FrameworkAPI.Helpers;

internal static class MachineDepartmentMapper
{
    internal static MachineDepartment MapToSchemaMachineDepartment(this MachineDataHandler.BusinessUnit businessUnit)
    {
        return businessUnit switch
        {
            MachineDataHandler.BusinessUnit.Extrusion => MachineDepartment.Extrusion,
            MachineDataHandler.BusinessUnit.Printing => MachineDepartment.Printing,
            MachineDataHandler.BusinessUnit.PaperSack => MachineDepartment.PaperSack,
            MachineDataHandler.BusinessUnit.Other => MachineDepartment.Other,
            _ => MachineDepartment.Other
        };
    }

    internal static MachineDataHandler.BusinessUnit MapToInternalBusinessUnit(this MachineDepartment department)
    {
        return department switch
        {
            MachineDepartment.Extrusion => MachineDataHandler.BusinessUnit.Extrusion,
            MachineDepartment.Printing => MachineDataHandler.BusinessUnit.Printing,
            MachineDepartment.PaperSack => MachineDataHandler.BusinessUnit.PaperSack,
            MachineDepartment.Other => MachineDataHandler.BusinessUnit.Other,
            _ => MachineDataHandler.BusinessUnit.Other
        };
    }
}