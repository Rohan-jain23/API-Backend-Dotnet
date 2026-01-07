using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.DataLoaders;
using FrameworkAPI.Schema.ProducedJob;
using FrameworkAPI.Services;
using FrameworkAPI.Services.Interfaces;
using FrameworkAPI.Test.Services.Helpers;
using FrameworkAPI.Test.TestHelpers;
using GreenDonut;
using HotChocolate;
using HotChocolate.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Npgsql;
using Snapshooter.Xunit;
using WuH.Ruby.Common.Core;
using WuH.Ruby.KpiDataHandler.Client;
using WuH.Ruby.MachineDataHandler.Client;
using WuH.Ruby.MachineSnapShooter.Client;
using WuH.Ruby.MachineSnapShooter.Client.Models;
using WuH.Ruby.MetaDataHandler.Client;
using WuH.Ruby.OpcUaForwarder.Client;
using WuH.Ruby.ProcessDataReader.Client;
using WuH.Ruby.ProductionPeriodsDataHandler.Client;
using Xunit;
using Machine = WuH.Ruby.MachineDataHandler.Client.Machine;
using MachineFamily = FrameworkAPI.Schema.Misc.MachineFamily;

namespace FrameworkAPI.Test.Queries.ProducedJobQuery;

public class ProducedJobQueryIntegrationTests
{
    private const string MachineId = "EQ12345";
    private const string ProductId = "FakeProductId";
    private const string JobId = "FakeJobId";
    private const string Customer = "FakeCustomer";
    private readonly ValueByColumnId<double?> _arbitraryNumericValueByColumnId = new() { { SnapshotColumnIds.JobSize, 1111.1 } };
    private static readonly List<TimeRange> MockTimeRanges =
        [new TimeRange(new DateTime(2002, 5, 10), new DateTime(2002, 5, 10))];
    private readonly JobInfo _baseJob = new()
    {
        MachineId = MachineId,
        ProductId = ProductId,
        JobId = JobId,
        StartTime = DateTime.UnixEpoch,
        EndTime = DateTime.UnixEpoch.AddHours(1)
    };
    private readonly JobInfo _baseJobWithNoEndTime = new()
    {
        MachineId = MachineId,
        ProductId = ProductId,
        JobId = JobId,
        StartTime = DateTime.UnixEpoch
    };
    private readonly JobInfo _baseJobWithTimeRanges = new()
    {
        MachineId = MachineId,
        ProductId = ProductId,
        JobId = JobId,
        StartTime = DateTime.UnixEpoch,
        EndTime = DateTime.UnixEpoch.AddHours(1),
        TimeRanges = MockTimeRanges
    };
    private readonly JobInfo _baseJobWithEmptyTimeRanges = new()
    {
        MachineId = MachineId,
        ProductId = ProductId,
        JobId = JobId,
        StartTime = DateTime.UnixEpoch,
        EndTime = DateTime.UnixEpoch.AddHours(1),
        TimeRanges = null
    };

    private readonly Mock<IMachineSnapshotHttpClient> _machineSnapshotHttpClientMock = new();
    private readonly Mock<IMetaDataHandlerHttpClient> _metaDataHandlerHttpClientMock = new();
    private readonly Mock<ILatestMachineSnapshotCachingService> _latestMachineSnapshotCachingServiceMock = new();
    private readonly Mock<IKpiDataCachingService> _kpiDataCachingServiceMock = new();
    private readonly Mock<IKpiDataHandlerClient> _kpiDataHandlerClientMock = new();
    private readonly Mock<IMachineCachingService> _machineCachingServiceMock = new();
    private readonly Mock<IProcessDataReaderHttpClient> _processDataReaderHttpClientMock = new();
    private readonly Mock<IProductionPeriodChangesQueueWrapper> _productionPeriodChangesQueueWrapper = new();
    private readonly Mock<IProductionPeriodsDataHandlerHttpClient> _productionPeriodsDataHandlerHttpClientMock = new();
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock = new();
    private readonly Mock<HttpContext> _httpContextMock = new();

