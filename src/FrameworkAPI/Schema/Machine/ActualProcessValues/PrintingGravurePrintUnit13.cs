using System;
using FrameworkAPI.Schema.Misc;
using WuH.Ruby.MachineSnapShooter.Client;

namespace FrameworkAPI.Schema.Machine.ActualProcessValues;

/// <summary>
/// All process parameters related to one gravure printing unit 13.
/// </summary>
public class PrintingGravurePrintUnit13(DateTime? queryTimestamp, string machineId)
{
    private readonly DateTime? _queryTimestamp = queryTimestamp;
    private readonly string _machineId = machineId;

    /// <summary>
    /// Measured ink viscosity of gravure print unit 
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Viscosity()
        => new(SnapshotColumnIds.PrintingGravureUnit13ActualValuesViscosity, _queryTimestamp, _machineId);

    /// <summary>
    /// ESA discharge state of gravure print unit 
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue ESADischargeState()
        => new(SnapshotColumnIds.PrintingGravureUnit13ESAActualValuesDischargeStatus, _queryTimestamp, _machineId);

    /// <summary>
    /// ESA charge state of gravure print unit 
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue ESAChargeState()
        => new(SnapshotColumnIds.PrintingGravureUnit13ESAActualValuesChargeStatus, _queryTimestamp, _machineId);

    /// <summary>
    /// ESA charge current of gravure print unit 
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue ESAChargeCurrent()
        => new(SnapshotColumnIds.PrintingGravureUnit13ESAActualValuesChargeCurrent, _queryTimestamp, _machineId);

    /// <summary>
    /// Drying zone 1 temperature of gravure print unit 
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Drying1Temperature()
        => new(SnapshotColumnIds.PrintingGravureUnit13Drying1ActualValuesTemperature, _queryTimestamp, _machineId);

    /// <summary>
    /// Drying zone 2 temperature of gravure print unit 
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Drying2Temperature()
        => new(SnapshotColumnIds.PrintingGravureUnit13Drying2ActualValuesTemperature, _queryTimestamp, _machineId);

    /// <summary>
    /// Drying zone 3 temperature of gravure print unit 
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Drying3Temperature()
        => new(SnapshotColumnIds.PrintingGravureUnit13Drying3ActualValuesTemperature, _queryTimestamp, _machineId);

    /// <summary>
    /// Drying zone 4 temperature of gravure print unit 
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Drying4Temperature()
        => new(SnapshotColumnIds.PrintingGravureUnit13Drying4ActualValuesTemperature, _queryTimestamp, _machineId);
}