using System;
using System.Collections.Generic;
using System.Linq;
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

namespace FrameworkAPI.Test.Queries.ProducedJobQuery;

public class ExtrusionMachineProducedJobIntegrationTests
{
    private const string MachineId = "EQ12345";
    private const string JobId = "FakeJobId";

    private readonly JobInfo _finishedJob = new()
    {
        MachineId = MachineId,
        ProductId = "FakeProductId",
        JobId = JobId,
        StartTime = DateTime.UnixEpoch,
        EndTime = DateTime.UnixEpoch.AddHours(1)
    };

    private readonly JobInfo _finishedJobWithTimeRanges = new()
    {
        MachineId = MachineId,
        ProductId = "FakeProductId",
        JobId = JobId,
        StartTime = DateTime.UnixEpoch,
        EndTime = DateTime.UnixEpoch.AddHours(1),
        TimeRanges = new List<TimeRange>
        {
            new(
                DateTime.UnixEpoch,
                DateTime.UnixEpoch.AddHours(1)
            )
        }
    };

    private readonly JobInfo _unfinishedJob = new()
    {
        MachineId = MachineId,
        ProductId = "FakeProductId",
        JobId = JobId,
        StartTime = DateTime.UnixEpoch
    };

    private readonly ValueByColumnId<GroupedSumByIdentifier> _arbitraryGroupedSumByIdentifierColumnId = new()
    {
        {
            "Extrusion.ExtruderA.Settings.Component1.MaterialName",
            new()
            {
                { "ArbitraryMaterialName1", 50 },
                { "ArbitraryMaterialName2", 250 },
                { "ArbitraryMaterialName3", 1000 }
            }
        },
        {
            "Extrusion.ExtruderB.Settings.Component1.MaterialName",
            new()
            {
                { "ArbitraryMaterialName1", 50 },
                { "ArbitraryMaterialName2", 250 },
                { "ArbitraryMaterialName3", 1000 }
            }
        }
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
    public ExtrusionMachineProducedJobIntegrationTests()
    {
        _machineCachingServiceMock
            .Setup(s =>
                s.GetMachinesAsInternalListResponse(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalListResponse<Machine>(new List<Machine>
            {
                new()
                {
                    MachineId = MachineId,
                    BusinessUnit = BusinessUnit.Extrusion
                }
            }));
    }

    [Fact]
    public async Task Get_Finished_Job_With_GoodLength_And_GoodWeight()
    {
        // Arrange
        var executor = await InitializeExecutor();
        KpiTestInitializer.InitializeMocks(
            MachineId,
            JobId,
            siUnitJobQuantityActual: "kg",
            siUnitJobQuantityActualInSecondUnit: "m",
            siUnitMachineSpeed: "kg/h",
            _metaDataHandlerHttpClientMock,
            _kpiDataCachingServiceMock,
            _productionPeriodsDataHandlerHttpClientMock);

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    producedJob(jobId: ""{JobId}"", machineId: ""{MachineId}"") {{
                        machineId
                        ... on ExtrusionProducedJob {{
                          goodWeight {{
                            value
                            unit
                          }},
                          goodLength {{
                            value
                            unit
                          }}
                        }}
                    }}
                }}")
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        KpiTestInitializer.VerifyResultAndMocks(
            result,
            _metaDataHandlerHttpClientMock,
            _kpiDataCachingServiceMock,
            _productionPeriodsDataHandlerHttpClientMock);
    }

