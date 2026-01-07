using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.Interceptors;
using FrameworkAPI.Mutations;
using FrameworkAPI.Schema.Misc;
using FrameworkAPI.Schema.ProducedJob;
using FrameworkAPI.Services;
using FrameworkAPI.Services.Interfaces;
using FrameworkAPI.Test.TestHelpers;
using HotChocolate;
using HotChocolate.Execution;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Snapshooter.Xunit;
using WuH.Ruby.Common.Core;
using WuH.Ruby.KpiDataHandler.Client;
using WuH.Ruby.MetaDataHandler.Client;
using WuH.Ruby.ProductionPeriodsDataHandler.Client;
using Xunit;

namespace FrameworkAPI.Test.Mutations;

public class ProducedJobsMutationIntegrationTests
{
    private const string UserId = "test-user-id";

    private readonly Mock<IKpiEventQueueWrapper> _kpiEventQueueWrapperMock = new();
    private readonly Mock<IProductionPeriodsDataHandlerHttpClient> _productionPeriodsDataHandlerMock = new();
    private readonly Mock<IMachineService> _machineServiceMock = new();

    [Fact]
    public async Task ProducedJobChangeMachineTargetSpeed_With_Value_Calls_Client_With_Event_And_Returns_Value()
    {
        // Arrange
        const double targetSpeed = 100;
        const string associatedJob = "test_job";
        const string machineId = "EQ10211";

        _kpiEventQueueWrapperMock
            .Setup(m => m.SendSetTargetSpeedOfJobEventAndWaitForReply(It.Is<SetTargetSpeedOfJobEventMessage>(message =>
                message.TargetSpeed == targetSpeed
                && message.AssociatedJob == associatedJob
                && message.UserId == UserId)))
            .ReturnsAsync(new InternalResponse())
            .Verifiable(Times.Once);

        SetupServiceMocks(machineId, associatedJob);

        var executor = await InitializeExecutor();

        var query = ProducedJobsChangeMachineTargetSpeedMutation(associatedJob, machineId, targetSpeed);

        var request = QueryRequestBuilder
            .New()
            .SetQuery(query)
            .AddRoleClaims("go-general")
            .AddGlobalState("userId", UserId)
            .Create();

        // Act
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();
    }

    [Fact]
    public async Task ProducedJobChangeMachineTargetSetupTimeInMin_With_Value_Calls_Client_With_Event_And_Returns_Value()
    {
        // Arrange
        const double targetSetupTime = 100;
        const string associatedJob = "test_job";
        const string machineId = "EQ10211";

        _kpiEventQueueWrapperMock
            .Setup(m => m.SendSetTargetSetupTimeOfJobEventAndWaitForReply(It.Is<SetTargetSetupTimeOfJobEventMessage>(message =>
                message.TargetSetupTime == targetSetupTime
                && message.AssociatedJob == associatedJob
                && message.UserId == UserId)))
            .ReturnsAsync(new InternalResponse())
            .Verifiable(Times.Once);

        SetupServiceMocks(machineId, associatedJob);

        var executor = await InitializeExecutor();

        var query = ProducedJobChangeMachineTargetSetupTimeInMinMutation(associatedJob, machineId, targetSetupTime);

        var request = QueryRequestBuilder
            .New()
            .SetQuery(query)
            .AddRoleClaims("go-general")
            .AddGlobalState("userId", UserId)
            .Create();

        // Act
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();
    }

