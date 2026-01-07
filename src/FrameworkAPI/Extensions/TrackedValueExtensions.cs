using System;
using PhysicalAssetDataHandler.Client.Models;

namespace FrameworkAPI.Extensions;

public static class TrackedValueExtensions
{
    public static Schema.PhysicalAsset.TrackedValue<T> ToSchema<T>(
        this TrackedValue<T> trackedValue) where T : IConvertible
    {
        return new Schema.PhysicalAsset.TrackedValue<T>(
            trackedValue.Value,
            trackedValue.TrackedAt);
    }
}