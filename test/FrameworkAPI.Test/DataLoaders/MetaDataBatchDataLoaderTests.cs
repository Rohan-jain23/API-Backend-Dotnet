using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FrameworkAPI.DataLoaders;
using FrameworkAPI.Exceptions;
using FrameworkAPI.Models;
using FrameworkAPI.Models.DataLoader;
using HotChocolate.Fetching;
using Microsoft.AspNetCore.Http;
using Moq;
using WuH.Ruby.Common.Core;
using WuH.Ruby.MetaDataHandler.Client;
using Xunit;

namespace FrameworkAPI.Test.DataLoaders;

public class MetaDataBatchDataLoaderTests
{
    private const string MachineId1 = "EQ00001";
    private const string MachineId2 = "EQ00002";
    private readonly Mock<IMetaDataHandlerHttpClient> _metaDataHandlerHttpClientMock = new();

    [Fact]
    public async Task LoadBatchAsync_With_Multiple_Request_Types_And_Multiple_Machines_Succeeds()
    {
        // Arrange
        SetupMockMetaDataHandlerForIdentifiers();
        SetupMockMetaDataHandlerForLastPartOfPath();

        var batchScheduler = new BatchScheduler();
        var dataLoader = new MachineMetaDataBatchDataLoader(_metaDataHandlerHttpClientMock.Object, batchScheduler);

        var tasks = new List<Task<DataResult<ProcessVariableMetaDataResponseItem>>>() {
            dataLoader.LoadAsync(new MetaDataRequestKey(MachineId1, Constants.LastPartOfPath.PrimaryProfile, MetaDataRequestType.LastPartOfPath)),
            dataLoader.LoadAsync(new MetaDataRequestKey(MachineId1, Constants.Identifiers.Profile, MetaDataRequestType.VariableIdentifier)),
            dataLoader.LoadAsync(new MetaDataRequestKey(MachineId2, Constants.LastPartOfPath.PrimaryProfile, MetaDataRequestType.LastPartOfPath)),
            dataLoader.LoadAsync(new MetaDataRequestKey(MachineId2, Constants.Identifiers.Profile, MetaDataRequestType.VariableIdentifier))
        };

        // Act
        batchScheduler.BeginDispatch();
        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().NotBeNull();
        results.Should().HaveCount(4);
        results.Should().AllSatisfy(result => result.Value.Should().NotBeNull());
        results[0].Value!.Path.Should().Be($"{MachineId1}/Fake/Path/To/LastPartOfPathProfile/0/{Constants.LastPartOfPath.PrimaryProfile}");
        results[1].Value!.Path.Should().Be($"{MachineId1}/Fake/Path/To/IdentifierProfile/0/{Constants.Identifiers.Profile}");
        results[2].Value!.Path.Should().Be($"{MachineId2}/Fake/Path/To/LastPartOfPathProfile/0/{Constants.LastPartOfPath.PrimaryProfile}");
        results[3].Value!.Path.Should().Be($"{MachineId2}/Fake/Path/To/IdentifierProfile/0/{Constants.Identifiers.Profile}");
    }

    [Fact]
    public async Task LoadBatchAsync_With_Multiple_Elements_In_Identifiers_Batch_Succeeds()
    {
        // Arrange
        SetupMockMetaDataHandlerForIdentifiers();

        var batchScheduler = new BatchScheduler();
        var dataLoader = new MachineMetaDataBatchDataLoader(_metaDataHandlerHttpClientMock.Object, batchScheduler);

        var tasks = new List<Task<DataResult<ProcessVariableMetaDataResponseItem>>>() {
            dataLoader.LoadAsync(new MetaDataRequestKey(MachineId1, Constants.Identifiers.Profile, MetaDataRequestType.VariableIdentifier)),
            dataLoader.LoadAsync(new MetaDataRequestKey(MachineId1, Constants.Identifiers.ProfileMeanValue, MetaDataRequestType.VariableIdentifier)),
        };

        // Act
        batchScheduler.BeginDispatch();
        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().NotBeNull();
        results.Should().HaveCount(2);
        results.Should().AllSatisfy(result => result.Value.Should().NotBeNull());
        results[0].Value!.Path.Should().Be($"{MachineId1}/Fake/Path/To/IdentifierProfile/0/{Constants.Identifiers.Profile}");
        results[1].Value!.Path.Should().Be($"{MachineId1}/Fake/Path/To/IdentifierProfile/1/{Constants.Identifiers.ProfileMeanValue}");
    }

