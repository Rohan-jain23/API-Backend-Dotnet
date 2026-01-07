using System;
using System.Collections.Generic;
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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using WuH.Ruby.Common.Core;
using WuH.Ruby.KpiDataHandler.Client;
using WuH.Ruby.MachineDataHandler.Client;
using WuH.Ruby.MachineSnapShooter.Client;
using WuH.Ruby.MetaDataHandler.Client;
using WuH.Ruby.OpcUaForwarder.Client;
using WuH.Ruby.ProcessDataReader.Client;
using WuH.Ruby.ProductionPeriodsDataHandler.Client;
using Xunit;
using Machine = WuH.Ruby.MachineDataHandler.Client.Machine;

namespace FrameworkAPI.Test.Queries.ProducedJobQuery;

public class ProducedJobKpiIntegrationTests
{
    private const string MachineId = "EQ00001";
    private const string JobId = "FakeJobId";

    private readonly Mock<IMachineSnapshotHttpClient> _machineSnapshotHttpClientMock = new();
    private readonly Mock<IMetaDataHandlerHttpClient> _metaDataHandlerHttpClientMock = new();
    private readonly Mock<ILatestMachineSnapshotCachingService> _latestMachineSnapshotCachingServiceMock = new();
    private readonly Mock<IKpiDataCachingService> _kpiDataCachingServiceMock = new();
    private readonly Mock<IMachineCachingService> _machineCachingServiceMock = new();
    private readonly Mock<IProcessDataReaderHttpClient> _processDataReaderHttpClientMock = new();
    private readonly Mock<IProductionPeriodChangesQueueWrapper> _productionPeriodChangesQueueWrapper = new();
    private readonly Mock<IProductionPeriodsDataHandlerHttpClient> _productionPeriodsDataHandlerHttpClientMock = new();
    private readonly Mock<IKpiDataHandlerClient> _kpiDataHandlerClientMock = new();

