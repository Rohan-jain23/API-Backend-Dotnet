using System;
using System.Collections.Generic;
using FluentAssertions;
using FrameworkAPI.Extensions;
using WuH.Ruby.Common.Core;
using Xunit;

namespace FrameworkAPI.Test.Helpers;

public class DateTimeExtensionsTests
{
    [Fact]
    public void Flatten_With_Single_Ranges_Returns_That_Range()
    {
        // Arrange
        List<TimeRange> list = [
            new TimeRange(DateTime.UnixEpoch, DateTime.UnixEpoch.AddDays(10))
        ];

        // Act
        var result = list.Flatten();

        // Assert
        result.Should().BeEquivalentTo([
            new TimeRange(DateTime.UnixEpoch, DateTime.UnixEpoch.AddDays(10))
        ]);
    }

    [Fact]
    public void Flatten_With_Disjunct_Ranges_Returns_Those_Ranges()
    {
        // Arrange
        List<TimeRange> list = [
            new TimeRange(DateTime.UnixEpoch, DateTime.UnixEpoch.AddDays(10)),
            new TimeRange(DateTime.UnixEpoch.AddDays(20), DateTime.UnixEpoch.AddDays(30)),
            new TimeRange(DateTime.UnixEpoch.AddDays(60), DateTime.UnixEpoch.AddDays(90))
        ];

        // Act
        var result = list.Flatten();

        // Assert
        result.Should().BeEquivalentTo([
            new TimeRange(DateTime.UnixEpoch, DateTime.UnixEpoch.AddDays(10)),
            new TimeRange(DateTime.UnixEpoch.AddDays(20), DateTime.UnixEpoch.AddDays(30)),
            new TimeRange(DateTime.UnixEpoch.AddDays(60), DateTime.UnixEpoch.AddDays(90))
        ]);
    }

    [Fact]
    public void Flatten_With_Overlapping_Ranges_Returns_One_Range()
    {
        // Arrange
        List<TimeRange> list = [
            new TimeRange(DateTime.UnixEpoch, DateTime.UnixEpoch.AddDays(30)),
            new TimeRange(DateTime.UnixEpoch.AddDays(20), DateTime.UnixEpoch.AddDays(60)),
            new TimeRange(DateTime.UnixEpoch.AddDays(60), DateTime.UnixEpoch.AddDays(90))
        ];

        // Act
        var result = list.Flatten();

        // Assert
        result.Should().BeEquivalentTo([
            new TimeRange(DateTime.UnixEpoch, DateTime.UnixEpoch.AddDays(90)),
        ]);
    }

    [Fact]
    public void Flatten_With_Overlapping_Ranges_Returns_Multiple_Range()
    {
        // Arrange
        List<TimeRange> list = [
            new TimeRange(DateTime.UnixEpoch, DateTime.UnixEpoch.AddDays(30)),
            new TimeRange(DateTime.UnixEpoch.AddDays(20), DateTime.UnixEpoch.AddDays(50)),
            new TimeRange(DateTime.UnixEpoch.AddDays(60), DateTime.UnixEpoch.AddDays(90)),
            new TimeRange(DateTime.UnixEpoch.AddDays(80), DateTime.UnixEpoch.AddDays(100))
        ];

        // Act
        var result = list.Flatten();

        // Assert
        result.Should().BeEquivalentTo([
            new TimeRange(DateTime.UnixEpoch, DateTime.UnixEpoch.AddDays(50)),
            new TimeRange(DateTime.UnixEpoch.AddDays(60), DateTime.UnixEpoch.AddDays(100))
        ]);
    }
}