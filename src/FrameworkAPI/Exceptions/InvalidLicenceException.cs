using System;

namespace FrameworkAPI.Exceptions;

public class InvalidLicenceException(string licence, string? machineId = null) : Exception
{
    public readonly string Licence = licence;
    public readonly string? MachineId = machineId;
}