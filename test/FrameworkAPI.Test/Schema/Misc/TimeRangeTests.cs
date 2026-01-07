using System;
using System.Collections.Generic;
using FluentAssertions;
using FrameworkAPI.Schema.Misc;
using Xunit;

namespace FrameworkAPI.Test.Schema.Misc;

public class TimeRangeTests
{
    [Theory]
    [MemberData(nameof(GetEqualTimeRanges))]
    public void TimeRanges_Two_Values_Are_Equal(List<TimeRange> timeRanges)
    {
        // Act
        var result = timeRanges[0].Equals(timeRanges[1]);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(GetUnequalTimeRanges))]
    public void TimeRanges_Two_Values_Are_Not_Equal(List<TimeRange> timeRanges)
    {
        // Act
        var result = timeRanges[0].Equals(timeRanges[1]);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void TimeRanges_Should_Not_Equal_Null_If_Not_Itself_Is_Null()
    {
        // Arrange
        var firstRange = new TimeRange(new WuH.Ruby.Common.Core.TimeRange(DateTime.MinValue, DateTime.MaxValue));

        // Act
        var result = firstRange.Equals(null);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void TimeRanges_Should_Check_If_Object_Is_TimeRange()
    {
        // Arrange
        var firstRange = new TimeRange(new WuH.Ruby.Common.Core.TimeRange(DateTime.MinValue, DateTime.MaxValue));
        object? obj = null;

        // Act
        var result = firstRange.Equals(obj);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void TimeRanges_Should_Check_If_Object_Is_TimeRange_And_Then_Equals()
    {
        // Arrange
        var firstRange = new TimeRange(new WuH.Ruby.Common.Core.TimeRange(DateTime.MinValue, DateTime.MaxValue));
        object obj = new TimeRange(new WuH.Ruby.Common.Core.TimeRange(DateTime.MinValue, DateTime.MaxValue));

        // Act
        var result = firstRange.Equals(obj);

        // Assert
        result.Should().BeTrue();
    }

    public static TheoryData<List<TimeRange>> GetEqualTimeRanges => new()
    {
        new List<TimeRange>
        {
            new(new WuH.Ruby.Common.Core.TimeRange(DateTime.MinValue, DateTime.MaxValue)),
            new(new WuH.Ruby.Common.Core.TimeRange(DateTime.MinValue, DateTime.MaxValue))
        },
        new List<TimeRange>
        {
            new(new WuH.Ruby.Common.Core.TimeRange(DateTime.MaxValue, DateTime.MinValue)),
            new(new WuH.Ruby.Common.Core.TimeRange(DateTime.MaxValue, DateTime.MinValue))
        },
        new List<TimeRange>
        {
            new(new WuH.Ruby.Common.Core.TimeRange()),
            new(new WuH.Ruby.Common.Core.TimeRange())
        }
    };

    public static TheoryData<List<TimeRange>> GetUnequalTimeRanges => new()
    {
        new List<TimeRange>
        {
            new(new WuH.Ruby.Common.Core.TimeRange(DateTime.MinValue, DateTime.MaxValue)),
            new(new WuH.Ruby.Common.Core.TimeRange(DateTime.MaxValue, DateTime.MinValue))
        },
        new List<TimeRange>
        {
            new(new WuH.Ruby.Common.Core.TimeRange()),
            new(new WuH.Ruby.Common.Core.TimeRange(DateTime.MaxValue, DateTime.MinValue))
        },
        new List<TimeRange>
        {
            new(new WuH.Ruby.Common.Core.TimeRange(DateTime.MaxValue, DateTime.MinValue)),
            new(new WuH.Ruby.Common.Core.TimeRange())
        }
    };
}