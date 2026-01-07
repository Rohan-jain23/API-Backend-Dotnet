using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using FrameworkAPI.DataLoaders;
using FrameworkAPI.Helpers;
using FrameworkAPI.Interceptors;
using FrameworkAPI.Mutations;
using FrameworkAPI.Schema.Misc;
using FrameworkAPI.Schema.Settings.DashboardSettings;
using FrameworkAPI.Services.Settings;
using FrameworkAPI.Test.TestHelpers;
using HotChocolate;
using HotChocolate.Execution;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Snapshooter.Xunit;
using WuH.Ruby.Common.Core;
using WuH.Ruby.Settings.Client;
using WuH.Ruby.Supervisor.Client;
using Xunit;
using SettingsModels = WuH.Ruby.Settings.Client.Models;

namespace FrameworkAPI.Test.Mutations;

public class DashboardSettingsMutationIntegrationTests
{
    private const string UserId = "12345678-1234-1234-1234-123456789012";
    private const string DashboardId = "87654321-27c8-4b83-ae0b-24154b66bdb1";
    private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());

    private readonly Mock<ISettingsService> _settingsServiceMock = new();
    private readonly Mock<ISupervisorHttpClient> _supervisorHttpClientMock = new();

    [Fact]
    public async Task DashboardSettingsCreateDashboard_Calls_Client_And_Returns_Value()
    {
        // Arrange
        var dashboard = GetSampleDashboardSettings();
        var createRequest = _fixture.Build<CreateOrEditConfiguredDashboardRequest>()
                                    .With(m => m.Department, _fixture.Create<MachineDepartment>())
                                    .Without(m => m.DashboardId)
                                    .Without(m => m.Widget1)
                                    .Create();

        var existingDashboards = _fixture.Build<SettingsModels.DashboardSettings>()
                                .With(m => m.Department, _fixture.Create<MachineDepartment>().ToString())
                                .CreateMany(5)
                                .ToList();

        var user = new User(Guid.Parse(UserId), "FakeName");

        var executor = await InitializeExecutor();
        var query = CreateDashboardSettingsMutation(createRequest);
        var request = QueryRequestBuilder
            .New()
            .SetQuery(query)
            .AddRoleClaims("go-general")
            .AddGlobalState("userId", UserId)
            .Create();

        _settingsServiceMock
            .Setup(mock => mock.AddOrUpdateDashboardSettings(
                null,
                createRequest.Department.ToString(),
                UserId,
                createRequest.IsPublic,
                true,
                createRequest.FriendlyName,
                It.IsAny<SettingsModels.DashboardWidgetSettings?>(),
                It.IsAny<SettingsModels.DashboardWidgetSettings?>(),
                It.IsAny<SettingsModels.DashboardWidgetSettings?>(),
                It.IsAny<SettingsModels.DashboardWidgetSettings?>(),
                It.IsAny<SettingsModels.DashboardWidgetSettings?>(),
                It.IsAny<SettingsModels.DashboardWidgetSettings?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<SettingsModels.DashboardSettings>(dashboard));

        _supervisorHttpClientMock
            .Setup(mock => mock.ResolveNames(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalListResponse<User>([user]))
            .Verifiable(Times.Once);

        // Act
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        _settingsServiceMock.VerifyAll();
        _settingsServiceMock.VerifyNoOtherCalls();
        _supervisorHttpClientMock.Verify();
        _supervisorHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task DashboardSettingsEditDashboard_Calls_Client_And_Returns_Value()
    {
        // Arrange
        var dashboard = GetSampleDashboardSettings();
        var createRequest = _fixture.Build<CreateOrEditConfiguredDashboardRequest>()
                                    .With(m => m.Department, _fixture.Create<MachineDepartment>())
                                    .With(m => m.DashboardId, DashboardId)
                                    .Without(m => m.Widget1)
                                    .Create();

        var existingDashboards = _fixture.Build<SettingsModels.DashboardSettings>()
                                .With(m => m.Department, _fixture.Create<MachineDepartment>().ToString())
                                .CreateMany(5)
                                .ToList();

        var user = new User(Guid.Parse(UserId), "FakeName");

        var executor = await InitializeExecutor();
        var query = EditDashboardSettingsMutation(createRequest);
        var request = QueryRequestBuilder
            .New()
            .SetQuery(query)
            .AddRoleClaims("go-general")
            .AddGlobalState("userId", UserId)
            .Create();

        _settingsServiceMock
            .Setup(mock => mock.GetDashboardSettingsById(DashboardId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<SettingsModels.DashboardSettings>(dashboard));

        _settingsServiceMock
            .Setup(mock => mock.AddOrUpdateDashboardSettings(
                DashboardId,
                createRequest.Department.ToString(),
                UserId,
                createRequest.IsPublic,
                true,
                createRequest.FriendlyName,
                It.IsAny<SettingsModels.DashboardWidgetSettings?>(),
                It.IsAny<SettingsModels.DashboardWidgetSettings?>(),
                It.IsAny<SettingsModels.DashboardWidgetSettings?>(),
                It.IsAny<SettingsModels.DashboardWidgetSettings?>(),
                It.IsAny<SettingsModels.DashboardWidgetSettings?>(),
                It.IsAny<SettingsModels.DashboardWidgetSettings?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<SettingsModels.DashboardSettings>(dashboard));

        _supervisorHttpClientMock
            .Setup(mock => mock.ResolveNames(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalListResponse<User>([user]))
            .Verifiable(Times.Once);

        // Act
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        _settingsServiceMock.VerifyAll();
        _settingsServiceMock.VerifyNoOtherCalls();
        _supervisorHttpClientMock.Verify();
        _supervisorHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task DashboardSettingsDeleteDashboard_Calls_Client_And_Returns_Value()
    {
        // Arrange
        var dashboard = GetSampleDashboardSettings();

        var existingDashboards = _fixture.Build<SettingsModels.DashboardSettings>()
                                .With(m => m.Department, _fixture.Create<MachineDepartment>().ToString())
                                .CreateMany(5)
                                .ToList();

        // Need at least one extra dashboard for the user so we can delete the test dashboard
        existingDashboards[0].CreatorUserId = UserId;

        var executor = await InitializeExecutor();
        var query = DeleteDashboardSettingsMutation();
        var request = QueryRequestBuilder
            .New()
            .SetQuery(query)
            .AddRoleClaims("go-general")
            .AddGlobalState("userId", UserId)
            .Create();

        _settingsServiceMock
            .Setup(mock => mock.GetAllDashboardSettings(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalListResponse<SettingsModels.DashboardSettings>([.. existingDashboards, dashboard]));

        _settingsServiceMock
            .Setup(mock => mock.DeleteDashboardSettingsById(DashboardId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalResponse());

        // Act
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        _settingsServiceMock.VerifyAll();
        _settingsServiceMock.VerifyNoOtherCalls();
        _supervisorHttpClientMock.VerifyAll();
        _supervisorHttpClientMock.VerifyNoOtherCalls();
    }

    private async Task<IRequestExecutor> InitializeExecutor()
    {
        var services = new ServiceCollection();

        WuH.Ruby.Common.ProjectTemplate.ServiceCollectionExtensions.AddAuthentication(services);

        var userSettingsService = new UserSettingsService(_settingsServiceMock.Object);
        var dashboardSettingsService = new DashboardSettingsService(_settingsServiceMock.Object);
        var userNameCacheDataLoader = new UserNameCacheDataLoader(_supervisorHttpClientMock.Object);

        return await services
            .AddSingleton<IUserSettingsService>(userSettingsService)
            .AddSingleton<IDashboardSettingsService>(dashboardSettingsService)
            .AddSingleton(userNameCacheDataLoader)
            .AddSingleton(new Mock<ILogger<DefaultAuthorizationService>>().Object)
            .AddAuthorization()
            .AddHttpContextAccessor()
            .AddGraphQLServer()
            .AddDefaultTransactionScopeHandler()
            .AddMutationConventions()
            .AddHttpRequestInterceptor<HttpRequestInterceptor>()
            .ModifyOptions(t => t.StrictValidation = false)
            .AddAuthorization()
            .AddMutationType(q => q.Name("Mutation"))
            .AddType<DashboardSettingsMutation>()
            .AddSorting()
            .AddFiltering()
            .BuildRequestExecutorAsync();
    }

    private static string DashboardWidgetSettingsInputString(DashboardWidgetSettings widgetSettings)
    {
        return $@"{{machineIds: ""{string.Join(",", widgetSettings.MachineIds)}"", widgetCatalogId: ""{widgetSettings.WidgetCatalogId}""}}";
    }

    private static string CreateOrEditConfiguredDashboardRequestInputString(CreateOrEditConfiguredDashboardRequest request)
    {
        var dashboardId = string.IsNullOrWhiteSpace(request.DashboardId) ? "" : $"dashboardId: \"{request.DashboardId}\", ";
        var widget1 = request.Widget1 is null ? "" : $", widget1: {DashboardWidgetSettingsInputString(request.Widget1)}";
        var widget2 = request.Widget2 is null ? "" : $", widget2: {DashboardWidgetSettingsInputString(request.Widget2)}";
        var widget3 = request.Widget3 is null ? "" : $", widget3: {DashboardWidgetSettingsInputString(request.Widget3)}";
        var widget4 = request.Widget4 is null ? "" : $", widget4: {DashboardWidgetSettingsInputString(request.Widget4)}";
        var widget5 = request.Widget5 is null ? "" : $", widget5: {DashboardWidgetSettingsInputString(request.Widget5)}";
        var widget6 = request.Widget6 is null ? "" : $", widget6: {DashboardWidgetSettingsInputString(request.Widget6)}";

        return $"{{ {dashboardId}department: {request.Department.ToScreamingSnakeCase()}, isPublic: {request.IsPublic.ToString().ToLower()}, friendlyName: \"{request.FriendlyName}\"{widget1}{widget2}{widget3}{widget4}{widget5}{widget6} }}";
    }

    private static string CreateDashboardSettingsMutation(CreateOrEditConfiguredDashboardRequest request)
    {
        return $@"mutation {{
            dashboardSettingsCreateDashboard(
                input: {{ createDashboardRequest: {CreateOrEditConfiguredDashboardRequestInputString(request)} }}
            ) {{
                dashboardSettings {{
                    canOnlyBeEditedByCreator
                    createdDate
                    creatorFullName
                    creatorUserId
                    dashboardId
                    department
                    friendlyName
                    isPublic
                    lastEditedDate
                    lastEditorFullName
                    lastEditorUserId
                    widget1 {{
                        additionalSetting
                        machineIds
                        widgetCatalogId
                    }}
                    widget2 {{
                        additionalSetting
                        machineIds
                        widgetCatalogId
                    }}
                    widget3 {{
                        additionalSetting
                        machineIds
                        widgetCatalogId
                    }}
                    widget4 {{
                        additionalSetting
                        machineIds
                        widgetCatalogId
                    }}
                    widget5 {{
                        additionalSetting
                        machineIds
                        widgetCatalogId
                    }}
                    widget6 {{
                        additionalSetting
                        machineIds
                        widgetCatalogId
                    }}
                }}
                errors {{
                    ... on Error {{
                        message
                    }}
                    ... on InternalServiceError {{
                        statusCode
                    }}
                }}
            }}
        }}";
    }

    private static string EditDashboardSettingsMutation(CreateOrEditConfiguredDashboardRequest request)
    {
        return $@"mutation {{
            dashboardSettingsEditDashboard(
                input: {{ editDashboardRequest: {CreateOrEditConfiguredDashboardRequestInputString(request)} }}
            ) {{
                dashboardSettings {{
                    canOnlyBeEditedByCreator
                    createdDate
                    creatorFullName
                    creatorUserId
                    dashboardId
                    department
                    friendlyName
                    isPublic
                    lastEditedDate
                    lastEditorFullName
                    lastEditorUserId
                    widget1 {{
                        additionalSetting
                        machineIds
                        widgetCatalogId
                    }}
                    widget2 {{
                        additionalSetting
                        machineIds
                        widgetCatalogId
                    }}
                    widget3 {{
                        additionalSetting
                        machineIds
                        widgetCatalogId
                    }}
                    widget4 {{
                        additionalSetting
                        machineIds
                        widgetCatalogId
                    }}
                    widget5 {{
                        additionalSetting
                        machineIds
                        widgetCatalogId
                    }}
                    widget6 {{
                        additionalSetting
                        machineIds
                        widgetCatalogId
                    }}
                }}
                errors {{
                    ... on Error {{
                        message
                    }}
                    ... on InternalServiceError {{
                        statusCode
                    }}
                }}
            }}
        }}";
    }

    private static string DeleteDashboardSettingsMutation()
    {
        return $@"mutation {{
            dashboardSettingsDeleteDashboard(
                input: {{ dashboardId: ""{DashboardId}"" }}
            ) {{
                dashboardId
                errors {{
                    ... on Error {{
                        message
                    }}
                    ... on InternalServiceError {{
                        statusCode
                    }}
                }}
            }}
        }}";
    }

    private static SettingsModels.DashboardSettings GetSampleDashboardSettings()
    => new()
    {
        DashboardId = DashboardId,
        CanOnlyBeEditedByCreator = true,
        CreatedDate = DateTime.UnixEpoch,
        CreatorUserId = UserId,
        Department = MachineDepartment.Extrusion.ToString(),
        FriendlyName = "FriendlyName",
        IsPublic = true,
        ModifiedDate = DateTime.UnixEpoch,
        ModifiedUserId = UserId,
        WidgetSettings1 = GetSampleDashboardWidgetSettings().MapToInternalDashboardWidgetSettings(),
        WidgetSettings2 = null,
        WidgetSettings3 = GetSampleDashboardWidgetSettings().MapToInternalDashboardWidgetSettings(),
        WidgetSettings4 = GetSampleDashboardWidgetSettings().MapToInternalDashboardWidgetSettings(),
        WidgetSettings5 = GetSampleDashboardWidgetSettings().MapToInternalDashboardWidgetSettings(),
        WidgetSettings6 = GetSampleDashboardWidgetSettings().MapToInternalDashboardWidgetSettings()

    };

    private static DashboardWidgetSettings GetSampleDashboardWidgetSettings()
    => new()
    {
        MachineIds = ["EQ12345", "EQ11111", "EQ33333"],
        WidgetCatalogId = "FakeCatalogId",
        AdditionalSetting = "PlainString"
    };
}