using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FrameworkAPI.DataLoaders;
using FrameworkAPI.Exceptions;
using FrameworkAPI.Schema.Misc;
using FrameworkAPI.Services.Settings;
using FrameworkAPI.Test.Services.Helpers;
using Microsoft.AspNetCore.Http;
using Moq;
using WuH.Ruby.Common.Core;
using WuH.Ruby.Settings.Client;
using Xunit;

namespace FrameworkAPI.Test.Services;

public class UserSettingsServiceTests
{
    private const string UserId = "user12345";

    private readonly Mock<ISettingsService> _settingsHttpClientMock;
    private readonly UserSettingsService _subject;
    private readonly UserSettingsBatchLoader _userSettingsBatchLoader;

    public UserSettingsServiceTests()
    {
        _settingsHttpClientMock = new Mock<ISettingsService>();
        _subject = new UserSettingsService(_settingsHttpClientMock.Object);

        var delayedBatchScheduler = new DelayedBatchScheduler();
        _userSettingsBatchLoader =
            new UserSettingsBatchLoader(_settingsHttpClientMock.Object, delayedBatchScheduler);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("EQ12345")]
    public async Task GetString_Returns_Value_From_Response(string? machineId)
    {
        // Arrange
        const string settingId = "StringSettingId";
        const string value = "My ruby value";

        _settingsHttpClientMock
            .Setup(mock => mock.GetSettingsForUserAndMachine(
                machineId,
                UserId,
                settingId,
                It.IsAny<CancellationToken>(),
                It.IsAny<bool>()))
            .ReturnsAsync(new InternalItemResponse<Setting>(new Setting(settingId, value)));

        // Act
        var result = await _subject.GetString(_userSettingsBatchLoader, UserId, machineId, settingId);

        // Assert
        result.Should().Be(value);

        _settingsHttpClientMock.VerifyAll();
        _settingsHttpClientMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("EQ12345")]
    public async Task GetString_Returns_Null_If_Response_Has_No_Content(string? machineId)
    {
        // Arrange
        const string settingId = "StringSettingId";

        _settingsHttpClientMock
            .Setup(mock => mock.GetSettingsForUserAndMachine(
                machineId,
                UserId,
                settingId,
                It.IsAny<CancellationToken>(),
                It.IsAny<bool>()))
            .ReturnsAsync(
                new InternalItemResponse<Setting>(
                    new InternalError(StatusCodes.Status204NoContent, "No content")));

        // Act
        var result = await _subject.GetString(_userSettingsBatchLoader, UserId, machineId, settingId);

        // Assert
        result.Should().BeNull();

        _settingsHttpClientMock.VerifyAll();
        _settingsHttpClientMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("EQ12345")]
    public async Task GetString_Returns_Fallback_If_Response_Has_No_Content(string? machineId)
    {
        // Arrange
        const string fallback = "fallbackValue";
        const string settingId = "StringSettingId";

        _settingsHttpClientMock
            .Setup(mock => mock.GetSettingsForUserAndMachine(
                machineId,
                UserId,
                settingId,
                It.IsAny<CancellationToken>(),
                It.IsAny<bool>()))
            .ReturnsAsync(
                new InternalItemResponse<Setting>(
                    new InternalError(StatusCodes.Status204NoContent, "No content")));

        // Act
        var result =
            await _subject.GetString(_userSettingsBatchLoader, UserId, machineId, settingId, fallbackValue: fallback);

        // Assert
        result.Should().Be(fallback);

        _settingsHttpClientMock.VerifyAll();
        _settingsHttpClientMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("EQ12345")]
    public async Task GetString_Throws_Exception_If_Request_Failed(string? machineId)
    {
        // Arrange
        const string settingId = "StringSettingId";

        _settingsHttpClientMock
            .Setup(mock => mock.GetSettingsForUserAndMachine(
                machineId,
                UserId,
                settingId,
                It.IsAny<CancellationToken>(),
                It.IsAny<bool>()))
            .ReturnsAsync(
                new InternalItemResponse<Setting>(
                    new InternalError(StatusCodes.Status500InternalServerError, "Internal error")));

        // Act
        var getStringAction = () => _subject.GetString(_userSettingsBatchLoader, UserId, machineId, settingId);

        // Assert
        await getStringAction.Should().ThrowAsync<InternalServiceException>();

        _settingsHttpClientMock.VerifyAll();
        _settingsHttpClientMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task GetString_With_Invalid_MachineId_Throws_Exception(string machineId)
    {
        // Arrange
        const string settingId = "StringSettingId";

        // Act
        var getStringAction = () => _subject.GetString(_userSettingsBatchLoader, UserId, machineId, settingId);

        // Assert
        await getStringAction.Should().ThrowAsync<ArgumentException>();

        _settingsHttpClientMock.VerifyAll();
        _settingsHttpClientMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("EQ12345")]
    public async Task GetAndParse_Returns_Value_From_Response(string? machineId)
    {
        // Arrange
        const string settingId = "SettingToParse";
        const string responseValue = "test123";
        const string parsedValue = "test456";

        _settingsHttpClientMock
            .Setup(mock => mock.GetSettingsForUserAndMachine(
                machineId,
                UserId,
                settingId,
                It.IsAny<CancellationToken>(),
                It.IsAny<bool>()))
            .ReturnsAsync(new InternalItemResponse<Setting>(new Setting(settingId, responseValue)));

        string? valueToParse = null;

        Func<string, string> parseFunc = value =>
        {
            valueToParse = value;
            return parsedValue;
        };

        // Act
        var result =
            await _subject.GetAndParse(_userSettingsBatchLoader, UserId, machineId, settingId, parseFunc);

        // Assert
        result.Should().Be(parsedValue);
        valueToParse.Should().Be(responseValue);

        _settingsHttpClientMock.VerifyAll();
        _settingsHttpClientMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("EQ12345")]
    public async Task GetAndParse_Returns_Null_If_Response_Has_No_Content(string? machineId)
    {
        // Arrange
        const string settingId = "SettingToParse";

        _settingsHttpClientMock
            .Setup(mock => mock.GetSettingsForUserAndMachine(
                machineId,
                UserId,
                settingId,
                It.IsAny<CancellationToken>(),
                It.IsAny<bool>()))
            .ReturnsAsync(
                new InternalItemResponse<Setting>(
                    new InternalError(StatusCodes.Status204NoContent, "No content")));

        var parsedFuncCalled = false;

        Func<string, string> parseFunc = value =>
        {
            parsedFuncCalled = true;
            return value;
        };

        // Act
        var result = await _subject.GetAndParse(
            _userSettingsBatchLoader, UserId, machineId, settingId, parseFunc);

        // Assert
        result.Should().BeNull();
        parsedFuncCalled.Should().BeFalse();

        _settingsHttpClientMock.VerifyAll();
        _settingsHttpClientMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("EQ12345")]
    public async Task GetAndParse_Returns_Fallback_If_Response_Has_No_Content(string? machineId)
    {
        // Arrange
        const MachineDepartment fallbackValue = MachineDepartment.Printing;
        const string settingId = "SettingToParse";

        _settingsHttpClientMock
            .Setup(mock => mock.GetSettingsForUserAndMachine(
                machineId,
                UserId,
                settingId,
                It.IsAny<CancellationToken>(),
                It.IsAny<bool>()))
            .ReturnsAsync(
                new InternalItemResponse<Setting>(
                    new InternalError(StatusCodes.Status204NoContent, "No content")));

        var parsedFuncCalled = false;

        Func<string, string> parseFunc = value =>
        {
            parsedFuncCalled = true;
            return value;
        };

        // Act
        var result = await _subject.GetAndParse(
            _userSettingsBatchLoader, UserId, machineId, settingId, parseFunc, fallbackValue);

        // Assert
        result.Should().Be(fallbackValue);
        parsedFuncCalled.Should().BeFalse();

        _settingsHttpClientMock.VerifyAll();
        _settingsHttpClientMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task GetAndParse_With_Invalid_MachineId_Throws_Exception(string machineId)
    {
        // Arrange
        const string settingId = "SettingId";

        // Act
        var getAndParseAction = () => _subject.GetAndParse(
            _userSettingsBatchLoader, UserId, machineId, settingId, value => value);

        // Assert
        await getAndParseAction.Should().ThrowAsync<ArgumentException>();

        _settingsHttpClientMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("EQ12345", null)]
    [InlineData(null, "testdata")]
    [InlineData("EQ12345", "testdata")]
    public async Task Change_With_Valid_SettingId_Calls_Client_With_Value(string? machineId, string? value)
    {
        // Arrange
        const string userId = "user12345";
        const string settingId = "SettingId";

        _settingsHttpClientMock
            .Setup(mock => mock.PostSettingsForUserAndMachine(
                machineId,
                userId,
                settingId,
                value,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalResponse());

        // Act
        var result = await _subject.Change(userId, machineId, settingId, value);

        // Assert
        result.Should().Be(value);

        _settingsHttpClientMock.VerifyAll();
        _settingsHttpClientMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("EQ12345", null)]
    [InlineData(null, "")]
    [InlineData("EQ12345", "")]
    [InlineData(null, " ")]
    [InlineData("EQ12345", " ")]
    public async Task Change_With_Invalid_SettingId_Throws_Exception(string? machineId, string? settingId)
    {
        // Arrange
        const string userId = "user12345";
        const string value = "testdata";

        // Act
        var changeAction = () => _subject.Change(userId, machineId, settingId!, value);

        // Assert
        await changeAction.Should().ThrowAsync<ArgumentException>();

        _settingsHttpClientMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("EQ12345", null)]
    [InlineData(null, "")]
    [InlineData("EQ12345", "")]
    [InlineData(null, " ")]
    [InlineData("EQ12345", " ")]
    public async Task Change_With_Invalid_UserId_Throws_Exception(string? machineId, string? userId)
    {
        // Arrange
        const string settingId = "SettingId";
        const string value = "testdata";

        // Act
        var changeAction = () => _subject.Change(userId!, machineId, settingId, value);

        // Assert
        await changeAction.Should().ThrowAsync<UserIdNotFoundException>();

        _settingsHttpClientMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("EQ12345")]
    public async Task Change_Request_With_Valid_Data_Failed_Throws_Exception(string? machineId)
    {
        // Arrange
        const string userId = "user12345";
        const string settingId = "SettingId";
        const string value = "testdata";

        _settingsHttpClientMock
            .Setup(mock => mock.PostSettingsForUserAndMachine(
                machineId,
                userId,
                settingId,
                value,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalResponse(new InternalError(500, "Internal error")));

        // Act
        var changeAction = () => _subject.Change(userId, machineId, settingId, value);

        // Assert
        await changeAction.Should().ThrowAsync<InternalServiceException>();

        _settingsHttpClientMock.VerifyAll();
        _settingsHttpClientMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Change_With_Invalid_MachineId_Throws_Exception(string machineId)
    {
        // Arrange
        const string userId = "user12345";
        const string settingId = "SettingId";
        const string value = "testdata";

        // Act
        var changeAction = () => _subject.Change(userId, machineId, settingId, value, CancellationToken.None);

        // Assert
        await changeAction.Should().ThrowAsync<ArgumentException>();

        _settingsHttpClientMock.VerifyNoOtherCalls();
    }
}