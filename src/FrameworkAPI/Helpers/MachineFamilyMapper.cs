using MachineDataHandler = WuH.Ruby.MachineDataHandler.Client;
using MachineFamily = FrameworkAPI.Schema.Misc.MachineFamily;

namespace FrameworkAPI.Helpers;

internal static class MachineFamilyMapper
{
    internal static MachineFamily MapToSchemaMachineFamily(this MachineDataHandler.MachineFamily machineFamily)
    {
        return machineFamily switch
        {
            MachineDataHandler.MachineFamily.FlexoPrint => MachineFamily.FlexoPrint,
            MachineDataHandler.MachineFamily.GravurePrint => MachineFamily.GravurePrint,
            MachineDataHandler.MachineFamily.BlowFilm => MachineFamily.BlowFilm,
            MachineDataHandler.MachineFamily.CastFilm => MachineFamily.CastFilm,
            MachineDataHandler.MachineFamily.PaperSackBottomer => MachineFamily.PaperSackBottomer,
            MachineDataHandler.MachineFamily.PaperSackTuber => MachineFamily.PaperSackTuber,
            _ => MachineFamily.Other
        };
    }

    internal static MachineDataHandler.MachineFamily? MapToInternalMachineFamily(this MachineFamily machineFamily)
    {
        return machineFamily switch
        {
            MachineFamily.FlexoPrint => MachineDataHandler.MachineFamily.FlexoPrint,
            MachineFamily.GravurePrint => MachineDataHandler.MachineFamily.GravurePrint,
            MachineFamily.BlowFilm => MachineDataHandler.MachineFamily.BlowFilm,
            MachineFamily.CastFilm => MachineDataHandler.MachineFamily.CastFilm,
            MachineFamily.PaperSackBottomer => MachineDataHandler.MachineFamily.PaperSackBottomer,
            MachineFamily.PaperSackTuber => MachineDataHandler.MachineFamily.PaperSackTuber,
            MachineFamily.Other => null,
            _ => null
        };
    }
}