    [Fact]
    public async Task ProducedJobChangeMachineTargetDownTimeInMin_With_Value_Calls_Client_With_Event_And_Returns_Value()
    {
        // Arrange
        const double targetDownTime = 100;
        const string associatedJob = "test_job";
        const string machineId = "EQ10211";

        _kpiEventQueueWrapperMock
            .Setup(m => m.SendSetTargetDownTimeOfJobEventAndWaitForReply(It.Is<SetTargetDownTimeOfJobEventMessage>(message =>
                message.TargetDownTime == targetDownTime
                && message.AssociatedJob == associatedJob
                && message.UserId == UserId)))
            .ReturnsAsync(new InternalResponse())
            .Verifiable(Times.Once);

        SetupServiceMocks(machineId, associatedJob);

        var executor = await InitializeExecutor();

        var query = ProducedJobChangeMachineTargetDownTimeInMinMutation(associatedJob, machineId, targetDownTime);

        var request = QueryRequestBuilder
            .New()
            .SetQuery(query)
            .AddRoleClaims("go-general")
            .AddGlobalState("userId", UserId)
            .Create();

        // Act
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();
    }

    [Fact]
    public async Task ProducedJobChangeMachineTargetScrapCountDuringProduction_With_Value_Calls_Client_With_Event_And_Returns_Value()
    {
        // Arrange
        const double scrapDuringProduction = 100;
        const string associatedJob = "test_job";
        const string machineId = "EQ10211";

        _kpiEventQueueWrapperMock
            .Setup(m => m.SendSetTargetScrapDuringProductionOfJobEventAndWaitForReply(It.Is<SetTargetScrapDuringProductionOfJobEventMessage>(message =>
                message.TargetScrapDuringProduction == scrapDuringProduction
                && message.AssociatedJob == associatedJob
                && message.UserId == UserId)))
            .ReturnsAsync(new InternalResponse())
            .Verifiable(Times.Once);

        SetupServiceMocks(machineId, associatedJob);

        var executor = await InitializeExecutor();

        var query = ProducedJobChangeMachineTargetScrapCountDuringProductionMutation(associatedJob, machineId, scrapDuringProduction);

        var request = QueryRequestBuilder
            .New()
            .SetQuery(query)
            .AddRoleClaims("go-general")
            .AddGlobalState("userId", UserId)
            .Create();

        // Act
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();
    }

    private void SetupServiceMocks(string machineId, string associatedJob)
    {
        var department = MachineDepartment.PaperSack;
        var family = MachineFamily.PaperSackBottomer;

        var jobInfo = new JobInfo()
        {
            JobId = associatedJob,
            MachineId = machineId
        };

        _productionPeriodsDataHandlerMock
            .Setup(x => x.GetJobInfo(It.IsAny<CancellationToken>(), machineId, associatedJob))
            .ReturnsAsync(new InternalItemResponse<JobInfo>(jobInfo))
            .Verifiable(Times.Once);
        _machineServiceMock
            .Setup(x => x.GetMachineBusinessUnit(machineId, CancellationToken.None))
            .ReturnsAsync(department)
            .Verifiable(Times.Once);
        _machineServiceMock
            .Setup(x => x.GetMachineFamily(machineId, CancellationToken.None))
            .ReturnsAsync(family)
            .Verifiable(Times.Once);
    }

    private async Task<IRequestExecutor> InitializeExecutor()
    {
        var services = new ServiceCollection();

        WuH.Ruby.Common.ProjectTemplate.ServiceCollectionExtensions.AddAuthentication(services);

        var jobInfoCachingService = new Mock<IJobInfoCachingService>();
        var logger = new Mock<ILogger<ProducedJobService>>();
        var producedJobService = new ProducedJobService(
            jobInfoCachingService.Object,
            _productionPeriodsDataHandlerMock.Object,
            _machineServiceMock.Object,
            logger.Object,
            _kpiEventQueueWrapperMock.Object);

        return await services
            .AddSingleton<IProducedJobService>(producedJobService)
            .AddSingleton(new Mock<ILogger<DefaultAuthorizationService>>().Object)
            .AddSingleton(new Mock<IMetaDataHandlerHttpClient>().Object)
            .AddAuthorization()
            .AddHttpContextAccessor()
            .AddGraphQLServer()
            .AddDefaultTransactionScopeHandler()
            .AddMutationConventions()
            .AddHttpRequestInterceptor<HttpRequestInterceptor>()
            .ModifyOptions(t => t.StrictValidation = false)
            .AddAuthorization()
            .AddMutationType(q => q.Name("Mutation"))
            .AddType<ProducedJobsMutation>()
            .AddType<PaperSackProducedJob>()
            .AddSorting()
            .AddFiltering()
            .BuildRequestExecutorAsync();
    }

