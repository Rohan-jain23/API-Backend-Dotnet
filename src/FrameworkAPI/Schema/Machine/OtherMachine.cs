using MachineDataHandler = WuH.Ruby.MachineDataHandler.Client;

namespace FrameworkAPI.Schema.Machine;

/// <summary>
/// Machine entity of machines that are other than extrusion, printing or paper sack.
/// </summary>
public class OtherMachine(MachineDataHandler.Machine internalMachine) : Machine(internalMachine)
{
}