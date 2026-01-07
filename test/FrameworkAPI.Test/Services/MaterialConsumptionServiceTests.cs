using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FrameworkAPI.DataLoaders;
using FrameworkAPI.Models;
using FrameworkAPI.Services;
using FrameworkAPI.Services.Interfaces;
using FrameworkAPI.Test.Services.Helpers;
using Moq;
using WuH.Ruby.Common.Core;
using WuH.Ruby.MachineSnapShooter.Client;
using WuH.Ruby.MachineSnapShooter.Client.Models;
using Xunit;

namespace FrameworkAPI.Test.Services;

public class MaterialConsumptionServiceTests
{
    private const string ArbitraryMachineId = "EQ12345";
    private const string ArbitraryKeyColumnId1 = "Extrusion.ExtruderA.Settings.Component1.MaterialName";
    private const string ArbitraryKeyColumnId2 = "Extrusion.ExtruderB.Settings.Component1.MaterialName";
    private const string ArbitraryValueColumnId1 = "Extrusion.ExtruderA.MaterialConsumption.Component1";
    private const string ArbitraryValueColumnId2 = "Extrusion.ExtruderB.MaterialConsumption.Component1";
    private const string ArbitraryMaterialName1 = "Material1";
    private const string ArbitraryMaterialName2 = "Material2";
    private const string ArbitraryMaterialName3 = "Material3";
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private static readonly TimeRange ArbitraryTimeRange = new(DateTime.UnixEpoch, DateTime.UnixEpoch + TimeSpan.FromHours(1));
    private readonly List<TimeRange> _arbitraryTimeRanges = [ArbitraryTimeRange];
    private readonly Mock<IMachineSnapshotService> _machineSnapshotService = new();
    private readonly Mock<IMachineSnapshotHttpClient> _machineSnapshotsHttpClientMock = new();
    private readonly SnapshotGroupedSumBatchDataLoader _groupedSumBatchDataLoader;
    private readonly MaterialConsumptionService _subject;

    public MaterialConsumptionServiceTests()
    {
        _subject = new MaterialConsumptionService(_machineSnapshotService.Object);
        var delayedBatchScheduler = new DelayedBatchScheduler();
        _groupedSumBatchDataLoader = new SnapshotGroupedSumBatchDataLoader(
            _machineSnapshotsHttpClientMock.Object,
            delayedBatchScheduler);
    }

    [Fact]
    public async Task GetRawMaterialConsumptionByMaterial_Calculates_The_Material_Consumption_For_Existent_Extruder_And_Components()
    {
        // Arrange
        var groupedSums1 = new GroupedSumByIdentifier() {
            { ArbitraryMaterialName1, 20.0 },
            { ArbitraryMaterialName2, 20.0 },
            { ArbitraryMaterialName3, 15.0 },
        };

        var groupedSums2 = new GroupedSumByIdentifier() {
            { ArbitraryMaterialName1, 30.0 },
            { ArbitraryMaterialName2, 0.0 },
            { ArbitraryMaterialName3, 15.0 },
        };

        _machineSnapshotService.Setup(m => m.GetGroupedSum(
            _groupedSumBatchDataLoader,
            ArbitraryMachineId,
            It.IsAny<GroupAssignment>(),
            It.IsAny<List<TimeRange>>(),
            It.IsAny<CancellationToken>()
        ))
            .ReturnsAsync((
                SnapshotGroupedSumBatchDataLoader _,
                string _,
                GroupAssignment groupAssignment,
                List<TimeRange> _,
                CancellationToken _) =>
            {
                if (groupAssignment.Equals(new GroupAssignment(ArbitraryKeyColumnId1, ArbitraryValueColumnId1)))
                    return new DataResult<GroupedSumByIdentifier>(groupedSums1, null);
                if (groupAssignment.Equals(new GroupAssignment(ArbitraryKeyColumnId2, ArbitraryValueColumnId2)))
                    return new DataResult<GroupedSumByIdentifier>(groupedSums2, null);

                return new DataResult<GroupedSumByIdentifier>(value: null, exception: null);
            });

        // Act
        var result = await _subject.GetRawMaterialConsumptionByMaterial(
            _groupedSumBatchDataLoader,
            ArbitraryMachineId,
            _arbitraryTimeRanges,
            _cancellationTokenSource.Token);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        (await result![ArbitraryMaterialName1].Value(_cancellationTokenSource.Token)).Should().Be(50);
        (await result[ArbitraryMaterialName3].Value(_cancellationTokenSource.Token)).Should().Be(30);
        (await result[ArbitraryMaterialName2].Value(_cancellationTokenSource.Token)).Should().Be(20);

        _machineSnapshotService.Verify(
            v => v.GetGroupedSum(
            _groupedSumBatchDataLoader,
            ArbitraryMachineId,
            It.IsAny<GroupAssignment>(),
            It.IsAny<List<TimeRange>>(),
            It.IsAny<CancellationToken>()),
            Times.Exactly(77));
        _machineSnapshotService.Verify(
            v => v.GetGroupedSum(
            _groupedSumBatchDataLoader,
            ArbitraryMachineId,
            new(ArbitraryKeyColumnId1, ArbitraryValueColumnId1),
            It.IsAny<List<TimeRange>>(),
            It.IsAny<CancellationToken>()),
            Times.Exactly(1));
        _machineSnapshotService.Verify(
            v => v.GetGroupedSum(
            _groupedSumBatchDataLoader,
            ArbitraryMachineId,
            new(ArbitraryKeyColumnId2, ArbitraryValueColumnId2),
            It.IsAny<List<TimeRange>>(),
            It.IsAny<CancellationToken>()),
            Times.Exactly(1));
    }

