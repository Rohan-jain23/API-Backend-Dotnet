using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FrameworkAPI.DataLoaders;
using FrameworkAPI.Exceptions;
using FrameworkAPI.Services;
using FrameworkAPI.Test.Services.Helpers;
using Moq;
using WuH.Ruby.Common.Core;
using WuH.Ruby.MetaDataHandler.Client;
using Xunit;

namespace FrameworkAPI.Test.Services;

public class MachineMetaDataServiceTest
{
    private const string MachineId = "FakeMachineId";
    private const string Identifier = VariableIdentifier.JobQuantityActualInSecondUnit;

    private readonly MachineMetaDataService _machineMetaDataService = new();

    private readonly Mock<IMetaDataHandlerHttpClient> _metaDataHandlerHttpClientMock = new(MockBehavior.Strict);
    private readonly MachineMetaDataBatchDataLoader _machineMetaDataBatchDataLoader;

    public MachineMetaDataServiceTest()
    {
        _machineMetaDataBatchDataLoader =
            new MachineMetaDataBatchDataLoader(_metaDataHandlerHttpClientMock.Object, new DelayedBatchScheduler());
    }

    [Fact]
    public async Task GetMachineMetadata_Throws_Exception()
    {
        // Arrange
        var internalListResponse = new InternalListResponse<ProcessVariableMetaDataResponseItem>(
            (int)HttpStatusCode.InternalServerError, "ErrorMessage");

        MockMetaDataHandlerHttpClient(internalListResponse);

        // Act & Assert
        await Assert.ThrowsAsync<InternalServiceException>(async () => await _machineMetaDataService.GetMachineMetadata(
            _machineMetaDataBatchDataLoader,
            MachineId,
            Identifier,
            It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task GetMachineMetadata_Returns_Correct_Unit()
    {
        // Arrange
        var expectedMetadata = new ProcessVariableMetaData
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

        var internalListResponse = new InternalListResponse<ProcessVariableMetaDataResponseItem>([
            new ProcessVariableMetaDataResponseItem
            {
                Data = expectedMetadata,
                Path = Identifier
            }
        ]);

        MockMetaDataHandlerHttpClient(internalListResponse);

        // Act
        var machineMetadata = await _machineMetaDataService.GetMachineMetadata(
            _machineMetaDataBatchDataLoader,
            MachineId,
            Identifier,
            It.IsAny<CancellationToken>());

        // Assert
        machineMetadata.Should().NotBeNull();
        machineMetadata.Units.Should().Be(expectedMetadata.Units);
    }

    [Fact]
    public async Task GetMachineMetadata_Returns_With_No_Units()
    {
        // Arrange
        var expectedMetadata = new ProcessVariableMetaData
        {
            VariableIdentifier = VariableIdentifier.JobQuantityActualInSecondUnit
        };

        var internalListResponse = new InternalListResponse<ProcessVariableMetaDataResponseItem>([
            new ProcessVariableMetaDataResponseItem
            {
                Data = expectedMetadata,
                Path = Identifier
            }
        ]);

        MockMetaDataHandlerHttpClient(internalListResponse);

        // Act
        var machineMetadata = await _machineMetaDataService.GetMachineMetadata(
            _machineMetaDataBatchDataLoader,
            MachineId,
            Identifier,
            It.IsAny<CancellationToken>());

        // Assert
        machineMetadata.Should().NotBeNull();
        machineMetadata.Units.Should().BeNull();
    }

    private void MockMetaDataHandlerHttpClient(
        InternalListResponse<ProcessVariableMetaDataResponseItem> internalListResponse)
    {
        _metaDataHandlerHttpClientMock.Setup(s => s.GetProcessVariableMetaDataByIdentifiers(
                It.IsAny<CancellationToken>(),
                MachineId,
                new List<string> { Identifier }))
            .ReturnsAsync(internalListResponse);
    }
}