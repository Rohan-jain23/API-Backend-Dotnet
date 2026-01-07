using System;
using System.Threading.Tasks;
using FrameworkAPI.Exceptions;
using FrameworkAPI.Services.Interfaces;

namespace FrameworkAPI.Services;

public class LicenceGuard(ILicenceService licenceService) : ILicenceGuard
{
    public async Task CheckMachineLicence(string machineId, string requiredLicence)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(requiredLicence);
        ArgumentException.ThrowIfNullOrWhiteSpace(machineId);

        var hasValidLicence = await licenceService.HasValidLicence(machineId, requiredLicence);

        if (!hasValidLicence)
        {
            throw new InvalidLicenceException(requiredLicence, machineId);
        }
    }
}