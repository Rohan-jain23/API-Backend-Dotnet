using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.Schema.ProducedJob;
using FrameworkAPI.Services;
using FrameworkAPI.Services.Interfaces;
using FrameworkAPI.Test.TestHelpers;
using HotChocolate;
using HotChocolate.Execution;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Snapshooter.Xunit;
using WuH.Ruby.Common.Core;
using WuH.Ruby.KpiDataHandler.Client;
using WuH.Ruby.MachineDataHandler.Client;
using WuH.Ruby.MachineSnapShooter.Client;
using WuH.Ruby.MetaDataHandler.Client;
using WuH.Ruby.OpcUaForwarder.Client;
using WuH.Ruby.ProcessDataReader.Client;
using WuH.Ruby.ProductionPeriodsDataHandler.Client;
using Xunit;
using MachineFamily = FrameworkAPI.Schema.Misc.MachineFamily;
using VariableUnits = WuH.Ruby.MetaDataHandler.Client.VariableUnits;

namespace FrameworkAPI.Test.Queries.ProducedJobsQuery;

public class ExtrusionProducedJobsQueryIntegrationTests
{
    private const string MachineId1 = "EQ00001";
    private const string MachineId2 = "EQ00002";
    private const string JobId = "FakeJobId";

    private readonly Mock<IMetaDataHandlerHttpClient> _metaDataHandlerHttpClientMock = new();
    private readonly Mock<IKpiDataCachingService> _kpiDataCachingServiceMock = new();
    private readonly Mock<IProductionPeriodsDataHandlerHttpClient> _productionPeriodsDataHandlerHttpClientMock = new();
    private readonly Mock<IProcessDataReaderHttpClient> _processDataReaderHttpClientMock = new();
    private readonly Mock<IKpiDataHandlerClient> _kpiDataHandlerClient = new();
    private readonly Mock<IMachineCachingService> _machineCachingServiceMock = new();
    private readonly Mock<ILatestMachineSnapshotCachingService> _latestMachineSnapshotCachingServiceMock = new();
    private readonly Mock<IProductionPeriodChangesQueueWrapper> _productionPeriodChangesQueueWrapper = new();

    [Fact]
    public async Task Get_Jobs_With_MachineId_And_GoodWeight_But_Resolving_Value_And_Unit_Fails_Once()
    {
        // Arrange
        var executor = await InitializeExecutor();
        var job = new JobInfo
        {
            MachineId = MachineId1,
            ProductId = "FakeProductId",
            JobId = JobId,
            StartTime = DateTime.UnixEpoch,
            EndTime = DateTime.UnixEpoch.AddHours(1)
        };
        var otherJob = new JobInfo
        {
            MachineId = MachineId2,
            ProductId = "FakeProductId",
            JobId = JobId,
            StartTime = DateTime.UnixEpoch,
            EndTime = DateTime.UnixEpoch.AddHours(1)
        };
        var standardKpis = new StandardJobKpis
        {
            ProductionData = new KpiProductionTimesAndOutput
            {
                GoodProductionCount = 128.99
            }
        };

        const string variableIdentifier = VariableIdentifier.JobQuantityActual;

        var processVariableMetaData = new ProcessVariableMetaData
        {
            VariableIdentifier = variableIdentifier,
            Units = new VariableUnits
            {
                Si = new VariableUnits.UnitWithCoefficient
                {
                    Unit = "kg",
                    Multiplier = 1
                }
            }
        };

        _machineCachingServiceMock
            .Setup(m =>
                m.GetMachinesAsInternalListResponse(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalListResponse<Machine>(new List<Machine>
            {
                new()
                {
                    MachineId = MachineId1,
                    MachineFamily = MachineFamily.BlowFilm.ToString(),
                    BusinessUnit = BusinessUnit.Extrusion
                },
                new()
                {
                    MachineId = MachineId2,
                    MachineFamily = MachineFamily.BlowFilm.ToString(),
                    BusinessUnit = BusinessUnit.Extrusion
                }
            }));

        _productionPeriodsDataHandlerHttpClientMock
            .Setup(s => s.GetAllJobInfos(
                It.IsAny<CancellationToken>(),
                new List<string> { MachineId1, MachineId2 },
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                0,
                20,
                null,
                true))
            .ReturnsAsync(new InternalListResponse<JobInfo>(new List<JobInfo>
            {
                job,
                otherJob
            }));
        _productionPeriodsDataHandlerHttpClientMock
            .Setup(s => s.GetAllJobIds(
                It.IsAny<CancellationToken>(),
                new List<string> { MachineId1, MachineId2 },
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<string>()))
            .ReturnsAsync(
                new InternalListResponse<string>(Enumerable.Range(1, 100).Select(i => i.ToString()).ToList()));

        _kpiDataCachingServiceMock
            .Setup(s => s.GetStandardKpis(
                It.IsAny<string>(),
                JobId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<StandardJobKpis>(standardKpis));

        _metaDataHandlerHttpClientMock
            .Setup(s => s.GetProcessVariableMetaDataByIdentifiers(
                It.IsAny<CancellationToken>(), MachineId1, new List<string> { variableIdentifier }))
            .ReturnsAsync(new InternalListResponse<ProcessVariableMetaDataResponseItem>(
                new List<ProcessVariableMetaDataResponseItem>
                {
                    new()
                    {
                        Path = variableIdentifier,
                        Data = processVariableMetaData
                    }
                }));

        _metaDataHandlerHttpClientMock
            .Setup(s => s.GetProcessVariableMetaDataByIdentifiers(
                It.IsAny<CancellationToken>(), MachineId2, new List<string> { variableIdentifier }))
            .ReturnsAsync(new InternalListResponse<ProcessVariableMetaDataResponseItem>(
                (int)HttpStatusCode.InternalServerError, "Error"));

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                @"{
                    producedJobs {
                    items {
                        machineId
                        ... on ExtrusionProducedJob {
                        goodWeight {
                            value
                            unit
                        }
                        }
                    }
                    }
                }")
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        _machineCachingServiceMock.VerifyAll();
        _machineCachingServiceMock.VerifyNoOtherCalls();

        _productionPeriodsDataHandlerHttpClientMock.VerifyAll();
        _productionPeriodsDataHandlerHttpClientMock.VerifyNoOtherCalls();

        _kpiDataCachingServiceMock.VerifyAll();
        _kpiDataCachingServiceMock.VerifyNoOtherCalls();

        _metaDataHandlerHttpClientMock.VerifyAll();
        _metaDataHandlerHttpClientMock.VerifyNoOtherCalls();
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
            .AddSingleton(_metaDataHandlerHttpClientMock.Object)
            .AddSingleton(_kpiDataCachingServiceMock.Object)
            .AddSingleton<IMachineService>(machineService)
            .AddSingleton<IProducedJobService>(producedJobService)
            .AddSingleton<IMachineSnapshotService>(machineSnapshotService)
            .AddSingleton<IKpiService>(
                new KpiService(new UnitService(),
                new MachineMetaDataService(),
                _kpiDataHandlerClient.Object))
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