    [Fact]
    public async Task Get_Finished_Job_With_ScrapLength_And_ScrapWeight()
    {
        // Arrange
        var executor = await InitializeExecutor();
        KpiTestInitializer.InitializeMocks(
            MachineId,
            JobId,
            siUnitJobQuantityActual: "kg",
            siUnitJobQuantityActualInSecondUnit: "m",
            siUnitMachineSpeed: "kg/h",
            _metaDataHandlerHttpClientMock,
            _kpiDataCachingServiceMock,
            _productionPeriodsDataHandlerHttpClientMock);

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    producedJob(jobId: ""{JobId}"", machineId: ""{MachineId}"") {{
                      machineId
                      ... on ExtrusionProducedJob {{
                        scrapWeight {{
                            value
                            unit
                        }},
                        setupScrapWeight {{
                            value
                            unit
                        }},
                        scrapLength {{
                            value
                            unit
                        }},
                        setupScrapLength {{
                            value
                            unit
                        }}
                      }}
                    }}
                }}")
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        await Task.Delay(TimeSpan.FromSeconds(1));
        KpiTestInitializer.VerifyResultAndMocks(
            result,
            _metaDataHandlerHttpClientMock,
            _kpiDataCachingServiceMock,
            _productionPeriodsDataHandlerHttpClientMock);
    }

    [Fact]
    public async Task Extrusion_MachineSetting_Latest_Width_By_Finished_Job()
    {
        // Arrange
        var executor = await InitializeExecutor();
        InitializeProductionPeriodsDataHandlerHttpClientMock(_finishedJob);

        InitializeMachineSnapshotServices(SnapshotColumnIds.ExtrusionFormatSettingsWidth, 11.1);

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    producedJob(jobId: ""{JobId}"", machineId: ""{MachineId}"") {{
                        machineId
                        ... on ExtrusionProducedJob {{
                        endTime
                        machineSettings {{
                            width {{
                            lastValue
                            unit
                            }}
                        }}
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
    public async Task Extrusion_MachineSetting_Latest_Thickness_By_Finished_Job()
    {
        // Arrange
        var executor = await InitializeExecutor();
        InitializeProductionPeriodsDataHandlerHttpClientMock(_finishedJob);

        InitializeMachineSnapshotServices(SnapshotColumnIds.ExtrusionFormatSettingsThickness, 11.1);

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    producedJob(jobId: ""{JobId}"", machineId: ""{MachineId}"") {{
                        machineId
                        ... on ExtrusionProducedJob {{
                        endTime
                        machineSettings {{
                            thickness {{
                            lastValue
                            unit
                            }}
                        }}
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
    public async Task Extrusion_MachineSetting_Latest_Width_By_Unfinished_Job()
    {
        // Arrange
        var executor = await InitializeExecutor();
        InitializeProductionPeriodsDataHandlerHttpClientMock(_unfinishedJob);

        InitializeMachineSnapshotServices(SnapshotColumnIds.ExtrusionFormatSettingsWidth, 22.2);

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    producedJob(jobId: ""{JobId}"", machineId: ""{MachineId}"") {{
                        machineId
                        ... on ExtrusionProducedJob {{
                        endTime
                        machineSettings {{
                            width {{
                            lastValue
                            unit
                            }}
                        }}
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
    public async Task Get_RawMaterialConsumptionByMaterial()
    {
        // Arrange
        InitializeHttpContextMocks();
        _machineSnapshotHttpClientMock
            .Setup(s => s.GetGroupedSums(
                MachineId,
                It.IsAny<List<GroupAssignment>>(),
                It.IsAny<List<TimeRange>>(),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new InternalItemResponse<ValueByColumnId<GroupedSumByIdentifier>>(_arbitraryGroupedSumByIdentifierColumnId)
            );
        var executor = await InitializeExecutor();
        InitializeProductionPeriodsDataHandlerHttpClientMock(_finishedJobWithTimeRanges);

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(GetRawMaterialConsumptionByMaterialQuery())
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();
    }

    [Fact]
    public async Task Get_RawMaterialConsumptionByMaterial_Returns_Null_When_No_Time_Ranges_Are_Available()
    {
        // Arrange
        InitializeHttpContextMocks();
        _machineSnapshotHttpClientMock
            .Setup(s => s.GetGroupedSums(
                MachineId,
                It.IsAny<List<GroupAssignment>>(),
                It.IsAny<List<TimeRange>>(),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new InternalItemResponse<ValueByColumnId<GroupedSumByIdentifier>>(_arbitraryGroupedSumByIdentifierColumnId)
            );
        var executor = await InitializeExecutor();
        InitializeProductionPeriodsDataHandlerHttpClientMock(_finishedJob);

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(GetRawMaterialConsumptionByMaterialQuery())
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();
    }

    [Fact]
    public async Task Get_RawMaterialConsumptionByMaterial_Returns_Null_For_Subscription()
    {
        // Arrange
        _machineSnapshotHttpClientMock
            .Setup(s => s.GetGroupedSums(
                MachineId,
                It.IsAny<List<GroupAssignment>>(),
                It.IsAny<List<TimeRange>>(),
                It.IsAny<List<Filter>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new InternalItemResponse<ValueByColumnId<GroupedSumByIdentifier>>(_arbitraryGroupedSumByIdentifierColumnId)
            );
        var executor = await InitializeExecutor();
        InitializeProductionPeriodsDataHandlerHttpClientMock(_finishedJobWithTimeRanges);

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(GetRawMaterialConsumptionByMaterialQuery())
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();
    }

    private static string GetRawMaterialConsumptionByMaterialQuery(string jobId = JobId, string machineId = MachineId)
    {
        return $@"
        {{
            producedJob(jobId: ""{jobId}"", machineId: ""{machineId}"")
            {{
                ... on ExtrusionProducedJob
                {{
                    rawMaterialConsumptionByMaterial
                    {{
                        key
                        value
                        {{
                            value
                            unit
                        }}
                    }}
                }}
            }}
        }}";
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

        var snapshotByTimestampBatchDataLoader = new SnapshotByTimestampBatchDataLoader(
            _machineSnapshotHttpClientMock.Object,
            new DelayedBatchScheduler(),
            new Mock<ILogger<SnapshotByTimestampBatchDataLoader>>().Object,
            new DataLoaderOptions()
        );

        var latestSnapshotCacheDataLoader =
            new LatestSnapshotCacheDataLoader(_latestMachineSnapshotCachingServiceMock.Object);

        var materialConsumptionService = new MaterialConsumptionService(machineSnapshotService);

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
            .AddSingleton<IMaterialConsumptionService>(materialConsumptionService)
            .AddLogging()
            .AddGraphQLServer()
            .AddAuthorization()
            .ModifyOptions(t => t.StrictValidation = false)
            .AddQueryType(q => q.Name("Query"))
            .AddType<FrameworkAPI.Queries.ProducedJobQuery>()
            .AddType<ExtrusionProducedJob>()
            .BuildRequestExecutorAsync();
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

    private void InitializeProductionPeriodsDataHandlerHttpClientMock(JobInfo job)
    {
        _productionPeriodsDataHandlerHttpClientMock
            .Setup(s => s.GetJobInfo(
                It.IsAny<CancellationToken>(),
                MachineId,
                JobId))
            .ReturnsAsync(new InternalItemResponse<JobInfo>(job));
    }

    private void InitializeMachineSnapshotServices(string columnId, object? value)
    {
        var snapshotMetaDto = new SnapshotMetaDto(
            MachineId,
            "fakeHash",
            DateTime.MinValue,
            new List<SnapshotColumnUnitDto>
            {
                new(columnId, "FakeUnit")
            });
        var snapshotDto = new SnapshotDto(
            new List<SnapshotColumnValueDto>
            {
                new(columnId, value)
            },
            DateTime.UtcNow);
        _machineSnapshotHttpClientMock
            .Setup(s => s.GetSnapshotsForTimestamps(
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
            .Setup(s => s.GetLatestMachineSnapshot(
                MachineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new InternalItemResponse<MachineSnapshotResponse>(
                    new MachineSnapshotResponse(snapshotMetaDto, snapshotDto)));
    }
}