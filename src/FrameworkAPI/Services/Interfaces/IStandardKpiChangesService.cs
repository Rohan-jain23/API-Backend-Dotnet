using System;

namespace FrameworkAPI.Services;

public interface IStandardKpiChangesService
{
    IObservable<string> WhenMachineStandardKpisChanged(string machineId);
}