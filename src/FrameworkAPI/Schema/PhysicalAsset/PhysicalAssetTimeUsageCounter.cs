using PhysicalAssetDataHandler.Client.Models.Dtos;

namespace FrameworkAPI.Schema.PhysicalAsset;

/// <summary>
/// A physical asset time usage counter.
/// </summary>
/// <param name="physicalAssetTimeUsageCounterDto">The physical asset usage counter dto.</param>
public class PhysicalAssetTimeUsageCounter(PhysicalAssetTimeUsageCounterDto physicalAssetTimeUsageCounterDto)
{
    /// <summary>
    /// Current value of the counter.
    /// [Source: PhysicalAsset]
    /// </summary>
    public long Current { get; set; } = physicalAssetTimeUsageCounterDto.Current;

    /// <summary>
    /// Value of the counter at the start of the last cleaning.
    /// [Source: PhysicalAsset]
    /// </summary>
    public long? AtLastCleaning { get; set; } = physicalAssetTimeUsageCounterDto.AtLastCleaning;

    /// <summary>
    /// Value of the counter since the last cleaning.
    /// [Source: PhysicalAsset]
    /// </summary>
    public long? SinceLastCleaning { get; set; } = physicalAssetTimeUsageCounterDto.SinceLastCleaning;

    /// <summary>
    /// The unit of the counter values.
    /// [Source: PhysicalAsset]
    /// </summary>
    public string Unit { get; set; } = physicalAssetTimeUsageCounterDto.Unit;
}