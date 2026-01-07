using PhysicalAssetDataHandler.Client.Models.Dtos;

namespace FrameworkAPI.Schema.PhysicalAsset;

/// <summary>
/// A physical asset usage counter.
/// </summary>
/// <param name="physicalAssetUsageCounterDto">The physical asset usage counter dto.</param>
public class PhysicalAssetUsageCounter(PhysicalAssetUsageCounterDto physicalAssetUsageCounterDto)
{
    /// <summary>
    /// Current value of the counter.
    /// [Source: PhysicalAsset]
    /// </summary>
    public long Current { get; set; } = physicalAssetUsageCounterDto.Current;

    /// <summary>
    /// Value of the counter at the start of the last cleaning.
    /// [Source: PhysicalAsset]
    /// </summary>
    public long? AtLastCleaning { get; set; } = physicalAssetUsageCounterDto.AtLastCleaning;

    /// <summary>
    /// Value of the counter since the last cleaning.
    /// [Source: PhysicalAsset]
    /// </summary>
    public long? SinceLastCleaning { get; set; } = physicalAssetUsageCounterDto.SinceLastCleaning;

    /// <summary>
    /// Value of the the interval the physical asset should get cleaned..
    /// [Source: PhysicalAssetSettings]
    /// </summary>
    public long CleaningInterval { get; set; } = physicalAssetUsageCounterDto.CleaningInterval;

    /// <summary>
    /// Flag if the the physical assset usage counter is greater then the cleaning interval.
    /// [Source: PhysicalAsset]
    /// </summary>
    public bool CleaningIntervalExceeded { get; set; } = physicalAssetUsageCounterDto.CleaningIntervalExceeded;

    /// <summary>
    /// The unit of the counter values.
    /// [Source: PhysicalAsset]
    /// </summary>
    public string Unit { get; set; } = physicalAssetUsageCounterDto.Unit;
}