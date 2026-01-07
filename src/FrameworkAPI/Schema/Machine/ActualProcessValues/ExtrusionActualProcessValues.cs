using System;
using FrameworkAPI.Schema.Misc;
using WuH.Ruby.MachineSnapShooter.Client;

namespace FrameworkAPI.Schema.Machine.ActualProcessValues;

/// <summary>
/// Values measured by sensors, quality measurements, actual values of machine settings, values calculated by PLC, ...
/// </summary>
public class ExtrusionActualProcessValues(DateTime? queryTimestamp, string machineId, MachineFamily machineFamily)
{
    private readonly DateTime? _queryTimestamp = queryTimestamp;
    private readonly string _machineId = machineId;
    private readonly MachineFamily _machineFamily = machineFamily;

    /// <summary>
    /// Current deviation of produced thickness from 2-sigma (in %).
    /// This is always the value from the currently most relevant thickness measurement (Primary/MDO/...).
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue TwoSigma()
        => new(SnapshotColumnIds.ExtrusionQualityActualValuesTwoSigma, _queryTimestamp, _machineId);

    /// <summary>
    /// Current film thickness.
    /// This is always the value from the currently most relevant thickness measurement (Primary/MDO/...).
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Thickness()
        => new(SnapshotColumnIds.ExtrusionFormatActualValuesThickness, _queryTimestamp, _machineId);

    /// <summary>
    /// Current status for the primary thickness gauge. True = On, False = Off.
    /// [Source: MachineSnapshot]
    /// </summary>
    public SnapshotValue<bool> ThicknessGaugeStatus()
        => new(SnapshotColumnIds.ExtrusionQualitySettingsIsThicknessGaugeOn, _queryTimestamp, _machineId);

    /// <summary>
    /// All profiles of the different thickness measurement systems.
    /// These profiles show the current deviation of produced thickness as a profile over the produced width.
    /// </summary>
    public ExtrusionThicknessProfiles ThicknessProfiles()
        => new(_queryTimestamp, _machineId, _machineFamily);

    /// <summary>
    /// Current width of the produced film.
    /// This is always the value from the currently most relevant width measurement (Primary/MDO/...).
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Width()
        => new(SnapshotColumnIds.ExtrusionFormatActualValuesWidth, _queryTimestamp, _machineId);

    /// <summary>
    /// Current status for the width controller.
    /// [Source: MachineSnapshot]
    /// </summary>
    public SnapshotValue<bool> WidthControllerStatus()
        => new(SnapshotColumnIds.ExtrusionQualitySettingsIsWidthControlOn, _queryTimestamp, _machineId);

    /// <summary>
    /// Current length of roll on winding station A.
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue RollLengthA()
        => new(SnapshotColumnIds.ExtrusionWindingStationAActualValuesRollLength, _queryTimestamp, _machineId);

    /// <summary>
    /// Current length of roll on winding station B.
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue RollLengthB()
        => new(SnapshotColumnIds.ExtrusionWindingStationBActualValuesRollLength, _queryTimestamp, _machineId);

    /// <summary>
    /// Current total power output.
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue PowerOutput()
        => new(SnapshotColumnIds.ExtrusionEnergyActualValuesElectricalPowerConsumption, _queryTimestamp, _machineId);

    /// <summary>
    /// The amount of energy currently used to produce one kg of film.
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue SpecificEnergy()
        => new(SnapshotColumnIds.ExtrusionEnergyActualValuesSpecificEnergyTotal, _queryTimestamp, _machineId);

    /// <summary>
    /// The actual process values for the bubble cooling.
    /// </summary>
    public ExtrusionCooling Cooling()
        => new(_queryTimestamp, _machineId);

    /// <summary>
    /// The actual process values for the cage.
    /// </summary>
    public ExtrusionCage Cage()
        => new(_queryTimestamp, _machineId);

    /// <summary>
    /// The actual process values for the haul-off.
    /// </summary>
    public ExtrusionHaulOff HaulOff()
        => new(_queryTimestamp, _machineId);

    /// <summary>
    /// The actual process values for the reversing unit.
    /// </summary>
    public ExtrusionReversingUnit ReversingUnit()
        => new ExtrusionReversingUnit(_queryTimestamp, _machineId);

    /// <summary>
    /// The actual process values for extruder A.
    /// </summary>
    public ExtrusionExtruderA ExtruderA()
        => new(_queryTimestamp, _machineId);

    /// <summary>
    /// The actual process values for extruder B.
    /// </summary>
    public ExtrusionExtruderB ExtruderB()
        => new(_queryTimestamp, _machineId);

    /// <summary>
    /// The actual process values for extruder C.
    /// </summary>
    public ExtrusionExtruderC ExtruderC()
        => new(_queryTimestamp, _machineId);

    /// <summary>
    /// The actual process values for extruder D.
    /// </summary>
    public ExtrusionExtruderD ExtruderD()
        => new(_queryTimestamp, _machineId);

    /// <summary>
    /// The actual process values for extruder E.
    /// </summary>
    public ExtrusionExtruderE ExtruderE()
        => new(_queryTimestamp, _machineId);

    /// <summary>
    /// The actual process values for extruder F.
    /// </summary>
    public ExtrusionExtruderF ExtruderF()
        => new(_queryTimestamp, _machineId);

    /// <summary>
    /// The actual process values for extruder G.
    /// </summary>
    public ExtrusionExtruderG ExtruderG()
        => new(_queryTimestamp, _machineId);

    /// <summary>
    /// The actual process values for extruder H.
    /// </summary>
    public ExtrusionExtruderH ExtruderH()
        => new(_queryTimestamp, _machineId);

    /// <summary>
    /// The actual process values for extruder I.
    /// </summary>
    public ExtrusionExtruderI ExtruderI()
        => new(_queryTimestamp, _machineId);

    /// <summary>
    /// The actual process values for extruder J.
    /// </summary>
    public ExtrusionExtruderJ ExtruderJ()
        => new(_queryTimestamp, _machineId);

    /// <summary>
    /// The actual process values for extruder K.
    /// </summary>
    public ExtrusionExtruderK ExtruderK()
        => new(_queryTimestamp, _machineId);

}