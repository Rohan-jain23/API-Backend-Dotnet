using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.Attributes;
using FrameworkAPI.Schema.PhysicalAsset;
using FrameworkAPI.Services.Interfaces;
using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Types;
using PhysicalAssetDataHandler.Client.Models.Enums;
using PhysicalAssetDataHandler.Client.QueueWrappers;

namespace FrameworkAPI.Subscriptions;

/// <summary>
/// GraphQL subscription for physical asset entity.
/// </summary>
[ExtendObjectType("Subscription")]
public class PhysicalAssetScrappedSubscription
{
    /// <summary>
    /// Subscribe to the current status of the physical asset.
    /// </summary>
    [Subscribe(With = nameof(WhenPhysicalAssetScrapped))]
    [Authorize(Roles = ["go-general"])]
    [LicenceCheck(Licence = Constants.LicensesApplications.Anilox)]
    public async Task<PhysicalAsset> PhysicalAssetScrapped(
        [EventMessage] string physicalAssetId,
        [Service] IPhysicalAssetService physicalAssetService,
        CancellationToken cancellationToken)
    {
        return await physicalAssetService.GetPhysicalAsset(physicalAssetId, cancellationToken);
    }

    /// <summary>
    /// Logic needed for the subscription of created and updated physical assets.
    /// </summary>
    public IObservable<string> WhenPhysicalAssetScrapped(
        PhysicalAssetType? physicalAssetTypeFilter,
        [Service] IPhysicalAssetQueueWrapper physicalAssetQueueWrapper)
    {
        return Observable.Create<string>(observer =>
        {
            return physicalAssetQueueWrapper.SubscribeToPhysicalAssetScrappedEvent(physicalAssetScrappedEvent =>
            {
                if (physicalAssetTypeFilter is null ||
                    physicalAssetScrappedEvent.PhysicalAssetType == physicalAssetTypeFilter)
                {
                    observer.OnNext(physicalAssetScrappedEvent.PhysicalAssetId);
                }

                return Task.CompletedTask;
            });
        });
    }
}