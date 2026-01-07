using System;
using System.Threading;
using System.Threading.Tasks;

namespace FrameworkAPI.Schema.Misc;

/// <summary>
/// A numeric value that is localized to SI unit.
/// </summary>
public class NumericValue
{
    private readonly Func<CancellationToken, Task<double?>> _valueFunc;
    private readonly Func<CancellationToken, Task<string?>> _unitFunc;

    /// <summary>
    /// Initializes a new instance of the <see cref="NumericValue"/> class.
    /// </summary>
    public NumericValue(
        Func<CancellationToken, Task<double?>> valueFunc, Func<CancellationToken, Task<string?>> unitFunc)
    {
        _valueFunc = valueFunc;
        _unitFunc = unitFunc;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="NumericValue" /> class.
    /// </summary>
    public NumericValue(
        double? value, string? unit)
    {
        _valueFunc = _ => Task.FromResult(value);
        _unitFunc = _ => Task.FromResult(unit);
    }

    /// <summary>
    /// The numeric value in SI unit.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The numeric value if it exists, or <c>null</c> otherwise.</returns>
    public async Task<double?> Value(CancellationToken cancellationToken)
    {
        var value = await _valueFunc(cancellationToken);
        return value;
    }

    /// <summary>
    /// The unit of the numeric value.
    /// (if the unit needs to be translated, the corresponding i18n tag is provided here; for example 'label.items').
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The unit if it exists, or <c>null</c> otherwise.</returns>
    public async Task<string?> Unit(CancellationToken cancellationToken)
    {
        var unit = await _unitFunc(cancellationToken);
        return unit;
    }
}