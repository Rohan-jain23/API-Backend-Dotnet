using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.Models.Settings;
using FrameworkAPI.Mutations;
using FrameworkAPI.Services.Settings;
using FrameworkAPI.Test.TestHelpers;
using HotChocolate;
using HotChocolate.Execution;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Snapshooter.Xunit;
using WuH.Ruby.Common.Core;
using WuH.Ruby.Settings.Client;
using Xunit;

namespace FrameworkAPI.Test.Mutations;

public class GlobalSettingsMutationIntegrationTests
{
    private readonly Mock<ISettingsService> _settingsServiceMock = new();

    [Fact]
    public async Task Update_Ruby_Friendly_Name_Calls_Client_And_Returns_Value()
    {
        // Arrange
        const string rubyFriendlyName = "My friendly ruby";

        var executor = await InitializeExecutor();

        _settingsServiceMock
            .Setup(mock => mock.PostGlobalSettings(
                null,
                GlobalSettingIds.RubyFriendlyName,
                rubyFriendlyName,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalResponse());

        var query = UpdateRubyFriendlyNameMutation(rubyFriendlyName);

        var request = QueryRequestBuilder
            .New()
            .SetQuery(query)
            .AddRoleClaims("admin")
            .Create();

        // Act
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        _settingsServiceMock.VerifyAll();
        _settingsServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Update_Null_Ruby_Friendly_Name_Calls_Client_And_Returns_Value()
    {
        // Arrange
        var executor = await InitializeExecutor();

        _settingsServiceMock
            .Setup(mock => mock.PostGlobalSettings(
                null,
                GlobalSettingIds.RubyFriendlyName,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalResponse());

        var query = UpdateRubyFriendlyNameMutation(null);

        var request = QueryRequestBuilder
            .New()
            .SetQuery(query)
            .AddRoleClaims("admin")
            .Create();

        // Act
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        _settingsServiceMock.VerifyAll();
        _settingsServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Update_Time_Zone_Calls_Client_And_Returns_Value()
    {
        // Arrange
        const string timeZone = "US/Eastern";

        var executor = await InitializeExecutor();

        _settingsServiceMock
            .Setup(mock => mock.PostGlobalSettings(
                null,
                GlobalSettingIds.RubyTimeZoneInfoTimeZone,
                timeZone,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalResponse());

        var query = UpdateTimeZoneMutation(timeZone);

        var request = QueryRequestBuilder
            .New()
            .SetQuery(query)
            .AddRoleClaims("admin")
            .Create();

        // Act
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        _settingsServiceMock.VerifyAll();
        _settingsServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Update_Time_Zone_With_Invalid_Value_Returns_Error()
    {
        // Arrange

        var executor = await InitializeExecutor();

        var query = UpdateTimeZoneMutation(null);

        var request = QueryRequestBuilder
            .New()
            .SetQuery(query)
            .AddRoleClaims("admin")
            .Create();

        // Act
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        _settingsServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Update_Time_Zone_Ip_Address_With_Port_Calls_Client_And_Returns_Value()
    {
        // Arrange
        const string timeZoneIpAddressWithPort = "ntp1.ptb.de";

        var executor = await InitializeExecutor();

        _settingsServiceMock
            .Setup(mock => mock.PostGlobalSettings(
                null,
                GlobalSettingIds.RubyTimeZoneInfoIpAddressWithPort,
                timeZoneIpAddressWithPort,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalResponse());

        var query = UpdateTimeZoneIpAddressWithPortMutation(timeZoneIpAddressWithPort);

        var request = QueryRequestBuilder
            .New()
            .SetQuery(query)
            .AddRoleClaims("admin")
            .Create();

        // Act
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        _settingsServiceMock.VerifyAll();
        _settingsServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Update_Null_Time_Zone_Ip_Address_With_Port_Calls_Client_And_Returns_Value()
    {
        // Arrange
        var executor = await InitializeExecutor();

        _settingsServiceMock
            .Setup(mock => mock.PostGlobalSettings(
                null,
                GlobalSettingIds.RubyTimeZoneInfoIpAddressWithPort,
                null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalResponse());

        var query = UpdateTimeZoneIpAddressWithPortMutation(null);

        var request = QueryRequestBuilder
            .New()
            .SetQuery(query)
            .AddRoleClaims("admin")
            .Create();

        // Act
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        _settingsServiceMock.VerifyAll();
        _settingsServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Update_Cloud_Enabled_Calls_Client_And_Returns_Value()
    {
        // Arrange
        const bool cloudEnabled = true;
        var cloudEnabledAsString = cloudEnabled.ToString();

        var executor = await InitializeExecutor();

        _settingsServiceMock
            .Setup(mock => mock.PostGlobalSettings(
                null,
                GlobalSettingIds.IsRubyCloudEnabled,
                cloudEnabledAsString,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalResponse());

        var query = UpdateRubyCloudEnabledMutation(cloudEnabled);

        var request = QueryRequestBuilder
            .New()
            .SetQuery(query)
            .AddRoleClaims("admin")
            .Create();

        // Act
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        _settingsServiceMock.VerifyAll();
        _settingsServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Update_Cloud_Enabled_With_Invalid_Value_Returns_Error()
    {
        // Arrange
        var executor = await InitializeExecutor();

        var query = UpdateRubyCloudEnabledMutation(null);

        var request = QueryRequestBuilder
            .New()
            .SetQuery(query)
            .AddRoleClaims("admin")
            .Create();

        // Act
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        _settingsServiceMock.VerifyAll();
        _settingsServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Update_User_Behavior_Tracking_Enabled_Calls_Client_And_Returns_Value()
    {
        // Arrange
        const bool userBehaviorTrackingEnabled = true;
        var userBehaviorTrackingEnabledAsString = userBehaviorTrackingEnabled.ToString();

        var executor = await InitializeExecutor();

        _settingsServiceMock
            .Setup(mock => mock.PostGlobalSettings(
                null,
                GlobalSettingIds.IsUserBehaviorTrackingEnabled,
                userBehaviorTrackingEnabledAsString,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalResponse());

        var query = UpdateUserBehaviorTrackingEnabledMutation(userBehaviorTrackingEnabled);

        var request = QueryRequestBuilder
            .New()
            .SetQuery(query)
            .AddRoleClaims("go-general")
            .Create();

        // Act
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        _settingsServiceMock.VerifyAll();
        _settingsServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Update_User_Behavior_Tracking_Enabled_With_Invalid_Value_Returns_Error()
    {
        // Arrange
        var executor = await InitializeExecutor();

        var query = UpdateUserBehaviorTrackingEnabledMutation(null);

        var request = QueryRequestBuilder
            .New()
            .SetQuery(query)
            .AddRoleClaims("go-general")
            .Create();

        // Act
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        _settingsServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Update_User_Behavior_Tracking_Url_Calls_Client_And_Returns_Value()
    {
        // Arrange
        const string userBehaviorTrackingUrl = "http://tracking";

        var executor = await InitializeExecutor();

        _settingsServiceMock
            .Setup(mock => mock.PostGlobalSettings(
                null,
                GlobalSettingIds.UserBehaviorTrackingUrl,
                userBehaviorTrackingUrl,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalResponse());

        var query = UpdateUserBehaviorTrackingUrlMutation(userBehaviorTrackingUrl);

        var request = QueryRequestBuilder
            .New()
            .SetQuery(query)
            .AddRoleClaims("go-general")
            .Create();

        // Act
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        _settingsServiceMock.VerifyAll();
        _settingsServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Update_Null_User_Behavior_Tracking_Url_Calls_Client_And_Returns_Value()
    {
        // Arrange
        var executor = await InitializeExecutor();

        _settingsServiceMock
            .Setup(mock => mock.PostGlobalSettings(
                null,
                GlobalSettingIds.UserBehaviorTrackingUrl,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalResponse());

        var query = UpdateUserBehaviorTrackingUrlMutation(null);

        var request = QueryRequestBuilder
            .New()
            .SetQuery(query)
            .AddRoleClaims("go-general")
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
        var globalSettingsService = new GlobalSettingsService(_settingsServiceMock.Object);

        return await new ServiceCollection()
            .AddSingleton<IGlobalSettingsService>(globalSettingsService)
            .AddSingleton(_settingsServiceMock.Object)
            .AddLogging()
            .AddHttpContextAccessor()
            .AddGraphQLServer()
            .AddDefaultTransactionScopeHandler()
            .AddMutationConventions()
            .AddAuthorization()
            .ModifyOptions(t => t.StrictValidation = false)
            .AddMutationType(q => q.Name("Mutation"))
            .AddType<GlobalSettingsMutation>()
            .AddSorting()
            .AddFiltering()
            .BuildRequestExecutorAsync();
    }

    private static string UpdateRubyFriendlyNameMutation(string? rubyFriendlyName)
    {
        var mutationValue = rubyFriendlyName is null ? "null" : "\"" + rubyFriendlyName + "\"";

        return $@"mutation 
        {{
            globalSettingsChangeRubyFriendlyName(input: 
                {{
                    rubyFriendlyName: {mutationValue}
                }}) 
            {{
                rubyFriendlyName
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

    private static string UpdateTimeZoneMutation(string? timeZone)
    {
        var mutationValue = timeZone is null ? "null" : "\"" + timeZone + "\"";

        return $@"mutation 
        {{
            globalSettingsChangeTimeZone(input: 
                {{
                    timeZone: {mutationValue}
                }}) 
            {{
                timeZone
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

    private static string UpdateTimeZoneIpAddressWithPortMutation(string? timeZoneIoAddressWithPort)
    {
        var mutationValue = timeZoneIoAddressWithPort is null ? "null" : "\"" + timeZoneIoAddressWithPort + "\"";

        return $@"mutation 
        {{
            globalSettingsChangeTimeZoneIpAddressWithPort(input: 
                {{
                    timeZoneIoAddressWithPort: {mutationValue}
                }}) 
            {{
                timeZoneIoAddressWithPort
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

    private static string UpdateRubyCloudEnabledMutation(bool? enabled)
    {
        return $@"mutation 
        {{
            globalSettingsChangeRubyCloudEnabled(input: 
                {{
                    cloudEnabled: {enabled?.ToString().ToLower() ?? "null"}
                }}) 
            {{
                cloudEnabled
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

    private static string UpdateUserBehaviorTrackingEnabledMutation(bool? enabled)
    {
        return $@"mutation 
        {{
            globalSettingsChangeRubyUserBehaviorTrackingEnabled(input: 
                {{
                    userBehaviorTrackingEnabled: {enabled?.ToString().ToLower() ?? "null"}
                }}) 
            {{
                userBehaviorTrackingEnabled
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

    private static string UpdateUserBehaviorTrackingUrlMutation(string? userBehaviorTrackingUrl)
    {
        var mutationValue = userBehaviorTrackingUrl is null ? "null" : "\"" + userBehaviorTrackingUrl + "\"";

        return $@"mutation 
        {{
            globalSettingsChangeUserBehaviorTrackingUrl(input: 
                {{
                    userBehaviorTrackingUrl: {mutationValue}
                }}) 
            {{
                userBehaviorTrackingUrl
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
}