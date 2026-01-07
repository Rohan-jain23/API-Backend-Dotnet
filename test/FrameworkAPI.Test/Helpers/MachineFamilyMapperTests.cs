using FluentAssertions;
using FrameworkAPI.Helpers;
using Xunit;
using MachineDataHandler = WuH.Ruby.MachineDataHandler.Client;
using MachineFamily = FrameworkAPI.Schema.Misc.MachineFamily;

namespace FrameworkAPI.Test.Helpers;

public class MachineFamilyMapperTests
{
    [Theory]
    [InlineData(MachineDataHandler.MachineFamily.FlexoPrint, MachineFamily.FlexoPrint)]
    [InlineData(MachineDataHandler.MachineFamily.GravurePrint, MachineFamily.GravurePrint)]
    [InlineData(MachineDataHandler.MachineFamily.BlowFilm, MachineFamily.BlowFilm)]
    [InlineData(MachineDataHandler.MachineFamily.CastFilm, MachineFamily.CastFilm)]
    [InlineData(MachineDataHandler.MachineFamily.PaperSackBottomer, MachineFamily.PaperSackBottomer)]
    [InlineData(MachineDataHandler.MachineFamily.PaperSackTuber, MachineFamily.PaperSackTuber)]
    [InlineData((MachineDataHandler.MachineFamily)9999999, MachineFamily.Other)]
    public void MapToSchemaMachineFamily(MachineDataHandler.MachineFamily internalMachineFamily, MachineFamily expectedSchemaFamily)
    {
        // Act
        var schemaMachineFamily = internalMachineFamily.MapToSchemaMachineFamily();

        // Assert
        schemaMachineFamily.Should().Be(expectedSchemaFamily);
    }

    [Theory]
    [InlineData(MachineFamily.FlexoPrint, MachineDataHandler.MachineFamily.FlexoPrint)]
    [InlineData(MachineFamily.GravurePrint, MachineDataHandler.MachineFamily.GravurePrint)]
    [InlineData(MachineFamily.BlowFilm, MachineDataHandler.MachineFamily.BlowFilm)]
    [InlineData(MachineFamily.CastFilm, MachineDataHandler.MachineFamily.CastFilm)]
    [InlineData(MachineFamily.PaperSackBottomer, MachineDataHandler.MachineFamily.PaperSackBottomer)]
    [InlineData(MachineFamily.PaperSackTuber, MachineDataHandler.MachineFamily.PaperSackTuber)]
    [InlineData(MachineFamily.Other, null)]
    [InlineData((MachineFamily)9999999, null)]
    public void MapToInternalMachineFamily(MachineFamily schemaMachineFamily, MachineDataHandler.MachineFamily? expectedInternalMachineFamily)
    {
        // Act
        var internalMachineFamily = schemaMachineFamily.MapToInternalMachineFamily();

        // Assert
        internalMachineFamily.Should().Be(expectedInternalMachineFamily);
    }
}