using System;
using FrameworkAPI.Schema.Misc;

namespace FrameworkAPI.Schema.MachineTimeSpan;

/// <summary>
/// Machine time span entity of machines that are other than extrusion, printing or paper sack.
/// </summary>
public class OtherMachineTimeSpan(string machineId, MachineDepartment machineDepartment, DateTime from, DateTime to)
    : MachineTimeSpan(machineId, machineDepartment, from, to)
{
}