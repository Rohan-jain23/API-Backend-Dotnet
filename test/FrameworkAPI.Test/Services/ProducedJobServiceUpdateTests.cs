using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FrameworkAPI.Exceptions;
using FrameworkAPI.Schema.Misc;
using FrameworkAPI.Schema.ProducedJob;
using FrameworkAPI.Services;
using FrameworkAPI.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using WuH.Ruby.Common.Core;
using WuH.Ruby.KpiDataHandler.Client;
using WuH.Ruby.ProductionPeriodsDataHandler.Client;
using Xunit;

namespace FrameworkAPI.Test.Services;

public class ProducedJobServiceUpdateTests
{
    private const string MachineId1 = "EQ00001";

    private readonly ProducedJobService _subject;
    private readonly Mock<IJobInfoCachingService> _jobInfoCachingServiceMock = new();
    private readonly Mock<IProductionPeriodsDataHandlerHttpClient> _productionPeriodsDataHandlerHttpClientMock;
    private readonly Mock<ILogger<ProducedJobService>> _logger = new();
    private readonly Mock<IKpiEventQueueWrapper> _kpiEventQueueWrapperMock = new();
    private readonly Mock<IMachineService> _machineServiceMock = new();

    public ProducedJobServiceUpdateTests()
    {
        _productionPeriodsDataHandlerHttpClientMock = new Mock<IProductionPeriodsDataHandlerHttpClient>();
        _subject = new ProducedJobService(
            _jobInfoCachingServiceMock.Object,
            _productionPeriodsDataHandlerHttpClientMock.Object,
            _machineServiceMock.Object,
            _logger.Object,
            _kpiEventQueueWrapperMock.Object
        );
    }

    [Fact]
    public async Task UpdateProducedJobMachineTargetSpeed_Returns_Produced_Job_Response()
    {
        // Arrange
        var targetSpeed = 100;
        var associatedJob = "job1";
        var userId = "userId";
        var department = MachineDepartment.PaperSack;
        var family = MachineFamily.PaperSackBottomer;

        _kpiEventQueueWrapperMock
            .Setup(x => x.SendSetTargetSpeedOfJobEventAndWaitForReply(
                It.IsAny<SetTargetSpeedOfJobEventMessage>()))
            .ReturnsAsync(new InternalResponse());
        _machineServiceMock
            .Setup(x => x.GetMachineBusinessUnit(MachineId1, CancellationToken.None))
            .ReturnsAsync(department);
        _machineServiceMock
            .Setup(x => x.GetMachineFamily(MachineId1, CancellationToken.None))
            .ReturnsAsync(family);
        MockProductionPeriodsDataHandlerHttpClientGetsJob(associatedJob, MachineId1);

        // Act
        var response = await _subject.UpdateProducedJobMachineTargetSpeed(
            targetSpeed,
            MachineId1,
            associatedJob,
            userId,
            CancellationToken.None);

        // Assert
        response.Should().BeOfType<PaperSackProducedJob>();
        response.MachineId.Should().Be(MachineId1);
        response.JobId.Should().Be(associatedJob);
    }