    [Fact]
    public async Task LoadBatchAsync_With_Multiple_Elements_In_LastPartOfPath_Batch_Succeeds()
    {
        // Arrange
        SetupMockMetaDataHandlerForLastPartOfPath();

        var batchScheduler = new BatchScheduler();
        var dataLoader = new MachineMetaDataBatchDataLoader(_metaDataHandlerHttpClientMock.Object, batchScheduler);

        var tasks = new List<Task<DataResult<ProcessVariableMetaDataResponseItem>>>() {
            dataLoader.LoadAsync(new MetaDataRequestKey(MachineId1, Constants.LastPartOfPath.PrimaryProfile, MetaDataRequestType.LastPartOfPath)),
            dataLoader.LoadAsync(new MetaDataRequestKey(MachineId1, Constants.LastPartOfPath.PrimaryProfileMeanValue, MetaDataRequestType.LastPartOfPath)),
        };

        // Act
        batchScheduler.BeginDispatch();
        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().NotBeNull();
        results.Should().HaveCount(2);
        results.Should().AllSatisfy(result => result.Value.Should().NotBeNull());
        results[0].Value!.Path.Should().Be($"{MachineId1}/Fake/Path/To/LastPartOfPathProfile/0/{Constants.LastPartOfPath.PrimaryProfile}");
        results[1].Value!.Path.Should().Be($"{MachineId1}/Fake/Path/To/LastPartOfPathProfile/1/{Constants.LastPartOfPath.PrimaryProfileMeanValue}");
    }

    [Fact]
    public async Task LoadBatchAsync_With_Multiple_But_Missing_Elements_In_Identifiers_Batch_Succeeds()
    {
        // Arrange
        var response = new InternalListResponse<ProcessVariableMetaDataResponseItem>(
            [
                new ProcessVariableMetaDataResponseItem()
                {
                    Path = $"{MachineId1}/Fake/Path/To/IdentifierProfile/0/{Constants.Identifiers.Profile}",
                    Data = new() { VariableIdentifier = Constants.Identifiers.Profile }
                }
            ]);

        SetupMockMetaDataHandlerForIdentifiers(response);

        var batchScheduler = new BatchScheduler();
        var dataLoader = new MachineMetaDataBatchDataLoader(_metaDataHandlerHttpClientMock.Object, batchScheduler);

        var tasks = new List<Task<DataResult<ProcessVariableMetaDataResponseItem>>>() {
            dataLoader.LoadAsync(new MetaDataRequestKey(MachineId1, "FakeIdentifier", MetaDataRequestType.VariableIdentifier)),
            dataLoader.LoadAsync(new MetaDataRequestKey(MachineId1, Constants.Identifiers.Profile, MetaDataRequestType.VariableIdentifier)),
        };

        // Act
        batchScheduler.BeginDispatch();
        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().NotBeNull();
        results.Should().HaveCount(2);
        results[0].Value.Should().BeNull();
        results[0].Exception.Should().NotBeNull();
        results[1].Value!.Path.Should().Be($"{MachineId1}/Fake/Path/To/IdentifierProfile/0/{Constants.Identifiers.Profile}");
        results[1].Exception.Should().BeNull();
    }

    [Fact]
    public async Task LoadBatchAsync_With_Multiple_But_Missing_Elements_In_LastPartOfPath_Batch_Succeeds()
    {
        // Arrange
        var response = new InternalListResponse<ProcessVariableMetaDataResponseItem>(
            [
                new ProcessVariableMetaDataResponseItem()
                {
                    Path = $"{MachineId1}/Fake/Path/To/LastPartOfPathProfile/0/{Constants.LastPartOfPath.PrimaryProfile}"
                }
            ]);

        SetupMockMetaDataHandlerForLastPartOfPath(response);

        var batchScheduler = new BatchScheduler();
        var dataLoader = new MachineMetaDataBatchDataLoader(_metaDataHandlerHttpClientMock.Object, batchScheduler);

        var tasks = new List<Task<DataResult<ProcessVariableMetaDataResponseItem>>>() {
            dataLoader.LoadAsync(new MetaDataRequestKey(MachineId1, "FakeIdentifier", MetaDataRequestType.LastPartOfPath)),
            dataLoader.LoadAsync(new MetaDataRequestKey(MachineId1, Constants.LastPartOfPath.PrimaryProfile, MetaDataRequestType.LastPartOfPath)),
        };

        // Act
        batchScheduler.BeginDispatch();
        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().NotBeNull();
        results.Should().HaveCount(2);
        results[0].Value.Should().BeNull();
        results[0].Exception.Should().NotBeNull();
        results[1].Value!.Path.Should().Be($"{MachineId1}/Fake/Path/To/LastPartOfPathProfile/0/{Constants.LastPartOfPath.PrimaryProfile}");
        results[1].Exception.Should().BeNull();
    }

