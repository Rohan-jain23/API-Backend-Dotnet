using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.Exceptions;
using FrameworkAPI.Schema.PhysicalAsset.CapabilityTest;
using FrameworkAPI.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using PhysicalAssetDataHandler.Client.HttpClients;
using PhysicalAssetDataHandler.Client.Models.Enums;

namespace FrameworkAPI.Services;

public class PhysicalAssetCapabilityTestSpecificationService(
    ICapabilityTestSpecificationHttpClient capabilityTestSpecificationHttpClient) : IPhysicalAssetCapabilityTestSpecificationService
{
    private readonly ICapabilityTestSpecificationHttpClient _capabilityTestSpecificationHttpClient = capabilityTestSpecificationHttpClient;

    public async Task<IEnumerable<CapabilityTestSpecification>> GetCurrentCapabilityTestSpecifications(
        CancellationToken cancellationToken = default)
    {
        var getCurrentVersionsResponse =
            await _capabilityTestSpecificationHttpClient.GetCurrentVersions(cancellationToken);

        if (getCurrentVersionsResponse.HasError &&
            getCurrentVersionsResponse.Error.StatusCode != StatusCodes.Status204NoContent)
        {
            throw new InternalServiceException(getCurrentVersionsResponse.Error);
        }

        return getCurrentVersionsResponse.Items.Select(CapabilityTestSpecification.CreateInstance);
    }

    public async Task<CapabilityTestSpecification> GetCurrentCapabilityTestSpecification(
        CapabilityTestType capabilityTestType,
        CancellationToken cancellationToken = default)
    {
        var getCurrentVersionResponse = await _capabilityTestSpecificationHttpClient.GetCurrentVersion(
            capabilityTestType, cancellationToken);

        if (getCurrentVersionResponse.HasError)
        {
            throw new InternalServiceException(getCurrentVersionResponse.Error);
        }

        return CapabilityTestSpecification.CreateInstance(getCurrentVersionResponse.Item);
    }
}