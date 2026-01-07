using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FrameworkAPI.DataLoaders;
using FrameworkAPI.Exceptions;
using FrameworkAPI.Schema.Misc;
using FrameworkAPI.Services;
using Microsoft.Extensions.Logging;
using Moq;
using WuH.Ruby.Common.Core;
using WuH.Ruby.ProductionPeriodsDataHandler.Client;
using WuH.Ruby.Settings.Client;
using WuH.Ruby.Supervisor.Client;
using Xunit;

namespace FrameworkAPI.Test.Services;

public class MachineShiftServiceTests
{
    private readonly MachineShiftService _subject;
    private readonly Mock<ISupervisorHttpClient> _supervisorHttpClientMock = new();
    private readonly Mock<IShiftSettingsService> _shiftSettingsServiceMock = new();
    private readonly Mock<IProductionPeriodsDataHandlerHttpClient> _productionPeriodsDataHandlerHttpClientMock = new();
    private readonly ProductionPeriodByTimestampCacheDataLoader _productionPeriodByTimestampCacheDataLoader;
    private readonly UserNameCacheDataLoader _userNameCacheDataLoader;

    public MachineShiftServiceTests()
    {
        _subject = new MachineShiftService(
            _shiftSettingsServiceMock.Object,
            new Mock<ILogger<MachineShiftService>>().Object);

        _productionPeriodByTimestampCacheDataLoader = new ProductionPeriodByTimestampCacheDataLoader(
            _productionPeriodsDataHandlerHttpClientMock.Object);

        _userNameCacheDataLoader = new UserNameCacheDataLoader(_supervisorHttpClientMock.Object);
    }

    [Fact]
    public async Task GetMachineShifts_Returns_Null_For_OperatorName_When_Not_Available()
    {
        // Arrange
        MockPeriodResponse(null);
        MockShifts();

        // Act
        var response = await _subject.GetMachineShifts(
            _productionPeriodByTimestampCacheDataLoader,
            _userNameCacheDataLoader,
            "FakeMachineId",
            DateTime.UnixEpoch,
            DateTime.UnixEpoch.AddDays(1),
            CancellationToken.None);

        // Assert
        response.Should().BeEquivalentTo(new List<MachineShift>
        {
            new ("Shift0", DateTime.UnixEpoch.AddHours(0), DateTime.UnixEpoch.AddHours(8), null),
            new ("Shift1", DateTime.UnixEpoch.AddHours(8), DateTime.UnixEpoch.AddHours(16), null),
            new ("Shift2", DateTime.UnixEpoch.AddHours(16), DateTime.UnixEpoch.AddHours(24), null),
        });
    }

    [Fact]
    public async Task GetMachineShifts_Returns_OperatorName_When_Its_Not_A_Guid()
    {
        // Arrange
        MockPeriodResponse(["fakeOperator"]);
        MockShifts();

        // Act
        var response = await _subject.GetMachineShifts(
            _productionPeriodByTimestampCacheDataLoader,
            _userNameCacheDataLoader,
            "FakeMachineId",
            DateTime.UnixEpoch,
            DateTime.UnixEpoch.AddDays(1),
            CancellationToken.None);

        // Assert
        response.Should().BeEquivalentTo(new List<MachineShift>
        {
            new ("Shift0", DateTime.UnixEpoch.AddHours(0), DateTime.UnixEpoch.AddHours(8), "fakeOperator"),
            new ("Shift1", DateTime.UnixEpoch.AddHours(8), DateTime.UnixEpoch.AddHours(16), "fakeOperator"),
            new ("Shift2", DateTime.UnixEpoch.AddHours(16), DateTime.UnixEpoch.AddHours(24), "fakeOperator"),
        });
    }

