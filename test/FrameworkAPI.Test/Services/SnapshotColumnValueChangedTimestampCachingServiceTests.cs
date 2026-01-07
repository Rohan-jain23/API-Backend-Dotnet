using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FrameworkAPI.Exceptions;
using FrameworkAPI.Services;
using Microsoft.Extensions.Logging;
using Moq;
using WuH.Ruby.Common.Core;
using WuH.Ruby.MachineSnapShooter.Client;
using WuH.Ruby.MachineSnapShooter.Client.Enums;
using WuH.Ruby.MachineSnapShooter.Client.Models;
using WuH.Ruby.MachineSnapShooter.Client.Queue;
using Xunit;

namespace FrameworkAPI.Test.Services;

public class SnapshotColumnValueChangedTimestampCachingServiceTests
{
    private const string MachineId = "EQ12345";
    private const string ColumnId = SnapshotColumnIds.ProductionStatusId;
    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    private readonly Mock<ILatestMachineSnapshotCachingService> _latestMachineSnapshotCachingServiceMock = new();
    private readonly Mock<IMachineSnapshotHttpClient> _machineSnapshotHttpClientMock = new();
    private readonly Mock<IMachineSnapshotQueueWrapper> _machineSnapshotQueueWrapperMock = new();
    private readonly Mock<ILogger<SnapshotColumnValueChangedTimestampCachingService>> _loggerMock = new();
    private readonly SnapshotColumnValueChangedTimestampCachingService _subject;
    private Func<HistoricSnapshotChangeMessage, Task>? _queueCallback;

