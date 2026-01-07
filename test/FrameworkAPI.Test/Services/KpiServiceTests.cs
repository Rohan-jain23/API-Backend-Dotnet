using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FrameworkAPI.DataLoaders;
using FrameworkAPI.Models.Enums;
using FrameworkAPI.Schema.Misc;
using FrameworkAPI.Services;
using FrameworkAPI.Services.Interfaces;
using FrameworkAPI.Test.Services.Helpers;
using Moq;
using WuH.Ruby.Common.Core;
using WuH.Ruby.KpiDataHandler.Client;
using WuH.Ruby.MetaDataHandler.Client;
using Xunit;
using TimeRange = WuH.Ruby.Common.Core.TimeRange;

namespace FrameworkAPI.Test.Services;

public class KpiServiceTests : IDisposable
{
    private const string MachineId = "EQ00001";
    private const string JobId = "FakeJobId";

    private readonly Mock<IKpiDataCachingService> _kpiDataCachingServiceMock = new(MockBehavior.Strict);
    private readonly Mock<IMetaDataHandlerHttpClient> _metaDataHandlerHttpClientMock = new(MockBehavior.Strict);
    private readonly Mock<IUnitService> _unitServiceMock = new(MockBehavior.Strict);
    private readonly Mock<MachineMetaDataService> _machineMetaDataServiceMock = new(MockBehavior.Strict);
    private readonly MachineMetaDataBatchDataLoader _machineMetaDataBatchDataLoader;
    private readonly JobStandardKpiCacheDataLoader _jobStandardKpiCacheDataLoader;
    private readonly Mock<IKpiDataHandlerClient> _kpiDataHandlerClientMock = new(MockBehavior.Strict);
    private readonly KpiService _subject;

    public KpiServiceTests()
    {
        var delayedBatchScheduler = new DelayedBatchScheduler();
        _machineMetaDataBatchDataLoader =
            new MachineMetaDataBatchDataLoader(_metaDataHandlerHttpClientMock.Object, delayedBatchScheduler);
        _jobStandardKpiCacheDataLoader =
            new JobStandardKpiCacheDataLoader(_kpiDataCachingServiceMock.Object);
        _subject = new KpiService(_unitServiceMock.Object,
            _machineMetaDataServiceMock.Object,
            _kpiDataHandlerClientMock.Object);
    }

