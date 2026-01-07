using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FrameworkAPI.Exceptions;
using FrameworkAPI.Helpers;
using FrameworkAPI.Schema.Misc;
using FrameworkAPI.Services;
using Microsoft.Extensions.Logging;
using Moq;
using WuH.Ruby.Common.Core;
using WuH.Ruby.MachineDataHandler.Client;
using Xunit;
using Machine = WuH.Ruby.MachineDataHandler.Client.Machine;
using MachineFamily = FrameworkAPI.Schema.Misc.MachineFamily;

namespace FrameworkAPI.Test.Services;

public class MachineServiceTests
{
    private readonly MachineService _subject;
    private readonly Mock<IMachineCachingService> _machineCachingServiceMock;

    public MachineServiceTests()
    {
        _machineCachingServiceMock = new Mock<IMachineCachingService>();
        _subject = new MachineService(_machineCachingServiceMock.Object, new Mock<ILogger<MachineService>>().Object);
    }

    [Fact]
    public async Task GetMachine_Returns_Machine()
    {
        // Arrange
        var machine = new Machine
        {
            MachineId = "FakeMachineId",
            BusinessUnit = BusinessUnit.Printing,
            MachineFamily = "PaperSackBottomer"
        };
        _machineCachingServiceMock
            .Setup(m => m.GetMachine("FakeMachineId", CancellationToken.None))
            .ReturnsAsync(machine);

        // Act
        var response = await _subject.GetMachine("FakeMachineId", CancellationToken.None);

        // Assert
        response.Should().BeEquivalentTo(FrameworkAPI.Schema.Machine.Machine.CreateInstance(machine));
    }

    [Fact]
    public async Task GetMachine_Throws_Exception_When_MachineCachingService_Has_No_Machine()
    {
        // Arrange
        _machineCachingServiceMock
            .Setup(m => m.GetMachine("FakeMachineId", CancellationToken.None))
            .ReturnsAsync(default(Machine));

        var getMachineAction = async () => await _subject.GetMachine("FakeMachineId", CancellationToken.None);

        // Act / Assert
        await getMachineAction.Should().ThrowAsync<InternalServiceException>();
    }

    [Fact]
    public async Task GetMachineBusinessUnit_Returns_BusinessUnit_By_MachineId()
    {
        // Arrange
        var machine = new Machine
        {
            MachineId = "FakeMachineId",
            BusinessUnit = BusinessUnit.Printing
        };
        _machineCachingServiceMock
            .Setup(m => m.GetMachinesAsInternalListResponse(CancellationToken.None))
            .ReturnsAsync(new InternalListResponse<Machine>(new List<Machine> { machine }));

        // Act
        var response = await _subject.GetMachineBusinessUnit("FakeMachineId", CancellationToken.None);

        // Assert
        response.Should().Be(machine.BusinessUnit.MapToSchemaMachineDepartment());
    }

    [Fact]
    public async Task GetMachineBusinessUnit_Throws_Exception_When_MachineCachingService_HasError()
    {
        // Arrange
        _machineCachingServiceMock
            .Setup(m => m.GetMachinesAsInternalListResponse(CancellationToken.None))
            .ReturnsAsync(
                new InternalListResponse<Machine>((int)HttpStatusCode.InternalServerError, "Oh no we have a error"));

        var getMachineBusinessUnitAction = async () =>
            await _subject.GetMachineBusinessUnit("FakeMachineId", CancellationToken.None);

        // Act / Assert
        await getMachineBusinessUnitAction.Should().ThrowAsync<InternalServiceException>();
    }

    [Fact]
    public async Task GetMachineFamily_Returns_BusinessUnit_By_MachineId()
    {
        // Arrange
        var machine = new Machine
        {
            MachineId = "FakeMachineId",
            MachineFamily = "PaperSackTuber"
        };
        _machineCachingServiceMock
            .Setup(m => m.GetMachinesAsInternalListResponse(CancellationToken.None))
            .ReturnsAsync(new InternalListResponse<Machine>(new List<Machine> { machine }));

        // Act
        var response = await _subject.GetMachineFamily("FakeMachineId", CancellationToken.None);

        // Assert
        response.Should().Be(machine.MachineFamilyEnum.MapToSchemaMachineFamily());
    }

