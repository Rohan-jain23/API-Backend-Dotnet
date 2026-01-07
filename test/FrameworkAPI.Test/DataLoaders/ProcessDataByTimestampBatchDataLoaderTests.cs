using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FrameworkAPI.DataLoaders;
using FrameworkAPI.Models;
using FrameworkAPI.Models.DataLoader;
using HotChocolate.Fetching;
using Moq;
using WuH.Ruby.Common.Core;
using WuH.Ruby.ProcessDataReader.Client;
using Xunit;

namespace FrameworkAPI.Test.DataLoaders;

public class ProcessDataByTimestampBatchDataLoaderTests
{
    private const string MachineId1 = "EQ00001";
    private const string MachineId2 = "EQ00002";
    private readonly Mock<IProcessDataReaderHttpClient> _processDataReaderHttpClientMock = new();

    [Fact]
    public async Task LoadBatchAsync_With_Multiple_Request_Types_And_Multiple_Machines_Succeeds()
    {
        // Arrange
        SetupMockProcessDataReaderForIdentifiers();
        SetupMockProcessDataReaderForPath();

        var batchScheduler = new BatchScheduler();
        var dataLoader = new ProcessDataByTimestampBatchDataLoader(_processDataReaderHttpClientMock.Object, batchScheduler);
        var testPathMachine1 = $"{MachineId1}/Full/Path/Dummy";
        var testPathMachine2 = $"{MachineId2}/Full/Path/Dummy";
        var tasks = new List<Task<DataResult<ProcessData>>>() {
            dataLoader.LoadAsync(new ProcessDataRequestKey(MachineId1, testPathMachine1, ProcessDataRequestType.Path, DateTime.UnixEpoch)),
            dataLoader.LoadAsync(new ProcessDataRequestKey(MachineId1, Constants.Identifiers.Profile, ProcessDataRequestType.VariableIdentifier, DateTime.UnixEpoch)),
            dataLoader.LoadAsync(new ProcessDataRequestKey(MachineId2, testPathMachine2, ProcessDataRequestType.Path, DateTime.UnixEpoch)),
            dataLoader.LoadAsync(new ProcessDataRequestKey(MachineId2, Constants.Identifiers.Profile, ProcessDataRequestType.VariableIdentifier, DateTime.UnixEpoch))
        };

        // Act
        batchScheduler.BeginDispatch();
        var results = await Task.WhenAll(tasks);

        // Assert
        _processDataReaderHttpClientMock.Verify(
            mock => mock.GetDataForOneTimestampByIdentifier(
                It.IsAny<CancellationToken>(),
                It.IsAny<string>(),
                It.IsAny<List<string>>(),
                It.IsAny<DateTime?>()),
            Times.Exactly(2));
        _processDataReaderHttpClientMock.Verify(
            mock => mock.GetDataForOneTimestampByPath(
                It.IsAny<CancellationToken>(),
                It.IsAny<string>(),
                It.IsAny<List<string>>(),
                It.IsAny<DateTime?>()),
            Times.Exactly(2));
        _processDataReaderHttpClientMock.VerifyNoOtherCalls();

        results.Should().NotBeNull();
        results.Should().HaveCount(4);
        results.Should().AllSatisfy(result => result.Value.Should().NotBeNull());
        results[0].Value!.Path.Should().Be(testPathMachine1);
        results[1].Value!.Path.Should().Be($"{MachineId1}/Fake/Path/To/IdentifierProfile/0/{Constants.Identifiers.Profile}");
        results[2].Value!.Path.Should().Be(testPathMachine2);
        results[3].Value!.Path.Should().Be($"{MachineId2}/Fake/Path/To/IdentifierProfile/0/{Constants.Identifiers.Profile}");
    }