    [Fact]
    public async Task UpdateProducedJobMachineTargetSpeed_Throws_Error_If_PPDH_Has_Internal_Server_Error()
    {
        // Arrange
        var targetSpeed = 100;
        var associatedJob = "job1";
        var userId = "userId";
        var department = MachineDepartment.PaperSack;
        var family = MachineFamily.PaperSackBottomer;

        _kpiEventQueueWrapperMock
            .Setup(x => x.SendSetTargetSpeedOfJobEventAndWaitForReply(
                It.IsAny<SetTargetSpeedOfJobEventMessage>()))
            .ReturnsAsync(new InternalResponse());
        _machineServiceMock
            .Setup(x => x.GetMachineBusinessUnit(MachineId1, CancellationToken.None))
            .ReturnsAsync(department);
        _machineServiceMock
            .Setup(x => x.GetMachineFamily(MachineId1, CancellationToken.None))
            .ReturnsAsync(family);
        _productionPeriodsDataHandlerHttpClientMock
            .Setup(m => m.GetJobInfo(
                It.IsAny<CancellationToken>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(new InternalItemResponse<JobInfo>(StatusCodes.Status500InternalServerError, "ErrorMessage"));

        // Act
        var response = async () => await _subject.UpdateProducedJobMachineTargetSpeed(
            targetSpeed,
            MachineId1,
            associatedJob,
            userId,
            CancellationToken.None);

        // Assert
        await response.Should().ThrowAsync<InternalServiceException>();
    }

    [Fact]
    public async Task UpdateProducedJobMachineTargetSpeed_Throws_Error_If_MachineService_Has_Internal_Server_Error()
    {
        // Arrange
        var targetSpeed = 100;
        var associatedJob = "job1";
        var userId = "userId";

        _kpiEventQueueWrapperMock
            .Setup(x => x.SendSetTargetSpeedOfJobEventAndWaitForReply(
                It.IsAny<SetTargetSpeedOfJobEventMessage>()))
            .ReturnsAsync(new InternalResponse());
        _machineServiceMock
            .Setup(x => x.GetMachineBusinessUnit(MachineId1, CancellationToken.None))
            .ThrowsAsync(new InternalServiceException(new InternalError(StatusCodes.Status500InternalServerError, "internal error")));

        // Act
        var response = async () => await _subject.UpdateProducedJobMachineTargetSpeed(
            targetSpeed,
            MachineId1,
            associatedJob,
            userId,
            CancellationToken.None);

        // Assert
        await response.Should().ThrowAsync<InternalServiceException>();
    }

    [Fact]
    public async Task UpdateProducedJobMachineTargetSpeed_Throws_Error_If_KpiEventQueueWrapper_Has_Internal_Server_Error()
    {
        // Arrange
        var targetSpeed = 100;
        var associatedJob = "job1";
        var userId = "userId";

        _kpiEventQueueWrapperMock
            .Setup(x => x.SendSetTargetSpeedOfJobEventAndWaitForReply(
                It.IsAny<SetTargetSpeedOfJobEventMessage>()))
            .ReturnsAsync(new InternalResponse(StatusCodes.Status500InternalServerError, "ErrorMessage"));

        // Act
        var response = async () => await _subject.UpdateProducedJobMachineTargetSpeed(
            targetSpeed,
            MachineId1,
            associatedJob,
            userId,
            CancellationToken.None);

        // Assert
        await response.Should().ThrowAsync<InternalServiceException>();
    }

    [Theory]
    [InlineData(400)]
    [InlineData(204)]
    public async Task UpdateProducedJobMachineTargetSpeed_Throws_Error_When_KpiEventQueueWrapper_Has_400_204_Response(int expectedStatusCode)
    {
        // Arrange
        var targetSpeed = 100;
        var associatedJob = "job1";
        var userId = "userId";

        _kpiEventQueueWrapperMock
            .Setup(x => x.SendSetTargetSpeedOfJobEventAndWaitForReply(
                It.IsAny<SetTargetSpeedOfJobEventMessage>()))
            .ReturnsAsync(new InternalResponse(expectedStatusCode, "ErrorMessage"));

        // Act
        var response = async () => await _subject.UpdateProducedJobMachineTargetSpeed(
            targetSpeed,
            MachineId1,
            associatedJob,
            userId,
            CancellationToken.None);

        // Assert
        await response.Should().ThrowAsync<ParameterInvalidException>();
    }

    [Fact]
    public async Task UpdateProducedJobMachineTargetScrapCountDuringProduction_Returns_Produced_Job_Response()
    {
        // Arrange
        var targetScrapCountDuringProduction = 100;
        var associatedJob = "job1";
        var userId = "userId";
        var department = MachineDepartment.PaperSack;
        var family = MachineFamily.PaperSackBottomer;

        _kpiEventQueueWrapperMock
            .Setup(x => x.SendSetTargetScrapDuringProductionOfJobEventAndWaitForReply(
                It.IsAny<SetTargetScrapDuringProductionOfJobEventMessage>()))
            .ReturnsAsync(new InternalResponse());
        _machineServiceMock
            .Setup(x => x.GetMachineBusinessUnit(MachineId1, CancellationToken.None))
            .ReturnsAsync(department);
        _machineServiceMock
            .Setup(x => x.GetMachineFamily(MachineId1, CancellationToken.None))
            .ReturnsAsync(family);
        MockProductionPeriodsDataHandlerHttpClientGetsJob(associatedJob, MachineId1);

        // Act
        var response = await _subject.UpdateProducedJobTargetScrapCountDuringProduction(
            targetScrapCountDuringProduction,
            MachineId1,
            associatedJob,
            userId,
            CancellationToken.None);

        // Assert
        response.Should().BeOfType<PaperSackProducedJob>();
        response.MachineId.Should().Be(MachineId1);
        response.JobId.Should().Be(associatedJob);
    }

    [Fact]
    public async Task UpdateProducedJobMachineTargetScrapCountDuringProduction_Throws_Error_If_PPDH_Has_Internal_Server_Error()
    {
        // Arrange
        var targetScrapCountDuringProduction = 100;
        var associatedJob = "job1";
        var userId = "userId";
        var department = MachineDepartment.PaperSack;
        var family = MachineFamily.PaperSackBottomer;

        _kpiEventQueueWrapperMock
            .Setup(x => x.SendSetTargetScrapDuringProductionOfJobEventAndWaitForReply(
                It.IsAny<SetTargetScrapDuringProductionOfJobEventMessage>()))
            .ReturnsAsync(new InternalResponse());
        _machineServiceMock
            .Setup(x => x.GetMachineBusinessUnit(MachineId1, CancellationToken.None))
            .ReturnsAsync(department);
        _machineServiceMock
            .Setup(x => x.GetMachineFamily(MachineId1, CancellationToken.None))
            .ReturnsAsync(family);
        _productionPeriodsDataHandlerHttpClientMock
            .Setup(m => m.GetJobInfo(
                It.IsAny<CancellationToken>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(new InternalItemResponse<JobInfo>(StatusCodes.Status500InternalServerError, "ErrorMessage"));

        // Act
        var response = async () => await _subject.UpdateProducedJobTargetScrapCountDuringProduction(
            targetScrapCountDuringProduction,
            MachineId1,
            associatedJob,
            userId,
            CancellationToken.None);

        // Assert
        await response.Should().ThrowAsync<InternalServiceException>();
    }

    [Fact]
    public async Task UpdateProducedJobMachineTargetScrapCountDuringProduction_Throws_Error_If_MachineService_Has_Internal_Server_Error()
    {
        // Arrange
        var targetScrapCountDuringProduction = 100;
        var associatedJob = "job1";
        var userId = "userId";

        _kpiEventQueueWrapperMock
            .Setup(x => x.SendSetTargetScrapDuringProductionOfJobEventAndWaitForReply(
                It.IsAny<SetTargetScrapDuringProductionOfJobEventMessage>()))
            .ReturnsAsync(new InternalResponse());
        _machineServiceMock
            .Setup(x => x.GetMachineBusinessUnit(MachineId1, CancellationToken.None))
            .ThrowsAsync(new InternalServiceException(new InternalError(StatusCodes.Status500InternalServerError, "internal error")));

        // Act
        var response = async () => await _subject.UpdateProducedJobTargetScrapCountDuringProduction(
            targetScrapCountDuringProduction,
            MachineId1,
            associatedJob,
            userId,
            CancellationToken.None);

        // Assert
        await response.Should().ThrowAsync<InternalServiceException>();
    }

    [Fact]
    public async Task UpdateProducedJobMachineTargetScrapCountDuringProduction_Throws_Error_If_KpiEventQueueWrapper_Has_Internal_Server_Error()
    {
        // Arrange
        var targetScrapCountDuringProduction = 100;
        var associatedJob = "job1";
        var userId = "userId";

        _kpiEventQueueWrapperMock
            .Setup(x => x.SendSetTargetScrapDuringProductionOfJobEventAndWaitForReply(
                It.IsAny<SetTargetScrapDuringProductionOfJobEventMessage>()))
            .ReturnsAsync(new InternalResponse(StatusCodes.Status500InternalServerError, "ErrorMessage"));

        // Act
        var response = async () => await _subject.UpdateProducedJobTargetScrapCountDuringProduction(
            targetScrapCountDuringProduction,
            MachineId1,
            associatedJob,
            userId,
            CancellationToken.None);

        // Assert
        await response.Should().ThrowAsync<InternalServiceException>();
    }

    [Theory]
    [InlineData(400)]
    [InlineData(204)]
    public async Task UpdateProducedJobMachineTargetScrapCountDuringProduction_Throws_Error_When_KpiEventQueueWrapper_Has_400_204_Response(int expectedStatusCode)
    {
        // Arrange
        var targetScrapCountDuringProduction = 100;
        var associatedJob = "job1";
        var userId = "userId";

        _kpiEventQueueWrapperMock
            .Setup(x => x.SendSetTargetScrapDuringProductionOfJobEventAndWaitForReply(
                It.IsAny<SetTargetScrapDuringProductionOfJobEventMessage>()))
            .ReturnsAsync(new InternalResponse(expectedStatusCode, "ErrorMessage"));

        // Act
        var response = async () => await _subject.UpdateProducedJobTargetScrapCountDuringProduction(
            targetScrapCountDuringProduction,
            MachineId1,
            associatedJob,
            userId,
            CancellationToken.None);

        // Assert
        await response.Should().ThrowAsync<ParameterInvalidException>();
    }

    [Fact]
    public async Task UpdateProducedJobMachineTargetDownTimeInMin_Returns_Produced_Job_Response()
    {
        // Arrange
        var targetDownTimeInMin = 100;
        var associatedJob = "job1";
        var userId = "userId";
        var department = MachineDepartment.PaperSack;
        var family = MachineFamily.PaperSackBottomer;

        _kpiEventQueueWrapperMock
            .Setup(x => x.SendSetTargetDownTimeOfJobEventAndWaitForReply(
                It.IsAny<SetTargetDownTimeOfJobEventMessage>()))
            .ReturnsAsync(new InternalResponse());
        _machineServiceMock
            .Setup(x => x.GetMachineBusinessUnit(MachineId1, CancellationToken.None))
            .ReturnsAsync(department);
        _machineServiceMock
            .Setup(x => x.GetMachineFamily(MachineId1, CancellationToken.None))
            .ReturnsAsync(family);
        MockProductionPeriodsDataHandlerHttpClientGetsJob(associatedJob, MachineId1);

        // Act
        var response = await _subject.UpdateProducedJobTargetDownTimeInMin(
            targetDownTimeInMin,
            MachineId1,
            associatedJob,
            userId,
            CancellationToken.None);

        // Assert
        response.Should().BeOfType<PaperSackProducedJob>();
        response.MachineId.Should().Be(MachineId1);
        response.JobId.Should().Be(associatedJob);
    }

    [Fact]
    public async Task UpdateProducedJobMachineTargetDownTimeInMin_Throws_Error_If_PPDH_Has_Internal_Server_Error()
    {
        // Arrange
        var targetDownTimeInMin = 100;
        var associatedJob = "job1";
        var userId = "userId";
        var department = MachineDepartment.PaperSack;
        var family = MachineFamily.PaperSackBottomer;

        _kpiEventQueueWrapperMock
            .Setup(x => x.SendSetTargetDownTimeOfJobEventAndWaitForReply(
                It.IsAny<SetTargetDownTimeOfJobEventMessage>()))
            .ReturnsAsync(new InternalResponse());
        _machineServiceMock
            .Setup(x => x.GetMachineBusinessUnit(MachineId1, CancellationToken.None))
            .ReturnsAsync(department);
        _machineServiceMock
            .Setup(x => x.GetMachineFamily(MachineId1, CancellationToken.None))
            .ReturnsAsync(family);
        _productionPeriodsDataHandlerHttpClientMock
            .Setup(m => m.GetJobInfo(
                It.IsAny<CancellationToken>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(new InternalItemResponse<JobInfo>(StatusCodes.Status500InternalServerError, "ErrorMessage"));

        // Act
        var response = async () => await _subject.UpdateProducedJobTargetDownTimeInMin(
            targetDownTimeInMin,
            MachineId1,
            associatedJob,
            userId,
            CancellationToken.None);

        // Assert
        await response.Should().ThrowAsync<InternalServiceException>();
    }

    [Fact]
    public async Task UpdateProducedJobMachineTargetDownTimeInMin_Throws_Error_If_MachineService_Has_Internal_Server_Error()
    {
        // Arrange
        var targetDownTimeInMin = 100;
        var associatedJob = "job1";
        var userId = "userId";

        _kpiEventQueueWrapperMock
            .Setup(x => x.SendSetTargetDownTimeOfJobEventAndWaitForReply(
                It.IsAny<SetTargetDownTimeOfJobEventMessage>()))
            .ReturnsAsync(new InternalResponse());
        _machineServiceMock
            .Setup(x => x.GetMachineBusinessUnit(MachineId1, CancellationToken.None))
            .ThrowsAsync(new InternalServiceException(new InternalError(StatusCodes.Status500InternalServerError, "internal error")));

        // Act
        var response = async () => await _subject.UpdateProducedJobTargetDownTimeInMin(
            targetDownTimeInMin,
            MachineId1,
            associatedJob,
            userId,
            CancellationToken.None);

        // Assert
        await response.Should().ThrowAsync<InternalServiceException>();
    }

    [Fact]
    public async Task UpdateProducedJobMachineTargetDownTimeInMin_Throws_Error_If_KpiEventQueueWrapper_Has_Internal_Server_Error()
    {
        // Arrange
        var targetDownTimeInMin = 100;
        var associatedJob = "job1";
        var userId = "userId";

        _kpiEventQueueWrapperMock
            .Setup(x => x.SendSetTargetDownTimeOfJobEventAndWaitForReply(
                It.IsAny<SetTargetDownTimeOfJobEventMessage>()))
            .ReturnsAsync(new InternalResponse(StatusCodes.Status500InternalServerError, "ErrorMessage"));

        // Act
        var response = async () => await _subject.UpdateProducedJobTargetDownTimeInMin(
            targetDownTimeInMin,
            MachineId1,
            associatedJob,
            userId,
            CancellationToken.None);

        // Assert
        await response.Should().ThrowAsync<InternalServiceException>();
    }

    [Theory]
    [InlineData(400)]
    [InlineData(204)]
    public async Task UpdateProducedJobMachineTargetDownTimeInMin_Throws_Error_When_KpiEventQueueWrapper_Has_400_204_Response(int expectedStatusCode)
    {
        // Arrange
        var targetDownTimeInMin = 100;
        var associatedJob = "job1";
        var userId = "userId";

        _kpiEventQueueWrapperMock
            .Setup(x => x.SendSetTargetDownTimeOfJobEventAndWaitForReply(
                It.IsAny<SetTargetDownTimeOfJobEventMessage>()))
            .ReturnsAsync(new InternalResponse(expectedStatusCode, "ErrorMessage"));

        // Act
        var response = async () => await _subject.UpdateProducedJobTargetDownTimeInMin(
            targetDownTimeInMin,
            MachineId1,
            associatedJob,
            userId,
            CancellationToken.None);

        // Assert
        await response.Should().ThrowAsync<ParameterInvalidException>();
    }

    [Fact]
    public async Task UpdateProducedJobTargetSetupTimeInMin_Returns_Produced_Job_Response()
    {
        // Arrange
        var targetSetupTimeInMin = 100;
        var associatedJob = "job1";
        var userId = "userId";
        var department = MachineDepartment.PaperSack;
        var family = MachineFamily.PaperSackBottomer;

        _kpiEventQueueWrapperMock
            .Setup(x => x.SendSetTargetSetupTimeOfJobEventAndWaitForReply(
                It.IsAny<SetTargetSetupTimeOfJobEventMessage>()))
            .ReturnsAsync(new InternalResponse());
        _machineServiceMock
            .Setup(x => x.GetMachineBusinessUnit(MachineId1, CancellationToken.None))
            .ReturnsAsync(department);
        _machineServiceMock
            .Setup(x => x.GetMachineFamily(MachineId1, CancellationToken.None))
            .ReturnsAsync(family);
        MockProductionPeriodsDataHandlerHttpClientGetsJob(associatedJob, MachineId1);

        // Act
        var response = await _subject.UpdateProducedJobTargetSetupTimeInMin(
            targetSetupTimeInMin,
            MachineId1,
            associatedJob,
            userId,
            CancellationToken.None);

        // Assert
        response.Should().BeOfType<PaperSackProducedJob>();
        response.MachineId.Should().Be(MachineId1);
        response.JobId.Should().Be(associatedJob);
    }

    [Fact]
    public async Task UpdateProducedJobTargetSetupTimeInMin_Throws_Error_If_PPDH_Has_Internal_Server_Error()
    {
        // Arrange
        var targetSetupTimeInMin = 100;
        var associatedJob = "job1";
        var userId = "userId";
        var department = MachineDepartment.PaperSack;
        var family = MachineFamily.PaperSackBottomer;

        _kpiEventQueueWrapperMock
            .Setup(x => x.SendSetTargetSetupTimeOfJobEventAndWaitForReply(
                It.IsAny<SetTargetSetupTimeOfJobEventMessage>()))
            .ReturnsAsync(new InternalResponse());
        _machineServiceMock
            .Setup(x => x.GetMachineBusinessUnit(MachineId1, CancellationToken.None))
            .ReturnsAsync(department);
        _machineServiceMock
            .Setup(x => x.GetMachineFamily(MachineId1, CancellationToken.None))
            .ReturnsAsync(family);
        _productionPeriodsDataHandlerHttpClientMock
            .Setup(m => m.GetJobInfo(
                It.IsAny<CancellationToken>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(new InternalItemResponse<JobInfo>(StatusCodes.Status500InternalServerError, "ErrorMessage"));

        // Act
        var response = async () => await _subject.UpdateProducedJobTargetSetupTimeInMin(
            targetSetupTimeInMin,
            MachineId1,
            associatedJob,
            userId,
            CancellationToken.None);

        // Assert
        await response.Should().ThrowAsync<InternalServiceException>();
    }

    [Fact]
    public async Task UpdateProducedJobTargetSetupTimeInMin_Throws_Error_If_MachineService_Has_Internal_Server_Error_()
    {
        // Arrange
        var targetSetupTimeInMin = 100;
        var associatedJob = "job1";
        var userId = "userId";

        _kpiEventQueueWrapperMock
            .Setup(x => x.SendSetTargetSetupTimeOfJobEventAndWaitForReply(
                It.IsAny<SetTargetSetupTimeOfJobEventMessage>()))
            .ReturnsAsync(new InternalResponse());
        _machineServiceMock
            .Setup(x => x.GetMachineBusinessUnit(MachineId1, CancellationToken.None))
            .ThrowsAsync(new InternalServiceException(new InternalError(StatusCodes.Status500InternalServerError, "Internal error")));

        // Act
        var response = async () => await _subject.UpdateProducedJobTargetSetupTimeInMin(
            targetSetupTimeInMin,
            MachineId1,
            associatedJob,
            userId,
            CancellationToken.None);

        // Assert
        await response.Should().ThrowAsync<InternalServiceException>();
    }

    [Fact]
    public async Task UpdateProducedJobTargetSetupTimeInMin_Throws_Error_If_KpiEventQueueWrapper_Has_Internal_Server_Error()
    {
        // Arrange
        var targetSetupTimeInMin = 100;
        var associatedJob = "job1";
        var userId = "userId";

        _kpiEventQueueWrapperMock
            .Setup(x => x.SendSetTargetSetupTimeOfJobEventAndWaitForReply(
                It.IsAny<SetTargetSetupTimeOfJobEventMessage>()))
            .ReturnsAsync(new InternalResponse(StatusCodes.Status500InternalServerError, "ErrorMessage"));

        // Act
        var response = async () => await _subject.UpdateProducedJobTargetSetupTimeInMin(
            targetSetupTimeInMin,
            MachineId1,
            associatedJob,
            userId,
            CancellationToken.None);

        // Assert
        await response.Should().ThrowAsync<InternalServiceException>();
    }

    [Theory]
    [InlineData(400)]
    [InlineData(204)]
    public async Task UpdateProducedJobTargetSetupTimeInMin_Throws_Error_When_KpiEventQueueWrapper_Has_400_204_Response(int expectedStatusCode)
    {
        // Arrange
        var targetSetupTimeInMin = 100;
        var associatedJob = "job1";
        var userId = "userId";

        _kpiEventQueueWrapperMock
            .Setup(x => x.SendSetTargetSetupTimeOfJobEventAndWaitForReply(
                It.IsAny<SetTargetSetupTimeOfJobEventMessage>()))
            .ReturnsAsync(new InternalResponse(expectedStatusCode, "ErrorMessage"));

        // Act
        var response = async () => await _subject.UpdateProducedJobTargetSetupTimeInMin(
            targetSetupTimeInMin,
            MachineId1,
            associatedJob,
            userId,
            CancellationToken.None);

        // Assert
        await response.Should().ThrowAsync<ParameterInvalidException>();
    }

    private void MockProductionPeriodsDataHandlerHttpClientGetsJob(string jobId, string machineId)
    {
        var jobInfo = new JobInfo
        {
            JobId = jobId,
            MachineId = machineId
        };

        _productionPeriodsDataHandlerHttpClientMock
            .Setup(m => m.GetJobInfo(
                It.IsAny<CancellationToken>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(new InternalItemResponse<JobInfo>(jobInfo));
    }

}