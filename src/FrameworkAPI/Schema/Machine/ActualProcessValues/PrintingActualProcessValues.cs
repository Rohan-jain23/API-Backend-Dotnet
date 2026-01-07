using System;
using FrameworkAPI.Schema.Misc;
using WuH.Ruby.MachineSnapShooter.Client;

namespace FrameworkAPI.Schema.Machine.ActualProcessValues;

/// <summary>
/// Values measured by sensors, quality measurements, actual Values of machine settings, Values calculated by PLC, ...
/// </summary>
public class PrintingActualProcessValues(DateTime? queryTimestamp, string machineId)
{
    private readonly DateTime? _queryTimestamp = queryTimestamp;
    private readonly string _machineId = machineId;

    /// <summary>
    /// The status of treater 1 of the printing press
    /// [Source: MachineSnapshot]
    /// </summary>
    public SnapshotValue<bool> Treater1IsActive()
        => new(SnapshotColumnIds.PrintingTreater1IsActive, _queryTimestamp, _machineId);

    /// <summary>
    /// The actual treatment value of treater 1 of the printing press
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Treater1Treatment()
        => new(SnapshotColumnIds.PrintingTreater1ActualValueTreatment, _queryTimestamp, _machineId);

    /// <summary>
    /// The status of treater 2 of the printing press
    /// [Source: MachineSnapshot]
    /// </summary>
    public SnapshotValue<bool> Treater2IsActive()
        => new(SnapshotColumnIds.PrintingTreater2IsActive, _queryTimestamp, _machineId);

    /// <summary>
    /// The actual treatment value of treater 2 of the printing press
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Treater2Treatment()
        => new(SnapshotColumnIds.PrintingTreater2ActualValueTreatment, _queryTimestamp, _machineId);

    /// <summary>
    /// The web tension value of draw #1 of the printing press
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Draw1WebTension()
        => new(SnapshotColumnIds.PrintingDraw1ActualValuesWebTension, _queryTimestamp, _machineId);

    /// <summary>
    /// The web tension value of draw #2 of the printing press
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Draw2WebTension()
        => new(SnapshotColumnIds.PrintingDraw2ActualValuesWebTension, _queryTimestamp, _machineId);

    /// <summary>
    /// The web tension value of draw #3 of the printing press
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Draw3WebTension()
        => new(SnapshotColumnIds.PrintingDraw3ActualValuesWebTension, _queryTimestamp, _machineId);

    /// <summary>
    /// The web tension value of draw #4 of the printing press
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Draw4WebTension()
        => new(SnapshotColumnIds.PrintingDraw4ActualValuesWebTension, _queryTimestamp, _machineId);

    /// <summary>
    /// The web tension value of draw #5 of the printing press
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Draw5WebTension()
        => new(SnapshotColumnIds.PrintingDraw5ActualValuesWebTension, _queryTimestamp, _machineId);

    /// <summary>
    /// The web tension value of draw #6 of the printing press
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Draw6WebTension()
        => new(SnapshotColumnIds.PrintingDraw6ActualValuesWebTension, _queryTimestamp, _machineId);

    /// <summary>
    /// The web tension value of draw #7 of the printing press
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Draw7WebTension()
        => new(SnapshotColumnIds.PrintingDraw7ActualValuesWebTension, _queryTimestamp, _machineId);

    /// <summary>
    /// The web tension value of draw #8 of the printing press
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Draw8WebTension()
        => new(SnapshotColumnIds.PrintingDraw8ActualValuesWebTension, _queryTimestamp, _machineId);

    /// <summary>
    /// The web tension value of draw #9 of the printing press
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Draw9WebTension()
        => new(SnapshotColumnIds.PrintingDraw9ActualValuesWebTension, _queryTimestamp, _machineId);

    /// <summary>
    /// The web tension value of draw #10 of the printing press
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Draw10WebTension()
        => new(SnapshotColumnIds.PrintingDraw10ActualValuesWebTension, _queryTimestamp, _machineId);

    /// <summary>
    /// The web tension value of draw #11 of the printing press
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Draw11WebTension()
        => new(SnapshotColumnIds.PrintingDraw11ActualValuesWebTension, _queryTimestamp, _machineId);

    /// <summary>
    /// The web tension value of draw #12 of the printing press
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValue Draw12WebTension()
        => new(SnapshotColumnIds.PrintingDraw12ActualValuesWebTension, _queryTimestamp, _machineId);

