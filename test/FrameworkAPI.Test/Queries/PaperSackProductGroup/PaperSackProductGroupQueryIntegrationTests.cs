using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.Services;
using FrameworkAPI.Services.Interfaces;
using FrameworkAPI.Test.TestHelpers;
using HotChocolate;
using HotChocolate.Execution;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Snapshooter.Xunit;
using WuH.Ruby.KpiDataHandler.Client;
using Xunit;
using System;
using System.Collections.Generic;
using FrameworkAPI.DataLoaders;
using FrameworkAPI.Models.Enums;
using FrameworkAPI.Schema.Misc;
using FrameworkAPI.Test.Services.Helpers;
using GreenDonut;
using WuH.Ruby.KpiDataHandler.Client.Models;
using WuH.Ruby.MachineSnapShooter.Client;
using WuH.Ruby.Common.Core;
using WuH.Ruby.MetaDataHandler.Client;
using WuH.Ruby.ProductionPeriodsDataHandler.Client;
using PaperSackProductGroup = WuH.Ruby.KpiDataHandler.Client.Models.PaperSackProductGroup;
using ProductGroupSortOption = WuH.Ruby.KpiDataHandler.Client.Models.ProductGroupSortOption;

namespace FrameworkAPI.Test.Queries.PaperSackProductGroupQuery;

public class PaperSackProductGroupQueryIntegrationTests
{
    private readonly Mock<IMachineService> _machineServiceMock = new();
    private readonly Mock<IKpiService> _kpiServiceMock = new();
    private readonly Mock<IKpiDataHandlerClient> _kpiDataHandlerClientMock = new();
    private readonly Mock<IProductionPeriodsDataHandlerHttpClient> _productionPeriodsDataHandlerHttpClientMock = new();

