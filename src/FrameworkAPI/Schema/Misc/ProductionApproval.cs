using System;

namespace FrameworkAPI.Schema.Misc;

/// <summary>
/// Information about the production approval event of this job.
/// The production approval can be performed on the OperatorUI (usually by the shift supervisor).
/// </summary>
public class ProductionApprovalEvent(DateTime timestamp, string signature)
{
    /// <summary>
    /// Associated date of the production approval event.
    /// </summary>
    public DateTime Timestamp { get; set; } = timestamp;

    /// <summary>
    /// Signature of the person who approved the production (usually shift supervisor).
    /// This string is the output of the Angular canvas component.
    /// </summary>
    public string Signature { get; set; } = signature;
}
