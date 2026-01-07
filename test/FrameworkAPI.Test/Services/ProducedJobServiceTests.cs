using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FrameworkAPI.Exceptions;
using FrameworkAPI.Models;
using FrameworkAPI.Schema.Misc;
using FrameworkAPI.Schema.ProducedJob;
using FrameworkAPI.Services;
using FrameworkAPI.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using WuH.Ruby.Common.Core;
using WuH.Ruby.KpiDataHandler.Client;
using WuH.Ruby.ProductionPeriodsDataHandler.Client;
using Xunit;

namespace FrameworkAPI.Test.Services;

public class ProducedJobServiceTests
{
    private const string MachineId1 = "EQ00001";
    private const string MachineId2 = "EQ00002";
    private readonly DateTime _timestamp = DateTime.UnixEpoch;

    private readonly ProducedJobService _subject;
    private readonly Mock<IJobInfoCachingService> _jobInfoCachingServiceMock = new();
    private readonly Mock<IProductionPeriodsDataHandlerHttpClient> _productionPeriodsDataHandlerHttpClientMock;
    private readonly Mock<ILogger<ProducedJobService>> _logger = new();
    private readonly Mock<IKpiEventQueueWrapper> _kpiEventQueueWrapperMock = new();
    private readonly Mock<IMachineService> _machineServiceMock = new();