    /// <summary>
    /// Indicates if the inspection system DEFECT-CHECK was active
    /// [Source: MachineSnapshot]
    /// </summary>
    public SnapshotValue<bool> DefectCheckIsActive()
        => new(SnapshotColumnIds.PrintingDefectCheckIsActive, _queryTimestamp, _machineId);

    /// <summary>
    /// Indicates if the inspection system BARCODE-CHECK was active
    /// [Source: MachineSnapshot]
    /// </summary>
    public SnapshotValue<bool> BarcodeCheckIsActive()
        => new(SnapshotColumnIds.PrintingBarcodeCheckIsActive, _queryTimestamp, _machineId);

    /// <summary>
    /// Indicates if the inspection system RGBLAB-CHECK was active
    /// [Source: MachineSnapshot]
    /// </summary>
    public SnapshotValue<bool> RgbLabCheckIsActive()
        => new(SnapshotColumnIds.PrintingRgbLabCheckIsActive, _queryTimestamp, _machineId);

    /// <summary>
    /// Value of the temperature of the in between color deck drying (only flexo print)
    /// </summary>
    public NumericSnapshotValue BetweenDeckDryingTemperature()
        => new(SnapshotColumnIds.PrintingDryingBDActualValuesTemperature, _queryTimestamp, _machineId);

    /// <summary>
    /// Value of the temperature of the tunnel drying (only flexo print)
    /// </summary>
    public NumericSnapshotValue TunnelDryingTemperature()
        => new(SnapshotColumnIds.PrintingDryingTDActualValuesTemperature, _queryTimestamp, _machineId);

    /// <summary>
    /// Value of the blower speed of the in between color deck drying (only flexo print)
    /// </summary>
    public NumericSnapshotValue BetweenDeckDryingBlowerSpeed()
        => new(SnapshotColumnIds.PrintingDryingBDActualValuesBlowerSpeed, _queryTimestamp, _machineId);

    /// <summary>
    /// Value of the blower speed of the tunnel drying (only flexo print)
    /// </summary>
    public NumericSnapshotValue TunnelDryingBlowerSpeed()
        => new(SnapshotColumnIds.PrintingDryingTDActualValuesBlowerSpeed, _queryTimestamp, _machineId);

    /// <summary>
    ///  Value of the viscosity in color deck 1 (only flexo print)
    /// </summary>
    public NumericSnapshotValue ColorDeck1Viscosity()
        => new(SnapshotColumnIds.PrintingColorDeck1ActualValuesViscosity, _queryTimestamp, _machineId);

    /// <summary>
    ///  Value of the viscosity in color deck 2 (only flexo print)
    /// </summary>
    public NumericSnapshotValue ColorDeck2Viscosity()
        => new(SnapshotColumnIds.PrintingColorDeck2ActualValuesViscosity, _queryTimestamp, _machineId);

    /// <summary>
    ///  Value of the viscosity in color deck 3 (only flexo print)
    /// </summary>
    public NumericSnapshotValue ColorDeck3Viscosity()
        => new(SnapshotColumnIds.PrintingColorDeck3ActualValuesViscosity, _queryTimestamp, _machineId);

    /// <summary>
    ///  Value of the viscosity in color deck 4 (only flexo print)
    /// </summary>
    public NumericSnapshotValue ColorDeck4Viscosity()
        => new(SnapshotColumnIds.PrintingColorDeck4ActualValuesViscosity, _queryTimestamp, _machineId);

    /// <summary>
    ///  Value of the viscosity in color deck 5 (only flexo print)
    /// </summary>
    public NumericSnapshotValue ColorDeck5Viscosity()
        => new(SnapshotColumnIds.PrintingColorDeck5ActualValuesViscosity, _queryTimestamp, _machineId);

    /// <summary>
    ///  Value of the viscosity in color deck 6 (only flexo print)
    /// </summary>
    public NumericSnapshotValue ColorDeck6Viscosity()
        => new(SnapshotColumnIds.PrintingColorDeck6ActualValuesViscosity, _queryTimestamp, _machineId);

    /// <summary>
    ///  Value of the viscosity in color deck 7 (only flexo print)
    /// </summary>
    public NumericSnapshotValue ColorDeck7Viscosity()
        => new(SnapshotColumnIds.PrintingColorDeck7ActualValuesViscosity, _queryTimestamp, _machineId);

