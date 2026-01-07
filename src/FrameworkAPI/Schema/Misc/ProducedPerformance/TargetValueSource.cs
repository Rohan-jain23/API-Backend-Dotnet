namespace FrameworkAPI.Schema.Misc;

/// <summary>
/// Different sources for target speed values.
/// </summary>
public enum TargetValueSource
{
    /// <summary>
    /// Job-specific value defined/corrected in RUBY (via OperatorUI or Track)
    /// </summary>
    JobCorrection,

    /// <summary>
    /// Product(group)-specific setting (via Track)
    /// </summary>
    ProductGroup,

    /// <summary>
    /// Job-specific value defined in customer system (via Connect 4 Flow)
    /// </summary>
    ProductionRequest,

    /// <summary>
    /// Value entered in ProControl (ProcessData)
    /// </summary>
    Machine,

    /// <summary>
    /// Default setting (via Track section in Admin)
    /// </summary>
    AdminSetting,
}
