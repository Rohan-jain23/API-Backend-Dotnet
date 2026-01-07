using System;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.DataLoaders;
using FrameworkAPI.Schema.Misc;
using FrameworkAPI.Services.Interfaces;
using HotChocolate;
using HotChocolate.Types;
using WuH.Ruby.Common.Core;
using WuH.Ruby.MachineSnapShooter.Client;

namespace FrameworkAPI.Schema.MaterialLot;

/// <summary>
/// Generic interface for produced job entities of all machine families.
/// </summary>
[InterfaceType]
public abstract class MaterialLot(WuH.Ruby.MaterialDataHandler.Client.Models.Lot.Lot materialLot)
{
    /// <summary>
    /// Unique identifier of the material lot
    /// (All material lots produced on WuH machines are identified by a UID:
    /// The UID contains 5 characters indicating the manufacturer,
    /// 6 digits for the commissioning number, UTC time in the format "yyyymmddhhmmss"
    /// and 2 digits for roll number in roll set).
    /// [Source: MaterialDataHandler]
    /// </summary>
    public string MaterialLotId { get; set; } = materialLot.GeneralProperties.Id;

    /// <summary>
    /// The machine this material lot was produced on (usually WuH equipment number, like: "EQ12345").
    /// [Source: MaterialDataHandler]
    /// </summary>
    public string MachineId { get; set; } = materialLot.GeneralProperties.MachineId;

    /// <summary>
    /// Quantity of product contained within the material lot (e.g. length of the roll).
    /// [Source: MaterialDataHandler]
    /// </summary>
    public async Task<NumericValue> Quantity(
        MachineMetaDataBatchDataLoader machineMetaDataBatchDataLoader,
        [Service] IMachineMetaDataService machineMetaDataService,
        [Service] IUnitService unitService,
        string machineId,
        CancellationToken cancellationToken)
    {
        return new NumericValue(
            materialLot.GeneralProperties.Quantity,
            await GetUnit(
                machineMetaDataBatchDataLoader,
                machineMetaDataService,
                unitService,
                machineId,
                cancellationToken));
    }

    /// <summary>
    /// ID of the job for which this lot was/is produced
    /// [Source: MachineSnapshots]
    /// </summary>
    ///
    public async Task<string> JobId(
        SnapshotValuesWithLongestDurationBatchDataLoader dataLoader,
        [Service] IMachineSnapshotService machineSnapshotService,
        [Service] IMachineTimeService machineTimeService,
        CancellationToken cancellationToken)
    {
        var longestSnapshotValue = new LongestSnapshotValue(
            SnapshotColumnIds.JobId,
            materialLot.GeneralProperties.MachineId,
            materialLot.GeneralProperties.StartTime,
            materialLot.GeneralProperties.EndTime
        );

        return (
            await longestSnapshotValue.Value(
                dataLoader,
                machineSnapshotService,
                machineTimeService,
                cancellationToken))!;
    }

    /// <summary>
    /// Start timestamp of production in UTC (machine time).
    /// [Source: MaterialDataHandler]
    /// </summary>
    public DateTime StartTime { get; set; } = materialLot.GeneralProperties.StartTime;

    /// <summary>
    /// End timestamp of production in UTC (machine time) | This is 'null' if the roll is currently produced.
    /// [Source: MaterialDataHandler]
    /// </summary>
    public DateTime? EndTime { get; set; } = materialLot.GeneralProperties.EndTime;

    /// <summary>
    /// Creates an instance.
    /// </summary>
    /// <param name="materialLot">The material lot.</param>
    /// <returns>A <see cref="MaterialLot"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when creating a material lot is not supported for the material class of the <paramref name="materialLot"/>.</exception>
    internal static MaterialLot? CreateInstance(WuH.Ruby.MaterialDataHandler.Client.Models.Lot.Lot? materialLot)
    {
        if (materialLot is null) return null;

        return materialLot.MaterialClass switch
        {
            WuH.Ruby.MaterialDataHandler.Client.Enums.Lot.TypeOfMaterial.ExtrudedRoll => new ExtrusionProducedRoll(
                materialLot),
            WuH.Ruby.MaterialDataHandler.Client.Enums.Lot.TypeOfMaterial.PrintedRoll => new PrintingProducedRoll(
                materialLot),
            _ => throw new ArgumentException(
                $"Creating a material lot is not supported for the material class '{materialLot.MaterialClass}'.")
        };
    }

    private static async Task<string?> GetUnit(
        MachineMetaDataBatchDataLoader machineMetaDataBatchDataLoader,
        [Service] IMachineMetaDataService machineMetaDataService,
        [Service] IUnitService unitService,
        string machineId,
        CancellationToken cancellationToken = default)
    {
        var machineMetadata =
            await machineMetaDataService.GetMachineMetadata(
                machineMetaDataBatchDataLoader,
                machineId,
                VariableIdentifier.JobQuantityActualInSecondUnit,
                cancellationToken);

        return unitService.GetSiUnit(machineMetadata);
    }
}