    [Fact]
    public async Task LoadBatchAsync_With_Error_In_Identifier_Still_Returns_LastPartOfPath()
    {
        // Arrange
        SetupMockMetaDataHandlerForIdentifiers(new InternalListResponse<ProcessVariableMetaDataResponseItem>(StatusCodes.Status400BadRequest, "Test Error"));
        SetupMockMetaDataHandlerForLastPartOfPath();

        var batchScheduler = new BatchScheduler();
        var dataLoader = new MachineMetaDataBatchDataLoader(_metaDataHandlerHttpClientMock.Object, batchScheduler);

        var tasks = new List<Task<DataResult<ProcessVariableMetaDataResponseItem>>>() {
            dataLoader.LoadAsync(new MetaDataRequestKey(MachineId1, Constants.LastPartOfPath.PrimaryProfile, MetaDataRequestType.LastPartOfPath)),
            dataLoader.LoadAsync(new MetaDataRequestKey(MachineId1, Constants.Identifiers.Profile, MetaDataRequestType.VariableIdentifier)),
        };

        // Act
        batchScheduler.BeginDispatch();
        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().NotBeNull();
        results.Should().HaveCount(2);
        results[0].Value.Should().NotBeNull();
        results[0].Value!.Path.Should().Be($"{MachineId1}/Fake/Path/To/LastPartOfPathProfile/0/{Constants.LastPartOfPath.PrimaryProfile}");
        results[1].Exception.Should().NotBeNull();
        results[1].Exception!.Should().BeOfType<InternalServiceException>();
        results[1].Exception!.Message.Should().Be("Test Error");
    }

    [Fact]
    public async Task LoadBatchAsync_With_Error_In_LastPartOfPath_Still_Returns_Identifier()
    {
        // Arrange
        SetupMockMetaDataHandlerForIdentifiers();
        SetupMockMetaDataHandlerForLastPartOfPath(new InternalListResponse<ProcessVariableMetaDataResponseItem>(StatusCodes.Status400BadRequest, "Test Error"));

        var batchScheduler = new BatchScheduler();
        var dataLoader = new MachineMetaDataBatchDataLoader(_metaDataHandlerHttpClientMock.Object, batchScheduler);

        var tasks = new List<Task<DataResult<ProcessVariableMetaDataResponseItem>>>() {
            dataLoader.LoadAsync(new MetaDataRequestKey(MachineId1, Constants.LastPartOfPath.PrimaryProfile, MetaDataRequestType.LastPartOfPath)),
            dataLoader.LoadAsync(new MetaDataRequestKey(MachineId1, Constants.Identifiers.Profile, MetaDataRequestType.VariableIdentifier)),
        };

        // Act
        batchScheduler.BeginDispatch();
        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().NotBeNull();
        results.Should().HaveCount(2);
        results[0].Exception.Should().NotBeNull();
        results[0].Exception!.Should().BeOfType<InternalServiceException>();
        results[0].Exception!.Message.Should().Be("Test Error");
        results[1].Value.Should().NotBeNull();
        results[1].Value!.Path.Should().Be($"{MachineId1}/Fake/Path/To/IdentifierProfile/0/{Constants.Identifiers.Profile}");
    }

    private void SetupMockMetaDataHandlerForIdentifiers(InternalListResponse<ProcessVariableMetaDataResponseItem>? response = null)
    {
        _metaDataHandlerHttpClientMock
            .Setup(mock => mock.GetProcessVariableMetaDataByIdentifiers(
                It.IsAny<CancellationToken>(),
                It.IsAny<string>(),
                It.IsAny<List<string>>()))
            .ReturnsAsync((CancellationToken _, string machineId, List<string> identifiers) =>
            {
                return response ?? new InternalListResponse<ProcessVariableMetaDataResponseItem>(
                    identifiers
                        .Select((identifier, index)
                            => new ProcessVariableMetaDataResponseItem()
                            {
                                Path = $"{machineId}/Fake/Path/To/IdentifierProfile/{index}/{identifier}",
                                Data = new() { VariableIdentifier = identifier }
                            })
                        .ToList()
                );
            });
    }

    private void SetupMockMetaDataHandlerForLastPartOfPath(InternalListResponse<ProcessVariableMetaDataResponseItem>? response = null)
    {
        _metaDataHandlerHttpClientMock
            .Setup(mock => mock.GetProcessVariableMetaDataByLastPartOfPathList(
                It.IsAny<CancellationToken>(),
                It.IsAny<string>(),
                It.IsAny<List<string>>()))
            .ReturnsAsync((CancellationToken _, string machineId, List<string> identifiers) =>
            {
                return response ?? new InternalListResponse<ProcessVariableMetaDataResponseItem>(
                    identifiers
                    .Select((identifier, index)
                        => new ProcessVariableMetaDataResponseItem()
                        {
                            Path = $"{machineId}/Fake/Path/To/LastPartOfPathProfile/{index}/{identifier}"
                        })
                    .ToList()
                );
            });
    }
}