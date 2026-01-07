using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using StrawberryShake;
using WuH.Ruby.Common.Core;
using WuH.Ruby.FrameworkAPI.Client.GraphQL;
using WuH.Ruby.FrameworkAPI.Client;
using Xunit;

namespace FrameworkAPI.Client.Test;

public class MaterialConsumptionTestsForProducedJob : FrameworkAPIBaseClass
{
    private const string ArbitraryMachineId = "EQ12345";
    private const string ArbitraryJobId = "Job 1";
    private const string ArbitraryMaterialName1 = "Material 1";
    private const string ArbitraryMaterialName2 = "Material 2";
    private const string ArbitraryMaterialName3 = "Material 3";
    private const double ArbitraryMaterialConsumption1 = 0.1234;
    private const double ArbitraryMaterialConsumption2 = 1.2345;
    private const double ArbitraryMaterialConsumption3 = 2.3456;
    private const string ArbitraryMaterialConsumptionUnit = "kg";

    private const string ExpectedNullDataError =
        "Unexpectedly received null for Data in from GraphQl in the operation result.";

    private static readonly RawMaterialConsumptionByMaterial ExpectedRawMaterialConsumptionByMaterialResult = new()
    {
        { ArbitraryMaterialName1, (ArbitraryMaterialConsumption1, ArbitraryMaterialConsumptionUnit) },
        { ArbitraryMaterialName2, (ArbitraryMaterialConsumption2, ArbitraryMaterialConsumptionUnit) },
        { ArbitraryMaterialName3, (ArbitraryMaterialConsumption3, ArbitraryMaterialConsumptionUnit) }
    };

    private readonly MockClientError _arbitraryClientError = new("Some error description ...");
    private readonly Mock<IFrameworkAPIGraphQLClient> _frameworkAPIGraphQLClientMock = new();
    private readonly IFrameworkAPIClientForProducedJob _subject;

    public MaterialConsumptionTestsForProducedJob()
    {
        _subject = new FrameworkAPIClientForProducedJob(_frameworkAPIGraphQLClientMock.Object);
    }

    [Fact]
    public async Task GetRawMaterialConsumptionByJob_GetsRawMaterialConsumptionByMaterial()
    {
        // Arrange
        var expectedJobResult =
            new MockIGenerateRawMaterialConsumptionByJobResult(ExpectedRawMaterialConsumptionByMaterialResult);
        InitializeMockFrameworkAPIGraphQLClient(ArbitraryMachineId, ArbitraryJobId, expectedJobResult);

        // Act
        var result = await _subject.GetExtrusionRawMaterialConsumptionByMaterial(
            ArbitraryMachineId,
            ArbitraryJobId,
            CancellationToken.None);

        // Assert
        var expectedResult =
            new InternalItemResponse<RawMaterialConsumptionByMaterial>(ExpectedRawMaterialConsumptionByMaterialResult);

        result.Equals(expectedResult);
    }

    [Fact]
    public async Task GetRawMaterialConsumptionByJob_ReturnsErrorWhenDataIsNull()
    {
        // Arrange
        InitializeMockFrameworkAPIGraphQLClient(ArbitraryMachineId, ArbitraryJobId, null);

        // Act
        var result = await _subject.GetExtrusionRawMaterialConsumptionByMaterial(
            ArbitraryMachineId,
            ArbitraryJobId,
            CancellationToken.None);

        // Assert
        result.Error.StatusCode.Should().Be(500);
        result.Error.ErrorMessage.Should().Be(ExpectedNullDataError);
    }

    [Fact]
    public async Task GetRawMaterialConsumptionByJob_ReturnsErrorWhenRawMaterialConsumptionByMaterialIsNull()
    {
        // Arrange
        var expectedJobResult = new MockIGenerateRawMaterialConsumptionByJobResult(null);
        InitializeMockFrameworkAPIGraphQLClient(
            ArbitraryMachineId,
            ArbitraryJobId,
            expectedJobResult,
            new List<IClientError>());

        // Act
        var result = await _subject.GetExtrusionRawMaterialConsumptionByMaterial(
            ArbitraryMachineId,
            ArbitraryJobId,
            CancellationToken.None);

        // Assert
        result.Error.StatusCode.Should().Be(500);
        result.Error.ErrorMessage.Should()
            .Be(
                "Unexpectedly received null for 'Data.ProducedJob.RawMaterialConsumptionByMaterial' in from GraphQl in the operation result.");
    }

    [Fact]
    public async Task GetRawMaterialConsumptionByJob_PassesErrorWhenOperationResultHasErrors()
    {
        // Arrange
        var errorList = new List<IClientError> { _arbitraryClientError };
        InitializeMockFrameworkAPIGraphQLClient(ArbitraryMachineId, ArbitraryJobId, null, errorList);

        // Act
        var result = await _subject.GetExtrusionRawMaterialConsumptionByMaterial(
            ArbitraryMachineId,
            ArbitraryJobId,
            CancellationToken.None);

        // Assert
        result.Error.StatusCode.Should().Be(500);
        result.Error.ErrorMessage.Should().Be(GenerateErrorMessage(errorList));
    }

    private void InitializeMockFrameworkAPIGraphQLClient(
        string machineId,
        string jobId,
        MockIGenerateRawMaterialConsumptionByJobResult? mockIGenerateRawMaterialConsumptionByJobResult,
        List<IClientError>? errorList = null)
    {
        var operationResult = new Mock<IOperationResult<IGenerateRawMaterialConsumptionByJobResult>>();

        operationResult
            .Setup(
                m => m.Errors
            )
            .Returns(errorList ?? []);

        operationResult
            .Setup(
                m => m.Data
            )
            .Returns(mockIGenerateRawMaterialConsumptionByJobResult);

        _frameworkAPIGraphQLClientMock
            .SetupSequence(
                x => x.GenerateRawMaterialConsumptionByJob.ExecuteAsync(
                    machineId,
                    jobId,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(operationResult.Object);
    }
}