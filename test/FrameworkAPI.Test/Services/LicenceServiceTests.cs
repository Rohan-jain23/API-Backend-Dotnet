using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FrameworkAPI.Exceptions;
using FrameworkAPI.Services;
using Microsoft.AspNetCore.Http;
using Moq;
using WuH.Ruby.Common.Core;
using WuH.Ruby.LicenceManager.Client;
using WuH.Ruby.MachineDataHandler.Client;
using Xunit;

namespace FrameworkAPI.Test.Services;

public class LicenceServiceTests
{
    private readonly LicenceService _licenceService;
    private readonly Mock<IMachineCachingService> _machineCachingServiceMock = new();
    private readonly Mock<ILicenceManagerCachingService> _licenceManagerCachingServiceMock = new();

    public LicenceServiceTests()
    {
        _licenceService = new LicenceService(_machineCachingServiceMock.Object, _licenceManagerCachingServiceMock.Object);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task HasValidLicence_GetDetailedRubyInstanceLicenceValidityReturnsLicenceValidationInfo_ReturnTrue(bool isValid)
    {
        // Arrange
        const string requiredLicence = "LICENCE";

        _licenceManagerCachingServiceMock
            .Setup(m => m.GetDetailedRubyInstanceLicenceValidity(requiredLicence, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<LicenceValidationInfo>(new LicenceValidationInfo { IsValid = isValid }))
            .Verifiable(Times.Once);

        // Act
        var hasValidLicence = await _licenceService.HasValidLicence(requiredLicence);

        // Assert
        hasValidLicence.Should().Be(isValid);

        _licenceManagerCachingServiceMock.VerifyAll();
        _licenceManagerCachingServiceMock.VerifyNoOtherCalls();

        _machineCachingServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HasValidLicence_GetDetailedRubyInstanceLicenceValidityReturnsNoContentResponse_ReturnFalse()
    {
        // Arrange
        const string requiredLicence = "LICENCE";

        _licenceManagerCachingServiceMock
            .Setup(m => m.GetDetailedRubyInstanceLicenceValidity(requiredLicence, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<LicenceValidationInfo>(StatusCodes.Status204NoContent, "No content"))
            .Verifiable(Times.Once);

        // Act
        var hasValidLicence = await _licenceService.HasValidLicence(requiredLicence);

        // Assert
        hasValidLicence.Should().BeFalse();

        _licenceManagerCachingServiceMock.VerifyAll();
        _licenceManagerCachingServiceMock.VerifyNoOtherCalls();

        _machineCachingServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HasValidLicence_GetDetailedRubyInstanceLicenceValidityReturnsErrorResponse_ThrowsInternalServiceException()
    {
        // Arrange
        const string requiredLicence = "LICENCE";

        _licenceManagerCachingServiceMock
            .Setup(m => m.GetDetailedRubyInstanceLicenceValidity(requiredLicence, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<LicenceValidationInfo>(StatusCodes.Status500InternalServerError, "Internal server error"))
            .Verifiable(Times.Once);

        // Act
        var hasValidLicenceAct = () => _licenceService.HasValidLicence(requiredLicence);

        // Assert
        await hasValidLicenceAct.Should().ThrowAsync<InternalServiceException>();

        _licenceManagerCachingServiceMock.VerifyAll();
        _licenceManagerCachingServiceMock.VerifyNoOtherCalls();

        _machineCachingServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HasValidLicence_Anilox_MachineWithValidLicenceFound_ReturnsTrue()
    {
        // Arrange
        const string aniloxLicenceKey = Constants.LicensesApplications.Anilox;

        var firstMachine = new Machine { MachineId = "EQ12345" };
        var secondMachine = new Machine { MachineId = "EQ54321" };
        var thrirdMachine = new Machine { MachineId = "EQ54321" };
        _machineCachingServiceMock
            .Setup(m => m.GetMachines(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync([firstMachine, secondMachine, thrirdMachine])
            .Verifiable(Times.Once);
        _licenceManagerCachingServiceMock
            .Setup(m => m.GetDetailedLicenceValidity(firstMachine.MachineId, aniloxLicenceKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<LicenceValidationInfo>(new LicenceValidationInfo { IsValid = false }))
            .Verifiable(Times.Once);
        _licenceManagerCachingServiceMock
            .Setup(m => m.GetDetailedLicenceValidity(secondMachine.MachineId, aniloxLicenceKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<LicenceValidationInfo>(new LicenceValidationInfo { IsValid = true }))
            .Verifiable(Times.Once);

        // Act
        var hasValidLicence = await _licenceService.HasValidLicence(aniloxLicenceKey);

        // Assert
        hasValidLicence.Should().BeTrue();

        _machineCachingServiceMock.VerifyAll();
        _machineCachingServiceMock.VerifyNoOtherCalls();

        _licenceManagerCachingServiceMock.VerifyAll();
        _licenceManagerCachingServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HasValidLicence_Anilox_MachineWithoutValidLicenceFound_ReturnsTrue()
    {
        // Arrange
        const string aniloxLicenceKey = Constants.LicensesApplications.Anilox;

        var firstMachine = new Machine { MachineId = "EQ12345" };
        var secondMachine = new Machine { MachineId = "EQ54321" };
        _machineCachingServiceMock
            .Setup(m => m.GetMachines(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync([firstMachine, secondMachine])
            .Verifiable(Times.Once);
        _licenceManagerCachingServiceMock
            .Setup(m => m.GetDetailedLicenceValidity(firstMachine.MachineId, aniloxLicenceKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<LicenceValidationInfo>(new LicenceValidationInfo { IsValid = false }))
            .Verifiable(Times.Once);
        _licenceManagerCachingServiceMock
            .Setup(m => m.GetDetailedLicenceValidity(secondMachine.MachineId, aniloxLicenceKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<LicenceValidationInfo>(new LicenceValidationInfo { IsValid = false }))
            .Verifiable(Times.Once);

        // Act
        var hasValidLicence = await _licenceService.HasValidLicence(aniloxLicenceKey);

        // Assert
        hasValidLicence.Should().BeFalse();

        _machineCachingServiceMock.VerifyAll();
        _machineCachingServiceMock.VerifyNoOtherCalls();

        _licenceManagerCachingServiceMock.VerifyAll();
        _licenceManagerCachingServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HasValidLicence_Anilox_GetMachinesReturnsEmptyList_ReturnsFalse()
    {
        // Arrange
        const string aniloxLicenceKey = Constants.LicensesApplications.Anilox;

        _machineCachingServiceMock
            .Setup(m => m.GetMachines(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync([])
            .Verifiable(Times.Once);

        // Act
        var hasValidLicence = await _licenceService.HasValidLicence(aniloxLicenceKey);

        // Assert
        hasValidLicence.Should().BeFalse();

        _machineCachingServiceMock.VerifyAll();
        _machineCachingServiceMock.VerifyNoOtherCalls();

        _licenceManagerCachingServiceMock.VerifyAll();
        _licenceManagerCachingServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HasValidLicence_Anilox_GetMachinesReturnsNull_ReturnsFalse()
    {
        // Arrange
        const string aniloxLicenceKey = Constants.LicensesApplications.Anilox;

        _machineCachingServiceMock
            .Setup(m => m.GetMachines(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<Machine>?)null)
            .Verifiable(Times.Once);

        // Act
        var hasValidLicence = await _licenceService.HasValidLicence(aniloxLicenceKey);

        // Assert
        hasValidLicence.Should().BeFalse();

        _machineCachingServiceMock.VerifyAll();
        _machineCachingServiceMock.VerifyNoOtherCalls();

        _licenceManagerCachingServiceMock.VerifyAll();
        _licenceManagerCachingServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HasValidLicence_Anilox_GetDetailedLicenceValidityReturnsErrorResponse_ThrowsInternalServiceException()
    {
        // Arrange
        const string aniloxLicenceKey = Constants.LicensesApplications.Anilox;

        var machine = new Machine { MachineId = "EQ12345" };
        _machineCachingServiceMock
            .Setup(m => m.GetMachines(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync([machine])
            .Verifiable(Times.Once);
        _licenceManagerCachingServiceMock
            .Setup(m => m.GetDetailedLicenceValidity(machine.MachineId, aniloxLicenceKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<LicenceValidationInfo>(StatusCodes.Status500InternalServerError, "Inernal server error"))
            .Verifiable(Times.Once);

        // Act
        var hasValidLicenceAct = () => _licenceService.HasValidLicence(aniloxLicenceKey);

        // Assert
        await hasValidLicenceAct.Should().ThrowAsync<InternalServiceException>();

        _machineCachingServiceMock.VerifyAll();
        _machineCachingServiceMock.VerifyNoOtherCalls();

        _licenceManagerCachingServiceMock.VerifyAll();
        _licenceManagerCachingServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HasValidLicence_Anilox_SkipsSimulationMachineEQ10101_NoValidLicenceFound_ReturnsFalse()
    {
        // Arrange
        const string aniloxLicenceKey = Constants.LicensesApplications.Anilox;

        var machine = new Machine { MachineId = "EQ10101" };
        _machineCachingServiceMock
            .Setup(m => m.GetMachines(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync([machine])
            .Verifiable(Times.Once);

        // Act
        var hasValidLicence = await _licenceService.HasValidLicence(aniloxLicenceKey);

        // Assert
        hasValidLicence.Should().BeFalse();

        _machineCachingServiceMock.VerifyAll();
        _machineCachingServiceMock.VerifyNoOtherCalls();

        _licenceManagerCachingServiceMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task HasValidLicence_WithMachineId_GetDetailedLicenceValidityReturnsLicenceInfo_ReturnsIsValid(bool isValid)
    {
        // Arrange
        const string machineId = "EQ12345";
        const string requiredLicence = "LICENCE_KEY";

        _licenceManagerCachingServiceMock
            .Setup(m => m.GetDetailedLicenceValidity(machineId, requiredLicence, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<LicenceValidationInfo>(new LicenceValidationInfo { IsValid = isValid }))
            .Verifiable(Times.Once);

        // Act
        var hasValidLicence = await _licenceService.HasValidLicence(machineId, requiredLicence);

        // Assert
        hasValidLicence.Should().Be(isValid);

        _machineCachingServiceMock.VerifyNoOtherCalls();

        _licenceManagerCachingServiceMock.VerifyAll();
        _licenceManagerCachingServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HasValidLicence_WithMachineId_GetDetailedLicenceValidityReturnsErrorResponse_ThrowsInternalServiceException()
    {
        // Arrange
        const string machineId = "EQ12345";
        const string requiredLicence = "LICENCE_KEY";

        _licenceManagerCachingServiceMock
            .Setup(m => m.GetDetailedLicenceValidity(machineId, requiredLicence, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<LicenceValidationInfo>(StatusCodes.Status500InternalServerError, "Inernal server error"))
            .Verifiable(Times.Once);

        // Act
        var hasValidLicenceAct = () => _licenceService.HasValidLicence(machineId, requiredLicence);

        // Assert
        await hasValidLicenceAct.Should().ThrowAsync<InternalServiceException>();

        _machineCachingServiceMock.VerifyNoOtherCalls();

        _licenceManagerCachingServiceMock.VerifyAll();
        _licenceManagerCachingServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetMachineLicenses_GetAllDetailedLicenceValidityReturnsLicences_ReturnsLicences()
    {
        // Arrange
        var now = DateTime.UtcNow;
        const string machineId = "EQ12345";
        var goLicence = new LicenceValidationInfo { ActivationDate = now.AddDays(-3), ExpiryDate = now.AddDays(3), IsValid = true };
        var trackLicence = new LicenceValidationInfo { ActivationDate = now.AddDays(-2), ExpiryDate = now.AddDays(9), IsValid = false };

        var licences = new Dictionary<string, LicenceValidationInfo>()
        {
            { Constants.LicensesApplications.Go, goLicence },
            { Constants.LicensesApplications.Track, trackLicence }
        };

        _licenceManagerCachingServiceMock
            .Setup(m => m.GetAllDetailedLicenceValidity(machineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<Dictionary<string, LicenceValidationInfo>>(licences))
            .Verifiable(Times.Once);

        // Act
        var rubyLicences = await _licenceService.GetMachineLicenses(machineId, CancellationToken.None);

        // Assert
        rubyLicences.Should().NotBeNull();

        rubyLicences!.HasValidAniloxLicense.Should().BeFalse();
        rubyLicences.ExpiryDateOfAniloxLicense.Should().BeNull();
        rubyLicences.HasValidCheckLicense.Should().BeFalse();
        rubyLicences.ExpiryDateOfCheckLicense.Should().BeNull();
        rubyLicences.HasValidConnect4FlowLicense.Should().BeFalse();
        rubyLicences.ExpiryDateOfConnect4FlowLicense.Should().BeNull();
        rubyLicences.HasValidGoLicense.Should().Be(goLicence.IsValid);
        rubyLicences.ExpiryDateOfGoLicense.Should().Be(goLicence.ExpiryDate);
        rubyLicences.HasValidTrackLicense.Should().Be(trackLicence.IsValid);
        rubyLicences.ExpiryDateOfTrackLicense.Should().Be(trackLicence.ExpiryDate);

        _licenceManagerCachingServiceMock.VerifyAll();
        _licenceManagerCachingServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetMachineLicenses_GetAllDetailedLicenceValidityReturnsNoContent_ReturnsLicences()
    {
        // Arrange
        const string machineId = "EQ12345";

        _licenceManagerCachingServiceMock
            .Setup(m => m.GetAllDetailedLicenceValidity(machineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<Dictionary<string, LicenceValidationInfo>>(StatusCodes.Status204NoContent, "No content"))
            .Verifiable(Times.Once);

        // Act
        var rubyLicences = await _licenceService.GetMachineLicenses(machineId, CancellationToken.None);

        // Assert
        rubyLicences.Should().NotBeNull();

        rubyLicences!.HasValidAniloxLicense.Should().BeFalse();
        rubyLicences.ExpiryDateOfAniloxLicense.Should().BeNull();
        rubyLicences.HasValidCheckLicense.Should().BeFalse();
        rubyLicences.ExpiryDateOfCheckLicense.Should().BeNull();
        rubyLicences.HasValidConnect4FlowLicense.Should().BeFalse();
        rubyLicences.ExpiryDateOfConnect4FlowLicense.Should().BeNull();
        rubyLicences.HasValidGoLicense.Should().BeFalse();
        rubyLicences.ExpiryDateOfGoLicense.Should().BeNull();
        rubyLicences.HasValidTrackLicense.Should().BeFalse();
        rubyLicences.ExpiryDateOfTrackLicense.Should().BeNull();

        _licenceManagerCachingServiceMock.VerifyAll();
        _licenceManagerCachingServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetMachineLicenses_GetAllDetailedLicenceValidityReturnsInternalServerError_ThrowsInternalServiceException()
    {
        // Arrange
        const string machineId = "EQ12345";

        _licenceManagerCachingServiceMock
            .Setup(m => m.GetAllDetailedLicenceValidity(machineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<Dictionary<string, LicenceValidationInfo>>(StatusCodes.Status500InternalServerError, "Internal server error"))
            .Verifiable(Times.Once);

        // Act
        var getMachineLicensesAct = () => _licenceService.GetMachineLicenses(machineId, CancellationToken.None);

        // Assert
        await getMachineLicensesAct.Should().ThrowAsync<InternalServiceException>();

        _licenceManagerCachingServiceMock.VerifyAll();
        _licenceManagerCachingServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetMachineLicenses_GetAllDetailedLicenceValidityThrowsExceptipon_ThrowsException()
    {
        // Arrange
        const string machineId = "EQ12345";

        _licenceManagerCachingServiceMock
            .Setup(m => m.GetAllDetailedLicenceValidity(machineId, It.IsAny<CancellationToken>()))
            .Throws<Exception>()
            .Verifiable(Times.Once);

        // Act
        var getMachineLicensesAct = () => _licenceService.GetMachineLicenses(machineId, CancellationToken.None);

        // Assert
        await getMachineLicensesAct.Should().ThrowAsync<Exception>();

        _licenceManagerCachingServiceMock.VerifyAll();
        _licenceManagerCachingServiceMock.VerifyNoOtherCalls();
    }
}