    /// <summary>
    ///  Value of the viscosity in color deck 8 (only flexo print)
    /// </summary>
    public NumericSnapshotValue ColorDeck8Viscosity()
        => new(SnapshotColumnIds.PrintingColorDeck8ActualValuesViscosity, _queryTimestamp, _machineId);

    /// <summary>
    ///  Value of the viscosity in color deck 9 (only flexo print)
    /// </summary>
    public NumericSnapshotValue ColorDeck9Viscosity()
        => new(SnapshotColumnIds.PrintingColorDeck9ActualValuesViscosity, _queryTimestamp, _machineId);

    /// <summary>
    ///  Value of the viscosity in color deck 10 (only flexo print)
    /// </summary>
    public NumericSnapshotValue ColorDeck10Viscosity()
        => new(SnapshotColumnIds.PrintingColorDeck10ActualValuesViscosity, _queryTimestamp, _machineId);

    /// <summary>
    ///  Value of the viscosity in color deck 11 (only flexo print)
    /// </summary>
    public NumericSnapshotValue ColorDeck11Viscosity()
        => new(SnapshotColumnIds.PrintingColorDeck11ActualValuesViscosity, _queryTimestamp, _machineId);

    /// <summary>
    ///  Value of the viscosity in color deck 12 (only flexo print)
    /// </summary>
    public NumericSnapshotValue ColorDeck12Viscosity()
        => new(SnapshotColumnIds.PrintingColorDeck12ActualValuesViscosity, _queryTimestamp, _machineId);

    /// <summary>
    /// The actual process values for gravure print unit 1.
    /// </summary>
    public PrintingGravurePrintUnit1 GravurePrintUnit1()
        => new(_queryTimestamp, _machineId);

    /// <summary>
    /// The actual process values for gravure print unit 2.
    /// </summary>
    public PrintingGravurePrintUnit2 GravurePrintUnit2()
        => new(_queryTimestamp, _machineId);

    /// <summary>
    /// The actual process values for gravure print unit 3.
    /// </summary>
    public PrintingGravurePrintUnit3 GravurePrintUnit3()
        => new(_queryTimestamp, _machineId);

    /// <summary>
    /// The actual process values for gravure print unit 4.
    /// </summary>
    public PrintingGravurePrintUnit4 GravurePrintUnit4()
        => new(_queryTimestamp, _machineId);

    /// <summary>
    /// The actual process values for gravure print unit 5.
    /// </summary>
    public PrintingGravurePrintUnit5 GravurePrintUnit5()
        => new(_queryTimestamp, _machineId);

    /// <summary>
    /// The actual process values for gravure print unit 6.
    /// </summary>
    public PrintingGravurePrintUnit6 GravurePrintUnit6()
        => new(_queryTimestamp, _machineId);

    /// <summary>
    /// The actual process values for gravure print unit 7.
    /// </summary>
    public PrintingGravurePrintUnit7 GravurePrintUnit7()
        => new(_queryTimestamp, _machineId);

    /// <summary>
    /// The actual process values for gravure print unit 8.
    /// </summary>
    public PrintingGravurePrintUnit8 GravurePrintUnit8()
        => new(_queryTimestamp, _machineId);

    /// <summary>
    /// The actual process values for gravure print unit 9.
    /// </summary>
    public PrintingGravurePrintUnit9 GravurePrintUnit9()
        => new(_queryTimestamp, _machineId);

    /// <summary>
    /// The actual process values for gravure print unit 10.
    /// </summary>
    public PrintingGravurePrintUnit10 GravurePrintUnit10()
        => new(_queryTimestamp, _machineId);

    /// <summary>
    /// The actual process values for gravure print unit 11.
    /// </summary>
    public PrintingGravurePrintUnit11 GravurePrintUnit11()
        => new(_queryTimestamp, _machineId);

    /// <summary>
    /// The actual process values for gravure print unit 12.
    /// </summary>
    public PrintingGravurePrintUnit12 GravurePrintUnit12()
        => new(_queryTimestamp, _machineId);

    /// <summary>
    /// The actual process values for gravure print unit 13.
    /// </summary>
    public PrintingGravurePrintUnit13 GravurePrintUnit13()
        => new(_queryTimestamp, _machineId);

    /// <summary>
    /// The actual process values for gravure print unit 14.
    /// </summary>
    public PrintingGravurePrintUnit14 GravurePrintUnit14()
        => new(_queryTimestamp, _machineId);
}
