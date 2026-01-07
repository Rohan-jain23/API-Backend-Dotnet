using System;
using System.Threading.Tasks;
using FluentAssertions;
using FrameworkAPI.Exceptions;
using FrameworkAPI.Services;
using FrameworkAPI.Services.Interfaces;
using Moq;
using Xunit;

namespace FrameworkAPI.Test.Services;

public class LicenceGuardTests
{
    private readonly LicenceGuard _licenceGuard;
    private readonly Mock<ILicenceService> _licenceServiceMock;

    public LicenceGuardTests()
    {
        _licenceServiceMock = new Mock<ILicenceService>();
        _licenceGuard = new LicenceGuard(_licenceServiceMock.Object);
    }

    [Theory]
    [InlineData((string?)null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task CheckMachineLicence_InvalidMachineIdParameter_ThrowsArgumentException(string? machineId)
    {
        // Arrange
        const string requiredLicence = "LICENCE";

        // Act
        var checkMachineLicenceAct = () => _licenceGuard.CheckMachineLicence(machineId!, requiredLicence);

        // Assert
        await checkMachineLicenceAct.Should().ThrowAsync<ArgumentException>();

        _licenceServiceMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData((string?)null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task CheckMachineLicence_InvalidRequiredLicenceParameter_ThrowsArgumentException(string? requiredLicence)
    {
        // Arrange
        const string machineId = "EQ12345";

        // Act
        var checkMachineLicenceAct = () => _licenceGuard.CheckMachineLicence(machineId, requiredLicence!);

        // Assert
        await checkMachineLicenceAct.Should().ThrowAsync<ArgumentException>();

        _licenceServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CheckMachineLicence_HasValidLicenceReturnsTrue_ReturnsWithoutException()
    {
        // Arrange
        const string machineId = "EQ12345";
        const string requiredLicence = "LICENCE";

        _licenceServiceMock
            .Setup(m => m.HasValidLicence(machineId, requiredLicence))
            .ReturnsAsync(true)
            .Verifiable(Times.Once);

        // Act
        var checkMachineLicenceAct = () => _licenceGuard.CheckMachineLicence(machineId, requiredLicence);

        // Assert
        await checkMachineLicenceAct.Should().NotThrowAsync();

        _licenceServiceMock.VerifyAll();
        _licenceServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CheckMachineLicence_HasValidLicenceReturnsFalse_Throws()
    {
        // Arrange
        const string machineId = "EQ12345";
        const string requiredLicence = "LICENCE";

        _licenceServiceMock
            .Setup(m => m.HasValidLicence(machineId, requiredLicence))
            .ReturnsAsync(false)
            .Verifiable(Times.Once);

        // Act
        var checkMachineLicenceAct = () => _licenceGuard.CheckMachineLicence(machineId, requiredLicence);

        // Assert
        await checkMachineLicenceAct.Should().ThrowAsync<InvalidLicenceException>();

        _licenceServiceMock.VerifyAll();
        _licenceServiceMock.VerifyNoOtherCalls();
    }
}