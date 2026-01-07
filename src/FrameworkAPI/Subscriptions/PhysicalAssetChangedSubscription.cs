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
public class PhysicalAssetChangedSubscription
{
    /// <summary>
    /// Subscribe to the current status of the physical asset.
    /// </summary>
    [Subscribe(With = nameof(WhenPhysicalAssetChanged))]
    [Authorize(Roles = ["go-general"])]
    [LicenceCheck(Licence = Constants.LicensesApplications.Anilox)]
    public async Task<PhysicalAsset> PhysicalAssetChanged(
        [EventMessage] string physicalAssetId,
        [Service] IPhysicalAssetService physicalAssetService,
        CancellationToken cancellationToken)
    {
        return await physicalAssetService.GetPhysicalAsset(physicalAssetId, cancellationToken);
    }

    /// <summary>
    /// Logic needed for the subscription of created and updated physical assets.
    /// </summary>
    public IObservable<string> WhenPhysicalAssetChanged(
        PhysicalAssetType? physicalAssetTypeFilter,
        [Service] IPhysicalAssetQueueWrapper physicalAssetQueueWrapper)
    {
        return Observable.Merge(
            WhenPhysicalAssetCreated(physicalAssetTypeFilter, physicalAssetQueueWrapper),
            WhenPhysicalAssetUpdated(physicalAssetTypeFilter, physicalAssetQueueWrapper));
    }

    private static IObservable<string> WhenPhysicalAssetCreated(
        PhysicalAssetType? physicalAssetTypeFilter, IPhysicalAssetQueueWrapper physicalAssetQueueWrapper)
    {
        return Observable.Create<string>(observer =>
        {
            return physicalAssetQueueWrapper.SubscribeToPhysicalAssetCreatedEvent(
                physicalAssetCreatedEvent =>
                {
                    if (physicalAssetTypeFilter is null ||
                        physicalAssetCreatedEvent.PhysicalAssetType == physicalAssetTypeFilter)
                    {
                        observer.OnNext(physicalAssetCreatedEvent.PhysicalAssetId);
                    }

                    return Task.CompletedTask;
                });
        });
    }

    private static IObservable<string> WhenPhysicalAssetUpdated(
        PhysicalAssetType? physicalAssetTypeFilter, IPhysicalAssetQueueWrapper physicalAssetQueueWrapper)
    {
        return Observable.Create<string>(observer =>
        {
            return physicalAssetQueueWrapper.SubscribeToPhysicalAssetUpdatedEvent(physicalAssetUpdatedEvent =>
            {
                if (physicalAssetTypeFilter is null ||
                    physicalAssetUpdatedEvent.PhysicalAssetType == physicalAssetTypeFilter)
                {
                    observer.OnNext(physicalAssetUpdatedEvent.PhysicalAssetId);
                }

                return Task.CompletedTask;
            });
        });
    }
}