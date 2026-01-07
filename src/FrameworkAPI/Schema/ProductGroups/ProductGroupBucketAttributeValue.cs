namespace FrameworkAPI.Schema.ProductGroup;

/// <summary>
/// Some attributes are based on numeric variables, where nearly equal values should be considered as one (bucket) value.
/// </summary>
public class ProductGroupBucketAttributeValue(string? formattedValue, string unit)
{
    /// <summary>
    /// The string representation of the buckets value (like '175-225').
    /// </summary>
    public string? FormattedValue { get; set; } = formattedValue;

    /// <summary>
    /// The unit of the 'FormattedValue'.
    /// </summary>
    public string Unit { get; set; } = unit;
}