    public void Dispose()
    {
        _kpiDataCachingServiceMock.VerifyAll();
        _kpiDataCachingServiceMock.VerifyNoOtherCalls();

        _metaDataHandlerHttpClientMock.VerifyAll();
        _metaDataHandlerHttpClientMock.VerifyNoOtherCalls();

        _unitServiceMock.VerifyAll();
        _unitServiceMock.VerifyNoOtherCalls();

        _kpiDataHandlerClientMock.VerifyAll();
        _kpiDataHandlerClientMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(KpiAttribute.GoodProductionCount, true)]
    [InlineData(KpiAttribute.GoodProductionCount, false)]
    [InlineData(KpiAttribute.ScrapProductionCount, true)]
    [InlineData(KpiAttribute.ScrapProductionCount, false)]
    public async Task GetValue_Returns_The_Correct_Value(KpiAttribute kpiAttribute, bool standardKpisAsParameter)
    {
        // Arrange
        double expectedValue;
        StandardJobKpis standardKpis;

        if (kpiAttribute == KpiAttribute.GoodProductionCount)
        {
            expectedValue = 1.23;
            standardKpis = new StandardJobKpis
            {
                ProductionData = new KpiProductionTimesAndOutput
                {
                    GoodProductionCount = expectedValue
                }
            };
        }
        else
        {
            expectedValue = 4.56;
            standardKpis = new StandardJobKpis
            {
                ProductionData = new KpiProductionTimesAndOutput
                {
                    ScrapProductionCount = expectedValue
                }
            };
        }

        if (!standardKpisAsParameter)
        {
            _kpiDataCachingServiceMock
                .Setup(m => m.GetStandardKpis(MachineId, JobId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new InternalItemResponse<StandardJobKpis>(standardKpis));
        }

        const string variableIdentifier = VariableIdentifier.JobQuantityActual;
        var processVariableMetaData = new ProcessVariableMetaData { VariableIdentifier = variableIdentifier };

        _metaDataHandlerHttpClientMock
            .Setup(m => m.GetProcessVariableMetaDataByIdentifiers(
                It.IsAny<CancellationToken>(), MachineId, new List<string> { variableIdentifier }))
            .ReturnsAsync(new InternalListResponse<ProcessVariableMetaDataResponseItem>(
                [new ProcessVariableMetaDataResponseItem { Data = processVariableMetaData, Path = variableIdentifier }]));

        _unitServiceMock
            .Setup(s => s.CalculateSiValue(expectedValue, processVariableMetaData))
            .Returns(expectedValue);

        // Act
        var value = await _subject.GetValue(
            _jobStandardKpiCacheDataLoader, _machineMetaDataBatchDataLoader, kpiAttribute, MachineId, JobId, standardJobKpis: standardKpisAsParameter ? standardKpis : null);

        // Assert
        value.Should().Be(expectedValue);
    }

    [Theory]
    [InlineData(KpiAttribute.TargetJobTimeInMin, true)]
    [InlineData(KpiAttribute.TargetJobTimeInMin, false)]
    public async Task GetRawDouble_Returns_The_Correct_Value(KpiAttribute kpiAttribute, bool standardKpisAsParameter)
    {
        // Arrange
        const double expectedValue = 1.23;
        var standardKpis = new StandardJobKpis
        {
            TargetJobTimeInMin = expectedValue
        };

        if (!standardKpisAsParameter)
        {
            _kpiDataCachingServiceMock
                .Setup(m => m.GetStandardKpis(MachineId, JobId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new InternalItemResponse<StandardJobKpis>(standardKpis));
        }

        // Act
        var value = await _subject.GetRawDouble(
            _jobStandardKpiCacheDataLoader, kpiAttribute, MachineId, JobId, standardJobKpis: standardKpisAsParameter ? standardKpis : null);

        // Assert
        value.Should().Be(expectedValue);
    }

    [Theory]
    [InlineData(KpiAttribute.IsApparentlyWrongGoodProductionCount, true)]
    [InlineData(KpiAttribute.IsApparentlyWrongGoodProductionCount, false)]
    public async Task GetBool_Returns_The_Correct_Value(KpiAttribute kpiAttribute, bool standardKpisAsParameter)
    {
        // Arrange
        const bool expectedValue = false;
        var standardKpis = new StandardJobKpis
        {
            ProductionData = new KpiProductionTimesAndOutput
            {
                IsApparentlyWrongGoodProductionCount = expectedValue
            }
        };

        if (!standardKpisAsParameter)
        {
            _kpiDataCachingServiceMock
                .Setup(m => m.GetStandardKpis(MachineId, JobId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new InternalItemResponse<StandardJobKpis>(standardKpis));
        }

        // Act
        var value = await _subject.GetBool(
            _jobStandardKpiCacheDataLoader, kpiAttribute, MachineId, JobId, standardJobKpis: standardKpisAsParameter ? standardKpis : null);

        // Assert
        value.Should().Be(expectedValue);
    }

    [Theory]
    [InlineData(KpiAttribute.GoodProductionCount)]
    [InlineData(KpiAttribute.ScrapProductionCount)]
    public async Task GetUnit_Returns_The_Correct_Unit(KpiAttribute kpiAttribute)
    {
        // Arrange
        const string variableIdentifier = VariableIdentifier.JobQuantityActual;

        const string expectedSiUnit = "kg";
        var processVariableMetaData = new ProcessVariableMetaData
        {
            VariableIdentifier = variableIdentifier,
            Units = new VariableUnits
            {
                Si = new VariableUnits.UnitWithCoefficient
                {
                    Unit = expectedSiUnit
                }
            }
        };
        var processVariableMetaDataResponseItem = new ProcessVariableMetaDataResponseItem
        {
            Path = variableIdentifier,
            Data = processVariableMetaData
        };

        _metaDataHandlerHttpClientMock
            .Setup(m => m.GetProcessVariableMetaDataByIdentifiers(
                It.IsAny<CancellationToken>(), MachineId, new List<string> { variableIdentifier }))
            .ReturnsAsync(new InternalListResponse<ProcessVariableMetaDataResponseItem>(
                [processVariableMetaDataResponseItem]));

        _unitServiceMock
            .Setup(s => s.GetSiUnit(processVariableMetaData))
            .Returns(expectedSiUnit);

        // Act
        var unit =
            await _subject.GetUnit(_machineMetaDataBatchDataLoader, kpiAttribute, MachineId, CancellationToken.None);

        // Assert
        unit.Should().Be(expectedSiUnit);
    }

    [Fact]
    public async Task GetUnit_Throws_An_ArgumentException_If_KpiAttribute_Cannot_Be_Mapped()
    {
        // Arrange
        var getUnitAction = async () => await _subject.GetUnit(_machineMetaDataBatchDataLoader,
            KpiAttribute.OEE, MachineId, CancellationToken.None);

        // Act / Assert
        await getUnitAction.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetUniqueIdOfRelatedProducedJobFromOtherMachine_Throws_Exception_If_CacheDataLoader_Returned_Error()
    {
        // Arrange
        _kpiDataCachingServiceMock
            .Setup(mock => mock.GetStandardKpis(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<StandardJobKpis>(400, "Test Error"));

        // Act
        var result = async () => await _subject.GetUniqueIdOfRelatedProducedJobFromOtherMachine(_jobStandardKpiCacheDataLoader, MachineId, JobId);

        // Assert
        await result.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task GetUniqueIdOfRelatedProducedJobFromOtherMachine_Returns_Null_If_StandardJobKpis_Is_Null()
    {
        // Arrange
        _kpiDataCachingServiceMock
            .Setup(mock => mock.GetStandardKpis(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<StandardJobKpis>(new StandardJobKpis()
            {
                RelatedJobMachineId = null,
                RelatedJobId = null
            }));

        // Act
        var result = await _subject.GetUniqueIdOfRelatedProducedJobFromOtherMachine(_jobStandardKpiCacheDataLoader, MachineId, JobId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetUniqueIdOfRelatedProducedJobFromOtherMachine_Returns_Correct_UniqueId()
    {
        // Arrange
        _kpiDataCachingServiceMock
            .Setup(mock => mock.GetStandardKpis(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<StandardJobKpis>(new StandardJobKpis()
            {
                RelatedJobMachineId = "EQ11111",
                RelatedJobId = "RelatedJobId"
            }));

        // Act
        var result = await _subject.GetUniqueIdOfRelatedProducedJobFromOtherMachine(_jobStandardKpiCacheDataLoader, MachineId, JobId);

        // Assert
        result.Should().Be("EQ11111_RelatedJobId");
    }

    [Fact]
    public async Task GetProductionApproval_With_TameRanges_Is_Null_Returns_Null()
    {
        // Arrange

        // Act
        var result = await _subject.GetProductionApproval(
            MachineId,
            null,
            CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetProductionApproval_With_TameRanges_Is_Empty_Returns_Null()
    {
        // Arrange

        // Act
        var result = await _subject.GetProductionApproval(
            MachineId,
            [],
            CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetProductionApproval_With_NoContent_Returns_Null()
    {
        // Arrange
        List<TimeRange> timeRanges =
        [
            new TimeRange(DateTime.UnixEpoch, DateTime.UnixEpoch.AddHours(1))
        ];

        _kpiDataHandlerClientMock
            .Setup(m => m.GetProductionApprovalByTimespan(
                It.IsAny<CancellationToken>(),
                MachineId,
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>()))
            .ReturnsAsync(new InternalItemResponse<ProductionApprovalEventResponseItem>(
                (int)HttpStatusCode.NoContent,
                "No Content"))
            .Verifiable();

        // Act
        var result = await _subject.GetProductionApproval(
            MachineId,
            timeRanges,
            CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetProductionApproval_With_Error_Throws()
    {
        // Arrange
        List<TimeRange> timeRanges =
        [
            new TimeRange(DateTime.UnixEpoch, DateTime.UnixEpoch.AddHours(1))
        ];

        _kpiDataHandlerClientMock
            .Setup(m => m.GetProductionApprovalByTimespan(
                It.IsAny<CancellationToken>(),
                MachineId,
                DateTime.UnixEpoch,
                DateTime.UnixEpoch.AddHours(1)))
            .ReturnsAsync(new InternalItemResponse<ProductionApprovalEventResponseItem>(
                500,
                "Test Error"))
            .Verifiable();

        // Act
        var act = async () => await _subject.GetProductionApproval(
            MachineId,
            timeRanges,
            CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("* Test Error");
    }

    [Fact]
    public async Task GetProductionApproval_With_Successful_Result()
    {
        // Arrange
        List<TimeRange> timeRanges =
        [
            new TimeRange(DateTime.UnixEpoch, DateTime.UnixEpoch.AddHours(1))
        ];

        _kpiDataHandlerClientMock
            .Setup(m => m.GetProductionApprovalByTimespan(
                It.IsAny<CancellationToken>(),
                MachineId,
                DateTime.UnixEpoch,
                DateTime.UnixEpoch.AddHours(1)))
            .ReturnsAsync(new InternalItemResponse<ProductionApprovalEventResponseItem>(
                new ProductionApprovalEventResponseItem(
                    "signature",
                    "supervisorUserId",
                    MachineId,
                    "associatedJob",
                    DateTime.UnixEpoch.AddMinutes(6),
                    DateTime.UnixEpoch.AddMinutes(9),
                    "userId")))
            .Verifiable();

        // Act
        var result = await _subject.GetProductionApproval(
            MachineId,
            timeRanges,
            CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(new ProductionApprovalEvent(
            DateTime.UnixEpoch.AddMinutes(9),
            "signature"));
    }

    [Fact]
    public async Task GetProductionApproval_With_Multiple_TimeRanges_And_Successful_Result()
    {
        // Arrange
        List<TimeRange> timeRanges =
        [
            new TimeRange(DateTime.UnixEpoch, DateTime.UnixEpoch.AddHours(1)),
            new TimeRange(DateTime.UnixEpoch.AddHours(5), DateTime.UnixEpoch.AddHours(6))
        ];

        _kpiDataHandlerClientMock
            .Setup(m => m.GetProductionApprovalByTimespan(
                It.IsAny<CancellationToken>(),
                MachineId,
                DateTime.UnixEpoch,
                DateTime.UnixEpoch.AddHours(6)))
            .ReturnsAsync(new InternalItemResponse<ProductionApprovalEventResponseItem>(
                new ProductionApprovalEventResponseItem(
                    "signature",
                    "supervisorUserId",
                    MachineId,
                    "associatedJob",
                    DateTime.UnixEpoch.AddMinutes(6),
                    DateTime.UnixEpoch.AddMinutes(9),
                    "userId")))
            .Verifiable();

        // Act
        var result = await _subject.GetProductionApproval(
            MachineId,
            timeRanges,
            CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(new ProductionApprovalEvent(
            DateTime.UnixEpoch.AddMinutes(9),
            "signature"));
    }

    [Theory]
    [InlineData(60.0, 30000.0, 500.0, 45000.0, false, 30.0)]
    [InlineData(60.0, 30000.0, 500.0, 45000.0, true, 30.0)]
    [InlineData(0.0, 30000.0, 500.0, 45000.0, false, null)]
    [InlineData(0.0, 30000.0, 500.0, 45000.0, true, null)]
    [InlineData(60.0, 0.0, 500.0, 45000.0, false, null)]
    [InlineData(60.0, 0.0, 500.0, 45000.0, true, null)]
    [InlineData(60.0, 30000.0, null, 45000.0, false, null)]
    [InlineData(60.0, 30000.0, null, 45000.0, true, null)]
    [InlineData(60.0, 30000.0, 0.0, 45000.0, false, null)]
    [InlineData(60.0, 30000.0, 0.0, 45000.0, true, null)]
    [InlineData(60.0, 30000.0, 500.0, null, false, null)]
    [InlineData(60.0, 30000.0, 500.0, null, true, null)]
    [InlineData(60.0, 30000.0, 500.0, 0.0, false, null)]
    [InlineData(60.0, 30000.0, 500.0, 0.0, true, null)]
    [InlineData(60.0, 30000.0, 500.0, 20000.0, false, 0.0)]
    [InlineData(60.0, 30000.0, 500.0, 20000.0, true, 0.0)]
    public async Task GetJobsRemainingProductionTime_With_ProductionData_Returns_The_ExpectedResult(
        double productionTimeInMin,
        double goodProductionCount,
        double? averageProductionSpeed,
        double? jobSize,
        bool standardKpisAsParameter,
        double? expectedResult)
    {
        // Arrange
        var standardKpis = new StandardJobKpis
        {
            ProductionData = new KpiProductionTimesAndOutput
            {
                ProductionTimeInMin = productionTimeInMin,
                GoodProductionCount = goodProductionCount,
                AverageProductionSpeed = averageProductionSpeed,
            },
            JobSize = jobSize
        };

        if (!standardKpisAsParameter)
        {
            _kpiDataCachingServiceMock
                .Setup(m => m.GetStandardKpis(MachineId, JobId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new InternalItemResponse<StandardJobKpis>(standardKpis));
        }
        // Act
        var value = await _subject.GetJobsRemainingProductionTime(
            _jobStandardKpiCacheDataLoader, MachineId, JobId, standardKpisAsParameter ? standardKpis : null);

        // Assert
        value.Should().Be(expectedResult);
    }

    [Fact]
    public async Task GetJobsRemainingProductionTime_Without_ProductionData_Returns_Null()
    {
        // Arrange
        var standardKpis = new StandardJobKpis
        {
            ProductionData = null,
        };

        _kpiDataCachingServiceMock
            .Setup(m => m.GetStandardKpis(MachineId, JobId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<StandardJobKpis>(standardKpis));

        // Act
        var value = await _subject.GetJobsRemainingProductionTime(
            _jobStandardKpiCacheDataLoader, MachineId, JobId);

        // Assert
        value.Should().Be(null);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task GetOverallEquipmentEffectiveness_Returns_The_ExpectedResult(bool standardKpisAsParameter)
    {
        // Arrange
        var standardKpis = new StandardJobKpis
        {
            OEE = 122.23,
            Availability = 53454.3,
            Effectiveness = 23123.0,
            QualityRatio = 2312.2
        };

        if (!standardKpisAsParameter)
        {
            _kpiDataCachingServiceMock
                .Setup(m => m.GetStandardKpis(MachineId, JobId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new InternalItemResponse<StandardJobKpis>(standardKpis));
        }

        // Act
        var value = await _subject.GetOverallEquipmentEffectiveness(
            _jobStandardKpiCacheDataLoader, MachineId, JobId, standardKpisAsParameter ? standardKpis : null);

        // Assert
        value.Should().NotBeNull();
        value.OEE.Should().Be(standardKpis.OEE);
        value.Availability.Should().Be(standardKpis.Availability);
        value.Effectiveness.Should().Be(standardKpis.Effectiveness);
        value.Quality.Should().Be(standardKpis.QualityRatio);
    }

    [Theory]
    [InlineData("m")]
    [InlineData("kg")]
    [InlineData("unit.items")]
    public async Task GetJobPerformance_Returns_Correct_Units(string jobQuantityUnit)
    {
        // Arrange
        var standardKpis = new StandardJobKpis
        {
            ProductionData = new(),
        };

        _kpiDataCachingServiceMock
            .Setup(m => m.GetStandardKpis(MachineId, JobId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<StandardJobKpis>(standardKpis));

        const string variableIdentifier = VariableIdentifier.JobQuantityActual;
        var processVariableMetaData = new ProcessVariableMetaData { VariableIdentifier = variableIdentifier };

        var processVariableMetaDataResponseItem = new ProcessVariableMetaDataResponseItem
        {
            Path = variableIdentifier,
            Data = processVariableMetaData
        };

        _metaDataHandlerHttpClientMock
            .Setup(m => m.GetProcessVariableMetaDataByIdentifiers(
                It.IsAny<CancellationToken>(), MachineId, new List<string> { variableIdentifier }))
            .ReturnsAsync(new InternalListResponse<ProcessVariableMetaDataResponseItem>(
                [processVariableMetaDataResponseItem]));

        _unitServiceMock
            .Setup(s => s.GetSiUnit(processVariableMetaData))
            .Returns(jobQuantityUnit);

        // Act
        var value = await _subject.GetJobPerformance(
            _jobStandardKpiCacheDataLoader, _machineMetaDataBatchDataLoader, MachineId, JobId);

        // Assert
        var minutesUnit = "label.minutesShort";
        var expectedSpeedUnit = jobQuantityUnit switch
        {
            "m" => "m/{{ label.minutesShort }}",
            "kg" => "kg/{{ label.minutesShort }}",
            "unit.items" => "{{ unit.items }}/{{ label.minutesShort }}",
            _ => throw new ArgumentException("Unknown 'jobQuantityUnit'."),
        };

        value.Should().NotBeNull();
        value!.Speed.Unit.Should().Be(expectedSpeedUnit);
        value!.Setup.Unit.Should().Be(minutesUnit);
        value!.Downtime.Unit.Should().Be(minutesUnit);
        value!.Scrap.Unit.Should().Be(jobQuantityUnit);
        value!.Total.Unit.Should().Be(expectedSpeedUnit);
    }
}