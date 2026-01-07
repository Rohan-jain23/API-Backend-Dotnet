namespace FrameworkAPI.Schema.ProductGroup;

/// <summary>
/// Sort option for product groups.
/// </summary>
public enum ProductGroupSortOption
{
    /// <summary>
    /// No sorting.
    /// </summary>
    None,

    /// <summary>
    /// Sort by id ascending.
    /// </summary>
    IdAscending,

    /// <summary>
    /// Sort by id descending.
    /// </summary>
    IdDescending,

    /// <summary>
    /// Sort by friendly name ascending.
    /// </summary>
    FriendlyNameAscending,

    /// <summary>
    /// Sort by friendly name descending.
    /// </summary>
    FriendlyNameDescending,

    /// <summary>
    /// Sort by first production date ascending.
    /// </summary>
    FirstProductionDateAscending,

    /// <summary>
    /// Sort by first production date descending.
    /// </summary>
    FirstProductionDateDescending,

    /// <summary>
    /// Sort by last production date ascending.
    /// </summary>
    LastProductionDateAscending,

    /// <summary>
    /// Sort by last production date descending.
    /// </summary>
    LastProductionDateDescending

}