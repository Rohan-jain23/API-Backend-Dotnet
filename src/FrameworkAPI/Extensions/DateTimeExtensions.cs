using System;
using System.Collections.Generic;
using System.Linq;
using WuH.Ruby.Common.Core;

namespace FrameworkAPI.Extensions;

public static class DateTimeExtensions
{
    public static DateTime RoundUp(this DateTime dateTime, TimeSpan interval)
        => new((dateTime.Ticks + interval.Ticks - 1) / interval.Ticks * interval.Ticks, dateTime.Kind);

    public static DateTime RoundDown(this DateTime dateTime, TimeSpan interval)
        => new(dateTime.Ticks - dateTime.Ticks % interval.Ticks, dateTime.Kind);

    public static bool Overlaps(this TimeRange timeRange, TimeRange other, bool countTouchingAsOverlap = false)
        => countTouchingAsOverlap
            ? timeRange.From <= other.To && other.From <= timeRange.To
            : timeRange.From < other.To && other.From < timeRange.To;

    public static IEnumerable<TimeRange> Flatten(this IEnumerable<TimeRange> timeRanges)
    {
        var resultingTimeRanges = new List<TimeRange>();

        foreach (var timeRange in timeRanges)
        {
            var overlappingResult =
                resultingTimeRanges.SingleOrDefault(other => timeRange.Overlaps(other, countTouchingAsOverlap: true));

            if (overlappingResult is null)
            {
                resultingTimeRanges.Add(new TimeRange(timeRange.From, timeRange.To));
                continue;
            }

            resultingTimeRanges.Remove(overlappingResult);
            resultingTimeRanges.Add(new TimeRange(
                new List<DateTime> { overlappingResult.From, timeRange.From }.Min(),
                new List<DateTime> { overlappingResult.To, timeRange.To }.Max()));
        }

        return resultingTimeRanges;
    }

    public static IEnumerable<DateTime> Every(this TimeRange timeRange, TimeSpan step)
    {
        for (var time = timeRange.From; time <= timeRange.To; time = time.Add(step))
        {
            yield return time;
        }
    }
}