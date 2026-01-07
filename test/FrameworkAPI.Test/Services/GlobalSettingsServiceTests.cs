using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FrameworkAPI.DataLoaders;
using FrameworkAPI.Exceptions;
using FrameworkAPI.Services.Settings;
using FrameworkAPI.Test.Services.Helpers;
using Moq;
using WuH.Ruby.Common.Core;
using WuH.Ruby.Settings.Client;
using Xunit;

namespace FrameworkAPI.Test.Services;

public class GlobalSettingsServiceTests
{
    private readonly Mock<ISettingsService> _settingsHttpClientMock;
    private readonly GlobalSettingsService _subject;
    private readonly GlobalSettingsBatchLoader _globalSettingsBatchLoader;

    public GlobalSettingsServiceTests()
    {
        _settingsHttpClientMock = new Mock<ISettingsService>();
        _subject = new GlobalSettingsService(_settingsHttpClientMock.Object);

        var delayedBatchScheduler = new DelayedBatchScheduler();
        _globalSettingsBatchLoader =
            new GlobalSettingsBatchLoader(_settingsHttpClientMock.Object, delayedBatchScheduler);
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
            .Setup(mock => mock.GetGlobalSettings(
                machineId,
                It.IsAny<List<string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalListResponse<Setting>(new List<Setting>
            {
                new(settingId, value)
            }));

        // Act
        var result = await _subject.GetString(_globalSettingsBatchLoader, machineId, settingId);

        // Assert
        result.Should().Be(value);

        _settingsHttpClientMock.VerifyAll();
        _settingsHttpClientMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("EQ12345")]
    public async Task GetString_Returns_Null_If_SettingId_Not_In_Response(string? machineId)
    {
        // Arrange
        const string settingId = "StringSettingId";

        _settingsHttpClientMock
            .Setup(mock => mock.GetGlobalSettings(
                machineId,
                It.IsAny<List<string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalListResponse<Setting>(new List<Setting>()));

        // Act
        var result = await _subject.GetString(_globalSettingsBatchLoader, machineId, settingId);

        // Assert
        result.Should().BeNull();

        _settingsHttpClientMock.VerifyAll();
        _settingsHttpClientMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("EQ12345")]
    public async Task GetString_Returns_Fallback_If_SettingId_Not_In_Response(string? machineId)
    {
        // Arrange
        const string fallback = "fallback value";
        const string settingId = "StringSettingId";

        _settingsHttpClientMock
            .Setup(mock => mock.GetGlobalSettings(
                machineId,
                It.IsAny<List<string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalListResponse<Setting>(new List<Setting>()));

        // Act
        var result = await _subject.GetString(
            _globalSettingsBatchLoader, machineId, settingId, fallbackValue: fallback);

        // Assert
        result.Should().Be(fallback);

        _settingsHttpClientMock.VerifyAll();
        _settingsHttpClientMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("EQ12345")]
    public async Task GetString_Returns_Null_If_Response_Is_Empty(string? machineId)
    {
        // Arrange
        const string settingId = "StringSettingId";

        _settingsHttpClientMock
            .Setup(mock => mock.GetGlobalSettings(
                machineId,
                It.IsAny<List<string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalListResponse<Setting>(new InternalError(204, "No content")));

        // Act
        var result = await _subject.GetString(_globalSettingsBatchLoader, machineId, settingId);
        // Assert
        result.Should().BeNull();

        _settingsHttpClientMock.VerifyAll();
        _settingsHttpClientMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("EQ12345")]
    public async Task GetString_Returns_Fallback_If_Response_Is_Empty(string? machineId)
    {
        // Arrange
        const string fallback = "fallback value";
        const string settingId = "StringSettingId";

        _settingsHttpClientMock
            .Setup(mock => mock.GetGlobalSettings(
                machineId,
                It.IsAny<List<string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalListResponse<Setting>(new InternalError(204, "No content")));

        // Act
        var result =
            await _subject.GetString(_globalSettingsBatchLoader, machineId, settingId, fallbackValue: fallback);

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
            .Setup(mock => mock.GetGlobalSettings(
                machineId,
                It.IsAny<List<string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalListResponse<Setting>(new InternalError(500, "Internal error")));

        // Act
        var getStringAction = () => _subject.GetString(_globalSettingsBatchLoader, machineId, settingId);

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
        const string settingId = "SettingId";

        // Act
        var getStringAction = () => _subject.GetString(_globalSettingsBatchLoader, machineId, settingId);

        // Assert
        await getStringAction.Should().ThrowAsync<ArgumentException>();

        _settingsHttpClientMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(null, "true", true)]
    [InlineData("EQ12345", "true", true)]
    [InlineData(null, "false", false)]
    [InlineData("EQ12345", "false", false)]
    [InlineData(null, "True", true)]
    [InlineData(null, "False", false)]
    [InlineData(null, "TRUE", true)]
    [InlineData(null, "FALSE", false)]
    public async Task GetBoolean_Returns_Value_From_Response(string? machineId, string responseValue,
        bool expectedValue)
    {
        // Arrange
        const string settingId = "BoolSettingId";

        _settingsHttpClientMock
            .Setup(mock => mock.GetGlobalSettings(
                machineId,
                It.IsAny<List<string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalListResponse<Setting>(new List<Setting>
            {
                new(settingId, responseValue)
            }));

        // Act
        var result = await _subject.GetBoolean(_globalSettingsBatchLoader, machineId, settingId);

        // Assert
        result.Should().Be(expectedValue);

        _settingsHttpClientMock.VerifyAll();
        _settingsHttpClientMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("EQ12345")]
    public async Task GetBoolean_Returns_Null_If_SettingId_Not_In_Response(string? machineId)
    {
        // Arrange
        const string settingId = "BoolSettingId";

        _settingsHttpClientMock
            .Setup(mock => mock.GetGlobalSettings(
                machineId,
                It.IsAny<List<string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalListResponse<Setting>(new List<Setting>()));

        // Act
        var result = await _subject.GetBoolean(_globalSettingsBatchLoader, machineId, settingId);

        // Assert
        result.Should().BeNull();

        _settingsHttpClientMock.VerifyAll();
        _settingsHttpClientMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("EQ12345")]
    public async Task GetBoolean_Returns_Fallback_If_SettingId_Not_In_Response(string? machineId)
    {
        // Arrange
        const bool fallback = true;
        const string settingId = "BoolSettingId";

        _settingsHttpClientMock
            .Setup(mock => mock.GetGlobalSettings(
                machineId,
                It.IsAny<List<string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalListResponse<Setting>(new List<Setting>()));

        // Act
        var result =
            await _subject.GetBoolean(_globalSettingsBatchLoader, machineId, settingId, fallbackValue: fallback);

        // Assert
        result.Should().Be(fallback);

        _settingsHttpClientMock.VerifyAll();
        _settingsHttpClientMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task GetBoolean_With_Invalid_MachineId_Throws_Exception(string machineId)
    {
        // Arrange
        const string settingId = "SettingId";

        // Act
        var getBooleanAction = () => _subject.GetBoolean(_globalSettingsBatchLoader, machineId, settingId);

        // Assert
        await getBooleanAction.Should().ThrowAsync<ArgumentException>();

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
        const string settingId = "SettingId";

        _settingsHttpClientMock
            .Setup(mock => mock.PostGlobalSettings(
                machineId,
                settingId,
                value,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalResponse());

        // Act
        var result = await _subject.Change(machineId, settingId, value);

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
        const string value = "testdata";

        // Act
        var changeAction = () => _subject.Change(machineId, settingId!, value);

        // Assert
        await changeAction.Should().ThrowAsync<ArgumentException>();

        _settingsHttpClientMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Change_With_Invalid_MachineId_Throws_Exception(string machineId)
    {
        // Arrange
        const string settingId = "SettingId";
        const string value = "testdata";

        // Act
        var changeAction = () => _subject.Change(machineId, settingId, value);

        // Assert
        await changeAction.Should().ThrowAsync<ArgumentException>();

        _settingsHttpClientMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("EQ12345")]
    public async Task Change_Request_With_Valid_Data_Failed_Throws_Exception(string? machineId)
    {
        // Arrange
        const string settingId = "SettingId";
        const string value = "testdata";

        _settingsHttpClientMock
            .Setup(mock => mock.PostGlobalSettings(
                machineId,
                settingId,
                value,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalResponse(new InternalError(500, "Internal error")));

        // Act
        var changeAction = () => _subject.Change(machineId, settingId, value);

        // Assert
        await changeAction.Should().ThrowAsync<InternalServiceException>();

        _settingsHttpClientMock.VerifyAll();
        _settingsHttpClientMock.VerifyNoOtherCalls();
    }
}