    [Fact]
    public async Task GetRawMaterialConsumptionByMaterial_Returns_Correct_Order()
    {
        // Arrange
        var groupedSums1 = new GroupedSumByIdentifier() {
            { ArbitraryMaterialName1, 20.0 },
            { ArbitraryMaterialName2, 20.0 },
            { ArbitraryMaterialName3, 15.0 },
        };

        var groupedSums2 = new GroupedSumByIdentifier() {
            { ArbitraryMaterialName1, 30.0 },
            { ArbitraryMaterialName2, 0.0 },
            { ArbitraryMaterialName3, 15.0 },
        };

        _machineSnapshotService.Setup(m => m.GetGroupedSum(
            _groupedSumBatchDataLoader,
            ArbitraryMachineId,
            It.IsAny<GroupAssignment>(),
            It.IsAny<List<TimeRange>>(),
            It.IsAny<CancellationToken>()
        ))
            .ReturnsAsync((
                SnapshotGroupedSumBatchDataLoader _,
                string _,
                GroupAssignment groupAssignment,
                List<TimeRange> _,
                CancellationToken _) =>
            {
                if (groupAssignment.Equals(new GroupAssignment(ArbitraryKeyColumnId1, ArbitraryValueColumnId1)))
                    return new DataResult<GroupedSumByIdentifier>(groupedSums1, null);
                if (groupAssignment.Equals(new GroupAssignment(ArbitraryKeyColumnId2, ArbitraryValueColumnId2)))
                    return new DataResult<GroupedSumByIdentifier>(groupedSums2, null);

                return new DataResult<GroupedSumByIdentifier>(value: null, exception: null);
            });

        // Act
        var result = await _subject.GetRawMaterialConsumptionByMaterial(
            _groupedSumBatchDataLoader,
            ArbitraryMachineId,
            _arbitraryTimeRanges,
            _cancellationTokenSource.Token);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result!.ElementAt(0).Key.Should().Be(ArbitraryMaterialName1);
        result!.ElementAt(1).Key.Should().Be(ArbitraryMaterialName3);
        result!.ElementAt(2).Key.Should().Be(ArbitraryMaterialName2);
    }

