namespace FrameworkAPI.Schema.Misc;

/// <summary>
/// Machine departments supported by RUBY. A machine department does not necessary relate to a WuH business unit.
/// It's more related to the type of product.
/// </summary>
public enum MachineDepartment
{
    /// <summary>
    /// Film extrusion lines (blow and cast film).
    /// </summary>
    Extrusion,

    /// <summary>
    /// Printing presses (flexo and gravure).
    /// </summary>
    Printing,

    /// <summary>
    /// Paper sack machines (tuber and bottomers).
    /// </summary>
    PaperSack,

    /// <summary>
    /// All other machines.
    /// </summary>
    Other
}