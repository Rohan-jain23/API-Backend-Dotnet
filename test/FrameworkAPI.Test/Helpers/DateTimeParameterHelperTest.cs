using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FrameworkAPI.Helpers;
using FrameworkAPI.Models;
using FrameworkAPI.Services.Interfaces;
using Moq;
using Xunit;

namespace FrameworkAPI.Test.Helpers;

public class DateTimeParameterHelperTest
{
    private readonly Mock<IMachineTimeService> _machineTimeServiceMock = new();
    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    [Fact]
    public async Task GetValidTimeForRequest_Returns_DateTime_When_Input_is_Invalid()
    {
        // Arrange
        var invalidDateTimeValue = DateTime.MinValue;
        var validResponse = new DateTime(2024, 12, 31, 14, 00, 17);
        _machineTimeServiceMock
            .Setup(m => m.Get("FakeMachineId", _cancellationToken))
            .ReturnsAsync(new DataResult<DateTime?>(validResponse, exception: null));

        // Act
        var result = await DateTimeParameterHelper.GetValidTimeForRequest(_machineTimeServiceMock.Object,
            invalidDateTimeValue,
            "FakeMachineId",
            _cancellationToken);

        // Assert
        result.Should().Be(validResponse);
    }

    [Fact]
    public async Task GetValidTimeForRequest_Returns_DateTime_When_Input_is_Null()
    {
        // Arrange
        DateTime? invalidDateTimeValue = null;
        var validResponse = new DateTime(2024, 12, 31, 14, 00, 17);
        _machineTimeServiceMock
            .Setup(m => m.Get("FakeMachineId", _cancellationToken))
            .ReturnsAsync(new DataResult<DateTime?>(validResponse, exception: null));

        // Act
        var result = await DateTimeParameterHelper.GetValidTimeForRequest(_machineTimeServiceMock.Object,
            invalidDateTimeValue,
            "FakeMachineId",
            _cancellationToken);

        // Assert
        result.Should().Be(validResponse);
    }

    [Fact]
    public async Task GetValidTimeForRequest_Returns_DateTime_When_Input_is_Valid()
    {
        // Arrange
        var validResponse = new DateTime(2024, 12, 31, 14, 00, 17);
        _machineTimeServiceMock
            .Setup(m => m.Get("FakeMachineId", _cancellationToken))
            .ReturnsAsync(new DataResult<DateTime?>(validResponse, exception: null));

        // Act
        var result = await DateTimeParameterHelper.GetValidTimeForRequest(_machineTimeServiceMock.Object,
            validResponse,
            "FakeMachineId",
            _cancellationToken);

        // Assert
        result.Should().Be(validResponse);
    }

    [Fact]
    public async Task GetValidTimeForRequest_Throws_Exception_When_MachineTimeService_Returns_Exception()
    {
        // Arrange
        DateTime? invalidDateTimeValue = null;
        _machineTimeServiceMock
            .Setup(m => m.Get("FakeMachineId", _cancellationToken))
            .ReturnsAsync(new DataResult<DateTime?>(null, exception: new Exception()));

        //Act + Assert
        await Assert.ThrowsAsync<Exception>(async () =>
            await DateTimeParameterHelper.GetValidTimeForRequest(
                _machineTimeServiceMock.Object,
                invalidDateTimeValue,
                "FakeMachineId",
                It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task GetValidTimeForRequest_Returns_DateTime_When_MachineTimeService_Returns_Null()
    {
        // Arrange
        var validResponse = DateTime.MinValue;
        _machineTimeServiceMock
            .Setup(m => m.Get("FakeMachineId", _cancellationToken))
            .ReturnsAsync(new DataResult<DateTime?>(null, exception: null));

        // Act
        var result = await DateTimeParameterHelper.GetValidTimeForRequest(_machineTimeServiceMock.Object,
            validResponse,
           "FakeMachineId",
            _cancellationToken);

        // Assert
        result.Should().BeWithin(TimeSpan.FromSeconds(20));
    }
}