    [Fact]
    public async Task GetMachineFamily_Throws_Exception_When_MachineCachingService_HasError()
    {
        // Arrange
        _machineCachingServiceMock
            .Setup(m => m.GetMachinesAsInternalListResponse(CancellationToken.None))
            .ReturnsAsync(
                new InternalListResponse<Machine>((int)HttpStatusCode.InternalServerError, "Oh no we have a error"));

        var getMachineFamilyAction =
            async () => await _subject.GetMachineFamily("FakeMachineId", CancellationToken.None);

        // Act / Assert
        await getMachineFamilyAction.Should().ThrowAsync<InternalServiceException>();
    }

    [Fact]
    public async Task GetAllMachines_Returns_All_Machines()
    {
        // Arrange
        var machine = new Machine
        {
            MachineId = "FakeMachineId",
            BusinessUnit = BusinessUnit.Printing,
            MachineFamily = "PaperSackBottomer"
        };
        _machineCachingServiceMock
            .Setup(m => m.GetMachinesAsInternalListResponse(CancellationToken.None))
            .ReturnsAsync(new InternalListResponse<Machine>(new List<Machine> { machine }));

        // Act
        var response = await _subject.GetAllMachines(CancellationToken.None);

        // Assert
        response.Should().BeEquivalentTo(
            new List<FrameworkAPI.Schema.Machine.Machine>
            {
                FrameworkAPI.Schema.Machine.Machine.CreateInstance(machine)
            });
    }

    [Fact]
    public async Task GetAllMachines_Throws_Exception_When_MachineCachingService_HasError()
    {
        // Arrange
        _machineCachingServiceMock
            .Setup(m => m.GetMachinesAsInternalListResponse(CancellationToken.None))
            .ReturnsAsync(
                new InternalListResponse<Machine>((int)HttpStatusCode.InternalServerError, "Oh no we have a error"));

        var getAllMachinesAction = async () => await _subject.GetAllMachines(CancellationToken.None);

        // Act / Assert
        await getAllMachinesAction.Should().ThrowAsync<InternalServiceException>();
    }

    [Fact]
    public async Task DoesMachineExist_Returns_True_when_Machine_Exists()
    {
        // Arrange
        var machine = new Machine
        {
            MachineId = "FakeMachineId",
            BusinessUnit = BusinessUnit.Printing,
            MachineFamily = "PaperSackBottomer"
        };
        _machineCachingServiceMock
            .Setup(m => m.GetMachine("FakeMachineId", CancellationToken.None))
            .ReturnsAsync(machine);

        // Act
        var response = await _subject.DoesMachineExist("FakeMachineId", CancellationToken.None);

        // Assert
        response.Should().BeTrue();
    }

    [Fact]
    public async Task DoesMachineExist_Returns_False_when_Machine_Does_Not_Exists()
    {
        // Arrange
        _machineCachingServiceMock
            .Setup(m => m.GetMachine("FakeMachineId", CancellationToken.None))
            .ReturnsAsync(default(Machine));

        // Act
        var response = await _subject.DoesMachineExist("FakeMachineId", CancellationToken.None);

        // Assert
        response.Should().BeFalse();
    }

    [Fact]
    public async Task GetMachineIdsByFilter_Returns_All_PaperSack_MachineIds()
    {
        // Arrange
        _machineCachingServiceMock
            .Setup(m => m.GetMachinesAsInternalListResponse(CancellationToken.None))
            .ReturnsAsync(new InternalListResponse<Machine>(GetMachineMockData()));

        // Act
        var response =
            await _subject.GetMachineIdsByFilter(null, MachineDepartment.PaperSack, null, CancellationToken.None);

        // Assert
        response.Should().BeEquivalentTo(new List<string>
            { "FakePaperSackBottomerMachineId", "FakePaperSackTuberMachineId" });
    }

