using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.Schema.ProducedJob;
using FrameworkAPI.Services;
using FrameworkAPI.Services.Interfaces;
using FrameworkAPI.Test.TestHelpers;
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

public class PrintingMachineProducedJobIntegrationTests
{
    private const string MachineId = "EQ00001";
    private const string JobId = "FakeJobId";

    private readonly Mock<ILatestMachineSnapshotCachingService> _latestMachineSnapshotCachingServiceMock = new();
    private readonly Mock<IKpiDataCachingService> _kpiDataCachingServiceMock = new();
    private readonly Mock<IKpiDataHandlerClient> _kpiDataHandlerClientMock = new();
    private readonly Mock<IMetaDataHandlerHttpClient> _metaDataHandlerHttpClientMock = new();
    private readonly Mock<IMachineCachingService> _machineCachingServiceMock = new();
    private readonly Mock<IProcessDataReaderHttpClient> _processDataReaderHttpClientMock = new();
    private readonly Mock<IProductionPeriodChangesQueueWrapper> _productionPeriodChangesQueueWrapper = new();
    private readonly Mock<IProductionPeriodsDataHandlerHttpClient> _productionPeriodsDataHandlerHttpClientMock = new();

    public PrintingMachineProducedJobIntegrationTests()
    {
        _machineCachingServiceMock
            .Setup(s =>
                s.GetMachinesAsInternalListResponse(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalListResponse<Machine>(new List<Machine>
            {
                new()
                {
                    MachineId = MachineId,
                    BusinessUnit = BusinessUnit.Printing
                }
            }));
    }

    [Fact]
    public async Task Get_Finished_Job_With_GoodLength()
    {
        // Arrange
        var executor = await InitializeExecutor();
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
    public async Task Get_Finished_Job_With_ScrapLength()
    {
        // Arrange
        var executor = await InitializeExecutor();
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
                        scrapLength {{
                            value
                            unit
                          }}
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
        KpiTestInitializer.VerifyResultAndMocks(
            result,
            _metaDataHandlerHttpClientMock,
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

        return await new ServiceCollection()
            .AddSingleton(_metaDataHandlerHttpClientMock.Object)
            .AddSingleton(_kpiDataCachingServiceMock.Object)
            .AddSingleton(_latestMachineSnapshotCachingServiceMock.Object)
            .AddSingleton(_productionPeriodsDataHandlerHttpClientMock.Object)
            .AddSingleton<IKpiService>(kpiService)
            .AddSingleton<IMachineService>(machineService)
            .AddSingleton<IProducedJobService>(producedJobService)
            .AddLogging()
            .AddGraphQLServer()
            .AddAuthorization()
            .ModifyOptions(t => t.StrictValidation = false)
            .AddQueryType(q => q.Name("Query"))
            .AddType<FrameworkAPI.Queries.ProducedJobQuery>()
            .AddType<PrintingProducedJob>()
            .AddSorting()
            .AddFiltering()
            .BuildRequestExecutorAsync();
    }
}