    public ProducedJobServiceTests()
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
    public async Task GetProducedJob_For_Active_Job_Returns_Correct_Value()
    {
        // Arrange
        var jobInfo1 = new JobInfo
        {
            JobId = "job1_id",
            MachineId = MachineId1
        };

        var jobInfo2 = new JobInfo
        {
            JobId = "job2_id",
            MachineId = MachineId2
        };

        MockJobInfoCachingServiceGet(jobInfo: jobInfo1);

        MockProductionPeriodsDataHandlerHttpClientGetLatestJobs(
            new InternalListResponse<JobInfo?>([jobInfo1, jobInfo2]));

        // Act
        var result = await _subject.GetProducedJob(
            MachineId1,
            MachineDepartment.Extrusion,
            MachineFamily.BlowFilm,
            null,
            CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(
            new ExtrusionProducedJob(jobInfo1, machineQueryTimestamp: null)
            {
                IsActive = true,
                JobId = "job1_id",
                MachineId = "EQ00001",
                StartTime = DateTime.MinValue,
                UniqueId = "EQ00001_job1_id"
            });
    }

    [Fact]
    public async Task GetProducedJob_For_Active_Job_Returns_An_Null_If_Internal_Response_Has_StatusCode_204()
    {
        // Arrange
        MockJobInfoCachingServiceGet(jobInfo: null);

        // Act
        var result = await _subject.GetProducedJob(
            MachineId1,
            MachineDepartment.Extrusion,
            MachineFamily.BlowFilm,
            null,
            CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetProducedJob_For_Active_Job_Returns_An_Null_If_JobInfo_EndTime_Is_Not_Null()
    {
        // Arrange
        var jobInfo = new JobInfo
        {
            JobId = "job1_id",
            MachineId = MachineId1,
            EndTime = DateTime.MaxValue
        };

        MockJobInfoCachingServiceGet(jobInfo: jobInfo);

        // Act
        var result = await _subject.GetProducedJob(
            MachineId1,
            MachineDepartment.Extrusion,
            MachineFamily.BlowFilm,
            null,
            CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetProducedJob_For_Active_Job_Returns_An_Erroneous_Internal_Response_If_GetLatestJobs_Fails()
    {
        // Arrange
        MockProductionPeriodsDataHandlerHttpClientGetLatestJobs(
            new InternalListResponse<JobInfo?>((int)HttpStatusCode.InternalServerError, "ErrorMessage"));

        MockJobInfoCachingServiceGet(
            exception: new InternalServiceException("ErrorMessage", (int)HttpStatusCode.InternalServerError));

        // Act
        var getLatestProducedJobAction = async () => await _subject.GetProducedJob(
            MachineId1,
            MachineDepartment.Extrusion,
            MachineFamily.BlowFilm,
            null,
            CancellationToken.None);

        // Assert
        await getLatestProducedJobAction.Should().ThrowAsync<InternalServiceException>();
    }

    [Fact]
    public async Task GetProducedJob_For_Historic_Job_Returns_Correct_Value()
    {
        // Arrange
        var jobInfo1 = new JobInfo
        {
            JobId = "job1_id",
            MachineId = MachineId1
        };

        var jobInfo2 = new JobInfo
        {
            JobId = "job2_id",
            MachineId = MachineId1
        };

        MockProductionPeriodsDataHandlerHttpClientGetAllJobInfosSingleMachineId(
            new InternalListResponse<JobInfo?>([jobInfo1, jobInfo2]));

        // Act
        var result = await _subject.GetProducedJob(
            MachineId1,
            MachineDepartment.Extrusion,
            MachineFamily.BlowFilm,
            _timestamp,
            CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(
            new ExtrusionProducedJob(jobInfo1, machineQueryTimestamp: null)
            {
                IsActive = true,
                JobId = "job1_id",
                MachineId = "EQ00001",
                StartTime = DateTime.MinValue,
                UniqueId = "EQ00001_job1_id"
            });
    }

    [Fact]
    public async Task GetProducedJob_For_Historic_Job_Propagates_Error()
    {
        //Arrange
        MockProductionPeriodsDataHandlerHttpClientGetAllJobInfosSingleMachineId(new InternalListResponse<JobInfo?>((int)HttpStatusCode.InternalServerError, "ErrorMessage"));

        // Act
        var getProducedJobAction = async () => await _subject.GetProducedJob(
            MachineId1,
            MachineDepartment.Extrusion,
            MachineFamily.BlowFilm,
            _timestamp,
            CancellationToken.None);

        // Assert
        await getProducedJobAction.Should().ThrowAsync<InternalServiceException>();
    }

    [Fact]
    public async Task GetProducedJob_Returns_Correct_Value()
    {
        // Arrange
        var jobInfo1 = new JobInfo
        {
            JobId = "job1_id",
            MachineId = MachineId1
        };

        _productionPeriodsDataHandlerHttpClientMock
            .Setup(m => m.GetJobInfo(
                It.IsAny<CancellationToken>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(new InternalItemResponse<JobInfo>(jobInfo1));

        // Act
        var result = await _subject.GetProducedJob(
            MachineId1,
            jobInfo1.JobId,
            MachineDepartment.Extrusion,
            MachineFamily.BlowFilm,
            CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(
            new ExtrusionProducedJob(jobInfo1, machineQueryTimestamp: null)
            {
                IsActive = true,
                JobId = "job1_id",
                MachineId = "EQ00001",
                StartTime = DateTime.MinValue,
                UniqueId = "EQ00001_job1_id"
            });
    }

    [Fact]
    public async Task GetProducedJob_Throws_InternalServiceHasErrorException_If_GetJobInfo_Returns_An_Error()
    {
        // Arrange
        _productionPeriodsDataHandlerHttpClientMock
            .Setup(m => m.GetJobInfo(
                It.IsAny<CancellationToken>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(new InternalItemResponse<JobInfo>((int)HttpStatusCode.InternalServerError, "ErrorMessage"));

        // Act / Assert
        var getProducedJobAction = async () => await _subject.GetProducedJob(
            MachineId1, "job1_id", MachineDepartment.Extrusion, MachineFamily.BlowFilm, CancellationToken.None);

        await getProducedJobAction.Should().ThrowAsync<InternalServiceException>();
    }

    [Fact]
    public async Task GetLatestProducedJobs_Returns_Correct_Value()
    {
        // Arrange
        var jobInfo1 = new JobInfo
        {
            JobId = "job1_id",
            MachineId = MachineId1
        };

        var jobInfo2 = new JobInfo
        {
            JobId = "job2_id",
            MachineId = MachineId2
        };

        MockProductionPeriodsDataHandlerHttpClientGetAllJobInfos(new InternalListResponse<JobInfo?>([jobInfo1, jobInfo2]));

        // Act
        var result = await _subject.GetLatestProducedJobs(
            [MachineId1],
            null,
            null,
            null,
            0,
            5,
            CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(
            new List<ExtrusionProducedJob>
            {
                new(jobInfo1, machineQueryTimestamp: null)
                {
                    IsActive = true,
                    JobId = "job1_id",
                    MachineId = "EQ00001",
                    ProductDefinition = null,
                    StartTime = DateTime.MinValue,
                    UniqueId = "EQ00001_job1_id"
                },
                new(jobInfo1, machineQueryTimestamp: null)
                {
                    IsActive = true,
                    JobId = "job2_id",
                    MachineId = MachineId2,
                    StartTime = DateTime.MinValue,
                    UniqueId = "EQ00002_job2_id"
                }
            });
    }

    [Fact]
    public async Task GetLatestProducedJobs_Returns_Correct_Value_With_Optional_Parameters()
    {
        // Arrange
        var jobInfo1 = new JobInfo
        {
            JobId = "job1_id",
            MachineId = MachineId1
        };

        var jobInfo2 = new JobInfo
        {
            JobId = "job2_id",
            MachineId = MachineId2
        };

        MockProductionPeriodsDataHandlerHttpClientGetAllJobInfos(
            new InternalListResponse<JobInfo?>([jobInfo1, jobInfo2]));

        // Act
        var result = await _subject.GetLatestProducedJobs(
            [MachineId1],
            DateTime.MinValue,
            DateTime.MaxValue,
            "job*",
            0,
            5,
            CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(
            new List<ExtrusionProducedJob>
            {
                new(jobInfo1, machineQueryTimestamp: null)
                {
                    IsActive = true,
                    JobId = "job1_id",
                    MachineId = "EQ00001",
                    ProductDefinition = null,
                    StartTime = DateTime.MinValue,
                    UniqueId = "EQ00001_job1_id"
                },
                new(jobInfo1, machineQueryTimestamp: null)
                {
                    IsActive = true,
                    JobId = "job2_id",
                    MachineId = MachineId2,
                    StartTime = DateTime.MinValue,
                    UniqueId = "EQ00002_job2_id"
                }
            });
    }

    [Fact]
    public async Task
        GetLatestProducedJobs_Throws_An_InternalServiceHasErrorException_If_GetLatestJobs_Returns_An_Erroneous_Response()
    {
        // Arrange
        MockProductionPeriodsDataHandlerHttpClientGetAllJobInfos(
            new InternalListResponse<JobInfo?>((int)HttpStatusCode.InternalServerError, "ErrorMessage"));

        // Act
        var getLatestProducedJobsAction = async () =>
            await _subject.GetLatestProducedJobs([MachineId1], null, null, null, 0, 5, CancellationToken.None);

        // Assert
        await getLatestProducedJobsAction.Should().ThrowAsync<InternalServiceException>();
    }

    [Fact]
    public async Task GetLatestProducedJobsTotalCount_Returns_Correct_Value()
    {
        // Arrange
        _productionPeriodsDataHandlerHttpClientMock
            .Setup(m => m.GetAllJobIds(
                It.IsAny<CancellationToken>(),
                It.IsAny<List<string>>(),
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                It.IsAny<string>()))
            .ReturnsAsync(new InternalListResponse<string>(["j0", "j1", "j2"]));

        // Act
        var result = await _subject.GetLatestProducedJobsTotalCount(
            [MachineId1],
            null,
            DateTime.MinValue,
            DateTime.MaxValue,
            CancellationToken.None);

        // Assert
        result.Should().Be(3);
    }

    [Fact]
    public async Task
        GetLatestProducedJobsTotalCount_Throws_InternalServiceHasErrorException_If_GetAllJobIds_Returns_An_Error()
    {
        // Arrange
        _productionPeriodsDataHandlerHttpClientMock
            .Setup(m => m.GetAllJobIds(
                It.IsAny<CancellationToken>(),
                It.IsAny<List<string>>(),
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                It.IsAny<string>()))
            .ReturnsAsync(new InternalListResponse<string>((int)HttpStatusCode.InternalServerError, "ErrorMessage"));

        // Act / Assert
        var getLatestProducedJobsTotalCountAction = async () => await _subject.GetLatestProducedJobsTotalCount(
            [MachineId1],
            null,
            DateTime.MinValue,
            DateTime.MaxValue,
            CancellationToken.None);

        await getLatestProducedJobsTotalCountAction.Should().ThrowAsync<InternalServiceException>();
    }

    private void MockProductionPeriodsDataHandlerHttpClientGetLatestJobs(InternalListResponse<JobInfo?> response)
    {
        _productionPeriodsDataHandlerHttpClientMock
            .Setup(m => m.GetLatestJobs(
                It.IsAny<CancellationToken>(),
                It.IsAny<List<string>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>()))
            .ReturnsAsync(response);
    }

    private void MockProductionPeriodsDataHandlerHttpClientGetAllJobInfos(InternalListResponse<JobInfo?> response)
    {
        _productionPeriodsDataHandlerHttpClientMock
            .Setup(m => m.GetAllJobInfos(
                It.IsAny<CancellationToken>(),
                It.IsAny<List<string>>(),
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<string?>(),
                It.IsAny<bool?>()))
            .ReturnsAsync(response);
    }

    private void MockProductionPeriodsDataHandlerHttpClientGetAllJobInfosSingleMachineId(InternalListResponse<JobInfo?> response)
    {
        _productionPeriodsDataHandlerHttpClientMock.Setup(mock => mock.GetAllJobInfos(
                It.IsAny<CancellationToken>(),
                MachineId1,
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                null,
                It.IsAny<bool?>()))
            .ReturnsAsync(response);
    }

    private void MockJobInfoCachingServiceGet(JobInfo? jobInfo = null, Exception? exception = null)
    {
        _jobInfoCachingServiceMock
            .Setup(m => m.GetLatest(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DataResult<JobInfo?>(jobInfo, exception));
    }
}