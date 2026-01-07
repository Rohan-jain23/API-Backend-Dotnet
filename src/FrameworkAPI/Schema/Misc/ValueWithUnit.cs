using System;
using System.Threading;
using System.Threading.Tasks;
using HotChocolate;

namespace FrameworkAPI.Schema.Misc;

/// <summary>
/// Generic classes need to be described via a "GraphQLDescription" attribute,
/// because the summary will not appear in the GraphQL documentation on the normal way.
/// </summary>
/// <typeparam name="T">Data type of the variable.</typeparam>
[GraphQLDescription("A generic value and the related unit.")]
public class ValueWithUnit<T>(
    Func<CancellationToken, Task<T?>> valueFunc, Func<CancellationToken, Task<string?>> unitFunc)
    where T : struct, IComparable, IComparable<T>, IConvertible, IEquatable<T>, IFormattable
{
    private readonly Func<CancellationToken, Task<T?>> _valueFunc = valueFunc;
    private readonly Func<CancellationToken, Task<string?>> _unitFunc = unitFunc;

    /// <summary>
    /// See GraphQL description.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The generic value if it exists, or <c>null</c> otherwise.</returns>
    [GraphQLDescription("The generic value in SI unit.")]
    public async Task<T?> Value(CancellationToken cancellationToken)
    {
        var value = await _valueFunc(cancellationToken);
        return value;
    }

    /// <summary>
    /// See GraphQL description.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The unit if it exists, or <c>null</c> otherwise.</returns>
    [GraphQLDescription("The unit of the generic value.")]
    public async Task<string?> Unit(CancellationToken cancellationToken)
    {
        var unit = await _unitFunc(cancellationToken);
        return unit;
    }
}