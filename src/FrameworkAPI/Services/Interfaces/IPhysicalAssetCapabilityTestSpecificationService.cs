using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.Schema.PhysicalAsset.CapabilityTest;
using PhysicalAssetDataHandler.Client.Models.Enums;

namespace FrameworkAPI.Services.Interfaces;

public interface IPhysicalAssetCapabilityTestSpecificationService
{
    Task<IEnumerable<CapabilityTestSpecification>> GetCurrentCapabilityTestSpecifications(
        CancellationToken cancellationToken = default);

    Task<CapabilityTestSpecification> GetCurrentCapabilityTestSpecification(
        CapabilityTestType capabilityTestType,
        CancellationToken cancellationToken = default);
}