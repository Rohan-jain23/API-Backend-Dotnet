using System;
using FrameworkAPI.Schema.Misc;
using WuH.Ruby.MachineSnapShooter.Client;

namespace FrameworkAPI.Schema.Machine.ActualProcessValues;

/// <summary>
/// All process parameters related to one gravure printing unit 12.
/// </summary>
public class PrintingGravurePrintUnit12(DateTime? queryTimestamp, string machineId)
{
    private readonly DateTime? _queryTimestamp = queryTimestamp;
    private readonly string _machineId = machineId;

    /// <summary>
    /// Measured ink viscosity of gravure print unit 
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Viscosity()
        => new(SnapshotColumnIds.PrintingGravureUnit12ActualValuesViscosity, _queryTimestamp, _machineId);

    /// <summary>
    /// ESA discharge state of gravure print unit 
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue ESADischargeState()
        => new(SnapshotColumnIds.PrintingGravureUnit12ESAActualValuesDischargeStatus, _queryTimestamp, _machineId);

    /// <summary>
    /// ESA charge state of gravure print unit 
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue ESAChargeState()
        => new(SnapshotColumnIds.PrintingGravureUnit12ESAActualValuesChargeStatus, _queryTimestamp, _machineId);

    /// <summary>
    /// ESA charge current of gravure print unit 
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue ESAChargeCurrent()
        => new(SnapshotColumnIds.PrintingGravureUnit12ESAActualValuesChargeCurrent, _queryTimestamp, _machineId);

    /// <summary>
    /// Drying zone 1 temperature of gravure print unit 
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Drying1Temperature()
        => new(SnapshotColumnIds.PrintingGravureUnit12Drying1ActualValuesTemperature, _queryTimestamp, _machineId);

    /// <summary>
    /// Drying zone 2 temperature of gravure print unit 
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Drying2Temperature()
        => new(SnapshotColumnIds.PrintingGravureUnit12Drying2ActualValuesTemperature, _queryTimestamp, _machineId);

    /// <summary>
    /// Drying zone 3 temperature of gravure print unit 
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Drying3Temperature()
        => new(SnapshotColumnIds.PrintingGravureUnit12Drying3ActualValuesTemperature, _queryTimestamp, _machineId);

    /// <summary>
    /// Drying zone 4 temperature of gravure print unit 
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Drying4Temperature()
        => new(SnapshotColumnIds.PrintingGravureUnit12Drying4ActualValuesTemperature, _queryTimestamp, _machineId);
}