    public SnapshotColumnValueChangedTimestampCachingServiceTests()
    {
        _machineSnapshotQueueWrapperMock
            .Setup(m => m.SubscribeForHistoricSnapshotChangeMessage(
                It.IsAny<Func<HistoricSnapshotChangeMessage, Task>>()))
            .Callback((Func<HistoricSnapshotChangeMessage, Task> callback) =>
                _queueCallback = callback);

        _subject = new SnapshotColumnValueChangedTimestampCachingService(
            _latestMachineSnapshotCachingServiceMock.Object,
            _machineSnapshotHttpClientMock.Object,
            _machineSnapshotQueueWrapperMock.Object,
            _loggerMock.Object);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Get_With_Invalid_MachineId_Throws_Exception(string? machineId)
    {
        // Arrange
        var getAction = () => _subject.Get(machineId!, ColumnId, _cancellationToken);

        // Act / Assert
        await getAction.Should().ThrowAsync<ArgumentNullException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("Invalid_Column_Id")]
    public async Task Get_With_Invalid_ColumnId_Throws_Exception(string? columnId)
    {
        // Arrange
        var getAction = () => _subject.Get(MachineId, columnId!, _cancellationToken);

        // Act / Assert
        await getAction.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Get_Throws_Exception_When_ColumnValueChangedTimestamp_Request_Has_Error()
    {
        // Arrange
        var error = new InternalError((int)HttpStatusCode.InternalServerError, "Internal Error");
        _machineSnapshotHttpClientMock
            .Setup(m => m.GetLatestSnapshotColumnValueChangedTimestamp(
                MachineId, ColumnId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<SnapshotColumnValueChangedResponse>(error));

        var getAction = () => _subject.Get(MachineId, ColumnId, _cancellationToken);

        // Act / Assert
        await getAction.Should().ThrowAsync<InternalServiceException>();

        _latestMachineSnapshotCachingServiceMock
            .VerifyAdd(m => m.CacheChanged += It.IsAny<EventHandler<LiveSnapshotEventArgs>>(), Times.Once);
        _latestMachineSnapshotCachingServiceMock.VerifyNoOtherCalls();

        _machineSnapshotHttpClientMock.VerifyAll();
        _machineSnapshotHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Get_Returns_Null_When_ColumnValueChangedTimestamp_Request_Has_Error_WaitingForFirstMinutelyTimestamp()
    {
        // Arrange
        var error = new InternalError(
            (int)HttpStatusCode.InternalServerError,
            MachineSnapshotErrorItemType.WaitingForFirstMinutelySnapshot.ToString(),
            MachineSnapshotErrorItemType.WaitingForFirstMinutelySnapshot);
        _machineSnapshotHttpClientMock
            .Setup(m => m.GetLatestSnapshotColumnValueChangedTimestamp(
                MachineId, ColumnId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalItemResponse<SnapshotColumnValueChangedResponse>(error));

        // Act
        var result = await _subject.Get(MachineId, ColumnId, _cancellationToken);

        // Assert
        result.Should().BeNull();

        _latestMachineSnapshotCachingServiceMock
            .VerifyAdd(m => m.CacheChanged += It.IsAny<EventHandler<LiveSnapshotEventArgs>>(), Times.Once);
        _latestMachineSnapshotCachingServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Get_With_Empty_Cache_Will_Fill_Up_Cache()
    {
        // Arrange
        var expectedChangedTimestamp = DateTime.UnixEpoch;
        var snapshotColumnValueChangedResponse = new SnapshotColumnValueChangedResponse(expectedChangedTimestamp, 123, expectedChangedTimestamp.AddMinutes(5));

        _machineSnapshotHttpClientMock
            .Setup(m => m.GetLatestSnapshotColumnValueChangedTimestamp(
                MachineId, ColumnId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new InternalItemResponse<SnapshotColumnValueChangedResponse>(snapshotColumnValueChangedResponse));

        // Act

        // Request data and add it to the cache
        var firstCallResult = await _subject.Get(MachineId, ColumnId, _cancellationToken);

        // Get the value from the cache
        var secondCallResult = await _subject.Get(MachineId, ColumnId, _cancellationToken);

        // Assert
        firstCallResult.Should().Be(expectedChangedTimestamp);
        secondCallResult.Should().Be(expectedChangedTimestamp);

        _latestMachineSnapshotCachingServiceMock
            .VerifyAdd(m => m.CacheChanged += It.IsAny<EventHandler<LiveSnapshotEventArgs>>(), Times.Once);
        _latestMachineSnapshotCachingServiceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Clear_Cache_For_Machine_When_Historic_Snapshot_Change_Received()
    {
        // Arrange initialize
        var expectedChangedTimestamp = DateTime.UnixEpoch;
        var snapshotColumnValueChangedResponse = new SnapshotColumnValueChangedResponse(expectedChangedTimestamp, 123, expectedChangedTimestamp.AddMinutes(5));

        _machineSnapshotHttpClientMock
            .Setup(m => m.GetLatestSnapshotColumnValueChangedTimestamp(
                MachineId, ColumnId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new InternalItemResponse<SnapshotColumnValueChangedResponse>(snapshotColumnValueChangedResponse));

        // Act initialize
        var firstCallResult = await _subject.Get(MachineId, ColumnId, _cancellationToken);

        // Assert initialize
        firstCallResult.Should().Be(expectedChangedTimestamp);

        // Act reset
        await SendMessageToQueue(new HistoricSnapshotChangeMessage(MachineId, "FakeHash", DateTime.MinValue, DateTime.UnixEpoch, DateTime.UnixEpoch, []));
        var secondCallResult = await _subject.Get(MachineId, ColumnId, _cancellationToken);

        // Assert
        secondCallResult.Should().Be(expectedChangedTimestamp);

        _latestMachineSnapshotCachingServiceMock
            .VerifyAdd(m => m.CacheChanged += It.IsAny<EventHandler<LiveSnapshotEventArgs>>(), Times.Once);
        _latestMachineSnapshotCachingServiceMock.VerifyNoOtherCalls();

        _machineSnapshotHttpClientMock.Verify(m =>
            m.GetLatestSnapshotColumnValueChangedTimestamp(MachineId, ColumnId, It.IsAny<CancellationToken>()), Times.Exactly(2));
        _machineSnapshotHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Clear_Cache_For_Machine_When_Historic_Snapshot_Change_Received_And_Ignore_Updates_In_Between()
    {
        // Arrange initialize
        var expectedChangedTimestamp = DateTime.UnixEpoch;
        var snapshotColumnValueChangedResponse = new SnapshotColumnValueChangedResponse(expectedChangedTimestamp, 123, expectedChangedTimestamp.AddMinutes(5));

        _machineSnapshotHttpClientMock
            .Setup(m => m.GetLatestSnapshotColumnValueChangedTimestamp(
                MachineId, ColumnId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new InternalItemResponse<SnapshotColumnValueChangedResponse>(snapshotColumnValueChangedResponse));

        // Act initialize
        var firstCallResult = await _subject.Get(MachineId, ColumnId, _cancellationToken);

        // Assert initialize
        firstCallResult.Should().Be(expectedChangedTimestamp);

        // Act reset
        await SendMessageToQueue(new HistoricSnapshotChangeMessage(MachineId, "FakeHash", DateTime.MinValue, DateTime.UnixEpoch, DateTime.UnixEpoch, []));
        var updatedSnapshotDto = new SnapshotQueueMessageDto(
            [new(ColumnId, 456)],
            DateTime.MinValue,
            "fakeHash",
            DateTime.UnixEpoch.AddHours(1).AddMinutes(10));
        _latestMachineSnapshotCachingServiceMock.Raise(m => m.CacheChanged += null, new LiveSnapshotEventArgs(MachineId, updatedSnapshotDto, isMinutelySnapshot: true));
        var secondCallResult = await _subject.Get(MachineId, ColumnId, _cancellationToken);

        // Assert
        secondCallResult.Should().Be(expectedChangedTimestamp);

        _latestMachineSnapshotCachingServiceMock
            .VerifyAdd(m => m.CacheChanged += It.IsAny<EventHandler<LiveSnapshotEventArgs>>(), Times.Once);
        _latestMachineSnapshotCachingServiceMock.VerifyNoOtherCalls();

        _machineSnapshotHttpClientMock.Verify(m =>
            m.GetLatestSnapshotColumnValueChangedTimestamp(MachineId, ColumnId, It.IsAny<CancellationToken>()), Times.Exactly(2));
        _machineSnapshotHttpClientMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(null, 123)]
    [InlineData(123, null)]
    [InlineData(123, 456)]
    [InlineData(null, false)]
    [InlineData(null, true)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(null, "Customer123")]
    [InlineData("Customer123", null)]
    [InlineData("Customer123", "Customer456")]
    public async Task Cache_Gets_Updated_When_Value_Changed(object? initialValue, object? updateValue)
    {
        // Arrange
        var initialChangedTimestamp = DateTime.UnixEpoch;
        var snapshotColumnValueChangedResponse = new SnapshotColumnValueChangedResponse(initialChangedTimestamp, initialValue, initialChangedTimestamp.AddMinutes(1));
        _machineSnapshotHttpClientMock
            .Setup(m => m.GetLatestSnapshotColumnValueChangedTimestamp(
                MachineId, ColumnId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new InternalItemResponse<SnapshotColumnValueChangedResponse>(snapshotColumnValueChangedResponse));

        // Request data and add it to the cache
        var firstCallResult = await _subject.Get(MachineId, ColumnId, _cancellationToken);
        firstCallResult.Should().Be(initialChangedTimestamp);

        var updateChangedTimestamp = initialChangedTimestamp.AddMinutes(2);
        var updatedSnapshotDto = SnapshotQueueMessageDto.FromSnapshotResponse(CreateMachineSnapshotResponse(ColumnId, updateValue, updateChangedTimestamp));
        var machineSnapshotChangedEventArgs =
            new LiveSnapshotEventArgs(MachineId, updatedSnapshotDto, isMinutelySnapshot: true);

        // Act

        // Trigger a cache update
        _latestMachineSnapshotCachingServiceMock.Raise(m => m.CacheChanged += null, machineSnapshotChangedEventArgs);

        // Assert
        var secondCallResult = await _subject.Get(MachineId, ColumnId, _cancellationToken);
        secondCallResult.Should().Be(updateChangedTimestamp);

        _latestMachineSnapshotCachingServiceMock
            .VerifyAdd(m => m.CacheChanged += It.IsAny<EventHandler<LiveSnapshotEventArgs>>(), Times.Once);
        _latestMachineSnapshotCachingServiceMock.VerifyNoOtherCalls();

        _machineSnapshotHttpClientMock.Verify(m =>
            m.GetLatestSnapshotColumnValueChangedTimestamp(MachineId, ColumnId, It.IsAny<CancellationToken>()), Times.Once);
        _machineSnapshotHttpClientMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(null)]
    [InlineData(false)]
    [InlineData(true)]
    [InlineData(123)]
    [InlineData("Customer123")]
    public async Task Cache_Does_Not_Get_Updated_When_Value_Not_Changed(object? value)
    {
        // Arrange
        var expectedChangedTimestamp = DateTime.UnixEpoch;
        var snapshotColumnValueChangedResponse = new SnapshotColumnValueChangedResponse(expectedChangedTimestamp, value, expectedChangedTimestamp.AddMinutes(1));

        _machineSnapshotHttpClientMock
            .Setup(m => m.GetLatestSnapshotColumnValueChangedTimestamp(
                MachineId, ColumnId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new InternalItemResponse<SnapshotColumnValueChangedResponse>(snapshotColumnValueChangedResponse));

        // Request data and add it to the cache
        var firstCallResult = await _subject.Get(MachineId, ColumnId, _cancellationToken);
        firstCallResult.Should().Be(expectedChangedTimestamp);

        var updatedSnapshotDto = SnapshotQueueMessageDto.FromSnapshotResponse(CreateMachineSnapshotResponse(ColumnId, value, expectedChangedTimestamp.AddMinutes(2)));
        var machineSnapshotChangedEventArgs =
            new LiveSnapshotEventArgs(MachineId, updatedSnapshotDto, isMinutelySnapshot: true);

        // Act

        // Trigger a cache update
        _latestMachineSnapshotCachingServiceMock.Raise(m => m.CacheChanged += null, machineSnapshotChangedEventArgs);

        // Assert
        var secondCallResult = await _subject.Get(MachineId, ColumnId, _cancellationToken);
        secondCallResult.Should().Be(expectedChangedTimestamp);

        _latestMachineSnapshotCachingServiceMock
            .VerifyAdd(m => m.CacheChanged += It.IsAny<EventHandler<LiveSnapshotEventArgs>>(), Times.Once);
        _latestMachineSnapshotCachingServiceMock.VerifyNoOtherCalls();

        _machineSnapshotHttpClientMock.Verify(m =>
            m.GetLatestSnapshotColumnValueChangedTimestamp(MachineId, ColumnId, It.IsAny<CancellationToken>()), Times.Once);
        _machineSnapshotHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Cache_Changes_Back_To_Full_Minute_Timestamp_On_Same_Value_Within_Minute()
    {
        // Arrange cache filled
        _machineSnapshotHttpClientMock
            .Setup(m => m.GetLatestSnapshotColumnValueChangedTimestamp(
                MachineId, ColumnId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new InternalItemResponse<SnapshotColumnValueChangedResponse>(new SnapshotColumnValueChangedResponse(DateTime.UnixEpoch, 11.1, DateTime.UnixEpoch.AddMinutes(1))));

        // Assert cache filled
        var firstCallResult = await _subject.Get(MachineId, ColumnId, _cancellationToken);
        firstCallResult.Should().Be(DateTime.UnixEpoch);

        // Arrange live value changed
        var updatedSnapshotDto = new SnapshotQueueMessageDto(
            [new(ColumnId, 22.2)],
            DateTime.MinValue,
            "fakeHash",
            DateTime.UnixEpoch.AddMinutes(1).AddSeconds(10));
        var machineSnapshotChangedEventArgs = new LiveSnapshotEventArgs(MachineId, updatedSnapshotDto, isMinutelySnapshot: false);

        // Act live value changed
        _latestMachineSnapshotCachingServiceMock.Raise(m => m.CacheChanged += null, machineSnapshotChangedEventArgs);

        // Assert live value changed
        var secondCallResult = await _subject.Get(MachineId, ColumnId, _cancellationToken);
        secondCallResult.Should().Be(DateTime.UnixEpoch.AddMinutes(1).AddSeconds(10));

        // Arrange live value changed back to initial value
        updatedSnapshotDto = new SnapshotQueueMessageDto(
            [new(ColumnId, 11.1)],
            DateTime.MinValue,
            "fakeHash",
            DateTime.UnixEpoch.AddMinutes(1).AddSeconds(20));
        machineSnapshotChangedEventArgs = new LiveSnapshotEventArgs(MachineId, updatedSnapshotDto, isMinutelySnapshot: false);

        // Act live value changed back to initial value
        _latestMachineSnapshotCachingServiceMock.Raise(m => m.CacheChanged += null, machineSnapshotChangedEventArgs);

        // Assert live value changed back to initial value
        var thirdCallResult = await _subject.Get(MachineId, ColumnId, _cancellationToken);
        thirdCallResult.Should().Be(DateTime.UnixEpoch);
    }

    [Fact]
    public async Task Cache_Changes_Back_To_Full_Minute_Timestamp_On_Live_Update_In_Between()
    {
        // Arrange cache filled
        _machineSnapshotHttpClientMock
            .Setup(m => m.GetLatestSnapshotColumnValueChangedTimestamp(
                MachineId, ColumnId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new InternalItemResponse<SnapshotColumnValueChangedResponse>(new SnapshotColumnValueChangedResponse(DateTime.UnixEpoch, 11.1, DateTime.UnixEpoch.AddMinutes(1))));

        // Assert cache filled
        var firstCallResult = await _subject.Get(MachineId, ColumnId, _cancellationToken);
        firstCallResult.Should().Be(DateTime.UnixEpoch);

        // Arrange live value changed
        var updatedSnapshotDto = new SnapshotQueueMessageDto(
            [new(ColumnId, 22.2)],
            DateTime.MinValue,
            "fakeHash",
            DateTime.UnixEpoch.AddMinutes(1).AddSeconds(10));
        var machineSnapshotChangedEventArgs = new LiveSnapshotEventArgs(MachineId, updatedSnapshotDto, isMinutelySnapshot: false);

        // Act live value changed
        _latestMachineSnapshotCachingServiceMock.Raise(m => m.CacheChanged += null, machineSnapshotChangedEventArgs);

        // Assert live value changed
        var secondCallResult = await _subject.Get(MachineId, ColumnId, _cancellationToken);
        secondCallResult.Should().Be(DateTime.UnixEpoch.AddMinutes(1).AddSeconds(10));

        // Arrange live value changed back to initial value
        updatedSnapshotDto = new SnapshotQueueMessageDto(
            [new(ColumnId, 11.1)],
            DateTime.MinValue,
            "fakeHash",
            DateTime.UnixEpoch.AddMinutes(2));
        machineSnapshotChangedEventArgs = new LiveSnapshotEventArgs(MachineId, updatedSnapshotDto, isMinutelySnapshot: true);

        // Act live value changed back to initial value
        _latestMachineSnapshotCachingServiceMock.Raise(m => m.CacheChanged += null, machineSnapshotChangedEventArgs);

        // Assert live value changed back to initial value
        var thirdCallResult = await _subject.Get(MachineId, ColumnId, _cancellationToken);
        thirdCallResult.Should().Be(DateTime.UnixEpoch);
    }

    private static MachineSnapshotResponse CreateMachineSnapshotResponse(
        string columnId, object? columnValue, DateTime snapshotTime)
    {
        var snapshotMetaDto = new SnapshotMetaDto(MachineId, "fakeHash", DateTime.MinValue, []);
        var snapshotDto = new SnapshotDto(
            [new(columnId, columnValue)],
            snapshotTime);
        return new MachineSnapshotResponse(snapshotMetaDto, snapshotDto);
    }

    private async Task SendMessageToQueue(HistoricSnapshotChangeMessage message) => await _queueCallback!(message);
}