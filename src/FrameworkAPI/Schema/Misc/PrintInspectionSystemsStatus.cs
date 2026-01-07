namespace FrameworkAPI.Schema.Misc;

/// <summary>
/// Status of print inspection systems.
/// </summary>
public class PrintInspectionSystemsStatus(
    bool isDefectCheckActive,
    bool hasDefectCheckResults,
    bool isBarcodeCheckActive,
    bool hasBarcodeCheckResults,
    bool isRgbLabCheckActive,
    bool hasRgbLabCheckResults)
{

    /// <summary>
    /// True, if the DEFECT-CHECK system is activated on the machine.
    /// </summary>
    public bool IsDefectCheckActive { get; set; } = isDefectCheckActive;

    /// <summary>
    /// True, if the DEFECT-CHECK system detected activities that might be interesting for the user.
    /// </summary>
    public bool HasDefectCheckResults { get; set; } = hasDefectCheckResults;

    /// <summary>
    /// True, if the BARCODE-CHECK system is activated on the machine.
    /// </summary>
    public bool IsBarcodeCheckActive { get; set; } = isBarcodeCheckActive;

    /// <summary>
    /// True, if the BARCODE-CHECK system detected activities that might be interesting for the user.
    /// </summary>
    public bool HasBarcodeCheckResults { get; set; } = hasBarcodeCheckResults;

    /// <summary>
    /// True, if the RGB-LAB-CHECK system is activated on the machine.
    /// </summary>
    public bool IsRgbLabCheckActive { get; set; } = isRgbLabCheckActive;

    /// <summary>
    /// True, if the RGB-LAB-CHECK system detected activities that might be interesting for the user.
    /// </summary>
    public bool HasRgbLabCheckResults { get; set; } = hasRgbLabCheckResults;
}