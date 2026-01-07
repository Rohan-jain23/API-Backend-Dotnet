using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.Attributes;
using FrameworkAPI.Schema.PhysicalAsset;
using FrameworkAPI.Schema.PhysicalAsset.CapabilityTest;
using FrameworkAPI.Services.Interfaces;
using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Types;
using PhysicalAssetDataHandler.Client.Models.Enums;

namespace FrameworkAPI.Queries;

/// <summary>
/// GraphQL query class for physical asset entity.
/// </summary>
[ExtendObjectType("Query")]
public class PhysicalAssetQuery
{
    /// <summary>
    /// Query to get physical asset settings.
    /// </summary>
    /// <param name="physicalAssetService">The physical asset service.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="PhysicalAssetSettings"/>.</returns>
    [Authorize(Roles = ["go-general"])]
    public Task<PhysicalAssetSettings> GetPhysicalAssetSettings(
        [Service] IPhysicalAssetService physicalAssetService,
        CancellationToken cancellationToken)
    {
        return physicalAssetService.GetPhysicalAssetSettings(cancellationToken);
    }

    /// <summary>
    /// Query to get a list of all physical assets.
    /// </summary>
    /// <param name="physicalAssetService">The physical asset service.</param>
    /// <param name="physicalAssetsFilter">Only physical assets where which are matching the group of <paramref name="physicalAssetsFilter"/> are returned.</param>
    /// <param name="physicalAssetTypeFilter">If set, only physical assets where <see cref="PhysicalAsset.PhysicalAssetType"/> is equal to <paramref name="physicalAssetTypeFilter"/> are returned.</param>
    /// <param name="lastChangeFilter">If set, only physical assets where <see cref="PhysicalAsset.LastChange"/> is greater than <paramref name="lastChangeFilter"/> are returned.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><see cref="PhysicalAsset"/>s.</returns>
    [Authorize(Roles = ["go-general"])]
    [LicenceCheck(Licence = Constants.LicensesApplications.Anilox)]
    public Task<IEnumerable<PhysicalAsset>> GetPhysicalAssets(
        [Service] IPhysicalAssetService physicalAssetService,
        [DefaultValue(PhysicalAssetsFilter.Utilisable)] PhysicalAssetsFilter physicalAssetsFilter,
        PhysicalAssetType? physicalAssetTypeFilter,
        DateTime? lastChangeFilter,
        CancellationToken cancellationToken)
    {
        return physicalAssetService.GetAllPhysicalAssets(
            physicalAssetsFilter, physicalAssetTypeFilter, lastChangeFilter, cancellationToken);
    }

    /// <summary>
    /// Query to get one physical asset by id.
    /// </summary>
    /// <param name="physicalAssetService">The physical asset service.</param>
    /// <param name="physicalAssetId">The physical asset id.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="PhysicalAsset"/>.</returns>
    [Authorize(Roles = ["go-general"])]
    [LicenceCheck(Licence = Constants.LicensesApplications.Anilox)]
    public Task<PhysicalAsset> GetPhysicalAsset(
        [Service] IPhysicalAssetService physicalAssetService,
        string physicalAssetId,
        CancellationToken cancellationToken)
    {
        return physicalAssetService.GetPhysicalAsset(physicalAssetId, cancellationToken);
    }

    /// <summary>
    /// Query to get a list of all physical assets test specification.
    /// </summary>
    /// <param name="physicalAssetCapabilityTestSpecificationService">The physical asset capability estSpecification service.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="CapabilityTestSpecification"/>.</returns>
    [Authorize(Roles = ["go-general"])]
    [LicenceCheck(Licence = Constants.LicensesApplications.Anilox)]
    public Task<IEnumerable<CapabilityTestSpecification>> GetPhysicalAssetCapabilityTestSpecifications(
        [Service] IPhysicalAssetCapabilityTestSpecificationService physicalAssetCapabilityTestSpecificationService,
        CancellationToken cancellationToken)
    {
        return physicalAssetCapabilityTestSpecificationService.GetCurrentCapabilityTestSpecifications(cancellationToken);
    }

    /// <summary>
    /// Query to get one physical asset test specification by capability test type.
    /// </summary>
    /// <param name="physicalAssetCapabilityTestSpecificationService">The physical asset capability estSpecification service.</param>
    /// <param name="capabilityTestType">The capability test type.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="CapabilityTestSpecification"/>.</returns>
    [Authorize(Roles = ["go-general"])]
    [LicenceCheck(Licence = Constants.LicensesApplications.Anilox)]
    public Task<CapabilityTestSpecification> GetPhysicalAssetCapabilityTestSpecification(
        [Service] IPhysicalAssetCapabilityTestSpecificationService physicalAssetCapabilityTestSpecificationService,
        CapabilityTestType capabilityTestType,
        CancellationToken cancellationToken)
    {
        return physicalAssetCapabilityTestSpecificationService.GetCurrentCapabilityTestSpecification(
            capabilityTestType, cancellationToken);
    }
}