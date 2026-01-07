namespace FrameworkAPI.Schema.Machine.ActualProcessValues;

/// <summary>
/// Describes which of the different thickness measurement systems is used.
/// </summary>
public enum ExtrusionThicknessMeasurementType
{
    /// <summary>
    /// The standard measurement.
    /// (Blow film: bubble)
    /// </summary>
    Primary,

    /// <summary>
    /// The measurement after the MDO before winding station A.
    /// </summary>
    MdoWinderA,

    /// <summary>
    /// The measurement after the MDO before winding station B.
    /// </summary>
    MdoWinderB
}