    [Fact]
    public async Task GetGroups_With_All_Requested_Properties_Should_Return_The_Correct_Response()
    {
        // Arrange
        var executor = await InitializeExecutor();

        var productGroup = new PaperSackProductGroup()
        {
            Id = "v0-TX",
            ProductGroupDefinitionVersion = 1,
            ParentId = null,
            FriendlyName = "FriendlyName",
            FirstProductionDate = DateTime.UnixEpoch,
            LastProductionDate = DateTime.UnixEpoch.AddMinutes(1),
            Attributes = [
                new PaperSackProductGroupAttribute(PaperSackProductGroupAttributeType.Bool, SnapshotColumnIds.PaperSackProductIsValveSack, null),
                new PaperSackProductGroupAttribute(PaperSackProductGroupAttributeType.Integer, SnapshotColumnIds.PaperSackProductSackDataSackWidth, null),
                new PaperSackProductGroupAttribute(PaperSackProductGroupAttributeType.Bucket, SnapshotColumnIds.PaperSackProductTubeLayers, null)
            ],
            ProductIds = ["Product01"],
            JobIdsPerMachine = new() { { "EQ10101", ["Job01"] } },
            Note = "Note",
            TargetSpeedPerMachine = new() { { "EQ10101", 120.0 } },
            NotesPerMachine = new() { { "EQ10101", "NoteForEQ10101" } },
        };

        _kpiDataHandlerClientMock
            .Setup(mock => mock.GetPaperSackProductGroups(
                It.IsAny<CancellationToken>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<ProductGroupSortOption>()
            ))
            .ReturnsAsync(new InternalListResponse<PaperSackProductGroup>([productGroup]))
            .Verifiable();

        _kpiDataHandlerClientMock
            .Setup(mock => mock.GetPaperSackProductGroupsCount(
                It.IsAny<CancellationToken>(),
                It.IsAny<string>()
            ))
            .ReturnsAsync(new InternalItemResponse<int>(1))
            .Verifiable();

        const string machineId = "MachineA";
        _machineServiceMock
            .Setup(m => m.GetMachineIdsByFilter(null, MachineDepartment.PaperSack, MachineFamily.PaperSackTuber, It.IsAny<CancellationToken>()))
            .ReturnsAsync([machineId])
            .Verifiable();
        _machineServiceMock
            .Setup(m => m.GetMachineIdsByFilter(null, MachineDepartment.PaperSack, MachineFamily.PaperSackBottomer, It.IsAny<CancellationToken>()))
            .ReturnsAsync([])
            .Verifiable();
        _machineServiceMock
            .Setup(m => m.GetMachineFamily(machineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MachineFamily.PaperSackTuber)
            .Verifiable();
        _kpiServiceMock
            .Setup(m => m.GetProducedPerformanceUnits(It.IsAny<MachineMetaDataBatchDataLoader>(), machineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(("qualityUnit", "rateUnit", "timeUnit"))
            .Verifiable(Times.Once);
        var standardJobKpis = new StandardJobKpis
        {
            JobId = "Job01",
            ProductionData = new KpiProductionTimesAndOutput
            {
                GoodProductionCount = 100,
            }
        };
        var standardProductGroupKpis = new StandardProductGroupKpis
        {
            TotalTimeInMin = 100,
            TotalPlannedProductionTimeInMin = 200,
            NotQueryRelatedTimeInMin = 300,
            ProductionData = new KpiProductionTimesAndOutput
            {
                ProductionTimeInMin = 400,
                JobRelatedDownTimeInMin = 410,
                DownTimeInMin = 420,
                SetupTimeInMin = 430,
                ScrapTimeInMin = 440,
                PlannedNoProductionTimeInMin = 450,
                AverageProductionSpeed = 460,
            },
            TargetSpeed = 500,
            TargetSetupTimeInMin = 600,
            TargetDowntimeInMin = 700,
            TargetTotalScrapCount = 700,
            TargetThroughputRate = 800,
            ThroughputRate = 900,
            PerformanceComparedToTargets = new PerformanceComparedToTargets
            {
                LostTimeDueToSpeedInMin = 1,
                WonProductivityDueToSpeedInPercent = 2,
                LostTimeDueToSetupInMin = 10,
                WonProductivityDueToSetupInPercent = 11,
                LostTimeDueToDowntimeInMin = 20,
                WonProductivityDueToDowntimeInPercent = 21,
                LostTimeDueToScrapInMin = 30,
                WonProductivityDueToScrapInPercent = 31,
                LostTimeTotalInMin = 40,
                WonProductivityTotalInPercent = 41,
            }
        };
        var paperSackProductGroupKpisByMachineId = new Dictionary<string, PaperSackProductGroupKpis?>
        {
            { machineId, new PaperSackProductGroupKpis([standardJobKpis], standardProductGroupKpis) }
        };
        _kpiDataHandlerClientMock
            .Setup(m => m.GetPaperSackProductGroupKpis(
                It.IsAny<CancellationToken>(),
                productGroup.Id,
                It.IsAny<DateTime>(),
                It.IsAny<DateTime?>(),
                It.IsAny<List<string>>(),
                It.IsAny<string>()))
            .ReturnsAsync(new InternalItemResponse<Dictionary<string, PaperSackProductGroupKpis?>>(paperSackProductGroupKpisByMachineId));
        _kpiServiceMock
            .Setup(m => m.GetUnit(It.IsAny<MachineMetaDataBatchDataLoader>(), KpiAttribute.TargetSpeed, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("TargetSpeedUnit")
            .Verifiable(Times.Once);

        var jobInfo = new JobInfo { JobId = standardJobKpis.JobId };
        _productionPeriodsDataHandlerHttpClientMock
            .Setup(m => m.GetJobInfosByIds(It.IsAny<CancellationToken>(), machineId, It.IsAny<List<string>>()))
            .ReturnsAsync(new InternalListResponse<JobInfo>([jobInfo]))
            .Verifiable(Times.Once);

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                @"{
                    paperSackProductGroups {
                        items {
                            id
                            firstProductionDate
                            friendlyName
                            producedJobsCount
                            parentId
                            productGroupDefinitionVersion
                            lastProductionDate
                            attributes {
                              isValveSack
                              sackWidth {
                                formattedValue
                                unit
                              }
                              tubeLayers
                            }
                            productIds
                            overallNote
                            targetSpeedSettingPerMachine {
                              key
                              value {
                                  unit
                                  value
                              }
                            }
                            notePerMachine {
                              key
                              value
                            }
                            statisticsPerMachine(from: ""2023-10-25T10:34:34.372Z"") {
                                value {
                                  performance {
                                    downtime {
                                      actualValue
                                      lostTimeInMin
                                      targetValueSource
                                      targetValue
                                      unit
                                      wonProductivity
                                    }
                                    scrap {
                                      actualValue
                                      targetValue
                                      lostTimeInMin
                                      targetValueSource
                                      unit
                                      wonProductivity
                                    }
                                    setup {
                                      actualValue
                                      lostTimeInMin
                                      targetValue
                                      targetValueSource
                                      unit
                                      wonProductivity
                                    }
                                    speed {
                                      actualValue
                                      lostTimeInMin
                                      targetValue
                                      targetValueSource
                                      unit
                                      wonProductivity
                                    }
                                    total {
                                      actualValue
                                      lostTimeInMin
                                      targetValueSource
                                      targetValue
                                      unit
                                      wonProductivity
                                    }
                                  }
                                  productionTimes {
                                    generalDownTimeInMin
                                    jobRelatedDownTimeInMin
                                    notQueryRelatedTimeInMin
                                    scheduledNonProductionTimeInMin
                                    productionTimeInMin
                                    scrapTimeInMin
                                    totalPlannedProductionTimeInMin
                                    totalTimeInMin
                                    setupTimeInMin
                                  }
                                  totalProducedGoodQuantity
                                }
                              }
                        }
                        pageInfo {
                            hasNextPage
                            hasPreviousPage
                        }
                        totalCount
                    }
                }")
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();
        _kpiDataHandlerClientMock.VerifyAll();
        _kpiDataHandlerClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetGroups_With_Partial_Requested_Properties_Should_Return_The_Correct_Response()
    {
        // Arrange
        var executor = await InitializeExecutor();

        var productGroup = new PaperSackProductGroup()
        {
            Id = "v0-TX",
            ProductGroupDefinitionVersion = 1,
            ParentId = null,
            FriendlyName = "FriendlyName",
            FirstProductionDate = DateTime.UnixEpoch,
            LastProductionDate = DateTime.UnixEpoch.AddMinutes(1),
            Attributes = [
                new PaperSackProductGroupAttribute(PaperSackProductGroupAttributeType.Bool, SnapshotColumnIds.PaperSackProductIsValveSack, null),
                new PaperSackProductGroupAttribute(PaperSackProductGroupAttributeType.Bucket, SnapshotColumnIds.PaperSackProductTubeLayers, null)
            ],
            ProductIds = ["Product01"],
            JobIdsPerMachine = new() { { "EQ10101", ["Job01"] } },
            Note = "Note",
            TargetSpeedPerMachine = new() { { "EQ10101", 120.0 } },
            NotesPerMachine = new() { { "EQ10101", "NoteForEQ10101" } },
        };

        _kpiDataHandlerClientMock
            .Setup(mock => mock.GetPaperSackProductGroups(
                It.IsAny<CancellationToken>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<ProductGroupSortOption>()
            ))
            .ReturnsAsync(new InternalListResponse<PaperSackProductGroup>([productGroup]))
            .Verifiable();

        _kpiDataHandlerClientMock
            .Setup(mock => mock.GetPaperSackProductGroupsCount(
                It.IsAny<CancellationToken>(),
                It.IsAny<string>()
            ))
            .ReturnsAsync(new InternalItemResponse<int>(1))
            .Verifiable();

        const string machineId = "MachineA";
        _machineServiceMock
            .Setup(m => m.GetMachineIdsByFilter(null, MachineDepartment.PaperSack, MachineFamily.PaperSackTuber, It.IsAny<CancellationToken>()))
            .ReturnsAsync([machineId])
            .Verifiable();
        _machineServiceMock
            .Setup(m => m.GetMachineIdsByFilter(null, MachineDepartment.PaperSack, MachineFamily.PaperSackBottomer, It.IsAny<CancellationToken>()))
            .ReturnsAsync([])
            .Verifiable();
        _machineServiceMock
            .Setup(m => m.GetMachineFamily(machineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MachineFamily.PaperSackTuber)
            .Verifiable();
        _kpiServiceMock
            .Setup(m => m.GetProducedPerformanceUnits(It.IsAny<MachineMetaDataBatchDataLoader>(), machineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(("qualityUnit", "rateUnit", "timeUnit"))
            .Verifiable(Times.Once);
        var standardJobKpis = new StandardJobKpis
        {
            ProductionData = new KpiProductionTimesAndOutput
            {
                GoodProductionCount = 100,
            }
        };
        var standardProductGroupKpis = new StandardProductGroupKpis
        {
            TotalTimeInMin = 100,
            TotalPlannedProductionTimeInMin = 200,
            NotQueryRelatedTimeInMin = 300,
            ProductionData = new KpiProductionTimesAndOutput
            {
                ProductionTimeInMin = 400,
                JobRelatedDownTimeInMin = 410,
                DownTimeInMin = 420,
                SetupTimeInMin = 430,
                ScrapTimeInMin = 440,
                PlannedNoProductionTimeInMin = 450,
                AverageProductionSpeed = 460,
            },
            TargetSpeed = 500,
            TargetSetupTimeInMin = 600,
            TargetDowntimeInMin = 700,
            TargetTotalScrapCount = 700,
            TargetThroughputRate = 800,
            ThroughputRate = 900,
            PerformanceComparedToTargets = new PerformanceComparedToTargets
            {
                LostTimeDueToSpeedInMin = 1,
                WonProductivityDueToSpeedInPercent = 2,
                LostTimeDueToSetupInMin = 10,
                WonProductivityDueToSetupInPercent = 11,
                LostTimeDueToDowntimeInMin = 20,
                WonProductivityDueToDowntimeInPercent = 21,
                LostTimeDueToScrapInMin = 30,
                WonProductivityDueToScrapInPercent = 31,
                LostTimeTotalInMin = 40,
                WonProductivityTotalInPercent = 41,
            }
        };
        var paperSackProductGroupKpisByMachineId = new Dictionary<string, PaperSackProductGroupKpis?>
        {
            { machineId, new PaperSackProductGroupKpis([standardJobKpis], standardProductGroupKpis) }
        };
        _kpiDataHandlerClientMock
            .Setup(m => m.GetPaperSackProductGroupKpis(
                It.IsAny<CancellationToken>(),
                productGroup.Id,
                It.IsAny<DateTime>(),
                It.IsAny<DateTime?>(),
                It.IsAny<List<string>>(),
                It.IsAny<string>()))
            .ReturnsAsync(new InternalItemResponse<Dictionary<string, PaperSackProductGroupKpis?>>(paperSackProductGroupKpisByMachineId));
        _kpiServiceMock
            .Setup(m => m.GetUnit(It.IsAny<MachineMetaDataBatchDataLoader>(), KpiAttribute.TargetSpeed, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("TargetSpeedUnit")
            .Verifiable(Times.Once);

        var jobInfo = new JobInfo { JobId = standardJobKpis.JobId };
        _productionPeriodsDataHandlerHttpClientMock
            .Setup(m => m.GetJobInfosByIds(It.IsAny<CancellationToken>(), machineId, It.IsAny<List<string>>()))
            .ReturnsAsync(new InternalListResponse<JobInfo>([jobInfo]))
            .Verifiable(Times.Once);

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                @"{
                    paperSackProductGroups {
                        items {
                            id
                            firstProductionDate
                            friendlyName
                            producedJobsCount
                            parentId
                            productGroupDefinitionVersion
                            lastProductionDate
                            attributes {
                              isValveSack
                              tubeLayers
                            }
                            productIds
                            overallNote
                            targetSpeedSettingPerMachine {
                              key
                              value {
                                  unit
                                  value
                              }
                            }
                            notePerMachine {
                              key
                              value
                            }
                            statisticsPerMachine(from: ""2023-10-25T10:34:34.372Z"") {
                                value {
                                  performance {
                                    downtime {
                                      actualValue
                                      lostTimeInMin
                                      targetValueSource
                                      targetValue
                                      unit
                                      wonProductivity
                                    }
                                    scrap {
                                      actualValue
                                      targetValue
                                      lostTimeInMin
                                      targetValueSource
                                      unit
                                      wonProductivity
                                    }
                                    setup {
                                      actualValue
                                      lostTimeInMin
                                      targetValue
                                      targetValueSource
                                      unit
                                      wonProductivity
                                    }
                                    speed {
                                      actualValue
                                      lostTimeInMin
                                      targetValue
                                      targetValueSource
                                      unit
                                      wonProductivity
                                    }
                                    total {
                                      actualValue
                                      lostTimeInMin
                                      targetValueSource
                                      targetValue
                                      unit
                                      wonProductivity
                                    }
                                  }
                                  productionTimes {
                                    generalDownTimeInMin
                                    jobRelatedDownTimeInMin
                                    notQueryRelatedTimeInMin
                                    scheduledNonProductionTimeInMin
                                    productionTimeInMin
                                    scrapTimeInMin
                                    totalPlannedProductionTimeInMin
                                    totalTimeInMin
                                    setupTimeInMin
                                  }
                                  totalProducedGoodQuantity
                                }
                              }
                        }
                        pageInfo {
                            hasNextPage
                            hasPreviousPage
                        }
                        totalCount
                    }
                }")
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();
        _kpiDataHandlerClientMock.VerifyAll();
        _kpiDataHandlerClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetGroups_With_No_Groups_Should_Return_The_Correct_Response()
    {
        // Arrange
        var executor = await InitializeExecutor();

        _kpiDataHandlerClientMock
            .Setup(mock => mock.GetPaperSackProductGroups(
                It.IsAny<CancellationToken>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<ProductGroupSortOption>()
            ))
            .ReturnsAsync(new InternalListResponse<PaperSackProductGroup>([]))
            .Verifiable();

        _kpiDataHandlerClientMock
            .Setup(mock => mock.GetPaperSackProductGroupsCount(
                It.IsAny<CancellationToken>(),
                It.IsAny<string>()
            ))
            .ReturnsAsync(new InternalItemResponse<int>(0))
            .Verifiable();

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                @"{
                    paperSackProductGroups {
                        items {
                            id
                        }
                        totalCount
                    }
                }")
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();
        _kpiDataHandlerClientMock.VerifyAll();
        _kpiDataHandlerClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetGroups_With_Limit_And_Multiple_Pages_Should_Return_The_Correct_Response()
    {
        // Arrange
        var executor = await InitializeExecutor();

        var productGroup1 = new PaperSackProductGroup()
        {
            Id = "v0-TX",
            ProductGroupDefinitionVersion = 1,
            ParentId = null,
            FriendlyName = "FriendlyName",
            FirstProductionDate = DateTime.UnixEpoch,
            LastProductionDate = DateTime.UnixEpoch.AddMinutes(1),
            Attributes = [new PaperSackProductGroupAttribute(PaperSackProductGroupAttributeType.Bool, SnapshotColumnIds.PaperSackProductIsValveSack, null)],
            ProductIds = ["Product01"],
            JobIdsPerMachine = new() { { "EQ10101", ["Job01"] } },
            Note = "Note",
            TargetSpeedPerMachine = new() { { "EQ10101", 120.0 } },
            NotesPerMachine = new() { { "EQ10101", "NoteForEQ10101" } },
        };

        _kpiDataHandlerClientMock
            .Setup(mock => mock.GetPaperSackProductGroups(
                It.IsAny<CancellationToken>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<ProductGroupSortOption>()
            ))
            .ReturnsAsync(new InternalListResponse<PaperSackProductGroup>([productGroup1]))
            .Verifiable();

        _kpiDataHandlerClientMock
            .Setup(mock => mock.GetPaperSackProductGroupsCount(
                It.IsAny<CancellationToken>(),
                It.IsAny<string>()
            ))
            .ReturnsAsync(new InternalItemResponse<int>(2))
            .Verifiable();

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                @"{
                    paperSackProductGroups(take: 1) {
                        items {
                            id
                        }
                        pageInfo {
                            hasNextPage
                            hasPreviousPage
                        }
                        totalCount
                    }
                }")
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();
        _kpiDataHandlerClientMock.VerifyAll();
        _kpiDataHandlerClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetGroup_With_All_Requested_Properties_Should_Return_The_Correct_Response()
    {
        // Arrange
        var executor = await InitializeExecutor();

        var productGroup = new PaperSackProductGroup()
        {
            Id = "v0-TX",
            ProductGroupDefinitionVersion = 1,
            ParentId = null,
            FriendlyName = "FriendlyName",
            FirstProductionDate = DateTime.UnixEpoch,
            LastProductionDate = DateTime.UnixEpoch.AddMinutes(1),
            Attributes = [
                new PaperSackProductGroupAttribute(PaperSackProductGroupAttributeType.Bool, SnapshotColumnIds.PaperSackProductIsValveSack, null),
                new PaperSackProductGroupAttribute(PaperSackProductGroupAttributeType.Integer, SnapshotColumnIds.PaperSackProductSackDataSackWidth, null),
                new PaperSackProductGroupAttribute(PaperSackProductGroupAttributeType.Bucket, SnapshotColumnIds.PaperSackProductTubeLayers, null)
            ],
            ProductIds = ["Product01"],
            JobIdsPerMachine = new() { { "EQ10101", ["Job01"] } },
            Note = "Note",
            TargetSpeedPerMachine = new() { { "EQ10101", 120.0 } },
            NotesPerMachine = new() { { "EQ10101", "NoteForEQ10101" } },
        };

        _kpiDataHandlerClientMock
            .Setup(mock => mock.GetPaperSackProductGroupById(
                It.IsAny<CancellationToken>(),
                It.IsAny<string>()
            ))
            .ReturnsAsync(new InternalItemResponse<PaperSackProductGroup>(productGroup))
            .Verifiable(Times.Once);
        _kpiServiceMock
            .Setup(m => m.GetUnit(It.IsAny<MachineMetaDataBatchDataLoader>(), KpiAttribute.TargetSpeed, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("TargetSpeedUnit")
            .Verifiable(Times.Once);

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                @"{
                    paperSackProductGroup(id: ""v0-TX"") {
                        id
                        firstProductionDate
                        friendlyName
                        producedJobsCount
                        parentId
                        productGroupDefinitionVersion
                        lastProductionDate
                        attributes {
                            isValveSack
                            sackWidth {
                                formattedValue
                                unit
                            }
                            tubeLayers
                        }
                        productIds
                        overallNote
                        targetSpeedSettingPerMachine {
                            key
                            value {
                                unit
                             value
                            }
                        }
                        notePerMachine {
                            key
                            value
                        }
                    }
                }")
            .AddRoleClaims("go-general")
            .Create();

        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();
        _kpiDataHandlerClientMock.VerifyAll();
        _kpiDataHandlerClientMock.VerifyNoOtherCalls();
    }

    private async Task<IRequestExecutor> InitializeExecutor()
    {
        var metaDataHandlerHttpClient = new Mock<IMetaDataHandlerHttpClient>(MockBehavior.Strict);

        var productGroupStandardKpiCacheDataLoader = new ProductGroupStandardKpiCacheDataLoader(_kpiDataHandlerClientMock.Object, new DataLoaderOptions());
        var machineMetaDataBatchDataLoader = new MachineMetaDataBatchDataLoader(metaDataHandlerHttpClient.Object, new DelayedBatchScheduler(), new DataLoaderOptions());

        var kpiEventQueueWrapperMock = new Mock<IKpiEventQueueWrapper>();

        return await new ServiceCollection()
            .AddSingleton<IProductGroupService>(new ProductGroupService(
                _machineServiceMock.Object,
                _kpiServiceMock.Object,
                kpiEventQueueWrapperMock.Object,
                _kpiDataHandlerClientMock.Object,
                _productionPeriodsDataHandlerHttpClientMock.Object))
            .AddSingleton(_kpiServiceMock.Object)
            .AddSingleton(productGroupStandardKpiCacheDataLoader)
            .AddSingleton(machineMetaDataBatchDataLoader)
            .AddLogging()
            .AddGraphQLServer()
            .AddAuthorization()
            .ModifyOptions(t => t.StrictValidation = false)
            .AddQueryType(q => q.Name("Query"))
            .AddType<FrameworkAPI.Queries.PaperSackProductGroupQuery>()
            .AddSorting()
            .AddFiltering()
            .BuildRequestExecutorAsync();
    }
}