    [Fact]
    public async Task GetMachineIdsByFilter_Returns_PaperSackBottomerMachineId()
    {
        // Arrange
        _machineCachingServiceMock
            .Setup(m => m.GetMachinesAsInternalListResponse(CancellationToken.None))
            .ReturnsAsync(new InternalListResponse<Machine>(GetMachineMockData()));

        // Act
        var response = await _subject.GetMachineIdsByFilter(null, MachineDepartment.PaperSack,
            MachineFamily.PaperSackBottomer, CancellationToken.None);

        // Assert
        response.Should().BeEquivalentTo(new List<string> { "FakePaperSackBottomerMachineId" });
    }

    [Fact]
    public async Task GetMachineIdsByFilter_Returns_FakeBlowFilmMachineId()
    {
        // Arrange
        _machineCachingServiceMock
            .Setup(m => m.GetMachinesAsInternalListResponse(CancellationToken.None))
            .ReturnsAsync(new InternalListResponse<Machine>(GetMachineMockData()));

        // Act
        var response =
            await _subject.GetMachineIdsByFilter("FakeBlowFilmMachineId", null, null, CancellationToken.None);

        // Assert
        response.Should().BeEquivalentTo(new List<string> { "FakeBlowFilmMachineId" });
    }

    [Fact]
    public async Task GetMachineIdsByFilter_Returns_EmptyList_When_No_Filter_Matches()
    {
        // Arrange
        _machineCachingServiceMock
            .Setup(m => m.GetMachinesAsInternalListResponse(CancellationToken.None))
            .ReturnsAsync(new InternalListResponse<Machine>(GetMachineMockData()));

        // Act
        var response = await _subject.GetMachineIdsByFilter("FakeBlowFilmMachineId", MachineDepartment.PaperSack, null,
            CancellationToken.None);

        // Assert
        response.Should().BeEquivalentTo(new List<string>());
    }

    [Fact]
    public async Task GetMachineIdsByFilter_Returns_EmptyList_When_MachineCachingServices_Has_No_Machines()
    {
        // Arrange
        _machineCachingServiceMock
            .Setup(m => m.GetMachinesAsInternalListResponse(CancellationToken.None))
            .ReturnsAsync(new InternalListResponse<Machine>((int)HttpStatusCode.NoContent, "No machines"));

        // Act
        var response = await _subject.GetMachineIdsByFilter("FakeBlowFilmMachineId", MachineDepartment.PaperSack, null,
            CancellationToken.None);

        // Assert
        response.Should().BeEquivalentTo(new List<string>());
    }

    [Fact]
    public async Task GetMachineIdsByFilter_Throws_Exception_When_MachineCachingService_HasError()
    {
        // Arrange
        _machineCachingServiceMock
            .Setup(m => m.GetMachinesAsInternalListResponse(CancellationToken.None))
            .ReturnsAsync(
                new InternalListResponse<Machine>((int)HttpStatusCode.InternalServerError, "Oh no we have a error"));

        var getMachineIdsByFilterAction = async () =>
            await _subject.GetMachineIdsByFilter(null, null, null, CancellationToken.None);

        // Act / Assert
        await getMachineIdsByFilterAction.Should().ThrowAsync<InternalServiceException>();
    }

    private static List<Machine> GetMachineMockData()
    {
        return new List<Machine>
        {
            new()
            {
                MachineId = "FakeFlexoPrintMachineId",
                BusinessUnit = BusinessUnit.Printing,
                MachineFamily = "FlexoPrint"
            },
            new()
            {
                MachineId = "FakePaperSackBottomerMachineId",
                BusinessUnit = BusinessUnit.PaperSack,
                MachineFamily = "PaperSackBottomer"
            },
            new()
            {
                MachineId = "FakePaperSackTuberMachineId",
                BusinessUnit = BusinessUnit.PaperSack,
                MachineFamily = "PaperSackTuber"
            },
            new()
            {
                MachineId = "FakeBlowFilmMachineId",
                BusinessUnit = BusinessUnit.Extrusion,
                MachineFamily = "BlowFilm"
            },
            new()
            {
                MachineId = "FakeOtherMachineId",
                BusinessUnit = BusinessUnit.Other,
                MachineFamily = "Slitter"
            }
        };
    }
}