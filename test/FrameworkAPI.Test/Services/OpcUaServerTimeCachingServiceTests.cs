using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FrameworkAPI.Exceptions;
using FrameworkAPI.Services;
using Moq;
using WuH.Ruby.Common.Core;
using WuH.Ruby.OpcUaForwarder.Client;
using WuH.Ruby.ProcessDataReader.Client;
using Xunit;

namespace FrameworkAPI.Test.Services;

public class OpcUaServerTimeCachingServiceTests
{
    private const string MachineId = "EQ00001";

    private readonly OpcUaServerTimeCachingService _subject;
    private readonly Mock<IProcessDataReaderHttpClient> _processDataReaderHttpClientMock = new();
    private readonly Mock<IProcessDataQueueWrapper> _processDataQueueWrapperMock = new();

    public OpcUaServerTimeCachingServiceTests()
    {
        _subject = new OpcUaServerTimeCachingService(
            _processDataReaderHttpClientMock.Object, _processDataQueueWrapperMock.Object);
    }

    [Fact]
    public async Task Get_Returns_MachineTime()
    {
        // Arrange
        var expectedMachineTime = DateTime.UnixEpoch;
        _processDataReaderHttpClientMock
            .Setup(m => m.GetLastReceivedOpcUaServerTime(It.IsAny<CancellationToken>(), MachineId))
            .ReturnsAsync(new InternalItemResponse<DateTime>(expectedMachineTime));

        // Act
        var (machineTime, _) = await _subject.Get(MachineId, CancellationToken.None);

        // Assert
        machineTime.Should().Be(expectedMachineTime);

        _processDataQueueWrapperMock.Verify(m => m.SubscribeForOpcUaServerTime(
            MachineId, It.IsAny<Func<string, DateTime?, Task>>()), Times.Once);
        _processDataQueueWrapperMock.VerifyNoOtherCalls();

        _processDataReaderHttpClientMock.VerifyAll();
        _processDataReaderHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Get_Returns_Cached_MachineTime()
    {
        // Arrange
        Func<string, DateTime?, Task>? callback = null;

        _processDataQueueWrapperMock
            .Setup(m => m.SubscribeForOpcUaServerTime(
                MachineId, It.IsAny<Func<string, DateTime?, Task>>()))
            .Callback(
                (string _, Func<string, DateTime?, Task> callbackObj) =>
                {
                    callback = callbackObj;
                });

        _processDataReaderHttpClientMock
            .Setup(m => m.GetLastReceivedOpcUaServerTime(It.IsAny<CancellationToken>(), MachineId))
            .ReturnsAsync(new InternalItemResponse<DateTime>(DateTime.UnixEpoch));

        await _subject.Get(MachineId, CancellationToken.None);

        _processDataQueueWrapperMock.Reset();
        _processDataReaderHttpClientMock.Reset();

        callback.Should().NotBeNull();

        var expectedMachineTime =
            new DateTime(year: 2023, month: 1, day: 15, hour: 0, minute: 0, second: 0, DateTimeKind.Utc);
        await callback!(MachineId, expectedMachineTime);

        // Act
        var (machineTime, _) = await _subject.Get(MachineId, CancellationToken.None);

        // Assert
        machineTime.Should().Be(expectedMachineTime);

        _processDataQueueWrapperMock.VerifyNoOtherCalls();
        _processDataReaderHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Get_Throws_Exception_When_ProcessDataReader_Has_Error()
    {
        // Arrange
        _processDataReaderHttpClientMock
            .Setup(m => m.GetLastReceivedOpcUaServerTime(It.IsAny<CancellationToken>(), MachineId))
            .ReturnsAsync(new InternalItemResponse<DateTime>((int)HttpStatusCode.InternalServerError, "Error"));

        // Act
        var (_, exception) = await _subject.Get(MachineId, CancellationToken.None);

        // Assert
        exception.Should().BeOfType<InternalServiceException>();

        _processDataQueueWrapperMock.Verify(m => m.SubscribeForOpcUaServerTime(
            MachineId, It.IsAny<Func<string, DateTime?, Task>>()), Times.Once);
        _processDataQueueWrapperMock.VerifyNoOtherCalls();

        _processDataReaderHttpClientMock.VerifyAll();
        _processDataReaderHttpClientMock.VerifyNoOtherCalls();
    }
}