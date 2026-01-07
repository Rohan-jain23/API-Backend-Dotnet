using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using FrameworkAPI.DataLoaders;
using FrameworkAPI.Exceptions;
using FrameworkAPI.Models.Enums;
using FrameworkAPI.Schema.Misc;
using FrameworkAPI.Services;
using FrameworkAPI.Services.Interfaces;
using FrameworkAPI.Test.Services.Helpers;
using GreenDonut;
using Microsoft.AspNetCore.Http;
using Moq;
using Moq.Language.Flow;
using WuH.Ruby.Common.Core;
using WuH.Ruby.KpiDataHandler.Client;
using WuH.Ruby.KpiDataHandler.Client.Models;
using WuH.Ruby.MachineSnapShooter.Client;
using WuH.Ruby.MetaDataHandler.Client;
using WuH.Ruby.ProductionPeriodsDataHandler.Client;
using Xunit;
using ProductGroupSchema = FrameworkAPI.Schema.ProductGroup;

namespace FrameworkAPI.Test.Services;

public class ProductGroupServiceTests
{
    private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());
    private readonly PaperSackProductGroup _paperSackProductGroup;
    private readonly Mock<IMachineService> _machineServiceMock = new(MockBehavior.Strict);
    private readonly Mock<IKpiService> _kpiServiceMock = new(MockBehavior.Strict);
    private readonly Mock<IKpiEventQueueWrapper> _kpiEventQueueWrapperMock = new(MockBehavior.Strict);
    private readonly Mock<IKpiDataHandlerClient> _kpiDataHandlerMock = new(MockBehavior.Strict);
    private readonly Mock<IMetaDataHandlerHttpClient> _metaDataHandlerHttpClientMock = new(MockBehavior.Strict);
    private readonly Mock<IProductionPeriodsDataHandlerHttpClient> _productionPeriodsDataHandlerHttpClientMock = new(MockBehavior.Strict);
    private readonly ProductGroupService _subject;
    private readonly MachineMetaDataBatchDataLoader _machineMetaDataBatchDataLoader;
    private readonly ProductGroupStandardKpiCacheDataLoader _productGroupStandardKpiCacheDataLoader;

    public ProductGroupServiceTests()
    {
        var paperSackProductGroupAttribute = new PaperSackProductGroupAttribute(PaperSackProductGroupAttributeType.Bool, SnapshotColumnIds.PaperSackProductIsValveSack, true);
        _paperSackProductGroup = _fixture.Build<PaperSackProductGroup>().With(x => x.Attributes, [paperSackProductGroupAttribute]).Create();

        _subject = new ProductGroupService(
            _machineServiceMock.Object,
            _kpiServiceMock.Object,
            _kpiEventQueueWrapperMock.Object,
            _kpiDataHandlerMock.Object,
            _productionPeriodsDataHandlerHttpClientMock.Object);

        _productGroupStandardKpiCacheDataLoader = new ProductGroupStandardKpiCacheDataLoader(_kpiDataHandlerMock.Object, new DataLoaderOptions());
        _machineMetaDataBatchDataLoader = new MachineMetaDataBatchDataLoader(_metaDataHandlerHttpClientMock.Object, new DelayedBatchScheduler(), new DataLoaderOptions());
    }

    [Fact]
    public async Task GetPaperSackProductGroupById_With_Error_In_KpiDataHandler_Returns_Error()
    {
        // Arrange
        MockGetPaperSackProductGroupById(new InternalItemResponse<PaperSackProductGroup>(400, "Test Error"))
            .Verifiable(Times.Once);

        // Act
        var action = () => _subject.GetPaperSackProductGroupById("Test", CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<InternalServiceException>();
    }

    [Fact]
    public async Task GetPaperSackProductGroupById_Returns_Success()
    {
        // Arrange
        MockGetPaperSackProductGroupById()
            .Verifiable(Times.Once);

        // Act
        var response = await _subject.GetPaperSackProductGroupById(_paperSackProductGroup.Id, CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response.Id.Should().Be(_paperSackProductGroup.Id);
    }

    [Fact]
    public async Task GetPaperSackProductGroupByJobId_With_NoContent_In_KpiDataHandler_Returns_Null()
    {
        // Arrange
        var machineId = _fixture.Create<string>();
        var jobId = _fixture.Create<string>();
        MockGetPaperSackProductGroupByJobId(machineId, jobId, new InternalItemResponse<PaperSackProductGroup>(StatusCodes.Status204NoContent, "No content"))
            .Verifiable(Times.Once);

        // Act
        var response = await _subject.GetPaperSackProductGroupByJobId(machineId, jobId, CancellationToken.None);

        // Assert
        response.Should().BeNull();
    }

    [Fact]
    public async Task GetPaperSackProductGroupByJobId_With_Error_In_KpiDataHandler_Returns_Error()
    {
        // Arrange
        var machineId = _fixture.Create<string>();
        var jobId = _fixture.Create<string>();
        MockGetPaperSackProductGroupByJobId(machineId, jobId, new InternalItemResponse<PaperSackProductGroup>(400, "Test Error"))
            .Verifiable(Times.Once);

        // Act
        var action = () => _subject.GetPaperSackProductGroupByJobId(machineId, jobId, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<InternalServiceException>();
    }

    [Fact]
    public async Task GetPaperSackProductGroupByJobId_Returns_Success()
    {
        // Arrange
        var machineId = _fixture.Create<string>();
        var jobId = _fixture.Create<string>();
        MockGetPaperSackProductGroupByJobId(machineId, jobId)
            .Verifiable(Times.Once);

        // Act
        var response = await _subject.GetPaperSackProductGroupByJobId(machineId, jobId, CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response.Id.Should().Be(_paperSackProductGroup.Id);
    }

    [Fact]
    public async Task GetPaperSackProductGroups_With_Error_In_KpiDataHandler_Returns_Error()
    {
        // Arrange
        MockGetPaperSackProductGroups(new InternalListResponse<PaperSackProductGroup>(400, "Test Error"))
            .Verifiable(Times.Once);

        // Act
        var action = () => _subject.GetPaperSackProductGroups(string.Empty, 1, 0, ProductGroupSchema.ProductGroupSortOption.None, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<InternalServiceException>();
    }

    [Fact]
    public async Task GetPaperSackProductGroups_With_No_Groups_In_KpiDataHandler_Returns_Empty_List()
    {
        // Arrange
        MockGetPaperSackProductGroups(new InternalListResponse<PaperSackProductGroup>([]))
            .Verifiable(Times.Once);

        // Act
        var response = await _subject.GetPaperSackProductGroups(string.Empty, 1, 0, ProductGroupSchema.ProductGroupSortOption.None, CancellationToken.None);

        // Assert
        response.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPaperSackProductGroups_With_Empty_Error_In_KpiDataHandler_Returns_Empty_List()
    {
        // Arrange
        MockGetPaperSackProductGroups(new InternalListResponse<PaperSackProductGroup>(204, "Empty Error"))
            .Verifiable(Times.Once);

        // Act
        var response = await _subject.GetPaperSackProductGroups(string.Empty, 1, 0, ProductGroupSchema.ProductGroupSortOption.None, CancellationToken.None);

        // Assert
        response.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPaperSackProductGroups_Returns_Success()
    {
        // Arrange
        MockGetPaperSackProductGroups()
            .Verifiable(Times.Once);

        // Act
        var response = (await _subject.GetPaperSackProductGroups(
            string.Empty, 1, 0, ProductGroupSchema.ProductGroupSortOption.None, CancellationToken.None)).ToList();

        // Assert
        response.Should().NotBeNull();
        response.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetPaperSackProductGroupsCount_With_Error_In_KpiDataHandler_Returns_Error()
    {
        // Arrange
        MockGetPaperSackProductGroupsCount(new InternalItemResponse<int>(400, "Test Error"))
            .Verifiable();

        // Act
        var action = () => _subject.GetPaperSackProductGroupsCount(null, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<InternalServiceException>();
    }

    [Fact]
    public async Task GetPaperSackProductGroupsCount_Returns_Success()
    {
        // Arrange
        MockGetPaperSackProductGroupsCount()
            .Verifiable();

        // Act
        var response = await _subject.GetPaperSackProductGroupsCount(null, CancellationToken.None);

        // Assert
        response.Should().Be(5);
    }

    [Fact]
    public async Task GetPaperSackProductGroupStatisticsPerMachine_WithMandatoryFilterAndPaperSackMachineFamilyFilterBoth_GetsMachineIdsAndUsesDataLoader_ReturnsMappedPaperSackProductGroupStatisticsPerMachine()
    {
        // Arrange
        var paperSackProductGroupId = _fixture.Create<string>();
        var from = _fixture.Create<DateTime>();
        const PaperSackMachineFamilyFilter machineFamilyFilter = PaperSackMachineFamilyFilter.Both;
        var machineIds = _fixture.CreateMany<string>(3).ToList();

        var tuberMachineIds = machineIds[..2];
        MockGetMachineIdsByFilter(machineIdFilter: null, MachineDepartment.PaperSack, MachineFamily.PaperSackTuber, tuberMachineIds)
            .Verifiable(Times.Once);

        var bottomerMachineIds = machineIds[2..];
        MockGetMachineIdsByFilter(machineIdFilter: null, MachineDepartment.PaperSack, MachineFamily.PaperSackBottomer, bottomerMachineIds)
            .Verifiable(Times.Once);

        var paperSackProductGroupKpisByMachineId = machineIds.ToDictionary(
            machineId => machineId, PaperSackProductGroupKpis? (_) => _fixture.Create<PaperSackProductGroupKpis>());

        MockGetPaperSackProductGroupKpis(
                paperSackProductGroupId,
                machineIds,
                from,
                to: null,
                productIdFilter: null,
                new InternalItemResponse<Dictionary<string, PaperSackProductGroupKpis?>>(paperSackProductGroupKpisByMachineId))
            .Verifiable(Times.Once);

        var qualityUnit = _fixture.Create<string>();
        var rateUnit = _fixture.Create<string>();
        var timeUnit = _fixture.Create<string>();
        MockGetProducedPerformanceUnits(machineIds, qualityUnit, rateUnit, timeUnit);

        MockGetMachineFamily(tuberMachineIds, MachineFamily.PaperSackTuber);
        MockGetMachineFamily(bottomerMachineIds, MachineFamily.PaperSackBottomer);

        foreach (var (machineId, paperSackProductGroupKpis) in paperSackProductGroupKpisByMachineId)
        {
            MockGetJobInfosByIds(
                    machineId,
                    jobIds: paperSackProductGroupKpis!.StandardJobKpis.Select(m => m.JobId).ToList())
                .Verifiable(Times.Once);
        }

        // Act
        var response = await _subject.GetPaperSackProductGroupStatisticsPerMachine(
            _productGroupStandardKpiCacheDataLoader,
            _machineMetaDataBatchDataLoader,
            paperSackProductGroupId,
            from,
            to: null,
            productIdFilter: null,
            machineIdFilter: null,
            machineFamilyFilter,
            CancellationToken.None);

        // Assert
        response.Should().HaveCount(paperSackProductGroupKpisByMachineId.Count);
        response.Should().ContainKeys(machineIds);
    }

    [Fact]
    public async Task GetPaperSackProductGroupStatisticsPerMachine_WithAllFilterAndPaperSackMachineFamilyFilterBoth_GetsMachineIdsAndUsesDataLoader_ReturnsMappedPaperSackProductGroupStatisticsPerMachine()
    {
        // Arrange
        var paperSackProductGroupId = _fixture.Create<string>();
        var from = _fixture.Create<DateTime>();
        var to = _fixture.Create<DateTime>();
        var productIdFilter = _fixture.Create<string>();
        var machineIdFilter = _fixture.Create<string>();
        const PaperSackMachineFamilyFilter machineFamilyFilter = PaperSackMachineFamilyFilter.Tuber;

        MockGetMachineIdsByFilter(machineIdFilter: machineIdFilter, MachineDepartment.PaperSack, MachineFamily.PaperSackTuber, [machineIdFilter])
            .Verifiable(Times.Once);

        var histogramItems = new List<HistogramItem>
        {
            new HistogramItem(null, 50, 80),
            new HistogramItem(0.3, 180, 90),
            new HistogramItem(0.5, 250, 100),
            new HistogramItem(0.1, 140, 110),
        };
        var firstStandardJobKpis = _fixture.Build<StandardJobKpis>()
            .With(m => m.ProductionData, _fixture.Build<KpiProductionTimesAndOutput>()
                .With(m => m.GoodProductionCount, 100)
                .Create())
            .Create();
        var secondStandardJobKpis = _fixture.Build<StandardJobKpis>()
            .With(m => m.ProductionData, _fixture.Build<KpiProductionTimesAndOutput>()
                .With(m => m.GoodProductionCount, 50)
                .Create())
            .Create();
        var standardProductGroupKpis = _fixture.Build<StandardProductGroupKpis>()
            .With(m => m.HistogramItems, histogramItems)
            .With(m => m.ProductionData, _fixture.Build<KpiProductionTimesAndOutput>()
                .With(m => m.DownTimeInMin, 50)
                .With(m => m.JobRelatedDownTimeInMin, 10)
                .With(m => m.ScrapProductionCount, 100)
                .With(m => m.SetupScrapCount, 25)
                .Create())
            .Create();
        var paperSackProductGroupKpisByMachineId = new Dictionary<string, PaperSackProductGroupKpis?>
        {
            { machineIdFilter, new PaperSackProductGroupKpis([firstStandardJobKpis, secondStandardJobKpis], standardProductGroupKpis) }
        };

        MockGetPaperSackProductGroupKpis(
                paperSackProductGroupId,
                machineIds: [machineIdFilter],
                from,
                to,
                productIdFilter,
                new InternalItemResponse<Dictionary<string, PaperSackProductGroupKpis?>>(paperSackProductGroupKpisByMachineId))
            .Verifiable(Times.Once);

        var qualityUnit = _fixture.Create<string>();
        var rateUnit = _fixture.Create<string>();
        var timeUnit = _fixture.Create<string>();
        MockGetProducedPerformanceUnits([machineIdFilter], qualityUnit, rateUnit, timeUnit);

        MockGetMachineFamily([machineIdFilter], MachineFamily.PaperSackTuber);

        MockGetJobInfosByIds(
                machineIdFilter,
                jobIds: [firstStandardJobKpis.JobId, secondStandardJobKpis.JobId])
            .Verifiable(Times.Once);

        // Act
        var response = await _subject.GetPaperSackProductGroupStatisticsPerMachine(
            _productGroupStandardKpiCacheDataLoader,
            _machineMetaDataBatchDataLoader,
            paperSackProductGroupId,
            from,
            to,
            productIdFilter,
            machineIdFilter,
            machineFamilyFilter,
            CancellationToken.None);

        // Assert
        response.Should().HaveCount(1);
        response.Should().ContainKey(machineIdFilter);

        var paperSackProductGroupStatistics = response[machineIdFilter]!;

        paperSackProductGroupStatistics.TotalProducedGoodQuantity.Should().Be(150);

        paperSackProductGroupStatistics.ProductionTimes.TotalTimeInMin.Should().Be(standardProductGroupKpis.TotalTimeInMin);
        paperSackProductGroupStatistics.ProductionTimes.TotalPlannedProductionTimeInMin.Should().Be(standardProductGroupKpis.TotalPlannedProductionTimeInMin);
        paperSackProductGroupStatistics.ProductionTimes.NotQueryRelatedTimeInMin.Should().Be(standardProductGroupKpis.NotQueryRelatedTimeInMin);
        paperSackProductGroupStatistics.ProductionTimes.ProductionTimeInMin.Should().Be(standardProductGroupKpis.ProductionData.ProductionTimeInMin);
        paperSackProductGroupStatistics.ProductionTimes.GeneralDownTimeInMin.Should().Be(40);
        paperSackProductGroupStatistics.ProductionTimes.JobRelatedDownTimeInMin.Should().Be(standardProductGroupKpis.ProductionData.JobRelatedDownTimeInMin);
        paperSackProductGroupStatistics.ProductionTimes.SetupTimeInMin.Should().Be(standardProductGroupKpis.ProductionData.SetupTimeInMin);
        paperSackProductGroupStatistics.ProductionTimes.ScrapTimeInMin.Should().Be(standardProductGroupKpis.ProductionData.ScrapTimeInMin);
        paperSackProductGroupStatistics.ProductionTimes.ScheduledNonProductionTimeInMin.Should().Be(standardProductGroupKpis.ProductionData.PlannedNoProductionTimeInMin);

        paperSackProductGroupStatistics.Performance.Speed.ActualValue.Should().Be(standardProductGroupKpis.ProductionData.AverageProductionSpeed);
        paperSackProductGroupStatistics.Performance.Speed.TargetValue.Should().Be(standardProductGroupKpis.TargetSpeed);
        paperSackProductGroupStatistics.Performance.Speed.TargetValueSource.Should().BeNull();
        paperSackProductGroupStatistics.Performance.Speed.LostTimeInMin.Should().Be(standardProductGroupKpis.PerformanceComparedToTargets.LostTimeDueToSpeedInMin);
        paperSackProductGroupStatistics.Performance.Speed.WonProductivity.Should().Be(standardProductGroupKpis.PerformanceComparedToTargets.WonProductivityDueToSpeedInPercent);
        paperSackProductGroupStatistics.Performance.Speed.Unit.Should().Be(rateUnit);

        paperSackProductGroupStatistics.Performance.Setup.ActualValue.Should().Be(standardProductGroupKpis.ProductionData.SetupTimeInMin);
        paperSackProductGroupStatistics.Performance.Setup.TargetValue.Should().Be(standardProductGroupKpis.TargetSetupTimeInMin);
        paperSackProductGroupStatistics.Performance.Setup.TargetValueSource.Should().BeNull();
        paperSackProductGroupStatistics.Performance.Setup.LostTimeInMin.Should().Be(standardProductGroupKpis.PerformanceComparedToTargets.LostTimeDueToSetupInMin);
        paperSackProductGroupStatistics.Performance.Setup.WonProductivity.Should().Be(standardProductGroupKpis.PerformanceComparedToTargets.WonProductivityDueToSetupInPercent);
        paperSackProductGroupStatistics.Performance.Setup.Unit.Should().Be(timeUnit);

        paperSackProductGroupStatistics.Performance.Downtime.ActualValue.Should().Be(standardProductGroupKpis.ProductionData.JobRelatedDownTimeInMin);
        paperSackProductGroupStatistics.Performance.Downtime.TargetValue.Should().Be(standardProductGroupKpis.TargetDowntimeInMin);
        paperSackProductGroupStatistics.Performance.Downtime.TargetValueSource.Should().BeNull();
        paperSackProductGroupStatistics.Performance.Downtime.LostTimeInMin.Should().Be(standardProductGroupKpis.PerformanceComparedToTargets.LostTimeDueToDowntimeInMin);
        paperSackProductGroupStatistics.Performance.Downtime.WonProductivity.Should().Be(standardProductGroupKpis.PerformanceComparedToTargets.WonProductivityDueToDowntimeInPercent);
        paperSackProductGroupStatistics.Performance.Downtime.Unit.Should().Be(timeUnit);

        paperSackProductGroupStatistics.Performance.Scrap.ActualValue.Should().Be(75);
        paperSackProductGroupStatistics.Performance.Scrap.TargetValue.Should().Be(standardProductGroupKpis.TargetTotalScrapCount);
        paperSackProductGroupStatistics.Performance.Scrap.TargetValueSource.Should().BeNull();
        paperSackProductGroupStatistics.Performance.Scrap.LostTimeInMin.Should().Be(standardProductGroupKpis.PerformanceComparedToTargets.LostTimeDueToScrapInMin);
        paperSackProductGroupStatistics.Performance.Scrap.WonProductivity.Should().Be(standardProductGroupKpis.PerformanceComparedToTargets.WonProductivityDueToScrapInPercent);
        paperSackProductGroupStatistics.Performance.Scrap.Unit.Should().Be(qualityUnit);

        paperSackProductGroupStatistics.Performance.Total.ActualValue.Should().Be(standardProductGroupKpis.ThroughputRate);
        paperSackProductGroupStatistics.Performance.Total.TargetValue.Should().Be(standardProductGroupKpis.TargetThroughputRate);
        paperSackProductGroupStatistics.Performance.Total.TargetValueSource.Should().BeNull();
        paperSackProductGroupStatistics.Performance.Total.LostTimeInMin.Should().Be(standardProductGroupKpis.PerformanceComparedToTargets.LostTimeTotalInMin);
        paperSackProductGroupStatistics.Performance.Total.WonProductivity.Should().Be(standardProductGroupKpis.PerformanceComparedToTargets.WonProductivityTotalInPercent);
        paperSackProductGroupStatistics.Performance.Total.Unit.Should().Be(rateUnit);

        paperSackProductGroupStatistics.SpeedHistogram?.Count.Should().Be(4);
        paperSackProductGroupStatistics.SpeedHistogram?[0].SpeedLevel.Should().Be(histogramItems[0].SpeedLevel);
        paperSackProductGroupStatistics.SpeedHistogram?[0].CapacityUtilizationRate.Should().Be(histogramItems[0].CapacityUtilizationRate);
        paperSackProductGroupStatistics.SpeedHistogram?[0].DurationInMin.Should().Be(histogramItems[0].DurationInMin);
        paperSackProductGroupStatistics.SpeedHistogram?[1].SpeedLevel.Should().Be(histogramItems[1].SpeedLevel);
        paperSackProductGroupStatistics.SpeedHistogram?[1].CapacityUtilizationRate.Should().Be(histogramItems[1].CapacityUtilizationRate);
        paperSackProductGroupStatistics.SpeedHistogram?[1].DurationInMin.Should().Be(histogramItems[1].DurationInMin);
        paperSackProductGroupStatistics.SpeedHistogram?[2].SpeedLevel.Should().Be(histogramItems[2].SpeedLevel);
        paperSackProductGroupStatistics.SpeedHistogram?[2].CapacityUtilizationRate.Should().Be(histogramItems[2].CapacityUtilizationRate);
        paperSackProductGroupStatistics.SpeedHistogram?[2].DurationInMin.Should().Be(histogramItems[2].DurationInMin);
        paperSackProductGroupStatistics.SpeedHistogram?[3].SpeedLevel.Should().Be(histogramItems[3].SpeedLevel);
        paperSackProductGroupStatistics.SpeedHistogram?[3].CapacityUtilizationRate.Should().Be(histogramItems[3].CapacityUtilizationRate);
        paperSackProductGroupStatistics.SpeedHistogram?[3].DurationInMin.Should().Be(histogramItems[3].DurationInMin);

        var recommendedTargetSpeed = await paperSackProductGroupStatistics.RecommendedTargetSpeed?.Value(CancellationToken.None)!;
        recommendedTargetSpeed.Should().Be(standardProductGroupKpis.RecommendedTargetSpeed);
    }

    [Fact]
    public async Task GetPaperSackProductGroupStatisticsPerMachine_ProductGroupStandardKpiCacheDataLoaderReturnNullValues_ReturnsEmptyDictionary()
    {
        // Arrange
        var paperSackProductGroupId = _fixture.Create<string>();
        var from = _fixture.Create<DateTime>();
        const PaperSackMachineFamilyFilter machineFamilyFilter = PaperSackMachineFamilyFilter.Both;
        var machineIds = _fixture.CreateMany<string>(3).ToList();

        var tuberMachineIds = machineIds[..2];
        MockGetMachineIdsByFilter(machineIdFilter: null, MachineDepartment.PaperSack, MachineFamily.PaperSackTuber, tuberMachineIds)
            .Verifiable(Times.Once);

        var bottomerMachineIds = machineIds[2..];
        MockGetMachineIdsByFilter(machineIdFilter: null, MachineDepartment.PaperSack, MachineFamily.PaperSackBottomer, bottomerMachineIds)
            .Verifiable(Times.Once);

        var paperSackProductGroupKpisByMachineId = machineIds.ToDictionary(
            machineId => machineId, PaperSackProductGroupKpis? (_) => null);

        MockGetPaperSackProductGroupKpis(
                paperSackProductGroupId,
                machineIds,
                from,
                to: null,
                productIdFilter: null,
                new InternalItemResponse<Dictionary<string, PaperSackProductGroupKpis?>>(paperSackProductGroupKpisByMachineId))
            .Verifiable(Times.Once);

        // Act
        var response = await _subject.GetPaperSackProductGroupStatisticsPerMachine(
            _productGroupStandardKpiCacheDataLoader,
            _machineMetaDataBatchDataLoader,
            paperSackProductGroupId,
            from,
            to: null,
            productIdFilter: null,
            machineIdFilter: null,
            machineFamilyFilter,
            CancellationToken.None);

        // Assert
        response.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPaperSackProductGroupStatisticsPerMachine_GetMachineIdsByFilterReturnNoMachinesIds_ReturnsEmptyDirectory()
    {
        // Arrange
        var paperSackProductGroupId = _fixture.Create<string>();
        var from = _fixture.Create<DateTime>();
        const PaperSackMachineFamilyFilter machineFamilyFilter = PaperSackMachineFamilyFilter.Both;

        _machineServiceMock
            .Setup(mock => mock.GetMachineIdsByFilter(null, MachineDepartment.PaperSack, MachineFamily.PaperSackTuber, It.IsAny<CancellationToken>()))
            .ReturnsAsync([])
            .Verifiable(Times.Once);
        _machineServiceMock
            .Setup(mock => mock.GetMachineIdsByFilter(null, MachineDepartment.PaperSack, MachineFamily.PaperSackBottomer, It.IsAny<CancellationToken>()))
            .ReturnsAsync([])
            .Verifiable(Times.Once);

        // Act
        var response = await _subject.GetPaperSackProductGroupStatisticsPerMachine(
            _productGroupStandardKpiCacheDataLoader,
            _machineMetaDataBatchDataLoader,
            paperSackProductGroupId,
            from,
            to: null,
            productIdFilter: null,
            machineIdFilter: null,
            machineFamilyFilter,
            CancellationToken.None);

        // Assert
        response.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPaperSackProductGroupStatisticsPerMachine_GetPaperSackProductGroupStatisticsPerMachineReturnsException_RethrowsException()
    {
        // Arrange
        var paperSackProductGroupId = _fixture.Create<string>();
        var from = _fixture.Create<DateTime>();
        const PaperSackMachineFamilyFilter machineFamilyFilter = PaperSackMachineFamilyFilter.Both;
        var machineIds = _fixture.CreateMany<string>(3).ToList();

        var tuberMachineIds = machineIds[..2];
        MockGetMachineIdsByFilter(machineIdFilter: null, MachineDepartment.PaperSack, MachineFamily.PaperSackTuber, tuberMachineIds)
            .Verifiable(Times.Once);

        var bottomerMachineIds = machineIds[2..];
        MockGetMachineIdsByFilter(machineIdFilter: null, MachineDepartment.PaperSack, MachineFamily.PaperSackBottomer, bottomerMachineIds)
            .Verifiable(Times.Once);

        MockGetPaperSackProductGroupKpis(
                paperSackProductGroupId,
                machineIds,
                from,
                to: null,
                productIdFilter: null,
                new InternalItemResponse<Dictionary<string, PaperSackProductGroupKpis?>>(StatusCodes.Status500InternalServerError, "Error"))
            .Verifiable(Times.Once);

        // Act
        var act = () => _subject.GetPaperSackProductGroupStatisticsPerMachine(
            _productGroupStandardKpiCacheDataLoader,
            _machineMetaDataBatchDataLoader,
            paperSackProductGroupId,
            from,
            to: null,
            productIdFilter: null,
            machineIdFilter: null,
            machineFamilyFilter,
            CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InternalServiceException>();
    }

    [Fact]
    public async Task GetPaperSackProductGroupStatisticsPerMachine_GetJobInfosByIdsReturnsResponseWithError_ThrowsInternalServiceException()
    {
        // Arrange
        var paperSackProductGroupId = _fixture.Create<string>();
        var from = _fixture.Create<DateTime>();
        const PaperSackMachineFamilyFilter machineFamilyFilter = PaperSackMachineFamilyFilter.Both;
        var machineIds = _fixture.CreateMany<string>(3).ToList();

        var tuberMachineIds = machineIds[..2];
        MockGetMachineIdsByFilter(machineIdFilter: null, MachineDepartment.PaperSack, MachineFamily.PaperSackTuber, tuberMachineIds)
            .Verifiable(Times.Once);

        var bottomerMachineIds = machineIds[2..];
        MockGetMachineIdsByFilter(machineIdFilter: null, MachineDepartment.PaperSack, MachineFamily.PaperSackBottomer, bottomerMachineIds)
            .Verifiable(Times.Once);

        var paperSackProductGroupKpisByMachineId = machineIds.ToDictionary(
            machineId => machineId, PaperSackProductGroupKpis? (_) => _fixture.Create<PaperSackProductGroupKpis>());

        MockGetPaperSackProductGroupKpis(
                paperSackProductGroupId,
                machineIds,
                from,
                to: null,
                productIdFilter: null,
                new InternalItemResponse<Dictionary<string, PaperSackProductGroupKpis?>>(paperSackProductGroupKpisByMachineId))
            .Verifiable(Times.Once);

        var qualityUnit = _fixture.Create<string>();
        var rateUnit = _fixture.Create<string>();
        var timeUnit = _fixture.Create<string>();
        MockGetProducedPerformanceUnits(machineIds, qualityUnit, rateUnit, timeUnit);

        MockGetMachineFamily(tuberMachineIds, MachineFamily.PaperSackTuber);
        MockGetMachineFamily(bottomerMachineIds, MachineFamily.PaperSackBottomer);

        _productionPeriodsDataHandlerHttpClientMock
            .Setup(m => m.GetJobInfosByIds(It.IsAny<CancellationToken>(), It.IsAny<string>(), It.IsAny<List<string>>()))
            .ReturnsAsync(new InternalListResponse<JobInfo>(StatusCodes.Status500InternalServerError, "Error"));

        // Act
        var act = () => _subject.GetPaperSackProductGroupStatisticsPerMachine(
            _productGroupStandardKpiCacheDataLoader,
            _machineMetaDataBatchDataLoader,
            paperSackProductGroupId,
            from,
            to: null,
            productIdFilter: null,
            machineIdFilter: null,
            machineFamilyFilter,
            CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InternalServiceException>();
    }

    [Theory]
    [InlineData("note")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task UpdatePaperSackProductGroupNote_SendSetOverallNoteOfProductGroupEventAndWaitForReplyReturnsSuccess_ReturnsResultOfGetPaperSackProductGroupById(string? note)
    {
        // Arrange
        var paperSackProductGroup = _fixture.Create<PaperSackProductGroup>();
        var userId = _fixture.Create<string>();

        _kpiEventQueueWrapperMock
            .Setup(m => m.SendSetOverallNoteOfProductGroupEventAndWaitForReply(It.Is<SetOverallNoteOfProductGroupEventMessage>(message =>
                message.PaperSackProductGroupId == paperSackProductGroup.Id
                && message.Note == note
                && message.UserId == userId)))
            .ReturnsAsync(new InternalResponse())
            .Verifiable(Times.Once);

        _kpiDataHandlerMock
            .Setup(m => m.GetPaperSackProductGroupById(It.IsAny<CancellationToken>(), paperSackProductGroup.Id))
            .ReturnsAsync(new InternalItemResponse<PaperSackProductGroup>(paperSackProductGroup))
            .Verifiable(Times.Once);

        // Act
        var response = await _subject.UpdatePaperSackProductGroupNote(paperSackProductGroup.Id, note, userId, CancellationToken.None);

        // Assert
        response.Id.Should().BeEquivalentTo(paperSackProductGroup.Id);
        response.OverallNote.Should().BeEquivalentTo(paperSackProductGroup.Note);
    }

    [Theory]
    [InlineData(StatusCodes.Status400BadRequest)]
    [InlineData(StatusCodes.Status204NoContent)]
    public async Task UpdatePaperSackProductGroupNote_SendSetOverallNoteOfProductGroupEventAndWaitForReplyReturnsParameterError_ThrowsParameterInvalidException(int statusCode)
    {
        // Arrange
        var paperSackProductGroup = _fixture.Create<PaperSackProductGroup>();
        var userId = _fixture.Create<string>();
        var note = _fixture.Create<string>();

        _kpiEventQueueWrapperMock
            .Setup(m => m.SendSetOverallNoteOfProductGroupEventAndWaitForReply(It.IsAny<SetOverallNoteOfProductGroupEventMessage>()))
            .ReturnsAsync(new InternalResponse(new InternalError(statusCode, "Error")))
            .Verifiable(Times.Once);

        _kpiDataHandlerMock
            .Setup(m => m.GetPaperSackProductGroupById(It.IsAny<CancellationToken>(), paperSackProductGroup.Id))
            .ReturnsAsync(new InternalItemResponse<PaperSackProductGroup>(paperSackProductGroup))
            .Verifiable(Times.Once);

        // Act
        var act = () => _subject.UpdatePaperSackProductGroupNote(paperSackProductGroup.Id, note, userId, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ParameterInvalidException>();
    }

    [Fact]
    public async Task UpdatePaperSackProductGroupNote_SendSetOverallNoteOfProductGroupEventAndWaitForReplyReturnsError_ThrowsPInternalServiceException()
    {
        // Arrange
        var paperSackProductGroup = _fixture.Create<PaperSackProductGroup>();
        var userId = _fixture.Create<string>();
        var note = _fixture.Create<string>();

        _kpiEventQueueWrapperMock
            .Setup(m => m.SendSetOverallNoteOfProductGroupEventAndWaitForReply(It.IsAny<SetOverallNoteOfProductGroupEventMessage>()))
            .ReturnsAsync(new InternalResponse(new InternalError(StatusCodes.Status500InternalServerError, "Error")))
            .Verifiable(Times.Once);

        _kpiDataHandlerMock
            .Setup(m => m.GetPaperSackProductGroupById(It.IsAny<CancellationToken>(), paperSackProductGroup.Id))
            .ReturnsAsync(new InternalItemResponse<PaperSackProductGroup>(paperSackProductGroup))
            .Verifiable(Times.Once);

        // Act
        var act = () => _subject.UpdatePaperSackProductGroupNote(paperSackProductGroup.Id, note, userId, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InternalServiceException>();
    }

    [Theory]
    [InlineData("note")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task UpdatePaperSackProductGroupMachineNote_SendSetMachineNoteOfProductGroupEventAndWaitForReplyReturnsSuccess_ReturnsResultOfGetPaperSackProductGroupById(string? note)
    {
        // Arrange
        var machineId = _fixture.Create<string>();
        var paperSackProductGroup = _fixture.Create<PaperSackProductGroup>();
        var userId = _fixture.Create<string>();

        _kpiEventQueueWrapperMock
            .Setup(m => m.SendSetMachineNoteOfProductGroupEventAndWaitForReply(It.Is<SetMachineNoteOfProductGroupEventMessage>(message =>
                message.PaperSackProductGroupId == paperSackProductGroup.Id
                && message.Note == note
                && message.MachineId == machineId
                && message.UserId == userId)))
            .ReturnsAsync(new InternalResponse())
            .Verifiable(Times.Once);

        _kpiDataHandlerMock
            .Setup(m => m.GetPaperSackProductGroupById(It.IsAny<CancellationToken>(), paperSackProductGroup.Id))
            .ReturnsAsync(new InternalItemResponse<PaperSackProductGroup>(paperSackProductGroup))
            .Verifiable(Times.Once);

        // Act
        var response = await _subject.UpdatePaperSackProductGroupMachineNote(paperSackProductGroup.Id, machineId, note, userId, CancellationToken.None);

        // Assert
        response.Id.Should().BeEquivalentTo(paperSackProductGroup.Id);
    }

    [Theory]
    [InlineData(StatusCodes.Status400BadRequest)]
    [InlineData(StatusCodes.Status204NoContent)]
    public async Task UpdatePaperSackProductGroupMachineNote_SendSetMachineNoteOfProductGroupEventAndWaitForReplyReturnsParameterError_ThrowsParameterInvalidException(int statusCode)
    {
        // Arrange
        var machineId = _fixture.Create<string>();
        var paperSackProductGroup = _fixture.Create<PaperSackProductGroup>();
        var userId = _fixture.Create<string>();
        var note = _fixture.Create<string>();

        _kpiEventQueueWrapperMock
            .Setup(m => m.SendSetMachineNoteOfProductGroupEventAndWaitForReply(It.IsAny<SetMachineNoteOfProductGroupEventMessage>()))
            .ReturnsAsync(new InternalResponse(new InternalError(statusCode, "Error")))
            .Verifiable(Times.Once);

        _kpiDataHandlerMock
            .Setup(m => m.GetPaperSackProductGroupById(It.IsAny<CancellationToken>(), paperSackProductGroup.Id))
            .ReturnsAsync(new InternalItemResponse<PaperSackProductGroup>(paperSackProductGroup))
            .Verifiable(Times.Once);

        // Act
        var act = () => _subject.UpdatePaperSackProductGroupMachineNote(paperSackProductGroup.Id, machineId, note, userId, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ParameterInvalidException>();
    }

    [Fact]
    public async Task UpdatePaperSackProductGroupMachineNote_SendSetMachineNoteOfProductGroupEventAndWaitForReplyReturnsError_ThrowsPInternalServiceException()
    {
        // Arrange
        var machineId = _fixture.Create<string>();
        var paperSackProductGroup = _fixture.Create<PaperSackProductGroup>();
        var userId = _fixture.Create<string>();
        var note = _fixture.Create<string>();

        _kpiEventQueueWrapperMock
            .Setup(m => m.SendSetMachineNoteOfProductGroupEventAndWaitForReply(It.IsAny<SetMachineNoteOfProductGroupEventMessage>()))
            .ReturnsAsync(new InternalResponse(new InternalError(StatusCodes.Status500InternalServerError, "Error")))
            .Verifiable(Times.Once);

        _kpiDataHandlerMock
            .Setup(m => m.GetPaperSackProductGroupById(It.IsAny<CancellationToken>(), paperSackProductGroup.Id))
            .ReturnsAsync(new InternalItemResponse<PaperSackProductGroup>(paperSackProductGroup))
            .Verifiable(Times.Once);

        // Act
        var act = () => _subject.UpdatePaperSackProductGroupMachineNote(paperSackProductGroup.Id, machineId, note, userId, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InternalServiceException>();
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(12.0)]
    [InlineData(null)]
    public async Task UpdatePaperSackProductGroupMachineTargetSpeed_SendSetMachineTargetSpeedOfProductGroupEventAndWaitForReplyReturnsSuccess_ReturnsResultOfGetPaperSackProductGroupById(double? targetSpeed)
    {
        // Arrange
        var machineId = _fixture.Create<string>();
        var paperSackProductGroup = _fixture.Create<PaperSackProductGroup>();
        var userId = _fixture.Create<string>();

        _kpiEventQueueWrapperMock
            .Setup(m => m.SendSetMachineTargetSpeedOfProductGroupEventAndWaitForReply(It.Is<SetMachineTargetSpeedOfProductGroupEventMessage>(message =>
                message.PaperSackProductGroupId == paperSackProductGroup.Id
                && message.TargetSpeed.Equals(targetSpeed)
                && message.MachineId == machineId
                && message.UserId == userId)))
            .ReturnsAsync(new InternalResponse())
            .Verifiable(Times.Once);

        _kpiDataHandlerMock
            .Setup(m => m.GetPaperSackProductGroupById(It.IsAny<CancellationToken>(), paperSackProductGroup.Id))
            .ReturnsAsync(new InternalItemResponse<PaperSackProductGroup>(paperSackProductGroup))
            .Verifiable(Times.Once);

        // Act
        var response = await _subject.UpdatePaperSackProductGroupMachineTargetSpeed(paperSackProductGroup.Id, machineId, targetSpeed, userId, CancellationToken.None);

        // Assert
        response.Id.Should().BeEquivalentTo(paperSackProductGroup.Id);
    }

    [Theory]
    [InlineData(StatusCodes.Status400BadRequest)]
    [InlineData(StatusCodes.Status204NoContent)]
    public async Task UpdatePaperSackProductGroupMachineTargetSpeed_SendSetMachineTargetSpeedOfProductGroupEventAndWaitForReplyReturnsParameterError_ThrowsParameterInvalidException(int statusCode)
    {
        // Arrange
        var machineId = _fixture.Create<string>();
        var paperSackProductGroup = _fixture.Create<PaperSackProductGroup>();
        var userId = _fixture.Create<string>();
        var targetSpeed = _fixture.Create<double>();

        _kpiEventQueueWrapperMock
            .Setup(m => m.SendSetMachineTargetSpeedOfProductGroupEventAndWaitForReply(It.IsAny<SetMachineTargetSpeedOfProductGroupEventMessage>()))
            .ReturnsAsync(new InternalResponse(new InternalError(statusCode, "Error")))
            .Verifiable(Times.Once);

        _kpiDataHandlerMock
            .Setup(m => m.GetPaperSackProductGroupById(It.IsAny<CancellationToken>(), paperSackProductGroup.Id))
            .ReturnsAsync(new InternalItemResponse<PaperSackProductGroup>(paperSackProductGroup))
            .Verifiable(Times.Once);

        // Act
        var act = () => _subject.UpdatePaperSackProductGroupMachineTargetSpeed(paperSackProductGroup.Id, machineId, targetSpeed, userId, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ParameterInvalidException>();
    }

    [Fact]
    public async Task UpdatePaperSackProductGroupMachineTargetSpeed_SendSetMachineTargetSpeedOfProductGroupEventAndWaitForReplyReturnsError_ThrowsPInternalServiceException()
    {
        // Arrange
        var machineId = _fixture.Create<string>();
        var paperSackProductGroup = _fixture.Create<PaperSackProductGroup>();
        var userId = _fixture.Create<string>();
        var targetSpeed = _fixture.Create<double>();

        _kpiEventQueueWrapperMock
            .Setup(m => m.SendSetMachineTargetSpeedOfProductGroupEventAndWaitForReply(It.IsAny<SetMachineTargetSpeedOfProductGroupEventMessage>()))
            .ReturnsAsync(new InternalResponse(new InternalError(StatusCodes.Status500InternalServerError, "Error")))
            .Verifiable(Times.Once);

        _kpiDataHandlerMock
            .Setup(m => m.GetPaperSackProductGroupById(It.IsAny<CancellationToken>(), paperSackProductGroup.Id))
            .ReturnsAsync(new InternalItemResponse<PaperSackProductGroup>(paperSackProductGroup))
            .Verifiable(Times.Once);

        // Act
        var act = () => _subject.UpdatePaperSackProductGroupMachineTargetSpeed(paperSackProductGroup.Id, machineId, targetSpeed, userId, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InternalServiceException>();
    }

    [Fact]
    public void MapTargetSpeedPerMachineToSchema_WithoutTargetSpeedValues_ReturnsEmptyDictionary()
    {
        // Arrange
        var targetSpeedPerMachine = new Dictionary<string, double>();

        // Act
        var mappedTargetSpeedPerMachine = _subject.MapTargetSpeedPerMachineToSchema(
            _machineMetaDataBatchDataLoader, _kpiServiceMock.Object, targetSpeedPerMachine);

        // Assert
        mappedTargetSpeedPerMachine.Should().BeEmpty();
    }

    [Fact]
    public async Task MapTargetSpeedPerMachineToSchema_WithTargetSpeedValues_ReturnsDictionaryWithNumericValue()
    {
        // Arrange
        var targetSpeedPerMachine = _fixture.Create<Dictionary<string, double>>();
        var units = targetSpeedPerMachine.ToDictionary(x => x.Key, _ => _fixture.Create<string>());

        foreach (var (machineId, _) in targetSpeedPerMachine)
        {
            _kpiServiceMock
                .Setup(m => m.GetUnit(_machineMetaDataBatchDataLoader, KpiAttribute.TargetSpeed, machineId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(units[machineId])
                .Verifiable(Times.Once);
        }

        // Act
        var mappedTargetSpeedPerMachine = _subject.MapTargetSpeedPerMachineToSchema(
            _machineMetaDataBatchDataLoader, _kpiServiceMock.Object, targetSpeedPerMachine);

        // Assert
        mappedTargetSpeedPerMachine.Should().HaveCount(targetSpeedPerMachine.Count);

        // Check if the func-parameter are used for the unit
        _kpiServiceMock.Verify(m => m.GetUnit(_machineMetaDataBatchDataLoader, KpiAttribute.TargetSpeed, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);

        foreach (var (machineId, expectedValue) in targetSpeedPerMachine)
        {
            var value = await mappedTargetSpeedPerMachine[machineId].Value(CancellationToken.None);
            var unit = await mappedTargetSpeedPerMachine[machineId].Unit(CancellationToken.None);

            value.Should().Be(expectedValue);
            unit.Should().Be(units[machineId]);
        }
    }

    private IReturnsResult<IKpiDataHandlerClient> MockGetPaperSackProductGroupById(InternalItemResponse<PaperSackProductGroup>? response = null)
    {
        return _kpiDataHandlerMock
            .Setup(mock => mock.GetPaperSackProductGroupById(
                It.IsAny<CancellationToken>(),
                It.IsAny<string>()
            ))
            .ReturnsAsync(response ?? new InternalItemResponse<PaperSackProductGroup>(_paperSackProductGroup));
    }

    private IReturnsResult<IKpiDataHandlerClient> MockGetPaperSackProductGroupByJobId(string machineId, string jobId, InternalItemResponse<PaperSackProductGroup>? response = null)
    {
        return _kpiDataHandlerMock
            .Setup(mock => mock.GetPaperSackProductGroupByJobId(
                It.IsAny<CancellationToken>(),
                machineId,
                jobId
            ))
            .ReturnsAsync(response ?? new InternalItemResponse<PaperSackProductGroup>(_paperSackProductGroup));
    }

    private IReturnsResult<IKpiDataHandlerClient> MockGetPaperSackProductGroups(InternalListResponse<PaperSackProductGroup>? response = null)
    {
        return _kpiDataHandlerMock
            .Setup(mock => mock.GetPaperSackProductGroups(
                It.IsAny<CancellationToken>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<ProductGroupSortOption>()
            ))
            .ReturnsAsync(response ?? new InternalListResponse<PaperSackProductGroup>([_paperSackProductGroup]));
    }

    private IReturnsResult<IKpiDataHandlerClient> MockGetPaperSackProductGroupsCount(InternalItemResponse<int>? response = null)
    {
        return _kpiDataHandlerMock
            .Setup(mock => mock.GetPaperSackProductGroupsCount(
                It.IsAny<CancellationToken>(),
                It.IsAny<string>()
            ))
            .ReturnsAsync(response ?? new InternalItemResponse<int>(5));
    }

    private IReturnsResult<IMachineService> MockGetMachineIdsByFilter(
        string? machineIdFilter,
        MachineDepartment machineDepartment,
        MachineFamily machineFamilyFilter,
        List<string> machineIds)
    {
        return _machineServiceMock
            .Setup(mock => mock.GetMachineIdsByFilter(machineIdFilter, machineDepartment, machineFamilyFilter, It.IsAny<CancellationToken>()))
            .ReturnsAsync(machineIds);
    }

    private IReturnsResult<IKpiDataHandlerClient> MockGetPaperSackProductGroupKpis(
        string paperSackProductGroupId,
        List<string> machineIds,
        DateTime from,
        DateTime? to,
        string? productIdFilter,
        InternalItemResponse<Dictionary<string, PaperSackProductGroupKpis?>> response)
    {
        return _kpiDataHandlerMock
            .Setup(mock => mock.GetPaperSackProductGroupKpis(
                It.IsAny<CancellationToken>(),
                paperSackProductGroupId,
                from,
                to,
                machineIds,
                productIdFilter))
            .ReturnsAsync(response);
    }

    private void MockGetMachineFamily(List<string> machineIds, MachineFamily machineFamily)
    {
        foreach (var machineId in machineIds)
        {
            _machineServiceMock
                .Setup(m => m.GetMachineFamily(machineId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(machineFamily)
                .Verifiable(Times.Once);
        }
    }

    private void MockGetProducedPerformanceUnits(List<string> machineIds, string qualityUnit, string rateUnit, string timeUnit)
    {
        foreach (var machineId in machineIds)
        {
            _kpiServiceMock
                .Setup(m => m.GetProducedPerformanceUnits(_machineMetaDataBatchDataLoader, machineId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((qualityUnit, rateUnit, timeUnit))
                .Verifiable(Times.Once);
        }
    }

    private IReturnsResult<IProductionPeriodsDataHandlerHttpClient> MockGetJobInfosByIds(string machineId, List<string> jobIds)
    {
        var jobInfos = jobIds.Select(jobId => _fixture.Build<JobInfo>()
            .With(m => m.JobId, jobId)
            .Create())
            .ToList();

        return _productionPeriodsDataHandlerHttpClientMock
            .Setup(m => m.GetJobInfosByIds(It.IsAny<CancellationToken>(), machineId, jobIds))
            .ReturnsAsync(new InternalListResponse<JobInfo>(jobInfos));
    }
}