    [Fact]
    public async Task Get_BottomerJob_With_ProductionTimes()
    {
        // Arrange
        var executor = await InitializeExecutor();
        InitializeBottomerJob();

        CreateMockForMachine(BusinessUnit.PaperSack, MachineFamily.PaperSackTuber);
        CreateMockForJob();
        KpiTestInitializer.InitializeMocks(
            MachineId,
            JobId,
            siUnitJobQuantityActual: "STK",
            siUnitJobQuantityActualInSecondUnit: string.Empty,
            siUnitMachineSpeed: "STKMIN",
            _metaDataHandlerHttpClientMock,
            _kpiDataCachingServiceMock,
            _productionPeriodsDataHandlerHttpClientMock);

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
              $@"{{
                    producedJob(jobId: ""{JobId}"", machineId: ""{MachineId}"") {{
                      productionTimes {{
                        generalDownTimeInMin
                        jobRelatedDownTimeInMin
                        notQueryRelatedTimeInMin
                        productionTimeInMin
                        scheduledNonProductionTimeInMin
                        scrapTimeInMin
                        setupTimeInMin
                        totalPlannedProductionTimeInMin
                        totalTimeInMin
                      }}
                    }}
                }}")
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        KpiTestInitializer.VerifyResultAndMocks(
            result,
            null, // MetaData is not called on this query
            _kpiDataCachingServiceMock,
            _productionPeriodsDataHandlerHttpClientMock);
    }

    [Fact]
    public async Task Get_BottomerJob_With_OEE()
    {
        // Arrange
        var executor = await InitializeExecutor();
        InitializeBottomerJob();

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
              $@"{{
                    producedJob(jobId: ""{JobId}"", machineId: ""{MachineId}"") {{
                      overallEquipmentEffectiveness {{
                        availability
                        effectiveness
                        oee
                        quality
                      }}
                    }}
                }}")
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        KpiTestInitializer.VerifyResultAndMocks(
            result,
            null, // MetaData is not called on this query
            _kpiDataCachingServiceMock,
            _productionPeriodsDataHandlerHttpClientMock);
    }

    [Fact]
    public async Task Get_BottomerJob_With_Performance()
    {
        // Arrange
        var executor = await InitializeExecutor();
        InitializeBottomerJob();

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
              $@"{{
                    producedJob(jobId: ""{JobId}"", machineId: ""{MachineId}"") {{
                      performance {{
                        downtime {{
                            actualValue
                            lostTimeInMin
                            targetValue
                            targetValueSource
                            unit
                            wonProductivity
                        }}
                        scrap {{
                            actualValue
                            lostTimeInMin
                            targetValue
                            targetValueSource
                            unit
                            wonProductivity
                        }}
                        setup {{
                            actualValue
                            lostTimeInMin
                            targetValue
                            targetValueSource
                            unit
                            wonProductivity
                        }}
                        speed {{
                            actualValue
                            lostTimeInMin
                            targetValue
                            targetValueSource
                            unit
                            wonProductivity
                        }}
                        total {{
                            actualValue
                            targetValue
                            lostTimeInMin
                            targetValueSource
                            unit
                            wonProductivity
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
    public async Task Get_BottomerJob_With_RemainingProductionTime()
    {
        // Arrange
        var executor = await InitializeExecutor();
        InitializeBottomerJob();

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
              $@"{{
                    producedJob(jobId: ""{JobId}"", machineId: ""{MachineId}"") {{
                      ... on PaperSackProducedJob {{
                        remainingProductionTimeInMin
                      }}
                    }}
                }}")
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        KpiTestInitializer.VerifyResultAndMocks(
            result,
            null, // MetaData is not called on this query
            _kpiDataCachingServiceMock,
            _productionPeriodsDataHandlerHttpClientMock);
    }

    [Fact]
    public async Task Get_BottomerJob_With_PaperSackSpecificKPIs()
    {
        // Arrange
        var executor = await InitializeExecutor();
        InitializeBottomerJob();

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
              $@"{{
                    producedJob(jobId: ""{JobId}"", machineId: ""{MachineId}"") {{
                      ... on PaperSackProducedJob {{
                        originalGoodQuantity {{
                            value
                            unit
                        }}
                        isApparentlyWrongGoodQuantity
                        averageSpeedDuringProduction {{
                            unit
                            value
                        }}
                        targetJobTimeInMin
                        targetSpeed {{
                            unit
                            value
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
    public async Task Get_BlownFilmJob_With_ExtrusionSpecific_SpeedKPIs()
    {
        // Arrange
        var executor = await InitializeExecutor();
        CreateMockForMachine(BusinessUnit.Extrusion, MachineFamily.BlowFilm);
        CreateMockForJob();
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
                        averageThroughputRateDuringProduction {{
                            unit
                            value
                        }}
                        targetThroughputRate {{
                            unit
                            value
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
    public async Task Get_FlexoPrintJob_With_PrintingSpecific_SpeedKPIs()
    {
        // Arrange
        var executor = await InitializeExecutor();
        CreateMockForMachine(BusinessUnit.Printing, MachineFamily.FlexoPrint);
        CreateMockForJob();
        KpiTestInitializer.InitializeMocks(
            MachineId,
            JobId,
            siUnitJobQuantityActual: "m",
            siUnitJobQuantityActualInSecondUnit: string.Empty,
            siUnitMachineSpeed: "m/min",
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
                      ... on PrintingProducedJob {{
                        averageSpeedDuringProduction {{
                            unit
                            value
                        }},
                        targetSpeed {{
                            unit
                            value
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
            null, // MetaData is not called on this query
            _kpiDataCachingServiceMock,
            _productionPeriodsDataHandlerHttpClientMock);
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

        return await new ServiceCollection()
            .AddSingleton(snapshotByTimestampBatchDataLoader)
            .AddSingleton(latestSnapshotCacheDataLoader)
            .AddSingleton(_machineSnapshotHttpClientMock.Object)
            .AddSingleton(_metaDataHandlerHttpClientMock.Object)
            .AddSingleton(_latestMachineSnapshotCachingServiceMock.Object)
            .AddSingleton(_kpiDataCachingServiceMock.Object)
            .AddSingleton<IKpiService>(kpiService)
            .AddSingleton<IMachineService>(machineService)
            .AddSingleton<IProducedJobService>(producedJobService)
            .AddSingleton<IMachineSnapshotService>(machineSnapshotService)
            .AddLogging()
            .AddGraphQLServer()
            .AddAuthorization()
            .ModifyOptions(t => t.StrictValidation = false)
            .AddQueryType(q => q.Name("Query"))
            .AddType<FrameworkAPI.Queries.ProducedJobQuery>()
            .AddType<PaperSackProducedJob>()
            .AddType<ExtrusionProducedJob>()
            .AddType<PrintingProducedJob>()
            .BuildRequestExecutorAsync();
    }

    private void InitializeBottomerJob()
    {
        CreateMockForMachine(BusinessUnit.PaperSack, MachineFamily.PaperSackTuber);
        CreateMockForJob();
        KpiTestInitializer.InitializeMocks(
            MachineId,
            JobId,
            siUnitJobQuantityActual: "STK",
            siUnitJobQuantityActualInSecondUnit: string.Empty,
            siUnitMachineSpeed: "STKMIN",
            _metaDataHandlerHttpClientMock,
            _kpiDataCachingServiceMock,
            _productionPeriodsDataHandlerHttpClientMock);
    }

    private void CreateMockForMachine(BusinessUnit businessUnit, MachineFamily machineFamily)
    {
        _machineCachingServiceMock
            .Setup(s =>
                s.GetMachinesAsInternalListResponse(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalListResponse<Machine>(new List<Machine>
            {
                new()
                {
                    MachineId = MachineId,
                    BusinessUnit = businessUnit,
                    MachineFamily = machineFamily.ToString()
                }
            }));
    }

    private void CreateMockForJob()
    {
        var job = new JobInfo
        {
            MachineId = MachineId,
            ProductId = "FakeProductId",
            JobId = JobId,
            StartTime = DateTime.UnixEpoch
        };

        _productionPeriodsDataHandlerHttpClientMock
            .Setup(s => s.GetJobInfo(
                It.IsAny<CancellationToken>(),
                MachineId,
                JobId))
            .ReturnsAsync(new InternalItemResponse<JobInfo>(job));
    }
}