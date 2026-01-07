using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.DataLoaders;
using FrameworkAPI.Schema.MaterialLot;
using FrameworkAPI.Services;
using FrameworkAPI.Services.Interfaces;
using FrameworkAPI.Test.TestHelpers;
using HotChocolate;
using HotChocolate.Execution;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Snapshooter.Xunit;
using WuH.Ruby.Common.Core;
using WuH.Ruby.MachineDataHandler.Client;
using WuH.Ruby.MaterialDataHandler.Client.Enums.Lot;
using WuH.Ruby.MaterialDataHandler.Client.HttpClient;
using WuH.Ruby.MaterialDataHandler.Client.Models.Lot;
using Xunit;

namespace FrameworkAPI.Test.Queries.MaterialLotQuery;

public class MaterialLotQueryIntegrationTests
{
    private const string MachineId = "EQ00001";

    private const string SimpleFilterQuery =
        @"{
             materialLots(take: 10, skip: 5) {
               items {
                 startTime
                 materialLotId
                 machineId
                 endTime
               } 
               pageInfo {
                 hasNextPage
                 hasPreviousPage
               }
             }
          }";

    private readonly Mock<IMaterialDataHandlerHttpClient> _materialDataHandlerHttpClientMock = new();
    private readonly Mock<IMachineCachingService> _machineCachingServiceMock = new();
    private readonly Mock<ILogger<MaterialLotCacheDataLoader>> _materialLotCacheDataLoaderLoggerMock = new();
    private readonly Mock<ILogger<MaterialLotsCacheDataLoader>> _materialLotsCacheDataLoaderLoggerMock = new();

    [Fact]
    public async Task GetMaterialLot_With_LotId_Should_Return_The_Correct_Response()
    {
        // Arrange
        var executor = await InitializeExecutor();

        using var fakeLocalTimeZone = new FakeLocalTimeZone(TimeZoneInfo.FindSystemTimeZoneById("US/Eastern"));

        _materialDataHandlerHttpClientMock
            .Setup(m => m.GetLot(It.IsAny<CancellationToken>(), "WIND001"))
            .ReturnsAsync(new InternalItemResponse<Lot>(MaterialLotMock.GenerateMaterialLot("EQ00001")));

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                @"{
                    materialLot(materialLotId: ""WIND001"") {
                    endTime
                    machineId
                    materialLotId
                    startTime
                    }
                }")
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        _materialDataHandlerHttpClientMock.VerifyAll();
        _materialDataHandlerHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetMaterialLot_With_LotId_Should_Return_An_Erroneous_Response_If_GetLot_Returns_An_Error()
    {
        // Arrange
        var executor = await InitializeExecutor();

        using var fakeLocalTimeZone = new FakeLocalTimeZone(TimeZoneInfo.FindSystemTimeZoneById("US/Eastern"));

        _materialDataHandlerHttpClientMock
            .Setup(m => m.GetLot(It.IsAny<CancellationToken>(), "WIND001"))
            .ReturnsAsync(new InternalItemResponse<Lot>((int)HttpStatusCode.InternalServerError, "Error"));

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(
                @"{
                    materialLot(materialLotId: ""WIND001"") {
                    endTime
                    machineId
                    materialLotId
                    startTime
                    }
                }")
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        _materialDataHandlerHttpClientMock.VerifyAll();
        _materialDataHandlerHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetMaterialLots_With_Filter_Should_Return_The_Correct_Response()
    {
        // Arrange
        var executor = await InitializeExecutor();

        using var fakeLocalTimeZone = new FakeLocalTimeZone(TimeZoneInfo.FindSystemTimeZoneById("US/Eastern"));

        _machineCachingServiceMock
            .Setup(m => m.GetMachinesAsInternalListResponse(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalListResponse<Machine>(new List<Machine>
            {
                new() { MachineId = MachineId }
            }));

        var materialLots = new List<Lot>();

        for (var i = 0; i < 15; i++)
        {
            materialLots.Add(MaterialLotMock.GenerateMaterialLot(MachineId, $"WIND{i}001"));
        }

        _materialDataHandlerHttpClientMock
            .Setup(m => m.FindLots(
                It.IsAny<CancellationToken>(),
                It.IsAny<string>(),
                MachineId,
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<bool?>(),
                15,
                It.IsAny<TypeOfMaterial?>(),
                It.IsAny<bool?>()))
            .ReturnsAsync(new InternalListResponse<Lot>(materialLots));

        _materialDataHandlerHttpClientMock
            .Setup(m => m.GetLotCount(It.IsAny<CancellationToken>(), MachineId))
            .ReturnsAsync(new InternalItemResponse<int>(1000));

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(SimpleFilterQuery)
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        _machineCachingServiceMock.VerifyAll();
        _machineCachingServiceMock.VerifyNoOtherCalls();

        _materialDataHandlerHttpClientMock.VerifyAll();
        _materialDataHandlerHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetMaterialLots_With_Filter_Should_Return_Empty_Response_If_Client_FindLots_Returns_204()
    {
        // Arrange
        var executor = await InitializeExecutor();

        using var fakeLocalTimeZone = new FakeLocalTimeZone(TimeZoneInfo.FindSystemTimeZoneById("US/Eastern"));

        _machineCachingServiceMock
            .Setup(m => m.GetMachinesAsInternalListResponse(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalListResponse<Machine>(new List<Machine>
            {
                new() { MachineId = MachineId }
            }));

        _materialDataHandlerHttpClientMock
            .Setup(m => m.FindLots(
                It.IsAny<CancellationToken>(),
                It.IsAny<string>(),
                MachineId,
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<bool?>(),
                15,
                It.IsAny<TypeOfMaterial?>(),
                It.IsAny<bool?>()))
            .ReturnsAsync(new InternalListResponse<Lot>((int)HttpStatusCode.NoContent, "Empty"));

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(SimpleFilterQuery)
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        _machineCachingServiceMock.VerifyAll();
        _machineCachingServiceMock.VerifyNoOtherCalls();

        _materialDataHandlerHttpClientMock.VerifyAll();
        _materialDataHandlerHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetMaterialLots_With_Filter_Should_Return_Erroneous_Response_If_Client_FindLots_Returns_500()
    {
        // Arrange
        var executor = await InitializeExecutor();

        using var fakeLocalTimeZone = new FakeLocalTimeZone(TimeZoneInfo.FindSystemTimeZoneById("US/Eastern"));

        _machineCachingServiceMock
            .Setup(m => m.GetMachinesAsInternalListResponse(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalListResponse<Machine>(new List<Machine>
            {
                new() { MachineId = MachineId }
            }));

        _materialDataHandlerHttpClientMock
            .Setup(m => m.FindLots(
                It.IsAny<CancellationToken>(),
                It.IsAny<string>(),
                MachineId,
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<bool?>(),
                15,
                It.IsAny<TypeOfMaterial?>(),
                It.IsAny<bool?>()))
            .ReturnsAsync(new InternalListResponse<Lot>((int)HttpStatusCode.InternalServerError, "Error"));

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(SimpleFilterQuery)
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        _machineCachingServiceMock.VerifyAll();
        _machineCachingServiceMock.VerifyNoOtherCalls();

        _materialDataHandlerHttpClientMock.VerifyAll();
        _materialDataHandlerHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetMaterialLots_With_Filter_Should_Return_An_Erroneous_Response_If_GetLotCount_Returns_500()
    {
        // Arrange
        var executor = await InitializeExecutor();

        using var fakeLocalTimeZone = new FakeLocalTimeZone(TimeZoneInfo.FindSystemTimeZoneById("US/Eastern"));

        _machineCachingServiceMock
            .Setup(m => m.GetMachinesAsInternalListResponse(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalListResponse<Machine>(new List<Machine>
            {
                new() { MachineId = MachineId }
            }));

        var materialLots = new List<Lot>();

        for (var i = 0; i < 15; i++)
        {
            materialLots.Add(MaterialLotMock.GenerateMaterialLot(MachineId, $"WIND{i}001"));
        }

        _materialDataHandlerHttpClientMock
            .Setup(m => m.FindLots(
                It.IsAny<CancellationToken>(),
                It.IsAny<string>(),
                MachineId,
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<bool?>(),
                15,
                It.IsAny<TypeOfMaterial?>(),
                It.IsAny<bool?>()))
            .ReturnsAsync(new InternalListResponse<Lot>(materialLots));

        _materialDataHandlerHttpClientMock
            .Setup(m => m.GetLotCount(It.IsAny<CancellationToken>(), MachineId))
            .ReturnsAsync(new InternalItemResponse<int>((int)HttpStatusCode.InternalServerError, "Exception"));

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(SimpleFilterQuery)
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        _machineCachingServiceMock.VerifyAll();
        _machineCachingServiceMock.VerifyNoOtherCalls();

        _materialDataHandlerHttpClientMock.VerifyAll();
        _materialDataHandlerHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task
        GetMaterialLots_With_Filter_Should_Return_A_New_CollectionSegment_If_GetMachineIdsByFilter_Returns_An_Empty_List()
    {
        // Arrange
        var executor = await InitializeExecutor();

        using var fakeLocalTimeZone = new FakeLocalTimeZone(TimeZoneInfo.FindSystemTimeZoneById("US/Eastern"));

        _machineCachingServiceMock
            .Setup(m => m.GetMachinesAsInternalListResponse(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InternalListResponse<Machine>(new List<Machine>()));

        // Act
        var request = QueryRequestBuilder
            .New()
            .SetQuery(SimpleFilterQuery)
            .AddRoleClaims("go-general")
            .Create();
        await using var result = await executor.ExecuteAsync(request);

        // Assert
        result.ToJson().MatchSnapshot();

        _machineCachingServiceMock.VerifyAll();
        _machineCachingServiceMock.VerifyNoOtherCalls();
    }

    private async Task<IRequestExecutor> InitializeExecutor()
    {
        var machineService =
            new MachineService(_machineCachingServiceMock.Object, new Mock<ILogger<MachineService>>().Object);

        var services = new ServiceCollection()
            .AddSingleton(_materialDataHandlerHttpClientMock.Object)
            .AddSingleton(_machineCachingServiceMock.Object)
            .AddSingleton(_materialLotCacheDataLoaderLoggerMock.Object)
            .AddSingleton(_materialLotsCacheDataLoaderLoggerMock.Object)
            .AddSingleton<IMachineService>(machineService)
            .AddLogging()
            .AddGraphQLServer()
            .AddAuthorization()
            .ModifyOptions(t => t.StrictValidation = false)
            .AddQueryType(q => q.Name("Query"))
            .AddType<FrameworkAPI.Queries.MaterialLotQuery>()
            .AddType<ExtrusionProducedRoll>()
            .AddSorting()
            .AddFiltering();

        var requestBuilder = await services.BuildRequestExecutorAsync();
        return requestBuilder;
    }
}