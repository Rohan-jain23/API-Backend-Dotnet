using System;
using FluentAssertions;
using FrameworkAPI.Schema.ProductGroup;
using WuH.Ruby.KpiDataHandler.Client.Models;
using WuH.Ruby.MachineSnapShooter.Client;
using Xunit;

namespace FrameworkAPI.Test.Schema.ProductGroups;

public class PaperSackProductGroupAttributesTests
{
    [Theory]
    [InlineData(null)]
    [InlineData(true)]
    [InlineData(false)]
    public void PaperSackProductGroupAttributes_With_Bool_Should_Assign_Correct_Value(bool? value)
    {
        // Arrange
        var attributeType = PaperSackProductGroupAttributeType.Bool;
        var snapshotColumnId = SnapshotColumnIds.PaperSackProductIsFlushCut;

        var attribute = new PaperSackProductGroupAttribute(attributeType, snapshotColumnId, value);

        // Act
        var instance = new PaperSackProductGroupAttributes([attribute]);

        // Assert
        instance.IsFlushCut.Should().Be(value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    [InlineData(null)]
    public void PaperSackProductGroupAttributes_With_Integer_Should_Assign_Correct_Value(int? value)
    {
        // Arrange
        var attributeType = PaperSackProductGroupAttributeType.Integer;
        var snapshotColumnId = SnapshotColumnIds.PaperSackProductTubeLayers;

        var attribute = new PaperSackProductGroupAttribute(attributeType, snapshotColumnId, value);

        // Act
        var instance = new PaperSackProductGroupAttributes([attribute]);

        // Assert
        instance.TubeLayers.Should().Be(value);
    }

    [Fact]
    public void PaperSackProductGroupAttributes_With_Long_Should_Convert_To_Int_When_Possible()
    {
        // Arrange
        var attributeType = PaperSackProductGroupAttributeType.Integer;
        var snapshotColumnId = SnapshotColumnIds.PaperSackProductTubeLayers;

        var attribute = new PaperSackProductGroupAttribute(attributeType, snapshotColumnId, (long)int.MaxValue);

        // Act
        var instance = new PaperSackProductGroupAttributes([attribute]);

        // Assert
        instance.TubeLayers.Should().Be(int.MaxValue);
    }

    [Fact]
    public void PaperSackProductGroupAttributes_With_Long_Should_Throw_Error_If_Value_Too_Large()
    {
        // Arrange
        var attributeType = PaperSackProductGroupAttributeType.Integer;
        var snapshotColumnId = SnapshotColumnIds.PaperSackProductTubeLayers;

        var attribute = new PaperSackProductGroupAttribute(attributeType, snapshotColumnId, long.MaxValue);

        // Act
        var action = () => new PaperSackProductGroupAttributes([attribute]);

        // Assert
        action.Should().Throw<OverflowException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("300-400")]
    public void PaperSackProductGroupAttributes_With_Bucket_Should_Assign_Correct_Value(string? value)
    {
        // Arrange
        var attributeType = PaperSackProductGroupAttributeType.Bucket;
        var snapshotColumnId = SnapshotColumnIds.PaperSackProductSackDataSackWidth;

        var attribute = new PaperSackProductGroupAttribute(attributeType, snapshotColumnId, value);

        // Act
        var instance = new PaperSackProductGroupAttributes([attribute]);

        // Assert
        instance.SackWidth.Should().NotBeNull();
        instance.SackWidth.FormattedValue.Should().Be(value);
        instance.SackWidth.Unit.Should().Be("mm");
    }
}