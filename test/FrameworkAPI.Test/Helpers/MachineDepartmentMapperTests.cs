using FluentAssertions;
using FrameworkAPI.Helpers;
using FrameworkAPI.Schema.Misc;
using Xunit;
using MachineDataHandler = WuH.Ruby.MachineDataHandler.Client;

namespace FrameworkAPI.Test.Helpers;

public class MachineDepartmentMapperTests
{
    [Theory]
    [InlineData(MachineDataHandler.BusinessUnit.Extrusion, MachineDepartment.Extrusion)]
    [InlineData(MachineDataHandler.BusinessUnit.PaperSack, MachineDepartment.PaperSack)]
    [InlineData(MachineDataHandler.BusinessUnit.Printing, MachineDepartment.Printing)]
    [InlineData(MachineDataHandler.BusinessUnit.Other, MachineDepartment.Other)]
    [InlineData((MachineDataHandler.BusinessUnit)101010101, MachineDepartment.Other)]
    public void MapToSchemaMachineDepartment(MachineDataHandler.BusinessUnit businessUnit, MachineDepartment expectedDepartment)
    {
        // Act
        var department = businessUnit.MapToSchemaMachineDepartment();

        // Assert
        department.Should().Be(expectedDepartment);
    }

    [Theory]
    [InlineData(MachineDepartment.Extrusion, MachineDataHandler.BusinessUnit.Extrusion)]
    [InlineData(MachineDepartment.PaperSack, MachineDataHandler.BusinessUnit.PaperSack)]
    [InlineData(MachineDepartment.Printing, MachineDataHandler.BusinessUnit.Printing)]
    [InlineData(MachineDepartment.Other, MachineDataHandler.BusinessUnit.Other)]
    [InlineData((MachineDepartment)101010101, MachineDataHandler.BusinessUnit.Other)]
    public void MapToInternalBusinessUnit(MachineDepartment department, MachineDataHandler.BusinessUnit expectedBusinessUnit)
    {
        // Act
        var businessUnit = department.MapToInternalBusinessUnit();

        // Assert
        businessUnit.Should().Be(expectedBusinessUnit);
    }
}