using System;
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

public class MaterialConsumptionTestsForMachineTimeSpan : FrameworkAPIBaseClass
{
    private const string ArbitraryMachineId = "EQ12345";
    private const string ArbitraryMaterialName1 = "Material 1";
    private const string ArbitraryMaterialName2 = "Material 2";
    private const string ArbitraryMaterialName3 = "Material 3";
    private const double ArbitraryMaterialConsumption1 = 0.1234;
    private const double ArbitraryMaterialConsumption2 = 1.2345;
    private const double ArbitraryMaterialConsumption3 = 2.3456;
    private const string ArbitraryMaterialConsumptionUnit = "kg";

    private const string ExpectedNullDataError =
        "Unexpectedly received null for Data in from GraphQl in the operation result.";

    private static readonly TimeRange ArbitraryTimeRange = new(DateTime.UnixEpoch, DateTime.UnixEpoch.AddDays(1));

    private static readonly RawMaterialConsumptionByMaterial ExpectedRawMaterialConsumptionByMaterialResult = new()
    {
        { ArbitraryMaterialName1, (ArbitraryMaterialConsumption1, ArbitraryMaterialConsumptionUnit) },
        { ArbitraryMaterialName2, (ArbitraryMaterialConsumption2, ArbitraryMaterialConsumptionUnit) },
        { ArbitraryMaterialName3, (ArbitraryMaterialConsumption3, ArbitraryMaterialConsumptionUnit) }
    };

    private readonly MockClientError _arbitraryClientError = new("Some error description ...");
    private readonly Mock<IFrameworkAPIGraphQLClient> _frameworkAPIGraphQLClientMock = new();
    private readonly IFrameworkAPIClientForMachineTimeSpan _subject;

    public MaterialConsumptionTestsForMachineTimeSpan()
    {
        _subject = new FrameworkAPIClientForMachineTimeSpan(_frameworkAPIGraphQLClientMock.Object);
    }

    [Fact]
    public async Task GetRawMaterialConsumptionByTimeSpan_GetsRawMaterialConsumptionByMaterial()
    {
        // Arrange
        var expectedTimeSpanResult = new MockGenerateRawMaterialConsumptionByTimeSpanResult(
            ExpectedRawMaterialConsumptionByMaterialResult);
        InitializeMockFrameworkAPIGraphQLClient(ArbitraryMachineId, ArbitraryTimeRange, expectedTimeSpanResult);

        // Act
        var result = await _subject.GetExtrusionRawMaterialConsumptionByMaterial(
            ArbitraryMachineId,
            ArbitraryTimeRange,
            CancellationToken.None);

        // Assert
        var expectedResult =
            new InternalItemResponse<RawMaterialConsumptionByMaterial>(ExpectedRawMaterialConsumptionByMaterialResult);

        result.Equals(expectedResult);
    }

    [Fact]
    public async Task GetRawMaterialConsumptionByTimeSpan_ReturnsErrorWhenDataIsNull()
    {
        // Arrange
        InitializeMockFrameworkAPIGraphQLClient(ArbitraryMachineId, ArbitraryTimeRange, null);

        // Act
        var result = await _subject.GetExtrusionRawMaterialConsumptionByMaterial(
            ArbitraryMachineId,
            ArbitraryTimeRange,
            CancellationToken.None);

        // Assert
        result.Error.StatusCode.Should().Be(500);
        result.Error.ErrorMessage.Should().Be(ExpectedNullDataError);
    }

    [Fact]
    public async Task GetRawMaterialConsumptionByTimeSpan_ReturnsErrorWhenRawMaterialConsumptionByMaterialIsNull()
    {
        // Arrange
        var expectedJobResult = new MockGenerateRawMaterialConsumptionByTimeSpanResult(null);
        InitializeMockFrameworkAPIGraphQLClient(ArbitraryMachineId, ArbitraryTimeRange, expectedJobResult);

        // Act
        var result =
            await _subject.GetExtrusionRawMaterialConsumptionByMaterial(
                ArbitraryMachineId,
                ArbitraryTimeRange,
                CancellationToken.None);

        // Assert
        result.Error.StatusCode.Should().Be(500);
        result.Error.ErrorMessage.Should()
            .Be(
                "Unexpectedly received null for 'Data.MachineTimeSpan.RawMaterialConsumptionByMaterial' in from GraphQl in the operation result.");
    }

    [Fact]
    public async Task GetRawMaterialConsumptionByTimeSpan_PassesErrorWhenOperationResultHasErrors()
    {
        // Arrange
        var errorList = new List<IClientError> { _arbitraryClientError };
        InitializeMockFrameworkAPIGraphQLClient(ArbitraryMachineId, ArbitraryTimeRange, null, errorList);

        // Act
        var result =
            await _subject.GetExtrusionRawMaterialConsumptionByMaterial(
                ArbitraryMachineId,
                ArbitraryTimeRange,
                CancellationToken.None);

        // Assert
        result.Error.StatusCode.Should().Be(500);
        result.Error.ErrorMessage.Should().Be(GenerateErrorMessage(errorList));
    }

    private void InitializeMockFrameworkAPIGraphQLClient(
        string machineId,
        TimeRange timeRange,
        IGenerateRawMaterialConsumptionByTimeSpanResult? generateRawMaterialConsumptionByTimeSpanResult,
        List<IClientError>? errorList = null)
    {
        var operationResult = new Mock<IOperationResult<IGenerateRawMaterialConsumptionByTimeSpanResult>>();

        operationResult
            .Setup(
                m => m.Errors
            )
            .Returns(errorList ?? []);

        operationResult
            .Setup(
                m => m.Data
            )
            .Returns(generateRawMaterialConsumptionByTimeSpanResult);

        _frameworkAPIGraphQLClientMock
            .SetupSequence(
                x => x.GenerateRawMaterialConsumptionByTimeSpan.ExecuteAsync(
                    timeRange.From,
                    timeRange.To,
                    machineId,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(operationResult.Object);
    }
}