using System;
using HotChocolate;

namespace FrameworkAPI.Schema.PhysicalAsset;

/// <summary>
/// Generic classes need to be described via a "GraphQLDescription" attribute,
/// because the summary will not appear in the GraphQL documentation on the normal way.
/// </summary>
/// <typeparam name="T">Data type of the variable.</typeparam>
[GraphQLDescription("A generic value that can be measured and can be tested against a set value.")]
public class TestableValueWithUnit<T>(T? setValue, T? measuredValue, DateTime? measuredAt, string? unit)
    where T : struct, IComparable, IComparable<T>, IConvertible, IEquatable<T>, IFormattable
{

    /// <summary>
    /// See GraphQL description.
    /// </summary>
    [GraphQLDescription("The optimal value the measured value can be tested against.")]
    public T? SetValue { get; set; } = setValue;

    /// <summary>
    /// See GraphQL description.
    /// </summary>
    [GraphQLDescription("The date and time when the value was measured.")]
    public DateTime? MeasuredAt { get; set; } = measuredAt;

    /// <summary>
    /// See GraphQL description.
    /// </summary>
    [GraphQLDescription("The generic measured value.")]
    public T? MeasuredValue { get; set; } = measuredValue;

    /// <summary>
    /// See GraphQL description.
    /// </summary>
    [GraphQLDescription("The unit of the value.")]
    public string? Unit { get; set; } = unit;
}