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

public class PaperSackMachineProducedJobIntegrationTests
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
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock = new();
    private readonly Mock<HttpContext> _httpContextMock = new();

    [Fact]
    public async Task Get_Finished_Job_With_GoodQuantity()
    {
        // Arrange
        var executor = await InitializeExecutor();

        CreateMockForMachine(MachineFamily.PaperSackTuber);
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
                      machineId
                      ... on PaperSackProducedJob {{
                        goodQuantity {{
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
    public async Task Get_Finished_Job_With_ScrapQuantity()
    {
        // Arrange
        var executor = await InitializeExecutor();

        CreateMockForMachine(MachineFamily.PaperSackTuber);
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
                      machineId
                      ... on PaperSackProducedJob {{
                        scrapQuantity {{
                          value
                          unit
                        }},
                        setupScrapQuantity {{
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
    public async Task PaperSackTuber_MachineSetting_Cut_Type_By_Unfinished_Job_Should_Be_FlushCut()
    {
        // Arrange
        var executor = await InitializeExecutor();
        CreateMockForMachine(MachineFamily.PaperSackTuber);
        CreateMockForJob();
        CreateMockForSnapshot(
            new SnapshotColumnValueDto(
                SnapshotColumnIds.PaperSackProductIsFlushCut, true));

        // Act
        await using var result =
            await ExecutePaperSackMachineWithProducedJobsAndMachineSettingsCutTypeQuery(executor);

        // Assert
        result.ToJson().MatchSnapshot();
    }

    [Fact]
    public async Task PaperSackTuber_MachineSetting_Cut_Type_By_Unfinished_Job_Should_Be_SteppedEnd()
    {
        // Arrange
        var executor = await InitializeExecutor();

        CreateMockForMachine(MachineFamily.PaperSackTuber);
        CreateMockForJob();
        CreateMockForSnapshot(
            new SnapshotColumnValueDto(
                SnapshotColumnIds.PaperSackProductIsFlushCut, false));

        // Act
        await using var result = await ExecutePaperSackMachineWithProducedJobsAndMachineSettingsCutTypeQuery(executor);

        // Assert
        result.ToJson().MatchSnapshot();
    }

    [Fact]
    public async Task PaperSackTuber_MachineSetting_Tube_Length()
    {
        // Arrange
        var executor = await InitializeExecutor();

        CreateMockForMachine(MachineFamily.PaperSackTuber);
        CreateMockForJob();
        CreateMockForSnapshot(
            new SnapshotColumnValueDto(
                SnapshotColumnIds.PaperSackProductTubeDataTubeLength, 22.2),
            new SnapshotColumnValueDto(
                SnapshotColumnIds.PaperSackProductTubeDataTubeWidth, 1111.1));

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    producedJob(jobId: ""{JobId}"", machineId: ""{MachineId}"") {{
                      machineId
                      ... on PaperSackProducedJob {{
                        machineSettings {{
                          tubeLength {{
                            lastValue
                          }}
                          tubeWidth {{
                            lastValue
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
    public async Task PaperSackBottomer_MachineSetting_Sack_Width_And_Length_By_Unfinished_Job()
    {
        // Arrange
        var executor = await InitializeExecutor();

        CreateMockForMachine(MachineFamily.PaperSackBottomer);
        CreateMockForJob();
        CreateMockForSnapshot(
            new SnapshotColumnValueDto(
                SnapshotColumnIds.PaperSackProductSackDataSackWidth, 44.4),
            new SnapshotColumnValueDto(
                SnapshotColumnIds.PaperSackProductSackDataSackLength, 33333.3));

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    producedJob(jobId: ""{JobId}"", machineId: ""{MachineId}"") {{
                      machineId
                      ... on PaperSackProducedJob {{
                        machineSettings {{
                          sackLength {{
                            lastValue
                          }}
                          sackWidth {{
                            lastValue
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
    public async Task PaperSackBottomer_MachineSetting_StandUp_And_Valve_Bottom_Width_By_Unfinished_Job()
    {
        // Arrange
        var executor = await InitializeExecutor();

        CreateMockForMachine(MachineFamily.PaperSackBottomer);
        CreateMockForJob();
        CreateMockForSnapshot(
            new SnapshotColumnValueDto(
                SnapshotColumnIds.PaperSackProductValveBottomBottomWidth, 1000.0),
            new SnapshotColumnValueDto(
                SnapshotColumnIds.PaperSackProductStandUpBottomBottomWidth, 999.9));

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    producedJob(jobId: ""{JobId}"", machineId: ""{MachineId}"") {{
                      machineId
                      ... on PaperSackProducedJob {{
                        machineSettings {{
                          valveBottomWidth {{
                            lastValue
                          }}
                          standUpBottomWidth {{
                            lastValue
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
    public async Task PaperSackBottomer_MachineSetting_ValveSack_ValveLayers_By_Unfinished_Job()
    {
        // Arrange
        var executor = await InitializeExecutor();

        CreateMockForMachine(MachineFamily.PaperSackBottomer);
        CreateMockForJob();
        CreateMockForSnapshot(
            new SnapshotColumnValueDto(
                SnapshotColumnIds.PaperSackProductIsValveSack, false),
            new SnapshotColumnValueDto(
                SnapshotColumnIds.PaperSackProductValveLayers, 10L));

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    producedJob(jobId: ""{JobId}"", machineId: ""{MachineId}"") {{
                      machineId
                      ... on PaperSackProducedJob {{
                        machineSettings {{
                          isValveSack {{
                            lastValue
                          }}
                          valveLayers {{
                            lastValue
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
    public async Task PaperSackBottomer_MachineSetting_Valve_Bottom_Patch_By_Unfinished_Job()
    {
        // Arrange
        var executor = await InitializeExecutor();

        CreateMockForMachine(MachineFamily.PaperSackBottomer);
        CreateMockForJob();
        CreateMockForSnapshot(
            new SnapshotColumnValueDto(
                SnapshotColumnIds.PaperSackProductValveBottomHasInnerPatch, false),
            new SnapshotColumnValueDto(
                SnapshotColumnIds.PaperSackProductValveBottomHasCoverPatch, true));

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    producedJob(jobId: ""{JobId}"", machineId: ""{MachineId}"") {{
                      machineId
                      ... on PaperSackProducedJob {{
                        machineSettings {{
                          hasValveBottomPatch {{
                            lastValue
                          }}
                          hasValveBottomInnerPatch {{
                            lastValue
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
    public async Task PaperSackBottomer_MachineSetting_StandUp_Bottom_Patch_By_Unfinished_Job()
    {
        // Arrange
        var executor = await InitializeExecutor();

        CreateMockForMachine(MachineFamily.PaperSackBottomer);
        CreateMockForJob();
        CreateMockForSnapshot(
            new SnapshotColumnValueDto(
                SnapshotColumnIds.PaperSackProductStandUpBottomHasInnerPatch, true),
            new SnapshotColumnValueDto(
                SnapshotColumnIds.PaperSackProductStandUpBottomHasCoverPatch, false));

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    producedJob(jobId: ""{JobId}"", machineId: ""{MachineId}"") {{
                      machineId
                      ... on PaperSackProducedJob {{
                        machineSettings {{
                          hasStandUpBottomPatch {{
                            lastValue
                          }}
                          hasStandUpBottomInnerPatch {{
                            lastValue
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
    public async Task PaperSackBottomer_MachineSetting_StandUp_Bottom_Patch_Null()
    {
        // Arrange
        var executor = await InitializeExecutor();

        CreateMockForMachine(MachineFamily.PaperSackBottomer);
        CreateMockForJob();
        CreateMockForSnapshot(
            new SnapshotColumnValueDto(
                SnapshotColumnIds.PaperSackProductStandUpBottomHasInnerPatch, null),
            new SnapshotColumnValueDto(
                SnapshotColumnIds.PaperSackProductStandUpBottomHasCoverPatch, null));

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    producedJob(jobId: ""{JobId}"", machineId: ""{MachineId}"") {{
                      machineId
                      ... on PaperSackProducedJob {{
                        machineSettings {{
                          hasStandUpBottomPatch {{
                            lastValue
                          }}
                          hasStandUpBottomInnerPatch {{
                            lastValue
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
    public async Task PaperSackBottomer_MachineSetting_With_Null_For_Tuber_Properties_By_Unfinished_Job()
    {
        // Arrange
        var executor = await InitializeExecutor();

        CreateMockForMachine(MachineFamily.PaperSackBottomer);
        CreateMockForJob();
        CreateMockForSnapshot(
            new SnapshotColumnValueDto(
                SnapshotColumnIds.PaperSackDrvSideBottomSettingsWidth, null),
            new SnapshotColumnValueDto(
                SnapshotColumnIds.PaperSackOpsSideBottomSettingsWidth, null),
            new SnapshotColumnValueDto(
                SnapshotColumnIds.PaperSackProductSackDataSackLength, null),
            new SnapshotColumnValueDto(
                SnapshotColumnIds.PaperSackProductSackDataSackWidth, null),
            new SnapshotColumnValueDto(
                SnapshotColumnIds.PaperSackProductTubeDataTubeLength, null),
            new SnapshotColumnValueDto(
                SnapshotColumnIds.PaperSackProductIsFlushCut, null));

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    producedJob(jobId: ""{JobId}"", machineId: ""{MachineId}"") {{
                      machineId
                      ... on PaperSackProducedJob {{
                        machineSettings {{
                          valveBottomWidth {{
                            lastValue
                          }}
                          standUpBottomWidth {{
                            lastValue
                          }}
                          sackLength {{
                            lastValue
                          }}
                          sackWidth {{
                            lastValue
                          }}
                          tubeLength {{
                            lastValue
                          }}
                          cutType {{
                            lastValue
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
    public async Task PaperSackTuber_MachineSetting_With_Null_For_Bottomer_Properties_By_Unfinished_Job()
    {
        // Arrange
        var executor = await InitializeExecutor();

        CreateMockForMachine(MachineFamily.PaperSackTuber);
        CreateMockForJob();
        CreateMockForSnapshot(
            new SnapshotColumnValueDto(
                SnapshotColumnIds.PaperSackDrvSideBottomSettingsWidth, null),
            new SnapshotColumnValueDto(
                SnapshotColumnIds.PaperSackOpsSideBottomSettingsWidth, null),
            new SnapshotColumnValueDto(
                SnapshotColumnIds.PaperSackProductSackDataSackLength, null),
            new SnapshotColumnValueDto(
                SnapshotColumnIds.PaperSackProductSackDataSackWidth, null),
            new SnapshotColumnValueDto(
                SnapshotColumnIds.PaperSackProductTubeDataTubeLength, null),
            new SnapshotColumnValueDto(
                SnapshotColumnIds.PaperSackProductIsFlushCut, null));

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    producedJob(jobId: ""{JobId}"", machineId: ""{MachineId}"") {{
                      machineId
                      ... on PaperSackProducedJob {{
                        machineSettings {{
                          valveBottomWidth {{
                            lastValue
                          }}
                          standUpBottomWidth {{
                            lastValue
                          }}
                          sackLength {{
                            lastValue
                          }}
                          sackWidth {{
                            lastValue
                          }}
                          tubeLength {{
                            lastValue
                          }}
                          cutType {{
                            lastValue
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
    public async Task Get_Unfinished_Job_With_PaperSack_Base_Properties()
    {
        // Arrange
        var executor = await InitializeExecutor();

        CreateMockForMachine(MachineFamily.PaperSackBottomer);
        CreateMockForJob();
        CreateMockForSnapshot(
            new SnapshotColumnValueDto(
                SnapshotColumnIds.PaperSackMaterialText, "FakePaperSackMaterialText"),
            new SnapshotColumnValueDto(
                SnapshotColumnIds.PaperSackMaterialInformation, "FakePaperSackMaterialInformation"));

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    producedJob(jobId: ""{JobId}"", machineId: ""{MachineId}"") {{
                      machineId
                      ... on PaperSackProducedJob {{
                        endTime
                        materialText {{
                          lastValue
                        }}
                        materialInformation {{
                          lastValue
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
    public async Task Get_Job_With_PaperSack_ProductGroup_Properties()
    {
        // Arrange
        var executor = await InitializeExecutor();
        InitializeHttpContextMocks();

        CreateMockForMachine(MachineFamily.PaperSackBottomer);
        CreateMockForJob();

        var firstProductionDate = DateTime.MinValue.ToUniversalTime();
        var lastProductionDate = firstProductionDate.AddMinutes(10);

        _kpiDataHandlerClientMock
            .Setup(m => m.GetPaperSackProductGroupByJobId(It.IsAny<CancellationToken>(), MachineId, JobId))
            .ReturnsAsync(new InternalItemResponse<WuH.Ruby.KpiDataHandler.Client.Models.PaperSackProductGroup>(new WuH.Ruby.KpiDataHandler.Client.Models.PaperSackProductGroup
            {
                Id = "v0-T0-VX",
                ProductGroupDefinitionVersion = 0,
                ParentId = null,
                FriendlyName = "Product Group 1",
                Attributes = [],
                ProductIds = ["Product1", "Product2"],
                FirstProductionDate = firstProductionDate,
                LastProductionDate = lastProductionDate,
                JobIdsPerMachine = [],
                Note = null,
                TargetSpeedPerMachine = [],
                NotesPerMachine = [],
            }))
            .Verifiable(Times.Once);

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    producedJob(jobId: ""{JobId}"", machineId: ""{MachineId}"") {{
                      machineId
                      ... on PaperSackProducedJob {{
                        productGroup {{
                          id
                          friendlyName
                          firstProductionDate
                          lastProductionDate
                          parentId
                          producedJobsCount
                          productGroupDefinitionVersion
                          productIds
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
    public async Task Get_Job_With_PaperSack_ProductGroup_Properties_With_Subscription()
    {
        // Arrange
        var executor = await InitializeExecutor();
        // InitializeHttpContextMocks();

        CreateMockForMachine(MachineFamily.PaperSackBottomer);
        CreateMockForJob();

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    producedJob(jobId: ""{JobId}"", machineId: ""{MachineId}"") {{
                      machineId
                      ... on PaperSackProducedJob {{
                        productGroup {{
                          id
                        }}
                      }}
                    }}
                }}")
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        _kpiDataHandlerClientMock
            .Verify(m => m.GetPaperSackProductGroupByJobId(It.IsAny<CancellationToken>(), MachineId, JobId), Times.Never);

        result.ToJson().MatchSnapshot();
    }

    private static async Task<IExecutionResult> ExecutePaperSackMachineWithProducedJobsAndMachineSettingsCutTypeQuery(
        IRequestExecutor executor)
    {
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                $@"{{
                    producedJob(jobId: ""{JobId}"", machineId: ""{MachineId}"") {{
                      machineId
                      ... on PaperSackProducedJob {{
                        machineSettings {{
                          cutType {{
                            lastValue
                          }}
                        }}
                      }}
                    }}
                }}")
            .AddRoleClaims("go-general")
            .Create();
        return await executor.ExecuteAsync(request);
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
        var kpiEventQueueWrapperMock = new Mock<IKpiEventQueueWrapper>();
        var productGroupServiceMock = new ProductGroupService(
            machineService, kpiService, kpiEventQueueWrapperMock.Object, _kpiDataHandlerClientMock.Object, _productionPeriodsDataHandlerHttpClientMock.Object);

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
            .AddSingleton(_httpContextAccessorMock.Object)
            .AddSingleton<IKpiService>(kpiService)
            .AddSingleton<IMachineService>(machineService)
            .AddSingleton<IProducedJobService>(producedJobService)
            .AddSingleton<IMachineSnapshotService>(machineSnapshotService)
            .AddSingleton<IProductGroupService>(productGroupServiceMock)
            .AddLogging()
            .AddGraphQLServer()
            .AddAuthorization()
            .ModifyOptions(t => t.StrictValidation = false)
            .AddQueryType(q => q.Name("Query"))
            .AddType<FrameworkAPI.Queries.ProducedJobQuery>()
            .AddType<PaperSackProducedJob>()
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

    private void CreateMockForMachine(MachineFamily machineFamily)
    {
        _machineCachingServiceMock
            .Setup(s =>
                s.GetMachinesAsInternalListResponse(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalListResponse<Machine>(new List<Machine>
            {
                new()
                {
                    MachineId = MachineId,
                    BusinessUnit = BusinessUnit.PaperSack,
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

    private void CreateMockForSnapshot(params SnapshotColumnValueDto[] columns)
    {
        var snapshotMetaDto = new SnapshotMetaDto(MachineId, "fakeHash", DateTime.MinValue, new List<SnapshotColumnUnitDto>());
        var snapshotDto = new SnapshotDto(
            columns.ToList(),
            DateTime.UtcNow);

        _latestMachineSnapshotCachingServiceMock
            .Setup(s => s.GetLatestMachineSnapshot(
                MachineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new InternalItemResponse<MachineSnapshotResponse>(
                    new MachineSnapshotResponse(snapshotMetaDto, snapshotDto)));
    }
}