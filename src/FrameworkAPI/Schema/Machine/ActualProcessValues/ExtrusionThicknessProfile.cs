using System;
using System.Collections.Generic;
using FrameworkAPI.Schema.Misc;

namespace FrameworkAPI.Schema.Machine.ActualProcessValues;

/// <summary>
/// Current deviation of produced thickness as a profile over the produced width.
/// </summary>
public class ExtrusionThicknessProfile(
    ExtrusionThicknessMeasurementType? type,
    IDictionary<int, double>? dataPoints,
    string? xAxisUnit,
    NumericValue? meanValue,
    NumericValue? twoSigma,
    bool? isControllerOn,
    IDictionary<int, double>? controlElements,
    DateTime? timestamp)
{

    /// <summary>
    /// Describes which of the different thickness measurement systems is used.
    /// [Source: FrameworkAPI]
    /// </summary>
    public ExtrusionThicknessMeasurementType? Type { get; set; } = type;

    /// <summary>
    /// The current deviation of produced thickness as a profile over the produced width.
    /// This dictionary contains the data points that describe the thickness profile:
    /// key => position (X axis)
    /// value => deviation from mean at position in % (Y axis)
    /// (the data points on cast film machines are down sampled to 480)
    /// [Source: ProcessData]
    /// </summary>
    public IDictionary<int, double>? DataPoints { get; set; } = dataPoints;

    /// <summary>
    /// Number of data points to expect.
    /// (is 360 on blow film and 480 on cast film lines).
    /// [Source: ProcessData]
    /// </summary>
    public int DataPointsCount { get; set; } = dataPoints?.Count ?? 0;

    /// <summary>
    /// Unit of the position values.
    /// (is '°' on blow film and '' on cast film lines).
    /// [Source: MetaDataHandler]
    /// </summary>
    public string? XAxisUnit { get; set; } = xAxisUnit;

    /// <summary>
    /// Mean value of the profile measurement.
    /// [Source: ProcessData, MetaDataHandler]
    /// </summary>
    public NumericValue? MeanValue { get; set; } = meanValue;

    /// <summary>
    /// 2-sigma deviation from the mean value of the profile measurement (in %).
    /// [Source: ProcessData, MetaDataHandler]
    /// </summary>
    public NumericValue? TwoSigma { get; set; } = twoSigma;

    /// <summary>
    /// Is true, if the controller that is using this thickness measurement is turned on.
    /// - Logic for 'Primary': Thickness gauge is on AND profile control mode is on but not 'MDO'.
    /// - Logic for 'MdoWinderA': Winder A has contact pressure AND profile control mode is 'MDO'.
    /// - Logic for 'MdoWinderB': Winder B has contact pressure AND profile control mode is 'MDO'.
    /// [Source: ProcessData]
    /// </summary>
    public bool? IsControllerOn { get; set; } = isControllerOn;

    /// <summary>
    /// The thickness profile can be controlled by different control elements over the produced width
    /// (this is 'null', if the 'ExtrusionThicknessMeasurementType' is not 'Primary' as the controlling of the other profiles is more complex).
    /// This dictionary contains the data points that describe the control elements profile:
    /// key => position (X axis)
    /// value => duty cycle of the heating element from 0 to 100 % (Y axis)
    /// (the data points on cast film machines are down sampled to 480)
    /// [Source: ProcessData]
    /// </summary>
    public IDictionary<int, double>? ControlElements { get; set; } = controlElements;

    /// <summary>
    /// Timestamp of the sampling of the profile.
    /// </summary>
    public DateTime? Timestamp { get; set; } = timestamp;
}