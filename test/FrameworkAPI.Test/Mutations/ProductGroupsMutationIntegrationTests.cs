using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.Interceptors;
using FrameworkAPI.Mutations;
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
using WuH.Ruby.KpiDataHandler.Client.Models;
using WuH.Ruby.MachineSnapShooter.Client;
using WuH.Ruby.MetaDataHandler.Client;
using WuH.Ruby.ProductionPeriodsDataHandler.Client;
using Xunit;

namespace FrameworkAPI.Test.Mutations;

public class ProductGroupsMutationIntegrationTests
{
    private const string UserId = "test-user-id";

    private readonly Mock<IKpiService> _kpiServiceMock = new();
    private readonly Mock<IKpiEventQueueWrapper> _kpiEventQueueWrapperMock = new();
    private readonly Mock<IKpiDataHandlerClient> _kpiDataHandlerClientMock = new();

    [Fact]
    public async Task Change_ProductGroup_OverallNote_With_Value_Calls_Client_With_Event_And_Returns_Value()
    {
        // Arrange
        const string note = "sampleNote";
        var productGroup = new PaperSackProductGroup
        {
            Id = "v0-TX",
            ProductGroupDefinitionVersion = 1,
            ParentId = null,
            FriendlyName = "FriendlyName",
            FirstProductionDate = DateTime.UnixEpoch,
            LastProductionDate = DateTime.UnixEpoch.AddMinutes(1),
            Attributes = [new PaperSackProductGroupAttribute(PaperSackProductGroupAttributeType.Bool, SnapshotColumnIds.PaperSackProductIsValveSack, null)],
            ProductIds = [],
            JobIdsPerMachine = [],
            Note = note,
            TargetSpeedPerMachine = [],
            NotesPerMachine = [],
        };

        _kpiEventQueueWrapperMock
            .Setup(m => m.SendSetOverallNoteOfProductGroupEventAndWaitForReply(It.Is<SetOverallNoteOfProductGroupEventMessage>(message =>
                message.PaperSackProductGroupId == productGroup.Id
                && message.Note == note
                && message.UserId == UserId)))
            .ReturnsAsync(new InternalResponse())
            .Verifiable(Times.Once);

        _kpiDataHandlerClientMock
            .Setup(m => m.GetPaperSackProductGroupById(It.IsAny<CancellationToken>(), productGroup.Id))
            .ReturnsAsync(new InternalItemResponse<PaperSackProductGroup>(productGroup))
            .Verifiable(Times.Once);

        var executor = await InitializeExecutor();

        var query = ProductGroupChangeOverallNoteMutation(productGroup.Id, note);

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
    public async Task Change_ProductGroup_MachineNote_With_Value_Calls_Client_With_Event_And_Returns_Value()
    {
        // Arrange
        const string machineId = "EQ10101";
        const string note = "sampleMachineNote";
        var productGroup = new PaperSackProductGroup
        {
            Id = "v0-TX",
            ProductGroupDefinitionVersion = 1,
            ParentId = null,
            FriendlyName = "FriendlyName",
            FirstProductionDate = DateTime.UnixEpoch,
            LastProductionDate = DateTime.UnixEpoch.AddMinutes(1),
            Attributes = [new PaperSackProductGroupAttribute(PaperSackProductGroupAttributeType.Bool, SnapshotColumnIds.PaperSackProductIsValveSack, null)],
            ProductIds = [],
            JobIdsPerMachine = [],
            Note = note,
            TargetSpeedPerMachine = [],
            NotesPerMachine = new Dictionary<string, string> { { machineId, note } },
        };

        _kpiEventQueueWrapperMock
            .Setup(m => m.SendSetMachineNoteOfProductGroupEventAndWaitForReply(It.Is<SetMachineNoteOfProductGroupEventMessage>(message =>
                message.PaperSackProductGroupId == productGroup.Id
                && message.MachineId == machineId
                && message.Note == note
                && message.UserId == UserId)))
            .ReturnsAsync(new InternalResponse())
            .Verifiable(Times.Once);

        _kpiDataHandlerClientMock
            .Setup(m => m.GetPaperSackProductGroupById(It.IsAny<CancellationToken>(), productGroup.Id))
            .ReturnsAsync(new InternalItemResponse<PaperSackProductGroup>(productGroup))
            .Verifiable(Times.Once);

        var executor = await InitializeExecutor();

        var query = ProductGroupChangeMachineNoteMutation(productGroup.Id, machineId, note);

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
    public async Task Change_ProductGroup_MachineTargetSpeed_With_Value_Calls_Client_With_Event_And_Returns_Value()
    {
        // Arrange
        const string machineId = "EQ10101";
        const double targetSpeed = 2321.4;
        var productGroup = new PaperSackProductGroup
        {
            Id = "v0-TX",
            ProductGroupDefinitionVersion = 1,
            ParentId = null,
            FriendlyName = "FriendlyName",
            FirstProductionDate = DateTime.UnixEpoch,
            LastProductionDate = DateTime.UnixEpoch.AddMinutes(1),
            Attributes = [new PaperSackProductGroupAttribute(PaperSackProductGroupAttributeType.Bool, SnapshotColumnIds.PaperSackProductIsValveSack, null)],
            ProductIds = [],
            JobIdsPerMachine = [],
            Note = null,
            TargetSpeedPerMachine = new Dictionary<string, double> { { machineId, targetSpeed } },
            NotesPerMachine = [],
        };

        _kpiEventQueueWrapperMock
            .Setup(m => m.SendSetMachineTargetSpeedOfProductGroupEventAndWaitForReply(It.Is<SetMachineTargetSpeedOfProductGroupEventMessage>(message =>
                message.PaperSackProductGroupId == productGroup.Id
                && message.MachineId == machineId
                && message.TargetSpeed.Equals(targetSpeed)
                && message.UserId == UserId)))
            .ReturnsAsync(new InternalResponse())
            .Verifiable(Times.Once);

        _kpiDataHandlerClientMock
            .Setup(m => m.GetPaperSackProductGroupById(It.IsAny<CancellationToken>(), productGroup.Id))
            .ReturnsAsync(new InternalItemResponse<PaperSackProductGroup>(productGroup))
            .Verifiable(Times.Once);

        var executor = await InitializeExecutor();

        var query = ProductGroupChangeMachineTargetSpeed(productGroup.Id, machineId, targetSpeed);

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

    private async Task<IRequestExecutor> InitializeExecutor()
    {
        var services = new ServiceCollection();

        WuH.Ruby.Common.ProjectTemplate.ServiceCollectionExtensions.AddAuthentication(services);

        var machineServiceMock = new Mock<IMachineService>();
        var productionPeriodsDataHandlerHttpClient = new Mock<IProductionPeriodsDataHandlerHttpClient>();
        var productGroupService = new ProductGroupService(
            machineServiceMock.Object,
            _kpiServiceMock.Object,
            _kpiEventQueueWrapperMock.Object,
            _kpiDataHandlerClientMock.Object,
            productionPeriodsDataHandlerHttpClient.Object);

        return await services
            .AddSingleton<IProductGroupService>(productGroupService)
            .AddSingleton(_kpiServiceMock.Object)
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
            .AddType<ProductGroupsMutation>()
            .AddSorting()
            .AddFiltering()
            .BuildRequestExecutorAsync();
    }

    private static string ProductGroupChangeOverallNoteMutation(
        string paperSackProductGroupId,
        string? note)
    {
        return $@"mutation
        {{
            productGroupChangeOverallNote(input:
                {{
                    productGroupChangeOverallNoteRequest: {{
                        paperSackProductGroupId: ""{paperSackProductGroupId}"",
                        note: {(note is not null ? $"\"{note}\"" : "null")}
                    }}
                }})
            {{
                changedProductGroup {{
                    overallNote
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
                        statusCode
                    }}
                }}
            }}
        }}";
    }

    private static string ProductGroupChangeMachineNoteMutation(
        string paperSackProductGroupId,
        string machineId,
        string? note)
    {
        return $@"mutation
        {{
            productGroupChangeMachineNote(input:
                {{
                    productGroupChangeMachineNoteRequest: {{
                        paperSackProductGroupId: ""{paperSackProductGroupId}"",
                        machineId: ""{machineId}"",
                        note: {(note is not null ? $"\"{note}\"" : "null")}
                    }}
                }})
            {{
                changedProductGroup {{
                     notePerMachine {{
                        key
                        value
                     }}
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
                        statusCode
                    }}
                }}
            }}
        }}";
    }

    private static string ProductGroupChangeMachineTargetSpeed(
        string paperSackProductGroupId,
        string machineId,
        double? targetSpeed)
    {
        return $@"mutation
        {{
            productGroupChangeMachineTargetSpeed(input:
                {{
                    productGroupChangeMachineTargetSpeedRequest: {{
                        paperSackProductGroupId: ""{paperSackProductGroupId}"",
                        machineId: ""{machineId}"",
                        targetSpeed: {targetSpeed?.ToString("F", CultureInfo.InvariantCulture) ?? "null"}
                    }}
                }})
            {{
                changedProductGroup {{
                     targetSpeedSettingPerMachine {{
                        key
                        value {{
                            value
                            unit
                        }}
                     }}
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
                        statusCode
                    }}
                }}
            }}
        }}";
    }
}