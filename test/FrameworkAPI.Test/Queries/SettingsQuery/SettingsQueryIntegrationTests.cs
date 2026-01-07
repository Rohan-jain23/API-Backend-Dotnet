using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.DataLoaders;
using FrameworkAPI.Interceptors;
using FrameworkAPI.Models.Settings;
using FrameworkAPI.Schema.Misc;
using FrameworkAPI.Services.Settings;
using FrameworkAPI.Test.Services.Helpers;
using FrameworkAPI.Test.TestHelpers;
using HotChocolate;
using HotChocolate.Execution;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Snapshooter.Xunit;
using WuH.Ruby.Common.Core;
using WuH.Ruby.Settings.Client;
using Xunit;

namespace FrameworkAPI.Test.Queries.SettingsQuery;

public class SettingsQueryIntegrationTests
{
    private readonly Mock<ISettingsService> _settingsServiceMock = new();

    [Fact]
    public async Task GetUserSettings_Should_Return_The_Correct_Response()
    {
        // Arrange
        const string userId = "test-user-id";

        var executor = await InitializeExecutor();

        _settingsServiceMock
            .Setup(mock => mock.GetSettingsForUserAndMachine(
                null,
                userId,
                UserSettingIds.Language,
                It.IsAny<CancellationToken>(),
                It.IsAny<bool>()))
            .ReturnsAsync(new InternalItemResponse<Setting>(new Setting(UserSettingIds.Language, "de-DE")));
        _settingsServiceMock
            .Setup(mock => mock.GetSettingsForUserAndMachine(
                null,
                userId,
                UserSettingIds.IsUnitRepresentationInSi,
                It.IsAny<CancellationToken>(),
                It.IsAny<bool>()))
            .ReturnsAsync(new InternalItemResponse<Setting>(
                new Setting(UserSettingIds.IsUnitRepresentationInSi, bool.TrueString)));
        _settingsServiceMock
            .Setup(mock => mock.GetSettingsForUserAndMachine(
                null,
                userId,
                UserSettingIds.SelectedMachineDepartment,
                It.IsAny<CancellationToken>(),
                It.IsAny<bool>()))
            .ReturnsAsync(new InternalItemResponse<Setting>(
                new Setting(UserSettingIds.SelectedMachineDepartment, MachineDepartment.Printing.ToString())));
        _settingsServiceMock
            .Setup(mock => mock.GetSettingsForUserAndMachine(
                null,
                userId,
                UserSettingIds.SelectedPrintingMachineFamily,
                It.IsAny<CancellationToken>(),
                It.IsAny<bool>()))
            .ReturnsAsync(new InternalItemResponse<Setting>(
                new Setting(UserSettingIds.SelectedPrintingMachineFamily, MachineFamily.FlexoPrint.ToString())));
        _settingsServiceMock
            .Setup(mock => mock.GetSettingsForUserAndMachine(
                null,
                userId,
                UserSettingIds.SelectedExtrusionMachineFamily,
                It.IsAny<CancellationToken>(),
                It.IsAny<bool>()))
            .ReturnsAsync(new InternalItemResponse<Setting>(
                new Setting(UserSettingIds.SelectedExtrusionMachineFamily, MachineFamily.BlowFilm.ToString())));
        _settingsServiceMock
            .Setup(mock => mock.GetSettingsForUserAndMachine(
                null,
                userId,
                UserSettingIds.SelectedPaperSackMachineFamily,
                It.IsAny<CancellationToken>(),
                It.IsAny<bool>()))
            .ReturnsAsync(new InternalItemResponse<Setting>(
                StatusCodes.Status204NoContent, "No content"));
        _settingsServiceMock
            .Setup(mock => mock.GetSettingsForUserAndMachine(
                null,
                userId,
                UserSettingIds.SelectedOtherMachineFamily,
                It.IsAny<CancellationToken>(),
                It.IsAny<bool>()))
            .ReturnsAsync(new InternalItemResponse<Setting>(
                new Setting(UserSettingIds.SelectedExtrusionMachineFamily, MachineFamily.Other.ToString())));

        const string query =
            @"{
                 userSettings {
                   languageTag
                   unitRepresentation
                   selectedMachineDepartment
                   selectedExtrusionMachineFamily
                   selectedOtherMachineFamily
                   selectedPaperSackMachineFamily
                   selectedPrintingMachineFamily
                 }
              }";

