using System;

namespace FrameworkAPI.Schema.Misc;

/// <summary>
/// Production status of the machine at a certain time in a trend.
/// Each machine type defines its own status and assigns it to a generic status category.
/// The production status might be changed automatically by RUBY or a user.
/// </summary>
public class ProductionStatusTrendItem(DateTime time, int statusId, ProductionStatusCategory statusCategory)
{
    /// <summary>
    /// The time.
    /// </summary>
    public DateTime Time { get; set; } = time;

    /// <summary>
    /// Unique id for this status.
    /// </summary>
    public int Id { get; set; } = statusId;

    /// <summary>
    /// Generic status category which this status is assigned to.
    /// </summary>
    public ProductionStatusCategory Category { get; set; } = statusCategory;
}