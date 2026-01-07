using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.Exceptions;
using FrameworkAPI.Schema.Misc;
using FrameworkAPI.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using WuH.Ruby.LicenceManager.Client;
using WuH.Ruby.MachineDataHandler.Client;

namespace FrameworkAPI.Services;

public class LicenceService(
    IMachineCachingService machineCachingService,
    ILicenceManagerCachingService licenceManagerCachingService) : ILicenceService
{
    public async Task<bool> HasValidLicence(string requiredLicence)
    {
        if (requiredLicence == Constants.LicensesApplications.Anilox)
        {
            // If any machine has an anilox licence it's valid for the complete ruby instance
            // This is only for now and will be changed after the MVP
            return await HasValidAniloxLicence();
        }

        var getDetailedRubyInstanceLicenceValidityResponse = await licenceManagerCachingService.GetDetailedRubyInstanceLicenceValidity(
            application: requiredLicence, CancellationToken.None);
        if (getDetailedRubyInstanceLicenceValidityResponse.HasError && getDetailedRubyInstanceLicenceValidityResponse.Error.StatusCode != StatusCodes.Status204NoContent)
        {
            throw new InternalServiceException(getDetailedRubyInstanceLicenceValidityResponse.Error);
        }

        return getDetailedRubyInstanceLicenceValidityResponse.Item?.IsValid == true;
    }

    public async Task<bool> HasValidLicence(string machineId, string requiredLicence)
    {
        var licenceValidityResponse = await licenceManagerCachingService.GetDetailedLicenceValidity(
            machineId, application: requiredLicence, CancellationToken.None);

        if (licenceValidityResponse.HasError && licenceValidityResponse.Error.StatusCode != StatusCodes.Status204NoContent)
        {
            throw new InternalServiceException(licenceValidityResponse.Error);
        }

        return licenceValidityResponse.Item?.IsValid == true;
    }

    public async Task<RubyLicenses?> GetMachineLicenses(string machineId, CancellationToken cancellationToken)
    {
        var getAllDetailedLicenceValidityResponse = await licenceManagerCachingService.GetAllDetailedLicenceValidity(machineId, cancellationToken);

        if (getAllDetailedLicenceValidityResponse.HasError && getAllDetailedLicenceValidityResponse.Error.StatusCode != StatusCodes.Status204NoContent)
        {
            throw new InternalServiceException(getAllDetailedLicenceValidityResponse.Error);
        }

        return new RubyLicenses(getAllDetailedLicenceValidityResponse.Item ?? []);
    }

    private async Task<bool> HasValidAniloxLicence()
    {
        var machines = await machineCachingService.GetMachines();
        if (machines is null)
        {
            return false;
        }

        // EQ10101 is the only machine which could be added at the customer (production) as well which would result in an always valid anilox licence
        // because the simulation machines are generated (in the licence service) with always valid licences
        var machineIds = machines
            .Select(m => m.MachineId)
            .Where(machineId => machineId != "EQ10101" && !string.IsNullOrWhiteSpace(machineId));

        foreach (var machineId in machineIds)
        {
            var licenceValidityResponse = await licenceManagerCachingService.GetDetailedLicenceValidity(
                machineId, application: Constants.LicensesApplications.Anilox, CancellationToken.None);

            if (licenceValidityResponse.HasError && licenceValidityResponse.Error.StatusCode != StatusCodes.Status204NoContent)
            {
                throw new InternalServiceException(licenceValidityResponse.Error);
            }

            if (licenceValidityResponse.Item?.IsValid == true)
            {
                return true;
            }
        }

        return false;
    }
}