    [Fact]
    public async Task GetMachineShifts_Returns_OperatorName_When_Its_A_Guid()
    {
        // Arrange
        var guid = Guid.NewGuid();
        MockPeriodResponse([$"{guid}"]);
        MockShifts();

        _supervisorHttpClientMock
            .Setup(x => x.ResolveNames(
                new List<Guid> { guid },
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalListResponse<User>([new User(guid, "ResolvedUsername")]));

        // Act
        var response = await _subject.GetMachineShifts(
            _productionPeriodByTimestampCacheDataLoader,
            _userNameCacheDataLoader,
            "FakeMachineId",
            DateTime.UnixEpoch,
            DateTime.UnixEpoch.AddDays(1),
            CancellationToken.None);

        // Assert
        response.Should().BeEquivalentTo(new List<MachineShift>
        {
            new ("Shift0", DateTime.UnixEpoch.AddHours(0), DateTime.UnixEpoch.AddHours(8), "ResolvedUsername"),
            new ("Shift1", DateTime.UnixEpoch.AddHours(8), DateTime.UnixEpoch.AddHours(16), "ResolvedUsername"),
            new ("Shift2", DateTime.UnixEpoch.AddHours(16), DateTime.UnixEpoch.AddHours(24), "ResolvedUsername"),
        });
    }

    [Fact]
    public async Task GetMachineShifts_Throws_When_OperatorName_Is_A_Guid_But_Supervisor_Has_Error()
    {
        // Arrange
        var guid = Guid.NewGuid();
        MockPeriodResponse([$"{guid}"]);
        MockShifts();

        _supervisorHttpClientMock
            .Setup(x => x.ResolveNames(
                new List<Guid> { guid },
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalListResponse<User>(400, "BadRequest"));

        // Act
        var action = async () => await _subject.GetMachineShifts(
            _productionPeriodByTimestampCacheDataLoader,
            _userNameCacheDataLoader,
            "FakeMachineId",
            DateTime.UnixEpoch,
            DateTime.UnixEpoch.AddDays(1),
            CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<InternalServiceException>();
    }

    [Fact]
    public async Task GetMachineShifts_Throws_InternalServiceException_When_ShiftService_Has_Error()
    {
        // Arrange
        _shiftSettingsServiceMock
            .Setup(mock => mock.GetShiftsInTimeRange(
                It.IsAny<string>(),
                It.IsAny<WuH.Ruby.Common.Core.TimeRange>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<List<ActualShift>>(500, "error"));

        // Act
        var action = async () => await _subject.GetMachineShifts(
            _productionPeriodByTimestampCacheDataLoader,
            _userNameCacheDataLoader,
            "FakeMachineId",
            DateTime.UnixEpoch,
            DateTime.UnixEpoch.AddDays(1),
            CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<InternalServiceException>();
    }

    [Fact]
    public async Task GetMachineShifts_Throws_InternalServiceException_GetPeriodByTimestamp_Has_Error()
    {
        // Arrange
        MockShifts();
        _productionPeriodsDataHandlerHttpClientMock
            .Setup(mock => mock.GetPeriodByTimestamp(
                It.IsAny<CancellationToken>(),
                It.IsAny<string>(),
                It.IsAny<DateTime>()))
            .ReturnsAsync(new InternalItemResponse<ProductionPeriodResponseItem>(500, "error"));

        // Act
        var action = async () => await _subject.GetMachineShifts(
            _productionPeriodByTimestampCacheDataLoader,
            _userNameCacheDataLoader,
            "FakeMachineId",
            DateTime.UnixEpoch,
            DateTime.UnixEpoch.AddDays(1),
            CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<InternalServiceException>();
    }

    private void MockShifts()
    {
        _shiftSettingsServiceMock
            .Setup(mock => mock.GetShiftsInTimeRange(
                It.IsAny<string>(),
                It.IsAny<WuH.Ruby.Common.Core.TimeRange>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((
                string machineId,
                WuH.Ruby.Common.Core.TimeRange timeRange,
                string languageTag,
                CancellationToken _) =>
            {
                var counter = 0;
                var results = new List<ActualShift>();
                for (var time = timeRange.From; time < timeRange.To; time = time.AddHours(8))
                {
                    results.Add(new ActualShift()
                    {
                        DisplayName = $"Shift{counter++}",
                        StartTime = time,
                        EndTime = time.AddHours(8)
                    });
                }
                return new InternalItemResponse<List<ActualShift>>(results);
            });
    }

    private void MockPeriodResponse(List<string>? operators)
    {
        _productionPeriodsDataHandlerHttpClientMock
            .Setup(mock => mock.GetPeriodByTimestamp(
                It.IsAny<CancellationToken>(),
                It.IsAny<string>(),
                It.IsAny<DateTime>()))
            .ReturnsAsync((
                CancellationToken _,
                string machineId,
                DateTime timestamp) =>
            {
                var periodResponse = new ProductionPeriodResponseItem(
                    machineId,
                    timestamp,
                    timestamp.AddHours(1),
                    0.0,
                    null,
                    new ProductionStatusDescription(),
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    operators: operators);
                return new InternalItemResponse<ProductionPeriodResponseItem>(periodResponse);
            });
    }
}