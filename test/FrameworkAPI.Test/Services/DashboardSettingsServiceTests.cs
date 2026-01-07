using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using FrameworkAPI.Exceptions;
using FrameworkAPI.Schema.Misc;
using FrameworkAPI.Schema.Settings.DashboardSettings;
using FrameworkAPI.Services.Settings;
using Microsoft.AspNetCore.Http;
using Moq;
using WuH.Ruby.Common.Core;
using WuH.Ruby.Settings.Client;
using Xunit;
using SettingsModels = WuH.Ruby.Settings.Client.Models;

namespace FrameworkAPI.Test.Services;

public class DashboardSettingsServiceTests : IDisposable
{
    private readonly CancellationToken _ct = CancellationToken.None;
    private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());
    private readonly Mock<ISettingsService> _settingsHttpClientMock = new();
    private readonly DashboardSettingsService _subject;

    public DashboardSettingsServiceTests()
    {
        _subject = new(_settingsHttpClientMock.Object);

        _fixture.Customize<SettingsModels.DashboardSettings>(x => x.With(m => m.Department, _fixture.Create<MachineDepartment>().ToString()));
    }

    public void Dispose()
    {
        _settingsHttpClientMock.VerifyAll();
        _settingsHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateDashboard_With_Id_In_Request_Should_Throw_Error()
    {
        // Arrange
        var userId = _fixture.Create<string>();
        var dashboardId = _fixture.Create<string>();
        var request = _fixture.Build<CreateOrEditConfiguredDashboardRequest>()
                              .With(m => m.DashboardId, dashboardId)
                              .Create();

        // Act
        var action = () => _subject.CreateDashboard(userId, request, _ct);

        // Assert
        await action.Should().ThrowAsync<ParameterInvalidException>().WithMessage("*DashboardId*");
    }

    [Fact]
    public async Task CreateDashboard_With_Friendly_Name_Already_Exists_Should_Return_Success()
    {
        // Arrange
        var userId = _fixture.Create<string>();
        var friendlyName = _fixture.Create<string>();
        var request = _fixture.Build<CreateOrEditConfiguredDashboardRequest>()
                              .Without(m => m.DashboardId)
                              .With(m => m.FriendlyName, friendlyName)
                              .Create();
        var dashboard = _fixture.Build<SettingsModels.DashboardSettings>()
                                .With(m => m.Department, _fixture.Create<MachineDepartment>().ToString())
                                .With(m => m.IsPublic, true)
                                .With(m => m.FriendlyName, friendlyName)
                                .Create();

        _settingsHttpClientMock
            .Setup(mock => mock.AddOrUpdateDashboardSettings(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                true,
                It.IsAny<string>(),
                It.IsAny<SettingsModels.DashboardWidgetSettings>(),
                It.IsAny<SettingsModels.DashboardWidgetSettings>(),
                It.IsAny<SettingsModels.DashboardWidgetSettings>(),
                It.IsAny<SettingsModels.DashboardWidgetSettings>(),
                It.IsAny<SettingsModels.DashboardWidgetSettings>(),
                It.IsAny<SettingsModels.DashboardWidgetSettings>(),
                _ct))
            .ReturnsAsync(new InternalItemResponse<SettingsModels.DashboardSettings>(dashboard));

        // Act
        var result = await _subject.CreateDashboard(userId, request, _ct);

        // Assert
        result.Should().BeEquivalentTo(new DashboardSettings(dashboard));
    }

    [Fact]
    public async Task CreateDashboard_Returns_Success()
    {
        // Arrange
        var userId = _fixture.Create<string>();
        var dashboardId = _fixture.Create<string>();
        var friendlyName = _fixture.Create<string>();
        var request = _fixture.Build<CreateOrEditConfiguredDashboardRequest>()
                              .Without(m => m.DashboardId)
                              .With(m => m.FriendlyName, friendlyName)
                              .Create();
        var dashboard = _fixture.Build<SettingsModels.DashboardSettings>()
                                .With(m => m.DashboardId, dashboardId)
                                .With(m => m.Department, _fixture.Create<MachineDepartment>().ToString())
                                .With(m => m.FriendlyName, friendlyName)
                                .Create();

        _settingsHttpClientMock
            .Setup(mock => mock.AddOrUpdateDashboardSettings(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                true,
                It.IsAny<string>(),
                It.IsAny<SettingsModels.DashboardWidgetSettings>(),
                It.IsAny<SettingsModels.DashboardWidgetSettings>(),
                It.IsAny<SettingsModels.DashboardWidgetSettings>(),
                It.IsAny<SettingsModels.DashboardWidgetSettings>(),
                It.IsAny<SettingsModels.DashboardWidgetSettings>(),
                It.IsAny<SettingsModels.DashboardWidgetSettings>(),
                _ct))
            .ReturnsAsync(new InternalItemResponse<SettingsModels.DashboardSettings>(dashboard));

        // Act
        var result = await _subject.CreateDashboard(userId, request, _ct);

        // Assert
        result.Should().BeEquivalentTo(new DashboardSettings(dashboard));
    }

    [Theory]
    [InlineData("")]
    [InlineData("    ")]
    public async Task EditDashboard_With_Invalid_UserId_Should_Throw_Error(string userId)
    {
        // Arrange
        var request = _fixture.Create<CreateOrEditConfiguredDashboardRequest>();

        // Act
        var action = () => _subject.EditDashboard(userId, request, _ct);

        // Assert
        await action.Should().ThrowAsync<UserIdNotFoundException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("    ")]
    public async Task EditDashboard_With_Invalid_DashboardId_Should_Throw_Error(string dashboardId)
    {
        // Arrange
        var userId = _fixture.Create<string>();
        var request = _fixture.Build<CreateOrEditConfiguredDashboardRequest>()
                              .With(m => m.DashboardId, dashboardId)
                              .Create();

        // Act
        var action = () => _subject.EditDashboard(userId, request, _ct);

        // Assert
        await action.Should().ThrowAsync<ParameterInvalidException>();
    }

    [Fact]
    public async Task EditDashboard_With_Client_Returning_Empty_Should_Throw_Error()
    {
        // Arrange
        var userId = _fixture.Create<string>();
        var request = _fixture.Create<CreateOrEditConfiguredDashboardRequest>();

        _settingsHttpClientMock
            .Setup(mock => mock.GetDashboardSettingsById(request.DashboardId, _ct))
            .ReturnsAsync(new InternalItemResponse<SettingsModels.DashboardSettings>(StatusCodes.Status204NoContent, "Empty"));

        // Act
        var action = () => _subject.EditDashboard(userId, request, _ct);

        // Assert
        await action.Should().ThrowAsync<IdNotFoundException>();
    }

    [Fact]
    public async Task EditDashboard_With_Dashboard_Not_Editable_Should_Throw_Error()
    {
        // Arrange
        var userId = _fixture.Create<string>();
        var dashboardId = _fixture.Create<string>();
        var request = _fixture.Build<CreateOrEditConfiguredDashboardRequest>()
                              .With(m => m.DashboardId, dashboardId)
                              .Create();
        var dashboard = _fixture.Build<SettingsModels.DashboardSettings>()
                                .With(m => m.DashboardId, dashboardId)
                                .With(m => m.Department, _fixture.Create<MachineDepartment>().ToString())
                                .With(m => m.IsPublic, true)
                                .With(m => m.CanOnlyBeEditedByCreator, true)
                                .Create();

        _settingsHttpClientMock
            .Setup(mock => mock.GetDashboardSettingsById(dashboardId, _ct))
            .ReturnsAsync(new InternalItemResponse<SettingsModels.DashboardSettings>(dashboard));

        // Act
        var action = () => _subject.EditDashboard(userId, request, _ct);

        // Assert
        await action.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task EditDashboard_With_Changed_FriendlyName_Already_Existing_Should_Return_Success()
    {
        // Arrange
        var userId = _fixture.Create<string>();
        var dashboardId = _fixture.Create<string>();
        var friendlyName = _fixture.Create<string>();
        var request = _fixture.Build<CreateOrEditConfiguredDashboardRequest>()
                              .With(m => m.DashboardId, dashboardId)
                              .With(m => m.FriendlyName, friendlyName)
                              .Create();
        var dashboard = _fixture.Build<SettingsModels.DashboardSettings>()
                                .With(m => m.DashboardId, dashboardId)
                                .With(m => m.Department, _fixture.Create<MachineDepartment>().ToString())
                                .With(m => m.IsPublic, true)
                                .With(m => m.CanOnlyBeEditedByCreator, false)
                                .Create();

        var dashboard2 = _fixture.Build<SettingsModels.DashboardSettings>()
                                .With(m => m.Department, _fixture.Create<MachineDepartment>().ToString())
                                .With(m => m.FriendlyName, friendlyName)
                                .With(m => m.IsPublic, true)
                                .Create();

        _settingsHttpClientMock
            .Setup(mock => mock.GetDashboardSettingsById(dashboardId, _ct))
            .ReturnsAsync(new InternalItemResponse<SettingsModels.DashboardSettings>(dashboard));

        _settingsHttpClientMock
            .Setup(mock => mock.AddOrUpdateDashboardSettings(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                true,
                It.IsAny<string>(),
                It.IsAny<SettingsModels.DashboardWidgetSettings>(),
                It.IsAny<SettingsModels.DashboardWidgetSettings>(),
                It.IsAny<SettingsModels.DashboardWidgetSettings>(),
                It.IsAny<SettingsModels.DashboardWidgetSettings>(),
                It.IsAny<SettingsModels.DashboardWidgetSettings>(),
                It.IsAny<SettingsModels.DashboardWidgetSettings>(),
                _ct))
            .ReturnsAsync(new InternalItemResponse<SettingsModels.DashboardSettings>(dashboard));

        // Act
        var result = await _subject.EditDashboard(userId, request, _ct);

        // Assert
        result.Should().BeEquivalentTo(new DashboardSettings(dashboard));
    }

    [Fact]
    public async Task EditDashboard_With_Client_Retuning_Error_On_Update_Should_Throw_Error()
    {
        // Arrange
        var userId = _fixture.Create<string>();
        var dashboardId = _fixture.Create<string>();
        var request = _fixture.Build<CreateOrEditConfiguredDashboardRequest>()
                              .With(m => m.DashboardId, dashboardId)
                              .Create();
        var dashboard = _fixture.Build<SettingsModels.DashboardSettings>()
                                .With(m => m.DashboardId, dashboardId)
                                .With(m => m.Department, _fixture.Create<MachineDepartment>().ToString())
                                .With(m => m.CreatorUserId, userId)
                                .Create();

        _settingsHttpClientMock
            .Setup(mock => mock.GetDashboardSettingsById(dashboardId, _ct))
            .ReturnsAsync(new InternalItemResponse<SettingsModels.DashboardSettings>(dashboard));

        _settingsHttpClientMock
            .Setup(mock => mock.AddOrUpdateDashboardSettings(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                true,
                It.IsAny<string>(),
                It.IsAny<SettingsModels.DashboardWidgetSettings>(),
                It.IsAny<SettingsModels.DashboardWidgetSettings>(),
                It.IsAny<SettingsModels.DashboardWidgetSettings>(),
                It.IsAny<SettingsModels.DashboardWidgetSettings>(),
                It.IsAny<SettingsModels.DashboardWidgetSettings>(),
                It.IsAny<SettingsModels.DashboardWidgetSettings>(),
                _ct))
            .ReturnsAsync(new InternalItemResponse<SettingsModels.DashboardSettings>(StatusCodes.Status500InternalServerError, "Error"));

        // Act
        var action = () => _subject.EditDashboard(userId, request, _ct);

        // Assert
        await action.Should().ThrowAsync<InternalServiceException>();
    }

    [Fact]
    public async Task EditDashboard_Returns_Success()
    {
        // Arrange
        var userId = _fixture.Create<string>();
        var dashboardId = _fixture.Create<string>();
        var request = _fixture.Build<CreateOrEditConfiguredDashboardRequest>()
                              .With(m => m.DashboardId, dashboardId)
                              .Create();
        var dashboard = _fixture.Build<SettingsModels.DashboardSettings>()
                              .With(m => m.DashboardId, dashboardId)
                                .With(m => m.Department, _fixture.Create<MachineDepartment>().ToString())
                                .With(m => m.CreatorUserId, userId)
                                .Create();

        _settingsHttpClientMock
            .Setup(mock => mock.GetDashboardSettingsById(dashboardId, _ct))
            .ReturnsAsync(new InternalItemResponse<SettingsModels.DashboardSettings>(dashboard));

        _settingsHttpClientMock
            .Setup(mock => mock.AddOrUpdateDashboardSettings(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                true,
                It.IsAny<string>(),
                It.IsAny<SettingsModels.DashboardWidgetSettings>(),
                It.IsAny<SettingsModels.DashboardWidgetSettings>(),
                It.IsAny<SettingsModels.DashboardWidgetSettings>(),
                It.IsAny<SettingsModels.DashboardWidgetSettings>(),
                It.IsAny<SettingsModels.DashboardWidgetSettings>(),
                It.IsAny<SettingsModels.DashboardWidgetSettings>(),
                _ct))
            .ReturnsAsync(new InternalItemResponse<SettingsModels.DashboardSettings>(dashboard));

        // Act
        var result = await _subject.EditDashboard(userId, request, _ct);

        // Assert
        result.Should().BeEquivalentTo(new DashboardSettings(dashboard));
    }

    [Theory]
    [InlineData("")]
    [InlineData("    ")]
    public async Task DeleteDashboard_With_Invalid_UserId_Should_Throw_Error(string userId)
    {
        // Arrange
        var dashboardId = _fixture.Create<string>();

        // Act
        var action = () => _subject.DeleteDashboard(userId, dashboardId, _ct);

        // Assert
        await action.Should().ThrowAsync<UserIdNotFoundException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("    ")]
    public async Task DeleteDashboard_With_Invalid_DashboardId_Should_Throw_Error(string dashboardId)
    {
        // Arrange
        var userId = _fixture.Create<string>();

        // Act
        var action = () => _subject.DeleteDashboard(userId, dashboardId, _ct);

        // Assert
        await action.Should().ThrowAsync<ParameterInvalidException>();
    }

    [Fact]
    public async Task DeleteDashboard_With_Client_Returning_Empty_Should_Throw_Error()
    {
        // Arrange
        var userId = _fixture.Create<string>();
        var dashboardId = _fixture.Create<string>();

        _settingsHttpClientMock
            .Setup(mock => mock.GetAllDashboardSettings(_ct))
            .ReturnsAsync(new InternalListResponse<SettingsModels.DashboardSettings>(StatusCodes.Status204NoContent, "Empty"));

        // Act
        var action = () => _subject.DeleteDashboard(userId, dashboardId, _ct);

        // Assert
        var exception = await action.Should().ThrowAsync<IdNotFoundException>().WithMessage("*does not exist*");
    }

    [Fact]
    public async Task DeleteDashboard_With_Dashboard_Deletion_By_Other_User_Should_Throw_Error()
    {
        // Arrange
        var userId = "UserA";
        var dashboardId = _fixture.Create<string>();
        var dashboard = _fixture.Build<SettingsModels.DashboardSettings>()
                                .With(m => m.DashboardId, dashboardId)
                                .With(m => m.CreatorUserId, "UserB")
                                .With(m => m.Department, _fixture.Create<MachineDepartment>().ToString())
                                .With(m => m.CanOnlyBeEditedByCreator, false)
                                .Create();

        _settingsHttpClientMock
            .Setup(mock => mock.GetAllDashboardSettings(_ct))
            .ReturnsAsync(new InternalListResponse<SettingsModels.DashboardSettings>([dashboard]));

        // Act
        var action = () => _subject.DeleteDashboard(userId, dashboardId, _ct);

        // Assert
        var exception = await action.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task DeleteDashboard_With_Last_Dashboard_Should_Throw_Error()
    {
        // Arrange
        var userId = _fixture.Create<string>();
        var dashboardId = _fixture.Create<string>();
        var dashboard = _fixture.Build<SettingsModels.DashboardSettings>()
                                .With(m => m.DashboardId, dashboardId)
                                .With(m => m.Department, _fixture.Create<MachineDepartment>().ToString())
                                .With(m => m.CreatorUserId, userId)
                                .Create();

        _settingsHttpClientMock
            .Setup(mock => mock.GetAllDashboardSettings(_ct))
            .ReturnsAsync(new InternalListResponse<SettingsModels.DashboardSettings>([dashboard]));

        // Act
        var action = () => _subject.DeleteDashboard(userId, dashboardId, _ct);

        // Assert
        var exception = await action.Should().ThrowAsync<InternalServiceException>().WithMessage("*last dashboard*");
    }

    [Fact]
    public async Task DeleteDashboard_With_One_Own_Dashboard_And_One_Public_Dashboard_Should_Throw_Error()
    {
        // Arrange
        var userId = _fixture.Create<string>();
        var dashboardId = _fixture.Create<string>();
        var privateDashboard = _fixture.Build<SettingsModels.DashboardSettings>()
                                .With(m => m.DashboardId, dashboardId)
                                .With(m => m.Department, _fixture.Create<MachineDepartment>().ToString())
                                .With(m => m.CreatorUserId, userId)
                                .Create();
        var publicDashboard = _fixture.Build<SettingsModels.DashboardSettings>()
                                .With(m => m.Department, _fixture.Create<MachineDepartment>().ToString())
                                .Create();

        _settingsHttpClientMock
            .Setup(mock => mock.GetAllDashboardSettings(_ct))
            .ReturnsAsync(new InternalListResponse<SettingsModels.DashboardSettings>([privateDashboard, publicDashboard]));

        // Act
        var action = () => _subject.DeleteDashboard(userId, dashboardId, _ct);

        // Assert
        var exception = await action.Should().ThrowAsync<InternalServiceException>().WithMessage("*last dashboard*");
    }

    [Fact]
    public async Task DeleteDashboard_With_Client_Returning_Error_On_Delete_Should_Throw_Error()
    {
        // Arrange
        var userId = _fixture.Create<string>();
        var dashboards = _fixture.Build<SettingsModels.DashboardSettings>()
                                .With(m => m.Department, _fixture.Create<MachineDepartment>().ToString())
                                .With(m => m.CreatorUserId, userId)
                                .CreateMany(2)
                                .ToList();
        var dashboardId = dashboards[0].DashboardId;

        _settingsHttpClientMock
            .Setup(mock => mock.GetAllDashboardSettings(_ct))
            .ReturnsAsync(new InternalListResponse<SettingsModels.DashboardSettings>(dashboards));

        _settingsHttpClientMock
            .Setup(mock => mock.DeleteDashboardSettingsById(dashboardId, _ct))
            .ReturnsAsync(new InternalResponse(StatusCodes.Status500InternalServerError, "Error"));

        // Act
        var action = () => _subject.DeleteDashboard(userId, dashboardId, _ct);

        // Assert
        await action.Should().ThrowAsync<InternalServiceException>().WithMessage("Error");
    }

    [Fact]
    public async Task DeleteDashboard_Should_Return_Success()
    {
        // Arrange
        var userId = _fixture.Create<string>();
        var dashboards = _fixture.Build<SettingsModels.DashboardSettings>()
                                .With(m => m.Department, _fixture.Create<MachineDepartment>().ToString())
                                .With(m => m.CreatorUserId, userId)
                                .CreateMany(2)
                                .ToList();
        var dashboardId = dashboards[0].DashboardId;

        _settingsHttpClientMock
            .Setup(mock => mock.GetAllDashboardSettings(_ct))
            .ReturnsAsync(new InternalListResponse<SettingsModels.DashboardSettings>(dashboards));

        _settingsHttpClientMock
            .Setup(mock => mock.DeleteDashboardSettingsById(dashboardId, _ct))
            .ReturnsAsync(new InternalResponse());

        // Act
        var result = await _subject.DeleteDashboard(userId, dashboardId, _ct);

        // Assert
        result.Should().Be(dashboardId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("    ")]
    public async Task GetDashboardSettingsById_With_Invalid_Dashboard_Id_Should_Throw_Error(string dashboardId)
    {
        // Act
        var action = () => _subject.GetDashboardSettingsById(dashboardId, _ct);

        // Assert
        await action.Should().ThrowAsync<ParameterInvalidException>();
    }

    [Fact]
    public async Task GetDashboardSettingsById_With_Client_Returning_Empty_Should_Throw_Error()
    {
        // Arrange
        var dashboardId = _fixture.Create<string>();

        _settingsHttpClientMock
            .Setup(mock => mock.GetDashboardSettingsById(dashboardId, _ct))
            .ReturnsAsync(new InternalItemResponse<SettingsModels.DashboardSettings>(StatusCodes.Status204NoContent, "Empty"));

        // Act
        var action = () => _subject.GetDashboardSettingsById(dashboardId, _ct);

        // Assert
        await action.Should().ThrowAsync<IdNotFoundException>();
    }

    [Fact]
    public async Task GetDashboardSettingsById_With_Client_Returning_Error_Should_Throw_Error()
    {
        // Arrange
        var dashboardId = _fixture.Create<string>();

        _settingsHttpClientMock
            .Setup(mock => mock.GetDashboardSettingsById(dashboardId, _ct))
            .ReturnsAsync(new InternalItemResponse<SettingsModels.DashboardSettings>(StatusCodes.Status500InternalServerError, "Error"));

        // Act
        var action = () => _subject.GetDashboardSettingsById(dashboardId, _ct);

        // Assert
        await action.Should().ThrowAsync<InternalServiceException>();
    }

    [Fact]
    public async Task GetDashboardSettingsById_Should_Return_DashboardSettings()
    {
        // Arrange
        var dashboardId = _fixture.Create<string>();
        var dashboard = _fixture.Create<SettingsModels.DashboardSettings>();

        _settingsHttpClientMock
            .Setup(mock => mock.GetDashboardSettingsById(dashboardId, _ct))
            .ReturnsAsync(new InternalItemResponse<SettingsModels.DashboardSettings>(dashboard));

        // Act
        var result = await _subject.GetDashboardSettingsById(dashboardId, _ct);

        // Assert
        result.Should().BeEquivalentTo(new DashboardSettings(dashboard));
    }

    [Theory]
    [InlineData("")]
    [InlineData("    ")]
    public async Task GetDashboardSettingsByIdForUser_With_Invalid_UserId_Should_Throw_Error(string userId)
    {
        // Arrange
        var dashboardId = _fixture.Create<string>();

        // Act
        var action = () => _subject.GetDashboardSettingsByIdForUser(userId, dashboardId, _ct);

        // Assert
        await action.Should().ThrowAsync<UserIdNotFoundException>();
    }

    [Fact]
    public async Task GetDashboardSettingsByIdForUser_With_Private_Dashboard_Of_Other_User_Should_Throw_Error()
    {
        // Arrange
        var userId = _fixture.Create<string>();
        var dashboardId = _fixture.Create<string>();
        var dashboard = _fixture.Build<SettingsModels.DashboardSettings>()
                                .With(m => m.Department, _fixture.Create<MachineDepartment>().ToString())
                                .With(m => m.IsPublic, false)
                                .Create();

        _settingsHttpClientMock
            .Setup(mock => mock.GetDashboardSettingsById(dashboardId, _ct))
            .ReturnsAsync(new InternalItemResponse<SettingsModels.DashboardSettings>(dashboard));

        // Act
        var action = () => _subject.GetDashboardSettingsByIdForUser(userId, dashboardId, _ct);

        // Assert
        await action.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task GetDashboardSettingsByIdForUser_With_Private_Dashboard_Of_Same_User_Should_Return_Success()
    {
        // Arrange
        var userId = _fixture.Create<string>();
        var dashboardId = _fixture.Create<string>();
        var dashboard = _fixture.Build<SettingsModels.DashboardSettings>()
                                .With(m => m.Department, _fixture.Create<MachineDepartment>().ToString())
                                .With(m => m.IsPublic, false)
                                .With(m => m.CreatorUserId, userId)
                                .Create();

        _settingsHttpClientMock
            .Setup(mock => mock.GetDashboardSettingsById(dashboardId, _ct))
            .ReturnsAsync(new InternalItemResponse<SettingsModels.DashboardSettings>(dashboard));

        // Act
        var result = await _subject.GetDashboardSettingsByIdForUser(userId, dashboardId, _ct);

        // Assert
        result.Should().BeEquivalentTo(new DashboardSettings(dashboard));
    }

    [Fact]
    public async Task GetDashboardSettingsByIdForUser_With_Public_Dashboard_Should_Return_Success()
    {
        // Arrange
        var userId = _fixture.Create<string>();
        var dashboardId = _fixture.Create<string>();
        var dashboard = _fixture.Build<SettingsModels.DashboardSettings>()
                                .With(m => m.Department, _fixture.Create<MachineDepartment>().ToString())
                                .With(m => m.IsPublic, true)
                                .Create();

        _settingsHttpClientMock
            .Setup(mock => mock.GetDashboardSettingsById(dashboardId, _ct))
            .ReturnsAsync(new InternalItemResponse<SettingsModels.DashboardSettings>(dashboard));

        // Act
        var result = await _subject.GetDashboardSettingsByIdForUser(userId, dashboardId, _ct);

        // Assert
        result.Should().BeEquivalentTo(new DashboardSettings(dashboard));
    }

    [Theory]
    [InlineData("")]
    [InlineData("    ")]
    public async Task GetDashboardSettingsForUser_With_Invalid_UserId_Should_Throw_Error(string userId)
    {
        // Act
        var action = () => _subject.GetDashboardSettingsForUser(userId, machineDepartmentFilter: null, _ct);

        // Assert
        await action.Should().ThrowAsync<UserIdNotFoundException>();
    }

    [Fact]
    public async Task GetDashboardSettingsForUser_Should_Return_Dashboards_For_User()
    {
        // Arrange
        var userId = _fixture.Create<string>();
        var clientResponse = _fixture.Build<SettingsModels.DashboardSettings>()
                                     .With(m => m.Department, _fixture.Create<MachineDepartment>().ToString())
                                     .With(m => m.IsPublic, false)
                                     .CreateMany(10)
                                     .ToList();

        clientResponse[0].CreatorUserId = userId;
        clientResponse[1].IsPublic = true;
        clientResponse[8].CreatorUserId = userId;

        _settingsHttpClientMock
            .Setup(mock => mock.GetAllDashboardSettings(_ct))
            .ReturnsAsync(new InternalListResponse<SettingsModels.DashboardSettings>(clientResponse));

        // Act
        var result = await _subject.GetDashboardSettingsForUser(userId, machineDepartmentFilter: null, _ct);

        // Assert
        result.Should().HaveCount(3);
        result[0].Should().BeEquivalentTo(new DashboardSettings(clientResponse[0]));
        result[1].Should().BeEquivalentTo(new DashboardSettings(clientResponse[1]));
        result[2].Should().BeEquivalentTo(new DashboardSettings(clientResponse[8]));
    }

    [Theory]
    [InlineData("")]
    [InlineData("    ")]
    public async Task AddOrUpdate_With_Invalid_UserId_Should_Throw_Error(string userId)
    {
        // Arrange
        var request = _fixture.Create<CreateOrEditConfiguredDashboardRequest>();

        // Act
        var action = () => _subject.AddOrUpdate(userId, request, _ct);

        // Assert
        await action.Should().ThrowAsync<UserIdNotFoundException>();
    }

    [Fact]
    public async Task AddOrUpdate_With_Client_Returning_Error_Should_Throw_Error()
    {
        // Arrange
        var userId = _fixture.Create<string>();
        var request = _fixture.Create<CreateOrEditConfiguredDashboardRequest>();

        _settingsHttpClientMock
            .Setup(mock => mock.AddOrUpdateDashboardSettings(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                true,
                It.IsAny<string>(),
                It.IsAny<SettingsModels.DashboardWidgetSettings>(),
                It.IsAny<SettingsModels.DashboardWidgetSettings>(),
                It.IsAny<SettingsModels.DashboardWidgetSettings>(),
                It.IsAny<SettingsModels.DashboardWidgetSettings>(),
                It.IsAny<SettingsModels.DashboardWidgetSettings>(),
                It.IsAny<SettingsModels.DashboardWidgetSettings>(),
                _ct))
            .ReturnsAsync(new InternalItemResponse<SettingsModels.DashboardSettings>(StatusCodes.Status500InternalServerError, "Error"));

        // Act
        var action = () => _subject.AddOrUpdate(userId, request, _ct);

        // Assert
        await action.Should().ThrowAsync<InternalServiceException>();
    }

    [Fact]
    public async Task AddOrUpdate_Should_Call_Client_With_Request_Data_And_Return_Response()
    {
        // Arrange
        var userId = _fixture.Create<string>();
        var request = _fixture.Create<CreateOrEditConfiguredDashboardRequest>();
        var clientResponse = _fixture.Create<SettingsModels.DashboardSettings>();
        var expectedResult = new DashboardSettings(clientResponse);

        _settingsHttpClientMock
            .Setup(mock => mock.AddOrUpdateDashboardSettings(
                request.DashboardId,
                request.Department.ToString(),
                userId,
                request.IsPublic,
                true,
                request.FriendlyName,
                It.IsAny<SettingsModels.DashboardWidgetSettings?>(),
                It.IsAny<SettingsModels.DashboardWidgetSettings?>(),
                It.IsAny<SettingsModels.DashboardWidgetSettings?>(),
                It.IsAny<SettingsModels.DashboardWidgetSettings?>(),
                It.IsAny<SettingsModels.DashboardWidgetSettings?>(),
                It.IsAny<SettingsModels.DashboardWidgetSettings?>(),
                _ct))
            .ReturnsAsync(new InternalItemResponse<SettingsModels.DashboardSettings>(clientResponse));

        // Act
        var result = await _subject.AddOrUpdate(userId, request, _ct);

        // Assert
        result.Should().BeEquivalentTo(expectedResult);
    }

    [Fact]
    public async Task GetAllDashboardSettings_With_Client_Returning_Empty_Should_Return_Empty_List()
    {
        // Arrange
        _settingsHttpClientMock
            .Setup(mock => mock.GetAllDashboardSettings(_ct))
            .ReturnsAsync(new InternalListResponse<SettingsModels.DashboardSettings>(StatusCodes.Status204NoContent, "Empty"));

        // Act
        var result = await _subject.GetAllDashboardSettings(machineDepartmentFilter: null, _ct);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllDashboardSettings_With_Client_Returning_Error_Should_Throw_Exception()
    {
        // Arrange
        _settingsHttpClientMock
            .Setup(mock => mock.GetAllDashboardSettings(_ct))
            .ReturnsAsync(new InternalListResponse<SettingsModels.DashboardSettings>(StatusCodes.Status500InternalServerError, "Error"));

        // Act
        var action = () => _subject.GetAllDashboardSettings(machineDepartmentFilter: null, _ct);

        // Assert
        await action.Should().ThrowAsync<InternalServiceException>();
    }

    [Fact]
    public async Task GetAllDashboardSettings_With_No_Filter_Should_Return_All_Dashboards()
    {
        // Arrange
        var dashboardSettings = _fixture.CreateMany<SettingsModels.DashboardSettings>(10).ToList();
        var expectedDashboardSettings = dashboardSettings.Select(dashboard => new DashboardSettings(dashboard)).ToList();

        _settingsHttpClientMock
            .Setup(mock => mock.GetAllDashboardSettings(_ct))
            .ReturnsAsync(new InternalListResponse<SettingsModels.DashboardSettings>(dashboardSettings));

        // Act
        var result = await _subject.GetAllDashboardSettings(machineDepartmentFilter: null, _ct);

        // Assert
        result.Should().BeEquivalentTo(expectedDashboardSettings);
    }

    [Fact]
    public async Task GetAllDashboardSettings_With_Filter_Should_Return_All_Matching_Dashboards()
    {
        // Arrange
        var filter = MachineDepartment.Extrusion;
        var dashboardSettings = _fixture.CreateMany<SettingsModels.DashboardSettings>(10).ToList();
        var expectedDashboardSettings = dashboardSettings.Select(dashboard => new DashboardSettings(dashboard))
                                                         .Where(dashboard => dashboard.Department == MachineDepartment.Extrusion)
                                                         .ToList();

        _settingsHttpClientMock
            .Setup(mock => mock.GetAllDashboardSettings(_ct))
            .ReturnsAsync(new InternalListResponse<SettingsModels.DashboardSettings>(dashboardSettings));

        // Act
        var result = await _subject.GetAllDashboardSettings(filter, _ct);

        // Assert
        result.Should().BeEquivalentTo(expectedDashboardSettings);
    }
}