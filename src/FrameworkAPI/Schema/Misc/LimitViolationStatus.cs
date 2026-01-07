namespace FrameworkAPI.Schema.Misc;

/// <summary>
/// Status of the violations of parameter limits monitored by RUBY Gain.
/// </summary>
public enum LimitViolationStatus
{
    /// <summary>
    /// RUBY Gain was not active.
    /// </summary>
    Uncertain,

    /// <summary>
    /// RUBY Gain did not monitor any limit violations.
    /// </summary>
    NoViolations,

    /// <summary>
    /// RUBY Gain monitored at least one limit violations.
    /// </summary>
    Violations
}