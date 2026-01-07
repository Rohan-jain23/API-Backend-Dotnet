using System;
using HotChocolate;

namespace FrameworkAPI.Schema.PhysicalAsset;

/// <summary>
/// Generic classes need to be described via a "GraphQLDescription" attribute,
/// because the summary will not appear in the GraphQL documentation on the normal way.
/// </summary>
/// <typeparam name="T">Data type of the variable.</typeparam>
[GraphQLDescription("A generic value that was tracked at some point in time.")]
public class TrackedValue<T>(T value, DateTime trackedAt) where T : IConvertible
{

    /// <summary>
    /// See GraphQL description.
    /// </summary>
    [GraphQLDescription("The generic tracked value.")]
    public T Value { get; set; } = value;

    /// <summary>
    /// See GraphQL description.
    /// </summary>
    [GraphQLDescription("The date and time when the value was tracked.")]
    public DateTime TrackedAt { get; set; } = trackedAt;
}