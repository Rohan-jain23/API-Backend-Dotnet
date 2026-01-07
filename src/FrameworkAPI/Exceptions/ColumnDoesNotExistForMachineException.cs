using System;

namespace FrameworkAPI.Exceptions;

public class ColumnDoesNotExistForMachineException : Exception
{
    public ColumnDoesNotExistForMachineException()
    {
    }

    public ColumnDoesNotExistForMachineException(string columnId, string machineId)
        : base($"Column with id '{columnId}' does not exist for machine '{machineId}'.")
    {
    }
}