    [Fact]
    public async Task GetRawMaterialConsumptionByMaterial_Throws_When_GetGroupedSums_Fails_Once()
    {
        // Arrange
        var groupedSums1 = new GroupedSumByIdentifier
        {
            { ArbitraryMaterialName1, 20.0 },
            { ArbitraryMaterialName2, 20.0 },
            { ArbitraryMaterialName3, 15.0 }
        };

        _machineSnapshotService.Setup(m => m.GetGroupedSum(
                _groupedSumBatchDataLoader,
                ArbitraryMachineId,
                It.IsAny<GroupAssignment>(),
                It.IsAny<List<TimeRange>>(),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync((
                SnapshotGroupedSumBatchDataLoader _,
                string _,
                GroupAssignment groupAssignment,
                List<TimeRange> _,
                CancellationToken _) =>
            {
                if (groupAssignment.Equals(new GroupAssignment(ArbitraryKeyColumnId1, ArbitraryValueColumnId1)))
                    return new DataResult<GroupedSumByIdentifier>(groupedSums1, exception: null);
                if (groupAssignment.Equals(new GroupAssignment(ArbitraryKeyColumnId2, ArbitraryValueColumnId2)))
                    return new DataResult<GroupedSumByIdentifier>(value: null, new Exception("Error"));

                return new DataResult<GroupedSumByIdentifier>(value: null, exception: null);
            });

        // Act
        var act = () => _subject.GetRawMaterialConsumptionByMaterial(
            _groupedSumBatchDataLoader,
            ArbitraryMachineId,
            _arbitraryTimeRanges,
            _cancellationTokenSource.Token);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Error");
    }

    [Fact]
    public async Task GetRawMaterialConsumptionByMaterial_Calculates_The_Material_Consumption_For_Existent_Extruder_And_Components_And_Ignores_Zero_Values()
    {
        // Arrange
        var groupedSums1 = new GroupedSumByIdentifier() {
            { ArbitraryMaterialName1, 0.0 },
            { ArbitraryMaterialName2, 0.0 },
            { ArbitraryMaterialName3, 15.0 },
        };

        var groupedSums2 = new GroupedSumByIdentifier() {
            { ArbitraryMaterialName1, 0.0 },
            { ArbitraryMaterialName2, 0.0 },
            { ArbitraryMaterialName3, 15.0 },
        };

        _machineSnapshotService.Setup(m => m.GetGroupedSum(
            _groupedSumBatchDataLoader,
            ArbitraryMachineId,
            It.IsAny<GroupAssignment>(),
            It.IsAny<List<TimeRange>>(),
            It.IsAny<CancellationToken>()
        ))
            .ReturnsAsync((
                SnapshotGroupedSumBatchDataLoader _,
                string _,
                GroupAssignment groupAssignment,
                List<TimeRange> _,
                CancellationToken _) =>
            {
                if (groupAssignment.Equals(new GroupAssignment(ArbitraryKeyColumnId1, ArbitraryValueColumnId1)))
                    return new DataResult<GroupedSumByIdentifier>(groupedSums1, null);
                if (groupAssignment.Equals(new GroupAssignment(ArbitraryKeyColumnId2, ArbitraryValueColumnId2)))
                    return new DataResult<GroupedSumByIdentifier>(groupedSums2, null);

                return new DataResult<GroupedSumByIdentifier>(null, null);
            });

        // Act
        var result = await _subject.GetRawMaterialConsumptionByMaterial(
            _groupedSumBatchDataLoader,
            ArbitraryMachineId,
            _arbitraryTimeRanges,
            _cancellationTokenSource.Token);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        (await result![ArbitraryMaterialName3].Value(_cancellationTokenSource.Token)).Should().Be(30);

        _machineSnapshotService.Verify(
            v => v.GetGroupedSum(
            _groupedSumBatchDataLoader,
            ArbitraryMachineId,
            It.IsAny<GroupAssignment>(),
            It.IsAny<List<TimeRange>>(),
            It.IsAny<CancellationToken>()),
            Times.Exactly(77));
        _machineSnapshotService.Verify(
            v => v.GetGroupedSum(
            _groupedSumBatchDataLoader,
            ArbitraryMachineId,
            new(ArbitraryKeyColumnId1, ArbitraryValueColumnId1),
            It.IsAny<List<TimeRange>>(),
            It.IsAny<CancellationToken>()),
            Times.Exactly(1));
        _machineSnapshotService.Verify(
            v => v.GetGroupedSum(
            _groupedSumBatchDataLoader,
            ArbitraryMachineId,
            new(ArbitraryKeyColumnId2, ArbitraryValueColumnId2),
            It.IsAny<List<TimeRange>>(),
            It.IsAny<CancellationToken>()),
            Times.Exactly(1));
    }

    [Fact]
    public async Task GetRawMaterialConsumptionByMaterial_Returns_Empty_Dictionary_When_No_Known_Columns_Exist()
    {
        // Arrange
        _machineSnapshotService.Setup(m => m.GetGroupedSum(
            _groupedSumBatchDataLoader,
            ArbitraryMachineId,
            It.IsAny<GroupAssignment>(),
            It.IsAny<List<TimeRange>>(),
            It.IsAny<CancellationToken>()
        ))
            .ReturnsAsync(new DataResult<GroupedSumByIdentifier>(null, null));

        // Act
        var result = await _subject.GetRawMaterialConsumptionByMaterial(
            _groupedSumBatchDataLoader,
            ArbitraryMachineId,
            _arbitraryTimeRanges,
            _cancellationTokenSource.Token);

        await Task.WhenAll(result!.Values.Select(numericValue => numericValue.Value(_cancellationTokenSource.Token)));

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(0);
    }
}