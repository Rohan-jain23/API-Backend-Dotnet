using System.Threading.Tasks;

namespace FrameworkAPI.Services.Interfaces;

public interface ILicenceGuard
{
    Task CheckMachineLicence(string machineId, string requiredLicence);
}