using System;

namespace FrameworkAPI.Schema.Misc;

/// <summary>
/// A numeric trend element.
/// </summary>
/// <param name="time">The time.</param>
/// <param name="value">The numeric value.</param>
public class NumericTrendElement(DateTime time, double? value)
{

    /// <summary>
    /// The time.
    /// </summary>
    public DateTime Time { get; set; } = time;

    /// <summary>
    /// The numeric value.
    /// </summary>
    public double? Value { get; set; } = value;
}