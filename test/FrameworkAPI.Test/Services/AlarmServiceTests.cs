using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FrameworkAPI.DataLoaders;
using FrameworkAPI.Schema.Misc;
using FrameworkAPI.Services;
using FrameworkAPI.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Moq;
using WuH.Ruby.AlarmDataHandler.Client;
using WuH.Ruby.Common.Core;
using Xunit;

namespace FrameworkAPI.Test.Services;

public class AlarmServiceTests
{
    private readonly Mock<IAlarmDataHandlerCachingService> _alarmDataHandlerCachingServiceMock = new();
    private readonly Mock<IAlarmDataHandlerHttpClient> _alarmDataHandlerHttpClientMock = new();
    private readonly ActiveAlarmsCacheDataLoader _activeAlarmsCacheDataLoader;
    private readonly IAlarmService _alarmService;
    private const string MachineId = "EQ10000";

    public AlarmServiceTests()
    {
        _alarmService = new AlarmService(_alarmDataHandlerHttpClientMock.Object);
        _activeAlarmsCacheDataLoader = new ActiveAlarmsCacheDataLoader(_alarmDataHandlerCachingServiceMock.Object);
    }

    [Fact]
    public async Task AlarmService_Should_Return_Alarms()
    {
        // Arrange
        var from = DateTime.UnixEpoch.AddDays(1);
        var to = DateTime.UnixEpoch.AddDays(4);
        var mocks = GetMockAlarms();
        var responseMock = new InternalListResponse<Alarm>(mocks);

        _alarmDataHandlerHttpClientMock.Setup(mock => mock.GetAlarms(
            It.IsAny<CancellationToken>(),
            MachineId,
            from,
            to,
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<bool>(),
            It.IsAny<string?>(),
            It.IsAny<bool>()
        )).ReturnsAsync(responseMock);

        // Act
        var result = await _alarmService.GetAlarmsByMachineIdAndTime(
            MachineId,
            from,
            to,
            0,
            10,
            true,
            null,
            false,
            "en-US",
            CancellationToken.None);

        //Assert
        result.Should().NotBeNull();
        result!.Count.Should().Be(mocks.Count);
        result.First().Should().BeEquivalentTo(new MachineAlarm(mocks.First(), "en-US"));
        result.Last().Should().BeEquivalentTo(new MachineAlarm(mocks.Last(), "en-US"));
    }

