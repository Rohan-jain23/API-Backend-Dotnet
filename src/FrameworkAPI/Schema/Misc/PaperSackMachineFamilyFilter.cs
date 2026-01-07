namespace FrameworkAPI.Schema.Misc;

/// <summary>
/// Filter for paper sack machine families.
/// </summary>
public enum PaperSackMachineFamilyFilter
{
    /// <summary>
    /// Only paper sack bottomers.
    /// </summary>
    Bottomer,

    /// <summary>
    /// Only paper sack tubers.
    /// </summary>
    Tuber,

    /// <summary>
    /// Paper sack bottomers and paper sack tubers.
    /// </summary>
    Both
}