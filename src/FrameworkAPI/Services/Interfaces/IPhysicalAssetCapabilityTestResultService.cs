using System.Threading.Tasks;
using FrameworkAPI.Schema.PhysicalAsset.CapabilityTest;

namespace FrameworkAPI.Services.Interfaces;

public interface IPhysicalAssetCapabilityTestResultService
{
    Task<AniloxCapabilityTestResult> CreateAniloxCapabilityTestResult(
        CreateAniloxCapabilityTestResultRequest createVolumeCapabilityTestResultRequest,
        string userId);

    Task<VolumeCapabilityTestResult> CreateVolumeCapabilityTestResult(
        CreateVolumeCapabilityTestResultRequest createVolumeCapabilityTestResultRequest,
        string userId);
}