    [Fact]
    public async Task AlarmService_Gets_Response_With_Error()
    {
        // Arrange
        var from = DateTime.UnixEpoch.AddDays(1);
        var to = DateTime.UnixEpoch.AddDays(4);
        var responseMock = new InternalListResponse<Alarm>(200, new Exception("I am a test"));

        _alarmDataHandlerHttpClientMock.Setup(mock => mock.GetAlarms(
            It.IsAny<CancellationToken>(),
            MachineId,
            from,
            to,
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<bool>(),
            It.IsAny<string?>(),
            It.IsAny<bool>()
        )).ReturnsAsync(responseMock);

        // Act
        var action = () => _alarmService.GetAlarmsByMachineIdAndTime(
            MachineId,
            from,
            to,
            0,
            10,
            true,
            null,
            false,
            "en-US",
            CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task AlarmService_Should_Return_Alarm_Count()
    {
        // Arrange
        const long mockCount = 42L;
        var from = DateTime.UnixEpoch.AddDays(1);
        var to = DateTime.UnixEpoch.AddDays(4);
        var responseMock = new InternalItemResponse<long>(mockCount);

        _alarmDataHandlerHttpClientMock.Setup(mock => mock.GetAlarmCount(
            It.IsAny<CancellationToken>(),
            It.IsAny<string>(),
            It.IsAny<DateTime?>(),
            It.IsAny<DateTime?>(),
            It.IsAny<string?>(),
            It.IsAny<bool?>()
        )).ReturnsAsync(responseMock);

        // Act
        var result = await _alarmService.GetAlarmCount(
            MachineId,
            from,
            to,
            null,
            false,
            CancellationToken.None);

        //Assert
        result.Should().NotBeNull();
        result.Should().Be((int)mockCount);
    }

    [Fact]
    public async Task AlarmService_Should_Return_Alarm_Count_But_Gets_Exception_From_Client()
    {
        // Arrange
        var from = DateTime.UnixEpoch.AddDays(1);
        var to = DateTime.UnixEpoch.AddDays(4);

        _alarmDataHandlerHttpClientMock.Setup(mock => mock.GetAlarmCount(
            It.IsAny<CancellationToken>(),
            It.IsAny<string>(),
            It.IsAny<DateTime?>(),
            It.IsAny<DateTime?>(),
            It.IsAny<string?>(),
            It.IsAny<bool?>()
        )).ReturnsAsync(new InternalItemResponse<long>(StatusCodes.Status500InternalServerError, new Exception("Test")));

        // Act
        var action = () => _alarmService.GetAlarmsByMachineIdAndTime(
            MachineId,
            from,
            to,
            0,
            10,
            true,
            null,
            false,
            "en-US",
            CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task AlarmService_Should_Return_Active_Alarms()
    {
        // Arrange
        const int mockCount = 42;
        var responseMock = new InternalListResponse<Alarm>(GetMockAlarms());

        _alarmDataHandlerCachingServiceMock.Setup(mock => mock.GetActiveAlarms(
            MachineId,
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(responseMock);

        // Act
        var result = await _alarmService.GetActiveAlarms(
            _activeAlarmsCacheDataLoader,
            MachineId,
            0,
            mockCount,
            true,
            null,
            "en-US",
            CancellationToken.None);

        // Assert
        result.Should().HaveCount(8);
        result.Should().BeEquivalentTo(GetMockAlarms().Select(externalAlarm => new MachineAlarm(externalAlarm, "en-US")));
    }

    [Fact]
    public async Task AlarmService_Should_Return_Only_One_Active_Alarm()
    {
        // Arrange
        var responseMock = new InternalListResponse<Alarm>(GetMockAlarms());

        _alarmDataHandlerCachingServiceMock.Setup(mock => mock.GetActiveAlarms(
            MachineId,
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(responseMock);

        // Act
        var result = await _alarmService.GetActiveAlarms(
            _activeAlarmsCacheDataLoader,
            MachineId,
            0,
            1,
            true,
            null,
            "en-US",
            CancellationToken.None);

        //Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task AlarmService_Should_Return_Only_Matching_Regex_AlarmCodes()
    {
        // Arrange
        var responseMock = new InternalListResponse<Alarm>(GetMockAlarms());

        _alarmDataHandlerCachingServiceMock.Setup(mock => mock.GetActiveAlarms(
            MachineId,
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(responseMock);

        // Act
        var result = await _alarmService.GetActiveAlarms(
            _activeAlarmsCacheDataLoader,
            MachineId,
            0,
            10,
            true,
            "1",
            "en-US",
            CancellationToken.None);

        //Assert
        result.Should().HaveCount(1);
        result!.First().AlarmCode.Should().Be("60-1");
    }

    [Fact]
    public async Task AlarmService_Should_Return_Filtered_Alarms()
    {
        // Arrange
        var responseMock = new InternalListResponse<Alarm>(GetMockAlarms());

        _alarmDataHandlerCachingServiceMock.Setup(mock => mock.GetActiveAlarms(
            MachineId,
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(responseMock);

        // Act
        var result = await _alarmService.GetActiveAlarmsCount(
            _activeAlarmsCacheDataLoader,
            MachineId,
            "1",
            CancellationToken.None);

        //Assert
        result.Should().Be(1);
    }

    [Fact]
    public async Task AlarmService_Should_Return_Active_Alarms_But_Gets_DataResult_With_Exception()
    {
        // Arrange
        _alarmDataHandlerCachingServiceMock.Setup(mock => mock.GetActiveAlarms(
            MachineId,
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(new InternalListResponse<Alarm>(StatusCodes.Status500InternalServerError, new Exception("I am a test")));

        // Act
        var action = () => _alarmService.GetActiveAlarms(
            _activeAlarmsCacheDataLoader,
            MachineId,
            0,
            2,
            true,
            "6",
            "en-US",
            CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<Exception>();
    }

    private static List<Alarm> GetMockAlarms(int amount = 7)
    {
        var alarmList = new List<Alarm>();
        for (var i = 0; i < amount; i++)
        {
            var endOfAlarm = DateTime.UnixEpoch.AddDays(i);
            alarmList.Add(new Alarm
            {
                Id = i.ToString(),
                AcknowledgeTimestamp = DateTime.UnixEpoch.AddDays(i),
                EndTimestamp = endOfAlarm,
                AlarmCode = $"60-{i}",
                IsPrimal = false
            });
        }
        alarmList.Add(new Alarm
        {
            Id = (amount + 1).ToString(),
            AcknowledgeTimestamp = DateTime.UnixEpoch.AddDays(amount),
            AlarmCode = $"60-{amount}",
            IsPrimal = true
        });
        return alarmList;
    }
}