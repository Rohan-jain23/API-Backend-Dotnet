using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FrameworkAPI.DataLoaders;
using FrameworkAPI.Exceptions;
using FrameworkAPI.Services;
using FrameworkAPI.Test.Services.Helpers;
using Moq;
using WuH.Ruby.Common.Core;
using WuH.Ruby.MetaDataHandler.Client;
using WuH.Ruby.OpcUaForwarder.Client;
using WuH.Ruby.ProcessDataReader.Client;
using Xunit;

namespace FrameworkAPI.Test.Services;

public class ProcessDataServiceTests
{
    private readonly ProcessDataService _subject = new();

    private readonly ProcessDataByTimestampBatchDataLoader _processDataByTimestampBatchDataLoader;
    private readonly MachineMetaDataBatchDataLoader _machineMetaDataBatchDataLoader;
    private readonly LatestProcessDataCacheDataLoader _latestProcessDataCacheDataLoader;
    private readonly Mock<IProcessDataReaderHttpClient> _processDataReaderHttpClientMock = new();
    private readonly Mock<IMetaDataHandlerHttpClient> _metaDataHandlerHttpClientMock = new();
    private readonly Mock<IProcessDataCachingService> _processDataCachingServiceMock = new();

    public ProcessDataServiceTests()
    {
        _processDataByTimestampBatchDataLoader = new ProcessDataByTimestampBatchDataLoader(
            _processDataReaderHttpClientMock.Object,
            new DelayedBatchScheduler());

        _latestProcessDataCacheDataLoader = new LatestProcessDataCacheDataLoader(
            _processDataCachingServiceMock.Object);

        _machineMetaDataBatchDataLoader = new MachineMetaDataBatchDataLoader(
            _metaDataHandlerHttpClientMock.Object,
            new DelayedBatchScheduler());
    }

    [Fact]
    public async Task Get_Process_Data_By_LastPartOfPath()
    {
        // Arrange

        const string machineId = "EQ12345";
        const string path = "/i/am/a/test";
        var timestamp = DateTime.UnixEpoch.AddMinutes(3);

        var metaDataResponseMock = new InternalListResponse<ProcessVariableMetaDataResponseItem>(
        [
            new()
            {
                Path = path,
                Data = new ProcessVariableMetaData
                {
                    VariableIdentifier = path,
                }
            }
        ]);

        var processDataReaderResponse = new InternalListResponse<ProcessData>(
            [
                new()
                {
                    Path = path,
                    VariableIdentifier = path,
                    Timestamp = timestamp,
                    Value = "Test"
                }
            ]
        );

        _metaDataHandlerHttpClientMock.Setup(request => request.GetProcessVariableMetaDataByIdentifiers(
            It.IsAny<CancellationToken>(),
            machineId,
            It.Is<List<string>>(list => list.Contains(path))
        )).ReturnsAsync(metaDataResponseMock);

        _processDataReaderHttpClientMock.Setup(request => request.GetDataForOneTimestampByPath(
            It.IsAny<CancellationToken>(),
            machineId,
            It.IsAny<List<string>>(),
            timestamp
        )).ReturnsAsync(processDataReaderResponse);

        // Act
        var result = await _subject.GetProcessDataByVariableIdentifier(
            _processDataByTimestampBatchDataLoader,
            _machineMetaDataBatchDataLoader,
            _latestProcessDataCacheDataLoader,
            machineId,
            path,
            timestamp,
            CancellationToken.None);

        // Assert
        _metaDataHandlerHttpClientMock.Verify(service => service.GetProcessVariableMetaDataByIdentifiers(
            It.IsAny<CancellationToken>(),
            machineId,
            It.Is<List<string>>(list => list.Contains(path))
        ), Times.Once);
        _processDataReaderHttpClientMock.Verify(request => request.GetDataForOneTimestampByPath(
            It.IsAny<CancellationToken>(),
            machineId,
            It.IsAny<List<string>>(),
            timestamp
        ), Times.Once);
        _processDataCachingServiceMock.Verify(service => service.GetLatestProcessData(
            machineId,
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()
        ), Times.Never);

        result.Exception.Should().BeNull();
        result.Value.Should().NotBeNull();
        result.Value!.Value.processData.Timestamp.Should().Be(timestamp);
        result.Value.Value.metaData.Path.Should().Be(path);
    }