    private static string ProducedJobsChangeMachineTargetSpeedMutation(
        string associatedJob,
        string machineId,
        double targetSpeed)
    {
        return $@"mutation
        {{
            producedJobChangeMachineTargetSpeed(input:
                {{
                    targetSpeedRequest: {{
                        associatedJob: ""{associatedJob}"",
                        machineId: ""{machineId}"",
                        targetSpeed: {targetSpeed.ToString("F", CultureInfo.InvariantCulture)}
                    }}
                }})
            {{
                changedProducedJob {{
                    jobId
                    machineId
                }}
                errors {{
                    ... on Error {{
                        __typename
                        message
                    }}
                    ... on ParameterInvalidError {{
                        __typename
                        message
                    }}
                    ... on InternalServiceError {{
                        __typename
                        message     
                        statusCode
                    }}
                }}
            }}
        }}";
    }

    private static string ProducedJobChangeMachineTargetSetupTimeInMinMutation(
        string associatedJob,
        string machineId,
        double targetSetupTimeInMin)
    {
        return $@"mutation
        {{
            producedJobChangeMachineTargetSetupTimeInMin(input:
                {{
                    setupTimeRequest: {{
                        associatedJob: ""{associatedJob}"",
                        machineId: ""{machineId}"",
                        targetSetupTimeInMin: {targetSetupTimeInMin.ToString("F", CultureInfo.InvariantCulture)}
                    }}
                }})
            {{
                changedProducedJob {{
                    jobId
                    machineId  
                }}
                errors {{
                    ... on Error {{
                        __typename
                        message
                    }}
                    ... on ParameterInvalidError {{
                        __typename
                        message
                    }}
                    ... on InternalServiceError {{
                        __typename
                        message     
                        statusCode
                    }}
                }}
            }}
        }}";
    }

    private static string ProducedJobChangeMachineTargetDownTimeInMinMutation(
        string associatedJob,
        string machineId,
        double targetDownTimeInMin)
    {
        return $@"mutation
        {{
            producedJobChangeMachineTargetDownTimeInMin(input:
                {{
                    downTimeRequest: {{
                        associatedJob: ""{associatedJob}"",
                        machineId: ""{machineId}"",
                        targetDownTimeInMin: {targetDownTimeInMin.ToString("F", CultureInfo.InvariantCulture)}
                    }}
                }})
            {{
                changedProducedJob {{
                    jobId
                    machineId  
                }}
                errors {{
                    ... on Error {{
                        __typename
                        message
                    }}
                    ... on ParameterInvalidError {{
                        __typename
                        message
                    }}
                    ... on InternalServiceError {{
                        __typename
                        message     
                        statusCode
                    }}
                }}
            }}
        }}";
    }

    private static string ProducedJobChangeMachineTargetScrapCountDuringProductionMutation(
        string associatedJob,
        string machineId,
        double targetScrapCountDuringProduction)
    {
        return $@"mutation
        {{
            producedJobChangeMachineTargetScrapCountDuringProduction(input:
                {{
                    scrapCountDuringProductionRequest: {{
                        associatedJob: ""{associatedJob}"",
                        machineId: ""{machineId}"",
                        targetScrapCountDuringProduction: {targetScrapCountDuringProduction.ToString("F", CultureInfo.InvariantCulture)}
                    }}
                }})
            {{
                changedProducedJob {{
                    jobId
                    machineId  
                }}
                errors {{
                    ... on Error {{
                        __typename
                        message
                    }}
                    ... on ParameterInvalidError {{
                        __typename
                        message
                    }}
                    ... on InternalServiceError {{
                        __typename
                        message     
                        statusCode
                    }}
                }}
            }}
        }}";
    }
}