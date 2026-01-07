using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using FrameworkAPI.Interceptors;
using FrameworkAPI.Models.Settings;
using FrameworkAPI.Mutations;
using FrameworkAPI.Schema.Misc;
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
using Xunit;
using SettingsModels = WuH.Ruby.Settings.Client.Models;

namespace FrameworkAPI.Test.Mutations;

public class UserSettingsMutationIntegrationTests
{
    private const string UserId = "test-user-id";
    private const string DashboardId = "de2939bc-27c8-4b83-ae0b-24154b66bdb1";
    private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());

    private readonly Mock<ISettingsService> _settingsServiceMock = new();

    [Fact]
    public async Task Update_User_Language_Calls_Client_And_Returns_Value()
    {
        // Arrange
        const string langTag = "de-DE";

        var executor = await InitializeExecutor();

        _settingsServiceMock
            .Setup(mock => mock.PostSettingsForUserAndMachine(
                null,
                UserId,
                UserSettingIds.Language,
                langTag,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalResponse());

        var query = UpdateLanguageMutation(langTag);

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

        _settingsServiceMock.VerifyAll();
        _settingsServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Update_User_Language_With_Invalid_Value_Returns_Error()
    {
        // Arrange
        var executor = await InitializeExecutor();

        var query = UpdateLanguageMutation("xx-XX");

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

        _settingsServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Update_User_UnitSystem_Calls_Client_And_Returns_Value()
    {
        // Arrange
        const string unitSystem = "SI";

        var executor = await InitializeExecutor();

        _settingsServiceMock
            .Setup(mock =>
                mock.PostSettingsForUserAndMachine(
                    null,
                    UserId,
                    UserSettingIds.IsUnitRepresentationInSi,
                    "True",
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalResponse());

        var query = UpdateUnitSystemMutation(unitSystem);

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

        _settingsServiceMock.VerifyAll();
        _settingsServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Update_User_UnitSystem_With_Null_Value_Returns_Error()
    {
        // Arrange
        var executor = await InitializeExecutor();

        var query = UpdateUnitSystemMutation(null);

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

        _settingsServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Update_Default_Dashboard_Extrusion_With_Private_Dashboard_Returns_Error()
    {
        // Arrange   
        var dashboard = _fixture.Build<SettingsModels.DashboardSettings>()
                                .With(m => m.Department, _fixture.Create<MachineDepartment>().ToString())
                                .With(m => m.DashboardId, DashboardId)
                                .With(m => m.IsPublic, false)
                                .Create();
        var executor = await InitializeExecutor();
        var query = UpdateFavoriteDashboardMutation(MachineDepartment.Extrusion, DashboardId);
        var request = QueryRequestBuilder
            .New()
            .SetQuery(query)
            .AddRoleClaims("go-general")
            .AddGlobalState("userId", UserId)
            .Create();

        _settingsServiceMock
            .Setup(mock => mock.GetDashboardSettingsById(DashboardId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<SettingsModels.DashboardSettings>(dashboard));

        // Act   
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        _settingsServiceMock.VerifyAll();
        _settingsServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Update_Default_Dashboard_Extrusion_With_Invalid_Department_Returns_Error()
    {
        // Arrange   
        var dashboard = _fixture.Build<SettingsModels.DashboardSettings>()
                                .With(m => m.DashboardId, DashboardId)
                                .With(m => m.Department, "Printing")
                                .With(m => m.IsPublic, true)
                                .Create();
        var executor = await InitializeExecutor();
        var query = UpdateFavoriteDashboardMutation(MachineDepartment.Extrusion, DashboardId);
        var request = QueryRequestBuilder
            .New()
            .SetQuery(query)
            .AddRoleClaims("go-general")
            .AddGlobalState("userId", UserId)
            .Create();

        _settingsServiceMock
            .Setup(mock => mock.GetDashboardSettingsById(DashboardId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<SettingsModels.DashboardSettings>(dashboard));

        // Act   
        await using var result = await executor.ExecuteAsync(request);

        // Assert   
        result.ToJson().MatchSnapshot();

        _settingsServiceMock.VerifyAll();
        _settingsServiceMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(MachineDepartment.Extrusion)]
    [InlineData(MachineDepartment.PaperSack)]
    [InlineData(MachineDepartment.Printing)]
    [InlineData(MachineDepartment.Other)]
    public async Task Update_Default_Dashboard_Calls_Client_And_Returns_Value(MachineDepartment department)
    {
        // Arrange   
        var dashboard = _fixture.Build<SettingsModels.DashboardSettings>()
                                .With(m => m.DashboardId, DashboardId)
                                .With(m => m.Department, department.ToString())
                                .With(m => m.IsPublic, true)
                                .Create();
        var executor = await InitializeExecutor();
        var query = UpdateFavoriteDashboardMutation(department, DashboardId);
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
            .Setup(mock => mock.PostSettingsForUserAndMachine(
                    null,
                    UserId,
                    It.Is<string>(id => id.Contains(department.ToString())),
                    DashboardId,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalResponse());

        // Act   
        await using var result = await executor.ExecuteAsync(request);

        // Assert   
        result.ToJson().MatchSnapshot($"{nameof(UserSettingsMutationIntegrationTests)}.{nameof(Update_Default_Dashboard_Calls_Client_And_Returns_Value)}_{department}");

        _settingsServiceMock.VerifyAll();
        _settingsServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Update_User_MachineDepartment_Calls_Client_And_Returns_Value()
    {
        // Arrange
        const string machineDepartment = "PRINTING";

        var executor = await InitializeExecutor();

        _settingsServiceMock
            .Setup(mock =>
                mock.PostSettingsForUserAndMachine(
                    null,
                    UserId,
                    UserSettingIds.SelectedMachineDepartment,
                    MachineDepartment.Printing.ToString(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalResponse());

        var query = UpdateMachineDepartment(machineDepartment);

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

        _settingsServiceMock.VerifyAll();
        _settingsServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Update_User_Null_MachineDepartment_Calls_Client_And_Returns_Value()
    {
        // Arrange
        var executor = await InitializeExecutor();

        _settingsServiceMock
            .Setup(mock =>
                mock.PostSettingsForUserAndMachine(
                    null,
                    UserId,
                    UserSettingIds.SelectedMachineDepartment,
                    null,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalResponse());

        var query = UpdateMachineDepartment(null);

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

        _settingsServiceMock.VerifyAll();
        _settingsServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Update_User_PrintingMachineFamily_Calls_Client_And_Returns_Value()
    {
        // Arrange
        const string machineFamily = "FLEXO_PRINT";

        var executor = await InitializeExecutor();

        _settingsServiceMock
            .Setup(mock =>
                mock.PostSettingsForUserAndMachine(
                    null,
                    UserId,
                    UserSettingIds.SelectedPrintingMachineFamily,
                    MachineFamily.FlexoPrint.ToString(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalResponse());

        var query = UpdatePrintingMachineFamilyMutation(machineFamily);

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

        _settingsServiceMock.VerifyAll();
        _settingsServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Update_User_Null_PrintingMachineFamily_Calls_Client_And_Returns_Value()
    {
        // Arrange
        var executor = await InitializeExecutor();

        _settingsServiceMock
            .Setup(mock =>
                mock.PostSettingsForUserAndMachine(
                    null,
                    UserId,
                    UserSettingIds.SelectedPrintingMachineFamily,
                    null,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalResponse());

        var query = UpdatePrintingMachineFamilyMutation(null);

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

        _settingsServiceMock.VerifyAll();
        _settingsServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Update_User_Null_ExtrusionMachineFamily_Calls_Client_And_Returns_Value()
    {
        // Arrange
        var executor = await InitializeExecutor();

        _settingsServiceMock
            .Setup(mock =>
                mock.PostSettingsForUserAndMachine(
                    null,
                    UserId,
                    UserSettingIds.SelectedExtrusionMachineFamily,
                    null,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalResponse());

        var query = UpdateExtrusionMachineFamilyMutation(null);

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

        _settingsServiceMock.VerifyAll();
        _settingsServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Update_User_ExtrusionMachineFamily_Calls_Client_And_Returns_Value()
    {
        // Arrange
        const string machineFamily = "BLOW_FILM";

        var executor = await InitializeExecutor();

        _settingsServiceMock
            .Setup(mock =>
                mock.PostSettingsForUserAndMachine(
                    null,
                    UserId,
                    UserSettingIds.SelectedExtrusionMachineFamily,
                    MachineFamily.BlowFilm.ToString(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalResponse());

        var query = UpdateExtrusionMachineFamilyMutation(machineFamily);

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

        _settingsServiceMock.VerifyAll();
        _settingsServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Update_User_PaperSackMachineFamily_Calls_Client_And_Returns_Value()
    {
        // Arrange
        const string machineFamily = "PAPER_SACK_BOTTOMER";

        var executor = await InitializeExecutor();

        _settingsServiceMock
            .Setup(mock =>
                mock.PostSettingsForUserAndMachine(
                    null,
                    UserId,
                    UserSettingIds.SelectedPaperSackMachineFamily,
                    MachineFamily.PaperSackBottomer.ToString(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalResponse());

        var query = UpdatePaperSackMachineFamilyMutation(machineFamily);

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

        _settingsServiceMock.VerifyAll();
        _settingsServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Update_User_Null_PaperSackMachineFamily_Calls_Client_And_Returns_Value()
    {
        // Arrange
        var executor = await InitializeExecutor();

        _settingsServiceMock
            .Setup(mock =>
                mock.PostSettingsForUserAndMachine(
                    null,
                    UserId,
                    UserSettingIds.SelectedPaperSackMachineFamily,
                    null,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalResponse());

        var query = UpdatePaperSackMachineFamilyMutation(null);

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

        _settingsServiceMock.VerifyAll();
        _settingsServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Update_User_OtherMachineFamily_Calls_Client_And_Returns_Value()
    {
        // Arrange
        const string machineFamily = "OTHER";

        var executor = await InitializeExecutor();

        _settingsServiceMock
            .Setup(mock =>
                mock.PostSettingsForUserAndMachine(
                    null,
                    UserId,
                    UserSettingIds.SelectedOtherMachineFamily,
                    MachineFamily.Other.ToString(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalResponse());

        var query = UpdateOtherMachineFamilyMutation(machineFamily);

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

        _settingsServiceMock.VerifyAll();
        _settingsServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Update_User_Null_OtherMachineFamily_Calls_Client_And_Returns_Value()
    {
        // Arrange
        var executor = await InitializeExecutor();

        _settingsServiceMock
            .Setup(mock =>
                mock.PostSettingsForUserAndMachine(
                    null,
                    UserId,
                    UserSettingIds.SelectedOtherMachineFamily,
                    null,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalResponse());

        var query = UpdateOtherMachineFamilyMutation(null);

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

        _settingsServiceMock.VerifyAll();
        _settingsServiceMock.VerifyNoOtherCalls();
    }

    private async Task<IRequestExecutor> InitializeExecutor()
    {
        var services = new ServiceCollection();

        WuH.Ruby.Common.ProjectTemplate.ServiceCollectionExtensions.AddAuthentication(services);

        var userSettingsService = new UserSettingsService(_settingsServiceMock.Object);
        var dashboardSettingsService = new DashboardSettingsService(_settingsServiceMock.Object);

        return await services
            .AddSingleton<IUserSettingsService>(userSettingsService)
            .AddSingleton<IDashboardSettingsService>(dashboardSettingsService)
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
            .AddType<UserSettingsMutation>()
            .AddSorting()
            .AddFiltering()
            .BuildRequestExecutorAsync();
    }

    private static string UpdateLanguageMutation(string langTag)
    {
        return $@"mutation 
        {{
            userSettingsChangeLanguage(input: 
                {{
                    languageTag: ""{langTag}""
                }}) 
            {{
                languageTag
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

    private static string UpdateUnitSystemMutation(string? unit)
    {
        return @"mutation {
            userSettingsChangeUnitSystem(input: {unitRepresentation: " + (unit ?? "null") + @"}) {
                unitRepresentation
                errors {
            __typename... on Error {
                message
                      }
                }
            }
        }";
    }

    private static string UpdateFavoriteDashboardMutation(MachineDepartment department, string dashboardId)
    {
        return $@"mutation {{
            userSettingsChangeFavoriteDashboard{department}(
                input: {{ dashboardId: ""{dashboardId}"" }}
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

    private static string UpdateMachineDepartment(string? machineDepartment)
    {
        return @"mutation {
            userSettingsChangeMachineDepartment(input: {machineDepartment: " + (machineDepartment ?? "null") + @"}) {
                machineDepartment
                errors {
            __typename... on Error {
                message
                      }
                }
            }
        }";
    }

    private static string UpdatePrintingMachineFamilyMutation(string? printingMachineFamily)
    {
        return @"mutation {
            userSettingsChangePrintingMachineFamily(input: {printingMachineFamily: " + (printingMachineFamily ?? "null") + @"}) {
                printingMachineFamily
                errors {
            __typename... on Error {
                message
                      }
                }
            }
        }";
    }

    private static string UpdateExtrusionMachineFamilyMutation(string? extrusionMachineFamily)
    {
        return @"mutation {
            userSettingsChangeExtrusionMachineFamily(input: {extrusionMachineFamily: " + (extrusionMachineFamily ?? "null") + @"}) {
                extrusionMachineFamily
                errors {
            __typename... on Error {
                message
                      }
                }
            }
        }";
    }

    private static string UpdatePaperSackMachineFamilyMutation(string? paperSackMachineFamily)
    {
        return @"mutation {
            userSettingsChangePaperSackMachineFamily(input: {paperSackMachineFamily: " + (paperSackMachineFamily ?? "null") + @"}) {
                paperSackMachineFamily
                errors {
            __typename... on Error {
                message
                      }
                }
            }
        }";
    }

    private static string UpdateOtherMachineFamilyMutation(string? otherMachineFamily)
    {
        return @"mutation {
            userSettingsChangeOtherMachineFamily(input: {otherMachineFamily: " + (otherMachineFamily ?? "null") + @"}) {
                otherMachineFamily
                errors {
            __typename... on Error {
                message
                      }
                }
            }
        }";
    }
}