    [Fact]
    public async Task Get_Process_Data_By_LastPartOfPath_Using_The_ProcessDataCache()
    {
        // Arrange
        const string machineId = "EQ12345";
        const string path = "/i/am/a/test";
        var timestamp = DateTime.UnixEpoch.AddMinutes(3);

        var metaDataResponseMock = new InternalListResponse<ProcessVariableMetaDataResponseItem>(
        [
            new ProcessVariableMetaDataResponseItem()
            {
                Path = path,
                Data = new ProcessVariableMetaData()
                {
                    VariableIdentifier = path,
                }
            }
        ]);

        var processDataReaderResponse = new ProcessData
        {
            Path = path,
            Timestamp = timestamp,
            Value = "Test"
        };

        _metaDataHandlerHttpClientMock.Setup(request => request.GetProcessVariableMetaDataByIdentifiers(
            It.IsAny<CancellationToken>(),
            machineId,
            It.Is<List<string>>(list => list.Contains(path))
        )).ReturnsAsync(metaDataResponseMock);

        _processDataCachingServiceMock.Setup(request => request.GetLatestProcessData(
            machineId,
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(processDataReaderResponse);

        // Act
        var result = await _subject.GetProcessDataByVariableIdentifier(
            _processDataByTimestampBatchDataLoader,
            _machineMetaDataBatchDataLoader,
            _latestProcessDataCacheDataLoader,
            machineId,
            path,
            null,
            CancellationToken.None);

        // Assert
        _processDataCachingServiceMock.Verify(service => service.GetLatestProcessData(
            machineId,
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
        _metaDataHandlerHttpClientMock.Verify(service => service.GetProcessVariableMetaDataByIdentifiers(
            It.IsAny<CancellationToken>(),
            machineId,
            It.Is<List<string>>(list => list.Contains(path))
        ), Times.Once);
        _processDataReaderHttpClientMock.Verify(request => request.GetDataForOneTimestampByPath(
            It.IsAny<CancellationToken>(),
            machineId,
            It.IsAny<List<string>>(),
            timestamp
        ), Times.Never);

        result.Exception.Should().BeNull();
        result.Value.Should().NotBeNull();
        result.Value!.Value.processData.Timestamp.Should().Be(timestamp);
        result.Value.Value.metaData.Path.Should().Be(path);
    }

    [Fact]
    public async Task Get_Process_Data_By_Timestamp_But_machineMetaDataByLastPartOfPathBatchDataLoader_Returns_Response_With_Error()
    {
        // Arrange

        const string machineId = "EQ12345";
        const string path = "/i/am/a/test";
        var timestamp = DateTime.UnixEpoch.AddMinutes(3);

        var metaDataResponseMock = new InternalListResponse<ProcessVariableMetaDataResponseItem>(400, new Exception("i am a test"));

        _metaDataHandlerHttpClientMock.Setup(request => request.GetProcessVariableMetaDataByIdentifiers(
            It.IsAny<CancellationToken>(),
            machineId,
            It.Is<List<string>>(list => list.Contains(path))
        )).ReturnsAsync(metaDataResponseMock);

        // Act
        var result = await _subject.GetProcessDataByVariableIdentifier(
            _processDataByTimestampBatchDataLoader,
            _machineMetaDataBatchDataLoader,
            _latestProcessDataCacheDataLoader,
            machineId,
            path,
            timestamp,
            CancellationToken.None);

        // Assert
        _metaDataHandlerHttpClientMock.Verify(service => service.GetProcessVariableMetaDataByIdentifiers(
            It.IsAny<CancellationToken>(),
            machineId,
            It.Is<List<string>>(list => list.Contains(path))
        ), Times.Once);
        _processDataReaderHttpClientMock.Verify(request => request.GetDataForOneTimestampByPath(
            It.IsAny<CancellationToken>(),
            machineId,
            It.IsAny<List<string>>(),
            timestamp
        ), Times.Never);
        _processDataCachingServiceMock.Verify(service => service.GetLatestProcessData(
            machineId,
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()
        ), Times.Never);

        result.Exception.Should().BeOfType<InternalServiceException>();
        result.Exception!.Message.Should().Be("i am a test");
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task Get_Process_Data_By_LastPartOfPath_But_ProcessDataByTimestampBatchDataLoader_Returns_Response_With_Error()
    {
        // Arrange

        const string machineId = "EQ12345";
        const string path = "/i/am/a/test";
        var timestamp = DateTime.UnixEpoch.AddMinutes(3);

        var metaDataResponseMock = new InternalListResponse<ProcessVariableMetaDataResponseItem>(
        [
            new ProcessVariableMetaDataResponseItem()
            {
                Path = path,
                Data = new ProcessVariableMetaData()
                {
                    VariableIdentifier = path,
                }
            }
        ]);

        var processDataReaderResponse = new InternalListResponse<ProcessData>(400, new Exception("i am a test"));

        _metaDataHandlerHttpClientMock.Setup(request => request.GetProcessVariableMetaDataByIdentifiers(
            It.IsAny<CancellationToken>(),
            machineId,
            It.Is<List<string>>(list => list.Contains(path))
        )).ReturnsAsync(metaDataResponseMock);

        _processDataReaderHttpClientMock.Setup(request => request.GetDataForOneTimestampByPath(
            It.IsAny<CancellationToken>(),
            machineId,
            It.IsAny<List<string>>(),
            timestamp
        )).ReturnsAsync(processDataReaderResponse);

        // Act
        var result = await _subject.GetProcessDataByVariableIdentifier(
            _processDataByTimestampBatchDataLoader,
            _machineMetaDataBatchDataLoader,
            _latestProcessDataCacheDataLoader,
            machineId,
            path,
            timestamp,
            CancellationToken.None);

        // Assert
        _metaDataHandlerHttpClientMock.Verify(service => service.GetProcessVariableMetaDataByIdentifiers(
            It.IsAny<CancellationToken>(),
            machineId,
            It.Is<List<string>>(list => list.Contains(path))
        ), Times.Once);
        _processDataReaderHttpClientMock.Verify(request => request.GetDataForOneTimestampByPath(
            It.IsAny<CancellationToken>(),
            machineId,
            It.IsAny<List<string>>(),
            timestamp
        ), Times.Once);
        _processDataCachingServiceMock.Verify(service => service.GetLatestProcessData(
            machineId,
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()
        ), Times.Never);

        result.Exception.Should().BeOfType<InternalServiceException>();
        result.Exception!.Message.Should().Be("i am a test");
        result.Value.Should().BeNull();
    }
}