    [Fact]
    public async Task LoadBatchAsync_With_Multiple_Identifiers_Should_Request_Once()
    {
        // Arrange
        SetupMockProcessDataReaderForIdentifiers();

        var batchScheduler = new BatchScheduler();
        var dataLoader = new ProcessDataByTimestampBatchDataLoader(_processDataReaderHttpClientMock.Object, batchScheduler);
        var tasks = new List<Task<DataResult<ProcessData>>>() {
            dataLoader.LoadAsync(new ProcessDataRequestKey(MachineId1, Constants.Identifiers.ProfileMeanValue, ProcessDataRequestType.VariableIdentifier, DateTime.UnixEpoch)),
            dataLoader.LoadAsync(new ProcessDataRequestKey(MachineId1, Constants.Identifiers.Profile, ProcessDataRequestType.VariableIdentifier, DateTime.UnixEpoch)),
        };

        // Act
        batchScheduler.BeginDispatch();
        var results = await Task.WhenAll(tasks);

        // Assert
        _processDataReaderHttpClientMock.Verify(
            mock => mock.GetDataForOneTimestampByIdentifier(
                It.IsAny<CancellationToken>(),
                It.IsAny<string>(),
                It.IsAny<List<string>>(),
                It.IsAny<DateTime?>()),
            Times.Once);
        _processDataReaderHttpClientMock.VerifyNoOtherCalls();

        results.Should().NotBeNull();
        results.Should().HaveCount(2);
        results.Should().AllSatisfy(result => result.Value.Should().NotBeNull());
        results[0].Value!.VariableIdentifier.Should().Be(Constants.Identifiers.ProfileMeanValue);
        results[1].Value!.VariableIdentifier.Should().Be(Constants.Identifiers.Profile);
    }

    [Fact]
    public async Task LoadBatchAsync_With_Multiple_Timestamps_Should_Return_Success()
    {
        // Arrange
        SetupMockProcessDataReaderForIdentifiers();

        var batchScheduler = new BatchScheduler();
        var dataLoader = new ProcessDataByTimestampBatchDataLoader(_processDataReaderHttpClientMock.Object, batchScheduler);
        var tasks = new List<Task<DataResult<ProcessData>>>() {
            dataLoader.LoadAsync(new ProcessDataRequestKey(MachineId1, Constants.Identifiers.Profile, ProcessDataRequestType.VariableIdentifier, DateTime.UnixEpoch)),
            dataLoader.LoadAsync(new ProcessDataRequestKey(MachineId1, Constants.Identifiers.Profile, ProcessDataRequestType.VariableIdentifier, DateTime.UnixEpoch.AddDays(1))),
        };

        // Act
        batchScheduler.BeginDispatch();
        var results = await Task.WhenAll(tasks);

        // Assert
        _processDataReaderHttpClientMock.Verify(
            mock => mock.GetDataForOneTimestampByIdentifier(
                It.IsAny<CancellationToken>(),
                It.IsAny<string>(),
                It.IsAny<List<string>>(),
                It.IsAny<DateTime?>()),
            Times.Exactly(2));
        _processDataReaderHttpClientMock.VerifyNoOtherCalls();

        results.Should().NotBeNull();
        results.Should().HaveCount(2);
        results.Should().AllSatisfy(result => result.Value.Should().NotBeNull());
        results[0].Value!.VariableIdentifier.Should().Be(Constants.Identifiers.Profile);
        results[0].Value!.Timestamp.Should().Be(DateTime.UnixEpoch);
        results[1].Value!.VariableIdentifier.Should().Be(Constants.Identifiers.Profile);
        results[1].Value!.Timestamp.Should().Be(DateTime.UnixEpoch.AddDays(1));
    }