    public ProducedJobQueryIntegrationTests()
    {
        _machineCachingServiceMock
            .Setup(m => m.GetMachinesAsInternalListResponse(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalListResponse<Machine>([
                new Machine
                {
                    MachineId = MachineId,
                    BusinessUnit = BusinessUnit.Extrusion,
                    MachineFamily = MachineFamily.BlowFilm.ToString()
                }
            ]));
    }

    [Fact]
    public async Task Get_Unfinished_Job_With_Base_Properties_By_JobId_And_MachineId()
    {
        // Arrange
        var executor = await InitializeExecutor();
        InitializeProductionPeriodsDataHandlerHttpClientMock(_baseJobWithNoEndTime);

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    producedJob(jobId: ""{JobId}"", machineId: ""{MachineId}"") {{
                        machineId
                        jobId
                        endTime
                        isActive
                        startTime
                        productId
                        uniqueId
                    }}
                }}")
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
    public async Task Get_Finished_Job_With_Base_Properties_By_JobId_And_MachineId()
    {
        // Arrange
        var executor = await InitializeExecutor();
        InitializeProductionPeriodsDataHandlerHttpClientMock(_baseJob);

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    producedJob(jobId: ""{JobId}"", machineId: ""{MachineId}"") {{
                        machineId
                        jobId
                        endTime
                        isActive
                        startTime
                        productId
                        uniqueId
                    }}
                }}")
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
    public async Task Get_Job_Where_ProductId_Is_Null()
    {
        // Arrange
        var executor = await InitializeExecutor();
        var jobWithoutProduct = new JobInfo()
        {
            MachineId = MachineId,
            ProductId = null,
            JobId = JobId,
            StartTime = DateTime.UnixEpoch,
        };

        InitializeProductionPeriodsDataHandlerHttpClientMock(jobWithoutProduct);

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    producedJob(jobId: ""{JobId}"", machineId: ""{MachineId}"") {{
                        machineId
                        jobId
                        productId
                    }}
                }}")
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
    public async Task Get_Job_Where_Customer_Is_Null()
    {
        // Arrange
        var executor = await InitializeExecutor();
        InitializeProductionPeriodsDataHandlerHttpClientMock(_baseJob);
        InitializeMachineSnapshotServices(SnapshotColumnIds.Customer, null);

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
              $@"{{
                    producedJob(jobId: ""{JobId}"", machineId: ""{MachineId}"") {{
                      customer {{
                        lastValue
                        valueWithLongestDuration
                      }}
                    }}
                }}")
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();
    }

    [Fact]
    public async Task Get_Finished_Job_With_JobSize()
    {
        // Arrange
        var executor = await InitializeExecutor();
        InitializeProductionPeriodsDataHandlerHttpClientMock(_baseJob);
        InitializeMachineSnapshotServices(SnapshotColumnIds.JobSize, 1111.1);

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    producedJob(jobId: ""{JobId}"", machineId: ""{MachineId}"") {{
                        machineId
                        jobSize {{
                        lastValue
                        unit
                        }}
                    }}
                }}")
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
    public async Task Get_MinValues()
    {
        // Arrange
        _machineSnapshotHttpClientMock
            .Setup(s => s.GetMinValues(
                MachineId,
                It.IsAny<List<string>>(),
                It.IsAny<List<TimeRange>>(),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new InternalItemResponse<ValueByColumnId<double?>>(_arbitraryNumericValueByColumnId)
            );

        var executor = await InitializeExecutor();
        InitializeProductionPeriodsDataHandlerHttpClientMock(_baseJobWithTimeRanges);
        InitializeHttpContextMocks();

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    producedJob(jobId: ""{JobId}"", machineId: ""{MachineId}"") {{
                        machineId
                        jobSize {{
                        minValue
                        }}
                    }}
                }}")
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
    public async Task Get_MinValues_For_NumericSnapshotValuesDuringProduction_With_No_TimeRanges()
    {
        // Arrange
        var executor = await InitializeExecutor();
        InitializeProductionPeriodsDataHandlerHttpClientMock(_baseJobWithEmptyTimeRanges);

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    producedJob(jobId: ""{JobId}"", machineId: ""{MachineId}"") {{
                        machineId
                        jobSize {{
                        minValue
                        }}
                    }}
                }}")
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
    public async Task Get_MinValues_Returns_Result_With_Exception_In_NumericSnapshotValuesDuringProduction()
    {
        // Arrange
        _machineSnapshotHttpClientMock
            .Setup(s => s.GetMinValues(
                MachineId,
                It.IsAny<List<string>>(),
                It.IsAny<List<TimeRange>>(),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new InternalItemResponse<ValueByColumnId<double?>>(500, new NpgsqlException())
            );

        var executor = await InitializeExecutor();
        InitializeProductionPeriodsDataHandlerHttpClientMock(_baseJobWithTimeRanges);
        InitializeHttpContextMocks();

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    producedJob(jobId: ""{JobId}"", machineId: ""{MachineId}"") {{
                        machineId
                        jobSize {{
                        minValue
                        }}
                    }}
                }}")
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
    public async Task Get_MaxValue()
    {
        // Arrange
        _machineSnapshotHttpClientMock
            .Setup(s => s.GetMaxValues(
                MachineId,
                It.IsAny<List<string>>(),
                It.IsAny<List<TimeRange>>(),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new InternalItemResponse<ValueByColumnId<double?>>(_arbitraryNumericValueByColumnId)
            );

        var executor = await InitializeExecutor();
        InitializeProductionPeriodsDataHandlerHttpClientMock(_baseJobWithTimeRanges);
        InitializeHttpContextMocks();

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    producedJob(jobId: ""{JobId}"", machineId: ""{MachineId}"") {{
                        machineId
                        jobSize {{
                        maxValue
                        }}
                    }}
                }}")
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
    public async Task Get_MaxValues_For_NumericSnapshotValuesDuringProduction_With_No_TimeRanges()
    {
        // Arrange
        var executor = await InitializeExecutor();
        InitializeProductionPeriodsDataHandlerHttpClientMock(_baseJobWithEmptyTimeRanges);

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    producedJob(jobId: ""{JobId}"", machineId: ""{MachineId}"") {{
                        machineId
                        jobSize {{
                        maxValue
                        }}
                    }}
                }}")
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
    public async Task Get_MaxValues_Returns_Result_With_Exception_In_NumericSnapshotValuesDuringProduction()
    {
        // Arrange
        _machineSnapshotHttpClientMock
            .Setup(s => s.GetMaxValues(
                MachineId,
                It.IsAny<List<string>>(),
                It.IsAny<List<TimeRange>>(),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new InternalItemResponse<ValueByColumnId<double?>>(500, new NpgsqlException())
            );

        var executor = await InitializeExecutor();
        InitializeProductionPeriodsDataHandlerHttpClientMock(_baseJobWithTimeRanges);
        InitializeHttpContextMocks();

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    producedJob(jobId: ""{JobId}"", machineId: ""{MachineId}"") {{
                        machineId
                        jobSize {{
                        maxValue
                        }}
                    }}
                }}")
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
    public async Task Get_ArithmeticMean()
    {
        // Arrange
        _machineSnapshotHttpClientMock
            .Setup(s => s.GetArithmeticMeans(
                MachineId,
                It.IsAny<List<string>>(),
                It.IsAny<List<TimeRange>>(),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new InternalItemResponse<ValueByColumnId<double?>>(_arbitraryNumericValueByColumnId)
            );

        var executor = await InitializeExecutor();
        InitializeProductionPeriodsDataHandlerHttpClientMock(_baseJobWithTimeRanges);
        InitializeHttpContextMocks();

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    producedJob(jobId: ""{JobId}"", machineId: ""{MachineId}"") {{
                        machineId
                        jobSize {{
                        averageValue
                        }}
                    }}
                }}")
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
    public async Task Get_ArithmeticMean_For_NumericSnapshotValuesDuringProduction_With_No_TimeRanges()
    {
        // Arrange
        var executor = await InitializeExecutor();
        InitializeProductionPeriodsDataHandlerHttpClientMock(_baseJobWithEmptyTimeRanges);

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    producedJob(jobId: ""{JobId}"", machineId: ""{MachineId}"") {{
                        machineId
                        jobSize {{
                        averageValue
                        }}
                    }}
                }}")
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
    public async Task Get_ArithmeticMean_Returns_Result_With_Exception_In_NumericSnapshotValuesDuringProduction()
    {
        // Arrange
        _machineSnapshotHttpClientMock
            .Setup(s => s.GetArithmeticMeans(
                MachineId,
                It.IsAny<List<string>>(),
                It.IsAny<List<TimeRange>>(),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new InternalItemResponse<ValueByColumnId<double?>>(500, new NpgsqlException())
            );

        var executor = await InitializeExecutor();
        InitializeProductionPeriodsDataHandlerHttpClientMock(_baseJobWithTimeRanges);
        InitializeHttpContextMocks();

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    producedJob(jobId: ""{JobId}"", machineId: ""{MachineId}"") {{
                        machineId
                        jobSize {{
                        averageValue
                        }}
                    }}
                }}")
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
    public async Task Get_MedianValues()
    {
        // Arrange
        _machineSnapshotHttpClientMock
            .Setup(s => s.GetMedianValues(
                MachineId,
                It.IsAny<List<string>>(),
                It.IsAny<List<TimeRange>>(),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new InternalItemResponse<ValueByColumnId<double?>>(_arbitraryNumericValueByColumnId)
            );

        var executor = await InitializeExecutor();
        InitializeProductionPeriodsDataHandlerHttpClientMock(_baseJobWithTimeRanges);
        InitializeHttpContextMocks();

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    producedJob(jobId: ""{JobId}"", machineId: ""{MachineId}"") {{
                        machineId
                        jobSize {{
                        median
                        }}
                    }}
                }}")
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
    public async Task Get_MedianValues_For_NumericSnapshotValuesDuringProduction_With_No_TimeRanges()
    {
        // Arrange
        var executor = await InitializeExecutor();
        InitializeProductionPeriodsDataHandlerHttpClientMock(_baseJobWithEmptyTimeRanges);
        InitializeHttpContextMocks();

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    producedJob(jobId: ""{JobId}"", machineId: ""{MachineId}"") {{
                        machineId
                        jobSize {{
                        median
                        }}
                    }}
                }}")
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
    public async Task Get_MedianValues_Returns_Result_With_Exception_In_NumericSnapshotValuesDuringProduction()
    {
        // Arrange
        _machineSnapshotHttpClientMock
            .Setup(s => s.GetMedianValues(
                MachineId,
                It.IsAny<List<string>>(),
                It.IsAny<List<TimeRange>>(),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new InternalItemResponse<ValueByColumnId<double?>>(500, new NpgsqlException())
            );

        var executor = await InitializeExecutor();
        InitializeProductionPeriodsDataHandlerHttpClientMock(_baseJobWithTimeRanges);
        InitializeHttpContextMocks();

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    producedJob(jobId: ""{JobId}"", machineId: ""{MachineId}"") {{
                        machineId
                        jobSize {{
                        median
                        }}
                    }}
                }}")
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
    public async Task Get_StandardDeviation()
    {
        // Arrange
        _machineSnapshotHttpClientMock
            .Setup(s => s.GetStandardDeviations(
                MachineId,
                It.IsAny<List<string>>(),
                It.IsAny<List<TimeRange>>(),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new InternalItemResponse<ValueByColumnId<double?>>(_arbitraryNumericValueByColumnId)
            );

        var executor = await InitializeExecutor();
        InitializeProductionPeriodsDataHandlerHttpClientMock(_baseJobWithTimeRanges);
        InitializeHttpContextMocks();

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    producedJob(jobId: ""{JobId}"", machineId: ""{MachineId}"") {{
                        machineId
                        jobSize {{
                        standardDeviationValue
                        }}
                    }}
                }}")
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
    public async Task Get_StandardDeviation_For_NumericSnapshotValuesDuringProduction_With_No_TimeRanges()
    {
        // Arrange
        var executor = await InitializeExecutor();
        InitializeProductionPeriodsDataHandlerHttpClientMock(_baseJobWithEmptyTimeRanges);
        InitializeHttpContextMocks();

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    producedJob(jobId: ""{JobId}"", machineId: ""{MachineId}"") {{
                        machineId
                        jobSize {{
                        standardDeviationValue
                        }}
                    }}
                }}")
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
    public async Task Get_StandardDeviation_Returns_Result_With_Exception_In_NumericSnapshotValuesDuringProduction()
    {
        // Arrange
        _machineSnapshotHttpClientMock
            .Setup(s => s.GetStandardDeviations(
                MachineId,
                It.IsAny<List<string>>(),
                It.IsAny<List<TimeRange>>(),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new InternalItemResponse<ValueByColumnId<double?>>(500, new NpgsqlException())
            );

        var executor = await InitializeExecutor();
        InitializeProductionPeriodsDataHandlerHttpClientMock(_baseJobWithTimeRanges);
        InitializeHttpContextMocks();

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    producedJob(jobId: ""{JobId}"", machineId: ""{MachineId}"") {{
                        machineId
                        jobSize {{
                        standardDeviationValue
                        }}
                    }}
                }}")
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
    public async Task Get_ValuesWithLongestDuration()
    {
        // Arrange
        var valuesWithLongestDuration = new ValueByColumnId<object?> { { SnapshotColumnIds.JobSize, 1111.1 } };

        _machineSnapshotHttpClientMock
            .Setup(m => m.GetValuesWithLongestDuration(
                MachineId,
                new List<string> { SnapshotColumnIds.JobSize },
                MockTimeRanges,
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<ValueByColumnId<object?>>(valuesWithLongestDuration));

        var executor = await InitializeExecutor();
        InitializeProductionPeriodsDataHandlerHttpClientMock(_baseJobWithTimeRanges);
        InitializeHttpContextMocks();

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    producedJob(jobId: ""{JobId}"", machineId: ""{MachineId}"") {{
                        machineId
                        jobSize {{
                        valueWithLongestDuration
                        }}
                    }}
                }}")
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
    public async Task Get_ValuesWithLongestDuration_Returns_Null_On_Subscription()
    {
        // Arrange
        var executor = await InitializeExecutor();
        InitializeProductionPeriodsDataHandlerHttpClientMock(_baseJobWithTimeRanges);
        InitializeHttpContextMocks(true);

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    producedJob(jobId: ""{JobId}"", machineId: ""{MachineId}"") {{
                        machineId
                        jobSize {{
                        valueWithLongestDuration
                        }}
                    }}
                }}")
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
    public async Task Get_ValuesWithLongestDuration_For_JobSize_And_Customer()
    {
        // Arrange
        var valuesWithLongestDuration = new ValueByColumnId<object?>
        {
            { SnapshotColumnIds.Customer, Customer },
            { SnapshotColumnIds.JobSize, 1111.1 }
        };

        _machineSnapshotHttpClientMock
            .Setup(m => m.GetValuesWithLongestDuration(
                MachineId,
                It.Is<List<string>>(columnIds =>
                    columnIds.Contains(SnapshotColumnIds.Customer) && columnIds.Contains(SnapshotColumnIds.JobSize)),
                MockTimeRanges,
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<ValueByColumnId<object?>>(valuesWithLongestDuration));

        var executor = await InitializeExecutor();
        InitializeProductionPeriodsDataHandlerHttpClientMock(_baseJobWithTimeRanges);
        InitializeHttpContextMocks();

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    producedJob(jobId: ""{JobId}"", machineId: ""{MachineId}"") {{
                        machineId
                        customer {{
                        valueWithLongestDuration
                        }}
                        jobSize {{
                        valueWithLongestDuration
                        }}
                    }}
                }}")
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
    public async Task Get_DistinctValues_For_JobSize_And_Customer()
    {
        // Arrange
        const int limit = 1;
        var distinctValues = new ValueByColumnId<List<object?>>
        {
            { SnapshotColumnIds.Customer, [Customer] },
            { SnapshotColumnIds.JobSize, [1111.1] }
        };

        _machineSnapshotHttpClientMock
            .Setup(m => m.GetDistinctValues(
                MachineId,
                It.Is<List<string>>(columnIds =>
                    columnIds.Contains(SnapshotColumnIds.Customer) && columnIds.Contains(SnapshotColumnIds.JobSize)),
                MockTimeRanges,
                limit,
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<ValueByColumnId<List<object?>>>(distinctValues));

        var executor = await InitializeExecutor();
        InitializeProductionPeriodsDataHandlerHttpClientMock(_baseJobWithTimeRanges);
        InitializeHttpContextMocks();

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    producedJob(jobId: ""{JobId}"", machineId: ""{MachineId}"") {{
                        machineId
                        customer {{
                        distinctValues(limit: 1)
                        }}
                        jobSize {{
                        distinctValues(limit: 1)
                        }}
                    }}
                }}")
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
    public async Task Get_ValuesWithLongestDuration_With_SnapshotValueDuringProduction_With_No_TimeRanges()
    {
        // Arrange
        var executor = await InitializeExecutor();
        InitializeProductionPeriodsDataHandlerHttpClientMock(_baseJobWithEmptyTimeRanges);
        InitializeHttpContextMocks();

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    producedJob(jobId: ""{JobId}"", machineId: ""{MachineId}"") {{
                        machineId
                        customer {{
                        valueWithLongestDuration
                        }}
                    }}
                }}")
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
    public async Task Get_ValuesWithLongestDuration_For_NumericSnapshotValuesDuringProduction_With_No_TimeRanges()
    {
        // Arrange
        var executor = await InitializeExecutor();
        InitializeProductionPeriodsDataHandlerHttpClientMock(_baseJobWithEmptyTimeRanges);
        InitializeHttpContextMocks();

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    producedJob(jobId: ""{JobId}"", machineId: ""{MachineId}"") {{
                        machineId
                        jobSize {{
                        valueWithLongestDuration
                        }}
                    }}
                }}")
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
    public async Task Get_ValuesWithLongestDuration_Returns_Result_With_Exception_In_SnapshotValuesDuringProduction()
    {
        // Arrange
        _machineSnapshotHttpClientMock
            .Setup(m => m.GetValuesWithLongestDuration(
                MachineId,
                new List<string> { SnapshotColumnIds.Customer },
                MockTimeRanges,
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<ValueByColumnId<object?>>(
                (int)HttpStatusCode.InternalServerError, new NpgsqlException()));

        var executor = await InitializeExecutor();
        InitializeProductionPeriodsDataHandlerHttpClientMock(_baseJobWithTimeRanges);
        InitializeHttpContextMocks();

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    producedJob(jobId: ""{JobId}"", machineId: ""{MachineId}"") {{
                        machineId
                        customer {{
                        valueWithLongestDuration
                        }}
                    }}
                }}")
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
    public async Task Get_ValuesWithLongestDuration_Returns_Result_With_Exception_In_NumericSnapshotValuesDuringProduction()
    {
        // Arrange
        _machineSnapshotHttpClientMock
            .Setup(m => m.GetValuesWithLongestDuration(
                MachineId,
                new List<string> { SnapshotColumnIds.JobSize },
                MockTimeRanges,
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<ValueByColumnId<object?>>(
                (int)HttpStatusCode.InternalServerError, new NpgsqlException()));

        var executor = await InitializeExecutor();
        InitializeProductionPeriodsDataHandlerHttpClientMock(_baseJobWithTimeRanges);
        InitializeHttpContextMocks();

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    producedJob(jobId: ""{JobId}"", machineId: ""{MachineId}"") {{
                        machineId
                        jobSize {{
                        valueWithLongestDuration
                        }}
                    }}
                }}")
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
    public async Task Get_DistinctValues_Returns_Result_With_Exception_In_SnapshotValuesDuringProduction()
    {
        // Arrange
        const int limit = 100;

        _machineSnapshotHttpClientMock
            .Setup(m => m.GetDistinctValues(
                MachineId,
                new List<string> { SnapshotColumnIds.Customer },
                MockTimeRanges,
                limit,
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<ValueByColumnId<List<object?>>>(
                (int)HttpStatusCode.InternalServerError, new NpgsqlException()));

        var executor = await InitializeExecutor();
        InitializeProductionPeriodsDataHandlerHttpClientMock(_baseJobWithTimeRanges);
        InitializeHttpContextMocks();

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    producedJob(jobId: ""{JobId}"", machineId: ""{MachineId}"") {{
                        machineId
                        customer {{
                        distinctValues
                        }}
                    }}
                }}")
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
    public async Task Get_DistinctValues_Returns_Result_With_Exception_In_NumericSnapshotValuesDuringProduction()
    {
        // Arrange
        const int limit = 100;

        _machineSnapshotHttpClientMock
            .Setup(m => m.GetDistinctValues(
                MachineId,
                new List<string> { SnapshotColumnIds.JobSize },
                MockTimeRanges,
                limit,
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<ValueByColumnId<List<object?>>>(
                (int)HttpStatusCode.InternalServerError, new NpgsqlException()));

        var executor = await InitializeExecutor();
        InitializeProductionPeriodsDataHandlerHttpClientMock(_baseJobWithTimeRanges);
        InitializeHttpContextMocks();

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    producedJob(jobId: ""{JobId}"", machineId: ""{MachineId}"") {{
                        machineId
                        jobSize {{
                        distinctValues
                        }}
                    }}
                }}")
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
    public async Task Get_DistinctValues_With_SnapshotValueDuringProduction_With_No_TimeRanges()
    {
        // Arrange
        var executor = await InitializeExecutor();
        InitializeProductionPeriodsDataHandlerHttpClientMock(_baseJobWithEmptyTimeRanges);
        InitializeHttpContextMocks();

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    producedJob(jobId: ""{JobId}"", machineId: ""{MachineId}"") {{
                        machineId
                        customer {{
                        distinctValues
                        }}
                    }}
                }}")
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
    public async Task Get_DistinctValues_For_NumericSnapshotValuesDuringProduction_With_No_TimeRanges()
    {
        // Arrange
        var executor = await InitializeExecutor();
        InitializeProductionPeriodsDataHandlerHttpClientMock(_baseJobWithEmptyTimeRanges);
        InitializeHttpContextMocks();

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    producedJob(jobId: ""{JobId}"", machineId: ""{MachineId}"") {{
                        machineId
                        jobSize {{
                        distinctValues
                        }}
                    }}
                }}")
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
    public async Task Get_Finished_Job_With_Customer()
    {
        // Arrange
        var executor = await InitializeExecutor();
        InitializeProductionPeriodsDataHandlerHttpClientMock(_baseJob);
        InitializeMachineSnapshotServices(SnapshotColumnIds.Customer, "FakeCustomer");

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    producedJob(jobId: ""{JobId}"", machineId: ""{MachineId}"") {{
                        machineId
                        customer {{
                        lastValue
                        }}
                    }}
                }}")
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

    private async Task<IRequestExecutor> InitializeExecutor()
    {
        var kpiService = new KpiService(
            new UnitService(),
            new MachineMetaDataService(),
            _kpiDataHandlerClientMock.Object);
        var machineService = new MachineService(
            _machineCachingServiceMock.Object, new Mock<ILogger<MachineService>>().Object);
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
        var machineSnapshotService = new MachineSnapshotService();
        var columnTrendService = new ColumnTrendOfLast8HoursService(machineTimeService, machineSnapshotService);

        var snapshotByTimestampBatchDataLoader = new SnapshotByTimestampBatchDataLoader(
            _machineSnapshotHttpClientMock.Object,
            new DelayedBatchScheduler(),
            new Mock<ILogger<SnapshotByTimestampBatchDataLoader>>().Object,
            new DataLoaderOptions()
        );
        var latestSnapshotCacheDataLoader =
            new LatestSnapshotCacheDataLoader(_latestMachineSnapshotCachingServiceMock.Object);

        return await new ServiceCollection()
            .AddSingleton(snapshotByTimestampBatchDataLoader)
            .AddSingleton(latestSnapshotCacheDataLoader)
            .AddSingleton(_machineSnapshotHttpClientMock.Object)
            .AddSingleton(_metaDataHandlerHttpClientMock.Object)
            .AddSingleton(_latestMachineSnapshotCachingServiceMock.Object)
            .AddSingleton(_kpiDataCachingServiceMock.Object)
            .AddSingleton(_productionPeriodsDataHandlerHttpClientMock.Object)
            .AddSingleton(_httpContextAccessorMock.Object)
            .AddSingleton<IKpiService>(kpiService)
            .AddSingleton<IMachineService>(machineService)
            .AddSingleton<IProducedJobService>(producedJobService)
            .AddSingleton<IMachineSnapshotService>(machineSnapshotService)
            .AddSingleton<IColumnTrendService>(columnTrendService)
            .AddLogging()
            .AddGraphQLServer()
            .AddAuthorization()
            .ModifyOptions(t => t.StrictValidation = false)
            .AddQueryType(q => q.Name("Query"))
            .AddType<FrameworkAPI.Queries.ProducedJobQuery>()
            .AddType<ExtrusionProducedJob>()
            .BuildRequestExecutorAsync();
    }

    private void InitializeProductionPeriodsDataHandlerHttpClientMock(JobInfo job)
    {
        _productionPeriodsDataHandlerHttpClientMock
            .Setup(m => m.GetJobInfo(It.IsAny<CancellationToken>(), MachineId, JobId))
            .ReturnsAsync(new InternalItemResponse<JobInfo>(job));
    }

    private void InitializeHttpContextMocks(bool isWebSocketRequest = false)
    {
        _httpContextMock
            .SetupGet(m => m.Request.Method)
            .Returns(HttpMethod.Get.ToString);
        _httpContextMock
            .SetupGet(m => m.WebSockets.IsWebSocketRequest)
            .Returns(isWebSocketRequest);
        _httpContextAccessorMock
            .SetupGet(m => m.HttpContext)
            .Returns(_httpContextMock.Object);
    }

    private void InitializeMachineSnapshotServices(string columnId, object? value)
    {
        var snapshotMetaDto = new SnapshotMetaDto(
            MachineId, "fakeHash", DateTime.MinValue, [new SnapshotColumnUnitDto(columnId, $"FakeUnit_{columnId}")]);
        var snapshotDto = new SnapshotDto(
            [new SnapshotColumnValueDto(columnId, value)],
            snapshotTime: DateTime.UtcNow);

        _machineSnapshotHttpClientMock
            .Setup(m => m.GetSnapshotsForTimestamps(
                MachineId,
                It.IsAny<List<DateTime>>(),
                It.IsAny<List<string>>(),
                It.IsAny<CancellationToken>()))
            .Returns<string, List<DateTime>, List<string>, CancellationToken>((_, timestamps, _, _) =>
            {
                var response = new MachineSnapshotForTimestampListResponse(snapshotMetaDto,
                    timestamps.Select(timestamp => new SnapshotForTimestampDto(snapshotDto, timestamp)).ToList());
                return Task.FromResult(new InternalItemResponse<MachineSnapshotForTimestampListResponse>(response));
            });

        _latestMachineSnapshotCachingServiceMock
            .Setup(m => m.GetLatestMachineSnapshot(MachineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new InternalItemResponse<MachineSnapshotResponse>(
                    new MachineSnapshotResponse(snapshotMetaDto, snapshotDto)));
    }
}