namespace FrameworkAPI.Schema.Misc;

/// <summary>
/// Machine family / Generic machine type.
/// </summary>
public enum MachineFamily
{
    /// <summary>
    /// Flexo printing press.
    /// </summary>
    FlexoPrint,

    /// <summary>
    /// Gravure printing press.
    /// </summary>
    GravurePrint,

    /// <summary>
    /// Blow film extrusion line.
    /// </summary>
    BlowFilm,

    /// <summary>
    /// Cast film extrusion line.
    /// </summary>
    CastFilm,

    /// <summary>
    /// Paper sack bottomer.
    /// </summary>
    PaperSackBottomer,

    /// <summary>
    /// Paper sack tuber.
    /// </summary>
    PaperSackTuber,

    /// <summary>
    /// All other machine families. Other machines can't be used as a machine filter. When it's used as filter, machines of all machine families will be selected.
    /// </summary>
    Other
}