    [Fact]
    public async Task LoadBatchAsync_With_Multiple_Paths_Should_Request_Once()
    {
        // Arrange
        SetupMockProcessDataReaderForPath();

        var batchScheduler = new BatchScheduler();
        var dataLoader = new ProcessDataByTimestampBatchDataLoader(_processDataReaderHttpClientMock.Object, batchScheduler);
        var testPath1 = $"{MachineId1}/Full/Path/Dummy/1";
        var testPath2 = $"{MachineId1}/Full/Path/Dummy/2";
        var tasks = new List<Task<DataResult<ProcessData>>>() {
            dataLoader.LoadAsync(new ProcessDataRequestKey(MachineId1, testPath1, ProcessDataRequestType.Path, DateTime.UnixEpoch)),
            dataLoader.LoadAsync(new ProcessDataRequestKey(MachineId1, testPath2, ProcessDataRequestType.Path, DateTime.UnixEpoch)),
        };

        // Act
        batchScheduler.BeginDispatch();
        var results = await Task.WhenAll(tasks);

        // Assert
        _processDataReaderHttpClientMock.Verify(
            mock => mock.GetDataForOneTimestampByPath(
                It.IsAny<CancellationToken>(),
                It.IsAny<string>(),
                It.IsAny<List<string>>(),
                It.IsAny<DateTime?>()),
            Times.Once);
        _processDataReaderHttpClientMock.VerifyNoOtherCalls();

        results.Should().NotBeNull();
        results.Should().HaveCount(2);
        results.Should().AllSatisfy(result => result.Value.Should().NotBeNull());
        results[0].Value!.Path.Should().Be(testPath1);
        results[1].Value!.Path.Should().Be(testPath2);
    }

    [Fact]
    public async Task LoadBatchAsync_With_Error_In_Identifier_Still_Returns_Path()
    {
        // Arrange
        SetupMockProcessDataReaderForIdentifiers(new InternalListResponse<ProcessData>(500, "Test Error"));
        SetupMockProcessDataReaderForPath();

        var batchScheduler = new BatchScheduler();
        var dataLoader = new ProcessDataByTimestampBatchDataLoader(_processDataReaderHttpClientMock.Object, batchScheduler);
        var testPath = "Fake/Path/To/Identifier";
        var tasks = new List<Task<DataResult<ProcessData>>>() {
            dataLoader.LoadAsync(new ProcessDataRequestKey(MachineId1, testPath, ProcessDataRequestType.Path, DateTime.UnixEpoch)),
            dataLoader.LoadAsync(new ProcessDataRequestKey(MachineId1, Constants.Identifiers.ProfileMeanValue, ProcessDataRequestType.VariableIdentifier, DateTime.UnixEpoch)),
        };

        // Act
        batchScheduler.BeginDispatch();
        var results = await Task.WhenAll(tasks);

        // Assert
        _processDataReaderHttpClientMock.Verify(
            mock => mock.GetDataForOneTimestampByIdentifier(
                It.IsAny<CancellationToken>(),
                It.IsAny<string>(),
                It.IsAny<List<string>>(),
                It.IsAny<DateTime?>()),
            Times.Once);
        _processDataReaderHttpClientMock.Verify(
            mock => mock.GetDataForOneTimestampByPath(
                It.IsAny<CancellationToken>(),
                It.IsAny<string>(),
                It.IsAny<List<string>>(),
                It.IsAny<DateTime?>()),
            Times.Once);
        _processDataReaderHttpClientMock.VerifyNoOtherCalls();

        results.Should().NotBeNull();
        results.Should().HaveCount(2);
        results[0].Value!.Path.Should().Be(testPath);
        results[1].Exception.Should().NotBeNull();
    }

    [Fact]
    public async Task LoadBatchAsync_With_Error_In_Path_Still_Returns_Identifier()
    {
        // Arrange
        SetupMockProcessDataReaderForIdentifiers();
        SetupMockProcessDataReaderForPath(new InternalListResponse<ProcessData>(500, "Test Error"));

        var batchScheduler = new BatchScheduler();
        var dataLoader = new ProcessDataByTimestampBatchDataLoader(_processDataReaderHttpClientMock.Object, batchScheduler);
        var tasks = new List<Task<DataResult<ProcessData>>>() {
            dataLoader.LoadAsync(new ProcessDataRequestKey(MachineId1, Constants.Identifiers.ProfileMeanValue, ProcessDataRequestType.VariableIdentifier, DateTime.UnixEpoch)),
            dataLoader.LoadAsync(new ProcessDataRequestKey(MachineId1, "Fake/Path/To/Identifier", ProcessDataRequestType.Path, DateTime.UnixEpoch)),
        };

        // Act
        batchScheduler.BeginDispatch();
        var results = await Task.WhenAll(tasks);

        // Assert
        _processDataReaderHttpClientMock.Verify(
            mock => mock.GetDataForOneTimestampByIdentifier(
                It.IsAny<CancellationToken>(),
                It.IsAny<string>(),
                It.IsAny<List<string>>(),
                It.IsAny<DateTime?>()),
            Times.Once);
        _processDataReaderHttpClientMock.Verify(
            mock => mock.GetDataForOneTimestampByPath(
                It.IsAny<CancellationToken>(),
                It.IsAny<string>(),
                It.IsAny<List<string>>(),
                It.IsAny<DateTime?>()),
            Times.Once);
        _processDataReaderHttpClientMock.VerifyNoOtherCalls();

        results.Should().NotBeNull();
        results.Should().HaveCount(2);
        results[0].Value!.VariableIdentifier.Should().Be(Constants.Identifiers.ProfileMeanValue);
        results[1].Exception.Should().NotBeNull();
    }

