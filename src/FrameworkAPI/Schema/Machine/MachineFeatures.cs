using System.Collections.Generic;
using System.Linq;
using WuH.Ruby.MachineDataHandler.Client;

namespace FrameworkAPI.Schema.Machine;

public class MachineFeatures(IEnumerable<MachineFeature>? machineFeatures)
{
    private readonly List<MachineFeature>? _machineFeatures = machineFeatures?.ToList();

    /// <summary>
    /// Flag if machine has the AlarmHandling feature.
    /// </summary>
    public bool HasAlarmHandlingFeature =>
        HasFeature(_machineFeatures, "AlarmHandling");

    /// <summary>
    /// Version of the AlarmHandling feature if <see cref="HasAlarmHandlingFeature"/> is true, otherwise null.
    /// </summary>
    public int? AlarmHandlingFeatureVersion =>
        GetFeatureVersion(_machineFeatures, "AlarmHandling");

    /// <summary>
    /// Flag if machine has the ProcessData feature.
    /// </summary>
    public bool HasProcessDataFeature => HasFeature(_machineFeatures, "ProcessData");

    /// <summary>
    /// Version of the ProcessData feature if <see cref="HasProcessDataFeature"/> is true, otherwise null.
    /// </summary>
    public int? ProcessDataFeatureVersion => GetFeatureVersion(_machineFeatures, "ProcessData");

    /// <summary>
    /// Flag if machine has the ProductionPeriods feature.
    /// </summary>
    public bool HasProductionPeriodsFeature => HasFeature(_machineFeatures, "ProductionPeriods");

    /// <summary>
    /// Version of the ProductionPeriods feature if <see cref="HasProductionPeriodsFeature"/> is true, otherwise null.
    /// </summary>
    public int? ProductionPeriodsFeatureVersion => GetFeatureVersion(_machineFeatures, "ProductionPeriods");

    /// <summary>
    /// Flag if machine has the Material feature.
    /// </summary>
    public bool HasMaterialFeature => HasFeature(_machineFeatures, "Material");

    /// <summary>
    /// Version of the Material feature if <see cref="HasMaterialFeature"/> is true, otherwise null.
    /// </summary>
    public int? MaterialFeatureVersion => GetFeatureVersion(_machineFeatures, "Material");

    /// <summary>
    /// Flag if machine has the Messaging feature.
    /// </summary>
    public bool HasMessagingFeature => HasFeature(_machineFeatures, "Messaging");

    /// <summary>
    /// Version of the Messaging feature if <see cref="HasMessagingFeature"/> is true, otherwise null.
    /// </summary>
    public int? MessagingFeatureVersion => GetFeatureVersion(_machineFeatures, "Messaging");

    /// <summary>
    /// Flag if machine has the Check feature.
    /// </summary>
    public bool HasCheckFeature => HasFeature(_machineFeatures, "Check");

    /// <summary>
    /// Version of the Check feature if <see cref="HasCheckFeature"/> is true, otherwise null.
    /// </summary>
    public int? CheckFeatureVersion => GetFeatureVersion(_machineFeatures, "Check");

    /// <summary>
    /// Flag if machine has the DefectCheck feature.
    /// </summary>
    public bool HasDefectCheckFeature => HasFeature(_machineFeatures, "DefectCheck");

    /// <summary>
    /// Version of the DefectCheck feature if <see cref="HasDefectCheckFeature"/> is true, otherwise null.
    /// </summary>
    public int? DefectCheckFeatureVersion => GetFeatureVersion(_machineFeatures, "DefectCheck");

    /// <summary>
    /// Flag if machine has the BarcodeCheck feature.
    /// </summary>
    public bool HasBarcodeCheckFeature => HasFeature(_machineFeatures, "BarcodeCheck");

    /// <summary>
    /// Version of the BarcodeCheck feature if <see cref="HasBarcodeCheckFeature"/> is true, otherwise null.
    /// </summary>
    public int? BarcodeCheckFeatureVersion => GetFeatureVersion(_machineFeatures, "BarcodeCheck");

    /// <summary>
    /// Flag if machine has the PDFCheck feature.
    /// </summary>
    public bool HasPdfCheckFeature => HasFeature(_machineFeatures, "PDFCheck");

    /// <summary>
    /// Version of the PDFCheck feature if <see cref="HasPdfCheckFeature"/> is true, otherwise null.
    /// </summary>
    public int? PdfCheckFeatureVersion => GetFeatureVersion(_machineFeatures, "PDFCheck");

    /// <summary>
    /// Flag if machine has the PDFCheckPackageTransfer feature.
    /// </summary>
    public bool HasPdfCheckPackageTransferFeature => HasFeature(_machineFeatures, "PDFCheckPackageTransfer");

    /// <summary>
    /// Version of the PDFCheckPackageTransfer feature if <see cref="HasPdfCheckPackageTransferFeature"/> is true, otherwise null.
    /// </summary>
    public int? PdfCheckPackageTransferFeatureVersion => GetFeatureVersion(_machineFeatures, "PDFCheckPackageTransfer");

    /// <summary>
    /// Flag if machine has the BrowserOverlay feature.
    /// </summary>
    public bool HasBrowserOverlayFeature => HasFeature(_machineFeatures, "BrowserOverlay");

    /// <summary>
    /// Version of the BrowserOverlay feature if <see cref="HasBrowserOverlayFeature"/> is true, otherwise null.
    /// </summary>
    public int? BrowserOverlayFeatureVersion => GetFeatureVersion(_machineFeatures, "BrowserOverlay");

    /// <summary>
    /// Flag if machine has the Flow feature.
    /// </summary>
    public bool HasFlowFeature => HasFeature(_machineFeatures, "Flow");

    /// <summary>
    /// Version of the Flow feature if <see cref="HasFlowFeature"/> is true, otherwise null.
    /// </summary>
    public int? FlowFeatureVersion => GetFeatureVersion(_machineFeatures, "Flow");

    /// <summary>
    /// Flag if machine has the RgbLabCheck feature.
    /// </summary>
    public bool HasRgbLabCheckFeature => HasFeature(_machineFeatures, "RgbLabCheck");

    /// <summary>
    /// Version of the RgbLabCheck feature if <see cref="HasRgbLabCheckFeature"/> is true, otherwise null.
    /// </summary>
    public int? RgbLabCheckFeatureVersion => GetFeatureVersion(_machineFeatures, "RgbLabCheck");

    private static bool HasFeature(IEnumerable<MachineFeature>? machineFeatures, string featureName)
    {
        return machineFeatures?.Any(machineFeature => machineFeature.Name == featureName) == true;
    }

    private static int? GetFeatureVersion(IEnumerable<MachineFeature>? machineFeatures, string featureName)
    {
        return machineFeatures?.FirstOrDefault(machineFeature => machineFeature.Name == featureName)?.FeatureVersion;
    }
}