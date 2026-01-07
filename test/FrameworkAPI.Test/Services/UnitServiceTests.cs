using System.Collections.Generic;
using FluentAssertions;
using FrameworkAPI.Services;
using WuH.Ruby.MetaDataHandler.Client;
using Xunit;

namespace FrameworkAPI.Test.Services;

public class UnitServiceTests
{
    public static IEnumerable<object?[]> GetMachineMetaDataIsNullTestData()
    {
        yield return new object?[] { null };
        yield return new object?[]
        {
            new ProcessVariableMetaData
            {
                Units = null
            }
        };
    }

    [Theory]
    [MemberData(nameof(GetMachineMetaDataIsNullTestData))]
    public void CalculateSiValue_MachineMetaDataIsNull_ReturnsValue(ProcessVariableMetaData? machineMetadata)
    {
        // Arrange
        const double expectedValue = 1234.5;

        var unitService = new UnitService();

        // Act
        var siValue = unitService.CalculateSiValue(expectedValue, machineMetadata);

        // Assert
        siValue.Should().Be(expectedValue);
    }

    [Fact]
    public void CalculateSiValue_MachineMetaDataUnitsIsNull_ReturnsValue()
    {
        // Arrange
        const double expectedValue = 1234.5;
        var machineMetadata = new ProcessVariableMetaData
        {
            Units = null
        };

        var unitService = new UnitService();

        // Act
        var siValue = unitService.CalculateSiValue(expectedValue, machineMetadata);

        // Assert
        siValue.Should().Be(expectedValue);
    }

    [Theory]
    [InlineData(10, 1.0, 0, 10)]
    [InlineData(10, 4.5, 5, 50)]
    [InlineData(20, 0.5, 0, 10)]
    public void CalculateSiValue_ProcessVariableMetaDataHasSiUnit_ReturnsCalculatedValue(
        double value, double multiplier, double offset, double expectedValue)
    {
        // Arrange
        var machineMetadata = new ProcessVariableMetaData
        {
            Units = new VariableUnits
            {
                Si = new VariableUnits.UnitWithCoefficient
                {
                    Multiplier = multiplier,
                    Offset = offset,
                }
            }
        };

        var unitService = new UnitService();

        // Act
        var siValue = unitService.CalculateSiValue(value, machineMetadata);

        // Assert
        siValue.Should().Be(expectedValue);
    }

    [Theory]
    [MemberData(nameof(GetMachineMetaDataIsNullTestData))]
    public void GetSiUnit_MachineMetaDataIsNull_ReturnsNull(ProcessVariableMetaData? machineMetadata)
    {
        // Arrange
        var unitService = new UnitService();

        // Act
        var siUnit = unitService.GetSiUnit(machineMetadata);

        // Assert
        siUnit.Should().BeNull();
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("km/h", "km/h")]
    [InlineData("unit.items", "unit.items")]
    [InlineData("STK", "unit.items")]
    [InlineData("unit.itemsPerMinute", "unit.itemsPerMinute")]
    [InlineData("STKMIN", "unit.itemsPerMinute")]
    [InlineData("LABEL.SLITROLLS", "LABEL.SLITROLLS")]
    [InlineData("Nutzen", "LABEL.SLITROLLS")]
    public void GetSiUnit_MachineMetaDataIsNull_ReturnsUnit(string? siUnit, string? expectedSiUnit)
    {
        // Arrange
        var machineMetadata = new ProcessVariableMetaData
        {
            Units = new VariableUnits
            {
                Si = new VariableUnits.UnitWithCoefficient
                {
                    Unit = siUnit
                }
            }
        };

        var unitService = new UnitService();

        // Act
        var siUnitObj = unitService.GetSiUnit(machineMetadata);

        // Assert
        siUnitObj.Should().Be(expectedSiUnit);
    }
}