        var request = QueryRequestBuilder
            .New()
            .SetQuery(query)
            .AddRoleClaims("go-general")
            .AddGlobalState("userId", userId)
            .Create();

        // Act
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        _settingsServiceMock.VerifyAll();
        _settingsServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetUserSettings_Returns_An_Error_If_The_UserId_Is_Not_Valid()
    {
        // Arrange
        var executor = await InitializeExecutor();

        const string query =
            @"{
                 userSettings {
                   languageTag
                   unitRepresentation
                   selectedMachineDepartment
                   selectedExtrusionMachineFamily
                   selectedOtherMachineFamily
                   selectedPaperSackMachineFamily
                   selectedPrintingMachineFamily
                 }
              }";

        var request = QueryRequestBuilder
            .New()
            .SetQuery(query)
            .AddRoleClaims("go-general")
            .AddGlobalState("userId", " ")
            .Create();

        // Act
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        _settingsServiceMock.VerifyAll();
        _settingsServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetGlobalSettings_Should_Return_The_Correct_Response()
    {
        // Arrange
        var executor = await InitializeExecutor();

        var globalSettings = new Dictionary<string, string>
        {
            { GlobalSettingIds.IsUserBehaviorTrackingEnabled, bool.FalseString },
            { GlobalSettingIds.IsRubyCloudEnabled, bool.TrueString },
            { GlobalSettingIds.RubyTimeZoneInfoIpAddressWithPort, "ntp1.ptb.de" },
            { GlobalSettingIds.RubyFriendlyName, "My friendly ruby" },
            { GlobalSettingIds.RubyTimeZoneInfoTimeZone, "Pacific/Rarotonga" },
            { GlobalSettingIds.RubyTimeZoneInfoDayLightSavingTime, bool.TrueString },
            { GlobalSettingIds.UserBehaviorTrackingUrl, "https://tracking/" }
        };

        var response = new InternalListResponse<Setting>(
            globalSettings.Select(setting => new Setting(setting.Key, setting.Value)).ToList());
        _settingsServiceMock.Setup(m =>
                m.GetGlobalSettings(
                    null,
                    It.Is<List<string>>(settingIdList =>
                        settingIdList.All(id => globalSettings.ContainsKey(id))),
                    It.IsAny<CancellationToken>()
                    ))
            .ReturnsAsync(response);

        const string query =
            @"{
                 globalSettings {
                   isUserBehaviorTrackingEnabled
                   isRubyCloudEnabled
                   rubyTimeZoneIpAddressWithPort
                   rubyFriendlyName
                   rubyTimeZone
                   rubyTimeZoneIsDayLightSavingTime
                   userBehaviorTrackingUrl
                }
              }";

        var request = QueryRequestBuilder
            .New()
            .SetQuery(query)
            .AddRoleClaims("go-general")
            .AddGlobalState("userId", "test-user-id")
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
        var globalSettingsService = new GlobalSettingsService(_settingsServiceMock.Object);

        var delayedBatchScheduler = new DelayedBatchScheduler();
        var userSettingsBatchLoader = new UserSettingsBatchLoader(_settingsServiceMock.Object, delayedBatchScheduler);
        var globalSettingsBatchLoader =
            new GlobalSettingsBatchLoader(_settingsServiceMock.Object, delayedBatchScheduler);

        return await services
            .AddSingleton(userSettingsBatchLoader)
            .AddSingleton(globalSettingsBatchLoader)
            .AddSingleton<IGlobalSettingsService>(globalSettingsService)
            .AddSingleton<IUserSettingsService>(userSettingsService)
            .AddSingleton(new Mock<ILogger<DefaultAuthorizationService>>().Object)
            .AddAuthorization()
            .AddHttpContextAccessor()
            .AddGraphQLServer()
            .AddHttpRequestInterceptor<HttpRequestInterceptor>()
            .ModifyOptions(t => t.StrictValidation = false)
            .AddAuthorization()
            .AddQueryType(q => q.Name("Query"))
            .AddType<FrameworkAPI.Queries.SettingsQuery>()
            .AddSorting()
            .AddFiltering()
            .BuildRequestExecutorAsync();
    }
}