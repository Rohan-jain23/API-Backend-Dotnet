using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.DataLoaders;
using FrameworkAPI.Schema.Misc;
using FrameworkAPI.Services.Interfaces;
using WuH.Ruby.MachineSnapShooter.Client.Models;
using TimeRange = WuH.Ruby.Common.Core.TimeRange;

namespace FrameworkAPI.Services;

public class MaterialConsumptionService(IMachineSnapshotService machineSnapshotService) : IMaterialConsumptionService
{
    // Note:    As this service is a really high level custom one and currently only concerns the weight
    //          we decided to make this constant and can be even more performant (instead of getting the unit 77 times (worst-case)).
    //          Reminder:   This can be adapted in the future and get the units based on the cache service to allow material consumption 
    //                      to be able to support other units like for example lengths (m) or volumes (m³).
    private const string MaterialConsumptionUnit = "kg";
    private const char LastExtruderNumber = 'K';
    private const int MaximumNumberOfComponents = 7;

    public async Task<Dictionary<string, NumericValue>?> GetRawMaterialConsumptionByMaterial(
        SnapshotGroupedSumBatchDataLoader dataLoader,
        string machineId,
        IEnumerable<TimeRange> timeRanges,
        CancellationToken cancellationToken)
    {
        var groupedSumsWithUnitsByColumId =
            await GetGroupedSumsForAllMaterials(dataLoader, machineId, timeRanges.ToList(), cancellationToken);

        return groupedSumsWithUnitsByColumId
            .SelectMany(outer => outer.Value)
            .GroupBy(inner => inner.Key)
            .Select(group => new
            {
                group.Key,
                Sum = group.Sum(inner => inner.Value)
            })
            .Where(group => group.Sum != 0)
            .OrderByDescending(group => group.Sum)
            .ThenBy(group => group.Key)
            .ToDictionary(
                group => group.Key,
                group => new NumericValue(group.Sum, MaterialConsumptionUnit));
    }

    private async Task<IDictionary<string, GroupedSumByIdentifier>> GetGroupedSumsForAllMaterials(
        SnapshotGroupedSumBatchDataLoader dataLoader,
        string machineId,
        List<TimeRange> timeRanges,
        CancellationToken cancellationToken)
    {
        return await GetAllMaterialGroupAssignments()
            .ToObservable()
            .SelectMany(groupAssignment =>
                Observable.FromAsync(async ctFromAsync =>
                    {
                        var groupedSumsResult = await machineSnapshotService.GetGroupedSum(
                            dataLoader,
                            machineId,
                            groupAssignment,
                            timeRanges,
                            ctFromAsync);
                        return (
                            keyColumnId: groupAssignment.KeyColumnId,
                            groupedSums: groupedSumsResult.GetValueOrThrow());
                    }
                ))
            .Where(kvp => kvp.groupedSums != null)
            .ToDictionary(
                kvp => kvp.keyColumnId,
                kvp => kvp.groupedSums!)
            .ToTask(cancellationToken);
    }

    private static IEnumerable<GroupAssignment> GetAllMaterialGroupAssignments()
    {
        var groupAssignments = new List<GroupAssignment>();

        for (var extruderKey = 'A'; extruderKey <= LastExtruderNumber; extruderKey++)
        {
            for (var componentKey = 1; componentKey <= MaximumNumberOfComponents; componentKey++)
            {
                groupAssignments.Add(new(
                    $"Extrusion.Extruder{extruderKey}.Settings.Component{componentKey}.MaterialName",
                    $"Extrusion.Extruder{extruderKey}.MaterialConsumption.Component{componentKey}"));
            }
        }

        return groupAssignments;
    }
}