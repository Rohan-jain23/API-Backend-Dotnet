using System;

namespace FrameworkAPI.Schema.Misc;

/// <summary>
/// A period of time.
/// </summary>
public class TimeRange : IEquatable<TimeRange>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TimeRange"/> class.
    /// </summary>
    /// <param name="timeRange">Common.Core model.</param>
    public TimeRange(WuH.Ruby.Common.Core.TimeRange timeRange)
    {
        if (timeRange is null)
        {
            throw new ArgumentNullException(nameof(timeRange));
        }

        From = timeRange.From;
        To = timeRange.To;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TimeRange"/> class.
    /// </summary>
    public TimeRange(DateTime from, DateTime to)
    {
        From = from;
        To = to;
    }

    /// <summary>
    /// Start timestamp of the time range in UTC.
    /// </summary>
    public DateTime From { get; }

    /// <summary>
    /// End timestamp of the time range in UTC.
    /// </summary>
    public DateTime To { get; }

    public override int GetHashCode()
    {
        var hashCode = HashCode.Combine(From, To);
        return hashCode;
    }

    public override bool Equals(object? obj)
    {
        return obj is TimeRange range && Equals(range);
    }

    public bool Equals(TimeRange? other)
    {
        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (other is null)
        {
            return false;
        }

        return From == other.From && To == other.To;
    }

    public static implicit operator WuH.Ruby.Common.Core.TimeRange(TimeRange d) => new(d.From, d.To);
    public static implicit operator TimeRange(WuH.Ruby.Common.Core.TimeRange b) => new(b.From, b.To);
}