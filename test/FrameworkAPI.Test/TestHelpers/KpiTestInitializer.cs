using System;
using System.Collections.Generic;
using System.Threading;
using HotChocolate;
using HotChocolate.Execution;
using Moq;
using Snapshooter.Xunit;
using WuH.Ruby.Common.Core;
using WuH.Ruby.KpiDataHandler.Client;
using WuH.Ruby.MetaDataHandler.Client;
using WuH.Ruby.ProductionPeriodsDataHandler.Client;

namespace FrameworkAPI.Test.TestHelpers;

public static class KpiTestInitializer
{
    public static void InitializeMocks(
        string machineId,
        string jobId,
        string siUnitJobQuantityActual,
        string siUnitJobQuantityActualInSecondUnit,
        string siUnitMachineSpeed,
        Mock<IMetaDataHandlerHttpClient> metaDataHandlerHttpClientMock,
        Mock<IKpiDataCachingService> kpiDataCachingServiceMock,
        Mock<IProductionPeriodsDataHandlerHttpClient> productionPeriodsDataHandlerHttpClientMock)
    {
        var job = new JobInfo
        {
            MachineId = machineId,
            ProductId = "FakeProductId",
            JobId = jobId,
            StartTime = DateTime.UnixEpoch,
            EndTime = DateTime.UnixEpoch.AddHours(1)
        };

        var standardKpis = new StandardJobKpis
        {
            ProductionData = new KpiProductionTimesAndOutput
            {
                MachineId = machineId,
                From = DateTime.Parse("2024-04-25T07:22:56.664Z"),
                To = DateTime.Parse("2024-04-25T09:15:34.745Z"),
                AllQueriedTimeRanges = new()
                {
                    new()
                    {
                        From = DateTime.Parse("2024-04-25T07:22:56.664Z"),
                        To = DateTime.Parse("2024-04-25T08:55:11.519Z"),
                    },
                    new()
                    {
                        From = DateTime.Parse("2024-04-25T09:07:20.529Z"),
                        To = DateTime.Parse("2024-04-25T09:15:34.745Z"),
                    }
                },
                GoodProductionCount = 5166,
                GoodProductionCountInSecondUnit = 234,
                ScrapProductionCount = 211,
                ScrapProductionCountInSecondUnit = 456,
                SetupScrapCount = 1,
                SetupScrapCountInSecondUnit = 2,
                ProductionTimeInMin = 66.41658333333334,
                DownTimeInMin = 6.64155,
                JobRelatedDownTimeInMin = 6.64155,
                SetupTimeInMin = 27.426383333333334,
                ScrapTimeInMin = 0,
                PlannedNoProductionTimeInMin = 0,
                AverageProductionSpeed = 80.73284910018707,
                Operators = [],
                Comments = [],
                IsApparentlyWrongGoodProductionCount = true,
            },
            TargetDowntimeInMin = 7.305813333333333,
            TargetDowntimeSource = TargetSource.AdminSetting,
            TargetTotalScrapCount = 361.28,
            TargetScrapCountDuringProduction = 261.28,
            TargetSetupScrapCount = 100,
            TargetScrapSource = TargetSource.AdminSetting,
            TargetThroughputRate = 160.80526361356664,
            TargetJobTimeInMin = 62.187019101757386,
            TargetSpeedSource = TargetSource.Machine,
            TargetSpeed = 200,
            JobSize = 10000,
            TotalTimeInMin = 112.63468333333333,
            TotalPlannedProductionTimeInMin = 100.48451666666666,
            NotQueryRelatedTimeInMin = 12.150166666666664,
            TotalProductionCount = 5376,
            OriginalGoodProductionCount = 5000,
            ThroughputRate = 51.41090559391323,
            ScrapRatioTotal = 0.0390625,
            ScrapRatioDuringProduction = 0.0390625,
            SetupRatio = 0.2729413868239402,
            DownTimeRatio = 0.06609525746172171,
            Availability = 0.6609633557143381,
            Effectiveness = 0.40471819914454094,
            QualityRatio = 0.9609375,
            OEE = 0.2570545279695661,
            PerformanceComparedToTargets = new()
            {
                LostTimeDueToSpeedInMin = 38.15882310704962,
                LostTimeDueToDowntimeInMin = -0.6642633333333334,
                LostTimeDueToScrapInMin = -0.3188950339538117,
                LostTimeTotalInMin = 38.29749756490928,
                WonProductivityDueToSpeedInPercent = -0.6136139608912572,
                WonProductivityDueToDowntimeInPercent = 0.0106817,
                WonProductivityDueToScrapInPercent = 0.005127999999999997,
                WonProductivityTotalInPercent = -0.6158439191665163
            },
        };

        const string identifier1 = VariableIdentifier.JobQuantityActual;
        var variableIdentifiers = new List<string> { identifier1 };
        var processVariableMetaDataResponseItems = new List<ProcessVariableMetaDataResponseItem>
        {
            new()
            {
                Path = identifier1,
                Data = new ProcessVariableMetaData
                {
                    VariableIdentifier = identifier1,
                    Units = new VariableUnits
                    {
                        Si = new VariableUnits.UnitWithCoefficient
                        {
                            Unit = siUnitJobQuantityActual,
                            Multiplier = 1
                        }
                    }
                }
            }
        };

        if (!string.IsNullOrWhiteSpace(siUnitJobQuantityActualInSecondUnit))
        {
            const string identifier2 = VariableIdentifier.JobQuantityActualInSecondUnit;
            variableIdentifiers.Add(identifier2);

            var processVariableMetaData = new ProcessVariableMetaData
            {
                VariableIdentifier = identifier2,
                Units = new VariableUnits
                {
                    Si = new VariableUnits.UnitWithCoefficient
                    {
                        Unit = siUnitJobQuantityActualInSecondUnit,
                        Multiplier = 1
                    }
                }
            };
            processVariableMetaDataResponseItems.Add(
                new ProcessVariableMetaDataResponseItem
                {
                    Path = identifier2,
                    Data = processVariableMetaData
                });
        }

        if (!string.IsNullOrWhiteSpace(siUnitMachineSpeed))
        {
            const string identifier3 = VariableIdentifier.MachineSpeed;
            variableIdentifiers.Add(identifier3);

            var processVariableMetaData = new ProcessVariableMetaData
            {
                VariableIdentifier = identifier3,
                Units = new VariableUnits
                {
                    Si = new VariableUnits.UnitWithCoefficient
                    {
                        Unit = siUnitMachineSpeed,
                        Multiplier = 1
                    }
                }
            };
            processVariableMetaDataResponseItems.Add(
                new ProcessVariableMetaDataResponseItem
                {
                    Path = identifier3,
                    Data = processVariableMetaData
                });
        }

        metaDataHandlerHttpClientMock
            .Setup(s => s.GetProcessVariableMetaDataByIdentifiers(
                It.IsAny<CancellationToken>(), machineId, It.IsAny<List<string>>()))
            .ReturnsAsync(
                new InternalListResponse<ProcessVariableMetaDataResponseItem>(processVariableMetaDataResponseItems));

        kpiDataCachingServiceMock
            .Setup(s => s.GetStandardKpis(
                machineId,
                jobId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<StandardJobKpis>(standardKpis));

        productionPeriodsDataHandlerHttpClientMock
            .Setup(s => s.GetJobInfo(
                It.IsAny<CancellationToken>(),
                machineId,
                jobId))
            .ReturnsAsync(new InternalItemResponse<JobInfo>(job));
    }

    public static void VerifyResultAndMocks(
        IExecutionResult result,
        Mock<IMetaDataHandlerHttpClient>? metaDataHandlerHttpClientMock,
        Mock<IKpiDataCachingService>? kpiDataCachingServiceMock,
        Mock<IProductionPeriodsDataHandlerHttpClient>? productionPeriodsDataHandlerHttpClientMock)
    {
        result.ToJson().MatchSnapshot();

        metaDataHandlerHttpClientMock?.VerifyAll();
        metaDataHandlerHttpClientMock?.VerifyNoOtherCalls();

        kpiDataCachingServiceMock?.VerifyAll();
        kpiDataCachingServiceMock?.VerifyNoOtherCalls();

        productionPeriodsDataHandlerHttpClientMock?.VerifyAll();
        productionPeriodsDataHandlerHttpClientMock?.VerifyNoOtherCalls();
    }
}