    [Fact]
    public async Task LoadBatchAsync_With_Timestamp_Null_Returns_Success()
    {
        // Arrange
        SetupMockProcessDataReaderForIdentifiers();
        SetupMockProcessDataReaderForPath();

        var batchScheduler = new BatchScheduler();
        var dataLoader = new ProcessDataByTimestampBatchDataLoader(_processDataReaderHttpClientMock.Object, batchScheduler);
        var tasks = new List<Task<DataResult<ProcessData>>>() {
            dataLoader.LoadAsync(new ProcessDataRequestKey(MachineId1, Constants.Identifiers.Profile, ProcessDataRequestType.VariableIdentifier)),
        };

        // Act
        batchScheduler.BeginDispatch();
        var results = await Task.WhenAll(tasks);

        // Assert
        _processDataReaderHttpClientMock.Verify(
            mock => mock.GetDataForOneTimestampByIdentifier(
                It.IsAny<CancellationToken>(),
                It.IsAny<string>(),
                It.IsAny<List<string>>(),
                null),
            Times.Once);
        _processDataReaderHttpClientMock.VerifyNoOtherCalls();

        results.Should().NotBeNull();
        results.Should().HaveCount(1);
        results[0].Exception.Should().BeNull();
        results[0].Value.Should().NotBeNull();
        results[0].Value!.VariableIdentifier.Should().Be(Constants.Identifiers.Profile);
    }

    private void SetupMockProcessDataReaderForIdentifiers(InternalListResponse<ProcessData>? response = null)
    {
        _processDataReaderHttpClientMock
            .Setup(mock => mock.GetDataForOneTimestampByIdentifier(
                It.IsAny<CancellationToken>(),
                It.IsAny<string>(),
                It.IsAny<List<string>>(),
                It.IsAny<DateTime?>()))
            .ReturnsAsync((CancellationToken _, string machineId, List<string> identifiers, DateTime? timestamp) =>
            {
                return response ?? new InternalListResponse<ProcessData>(
                    identifiers
                        .Select((identifier, index)
                            => new ProcessData()
                            {
                                Path = $"{machineId}/Fake/Path/To/IdentifierProfile/{index}/{identifier}",
                                VariableIdentifier = identifier,
                                Timestamp = timestamp ?? DateTime.MaxValue
                            })
                        .ToList()
                );
            });
    }

    private void SetupMockProcessDataReaderForPath(InternalListResponse<ProcessData>? response = null)
    {
        _processDataReaderHttpClientMock
            .Setup(mock => mock.GetDataForOneTimestampByPath(
                It.IsAny<CancellationToken>(),
                It.IsAny<string>(),
                It.IsAny<List<string>>(),
                It.IsAny<DateTime?>()))
            .ReturnsAsync((CancellationToken _, string machineId, List<string> paths, DateTime? timestamp) =>
            {
                return response ?? new InternalListResponse<ProcessData>(
                    paths
                    .Select((path, index)
                        => new ProcessData()
                        {
                            Path = path,
                            Timestamp = timestamp ?? DateTime.MaxValue
                        })
                    .ToList()
                );
            });
    }
}