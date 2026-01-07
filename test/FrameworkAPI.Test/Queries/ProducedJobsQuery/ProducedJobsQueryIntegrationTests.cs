using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.Schema.ProducedJob;
using FrameworkAPI.Services;
using FrameworkAPI.Services.Interfaces;
using FrameworkAPI.Test.TestHelpers;
using HotChocolate;
using HotChocolate.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Snapshooter.Xunit;
using WuH.Ruby.Common.Core;
using WuH.Ruby.KpiDataHandler.Client;
using WuH.Ruby.MachineDataHandler.Client;
using WuH.Ruby.MachineSnapShooter.Client;
using WuH.Ruby.MachineSnapShooter.Client.Models;
using WuH.Ruby.OpcUaForwarder.Client;
using WuH.Ruby.ProcessDataReader.Client;
using WuH.Ruby.ProductionPeriodsDataHandler.Client;
using Xunit;
using Machine = WuH.Ruby.MachineDataHandler.Client.Machine;
using MachineFamily = FrameworkAPI.Schema.Misc.MachineFamily;

namespace FrameworkAPI.Test.Queries.ProducedJobsQuery;

public class ProducedJobsQueryIntegrationTests
{
    private const string MachineId = "EQ12345";
    private const string AlternativeMachineId = "EQ6789";
    private const string JobId = "FakeJobId";
    private const string AlternativeJobId = "AlternativeFakeJobId";

    private readonly Mock<IProductionPeriodsDataHandlerHttpClient> _productionPeriodsDataHandlerHttpClientMock = new();
    private readonly Mock<IProcessDataReaderHttpClient> _processDataReaderHttpClientMock = new();
    private readonly Mock<IMachineCachingService> _machineCachingServiceMock = new();
    private readonly Mock<IMachineSnapshotHttpClient> _machineSnapshotHttpClientMock = new();
    private readonly Mock<ILatestMachineSnapshotCachingService> _latestMachineSnapshotCachingServiceMock = new();
    private readonly Mock<IProductionPeriodChangesQueueWrapper> _productionPeriodChangesQueueWrapper = new();
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock = new();
    private readonly Mock<HttpContext> _httpContextMock = new();

    [Fact]
    public async Task Return_Empty_List_When_There_Are_No_Jobs()
    {
        // Arrange
        var executor = await InitializeExecutor();

        _machineCachingServiceMock
            .Setup(m => m.GetMachinesAsInternalListResponse(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalListResponse<Machine>(new List<Machine> { new() { MachineId = MachineId } }));

        _productionPeriodsDataHandlerHttpClientMock
            .Setup(m => m.GetAllJobInfos(
                It.IsAny<CancellationToken>(),
                new List<string> { MachineId },
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                true))
            .ReturnsAsync(new InternalListResponse<JobInfo>(new List<JobInfo>()));

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    producedJobs(machineIdFilter: ""{MachineId}"") {{
                        items {{
                        machineId
                        jobId
                        endTime 
                        isActive
                        startTime
                        productId
                        uniqueId
                        }}
                    }}
                }}")
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        _machineCachingServiceMock.VerifyAll();
        _machineCachingServiceMock.VerifyNoOtherCalls();

        _productionPeriodsDataHandlerHttpClientMock.VerifyAll();
        _productionPeriodsDataHandlerHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Return_Empty_List_When_Filter_Do_Not_Match_For_Machine()
    {
        // Arrange
        var executor = await InitializeExecutor();

        _machineCachingServiceMock
            .Setup(m => m.GetMachinesAsInternalListResponse(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalListResponse<Machine>(new List<Machine>()));

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    producedJobs(machineIdFilter: ""{MachineId}"") {{
                        items {{
                        machineId
                        jobId
                        endTime
                        isActive
                        startTime
                        productId
                        uniqueId
                        }}
                    }}
                }}")
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        _machineCachingServiceMock.VerifyAll();
        _machineCachingServiceMock.VerifyNoOtherCalls();

        _productionPeriodsDataHandlerHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Get_MinValue_With_Two_Columns_And_Same_TimeRanges_Should_Call_SnapShooter_Once()
    {
        // Arrange
        var mockTimeRanges = new List<TimeRange>
        {
            new(
                new DateTime(2002, 5, 10),
                new DateTime(2002, 5, 10)
            )
        };

        var tupleListOfColumnValues = new ValueByColumnId<double?>
        {
            { SnapshotColumnIds.JobSize, 1111.1 }
        };

        _machineCachingServiceMock
            .Setup(m =>
                m.GetMachinesAsInternalListResponse(
                    It.IsAny<CancellationToken>()
                ))
            .ReturnsAsync(new InternalListResponse<Machine>(new List<Machine>
            {
                new()
                {
                    MachineId = MachineId,
                    MachineFamily = MachineFamily.BlowFilm.ToString(),
                    BusinessUnit = BusinessUnit.Extrusion
                }
            }));

        _machineSnapshotHttpClientMock
            .Setup(s => s.GetMinValues(
                MachineId,
                It.IsAny<List<string>>(),
                It.IsAny<List<TimeRange>>(),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new InternalItemResponse<ValueByColumnId<double?>>(tupleListOfColumnValues)
            );

        var executor = await InitializeExecutor();
        var job = new JobInfo
        {
            MachineId = MachineId,
            ProductId = "FakeProductId",
            JobId = JobId,
            StartTime = DateTime.UnixEpoch,
            EndTime = DateTime.UnixEpoch.AddHours(1),
            TimeRanges = mockTimeRanges
        };

        PrepareProductionPeriodsDataHandlerMock(new List<JobInfo> { job, job });
        InitializeHttpContextMocks();

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                @"{
                    producedJobs {
                    items {
                        machineId
                        jobSize {
                        minValue
                        }
                    }
                    }
                }")
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        var test = result.ToJson();
        test.MatchSnapshot();

        _machineSnapshotHttpClientMock.Verify(
            m => m.GetMinValues(
                MachineId,
                It.IsAny<List<string>>(),
                It.IsAny<List<TimeRange>>(),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()), Times.Once);

        _machineSnapshotHttpClientMock.VerifyAll();
        _machineSnapshotHttpClientMock.VerifyNoOtherCalls();

        _productionPeriodsDataHandlerHttpClientMock.VerifyAll();
        _productionPeriodsDataHandlerHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Get_MinValue_With_Two_Columns_And_Different_TimeRanges_Should_Call_SnapShooter_Twice()
    {
        // Arrange
        var timeRanges = new List<TimeRange>
        {
            new(
                new DateTime(2002, 5, 10),
                new DateTime(2002, 5, 10)
            )
        };

        var alternativeTimeRanges = new List<TimeRange>
        {
            new(
                new DateTime(2002, 5, 10),
                new DateTime(2002, 5, 11)
            )
        };

        var tupleListOfColumnValues = new ValueByColumnId<double?>
        {
            { SnapshotColumnIds.JobSize, 1111.1 }
        };

        var alternativeTupleListOfColumnValues = new ValueByColumnId<double?>
        {
            { SnapshotColumnIds.JobSize, 2222.2}
        };

        _machineCachingServiceMock
            .Setup(m =>
                m.GetMachinesAsInternalListResponse(
                    It.IsAny<CancellationToken>()
                ))
            .ReturnsAsync(new InternalListResponse<Machine>(new List<Machine>
            {
                new()
                {
                    MachineId = MachineId,
                    MachineFamily = MachineFamily.BlowFilm.ToString(),
                    BusinessUnit = BusinessUnit.Extrusion
                }
            }));

        _machineSnapshotHttpClientMock
            .Setup(s => s.GetMinValues(
                MachineId,
                It.IsAny<List<string>>(),
                It.Is<List<TimeRange>>(list => list.SequenceEqual(timeRanges)),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new InternalItemResponse<ValueByColumnId<double?>>(tupleListOfColumnValues)
            );

        _machineSnapshotHttpClientMock
            .Setup(s => s.GetMinValues(
                MachineId,
                It.IsAny<List<string>>(),
                It.Is<List<TimeRange>>(list => list.SequenceEqual(alternativeTimeRanges)),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new InternalItemResponse<ValueByColumnId<double?>>(alternativeTupleListOfColumnValues)
            );

        var executor = await InitializeExecutor();
        var job = new JobInfo
        {
            MachineId = MachineId,
            ProductId = "FakeProductId",
            JobId = JobId,
            StartTime = DateTime.UnixEpoch,
            EndTime = DateTime.UnixEpoch.AddHours(1),
            TimeRanges = timeRanges
        };

        var alternativeJob = new JobInfo
        {
            MachineId = MachineId,
            ProductId = "FakeProductId",
            JobId = JobId,
            StartTime = DateTime.UnixEpoch,
            EndTime = DateTime.UnixEpoch.AddHours(1),
            TimeRanges = alternativeTimeRanges
        };

        PrepareProductionPeriodsDataHandlerMock(new List<JobInfo> { job, alternativeJob });
        InitializeHttpContextMocks();

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                @"{
                    producedJobs {
                    items {
                        machineId
                        jobSize {
                        minValue
                        }
                    }
                    }
                }")
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);
        // Assert
        var test = result.ToJson();
        test.MatchSnapshot();

        _machineSnapshotHttpClientMock.Verify(
            m => m.GetMinValues(
                MachineId,
                It.IsAny<List<string>>(),
                It.IsAny<List<TimeRange>>(),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()), Times.Exactly(2));

        _machineSnapshotHttpClientMock.VerifyAll();
        _machineSnapshotHttpClientMock.VerifyNoOtherCalls();

        _productionPeriodsDataHandlerHttpClientMock.VerifyAll();
        _productionPeriodsDataHandlerHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Get_MinValue_With_Same_Columns_And_Different_TimeRanges_Should_Call_SnapshotClient_Each_Time()
    {
        // Arrange
        var firstTimeRanges = new List<TimeRange>
        {
            new(
                new DateTime(2002, 5, 10),
                new DateTime(2002, 5, 10)
            ),
            new(
                new DateTime(2002, 5, 12),
                new DateTime(2002, 5, 13)
            )
        };

        var secondTimeRanges = new List<TimeRange>
        {
            new(
                new DateTime(2002, 5, 10),
                new DateTime(2002, 5, 11)
            )
        };

        var thirdTimeRanges = new List<TimeRange>
        {
            new(
                new DateTime(2002, 5, 11),
                new DateTime(2002, 5, 12)
            )
        };

        var tupleListOfColumnValues = new ValueByColumnId<double?>
        {
            { SnapshotColumnIds.JobSize, 1111.1 }
        };

        _machineCachingServiceMock
            .Setup(m =>
                m.GetMachinesAsInternalListResponse(
                    It.IsAny<CancellationToken>()
                ))
            .ReturnsAsync(new InternalListResponse<Machine>(new List<Machine>
            {
                new()
                {
                    MachineId = MachineId,
                    MachineFamily = MachineFamily.BlowFilm.ToString(),
                    BusinessUnit = BusinessUnit.Extrusion
                }
            }));

        _machineSnapshotHttpClientMock
            .Setup(s => s.GetMinValues(
                MachineId,
                It.IsAny<List<string>>(),
                It.IsAny<List<TimeRange>>(),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new InternalItemResponse<ValueByColumnId<double?>>(tupleListOfColumnValues)
            );

        var executor = await InitializeExecutor();
        var firstJob = new JobInfo
        {
            MachineId = MachineId,
            ProductId = "FakeProductId",
            JobId = JobId,
            StartTime = DateTime.UnixEpoch,
            EndTime = DateTime.UnixEpoch.AddHours(1),
            TimeRanges = firstTimeRanges
        };

        var secondJob = new JobInfo
        {
            MachineId = MachineId,
            ProductId = "FakeProductId",
            JobId = JobId,
            StartTime = DateTime.UnixEpoch,
            EndTime = DateTime.UnixEpoch.AddHours(1),
            TimeRanges = secondTimeRanges
        };

        var thirdJob = new JobInfo
        {
            MachineId = MachineId,
            ProductId = "FakeProductId",
            JobId = JobId,
            StartTime = DateTime.UnixEpoch,
            EndTime = DateTime.UnixEpoch.AddHours(1),
            TimeRanges = thirdTimeRanges
        };

        PrepareProductionPeriodsDataHandlerMock(new List<JobInfo> { firstJob, secondJob, thirdJob });
        InitializeHttpContextMocks();

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                @"{
                    producedJobs {
                    items {
                        machineId
                        jobSize {
                        minValue
                        }
                    }
                    }
                }")
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        var test = result.ToJson();
        test.MatchSnapshot();

        _machineSnapshotHttpClientMock.Verify(
            m => m.GetMinValues(
                MachineId,
                It.IsAny<List<string>>(),
                It.IsAny<List<TimeRange>>(),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()), Times.Exactly(3));

        _machineSnapshotHttpClientMock.VerifyAll();
        _machineSnapshotHttpClientMock.VerifyNoOtherCalls();

        _productionPeriodsDataHandlerHttpClientMock.VerifyAll();
        _productionPeriodsDataHandlerHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Get_MinValue_For_Two_Machines_With_Same_Column_Should_Return_Corresponding_Value()
    {
        // Arrange
        var mockTimeRanges = new List<TimeRange>
        {
            new(
                new DateTime(2002, 5, 10),
                new DateTime(2002, 5, 10)
            )
        };

        var firstTupleListOfColumnValues = new ValueByColumnId<double?>
        {
            { SnapshotColumnIds.JobSize, 1111.1 },
        };

        var alternativeTupleListOfColumnValues = new ValueByColumnId<double?>
        {
            { SnapshotColumnIds.JobSize, 2222.2}
        };

        _machineCachingServiceMock
            .Setup(m =>
                m.GetMachinesAsInternalListResponse(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalListResponse<Machine>(new List<Machine>
            {
                new()
                {
                    MachineId = MachineId,
                    MachineFamily = MachineFamily.FlexoPrint.ToString(),
                    BusinessUnit = BusinessUnit.Extrusion
                },
                new()
                {
                    MachineId = AlternativeMachineId,
                    MachineFamily = MachineFamily.BlowFilm.ToString(),
                    BusinessUnit = BusinessUnit.Extrusion
                }
            }));

        _machineSnapshotHttpClientMock
            .Setup(s => s.GetMinValues(MachineId, It.IsAny<List<string>>(),
                It.IsAny<List<TimeRange>>(),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new InternalItemResponse<ValueByColumnId<double?>>(firstTupleListOfColumnValues)
            );

        _machineSnapshotHttpClientMock
            .Setup(s => s.GetMinValues(AlternativeMachineId, It.IsAny<List<string>>(),
                It.IsAny<List<TimeRange>>(),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new InternalItemResponse<ValueByColumnId<double?>>(alternativeTupleListOfColumnValues)
            );

        var executor = await InitializeExecutor();
        var job = new JobInfo
        {
            MachineId = MachineId,
            ProductId = "FakeProductId",
            JobId = JobId,
            StartTime = DateTime.UnixEpoch,
            EndTime = DateTime.UnixEpoch.AddHours(1),
            TimeRanges = mockTimeRanges
        };

        var alternativeJob = new JobInfo
        {
            MachineId = AlternativeMachineId,
            ProductId = "FakeProductId",
            JobId = AlternativeJobId,
            StartTime = DateTime.UnixEpoch,
            EndTime = DateTime.UnixEpoch.AddHours(1),
            TimeRanges = mockTimeRanges
        };

        PrepareProductionPeriodsDataHandlerMock(new List<JobInfo> { job, alternativeJob });
        InitializeHttpContextMocks();

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                @"{
                    producedJobs {
                    items {
                        machineId
                        jobSize {
                        minValue
                        }
                    }
                    }
                }")
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        var test = result.ToJson();
        test.MatchSnapshot();

        _machineSnapshotHttpClientMock.VerifyAll();
        _machineSnapshotHttpClientMock.VerifyNoOtherCalls();

        _productionPeriodsDataHandlerHttpClientMock.VerifyAll();
        _productionPeriodsDataHandlerHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Get_MaxValue_With_Two_Columns_And_Same_TimeRanges_Should_Call_SnapShooter_Once()
    {
        // Arrange
        var mockTimeRanges = new List<TimeRange>
        {
            new(
                new DateTime(2002, 5, 10),
                new DateTime(2002, 5, 10)
            )
        };

        var tupleListOfColumnValues = new ValueByColumnId<double?>
        {
            { SnapshotColumnIds.JobSize, 1111.1 }
        };

        _machineCachingServiceMock
            .Setup(m =>
                m.GetMachinesAsInternalListResponse(
                    It.IsAny<CancellationToken>()
                ))
            .ReturnsAsync(new InternalListResponse<Machine>(new List<Machine>
            {
                new()
                {
                    MachineId = MachineId,
                    MachineFamily = MachineFamily.BlowFilm.ToString(),
                    BusinessUnit = BusinessUnit.Extrusion
                }
            }));

        _machineSnapshotHttpClientMock
            .Setup(s => s.GetMaxValues(
                MachineId,
                It.IsAny<List<string>>(),
                It.IsAny<List<TimeRange>>(),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new InternalItemResponse<ValueByColumnId<double?>>(tupleListOfColumnValues)
            );

        var executor = await InitializeExecutor();
        var job = new JobInfo
        {
            MachineId = MachineId,
            ProductId = "FakeProductId",
            JobId = JobId,
            StartTime = DateTime.UnixEpoch,
            EndTime = DateTime.UnixEpoch.AddHours(1),
            TimeRanges = mockTimeRanges
        };

        PrepareProductionPeriodsDataHandlerMock(new List<JobInfo> { job, job });
        InitializeHttpContextMocks();

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                @"{
                    producedJobs {
                    items {
                        machineId
                        jobSize {
                        maxValue
                        }
                    }
                    }
                }")
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        var test = result.ToJson();
        test.MatchSnapshot();

        _machineSnapshotHttpClientMock.Verify(
            m => m.GetMaxValues(
                MachineId,
                It.IsAny<List<string>>(),
                It.IsAny<List<TimeRange>>(),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()), Times.Once);

        _machineSnapshotHttpClientMock.VerifyAll();
        _machineSnapshotHttpClientMock.VerifyNoOtherCalls();

        _productionPeriodsDataHandlerHttpClientMock.VerifyAll();
        _productionPeriodsDataHandlerHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Get_MaxValue_With_Two_Columns_And_Different_TimeRanges_Should_Call_SnapShooter_Twice()
    {
        // Arrange
        var timeRanges = new List<TimeRange>
        {
            new(
                new DateTime(2002, 5, 10),
                new DateTime(2002, 5, 10)
            )
        };

        var alternativeTimeRanges = new List<TimeRange>
        {
            new(
                new DateTime(2002, 5, 10),
                new DateTime(2002, 5, 11)
            )
        };

        var tupleListOfColumnValues = new ValueByColumnId<double?>
        {
            { SnapshotColumnIds.JobSize, 1111.1 }
        };

        var alternativeTupleListOfColumnValues = new ValueByColumnId<double?>
        {
            { SnapshotColumnIds.JobSize, 2222.2}
        };

        _machineCachingServiceMock
            .Setup(m =>
                m.GetMachinesAsInternalListResponse(
                    It.IsAny<CancellationToken>()
                ))
            .ReturnsAsync(new InternalListResponse<Machine>(new List<Machine>
            {
                new()
                {
                    MachineId = MachineId,
                    MachineFamily = MachineFamily.BlowFilm.ToString(),
                    BusinessUnit = BusinessUnit.Extrusion
                }
            }));

        _machineSnapshotHttpClientMock
            .Setup(s => s.GetMaxValues(
                MachineId,
                It.IsAny<List<string>>(),
                It.Is<List<TimeRange>>(list => list.SequenceEqual(timeRanges)),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new InternalItemResponse<ValueByColumnId<double?>>(tupleListOfColumnValues)
            );

        _machineSnapshotHttpClientMock
            .Setup(s => s.GetMaxValues(
                MachineId,
                It.IsAny<List<string>>(),
                It.Is<List<TimeRange>>(list => list.SequenceEqual(alternativeTimeRanges)),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new InternalItemResponse<ValueByColumnId<double?>>(alternativeTupleListOfColumnValues)
            );

        var executor = await InitializeExecutor();
        var job = new JobInfo
        {
            MachineId = MachineId,
            ProductId = "FakeProductId",
            JobId = JobId,
            StartTime = DateTime.UnixEpoch,
            EndTime = DateTime.UnixEpoch.AddHours(1),
            TimeRanges = timeRanges
        };

        var alternativeJob = new JobInfo
        {
            MachineId = MachineId,
            ProductId = "FakeProductId",
            JobId = JobId,
            StartTime = DateTime.UnixEpoch,
            EndTime = DateTime.UnixEpoch.AddHours(1),
            TimeRanges = alternativeTimeRanges
        };

        PrepareProductionPeriodsDataHandlerMock(new List<JobInfo> { job, alternativeJob });
        InitializeHttpContextMocks();

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                @"{
                    producedJobs {
                    items {
                        machineId
                        jobSize {
                        maxValue
                        }
                    }
                    }
                }")
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        var test = result.ToJson();
        test.MatchSnapshot();

        _machineSnapshotHttpClientMock.Verify(
            m => m.GetMaxValues(
                MachineId,
                It.IsAny<List<string>>(),
                It.IsAny<List<TimeRange>>(),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()), Times.Exactly(2));

        _machineSnapshotHttpClientMock.VerifyAll();
        _machineSnapshotHttpClientMock.VerifyNoOtherCalls();

        _productionPeriodsDataHandlerHttpClientMock.VerifyAll();
        _productionPeriodsDataHandlerHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Get_MaxValue_With_Same_Columns_And_Different_TimeRanges_Should_Call_SnapshotClient_Each_Time()
    {
        // Arrange
        var firstTimeRanges = new List<TimeRange>
        {
            new(
                new DateTime(2002, 5, 10),
                new DateTime(2002, 5, 10)
            ),
            new(
                new DateTime(2002, 5, 12),
                new DateTime(2002, 5, 13)
            )
        };

        var secondTimeRanges = new List<TimeRange>
        {
            new(
                new DateTime(2002, 5, 10),
                new DateTime(2002, 5, 11)
            )
        };

        var thirdTimeRanges = new List<TimeRange>
        {
            new(
                new DateTime(2002, 5, 11),
                new DateTime(2002, 5, 12)
            )
        };

        var tupleListOfColumnValues = new ValueByColumnId<double?>
        {
            { SnapshotColumnIds.JobSize, 1111.1 }
        };

        _machineCachingServiceMock
            .Setup(m =>
                m.GetMachinesAsInternalListResponse(
                    It.IsAny<CancellationToken>()
                ))
            .ReturnsAsync(new InternalListResponse<Machine>(new List<Machine>
            {
                new()
                {
                    MachineId = MachineId,
                    MachineFamily = MachineFamily.BlowFilm.ToString(),
                    BusinessUnit = BusinessUnit.Extrusion
                }
            }));

        _machineSnapshotHttpClientMock
            .Setup(s => s.GetMaxValues(
                MachineId,
                It.IsAny<List<string>>(),
                It.IsAny<List<TimeRange>>(),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new InternalItemResponse<ValueByColumnId<double?>>(tupleListOfColumnValues)
            );

        var executor = await InitializeExecutor();
        var firstJob = new JobInfo
        {
            MachineId = MachineId,
            ProductId = "FakeProductId",
            JobId = JobId,
            StartTime = DateTime.UnixEpoch,
            EndTime = DateTime.UnixEpoch.AddHours(1),
            TimeRanges = firstTimeRanges
        };

        var secondJob = new JobInfo
        {
            MachineId = MachineId,
            ProductId = "FakeProductId",
            JobId = JobId,
            StartTime = DateTime.UnixEpoch,
            EndTime = DateTime.UnixEpoch.AddHours(1),
            TimeRanges = secondTimeRanges
        };

        var thirdJob = new JobInfo
        {
            MachineId = MachineId,
            ProductId = "FakeProductId",
            JobId = JobId,
            StartTime = DateTime.UnixEpoch,
            EndTime = DateTime.UnixEpoch.AddHours(1),
            TimeRanges = thirdTimeRanges
        };

        PrepareProductionPeriodsDataHandlerMock(new List<JobInfo> { firstJob, secondJob, thirdJob });
        InitializeHttpContextMocks();

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                @"{
                    producedJobs {
                    items {
                        machineId
                        jobSize {
                        maxValue
                        }
                    }
                    }
                }")
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        var test = result.ToJson();
        test.MatchSnapshot();

        _machineSnapshotHttpClientMock.Verify(
            m => m.GetMaxValues(
                MachineId,
                It.IsAny<List<string>>(),
                It.IsAny<List<TimeRange>>(),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()), Times.Exactly(3));

        _machineSnapshotHttpClientMock.VerifyAll();
        _machineSnapshotHttpClientMock.VerifyNoOtherCalls();

        _productionPeriodsDataHandlerHttpClientMock.VerifyAll();
        _productionPeriodsDataHandlerHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Get_MaxValue_For_Two_Machines_With_Same_Column_Should_Return_Corresponding_Value()
    {
        // Arrange
        var mockTimeRanges = new List<TimeRange>
        {
            new(
                new DateTime(2002, 5, 10),
                new DateTime(2002, 5, 10)
            )
        };

        var firstTupleListOfColumnValues = new ValueByColumnId<double?>
        {
            { SnapshotColumnIds.JobSize, 1111.1 },
        };

        var alternativeTupleListOfColumnValues = new ValueByColumnId<double?>
        {
            { SnapshotColumnIds.JobSize, 2222.2}
        };

        _machineCachingServiceMock
            .Setup(m =>
                m.GetMachinesAsInternalListResponse(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalListResponse<Machine>(new List<Machine>
            {
                new()
                {
                    MachineId = MachineId,
                    MachineFamily = MachineFamily.FlexoPrint.ToString(),
                    BusinessUnit = BusinessUnit.Extrusion
                },
                new()
                {
                    MachineId = AlternativeMachineId,
                    MachineFamily = MachineFamily.BlowFilm.ToString(),
                    BusinessUnit = BusinessUnit.Extrusion
                }
            }));

        _machineSnapshotHttpClientMock
            .Setup(s => s.GetMaxValues(MachineId, It.IsAny<List<string>>(),
                It.IsAny<List<TimeRange>>(),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new InternalItemResponse<ValueByColumnId<double?>>(firstTupleListOfColumnValues)
            );

        _machineSnapshotHttpClientMock
            .Setup(s => s.GetMaxValues(AlternativeMachineId, It.IsAny<List<string>>(),
                It.IsAny<List<TimeRange>>(),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new InternalItemResponse<ValueByColumnId<double?>>(alternativeTupleListOfColumnValues)
            );

        var executor = await InitializeExecutor();
        var job = new JobInfo
        {
            MachineId = MachineId,
            ProductId = "FakeProductId",
            JobId = JobId,
            StartTime = DateTime.UnixEpoch,
            EndTime = DateTime.UnixEpoch.AddHours(1),
            TimeRanges = mockTimeRanges
        };

        var alternativeJob = new JobInfo
        {
            MachineId = AlternativeMachineId,
            ProductId = "FakeProductId",
            JobId = AlternativeJobId,
            StartTime = DateTime.UnixEpoch,
            EndTime = DateTime.UnixEpoch.AddHours(1),
            TimeRanges = mockTimeRanges
        };

        PrepareProductionPeriodsDataHandlerMock(new List<JobInfo> { job, alternativeJob });
        InitializeHttpContextMocks();

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                @"{
                    producedJobs {
                    items {
                        machineId
                        jobSize {
                        maxValue
                        }
                    }
                    }
                }")
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        var test = result.ToJson();
        test.MatchSnapshot();

        _machineSnapshotHttpClientMock.VerifyAll();
        _machineSnapshotHttpClientMock.VerifyNoOtherCalls();

        _productionPeriodsDataHandlerHttpClientMock.VerifyAll();
        _productionPeriodsDataHandlerHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Get_Min_And_Max_With_Same_Columns_And_Same_TimeRanges_Should_Call_SnapShooter_Twice()
    {
        // Arrange
        var timeRanges = new List<TimeRange>
        {
            new(
                new DateTime(2002, 5, 10),
                new DateTime(2002, 5, 10)
            )
        };

        var tupleListOfColumnValues = new ValueByColumnId<double?>
        {
            { SnapshotColumnIds.JobSize, 1111.1 }
        };

        var alternativeTupleListOfColumnValues = new ValueByColumnId<double?>
        {
            { SnapshotColumnIds.JobSize, 2222.2}
        };

        _machineCachingServiceMock
            .Setup(m =>
                m.GetMachinesAsInternalListResponse(
                    It.IsAny<CancellationToken>()
                ))
            .ReturnsAsync(new InternalListResponse<Machine>(new List<Machine>
            {
                new()
                {
                    MachineId = MachineId,
                    MachineFamily = MachineFamily.BlowFilm.ToString(),
                    BusinessUnit = BusinessUnit.Extrusion
                }
            }));

        _machineSnapshotHttpClientMock
            .Setup(s => s.GetMinValues(
                MachineId,
                It.IsAny<List<string>>(),
                It.Is<List<TimeRange>>(list => list.SequenceEqual(timeRanges)),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new InternalItemResponse<ValueByColumnId<double?>>(tupleListOfColumnValues)
            );

        _machineSnapshotHttpClientMock
            .Setup(s => s.GetMaxValues(
                MachineId,
                It.IsAny<List<string>>(),
                It.Is<List<TimeRange>>(list => list.SequenceEqual(timeRanges)),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new InternalItemResponse<ValueByColumnId<double?>>(alternativeTupleListOfColumnValues)
            );

        var executor = await InitializeExecutor();
        var job = new JobInfo
        {
            MachineId = MachineId,
            ProductId = "FakeProductId",
            JobId = JobId,
            StartTime = DateTime.UnixEpoch,
            EndTime = DateTime.UnixEpoch.AddHours(1),
            TimeRanges = timeRanges
        };

        PrepareProductionPeriodsDataHandlerMock(new List<JobInfo> { job });
        InitializeHttpContextMocks();

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                @"{
                    test1: producedJobs {
                    items {
                        machineId
                        jobSize {
                            minValue
                        }
                    }
                    }
                    test2: producedJobs {
                    items {
                        machineId
                        jobSize {
                        maxValue
                        }
                    }
                    }
                }")
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        var test = result.ToJson();
        test.MatchSnapshot();

        _machineSnapshotHttpClientMock.Verify(
            m => m.GetMinValues(
                MachineId,
                It.IsAny<List<string>>(),
                It.IsAny<List<TimeRange>>(),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()), Times.Once);

        _machineSnapshotHttpClientMock.Verify(
            m => m.GetMaxValues(
                MachineId,
                It.IsAny<List<string>>(),
                It.IsAny<List<TimeRange>>(),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()), Times.Once);

        _machineSnapshotHttpClientMock.VerifyAll();
        _machineSnapshotHttpClientMock.VerifyNoOtherCalls();

        _productionPeriodsDataHandlerHttpClientMock.VerifyAll();
        _productionPeriodsDataHandlerHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Get_Min_With_Different_Columns_And_Same_TimeRanges_Should_Call_SnapShooter_Once()
    {
        // Arrange
        var timeRanges = new List<TimeRange>
        {
            new(
                new DateTime(2002, 5, 10),
                new DateTime(2002, 5, 10)
            )
        };

        var tupleListOfColumnValues = new ValueByColumnId<double?>
        {
            { SnapshotColumnIds.JobSize, 1111.1 },
            { SnapshotColumnIds.ExtrusionFormatSettingsThickness, 2222.2 }
        };

        _machineCachingServiceMock
            .Setup(m =>
                m.GetMachinesAsInternalListResponse(
                    It.IsAny<CancellationToken>()
                ))
            .ReturnsAsync(new InternalListResponse<Machine>(new List<Machine>
            {
                new()
                {
                    MachineId = MachineId,
                    MachineFamily = MachineFamily.BlowFilm.ToString(),
                    BusinessUnit = BusinessUnit.Extrusion
                }
            }));

        _machineSnapshotHttpClientMock
            .Setup(s => s.GetMinValues(
                MachineId,
                It.IsAny<List<string>>(),
                It.Is<List<TimeRange>>(list => list.SequenceEqual(timeRanges)),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new InternalItemResponse<ValueByColumnId<double?>>(tupleListOfColumnValues)
            );

        var executor = await InitializeExecutor();
        var job = new JobInfo
        {
            MachineId = MachineId,
            ProductId = "FakeProductId",
            JobId = JobId,
            StartTime = DateTime.UnixEpoch,
            EndTime = DateTime.UnixEpoch.AddHours(1),
            TimeRanges = timeRanges
        };

        PrepareProductionPeriodsDataHandlerMock(new List<JobInfo> { job });
        InitializeHttpContextMocks();

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                @"{
                    producedJobs {
                    items {
                        ... on ExtrusionProducedJob {
                        jobSize {
                            minValue
                        }
                        machineSettings {
                            thickness {
                            minValue
                            }
                        }
                        }
                    }
                    }
                }")
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        var test = result.ToJson();
        test.MatchSnapshot();

        _machineSnapshotHttpClientMock.Verify(
            m => m.GetMinValues(
                MachineId,
                It.IsAny<List<string>>(),
                It.IsAny<List<TimeRange>>(),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()), Times.Once);

        _machineSnapshotHttpClientMock.VerifyAll();
        _machineSnapshotHttpClientMock.VerifyNoOtherCalls();

        _productionPeriodsDataHandlerHttpClientMock.VerifyAll();
        _productionPeriodsDataHandlerHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Get_MedianValue_With_Two_Columns_And_Same_TimeRanges_Should_Call_SnapShooter_Once()
    {
        // Arrange
        var mockTimeRanges = new List<TimeRange>
        {
            new(
                new DateTime(2002, 5, 10),
                new DateTime(2002, 5, 10)
            )
        };

        var tupleListOfColumnValues = new ValueByColumnId<double?>
        {
            { SnapshotColumnIds.JobSize, 1111.1 }
        };

        _machineCachingServiceMock
            .Setup(m =>
                m.GetMachinesAsInternalListResponse(
                    It.IsAny<CancellationToken>()
                ))
            .ReturnsAsync(new InternalListResponse<Machine>(new List<Machine>
            {
                new()
                {
                    MachineId = MachineId,
                    MachineFamily = MachineFamily.BlowFilm.ToString(),
                    BusinessUnit = BusinessUnit.Extrusion
                }
            }));

        _machineSnapshotHttpClientMock
            .Setup(s => s.GetMedianValues(
                MachineId,
                It.IsAny<List<string>>(),
                It.IsAny<List<TimeRange>>(),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new InternalItemResponse<ValueByColumnId<double?>>(tupleListOfColumnValues)
            );

        var executor = await InitializeExecutor();
        var job = new JobInfo
        {
            MachineId = MachineId,
            ProductId = "FakeProductId",
            JobId = JobId,
            StartTime = DateTime.UnixEpoch,
            EndTime = DateTime.UnixEpoch.AddHours(1),
            TimeRanges = mockTimeRanges
        };

        PrepareProductionPeriodsDataHandlerMock(new List<JobInfo> { job, job });
        InitializeHttpContextMocks();

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                @"{
                    producedJobs {
                    items {
                        machineId
                        jobSize {
                        median
                        }
                    }
                    }
                }")
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        var test = result.ToJson();
        test.MatchSnapshot();

        _machineSnapshotHttpClientMock.Verify(
            m => m.GetMedianValues(
                MachineId,
                It.IsAny<List<string>>(),
                It.IsAny<List<TimeRange>>(),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()), Times.Once);

        _machineSnapshotHttpClientMock.VerifyAll();
        _machineSnapshotHttpClientMock.VerifyNoOtherCalls();

        _productionPeriodsDataHandlerHttpClientMock.VerifyAll();
        _productionPeriodsDataHandlerHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Get_MedianValue_With_Two_Columns_And_Different_TimeRanges_Should_Call_SnapShooter_Twice()
    {
        // Arrange
        var timeRanges = new List<TimeRange>
        {
            new(
                new DateTime(2002, 5, 10),
                new DateTime(2002, 5, 10)
            )
        };

        var alternativeTimeRanges = new List<TimeRange>
        {
            new(
                new DateTime(2002, 5, 10),
                new DateTime(2002, 5, 11)
            )
        };

        var tupleListOfColumnValues = new ValueByColumnId<double?>
        {
            { SnapshotColumnIds.JobSize, 1111.1 }
        };

        var alternativeTupleListOfColumnValues = new ValueByColumnId<double?>
        {
            { SnapshotColumnIds.JobSize, 2222.2}
        };

        _machineCachingServiceMock
            .Setup(m =>
                m.GetMachinesAsInternalListResponse(
                    It.IsAny<CancellationToken>()
                ))
            .ReturnsAsync(new InternalListResponse<Machine>(new List<Machine>
            {
                new()
                {
                    MachineId = MachineId,
                    MachineFamily = MachineFamily.BlowFilm.ToString(),
                    BusinessUnit = BusinessUnit.Extrusion
                }
            }));

        _machineSnapshotHttpClientMock
            .Setup(s => s.GetMedianValues(
                MachineId,
                It.IsAny<List<string>>(),
                It.Is<List<TimeRange>>(list => list.SequenceEqual(timeRanges)),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new InternalItemResponse<ValueByColumnId<double?>>(tupleListOfColumnValues)
            );

        _machineSnapshotHttpClientMock
            .Setup(s => s.GetMedianValues(
                MachineId,
                It.IsAny<List<string>>(),
                It.Is<List<TimeRange>>(list => list.SequenceEqual(alternativeTimeRanges)),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new InternalItemResponse<ValueByColumnId<double?>>(alternativeTupleListOfColumnValues)
            );

        var executor = await InitializeExecutor();
        var job = new JobInfo
        {
            MachineId = MachineId,
            ProductId = "FakeProductId",
            JobId = JobId,
            StartTime = DateTime.UnixEpoch,
            EndTime = DateTime.UnixEpoch.AddHours(1),
            TimeRanges = timeRanges
        };

        var alternativeJob = new JobInfo
        {
            MachineId = MachineId,
            ProductId = "FakeProductId",
            JobId = JobId,
            StartTime = DateTime.UnixEpoch,
            EndTime = DateTime.UnixEpoch.AddHours(1),
            TimeRanges = alternativeTimeRanges
        };

        PrepareProductionPeriodsDataHandlerMock(new List<JobInfo> { job, alternativeJob });
        InitializeHttpContextMocks();

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                @"{
                    producedJobs {
                    items {
                        machineId
                        jobSize {
                        median
                        }
                    }
                    }
                }")
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        var test = result.ToJson();
        test.MatchSnapshot();

        _machineSnapshotHttpClientMock.Verify(
            m => m.GetMedianValues(
                MachineId,
                It.IsAny<List<string>>(),
                It.IsAny<List<TimeRange>>(),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()), Times.Exactly(2));

        _machineSnapshotHttpClientMock.VerifyAll();
        _machineSnapshotHttpClientMock.VerifyNoOtherCalls();

        _productionPeriodsDataHandlerHttpClientMock.VerifyAll();
        _productionPeriodsDataHandlerHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Get_MedianValue_With_Same_Columns_And_Different_TimeRanges_Should_Call_SnapshotClient_Each_Time()
    {
        // Arrange
        var firstTimeRanges = new List<TimeRange>
        {
            new(
                new DateTime(2002, 5, 10),
                new DateTime(2002, 5, 10)
            ),
            new(
                new DateTime(2002, 5, 12),
                new DateTime(2002, 5, 13)
            )
        };

        var secondTimeRanges = new List<TimeRange>
        {
            new(
                new DateTime(2002, 5, 10),
                new DateTime(2002, 5, 11)
            )
        };

        var thirdTimeRanges = new List<TimeRange>
        {
            new(
                new DateTime(2002, 5, 11),
                new DateTime(2002, 5, 12)
            )
        };

        var tupleListOfColumnValues = new ValueByColumnId<double?>
        {
            { SnapshotColumnIds.JobSize, 1111.1 }
        };

        _machineCachingServiceMock
            .Setup(m =>
                m.GetMachinesAsInternalListResponse(
                    It.IsAny<CancellationToken>()
                ))
            .ReturnsAsync(new InternalListResponse<Machine>(new List<Machine>
            {
                new()
                {
                    MachineId = MachineId,
                    MachineFamily = MachineFamily.BlowFilm.ToString(),
                    BusinessUnit = BusinessUnit.Extrusion
                }
            }));

        _machineSnapshotHttpClientMock
            .Setup(s => s.GetMedianValues(
                MachineId,
                It.IsAny<List<string>>(),
                It.IsAny<List<TimeRange>>(),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new InternalItemResponse<ValueByColumnId<double?>>(tupleListOfColumnValues)
            );

        var executor = await InitializeExecutor();
        var firstJob = new JobInfo
        {
            MachineId = MachineId,
            ProductId = "FakeProductId",
            JobId = JobId,
            StartTime = DateTime.UnixEpoch,
            EndTime = DateTime.UnixEpoch.AddHours(1),
            TimeRanges = firstTimeRanges
        };

        var secondJob = new JobInfo
        {
            MachineId = MachineId,
            ProductId = "FakeProductId",
            JobId = JobId,
            StartTime = DateTime.UnixEpoch,
            EndTime = DateTime.UnixEpoch.AddHours(1),
            TimeRanges = secondTimeRanges
        };

        var thirdJob = new JobInfo
        {
            MachineId = MachineId,
            ProductId = "FakeProductId",
            JobId = JobId,
            StartTime = DateTime.UnixEpoch,
            EndTime = DateTime.UnixEpoch.AddHours(1),
            TimeRanges = thirdTimeRanges
        };

        PrepareProductionPeriodsDataHandlerMock(new List<JobInfo> { firstJob, secondJob, thirdJob });
        InitializeHttpContextMocks();

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                @"{
                    producedJobs {
                    items {
                        machineId
                        jobSize {
                        median
                        }
                    }
                    }
                }")
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);
        // Assert
        var test = result.ToJson();
        test.MatchSnapshot();

        _machineSnapshotHttpClientMock.Verify(
            m => m.GetMedianValues(
                MachineId,
                It.IsAny<List<string>>(),
                It.IsAny<List<TimeRange>>(),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()), Times.Exactly(3));

        _machineSnapshotHttpClientMock.VerifyAll();
        _machineSnapshotHttpClientMock.VerifyNoOtherCalls();

        _productionPeriodsDataHandlerHttpClientMock.VerifyAll();
        _productionPeriodsDataHandlerHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Get_MedianValue_For_Two_Machines_With_Same_Column_Should_Return_Corresponding_Value()
    {
        // Arrange
        var mockTimeRanges = new List<TimeRange>
        {
            new(
                new DateTime(2002, 5, 10),
                new DateTime(2002, 5, 10)
            )
        };

        var firstTupleListOfColumnValues = new ValueByColumnId<double?>
        {
            { SnapshotColumnIds.JobSize, 1111.1 },
        };

        var alternativeTupleListOfColumnValues = new ValueByColumnId<double?>
        {
            { SnapshotColumnIds.JobSize, 2222.2}
        };

        _machineCachingServiceMock
            .Setup(m =>
                m.GetMachinesAsInternalListResponse(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalListResponse<Machine>(new List<Machine>
            {
                new()
                {
                    MachineId = MachineId,
                    MachineFamily = MachineFamily.FlexoPrint.ToString(),
                    BusinessUnit = BusinessUnit.Extrusion
                },
                new()
                {
                    MachineId = AlternativeMachineId,
                    MachineFamily = MachineFamily.BlowFilm.ToString(),
                    BusinessUnit = BusinessUnit.Extrusion
                }
            }));

        _machineSnapshotHttpClientMock
            .Setup(s => s.GetMedianValues(
                MachineId,
                It.IsAny<List<string>>(),
                It.IsAny<List<TimeRange>>(),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new InternalItemResponse<ValueByColumnId<double?>>(firstTupleListOfColumnValues)
            );

        _machineSnapshotHttpClientMock
            .Setup(s => s.GetMedianValues(
                AlternativeMachineId,
                It.IsAny<List<string>>(),
                It.IsAny<List<TimeRange>>(),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new InternalItemResponse<ValueByColumnId<double?>>(alternativeTupleListOfColumnValues)
            );

        var executor = await InitializeExecutor();
        var job = new JobInfo
        {
            MachineId = MachineId,
            ProductId = "FakeProductId",
            JobId = JobId,
            StartTime = DateTime.UnixEpoch,
            EndTime = DateTime.UnixEpoch.AddHours(1),
            TimeRanges = mockTimeRanges
        };

        var alternativeJob = new JobInfo
        {
            MachineId = AlternativeMachineId,
            ProductId = "FakeProductId",
            JobId = AlternativeJobId,
            StartTime = DateTime.UnixEpoch,
            EndTime = DateTime.UnixEpoch.AddHours(1),
            TimeRanges = mockTimeRanges
        };

        PrepareProductionPeriodsDataHandlerMock(new List<JobInfo> { job, alternativeJob });
        InitializeHttpContextMocks();

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                @"{
                    producedJobs {
                    items {
                        machineId
                        jobSize {
                        median
                        }
                    }
                    }
                }")
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        var test = result.ToJson();
        test.MatchSnapshot();

        _machineSnapshotHttpClientMock.VerifyAll();
        _machineSnapshotHttpClientMock.VerifyNoOtherCalls();

        _productionPeriodsDataHandlerHttpClientMock.VerifyAll();
        _productionPeriodsDataHandlerHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Get_StandardDeviation_With_Two_Columns_And_Same_TimeRanges_Should_Call_SnapShooter_Once()
    {
        // Arrange
        var mockTimeRanges = new List<TimeRange>
        {
            new(
                new DateTime(2002, 5, 10),
                new DateTime(2002, 5, 10)
            )
        };

        var tupleListOfColumnValues = new ValueByColumnId<double?>
        {
            { SnapshotColumnIds.JobSize, 1111.1 }
        };

        _machineCachingServiceMock
            .Setup(m =>
                m.GetMachinesAsInternalListResponse(
                    It.IsAny<CancellationToken>()
                ))
            .ReturnsAsync(new InternalListResponse<Machine>(new List<Machine>
            {
                new()
                {
                    MachineId = MachineId,
                    MachineFamily = MachineFamily.BlowFilm.ToString(),
                    BusinessUnit = BusinessUnit.Extrusion
                }
            }));

        _machineSnapshotHttpClientMock
            .Setup(s => s.GetStandardDeviations(
                MachineId,
                It.IsAny<List<string>>(),
                It.IsAny<List<TimeRange>>(),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new InternalItemResponse<ValueByColumnId<double?>>(tupleListOfColumnValues)
            );

        var executor = await InitializeExecutor();
        var job = new JobInfo
        {
            MachineId = MachineId,
            ProductId = "FakeProductId",
            JobId = JobId,
            StartTime = DateTime.UnixEpoch,
            EndTime = DateTime.UnixEpoch.AddHours(1),
            TimeRanges = mockTimeRanges
        };

        PrepareProductionPeriodsDataHandlerMock(new List<JobInfo> { job, job });
        InitializeHttpContextMocks();

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                @"{
                    producedJobs {
                    items {
                        machineId
                        jobSize {
                        standardDeviationValue
                        }
                    }
                    }
                }")
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        var test = result.ToJson();
        test.MatchSnapshot();

        _machineSnapshotHttpClientMock.Verify(
            m => m.GetStandardDeviations(
                MachineId,
                It.IsAny<List<string>>(),
                It.IsAny<List<TimeRange>>(),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()), Times.Once);

        _machineSnapshotHttpClientMock.VerifyAll();
        _machineSnapshotHttpClientMock.VerifyNoOtherCalls();

        _productionPeriodsDataHandlerHttpClientMock.VerifyAll();
        _productionPeriodsDataHandlerHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Get_StandardDeviation_With_Two_Columns_And_Different_TimeRanges_Should_Call_SnapShooter_Twice()
    {
        // Arrange
        var timeRanges = new List<TimeRange>
        {
            new(
                new DateTime(2002, 5, 10),
                new DateTime(2002, 5, 10)
            )
        };

        var alternativeTimeRanges = new List<TimeRange>
        {
            new(
                new DateTime(2002, 5, 10),
                new DateTime(2002, 5, 11)
            )
        };

        var tupleListOfColumnValues = new ValueByColumnId<double?>
        {
            { SnapshotColumnIds.JobSize, 1111.1 }
        };

        var alternativeTupleListOfColumnValues = new ValueByColumnId<double?>
        {
            { SnapshotColumnIds.JobSize, 2222.2}
        };

        _machineCachingServiceMock
            .Setup(m =>
                m.GetMachinesAsInternalListResponse(
                    It.IsAny<CancellationToken>()
                ))
            .ReturnsAsync(new InternalListResponse<Machine>(new List<Machine>
            {
                new()
                {
                    MachineId = MachineId,
                    MachineFamily = MachineFamily.BlowFilm.ToString(),
                    BusinessUnit = BusinessUnit.Extrusion
                }
            }));

        _machineSnapshotHttpClientMock
            .Setup(s => s.GetStandardDeviations(
                MachineId,
                It.IsAny<List<string>>(),
                It.Is<List<TimeRange>>(list => list.SequenceEqual(timeRanges)),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new InternalItemResponse<ValueByColumnId<double?>>(tupleListOfColumnValues)
            );

        _machineSnapshotHttpClientMock
            .Setup(s => s.GetStandardDeviations(
                MachineId,
                It.IsAny<List<string>>(),
                It.Is<List<TimeRange>>(list => list.SequenceEqual(alternativeTimeRanges)),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new InternalItemResponse<ValueByColumnId<double?>>(alternativeTupleListOfColumnValues)
            );

        var executor = await InitializeExecutor();
        var job = new JobInfo
        {
            MachineId = MachineId,
            ProductId = "FakeProductId",
            JobId = JobId,
            StartTime = DateTime.UnixEpoch,
            EndTime = DateTime.UnixEpoch.AddHours(1),
            TimeRanges = timeRanges
        };

        var alternativeJob = new JobInfo
        {
            MachineId = MachineId,
            ProductId = "FakeProductId",
            JobId = JobId,
            StartTime = DateTime.UnixEpoch,
            EndTime = DateTime.UnixEpoch.AddHours(1),
            TimeRanges = alternativeTimeRanges
        };

        PrepareProductionPeriodsDataHandlerMock(new List<JobInfo> { job, alternativeJob });
        InitializeHttpContextMocks();

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                @"{
                    producedJobs {
                    items {
                        machineId
                        jobSize {
                        standardDeviationValue
                        }
                    }
                    }
                }")
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        var test = result.ToJson();
        test.MatchSnapshot();

        _machineSnapshotHttpClientMock.Verify(
            m => m.GetStandardDeviations(
                MachineId,
                It.IsAny<List<string>>(),
                It.IsAny<List<TimeRange>>(),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()), Times.Exactly(2));

        _machineSnapshotHttpClientMock.VerifyAll();
        _machineSnapshotHttpClientMock.VerifyNoOtherCalls();

        _productionPeriodsDataHandlerHttpClientMock.VerifyAll();
        _productionPeriodsDataHandlerHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task
        Get_StandardDeviation_With_Same_Columns_And_Different_TimeRanges_Should_Call_SnapshotClient_Each_Time()
    {
        // Arrange
        var firstTimeRanges = new List<TimeRange>
        {
            new(
                new DateTime(2002, 5, 10),
                new DateTime(2002, 5, 10)
            ),
            new(
                new DateTime(2002, 5, 12),
                new DateTime(2002, 5, 13)
            )
        };

        var secondTimeRanges = new List<TimeRange>
        {
            new(
                new DateTime(2002, 5, 10),
                new DateTime(2002, 5, 11)
            )
        };

        var thirdTimeRanges = new List<TimeRange>
        {
            new(
                new DateTime(2002, 5, 11),
                new DateTime(2002, 5, 12)
            )
        };

        var tupleListOfColumnValues = new ValueByColumnId<double?>
        {
            { SnapshotColumnIds.JobSize, 1111.1 }
        };

        _machineCachingServiceMock
            .Setup(m =>
                m.GetMachinesAsInternalListResponse(
                    It.IsAny<CancellationToken>()
                ))
            .ReturnsAsync(new InternalListResponse<Machine>(new List<Machine>
            {
                new()
                {
                    MachineId = MachineId,
                    MachineFamily = MachineFamily.BlowFilm.ToString(),
                    BusinessUnit = BusinessUnit.Extrusion
                }
            }));

        _machineSnapshotHttpClientMock
            .Setup(s => s.GetStandardDeviations(
                MachineId,
                It.IsAny<List<string>>(),
                It.IsAny<List<TimeRange>>(),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new InternalItemResponse<ValueByColumnId<double?>>(tupleListOfColumnValues)
            );

        var executor = await InitializeExecutor();
        var firstJob = new JobInfo
        {
            MachineId = MachineId,
            ProductId = "FakeProductId",
            JobId = JobId,
            StartTime = DateTime.UnixEpoch,
            EndTime = DateTime.UnixEpoch.AddHours(1),
            TimeRanges = firstTimeRanges
        };

        var secondJob = new JobInfo
        {
            MachineId = MachineId,
            ProductId = "FakeProductId",
            JobId = JobId,
            StartTime = DateTime.UnixEpoch,
            EndTime = DateTime.UnixEpoch.AddHours(1),
            TimeRanges = secondTimeRanges
        };

        var thirdJob = new JobInfo
        {
            MachineId = MachineId,
            ProductId = "FakeProductId",
            JobId = JobId,
            StartTime = DateTime.UnixEpoch,
            EndTime = DateTime.UnixEpoch.AddHours(1),
            TimeRanges = thirdTimeRanges
        };

        PrepareProductionPeriodsDataHandlerMock(new List<JobInfo> { firstJob, secondJob, thirdJob });
        InitializeHttpContextMocks();

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                @"{
                    producedJobs {
                    items {
                        machineId
                        jobSize {
                        standardDeviationValue
                        }
                    }
                    }
                }")
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        var test = result.ToJson();
        test.MatchSnapshot();

        _machineSnapshotHttpClientMock.Verify(
            m => m.GetStandardDeviations(
                MachineId,
                It.IsAny<List<string>>(),
                It.IsAny<List<TimeRange>>(),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()), Times.Exactly(3));

        _machineSnapshotHttpClientMock.VerifyAll();
        _machineSnapshotHttpClientMock.VerifyNoOtherCalls();

        _productionPeriodsDataHandlerHttpClientMock.VerifyAll();
        _productionPeriodsDataHandlerHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Get_StandardDeviation_For_Two_Machines_With_Same_Column_Should_Return_Corresponding_Value()
    {
        // Arrange
        var mockTimeRanges = new List<TimeRange>
        {
            new(
                new DateTime(2002, 5, 10),
                new DateTime(2002, 5, 10)
            )
        };

        var firstTupleListOfColumnValues = new ValueByColumnId<double?>
        {
            { SnapshotColumnIds.JobSize, 1111.1 },
        };

        var alternativeTupleListOfColumnValues = new ValueByColumnId<double?>
        {
            { SnapshotColumnIds.JobSize, 2222.2}
        };

        _machineCachingServiceMock
            .Setup(m =>
                m.GetMachinesAsInternalListResponse(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalListResponse<Machine>(new List<Machine>
            {
                new()
                {
                    MachineId = MachineId,
                    MachineFamily = MachineFamily.FlexoPrint.ToString(),
                    BusinessUnit = BusinessUnit.Extrusion
                },
                new()
                {
                    MachineId = AlternativeMachineId,
                    MachineFamily = MachineFamily.BlowFilm.ToString(),
                    BusinessUnit = BusinessUnit.Extrusion
                }
            }));

        _machineSnapshotHttpClientMock
            .Setup(s => s.GetStandardDeviations(MachineId, It.IsAny<List<string>>(),
                It.IsAny<List<TimeRange>>(),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new InternalItemResponse<ValueByColumnId<double?>>(firstTupleListOfColumnValues)
            );

        _machineSnapshotHttpClientMock
            .Setup(s => s.GetStandardDeviations(AlternativeMachineId, It.IsAny<List<string>>(),
                It.IsAny<List<TimeRange>>(),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new InternalItemResponse<ValueByColumnId<double?>>(alternativeTupleListOfColumnValues)
            );

        var executor = await InitializeExecutor();
        var job = new JobInfo
        {
            MachineId = MachineId,
            ProductId = "FakeProductId",
            JobId = JobId,
            StartTime = DateTime.UnixEpoch,
            EndTime = DateTime.UnixEpoch.AddHours(1),
            TimeRanges = mockTimeRanges
        };

        var alternativeJob = new JobInfo
        {
            MachineId = AlternativeMachineId,
            ProductId = "FakeProductId",
            JobId = AlternativeJobId,
            StartTime = DateTime.UnixEpoch,
            EndTime = DateTime.UnixEpoch.AddHours(1),
            TimeRanges = mockTimeRanges
        };

        PrepareProductionPeriodsDataHandlerMock(new List<JobInfo> { job, alternativeJob });
        InitializeHttpContextMocks();

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                @"{
                    producedJobs {
                    items {
                        machineId
                        jobSize {
                        standardDeviationValue
                        }
                    }
                    }
                }")
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        var test = result.ToJson();
        test.MatchSnapshot();

        _machineSnapshotHttpClientMock.VerifyAll();
        _machineSnapshotHttpClientMock.VerifyNoOtherCalls();

        _productionPeriodsDataHandlerHttpClientMock.VerifyAll();
        _productionPeriodsDataHandlerHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Get_ValuesWithLongestDuration_With_Two_Columns_And_Same_TimeRanges_Should_Call_SnapShooter_Once()
    {
        // Arrange
        var timeRanges = new List<TimeRange>
            { new(from: new DateTime(year: 2002, month: 5, day: 10), to: new DateTime(year: 2002, month: 5, day: 10)) };

        var tupleListOfColumnValues = new ValueByColumnId<object?>
        {
            { SnapshotColumnIds.JobSize, 1111.1 }
        };

        var job = new JobInfo
        {
            MachineId = MachineId,
            ProductId = "FakeProductId",
            JobId = JobId,
            StartTime = DateTime.UnixEpoch,
            EndTime = DateTime.UnixEpoch.AddHours(1),
            TimeRanges = timeRanges
        };

        _machineCachingServiceMock
            .Setup(m => m.GetMachinesAsInternalListResponse(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalListResponse<Machine>(new List<Machine>
            {
                new()
                {
                    MachineId = MachineId,
                    MachineFamily = MachineFamily.BlowFilm.ToString(),
                    BusinessUnit = BusinessUnit.Extrusion
                }
            }));

        _machineSnapshotHttpClientMock
            .Setup(m => m.GetValuesWithLongestDuration(
                MachineId, It.IsAny<List<string>>(), It.IsAny<List<TimeRange>>(), It.IsAny<List<Filter>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<ValueByColumnId<object?>>(tupleListOfColumnValues));

        PrepareProductionPeriodsDataHandlerMock(new List<JobInfo> { job, job });
        InitializeHttpContextMocks();
        var executor = await InitializeExecutor();

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                @"{
                    producedJobs {
                    items {
                        machineId
                        jobSize {
                        valueWithLongestDuration
                        }
                    }
                    }
                }")
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        _machineSnapshotHttpClientMock.Verify(
            m => m.GetValuesWithLongestDuration(
                MachineId, It.IsAny<List<string>>(), It.IsAny<List<TimeRange>>(), It.IsAny<List<Filter>>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _machineSnapshotHttpClientMock.VerifyAll();
        _machineSnapshotHttpClientMock.VerifyNoOtherCalls();

        _productionPeriodsDataHandlerHttpClientMock.VerifyAll();
        _productionPeriodsDataHandlerHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task
        Get_ValuesWithLongestDuration_With_Two_Columns_And_Different_TimeRanges_Should_Call_SnapShooter_Twice()
    {
        // Arrange
        var timeRanges = new List<TimeRange>
            { new(from: new DateTime(year: 2002, month: 5, day: 10), to: new DateTime(year: 2002, month: 5, day: 10)) };

        var alternativeTimeRanges = new List<TimeRange>
            { new(from: new DateTime(year: 2002, month: 5, day: 10), to: new DateTime(year: 2002, month: 5, day: 11)) };

        var tupleListOfColumnValues = new ValueByColumnId<object?>
        {
            { SnapshotColumnIds.JobSize, 1111.1 }
        };

        var alternativeTupleListOfColumnValues = new ValueByColumnId<object?>
        {
            { SnapshotColumnIds.JobSize, 2222.2}
        };

        var job = new JobInfo
        {
            MachineId = MachineId,
            ProductId = "FakeProductId",
            JobId = JobId,
            StartTime = DateTime.UnixEpoch,
            EndTime = DateTime.UnixEpoch.AddHours(1),
            TimeRanges = timeRanges
        };

        var alternativeJob = new JobInfo
        {
            MachineId = MachineId,
            ProductId = "FakeProductId",
            JobId = JobId,
            StartTime = DateTime.UnixEpoch,
            EndTime = DateTime.UnixEpoch.AddHours(1),
            TimeRanges = alternativeTimeRanges
        };

        _machineCachingServiceMock
            .Setup(m => m.GetMachinesAsInternalListResponse(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalListResponse<Machine>(new List<Machine>
            {
                new()
                {
                    MachineId = MachineId,
                    MachineFamily = MachineFamily.BlowFilm.ToString(),
                    BusinessUnit = BusinessUnit.Extrusion
                }
            }));

        _machineSnapshotHttpClientMock
            .Setup(m => m.GetValuesWithLongestDuration(
                MachineId,
                It.IsAny<List<string>>(),
                It.Is<List<TimeRange>>(list => list.SequenceEqual(timeRanges)),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<ValueByColumnId<object?>>(tupleListOfColumnValues));

        _machineSnapshotHttpClientMock
            .Setup(m => m.GetValuesWithLongestDuration(
                MachineId,
                It.IsAny<List<string>>(),
                It.Is<List<TimeRange>>(list => list.SequenceEqual(alternativeTimeRanges)),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<ValueByColumnId<object?>>(alternativeTupleListOfColumnValues));

        PrepareProductionPeriodsDataHandlerMock(new List<JobInfo> { job, alternativeJob });
        InitializeHttpContextMocks();
        var executor = await InitializeExecutor();

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                @"{
                    producedJobs {
                    items {
                        machineId
                        jobSize {
                        valueWithLongestDuration
                        }
                    }
                    }
                }")
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        _machineSnapshotHttpClientMock.Verify(
            m => m.GetValuesWithLongestDuration(
                MachineId,
                It.IsAny<List<string>>(),
                It.IsAny<List<TimeRange>>(),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()),
            Times.Exactly(2));

        _machineSnapshotHttpClientMock.VerifyAll();
        _machineSnapshotHttpClientMock.VerifyNoOtherCalls();

        _productionPeriodsDataHandlerHttpClientMock.VerifyAll();
        _productionPeriodsDataHandlerHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task
        Get_ValuesWithLongestDuration_With_Same_Columns_And_Different_TimeRanges_Should_Call_SnapshotClient_Each_Time()
    {
        // Arrange
        var firstTimeRanges = new List<TimeRange>
        {
            new(from: new DateTime(year: 2002, month: 5, day: 10), to: new DateTime(year: 2002, month: 5, day: 10)),
            new(from: new DateTime(year: 2002, month: 5, day: 12), to: new DateTime(year: 2002, month: 5, day: 13))
        };

        var secondTimeRanges = new List<TimeRange>
            { new(from: new DateTime(year: 2002, month: 5, day: 10), to: new DateTime(year: 2002, month: 5, day: 11)) };

        var thirdTimeRanges = new List<TimeRange>
            { new(from: new DateTime(year: 2002, month: 5, day: 11), to: new DateTime(year: 2002, month: 5, day: 12)) };

        var tupleListOfColumnValues = new ValueByColumnId<object?>
        {
            { SnapshotColumnIds.JobSize, 1111.1 }
        };

        var firstJob = new JobInfo
        {
            MachineId = MachineId,
            ProductId = "FakeProductId",
            JobId = JobId,
            StartTime = DateTime.UnixEpoch,
            EndTime = DateTime.UnixEpoch.AddHours(1),
            TimeRanges = firstTimeRanges
        };

        var secondJob = new JobInfo
        {
            MachineId = MachineId,
            ProductId = "FakeProductId",
            JobId = JobId,
            StartTime = DateTime.UnixEpoch,
            EndTime = DateTime.UnixEpoch.AddHours(1),
            TimeRanges = secondTimeRanges
        };

        var thirdJob = new JobInfo
        {
            MachineId = MachineId,
            ProductId = "FakeProductId",
            JobId = JobId,
            StartTime = DateTime.UnixEpoch,
            EndTime = DateTime.UnixEpoch.AddHours(1),
            TimeRanges = thirdTimeRanges
        };

        _machineCachingServiceMock
            .Setup(m => m.GetMachinesAsInternalListResponse(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalListResponse<Machine>(new List<Machine>
            {
                new()
                {
                    MachineId = MachineId,
                    MachineFamily = MachineFamily.BlowFilm.ToString(),
                    BusinessUnit = BusinessUnit.Extrusion
                }
            }));

        _machineSnapshotHttpClientMock
            .Setup(m => m.GetValuesWithLongestDuration(
                MachineId,
                It.IsAny<List<string>>(),
                It.IsAny<List<TimeRange>>(),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<ValueByColumnId<object?>>(tupleListOfColumnValues));

        var executor = await InitializeExecutor();

        PrepareProductionPeriodsDataHandlerMock(new List<JobInfo> { firstJob, secondJob, thirdJob });
        InitializeHttpContextMocks();

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                @"{
                    producedJobs {
                    items {
                        machineId
                        jobSize {
                        valueWithLongestDuration
                        }
                    }
                    }
                }")
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        _machineSnapshotHttpClientMock.Verify(
            m => m.GetValuesWithLongestDuration(
                MachineId,
                It.IsAny<List<string>>(),
                It.IsAny<List<TimeRange>>(),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()), Times.Exactly(3));

        _machineSnapshotHttpClientMock.VerifyAll();
        _machineSnapshotHttpClientMock.VerifyNoOtherCalls();

        _productionPeriodsDataHandlerHttpClientMock.VerifyAll();
        _productionPeriodsDataHandlerHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task
        Get_ValuesWithLongestDuration_For_Two_Machines_With_Same_Column_Should_Return_Corresponding_Value()
    {
        // Arrange
        var timeRanges = new List<TimeRange>
            { new(from: new DateTime(year: 2002, month: 5, day: 10), to: new DateTime(year: 2002, month: 5, day: 10)) };

        var firstTupleListOfColumnValues = new ValueByColumnId<object?>
        {
            { SnapshotColumnIds.JobSize, 1111.1 },
        };

        var alternativeTupleListOfColumnValues = new ValueByColumnId<object?>
        {
            { SnapshotColumnIds.JobSize, 2222.2}
        };

        var job = new JobInfo
        {
            MachineId = MachineId,
            ProductId = "FakeProductId",
            JobId = JobId,
            StartTime = DateTime.UnixEpoch,
            EndTime = DateTime.UnixEpoch.AddHours(1),
            TimeRanges = timeRanges
        };

        var alternativeJob = new JobInfo
        {
            MachineId = AlternativeMachineId,
            ProductId = "FakeProductId",
            JobId = AlternativeJobId,
            StartTime = DateTime.UnixEpoch,
            EndTime = DateTime.UnixEpoch.AddHours(1),
            TimeRanges = timeRanges
        };

        _machineCachingServiceMock
            .Setup(m => m.GetMachinesAsInternalListResponse(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalListResponse<Machine>(new List<Machine>
            {
                new()
                {
                    MachineId = MachineId,
                    MachineFamily = MachineFamily.FlexoPrint.ToString(),
                    BusinessUnit = BusinessUnit.Extrusion
                },
                new()
                {
                    MachineId = AlternativeMachineId,
                    MachineFamily = MachineFamily.BlowFilm.ToString(),
                    BusinessUnit = BusinessUnit.Extrusion
                }
            }));

        _machineSnapshotHttpClientMock
            .Setup(m => m.GetValuesWithLongestDuration(
                MachineId, It.IsAny<List<string>>(), It.IsAny<List<TimeRange>>(), It.IsAny<List<Filter>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<ValueByColumnId<object?>>(firstTupleListOfColumnValues));

        _machineSnapshotHttpClientMock
            .Setup(m => m.GetValuesWithLongestDuration(
                AlternativeMachineId,
                It.IsAny<List<string>>(),
                It.IsAny<List<TimeRange>>(),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<ValueByColumnId<object?>>(alternativeTupleListOfColumnValues));

        var executor = await InitializeExecutor();

        PrepareProductionPeriodsDataHandlerMock(new List<JobInfo> { job, alternativeJob });
        InitializeHttpContextMocks();

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                @"{
                    producedJobs {
                    items {
                        machineId
                        jobSize {
                        valueWithLongestDuration
                        }
                    }
                    }
                }")
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        _machineSnapshotHttpClientMock.VerifyAll();
        _machineSnapshotHttpClientMock.VerifyNoOtherCalls();

        _productionPeriodsDataHandlerHttpClientMock.VerifyAll();
        _productionPeriodsDataHandlerHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Get_ArithmeticMeans_With_Two_Columns_And_Same_TimeRanges_Should_Call_SnapShooter_Once()
    {
        // Arrange
        var timeRanges = new List<TimeRange>
            { new(from: new DateTime(year: 2002, month: 5, day: 10), to: new DateTime(year: 2002, month: 5, day: 10)) };

        var tupleListOfColumnValues = new ValueByColumnId<double?>
        {
            { SnapshotColumnIds.JobSize, 1111.1 }
        };

        var job = new JobInfo
        {
            MachineId = MachineId,
            ProductId = "FakeProductId",
            JobId = JobId,
            StartTime = DateTime.UnixEpoch,
            EndTime = DateTime.UnixEpoch.AddHours(1),
            TimeRanges = timeRanges
        };

        _machineCachingServiceMock
            .Setup(m => m.GetMachinesAsInternalListResponse(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalListResponse<Machine>(new List<Machine>
            {
                new()
                {
                    MachineId = MachineId,
                    MachineFamily = MachineFamily.BlowFilm.ToString(),
                    BusinessUnit = BusinessUnit.Extrusion
                }
            }));

        _machineSnapshotHttpClientMock
            .Setup(m => m.GetArithmeticMeans(
                MachineId, It.IsAny<List<string>>(), It.IsAny<List<TimeRange>>(), It.IsAny<List<Filter>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<ValueByColumnId<double?>>(tupleListOfColumnValues));

        PrepareProductionPeriodsDataHandlerMock(new List<JobInfo> { job, job });
        InitializeHttpContextMocks();
        var executor = await InitializeExecutor();

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                @"{
                    producedJobs {
                    items {
                        machineId
                        jobSize {
                        averageValue
                        }
                    }
                    }
                }")
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        _machineSnapshotHttpClientMock.Verify(
            m => m.GetArithmeticMeans(
                MachineId, It.IsAny<List<string>>(), It.IsAny<List<TimeRange>>(), It.IsAny<List<Filter>>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _machineSnapshotHttpClientMock.VerifyAll();
        _machineSnapshotHttpClientMock.VerifyNoOtherCalls();

        _productionPeriodsDataHandlerHttpClientMock.VerifyAll();
        _productionPeriodsDataHandlerHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Get_ArithmeticMeans_With_Two_Columns_And_Different_TimeRanges_Should_Call_SnapShooter_Twice()
    {
        // Arrange
        var timeRanges = new List<TimeRange>
            { new(from: new DateTime(year: 2002, month: 5, day: 10), to: new DateTime(year: 2002, month: 5, day: 10)) };

        var alternativeTimeRanges = new List<TimeRange>
            { new(from: new DateTime(year: 2002, month: 5, day: 10), to: new DateTime(year: 2002, month: 5, day: 11)) };

        var tupleListOfColumnValues = new ValueByColumnId<double?>
        {
            { SnapshotColumnIds.JobSize, 1111.1 }
        };

        var alternativeTupleListOfColumnValues = new ValueByColumnId<double?>
        {
            { SnapshotColumnIds.JobSize, 2222.2}
        };

        var job = new JobInfo
        {
            MachineId = MachineId,
            ProductId = "FakeProductId",
            JobId = JobId,
            StartTime = DateTime.UnixEpoch,
            EndTime = DateTime.UnixEpoch.AddHours(1),
            TimeRanges = timeRanges
        };

        var alternativeJob = new JobInfo
        {
            MachineId = MachineId,
            ProductId = "FakeProductId",
            JobId = JobId,
            StartTime = DateTime.UnixEpoch,
            EndTime = DateTime.UnixEpoch.AddHours(1),
            TimeRanges = alternativeTimeRanges
        };

        _machineCachingServiceMock
            .Setup(m => m.GetMachinesAsInternalListResponse(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalListResponse<Machine>(new List<Machine>
            {
                new()
                {
                    MachineId = MachineId,
                    MachineFamily = MachineFamily.BlowFilm.ToString(),
                    BusinessUnit = BusinessUnit.Extrusion
                }
            }));

        _machineSnapshotHttpClientMock
            .Setup(m => m.GetArithmeticMeans(
                MachineId,
                It.IsAny<List<string>>(),
                It.Is<List<TimeRange>>(list => list.SequenceEqual(timeRanges)),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<ValueByColumnId<double?>>(tupleListOfColumnValues));

        _machineSnapshotHttpClientMock
            .Setup(m => m.GetArithmeticMeans(
                MachineId,
                It.IsAny<List<string>>(),
                It.Is<List<TimeRange>>(list => list.SequenceEqual(alternativeTimeRanges)),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<ValueByColumnId<double?>>(alternativeTupleListOfColumnValues));

        PrepareProductionPeriodsDataHandlerMock(new List<JobInfo> { job, alternativeJob });
        InitializeHttpContextMocks();
        var executor = await InitializeExecutor();

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                @"{
                    producedJobs {
                    items {
                        machineId
                        jobSize {
                        averageValue
                        }
                    }
                    }
                }")
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        _machineSnapshotHttpClientMock.Verify(
            m => m.GetArithmeticMeans(
                MachineId,
                It.IsAny<List<string>>(),
                It.IsAny<List<TimeRange>>(),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()),
            Times.Exactly(2));

        _machineSnapshotHttpClientMock.VerifyAll();
        _machineSnapshotHttpClientMock.VerifyNoOtherCalls();

        _productionPeriodsDataHandlerHttpClientMock.VerifyAll();
        _productionPeriodsDataHandlerHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Get_ArithmeticMeans_With_Same_Columns_And_Different_TimeRanges_Should_Call_SnapshotClient_Each_Time()
    {
        // Arrange
        var firstTimeRanges = new List<TimeRange>
        {
            new(from: new DateTime(year: 2002, month: 5, day: 10), to: new DateTime(year: 2002, month: 5, day: 10)),
            new(from: new DateTime(year: 2002, month: 5, day: 12), to: new DateTime(year: 2002, month: 5, day: 13))
        };

        var secondTimeRanges = new List<TimeRange>
            { new(from: new DateTime(year: 2002, month: 5, day: 10), to: new DateTime(year: 2002, month: 5, day: 11)) };

        var thirdTimeRanges = new List<TimeRange>
            { new(from: new DateTime(year: 2002, month: 5, day: 11), to: new DateTime(year: 2002, month: 5, day: 12)) };

        var tupleListOfColumnValues = new ValueByColumnId<double?>
        {
            { SnapshotColumnIds.JobSize, 1111.1 }
        };

        var firstJob = new JobInfo
        {
            MachineId = MachineId,
            ProductId = "FakeProductId",
            JobId = JobId,
            StartTime = DateTime.UnixEpoch,
            EndTime = DateTime.UnixEpoch.AddHours(1),
            TimeRanges = firstTimeRanges
        };

        var secondJob = new JobInfo
        {
            MachineId = MachineId,
            ProductId = "FakeProductId",
            JobId = JobId,
            StartTime = DateTime.UnixEpoch,
            EndTime = DateTime.UnixEpoch.AddHours(1),
            TimeRanges = secondTimeRanges
        };

        var thirdJob = new JobInfo
        {
            MachineId = MachineId,
            ProductId = "FakeProductId",
            JobId = JobId,
            StartTime = DateTime.UnixEpoch,
            EndTime = DateTime.UnixEpoch.AddHours(1),
            TimeRanges = thirdTimeRanges
        };

        _machineCachingServiceMock
            .Setup(m => m.GetMachinesAsInternalListResponse(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalListResponse<Machine>(new List<Machine>
            {
                new()
                {
                    MachineId = MachineId,
                    MachineFamily = MachineFamily.BlowFilm.ToString(),
                    BusinessUnit = BusinessUnit.Extrusion
                }
            }));

        _machineSnapshotHttpClientMock
            .Setup(m => m.GetArithmeticMeans(
                MachineId,
                It.IsAny<List<string>>(),
                It.IsAny<List<TimeRange>>(),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<ValueByColumnId<double?>>(tupleListOfColumnValues));

        var executor = await InitializeExecutor();

        PrepareProductionPeriodsDataHandlerMock(new List<JobInfo> { firstJob, secondJob, thirdJob });
        InitializeHttpContextMocks();

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                @"{
                    producedJobs {
                    items {
                        machineId
                        jobSize {
                        averageValue
                        }
                    }
                    }
                }")
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        _machineSnapshotHttpClientMock.Verify(
            m => m.GetArithmeticMeans(
                MachineId,
                It.IsAny<List<string>>(),
                It.IsAny<List<TimeRange>>(),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()), Times.Exactly(3));

        _machineSnapshotHttpClientMock.VerifyAll();
        _machineSnapshotHttpClientMock.VerifyNoOtherCalls();

        _productionPeriodsDataHandlerHttpClientMock.VerifyAll();
        _productionPeriodsDataHandlerHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Get_ArithmeticMeans_For_Two_Machines_With_Same_Column_Should_Return_Corresponding_Value()
    {
        // Arrange
        var timeRanges = new List<TimeRange>
            { new(from: new DateTime(year: 2002, month: 5, day: 10), to: new DateTime(year: 2002, month: 5, day: 10)) };

        var firstTupleListOfColumnValues = new ValueByColumnId<double?>
        {
            { SnapshotColumnIds.JobSize, 1111.1 },
        };

        var alternativeTupleListOfColumnValues = new ValueByColumnId<double?>
        {
            { SnapshotColumnIds.JobSize, 2222.2}
        };

        var job = new JobInfo
        {
            MachineId = MachineId,
            ProductId = "FakeProductId",
            JobId = JobId,
            StartTime = DateTime.UnixEpoch,
            EndTime = DateTime.UnixEpoch.AddHours(1),
            TimeRanges = timeRanges
        };

        var alternativeJob = new JobInfo
        {
            MachineId = AlternativeMachineId,
            ProductId = "FakeProductId",
            JobId = AlternativeJobId,
            StartTime = DateTime.UnixEpoch,
            EndTime = DateTime.UnixEpoch.AddHours(1),
            TimeRanges = timeRanges
        };

        _machineCachingServiceMock
            .Setup(m => m.GetMachinesAsInternalListResponse(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalListResponse<Machine>(new List<Machine>
            {
                new()
                {
                    MachineId = MachineId,
                    MachineFamily = MachineFamily.FlexoPrint.ToString(),
                    BusinessUnit = BusinessUnit.Extrusion
                },
                new()
                {
                    MachineId = AlternativeMachineId,
                    MachineFamily = MachineFamily.BlowFilm.ToString(),
                    BusinessUnit = BusinessUnit.Extrusion
                }
            }));

        _machineSnapshotHttpClientMock
            .Setup(m => m.GetArithmeticMeans(
                MachineId, It.IsAny<List<string>>(), It.IsAny<List<TimeRange>>(), It.IsAny<List<Filter>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<ValueByColumnId<double?>>(firstTupleListOfColumnValues));

        _machineSnapshotHttpClientMock
            .Setup(m => m.GetArithmeticMeans(
                AlternativeMachineId,
                It.IsAny<List<string>>(),
                It.IsAny<List<TimeRange>>(),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<ValueByColumnId<double?>>(alternativeTupleListOfColumnValues));

        var executor = await InitializeExecutor();

        PrepareProductionPeriodsDataHandlerMock(new List<JobInfo> { job, alternativeJob });
        InitializeHttpContextMocks();

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                @"{
                    producedJobs {
                    items {
                        machineId
                        jobSize {
                        averageValue
                        }
                    }
                    }
                }")
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        _machineSnapshotHttpClientMock.VerifyAll();
        _machineSnapshotHttpClientMock.VerifyNoOtherCalls();

        _productionPeriodsDataHandlerHttpClientMock.VerifyAll();
        _productionPeriodsDataHandlerHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Get_DistinctValues_With_Two_Columns_And_Different_TimeRanges_Should_Call_SnapShooter_Twice()
    {
        // Arrange
        var timeRanges = new List<TimeRange>
            { new(from: new DateTime(year: 2002, month: 5, day: 10), to: new DateTime(year: 2002, month: 5, day: 10)) };

        var alternativeTimeRanges = new List<TimeRange>
            { new(from: new DateTime(year: 2002, month: 5, day: 10), to: new DateTime(year: 2002, month: 5, day: 11)) };

        var dictionaryOfColumnValues = new ValueByColumnId<List<object?>>
        {
            {
                SnapshotColumnIds.JobSize, new List<object?> { 1111.1, 1112.2 }
            }
        };

        var alternativeDictionaryOfColumnValues = new ValueByColumnId<List<object?>>
        {
            {
                SnapshotColumnIds.JobSize, new List<object?> { 2222.2, 2223.3 }
            }
        };

        var executor = await InitializeExecutor();
        var job = new JobInfo
        {
            MachineId = MachineId,
            ProductId = "FakeProductId",
            JobId = JobId,
            StartTime = DateTime.UnixEpoch,
            EndTime = DateTime.UnixEpoch.AddHours(1),
            TimeRanges = timeRanges
        };

        var alternativeJob = new JobInfo
        {
            MachineId = MachineId,
            ProductId = "FakeProductId",
            JobId = JobId,
            StartTime = DateTime.UnixEpoch,
            EndTime = DateTime.UnixEpoch.AddHours(1),
            TimeRanges = alternativeTimeRanges
        };

        _machineCachingServiceMock
            .Setup(m => m.GetMachinesAsInternalListResponse(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalListResponse<Machine>(new List<Machine>
            {
                new()
                {
                    MachineId = MachineId,
                    MachineFamily = MachineFamily.BlowFilm.ToString(),
                    BusinessUnit = BusinessUnit.Extrusion
                }
            }));

        _machineSnapshotHttpClientMock
            .Setup(m => m.GetDistinctValues(
                MachineId,
                It.IsAny<List<string>>(),
                It.Is<List<TimeRange>>(list => list.SequenceEqual(timeRanges)),
                It.IsAny<int>(),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<ValueByColumnId<List<object?>>>(dictionaryOfColumnValues));

        _machineSnapshotHttpClientMock
            .Setup(m => m.GetDistinctValues(
                MachineId,
                It.IsAny<List<string>>(),
                It.Is<List<TimeRange>>(list => list.SequenceEqual(alternativeTimeRanges)),
                It.IsAny<int>(),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new InternalItemResponse<ValueByColumnId<List<object?>>>(alternativeDictionaryOfColumnValues));

        PrepareProductionPeriodsDataHandlerMock(new List<JobInfo> { job, alternativeJob });
        InitializeHttpContextMocks();

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                @"{
                    producedJobs {
                    items {
                        machineId
                        jobSize {
                        distinctValues
                        }
                    }
                    }
                }")
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        _machineSnapshotHttpClientMock.Verify(
            m => m.GetDistinctValues(
                MachineId,
                It.IsAny<List<string>>(),
                It.IsAny<List<TimeRange>>(),
                It.IsAny<int>(),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()), Times.Exactly(2));

        _machineSnapshotHttpClientMock.VerifyAll();
        _machineSnapshotHttpClientMock.VerifyNoOtherCalls();

        _productionPeriodsDataHandlerHttpClientMock.VerifyAll();
        _productionPeriodsDataHandlerHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task
        Get_DistinctValues_With_Same_Columns_And_Same_TimeRanges_But_Different_Limits_Should_Call_SnapShooter_Twice()
    {
        const int firstLimit = 1;
        const int secondLimit = 2;

        // Arrange
        var timeRanges = new List<TimeRange>
            { new(from: new DateTime(year: 2002, month: 5, day: 10), to: new DateTime(year: 2002, month: 5, day: 10)) };

        var dictionaryOfColumnValues = new ValueByColumnId<List<object?>>
        {
            {
                SnapshotColumnIds.JobSize, new List<object?> { 1111.1 }
            }
        };

        var alternativeDictionaryOfColumnValues = new ValueByColumnId<List<object?>>
        {
            {
                SnapshotColumnIds.JobSize, new List<object?> { 2222.2, 2223.3 }
            }
        };

        var executor = await InitializeExecutor();
        var job = new JobInfo
        {
            MachineId = MachineId,
            ProductId = "FakeProductId",
            JobId = JobId,
            StartTime = DateTime.UnixEpoch,
            EndTime = DateTime.UnixEpoch.AddHours(1),
            TimeRanges = timeRanges
        };

        _machineCachingServiceMock
            .Setup(m => m.GetMachinesAsInternalListResponse(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalListResponse<Machine>(new List<Machine>
            {
                new()
                {
                    MachineId = MachineId,
                    MachineFamily = MachineFamily.BlowFilm.ToString(),
                    BusinessUnit = BusinessUnit.Extrusion
                }
            }));

        _machineSnapshotHttpClientMock
            .Setup(m => m.GetDistinctValues(
                MachineId,
                It.IsAny<List<string>>(),
                It.IsAny<List<TimeRange>>(),
                It.Is<int>(value => value.Equals(firstLimit)),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<ValueByColumnId<List<object?>>>(dictionaryOfColumnValues));

        _machineSnapshotHttpClientMock
            .Setup(m => m.GetDistinctValues(
                MachineId,
                It.IsAny<List<string>>(),
                It.IsAny<List<TimeRange>>(),
                It.Is<int>(value => value.Equals(secondLimit)),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new InternalItemResponse<ValueByColumnId<List<object?>>>(alternativeDictionaryOfColumnValues));

        PrepareProductionPeriodsDataHandlerMock(new List<JobInfo> { job });
        InitializeHttpContextMocks();

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                @"{
                    fakeJob1: producedJobs {
                    items {
                        machineId
                        jobSize {
                        distinctValues(limit: 1)
                        }
                    }
                    }
                    fakeJob2: producedJobs {
                    items {
                        machineId
                        jobSize {
                        distinctValues(limit: 2)
                        }
                    }
                    }
                }")
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        _machineSnapshotHttpClientMock.Verify(
            m => m.GetDistinctValues(
                MachineId,
                It.IsAny<List<string>>(),
                It.IsAny<List<TimeRange>>(),
                It.IsAny<int>(),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()),
            Times.Exactly(2));

        _machineSnapshotHttpClientMock.VerifyAll();
        _machineSnapshotHttpClientMock.VerifyNoOtherCalls();

        _productionPeriodsDataHandlerHttpClientMock.VerifyAll();
        _productionPeriodsDataHandlerHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task
        Get_DistinctValues_With_Same_Columns_And_Different_TimeRanges_Should_Call_SnapshotClient_Each_Time()
    {
        // Arrange
        var firstTimeRanges = new List<TimeRange>
        {
            new(
                new DateTime(2002, 5, 10),
                new DateTime(2002, 5, 10)
            ),
            new(
                new DateTime(2002, 5, 12),
                new DateTime(2002, 5, 13)
            )
        };

        var secondTimeRanges = new List<TimeRange>
        {
            new(
                new DateTime(2002, 5, 10),
                new DateTime(2002, 5, 11)
            )
        };

        var thirdTimeRanges = new List<TimeRange>
        {
            new(
                new DateTime(2002, 5, 11),
                new DateTime(2002, 5, 12)
            )
        };

        var tupleListOfColumnValues = new ValueByColumnId<List<object?>>
        {
            {
                SnapshotColumnIds.JobSize, new List<object?> { 1111.1 }
            }
        };

        var firstJob = new JobInfo
        {
            MachineId = MachineId,
            ProductId = "FakeProductId",
            JobId = JobId,
            StartTime = DateTime.UnixEpoch,
            EndTime = DateTime.UnixEpoch.AddHours(1),
            TimeRanges = firstTimeRanges
        };

        var secondJob = new JobInfo
        {
            MachineId = MachineId,
            ProductId = "FakeProductId",
            JobId = JobId,
            StartTime = DateTime.UnixEpoch,
            EndTime = DateTime.UnixEpoch.AddHours(1),
            TimeRanges = secondTimeRanges
        };

        var thirdJob = new JobInfo
        {
            MachineId = MachineId,
            ProductId = "FakeProductId",
            JobId = JobId,
            StartTime = DateTime.UnixEpoch,
            EndTime = DateTime.UnixEpoch.AddHours(1),
            TimeRanges = thirdTimeRanges
        };

        _machineCachingServiceMock
            .Setup(m => m.GetMachinesAsInternalListResponse(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalListResponse<Machine>(new List<Machine>
            {
                new()
                {
                    MachineId = MachineId,
                    MachineFamily = MachineFamily.BlowFilm.ToString(),
                    BusinessUnit = BusinessUnit.Extrusion
                }
            }));

        _machineSnapshotHttpClientMock
            .Setup(m => m.GetDistinctValues(
                MachineId,
                It.IsAny<List<string>>(),
                It.IsAny<List<TimeRange>>(),
                It.IsAny<int>(),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<ValueByColumnId<List<object?>>>(tupleListOfColumnValues));

        PrepareProductionPeriodsDataHandlerMock(new List<JobInfo> { firstJob, secondJob, thirdJob });
        InitializeHttpContextMocks();
        var executor = await InitializeExecutor();

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                @"{
                    producedJobs {
                    items {
                        machineId
                        jobSize {
                        distinctValues
                        }
                    }
                    }
                }")
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        _machineSnapshotHttpClientMock.Verify(
            m => m.GetDistinctValues(
                MachineId,
                It.IsAny<List<string>>(),
                It.IsAny<List<TimeRange>>(),
                It.IsAny<int>(),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()),
            Times.Exactly(3));

        _machineSnapshotHttpClientMock.VerifyAll();
        _machineSnapshotHttpClientMock.VerifyNoOtherCalls();

        _productionPeriodsDataHandlerHttpClientMock.VerifyAll();
        _productionPeriodsDataHandlerHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Get_DistinctValues_For_Two_Machines_With_Same_Column_Should_Return_Corresponding_Value()
    {
        // Arrange
        var timeRanges = new List<TimeRange>
            { new(from: new DateTime(year: 2002, month: 5, day: 10), to: new DateTime(year: 2002, month: 5, day: 10)) };

        var firstDictionaryOfColumnValues = new ValueByColumnId<List<object?>>
        {
            {
                SnapshotColumnIds.JobSize, new List<object?> { 1111.1, 2222.2 }
            }
        };

        var alternativeDictionaryOfColumnValues = new ValueByColumnId<List<object?>>
        {
            {
                SnapshotColumnIds.JobSize, new List<object?> { 3333.3, 4444.4, 5555.5 }
            }
        };

        _machineCachingServiceMock
            .Setup(m => m.GetMachinesAsInternalListResponse(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalListResponse<Machine>(new List<Machine>
            {
                new()
                {
                    MachineId = MachineId,
                    MachineFamily = MachineFamily.FlexoPrint.ToString(),
                    BusinessUnit = BusinessUnit.Extrusion
                },
                new()
                {
                    MachineId = AlternativeMachineId,
                    MachineFamily = MachineFamily.BlowFilm.ToString(),
                    BusinessUnit = BusinessUnit.Extrusion
                }
            }));

        var job = new JobInfo
        {
            MachineId = MachineId,
            ProductId = "FakeProductId",
            JobId = JobId,
            StartTime = DateTime.UnixEpoch,
            EndTime = DateTime.UnixEpoch.AddHours(1),
            TimeRanges = timeRanges
        };

        var alternativeJob = new JobInfo
        {
            MachineId = AlternativeMachineId,
            ProductId = "FakeProductId",
            JobId = AlternativeJobId,
            StartTime = DateTime.UnixEpoch,
            EndTime = DateTime.UnixEpoch.AddHours(1),
            TimeRanges = timeRanges
        };

        _machineSnapshotHttpClientMock
            .Setup(m => m.GetDistinctValues(
                MachineId,
                It.IsAny<List<string>>(),
                It.IsAny<List<TimeRange>>(),
                It.IsAny<int>(),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<ValueByColumnId<List<object?>>>(firstDictionaryOfColumnValues));

        _machineSnapshotHttpClientMock
            .Setup(m => m.GetDistinctValues(
                AlternativeMachineId,
                It.IsAny<List<string>>(),
                It.IsAny<List<TimeRange>>(),
                It.IsAny<int>(),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new InternalItemResponse<ValueByColumnId<List<object?>>>(alternativeDictionaryOfColumnValues));

        var executor = await InitializeExecutor();

        PrepareProductionPeriodsDataHandlerMock(new List<JobInfo> { job, alternativeJob });
        InitializeHttpContextMocks();

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                @"{
                    producedJobs {
                    items {
                        machineId
                        jobSize {
                        distinctValues
                        }
                    }
                    }
                }")
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        _machineSnapshotHttpClientMock.VerifyAll();
        _machineSnapshotHttpClientMock.VerifyNoOtherCalls();

        _productionPeriodsDataHandlerHttpClientMock.VerifyAll();
        _productionPeriodsDataHandlerHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Get_Jobs_With_Base_Properties_By_MachineIdFilter()
    {
        // Arrange
        var executor = await InitializeExecutor();
        var job = new JobInfo
        {
            MachineId = MachineId,
            ProductId = "FakeProductId",
            JobId = JobId,
            StartTime = DateTime.UnixEpoch
        };

        _machineCachingServiceMock
            .Setup(m => m.GetMachinesAsInternalListResponse(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalListResponse<Machine>(new List<Machine>
            {
                new()
                {
                    MachineId = MachineId,
                    MachineFamily = MachineFamily.FlexoPrint.ToString(),
                    BusinessUnit = BusinessUnit.Extrusion
                }
            }));
        PrepareProductionPeriodsDataHandlerMock(new List<JobInfo> { job });

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    producedJobs(machineIdFilter: ""{MachineId}"") {{
                        items {{
                        machineId
                        jobId
                        endTime
                        isActive
                        startTime
                        productId
                        uniqueId
                        }}
                    }}
                }}")
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        _machineCachingServiceMock.VerifyAll();
        _machineCachingServiceMock.VerifyNoOtherCalls();

        _productionPeriodsDataHandlerHttpClientMock.VerifyAll();
        _productionPeriodsDataHandlerHttpClientMock.VerifyNoOtherCalls();
    }

    private void PrepareProductionPeriodsDataHandlerMock(List<JobInfo> jobInfos)
    {
        _productionPeriodsDataHandlerHttpClientMock
            .Setup(m => m.GetAllJobInfos(
                It.IsAny<CancellationToken>(),
                It.IsAny<List<string>>(),
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                true))
            .ReturnsAsync(new InternalListResponse<JobInfo>(jobInfos));

        _productionPeriodsDataHandlerHttpClientMock
            .Setup(m => m.GetAllJobIds(
                It.IsAny<CancellationToken>(),
                It.IsAny<List<string>>(),
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<string>()))
            .ReturnsAsync(
                new InternalListResponse<string>(Enumerable.Range(1, 100).Select(i => i.ToString()).ToList()));
    }

    private void InitializeHttpContextMocks()
    {
        _httpContextMock
            .SetupGet(m => m.Request.Method)
            .Returns("POST");
        _httpContextMock
            .SetupGet(m => m.WebSockets.IsWebSocketRequest)
            .Returns(false);
        _httpContextAccessorMock
            .SetupGet(m => m.HttpContext)
            .Returns(_httpContextMock.Object);
    }

    private async Task<IRequestExecutor> InitializeExecutor()
    {
        var machineService = new MachineService(
            _machineCachingServiceMock.Object, new Mock<ILogger<MachineService>>().Object);
        var machineSnapshotService = new MachineSnapshotService();
        var machineTimeService = new MachineTimeService(
            new OpcUaServerTimeCachingService(
                _processDataReaderHttpClientMock.Object, new Mock<IProcessDataQueueWrapper>().Object),
            _latestMachineSnapshotCachingServiceMock.Object,
            new Mock<ILogger<MachineTimeService>>().Object);
        var jobInfoCachingService = new JobInfoCachingService(
            machineTimeService,
            _productionPeriodsDataHandlerHttpClientMock.Object,
            _productionPeriodChangesQueueWrapper.Object,
            new Mock<ILogger<JobInfoCachingService>>().Object);
        var producedJobService = new ProducedJobService(
            jobInfoCachingService,
            _productionPeriodsDataHandlerHttpClientMock.Object,
            machineService,
            new Mock<ILogger<ProducedJobService>>().Object,
            new Mock<IKpiEventQueueWrapper>().Object);

        return await new ServiceCollection()
            .AddSingleton<IMachineService>(machineService)
            .AddSingleton<IProducedJobService>(producedJobService)
            .AddSingleton<IMachineSnapshotService>(machineSnapshotService)
            .AddSingleton(_machineSnapshotHttpClientMock.Object)
            .AddSingleton(_httpContextAccessorMock.Object)
            .AddLogging()
            .AddGraphQLServer()
            .AddAuthorization()
            .ModifyOptions(t => t.StrictValidation = false)
            .AddQueryType(q => q.Name("Query"))
            .AddType<FrameworkAPI.Queries.ProducedJobQuery>()
            .AddType<ExtrusionProducedJob>()
            .BuildRequestExecutorAsync();
    }
}