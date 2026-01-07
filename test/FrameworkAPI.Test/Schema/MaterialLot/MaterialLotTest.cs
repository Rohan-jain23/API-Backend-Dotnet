using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FrameworkAPI.DataLoaders;
using FrameworkAPI.Services.Interfaces;
using FrameworkAPI.Test.Services.Helpers;
using Moq;
using WuH.Ruby.Common.Core;
using WuH.Ruby.MaterialDataHandler.Client.Enums.Lot;
using WuH.Ruby.MaterialDataHandler.Client.Models.Lot;
using WuH.Ruby.MetaDataHandler.Client;
using Xunit;

namespace FrameworkAPI.Test.Schema.MaterialLot;

public class MaterialLotTest
{
    private const string MachineId = "EQ99902";

    private readonly Mock<IUnitService> _unitServiceMock = new();
    private readonly Mock<IMachineMetaDataService> _machineMetaDataServiceMock = new();
    private readonly MachineMetaDataBatchDataLoader _machineMetaDataBatchDataLoader;
    private readonly FrameworkAPI.Schema.MaterialLot.MaterialLot? _materialLot;

    private readonly Lot _lot = new()
    {
        GeneralProperties = new GeneralProperties
        {
            MachineId = MachineId,
            Id = "testId",
            Quantity = 285
        },
        MaterialClass = TypeOfMaterial.ExtrudedRoll,
        SchemaVersion = 1
    };

    public MaterialLotTest()
    {
        _machineMetaDataBatchDataLoader = new MachineMetaDataBatchDataLoader(
            new Mock<IMetaDataHandlerHttpClient>().Object,
            new DelayedBatchScheduler());
        _materialLot = FrameworkAPI.Schema.MaterialLot.MaterialLot.CreateInstance(_lot);
    }

    [Fact]
    public async Task Quantity_Has_Correct_Unit()
    {
        // Arrange
        const string expectedSiUnit = "m";
        var processVariableMetaData = new ProcessVariableMetaData
        {
            VariableIdentifier = VariableIdentifier.JobQuantityActualInSecondUnit,
            Units = new VariableUnits
            {
                Si = new VariableUnits.UnitWithCoefficient
                {
                    Unit = "m"
                }
            }
        };

        MockMachineMetaDataService(processVariableMetaData);
        MockUnitService(processVariableMetaData, expectedSiUnit);

        // Act
        var quantity = await _materialLot?.Quantity(
            _machineMetaDataBatchDataLoader,
            _machineMetaDataServiceMock.Object,
            _unitServiceMock.Object,
            MachineId,
            It.IsAny<CancellationToken>())!;

        var unit = await quantity.Unit(It.IsAny<CancellationToken>());
        var quantityValue = await quantity.Value(It.IsAny<CancellationToken>());

        // Assert
        quantityValue.Should().BeOfType(typeof(double));
        unit.Should().NotBeNull();
        unit.Should().Be(expectedSiUnit);
    }

    [Fact]
    public async Task Quantity_Unit_Is_Null()
    {
        // Arrange
        var processVariableMetaData = new ProcessVariableMetaData
        {
            VariableIdentifier = VariableIdentifier.JobQuantityActualInSecondUnit,
            Units = new VariableUnits
            {
                Si = new VariableUnits.UnitWithCoefficient
                {
                    Unit = "m"
                }
            }
        };

        MockMachineMetaDataService(processVariableMetaData);
        MockUnitService(processVariableMetaData, null);

        // Act
        var quantity = await _materialLot?.Quantity(
            _machineMetaDataBatchDataLoader,
            _machineMetaDataServiceMock.Object,
            _unitServiceMock.Object,
            MachineId,
            It.IsAny<CancellationToken>())!;

        var unit = await quantity.Unit(It.IsAny<CancellationToken>());
        var quantityValue = await quantity.Value(It.IsAny<CancellationToken>());

        // Assert
        quantityValue.Should().BeOfType(typeof(double));
        unit.Should().BeNull();
    }

    private void MockUnitService(ProcessVariableMetaData processVariableMetaData, string? expectedSiUnit)
    {
        _unitServiceMock
            .Setup(s => s.GetSiUnit(processVariableMetaData))
            .Returns(expectedSiUnit);
    }

    private void MockMachineMetaDataService(ProcessVariableMetaData processVariableMetaData)
    {
        _machineMetaDataServiceMock
            .Setup(s => s.GetMachineMetadata(
                _machineMetaDataBatchDataLoader,
                MachineId,
                VariableIdentifier.JobQuantityActualInSecondUnit,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(processVariableMetaData);
    }
}