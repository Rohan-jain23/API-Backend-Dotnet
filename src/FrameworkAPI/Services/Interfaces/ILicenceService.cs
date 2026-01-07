using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.Schema.Misc;

namespace FrameworkAPI.Services.Interfaces;

public interface ILicenceService
{
    Task<bool> HasValidLicence(string requiredLicence);

    Task<bool> HasValidLicence(string machineId, string requiredLicence);

    Task<RubyLicenses?> GetMachineLicenses(string machineId, CancellationToken cancellationToken);
}