using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FrameworkAPI.Exceptions;
using FrameworkAPI.Models;
using FrameworkAPI.Services;
using FrameworkAPI.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using RabbitMQ.Client;
using WuH.Ruby.Common.Core;
using WuH.Ruby.Common.Queue;
using WuH.Ruby.ProductionPeriodsDataHandler.Client;
using Xunit;

namespace FrameworkAPI.Test.Services;

public class JobInfoCachingServiceTests : IDisposable
{
    private const string MachineId = "EQ00001";

    private readonly JobInfoCachingService _jobInfoCachingService;
    private readonly Mock<IMachineTimeService> _machineTimeServiceMock = new();
    private readonly Mock<IProductionPeriodsDataHandlerHttpClient> _productionPeriodsDataHandlerHttpClientMock = new();
    private readonly Mock<IProductionPeriodChangesQueueWrapper> _productionPeriodChangesQueueWrapperMock = new();

    public JobInfoCachingServiceTests()
    {
        _jobInfoCachingService = new JobInfoCachingService(
            _machineTimeServiceMock.Object,
            _productionPeriodsDataHandlerHttpClientMock.Object,
            _productionPeriodChangesQueueWrapperMock.Object,
            new Mock<ILogger<JobInfoCachingService>>().Object);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _machineTimeServiceMock.VerifyNoOtherCalls();
        _productionPeriodsDataHandlerHttpClientMock.VerifyNoOtherCalls();
        _productionPeriodChangesQueueWrapperMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetLatest_Returns_DataResult_With_JobInfo_And_Replaced_TimeRange_From_Client_And_Cache_On_Second_Get()
    {
        // Arrange
        var machineTime = DateTime.UtcNow;
        var end = machineTime.AddMinutes(-1);
        var start = end.AddMinutes(-10);

        var jobInfo = new JobInfo
        {
            MachineId = MachineId,
            ProductId = "FakeProductId",
            JobId = "FakeJobId",
            StartTime = start,
            TimeRanges = new List<TimeRange>
            {
                new(start, end),
                new(start.AddMinutes(-20), end.AddMinutes(-15))
            }
        };
        var expectedJobInfo = new JobInfo
        {
            MachineId = MachineId,
            ProductId = "FakeProductId",
            JobId = "FakeJobId",
            StartTime = start,
            TimeRanges = new List<TimeRange>
            {
                new(start, machineTime),
                new(start.AddMinutes(-20), end.AddMinutes(-15))
            }
        };

        var response = new InternalListResponse<JobInfo>(new List<JobInfo> { jobInfo });
        MockProductionPeriodsDataHandlerHttpClientGetLatestJobs(response);

        MockQueueWrapperSubscribeForPeriodChanges();

        MockMachineTimeServiceGet(machineTime, times: 2);

        // Act
        var (firstResponseJobInfo, firstResponseException) =
            await _jobInfoCachingService.GetLatest(MachineId, CancellationToken.None);
        var (secondResponseJobInfo, secondResponseException) =
            await _jobInfoCachingService.GetLatest(MachineId, CancellationToken.None);

        // Assert
        firstResponseJobInfo.Should().BeEquivalentTo(expectedJobInfo);
        firstResponseException.Should().BeNull();

        secondResponseJobInfo.Should().BeEquivalentTo(firstResponseJobInfo);
        secondResponseException.Should().BeNull();

        _productionPeriodsDataHandlerHttpClientMock.VerifyAll();
        _productionPeriodChangesQueueWrapperMock.VerifyAll();
        _machineTimeServiceMock.VerifyAll();
    }

    [Fact]
    public async Task GetLatest_Returns_DataResult_With_Exception_If_Client_Request_Has_Error_Code_500()
    {
        // Arrange
        var response = new InternalListResponse<JobInfo>(500, "Internal error");
        MockProductionPeriodsDataHandlerHttpClientGetLatestJobs(response);

        MockQueueWrapperSubscribeForPeriodChanges();

        // Act
        var (jobInfo, exception) = await _jobInfoCachingService.GetLatest(MachineId, CancellationToken.None);

        // Assert
        jobInfo.Should().BeNull();
        exception.Should().NotBeNull();
        exception.Should().BeOfType<InternalServiceException>();

        _productionPeriodsDataHandlerHttpClientMock.VerifyAll();
        _productionPeriodChangesQueueWrapperMock.VerifyAll();
    }

    [Fact]
    public async Task GetLatest_Returns_DataResult_With_Null_As_JobInfo_If_Client_Request_Has_Error_Code_204()
    {
        // Arrange
        var response = new InternalListResponse<JobInfo>(204, "No content");
        MockProductionPeriodsDataHandlerHttpClientGetLatestJobs(response);

        MockQueueWrapperSubscribeForPeriodChanges();

        // Act
        var (jobInfo, exception) = await _jobInfoCachingService.GetLatest(MachineId, CancellationToken.None);

        // Assert
        jobInfo.Should().BeNull();
        exception.Should().BeNull();

        _productionPeriodsDataHandlerHttpClientMock.VerifyAll();
        _productionPeriodChangesQueueWrapperMock.VerifyAll();
    }

    [Fact]
    public async Task GetLatest_Returns_DataResult_From_Cache_If_Initial_Client_Request_Has_Error_Code_204()
    {
        // Arrange
        var response = new InternalListResponse<JobInfo>(204, "No content");
        MockProductionPeriodsDataHandlerHttpClientGetLatestJobs(response, times: 1);

        MockQueueWrapperSubscribeForPeriodChanges();

        // Act
        await _jobInfoCachingService.GetLatest(MachineId, CancellationToken.None);
        var (jobInfo, exception) = await _jobInfoCachingService.GetLatest(MachineId, CancellationToken.None);

        // Assert
        jobInfo.Should().BeNull();
        exception.Should().BeNull();

        _productionPeriodsDataHandlerHttpClientMock.VerifyAll();
        _productionPeriodChangesQueueWrapperMock.VerifyAll();
    }

    [Fact]
    public async Task GetLatest_Returns_DataResult_With_JobInfo_And_TimeRange_If_MachineTime_Service_Returns_Null()
    {
        // Arrange
        var machineTime = DateTime.UtcNow;
        var end = machineTime.AddMinutes(-1);
        var start = end.AddMinutes(-10);

        var jobInfo = new JobInfo
        {
            MachineId = MachineId,
            ProductId = "FakeProductId",
            JobId = "FakeJobId",
            StartTime = start,
            TimeRanges = new List<TimeRange>
            {
                new(start, end)
            }
        };

        var response = new InternalListResponse<JobInfo>(new List<JobInfo> { jobInfo });
        MockProductionPeriodsDataHandlerHttpClientGetLatestJobs(response);

        MockQueueWrapperSubscribeForPeriodChanges();

        MockMachineTimeServiceGet(null);

        // Act
        var (firstResponseJobInfo, firstResponseException) =
            await _jobInfoCachingService.GetLatest(MachineId, CancellationToken.None);

        // Assert
        firstResponseJobInfo.Should().BeEquivalentTo(jobInfo);
        firstResponseException.Should().BeNull();

        _productionPeriodsDataHandlerHttpClientMock.VerifyAll();
        _productionPeriodChangesQueueWrapperMock.VerifyAll();
        _machineTimeServiceMock.VerifyAll();
    }

    [Fact]
    public async Task GetLatest_Returns_DataResult_With_JobInfo_And_TimeRange_If_MachineTime_Service_Returns_Exception()
    {
        // Arrange
        var machineTime = DateTime.UtcNow;
        var end = machineTime.AddMinutes(-1);
        var start = end.AddMinutes(-10);

        var jobInfo = new JobInfo
        {
            MachineId = MachineId,
            ProductId = "FakeProductId",
            JobId = "FakeJobId",
            StartTime = start,
            TimeRanges = new List<TimeRange>
            {
                new(start, end)
            }
        };

        var response = new InternalListResponse<JobInfo>(new List<JobInfo> { jobInfo });
        MockProductionPeriodsDataHandlerHttpClientGetLatestJobs(response);

        MockQueueWrapperSubscribeForPeriodChanges();

        _machineTimeServiceMock
            .Setup(m => m.Get(MachineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DataResult<DateTime?>(null, exception: new InternalServiceException()))
            .Verifiable(Times.Once);

        // Act
        var (responseJobInfo, responseException) =
            await _jobInfoCachingService.GetLatest(MachineId, CancellationToken.None);

        // Assert
        responseJobInfo.Should().BeEquivalentTo(jobInfo);
        responseException.Should().BeNull();

        _productionPeriodsDataHandlerHttpClientMock.VerifyAll();
        _productionPeriodChangesQueueWrapperMock.VerifyAll();
        _machineTimeServiceMock.VerifyAll();
    }

    [Fact]
    public async Task GetLatest_Returns_DataResult_With_JobInfo_If_TimeRange_Is_Empty()
    {
        // Arrange
        var jobInfo = new JobInfo
        {
            MachineId = MachineId,
            ProductId = "FakeProductId",
            JobId = "FakeJobId",
            StartTime = DateTime.UtcNow,
            TimeRanges = new List<TimeRange>()
        };

        var response = new InternalListResponse<JobInfo>(new List<JobInfo> { jobInfo });
        MockProductionPeriodsDataHandlerHttpClientGetLatestJobs(response);

        MockQueueWrapperSubscribeForPeriodChanges();

        // Act
        var (responseJobInfo, responseException) =
            await _jobInfoCachingService.GetLatest(MachineId, CancellationToken.None);

        // Assert
        responseJobInfo.Should().BeEquivalentTo(jobInfo);
        responseException.Should().BeNull();

        _productionPeriodsDataHandlerHttpClientMock.VerifyAll();
        _productionPeriodChangesQueueWrapperMock.VerifyAll();
        _machineTimeServiceMock.VerifyAll();
    }

    [Fact]
    public async Task GetLatest_Returns_DataResult_With_JobInfo_If_TimeRange_Is_Null()
    {
        // Arrange
        var jobInfo = new JobInfo
        {
            MachineId = MachineId,
            ProductId = "FakeProductId",
            JobId = "FakeJobId",
            StartTime = DateTime.UtcNow
        };

        var response = new InternalListResponse<JobInfo>(new List<JobInfo> { jobInfo });
        MockProductionPeriodsDataHandlerHttpClientGetLatestJobs(response);

        MockQueueWrapperSubscribeForPeriodChanges();

        // Act
        var (responseJobInfo, responseException) =
            await _jobInfoCachingService.GetLatest(MachineId, CancellationToken.None);

        // Assert
        responseJobInfo.Should().BeEquivalentTo(jobInfo);
        responseException.Should().BeNull();

        _productionPeriodsDataHandlerHttpClientMock.VerifyAll();
        _productionPeriodChangesQueueWrapperMock.VerifyAll();
        _machineTimeServiceMock.VerifyAll();
    }

    [Fact]
    public async Task GetLatest_Returns_DataResult_With_JobInfo_If_EndTime_Is_Not_Null()
    {
        // Arrange
        var machineTime = DateTime.UtcNow;
        var end = machineTime.AddMinutes(-1);
        var start = end.AddMinutes(-10);

        var jobInfo = new JobInfo
        {
            MachineId = MachineId,
            ProductId = "FakeProductId",
            JobId = "FakeJobId",
            StartTime = start,
            EndTime = end,
            TimeRanges = new List<TimeRange>
            {
                new(start, end)
            }
        };

        var response = new InternalListResponse<JobInfo>(new List<JobInfo> { jobInfo });
        MockProductionPeriodsDataHandlerHttpClientGetLatestJobs(response);

        MockQueueWrapperSubscribeForPeriodChanges();

        // Act
        var (firstResponseJobInfo, firstResponseException) =
            await _jobInfoCachingService.GetLatest(MachineId, CancellationToken.None);
        var (secondResponseJobInfo, secondResponseException) =
            await _jobInfoCachingService.GetLatest(MachineId, CancellationToken.None);

        // Assert
        firstResponseJobInfo.Should().BeEquivalentTo(jobInfo);
        firstResponseException.Should().BeNull();

        secondResponseJobInfo.Should().BeEquivalentTo(firstResponseJobInfo);
        secondResponseException.Should().BeNull();

        _productionPeriodsDataHandlerHttpClientMock.VerifyAll();
        _productionPeriodChangesQueueWrapperMock.VerifyAll();
        _machineTimeServiceMock.VerifyAll();
    }

    [Fact]
    public async Task Cache_Get_Updated_When_Production_Period_Changed()
    {
        // Arrange
        ProductionPeriodChangesCallback? subscriptionCallback = null;

        var firstJobInfo = new JobInfo
        {
            MachineId = MachineId,
            JobId = "Job001",
            ProductId = "Product001"
        };
        var secondJobInfo = new JobInfo
        {
            MachineId = MachineId,
            JobId = "Job002",
            ProductId = "Product002"
        };
        _productionPeriodsDataHandlerHttpClientMock
            .SetupSequence(m => m.GetLatestJobs(
                It.IsAny<CancellationToken>(),
                It.Is<List<string>>(machineIds => machineIds.Contains(MachineId)),
                0,
                1,
                null))
            .ReturnsAsync(new InternalListResponse<JobInfo>(new List<JobInfo> { firstJobInfo }))
            .ReturnsAsync(new InternalListResponse<JobInfo>(new List<JobInfo> { secondJobInfo }));

        TrackProductionPeriodChangesQueueWrapperSubscribeForPeriodChanges((_, callback) =>
        {
            subscriptionCallback = callback;
        });

        var changedPeriod = new CombinedProductionPeriod
        {
            EndTime = null,
            Job = "Job002"
        };

        // Act
        await _jobInfoCachingService.GetLatest(MachineId, CancellationToken.None);

        subscriptionCallback.Should().NotBeNull();

        await subscriptionCallback!(
            new Mock<IModel>().Object,
            MachineId,
            new List<CombinedProductionPeriod> { changedPeriod },
            new Mock<List<IMemorySafeQueueEventArgs>>().Object.ToArray());

        var (responseJobInfo, responseException) =
            await _jobInfoCachingService.GetLatest(MachineId, CancellationToken.None);

        // Assert
        responseJobInfo.Should().BeEquivalentTo(secondJobInfo);
        responseException.Should().BeNull();

        _productionPeriodsDataHandlerHttpClientMock.VerifyAll();
        _productionPeriodChangesQueueWrapperMock.VerifyAll();
        _machineTimeServiceMock.VerifyAll();
    }

    [Fact]
    public async Task Cache_Does_Not_Get_Updated_When_Production_Period_Changed_And_MachineId_Key_Is_Not_Found()
    {
        // Arrange
        const string otherMachineId = "EQ00002";
        string? subscriptionMachineId = null;
        ProductionPeriodChangesCallback? subscriptionCallback = null;

        var jobInfo = new JobInfo { MachineId = MachineId };
        var response = new InternalListResponse<JobInfo>(new List<JobInfo> { jobInfo });
        MockProductionPeriodsDataHandlerHttpClientGetLatestJobs(response);

        TrackProductionPeriodChangesQueueWrapperSubscribeForPeriodChanges((machineId, callback) =>
        {
            subscriptionMachineId = machineId;
            subscriptionCallback = callback;
        });

        // Act
        await _jobInfoCachingService.GetLatest(MachineId, CancellationToken.None);

        subscriptionMachineId.Should().NotBe(otherMachineId);
        subscriptionCallback.Should().NotBeNull();

        await subscriptionCallback!(
            new Mock<IModel>().Object,
            otherMachineId,
            new List<CombinedProductionPeriod>(),
            new Mock<List<IMemorySafeQueueEventArgs>>().Object.ToArray());

        var (responseJobInfo, responseException) =
            await _jobInfoCachingService.GetLatest(MachineId, CancellationToken.None);

        // Assert
        responseJobInfo.Should().BeEquivalentTo(jobInfo);
        responseException.Should().BeNull();

        _productionPeriodsDataHandlerHttpClientMock.VerifyAll();
        _productionPeriodChangesQueueWrapperMock.VerifyAll();
        _machineTimeServiceMock.VerifyAll();
    }

    [Fact]
    public async Task Cache_Does_Not_Get_Updated_When_Production_Period_Changed_Is_Empty()
    {
        // Arrange
        ProductionPeriodChangesCallback? subscriptionCallback = null;

        var jobInfo = new JobInfo { MachineId = MachineId };
        var response = new InternalListResponse<JobInfo>(new List<JobInfo> { jobInfo });
        MockProductionPeriodsDataHandlerHttpClientGetLatestJobs(response);

        TrackProductionPeriodChangesQueueWrapperSubscribeForPeriodChanges((_, callback) =>
        {
            subscriptionCallback = callback;
        });

        // Act
        await _jobInfoCachingService.GetLatest(MachineId, CancellationToken.None);

        subscriptionCallback.Should().NotBeNull();
        await subscriptionCallback!(
            new Mock<IModel>().Object,
            MachineId,
            new List<CombinedProductionPeriod>(),
            new Mock<List<IMemorySafeQueueEventArgs>>().Object.ToArray());

        var (responseJobInfo, responseException) =
            await _jobInfoCachingService.GetLatest(MachineId, CancellationToken.None);

        // Assert
        responseJobInfo.Should().BeEquivalentTo(jobInfo);
        responseException.Should().BeNull();

        _productionPeriodsDataHandlerHttpClientMock.VerifyAll();
        _productionPeriodChangesQueueWrapperMock.VerifyAll();
        _machineTimeServiceMock.VerifyAll();
    }

    private void MockQueueWrapperSubscribeForPeriodChanges(int times = 1)
    {
        _productionPeriodChangesQueueWrapperMock
            .Setup(m => m.SubscribeForPeriodChanges(
                MachineId,
                It.IsAny<ProductionPeriodChangesCallback>(),
                It.IsAny<ushort>(),
                It.IsAny<bool>(),
                It.IsAny<int?>(),
                It.IsAny<bool>()))
            .Verifiable(Times.Exactly(times));
    }

    private void MockProductionPeriodsDataHandlerHttpClientGetLatestJobs(
        InternalListResponse<JobInfo> response, int times = 1)
    {
        _productionPeriodsDataHandlerHttpClientMock
            .Setup(m => m.GetLatestJobs(
                It.IsAny<CancellationToken>(),
                It.Is<List<string>>(machineIds => machineIds.Contains(MachineId)),
                0,
                1,
                null))
            .ReturnsAsync(response)
            .Verifiable(Times.Exactly(times));
    }

    private void MockMachineTimeServiceGet(DateTime? dateTime, int times = 1)
    {
        _machineTimeServiceMock
            .Setup(m => m.Get(MachineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DataResult<DateTime?>(dateTime, exception: null))
            .Verifiable(Times.Exactly(times));
    }

    private void TrackProductionPeriodChangesQueueWrapperSubscribeForPeriodChanges(
        Action<string, ProductionPeriodChangesCallback> trackSubscribeForPeriodChanges)
    {
        _productionPeriodChangesQueueWrapperMock
            .Setup(m => m.SubscribeForPeriodChanges(
                MachineId,
                It.IsAny<ProductionPeriodChangesCallback>(),
                It.IsAny<ushort>(),
                It.IsAny<bool>(),
                It.IsAny<int?>(),
                It.IsAny<bool>()))
            .Callback<string, ProductionPeriodChangesCallback, ushort, bool, int?, bool>(
                (machineId, callback, _, _, _, _) => { trackSubscribeForPeriodChanges(machineId, callback); });
    }
}