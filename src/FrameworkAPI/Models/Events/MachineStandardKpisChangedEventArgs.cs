using System;
using WuH.Ruby.KpiDataHandler.Client;

namespace FrameworkAPI.Models.Events;

public class MachineStandardKpisChangedEventArgs(string machineId, StandardKpis standardKpis) : EventArgs
{
    public string MachineId { get; } = machineId;

    public StandardKpis StandardKpis { get; } = standardKpis;
}