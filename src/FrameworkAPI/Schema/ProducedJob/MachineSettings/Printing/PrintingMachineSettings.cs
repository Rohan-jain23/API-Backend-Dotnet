using System;
using System.Collections.Generic;
using FrameworkAPI.Schema.Misc;
using WuH.Ruby.MachineSnapShooter.Client;

namespace FrameworkAPI.Schema.ProducedJob.MachineSettings.Printing;

/// <summary>
/// Machine settings during a printing job.
/// </summary>
public class PrintingMachineSettings(string machineId, DateTime? endTime, IEnumerable<TimeRange>? timeRanges, DateTime? machineQueryTimestamp)
{

    /// <summary>
    /// Set value of the substrate width from the unwinder
    /// </summary>
    public NumericSnapshotValuesDuringProduction MaterialWidth()
        => new(SnapshotColumnIds.PrintingUnwindMaterialSettingsWidth, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Set value of the substrate thickness from the unwinder
    /// </summary>
    public NumericSnapshotValuesDuringProduction MaterialThickness()
        => new(SnapshotColumnIds.PrintingUnwindMaterialSettingsThickness, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Set value of the substrate e module thickness from the unwinder
    /// </summary>
    public NumericSnapshotValuesDuringProduction MaterialEModule()
        => new(SnapshotColumnIds.PrintingUnwindMaterialSettingsEModule, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Set value of the substrate density from the unwinder
    /// </summary>
    public NumericSnapshotValuesDuringProduction MaterialDensity()
        => new(SnapshotColumnIds.PrintingUnwindMaterialSettingsDensity, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Name of the substrate from the unwinder
    /// </summary>
    public SnapshotValuesDuringProduction<string> MaterialName()
        => new(SnapshotColumnIds.PrintingUnwindMaterialSettingsName, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Set value of the production speed
    /// </summary>
    public NumericSnapshotValuesDuringProduction MachineSpeed()
        => new(SnapshotColumnIds.PrintingTargetSpeedFromProcessData, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Chosen id value of the machine web mode
    /// </summary>
    public SnapshotValuesDuringProduction<int?> MachineWebModeId()
        => new(SnapshotColumnIds.PrintingMachineSettingsWebModeId, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Chosen id value of the machine print style mode
    /// </summary>
    public SnapshotValuesDuringProduction<int?> MachinePrintStyleId()
        => new(SnapshotColumnIds.PrintingMachineSettingsPrintStyleId, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Set value of the temperature of the in between color deck drying (only flexo print)
    /// </summary>
    public NumericSnapshotValuesDuringProduction BDDryingTemperature()
        => new(SnapshotColumnIds.PrintingDryingBDSettingsTemperature, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Set value of the fresh air stage of the in between color deck drying (only flexo print)
    /// 0 means "minimal", 1 "medium" and 2 "maximum"
    /// </summary>
    public SnapshotValuesDuringProduction<int?> BDDryingFreshAirStage()
        => new(SnapshotColumnIds.PrintingDryingBDSettingsFreshAirStage, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Set value of the temperature of the tunnel drying (only flexo print)
    /// </summary>
    public NumericSnapshotValuesDuringProduction TDDryingTemperature()
        => new(SnapshotColumnIds.PrintingDryingTDSettingsTemperature, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Set value of the fresh air stage of the tunnel drying (only flexo print)
    /// 0 means "minimal", 1 "medium" and 2 "maximum"
    /// </summary>
    public SnapshotValuesDuringProduction<int?> TDDryingFreshAirStage()
          => new(SnapshotColumnIds.PrintingDryingTDSettingsFreshAirStage, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Indicates if the color deck 1 was active
    /// [Source: MachineSnapshot]
    /// </summary>
    public SnapshotValuesDuringProduction<bool?> ColorDeck1IsActive()
        => new(SnapshotColumnIds.PrintingColorDeck1IsActive, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Set value of the format in color deck 1 (only flexo print)
    /// </summary>
    public NumericSnapshotValuesDuringProduction ColorDeck1Format()
          => new(SnapshotColumnIds.PrintingColorDeck1SettingsFormatLength, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Set value of the anilox roller name in color deck 1 (only flexo print)
    /// </summary>
    public NumericSnapshotValuesDuringProduction ColorDeck1AniloxRoller()
          => new(SnapshotColumnIds.PrintingColorDeck1SettingsAniloxRoller, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Set value of the viscosity in color deck 1 (only flexo print)
    /// </summary>
    public NumericSnapshotValuesDuringProduction ColorDeck1Viscosity()
          => new(SnapshotColumnIds.PrintingColorDeck1SettingsViscosity, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Set value of the doctor blade pressure in color deck 1 (only flexo print)
    /// </summary>
    public NumericSnapshotValuesDuringProduction ColorDeck1DoctorBladePressure()
          => new(SnapshotColumnIds.PrintingColorDeck1SettingsDoctorbladePressure, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Indicates if the color deck 2 was active
    /// [Source: MachineSnapshot]
    /// </summary>
    public SnapshotValuesDuringProduction<bool?> ColorDeck2IsActive()
        => new(SnapshotColumnIds.PrintingColorDeck2IsActive, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Set value of the format in color deck 2 (only flexo print)
    /// </summary>
    public NumericSnapshotValuesDuringProduction ColorDeck2Format()
          => new(SnapshotColumnIds.PrintingColorDeck1SettingsFormatLength, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Set value of the anilox roller name in color deck 2 (only flexo print)
    /// </summary>
    public NumericSnapshotValuesDuringProduction ColorDeck2AniloxRoller()
          => new(SnapshotColumnIds.PrintingColorDeck1SettingsAniloxRoller, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Set value of the viscosity in color deck 2 (only flexo print)
    /// </summary>
    public NumericSnapshotValuesDuringProduction ColorDeck2Viscosity()
          => new(SnapshotColumnIds.PrintingColorDeck1SettingsViscosity, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Set value of the doctor blade pressure in color deck 2 (only flexo print)
    /// </summary>
    public NumericSnapshotValuesDuringProduction ColorDeck2DoctorBladePressure()
          => new(SnapshotColumnIds.PrintingColorDeck1SettingsDoctorbladePressure, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Indicates if the color deck 3 was active
    /// [Source: MachineSnapshot]
    /// </summary>
    public SnapshotValuesDuringProduction<bool?> ColorDeck3IsActive()
        => new(SnapshotColumnIds.PrintingColorDeck3IsActive, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Set value of the format in color deck 3 (only flexo print)
    /// </summary>
    public NumericSnapshotValuesDuringProduction ColorDeck3Format()
          => new(SnapshotColumnIds.PrintingColorDeck3SettingsFormatLength, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Set value of the anilox roller name in color deck 3 (only flexo print)
    /// </summary>
    public NumericSnapshotValuesDuringProduction ColorDeck3AniloxRoller()
          => new(SnapshotColumnIds.PrintingColorDeck3SettingsAniloxRoller, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Set value of the viscosity in color deck 3 (only flexo print)
    /// </summary>
    public NumericSnapshotValuesDuringProduction ColorDeck3Viscosity()
          => new(SnapshotColumnIds.PrintingColorDeck3SettingsViscosity, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Set value of the doctor blade pressure in color deck 3 (only flexo print)
    /// </summary>
    public NumericSnapshotValuesDuringProduction ColorDeck3DoctorBladePressure()
          => new(SnapshotColumnIds.PrintingColorDeck3SettingsDoctorbladePressure, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Indicates if the color deck 4 was active
    /// [Source: MachineSnapshot]
    /// </summary>
    public SnapshotValuesDuringProduction<bool?> ColorDeck4IsActive()
        => new(SnapshotColumnIds.PrintingColorDeck4IsActive, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Set value of the format in color deck 4 (only flexo print)
    /// </summary>
    public NumericSnapshotValuesDuringProduction ColorDeck4Format()
          => new(SnapshotColumnIds.PrintingColorDeck4SettingsFormatLength, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Set value of the anilox roller name in color deck 4 (only flexo print)
    /// </summary>
    public NumericSnapshotValuesDuringProduction ColorDeck4AniloxRoller()
          => new(SnapshotColumnIds.PrintingColorDeck4SettingsAniloxRoller, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Set value of the viscosity in color deck 4 (only flexo print)
    /// </summary>
    public NumericSnapshotValuesDuringProduction ColorDeck4Viscosity()
          => new(SnapshotColumnIds.PrintingColorDeck1SettingsViscosity, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Set value of the doctor blade pressure in color deck 4 (only flexo print)
    /// </summary>
    public NumericSnapshotValuesDuringProduction ColorDeck4DoctorBladePressure()
          => new(SnapshotColumnIds.PrintingColorDeck1SettingsDoctorbladePressure, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Indicates if the color deck 5 was active
    /// [Source: MachineSnapshot]
    /// </summary>
    public SnapshotValuesDuringProduction<bool?> ColorDeck5IsActive()
        => new(SnapshotColumnIds.PrintingColorDeck5IsActive, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Set value of the format in color deck 5 (only flexo print)
    /// </summary>
    public NumericSnapshotValuesDuringProduction ColorDeck5Format()
          => new(SnapshotColumnIds.PrintingColorDeck5SettingsFormatLength, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Set value of the anilox roller name in color deck 5 (only flexo print)
    /// </summary>
    public NumericSnapshotValuesDuringProduction ColorDeck5AniloxRoller()
          => new(SnapshotColumnIds.PrintingColorDeck1SettingsAniloxRoller, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Set value of the viscosity in color deck 5 (only flexo print)
    /// </summary>
    public NumericSnapshotValuesDuringProduction ColorDeck5Viscosity()
          => new(SnapshotColumnIds.PrintingColorDeck5SettingsViscosity, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Set value of the doctor blade pressure in color deck 5 (only flexo print)
    /// </summary>
    public NumericSnapshotValuesDuringProduction ColorDeck5DoctorBladePressure()
          => new(SnapshotColumnIds.PrintingColorDeck5SettingsDoctorbladePressure, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Indicates if the color deck 6 was active
    /// [Source: MachineSnapshot]
    /// </summary>
    public SnapshotValuesDuringProduction<bool?> ColorDeck6IsActive()
        => new(SnapshotColumnIds.PrintingColorDeck6IsActive, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Set value of the format in color deck 6 (only flexo print)
    /// </summary>
    public NumericSnapshotValuesDuringProduction ColorDeck6Format()
          => new(SnapshotColumnIds.PrintingColorDeck6SettingsFormatLength, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Set value of the anilox roller name in color deck 6 (only flexo print)
    /// </summary>
    public NumericSnapshotValuesDuringProduction ColorDeck6AniloxRoller()
          => new(SnapshotColumnIds.PrintingColorDeck6SettingsAniloxRoller, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Set value of the viscosity in color deck 6 (only flexo print)
    /// </summary>
    public NumericSnapshotValuesDuringProduction ColorDeck6Viscosity()
          => new(SnapshotColumnIds.PrintingColorDeck1SettingsViscosity, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Set value of the doctor blade pressure in color deck 6 (only flexo print)
    /// </summary>
    public NumericSnapshotValuesDuringProduction ColorDeck6DoctorBladePressure()
          => new(SnapshotColumnIds.PrintingColorDeck6SettingsDoctorbladePressure, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Indicates if the color deck 7 was active
    /// [Source: MachineSnapshot]
    /// </summary>
    public SnapshotValuesDuringProduction<bool?> ColorDeck7IsActive()
        => new(SnapshotColumnIds.PrintingColorDeck7IsActive, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Set value of the format in color deck 7 (only flexo print)
    /// </summary>
    public NumericSnapshotValuesDuringProduction ColorDeck7Format()
          => new(SnapshotColumnIds.PrintingColorDeck7SettingsFormatLength, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Set value of the anilox roller name in color deck 7 (only flexo print)
    /// </summary>
    public NumericSnapshotValuesDuringProduction ColorDeck7AniloxRoller()
          => new(SnapshotColumnIds.PrintingColorDeck7SettingsAniloxRoller, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Set value of the viscosity in color deck 7 (only flexo print)
    /// </summary>
    public NumericSnapshotValuesDuringProduction ColorDeck7Viscosity()
          => new(SnapshotColumnIds.PrintingColorDeck7SettingsViscosity, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Set value of the doctor blade pressure in color deck 7 (only flexo print)
    /// </summary>
    public NumericSnapshotValuesDuringProduction ColorDeck7DoctorBladePressure()
          => new(SnapshotColumnIds.PrintingColorDeck7SettingsDoctorbladePressure, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Indicates if the color deck 8 was active
    /// [Source: MachineSnapshot]
    /// </summary>
    public SnapshotValuesDuringProduction<bool?> ColorDeck8IsActive()
        => new(SnapshotColumnIds.PrintingColorDeck8IsActive, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Set value of the format in color deck 8 (only flexo print)
    /// </summary>
    public NumericSnapshotValuesDuringProduction ColorDeck8Format()
          => new(SnapshotColumnIds.PrintingColorDeck8SettingsFormatLength, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Set value of the anilox roller name in color deck 8 (only flexo print)
    /// </summary>
    public NumericSnapshotValuesDuringProduction ColorDeck8AniloxRoller()
          => new(SnapshotColumnIds.PrintingColorDeck8SettingsAniloxRoller, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Set value of the viscosity in color deck 8 (only flexo print)
    /// </summary>
    public NumericSnapshotValuesDuringProduction ColorDeck8Viscosity()
          => new(SnapshotColumnIds.PrintingColorDeck8SettingsViscosity, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Set value of the doctor blade pressure in color deck 8 (only flexo print)
    /// </summary>
    public NumericSnapshotValuesDuringProduction ColorDeck8DoctorBladePressure()
          => new(SnapshotColumnIds.PrintingColorDeck8SettingsDoctorbladePressure, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Indicates if the color deck 9 was active
    /// [Source: MachineSnapshot]
    /// </summary>
    public SnapshotValuesDuringProduction<bool?> ColorDeck9IsActive()
        => new(SnapshotColumnIds.PrintingColorDeck9IsActive, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Set value of the format in color deck 9 (only flexo print)
    /// </summary>
    public NumericSnapshotValuesDuringProduction ColorDeck9Format()
          => new(SnapshotColumnIds.PrintingColorDeck9SettingsFormatLength, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Set value of the anilox roller name in color deck 9 (only flexo print)
    /// </summary>
    public NumericSnapshotValuesDuringProduction ColorDeck9AniloxRoller()
          => new(SnapshotColumnIds.PrintingColorDeck9SettingsAniloxRoller, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Set value of the viscosity in color deck 9 (only flexo print)
    /// </summary>
    public NumericSnapshotValuesDuringProduction ColorDeck9Viscosity()
          => new(SnapshotColumnIds.PrintingColorDeck9SettingsViscosity, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Set value of the doctor blade pressure in color deck 9 (only flexo print)
    /// </summary>
    public NumericSnapshotValuesDuringProduction ColorDeck9DoctorBladePressure()
          => new(SnapshotColumnIds.PrintingColorDeck9SettingsDoctorbladePressure, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Indicates if the color deck 10 was active
    /// [Source: MachineSnapshot]
    /// </summary>
    public SnapshotValuesDuringProduction<bool?> ColorDeck10IsActive()
        => new(SnapshotColumnIds.PrintingColorDeck10IsActive, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Set value of the format in color deck 10 (only flexo print)
    /// </summary>
    public NumericSnapshotValuesDuringProduction ColorDeck10Format()
          => new(SnapshotColumnIds.PrintingColorDeck10SettingsFormatLength, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Set value of the anilox roller name in color deck 10 (only flexo print)
    /// </summary>
    public NumericSnapshotValuesDuringProduction ColorDeck10AniloxRoller()
          => new(SnapshotColumnIds.PrintingColorDeck10SettingsAniloxRoller, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Set value of the viscosity in color deck 10 (only flexo print)
    /// </summary>
    public NumericSnapshotValuesDuringProduction ColorDeck10Viscosity()
          => new(SnapshotColumnIds.PrintingColorDeck10SettingsViscosity, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Set value of the doctor blade pressure in color deck 10 (only flexo print)
    /// </summary>
    public NumericSnapshotValuesDuringProduction ColorDeck10DoctorBladePressure()
          => new(SnapshotColumnIds.PrintingColorDeck10SettingsDoctorbladePressure, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Indicates if the color deck 11 was active
    /// [Source: MachineSnapshot]
    /// </summary>
    public SnapshotValuesDuringProduction<bool?> ColorDeck11IsActive()
        => new(SnapshotColumnIds.PrintingColorDeck11IsActive, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Set value of the format in color deck 11 (only flexo print)
    /// </summary>
    public NumericSnapshotValuesDuringProduction ColorDeck11Format()
          => new(SnapshotColumnIds.PrintingColorDeck11SettingsFormatLength, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Set value of the anilox roller name in color deck 11 (only flexo print)
    /// </summary>
    public NumericSnapshotValuesDuringProduction ColorDeck11AniloxRoller()
          => new(SnapshotColumnIds.PrintingColorDeck11SettingsAniloxRoller, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Set value of the viscosity in color deck 11 (only flexo print)
    /// </summary>
    public NumericSnapshotValuesDuringProduction ColorDeck11Viscosity()
          => new(SnapshotColumnIds.PrintingColorDeck11SettingsViscosity, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Set value of the doctor blade pressure in color deck 11 (only flexo print)
    /// </summary>
    public NumericSnapshotValuesDuringProduction ColorDeck11DoctorBladePressure()
          => new(SnapshotColumnIds.PrintingColorDeck11SettingsDoctorbladePressure, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Indicates if the color deck 12 was active
    /// [Source: MachineSnapshot]
    /// </summary>
    public SnapshotValuesDuringProduction<bool?> ColorDeck12IsActive()
        => new(SnapshotColumnIds.PrintingColorDeck12IsActive, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Set value of the format in color deck 12 (only flexo print)
    /// </summary>
    public NumericSnapshotValuesDuringProduction ColorDeck12Format()
          => new(SnapshotColumnIds.PrintingColorDeck12SettingsFormatLength, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Set value of the anilox roller name in color deck 12 (only flexo print)
    /// </summary>
    public NumericSnapshotValuesDuringProduction ColorDeck12AniloxRoller()
          => new(SnapshotColumnIds.PrintingColorDeck12SettingsAniloxRoller, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Set value of the viscosity in color deck 12 (only flexo print)
    /// </summary>
    public NumericSnapshotValuesDuringProduction ColorDeck12Viscosity()
          => new(SnapshotColumnIds.PrintingColorDeck12SettingsViscosity, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Set value of the doctor blade pressure in color deck 12 (only flexo print)
    /// </summary>
    public NumericSnapshotValuesDuringProduction ColorDeck12DoctorBladePressure()
          => new(SnapshotColumnIds.PrintingColorDeck12SettingsDoctorbladePressure, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Indicates if the gravure unit 1 was active
    /// [Source: MachineSnapshot]
    /// </summary>
    public SnapshotValuesDuringProduction<bool?> GravureUnit1IsActive()
        => new(SnapshotColumnIds.PrintingGravureUnit1IsActive, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Indicates if the gravure unit 2 was active
    /// [Source: MachineSnapshot]
    /// </summary>
    public SnapshotValuesDuringProduction<bool?> GravureUnit2IsActive()
        => new(SnapshotColumnIds.PrintingGravureUnit2IsActive, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Indicates if the gravure unit 3 was active
    /// [Source: MachineSnapshot]
    /// </summary>
    public SnapshotValuesDuringProduction<bool?> GravureUnit3IsActive()
        => new(SnapshotColumnIds.PrintingGravureUnit3IsActive, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Indicates if the gravure unit 4 was active
    /// [Source: MachineSnapshot]
    /// </summary>
    public SnapshotValuesDuringProduction<bool?> GravureUnit4IsActive()
        => new(SnapshotColumnIds.PrintingGravureUnit4IsActive, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Indicates if the gravure unit 5 was active
    /// [Source: MachineSnapshot]
    /// </summary>
    public SnapshotValuesDuringProduction<bool?> GravureUnit5IsActive()
        => new(SnapshotColumnIds.PrintingGravureUnit5IsActive, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Indicates if the gravure unit 6 was active
    /// [Source: MachineSnapshot]
    /// </summary>
    public SnapshotValuesDuringProduction<bool?> GravureUnit6IsActive()
        => new(SnapshotColumnIds.PrintingGravureUnit6IsActive, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Indicates if the gravure unit 7 was active
    /// [Source: MachineSnapshot]
    /// </summary>
    public SnapshotValuesDuringProduction<bool?> GravureUnit7IsActive()
        => new(SnapshotColumnIds.PrintingGravureUnit7IsActive, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Indicates if the gravure unit 8 was active
    /// [Source: MachineSnapshot]
    /// </summary>
    public SnapshotValuesDuringProduction<bool?> GravureUnit8IsActive()
        => new(SnapshotColumnIds.PrintingGravureUnit8IsActive, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Indicates if the gravure unit 9 was active
    /// [Source: MachineSnapshot]
    /// </summary>
    public SnapshotValuesDuringProduction<bool?> GravureUnit9IsActive()
        => new(SnapshotColumnIds.PrintingGravureUnit9IsActive, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Indicates if the gravure unit 10 was active
    /// [Source: MachineSnapshot]
    /// </summary>
    public SnapshotValuesDuringProduction<bool?> GravureUnit10IsActive()
        => new(SnapshotColumnIds.PrintingGravureUnit10IsActive, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Indicates if the gravure unit 11 was active
    /// [Source: MachineSnapshot]
    /// </summary>
    public SnapshotValuesDuringProduction<bool?> GravureUnit11IsActive()
        => new(SnapshotColumnIds.PrintingGravureUnit11IsActive, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Indicates if the gravure unit 12 was active
    /// [Source: MachineSnapshot]
    /// </summary>
    public SnapshotValuesDuringProduction<bool?> GravureUnit12IsActive()
        => new(SnapshotColumnIds.PrintingGravureUnit12IsActive, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Indicates if the gravure unit 13 was active
    /// [Source: MachineSnapshot]
    /// </summary>
    public SnapshotValuesDuringProduction<bool?> GravureUnit13IsActive()
        => new(SnapshotColumnIds.PrintingGravureUnit13IsActive, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Indicates if the gravure unit 14 was active
    /// [Source: MachineSnapshot]
    /// </summary>
    public SnapshotValuesDuringProduction<bool?> GravureUnit14IsActive()
        => new(SnapshotColumnIds.PrintingGravureUnit14IsActive, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// format length of gravure unit 1
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit1FormatLength()
        => new(SnapshotColumnIds.PrintingGravureUnit1SettingsFormatLength, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// format length of gravure unit 2
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit2FormatLength()
        => new(SnapshotColumnIds.PrintingGravureUnit2SettingsFormatLength, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// format length of gravure unit 3
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit3FormatLength()
        => new(SnapshotColumnIds.PrintingGravureUnit3SettingsFormatLength, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// format length of gravure unit 4
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit4FormatLength()
        => new(SnapshotColumnIds.PrintingGravureUnit4SettingsFormatLength, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// format length of gravure unit 5
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit5FormatLength()
        => new(SnapshotColumnIds.PrintingGravureUnit5SettingsFormatLength, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// format length of gravure unit 6
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit6FormatLength()
        => new(SnapshotColumnIds.PrintingGravureUnit6SettingsFormatLength, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// format length of gravure unit 7
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit7FormatLength()
        => new(SnapshotColumnIds.PrintingGravureUnit7SettingsFormatLength, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// format length of gravure unit 8
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit8FormatLength()
        => new(SnapshotColumnIds.PrintingGravureUnit8SettingsFormatLength, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// format length of gravure unit 9
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit9FormatLength()
        => new(SnapshotColumnIds.PrintingGravureUnit9SettingsFormatLength, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// format length of gravure unit 10
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit10FormatLength()
        => new(SnapshotColumnIds.PrintingGravureUnit10SettingsFormatLength, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// format length of gravure unit 11
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit11FormatLength()
        => new(SnapshotColumnIds.PrintingGravureUnit11SettingsFormatLength, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// format length of gravure unit 12
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit12FormatLength()
        => new(SnapshotColumnIds.PrintingGravureUnit12SettingsFormatLength, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// format length of gravure unit 13
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit13FormatLength()
        => new(SnapshotColumnIds.PrintingGravureUnit13SettingsFormatLength, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// format length of gravure unit 14
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit14FormatLength()
        => new(SnapshotColumnIds.PrintingGravureUnit14SettingsFormatLength, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Doctorblade Pressure of gravure unit 1
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit1SettingsDoctorbladePressure()
        => new(SnapshotColumnIds.PrintingGravureUnit1SettingsDoctorbladePressure, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Doctorblade Pressure of gravure unit 2
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit2SettingsDoctorbladePressure()
        => new(SnapshotColumnIds.PrintingGravureUnit2SettingsDoctorbladePressure, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Doctorblade Pressure of gravure unit 3
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit3SettingsDoctorbladePressure()
        => new(SnapshotColumnIds.PrintingGravureUnit3SettingsDoctorbladePressure, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Doctorblade Pressure of gravure unit 4
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit4SettingsDoctorbladePressure()
        => new(SnapshotColumnIds.PrintingGravureUnit4SettingsDoctorbladePressure, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Doctorblade Pressure of gravure unit 5
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit5SettingsDoctorbladePressure()
        => new(SnapshotColumnIds.PrintingGravureUnit5SettingsDoctorbladePressure, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Doctorblade Pressure of gravure unit 6
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit6SettingsDoctorbladePressure()
        => new(SnapshotColumnIds.PrintingGravureUnit6SettingsDoctorbladePressure, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Doctorblade Pressure of gravure unit 7
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit7SettingsDoctorbladePressure()
        => new(SnapshotColumnIds.PrintingGravureUnit7SettingsDoctorbladePressure, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Doctorblade Pressure of gravure unit 8
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit8SettingsDoctorbladePressure()
        => new(SnapshotColumnIds.PrintingGravureUnit8SettingsDoctorbladePressure, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Doctorblade Pressure of gravure unit 9
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit9SettingsDoctorbladePressure()
        => new(SnapshotColumnIds.PrintingGravureUnit9SettingsDoctorbladePressure, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Doctorblade Pressure of gravure unit 10
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit10SettingsDoctorbladePressure()
        => new(SnapshotColumnIds.PrintingGravureUnit10SettingsDoctorbladePressure, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Doctorblade Pressure of gravure unit 11
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit11SettingsDoctorbladePressure()
        => new(SnapshotColumnIds.PrintingGravureUnit11SettingsDoctorbladePressure, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Doctorblade Pressure of gravure unit 12
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit12SettingsDoctorbladePressure()
        => new(SnapshotColumnIds.PrintingGravureUnit12SettingsDoctorbladePressure, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Doctorblade Pressure of gravure unit 13
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit13SettingsDoctorbladePressure()
        => new(SnapshotColumnIds.PrintingGravureUnit13SettingsDoctorbladePressure, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Doctorblade Pressure of gravure unit 14
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit14SettingsDoctorbladePressure()
        => new(SnapshotColumnIds.PrintingGravureUnit14SettingsDoctorbladePressure, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Viscosity of gravure unit 1
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit1SettingsViscosity()
        => new(SnapshotColumnIds.PrintingGravureUnit1SettingsViscosity, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Viscosity of gravure unit 2
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit2SettingsViscosity()
        => new(SnapshotColumnIds.PrintingGravureUnit2SettingsViscosity, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Viscosity of gravure unit 3
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit3SettingsViscosity()
        => new(SnapshotColumnIds.PrintingGravureUnit3SettingsViscosity, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Viscosity of gravure unit 4
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit4SettingsViscosity()
        => new(SnapshotColumnIds.PrintingGravureUnit4SettingsViscosity, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Viscosity of gravure unit 5
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit5SettingsViscosity()
        => new(SnapshotColumnIds.PrintingGravureUnit5SettingsViscosity, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Viscosity of gravure unit 6
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit6SettingsViscosity()
        => new(SnapshotColumnIds.PrintingGravureUnit6SettingsViscosity, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Viscosity of gravure unit 7
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit7SettingsViscosity()
        => new(SnapshotColumnIds.PrintingGravureUnit7SettingsViscosity, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Viscosity of gravure unit 8
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit8SettingsViscosity()
        => new(SnapshotColumnIds.PrintingGravureUnit8SettingsViscosity, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Viscosity of gravure unit 9
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit9SettingsViscosity()
        => new(SnapshotColumnIds.PrintingGravureUnit9SettingsViscosity, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Viscosity of gravure unit 10
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit10SettingsViscosity()
        => new(SnapshotColumnIds.PrintingGravureUnit10SettingsViscosity, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Viscosity of gravure unit 11
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit11SettingsViscosity()
        => new(SnapshotColumnIds.PrintingGravureUnit11SettingsViscosity, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Viscosity of gravure unit 12
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit12SettingsViscosity()
        => new(SnapshotColumnIds.PrintingGravureUnit12SettingsViscosity, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Viscosity of gravure unit 13
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit13SettingsViscosity()
        => new(SnapshotColumnIds.PrintingGravureUnit13SettingsViscosity, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Viscosity of gravure unit 14
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit14SettingsViscosity()
        => new(SnapshotColumnIds.PrintingGravureUnit14SettingsViscosity, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// impression roller pressure ds of gravure unit 1
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit1SettingsImpressionRollerPressureDS()
        => new(SnapshotColumnIds.PrintingGravureUnit1SettingsImpressionRollerPressureDS, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// impression roller pressure ds of gravure unit 2
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit2SettingsImpressionRollerPressureDS()
        => new(SnapshotColumnIds.PrintingGravureUnit2SettingsImpressionRollerPressureDS, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// impression roller pressure ds of gravure unit 3
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit3SettingsImpressionRollerPressureDS()
        => new(SnapshotColumnIds.PrintingGravureUnit3SettingsImpressionRollerPressureDS, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// impression roller pressure ds of gravure unit 4
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit4SettingsImpressionRollerPressureDS()
        => new(SnapshotColumnIds.PrintingGravureUnit4SettingsImpressionRollerPressureDS, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// impression roller pressure ds of gravure unit 5
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit5SettingsImpressionRollerPressureDS()
        => new(SnapshotColumnIds.PrintingGravureUnit5SettingsImpressionRollerPressureDS, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// impression roller pressure ds of gravure unit 6
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit6SettingsImpressionRollerPressureDS()
        => new(SnapshotColumnIds.PrintingGravureUnit6SettingsImpressionRollerPressureDS, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// impression roller pressure ds of gravure unit 7
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit7SettingsImpressionRollerPressureDS()
        => new(SnapshotColumnIds.PrintingGravureUnit7SettingsImpressionRollerPressureDS, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// impression roller pressure ds of gravure unit 8
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit8SettingsImpressionRollerPressureDS()
        => new(SnapshotColumnIds.PrintingGravureUnit8SettingsImpressionRollerPressureDS, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// impression roller pressure ds of gravure unit 9
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit9SettingsImpressionRollerPressureDS()
        => new(SnapshotColumnIds.PrintingGravureUnit9SettingsImpressionRollerPressureDS, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// impression roller pressure ds of gravure unit 10
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit10SettingsImpressionRollerPressureDS()
        => new(SnapshotColumnIds.PrintingGravureUnit10SettingsImpressionRollerPressureDS, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// impression roller pressure ds of gravure unit 11
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit11SettingsImpressionRollerPressureDS()
        => new(SnapshotColumnIds.PrintingGravureUnit11SettingsImpressionRollerPressureDS, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// impression roller pressure ds of gravure unit 12
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit12SettingsImpressionRollerPressureDS()
        => new(SnapshotColumnIds.PrintingGravureUnit12SettingsImpressionRollerPressureDS, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// impression roller pressure ds of gravure unit 13
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit13SettingsImpressionRollerPressureDS()
        => new(SnapshotColumnIds.PrintingGravureUnit13SettingsImpressionRollerPressureDS, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// impression roller pressure ds of gravure unit 14
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit14SettingsImpressionRollerPressureDS()
        => new(SnapshotColumnIds.PrintingGravureUnit14SettingsImpressionRollerPressureDS, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// impression roller pressure os of gravure unit 1
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit1SettingsImpressionRollerPressureOS()
        => new(SnapshotColumnIds.PrintingGravureUnit1SettingsImpressionRollerPressureOS, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// impression roller pressure os of gravure unit 2
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit2SettingsImpressionRollerPressureOS()
        => new(SnapshotColumnIds.PrintingGravureUnit2SettingsImpressionRollerPressureOS, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// impression roller pressure os of gravure unit 3
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit3SettingsImpressionRollerPressureOS()
        => new(SnapshotColumnIds.PrintingGravureUnit3SettingsImpressionRollerPressureOS, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// impression roller pressure os of gravure unit 4
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit4SettingsImpressionRollerPressureOS()
        => new(SnapshotColumnIds.PrintingGravureUnit4SettingsImpressionRollerPressureOS, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// impression roller pressure os of gravure unit 5
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit5SettingsImpressionRollerPressureOS()
        => new(SnapshotColumnIds.PrintingGravureUnit5SettingsImpressionRollerPressureOS, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// impression roller pressure os of gravure unit 6
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit6SettingsImpressionRollerPressureOS()
        => new(SnapshotColumnIds.PrintingGravureUnit6SettingsImpressionRollerPressureOS, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// impression roller pressure os of gravure unit 7
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit7SettingsImpressionRollerPressureOS()
        => new(SnapshotColumnIds.PrintingGravureUnit7SettingsImpressionRollerPressureOS, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// impression roller pressure os of gravure unit 8
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit8SettingsImpressionRollerPressureOS()
        => new(SnapshotColumnIds.PrintingGravureUnit8SettingsImpressionRollerPressureOS, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// impression roller pressure os of gravure unit 9
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit9SettingsImpressionRollerPressureOS()
        => new(SnapshotColumnIds.PrintingGravureUnit9SettingsImpressionRollerPressureOS, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// impression roller pressure os of gravure unit 10
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit10SettingsImpressionRollerPressureOS()
        => new(SnapshotColumnIds.PrintingGravureUnit10SettingsImpressionRollerPressureOS, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// impression roller pressure os of gravure unit 11
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit11SettingsImpressionRollerPressureOS()
        => new(SnapshotColumnIds.PrintingGravureUnit11SettingsImpressionRollerPressureOS, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// impression roller pressure os of gravure unit 12
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit12SettingsImpressionRollerPressureOS()
        => new(SnapshotColumnIds.PrintingGravureUnit12SettingsImpressionRollerPressureOS, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// impression roller pressure os of gravure unit 13
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit13SettingsImpressionRollerPressureOS()
        => new(SnapshotColumnIds.PrintingGravureUnit13SettingsImpressionRollerPressureOS, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// impression roller pressure os of gravure unit 14
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit14SettingsImpressionRollerPressureOS()
        => new(SnapshotColumnIds.PrintingGravureUnit14SettingsImpressionRollerPressureOS, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// heater mode of grauvre unit 1, drying 1
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit1Drying1SettingsHeaterMode()
        => new(SnapshotColumnIds.PrintingGravureUnit1Drying1SettingsHeaterMode, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// temperature of grauvre unit 1, drying 1
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit1Drying1SettingsTemperature()
        => new(SnapshotColumnIds.PrintingGravureUnit1Drying1SettingsTemperature, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Blower speed of grauvre unit 1, drying 1
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit1Drying1SettingsBlowerSpeed()
        => new(SnapshotColumnIds.PrintingGravureUnit1Drying1SettingsBlowerSpeed, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// heater mode of grauvre unit 1, drying 2
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit1Drying2SettingsHeaterMode()
        => new(SnapshotColumnIds.PrintingGravureUnit1Drying2SettingsHeaterMode, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// temperature of grauvre unit 1, drying 2
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit1Drying2SettingsTemperature()
        => new(SnapshotColumnIds.PrintingGravureUnit1Drying2SettingsTemperature, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Blower speed of grauvre unit 1, drying 2
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit1Drying2SettingsBlowerSpeed()
        => new(SnapshotColumnIds.PrintingGravureUnit1Drying2SettingsBlowerSpeed, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// heater mode of grauvre unit 1, drying 3
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit1Drying3SettingsHeaterMode()
        => new(SnapshotColumnIds.PrintingGravureUnit1Drying3SettingsHeaterMode, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// temperature of grauvre unit 1, drying 3
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit1Drying3SettingsTemperature()
        => new(SnapshotColumnIds.PrintingGravureUnit1Drying3SettingsTemperature, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Blower speed of grauvre unit 1, drying 3
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit1Drying3SettingsBlowerSpeed()
        => new(SnapshotColumnIds.PrintingGravureUnit1Drying3SettingsBlowerSpeed, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// heater mode of grauvre unit 1, drying 4
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit1Drying4SettingsHeaterMode()
        => new(SnapshotColumnIds.PrintingGravureUnit1Drying4SettingsHeaterMode, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// temperature of grauvre unit 1, drying 4
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit1Drying4SettingsTemperature()
        => new(SnapshotColumnIds.PrintingGravureUnit1Drying4SettingsTemperature, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Blower speed of grauvre unit 1, drying 4
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit1Drying4SettingsBlowerSpeed()
        => new(SnapshotColumnIds.PrintingGravureUnit1Drying4SettingsBlowerSpeed, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// heater mode of gravure Unit 2, drying 1
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit2Drying1SettingsHeaterMode()
        => new(SnapshotColumnIds.PrintingGravureUnit2Drying1SettingsHeaterMode, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// temperature of gravure Unit 2, drying 1
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit2Drying1SettingsTemperature()
        => new(SnapshotColumnIds.PrintingGravureUnit2Drying1SettingsTemperature, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Blower speed of gravure Unit 2, drying 1
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit2Drying1SettingsBlowerSpeed()
        => new(SnapshotColumnIds.PrintingGravureUnit2Drying1SettingsBlowerSpeed, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// heater mode of gravure Unit 2, drying 2
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit2Drying2SettingsHeaterMode()
        => new(SnapshotColumnIds.PrintingGravureUnit2Drying2SettingsHeaterMode, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// temperature of gravure Unit 2, drying 2
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit2Drying2SettingsTemperature()
        => new(SnapshotColumnIds.PrintingGravureUnit2Drying2SettingsTemperature, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Blower speed of gravure Unit 2, drying 2
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit2Drying2SettingsBlowerSpeed()
        => new(SnapshotColumnIds.PrintingGravureUnit2Drying2SettingsBlowerSpeed, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// heater mode of gravure Unit 2, drying 3
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit2Drying3SettingsHeaterMode()
        => new(SnapshotColumnIds.PrintingGravureUnit2Drying3SettingsHeaterMode, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// temperature of gravure Unit 2, drying 3
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit2Drying3SettingsTemperature()
        => new(SnapshotColumnIds.PrintingGravureUnit2Drying3SettingsTemperature, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Blower speed of gravure Unit 2, drying 3
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit2Drying3SettingsBlowerSpeed()
        => new(SnapshotColumnIds.PrintingGravureUnit2Drying3SettingsBlowerSpeed, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// heater mode of gravure Unit 2, drying 4
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit2Drying4SettingsHeaterMode()
        => new(SnapshotColumnIds.PrintingGravureUnit2Drying4SettingsHeaterMode, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// temperature of gravure Unit 2, drying 4
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit2Drying4SettingsTemperature()
        => new(SnapshotColumnIds.PrintingGravureUnit2Drying4SettingsTemperature, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Blower speed of gravure Unit 2, drying 4
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit2Drying4SettingsBlowerSpeed()
        => new(SnapshotColumnIds.PrintingGravureUnit2Drying4SettingsBlowerSpeed, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// heater mode of gravure unit 3, drying 1
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit3Drying1SettingsHeaterMode()
        => new(SnapshotColumnIds.PrintingGravureUnit3Drying1SettingsHeaterMode, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// temperature of gravure unit 3, drying 1
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit3Drying1SettingsTemperature()
        => new(SnapshotColumnIds.PrintingGravureUnit3Drying1SettingsTemperature, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Blower speed of gravure unit 3, drying 1
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit3Drying1SettingsBlowerSpeed()
        => new(SnapshotColumnIds.PrintingGravureUnit3Drying1SettingsBlowerSpeed, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// heater mode of gravure unit 3, drying 2
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit3Drying2SettingsHeaterMode()
        => new(SnapshotColumnIds.PrintingGravureUnit3Drying2SettingsHeaterMode, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// temperature of gravure unit 3, drying 2
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit3Drying2SettingsTemperature()
        => new(SnapshotColumnIds.PrintingGravureUnit3Drying2SettingsTemperature, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Blower speed of gravure unit 3, drying 2
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit3Drying2SettingsBlowerSpeed()
        => new(SnapshotColumnIds.PrintingGravureUnit3Drying2SettingsBlowerSpeed, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// heater mode of gravure unit 3, drying 3
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit3Drying3SettingsHeaterMode()
        => new(SnapshotColumnIds.PrintingGravureUnit3Drying3SettingsHeaterMode, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// temperature of gravure unit 3, drying 3
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit3Drying3SettingsTemperature()
        => new(SnapshotColumnIds.PrintingGravureUnit3Drying3SettingsTemperature, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Blower speed of gravure unit 3, drying 3
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit3Drying3SettingsBlowerSpeed()
        => new(SnapshotColumnIds.PrintingGravureUnit3Drying3SettingsBlowerSpeed, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// heater mode of gravure unit 3, drying 4
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit3Drying4SettingsHeaterMode()
        => new(SnapshotColumnIds.PrintingGravureUnit3Drying4SettingsHeaterMode, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// temperature of gravure unit 3, drying 4
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit3Drying4SettingsTemperature()
        => new(SnapshotColumnIds.PrintingGravureUnit3Drying4SettingsTemperature, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Blower speed of gravure unit 3, drying 4
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit3Drying4SettingsBlowerSpeed()
        => new(SnapshotColumnIds.PrintingGravureUnit3Drying4SettingsBlowerSpeed, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// heater mode of gravure Unit 4, drying 1
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit4Drying1SettingsHeaterMode()
        => new(SnapshotColumnIds.PrintingGravureUnit4Drying1SettingsHeaterMode, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// temperature of gravure Unit 4, drying 1
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit4Drying1SettingsTemperature()
        => new(SnapshotColumnIds.PrintingGravureUnit4Drying1SettingsTemperature, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Blower speed of gravure Unit 4, drying 1
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit4Drying1SettingsBlowerSpeed()
        => new(SnapshotColumnIds.PrintingGravureUnit4Drying1SettingsBlowerSpeed, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// heater mode of gravure Unit 4, drying 2
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit4Drying2SettingsHeaterMode()
        => new(SnapshotColumnIds.PrintingGravureUnit4Drying2SettingsHeaterMode, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// temperature of gravure Unit 4, drying 2
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit4Drying2SettingsTemperature()
        => new(SnapshotColumnIds.PrintingGravureUnit4Drying2SettingsTemperature, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Blower speed of gravure Unit 4, drying 2
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit4Drying2SettingsBlowerSpeed()
        => new(SnapshotColumnIds.PrintingGravureUnit4Drying2SettingsBlowerSpeed, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// heater mode of gravure Unit 4, drying 3
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit4Drying3SettingsHeaterMode()
        => new(SnapshotColumnIds.PrintingGravureUnit4Drying3SettingsHeaterMode, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// temperature of gravure Unit 4, drying 3
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit4Drying3SettingsTemperature()
        => new(SnapshotColumnIds.PrintingGravureUnit4Drying3SettingsTemperature, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Blower speed of gravure Unit 4, drying 3
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit4Drying3SettingsBlowerSpeed()
        => new(SnapshotColumnIds.PrintingGravureUnit4Drying3SettingsBlowerSpeed, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// heater mode of gravure Unit 4, drying 4
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit4Drying4SettingsHeaterMode()
        => new(SnapshotColumnIds.PrintingGravureUnit4Drying4SettingsHeaterMode, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// temperature of gravure Unit 4, drying 4
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit4Drying4SettingsTemperature()
        => new(SnapshotColumnIds.PrintingGravureUnit4Drying4SettingsTemperature, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Blower speed of gravure Unit 4, drying 4
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit4Drying4SettingsBlowerSpeed()
        => new(SnapshotColumnIds.PrintingGravureUnit4Drying4SettingsBlowerSpeed, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// heater mode of gravure Unit 5, drying 1
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit5Drying1SettingsHeaterMode()
        => new(SnapshotColumnIds.PrintingGravureUnit5Drying1SettingsHeaterMode, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// temperature of gravure Unit 5, drying 1
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit5Drying1SettingsTemperature()
        => new(SnapshotColumnIds.PrintingGravureUnit5Drying1SettingsTemperature, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Blower speed of gravure Unit 5, drying 1
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit5Drying1SettingsBlowerSpeed()
        => new(SnapshotColumnIds.PrintingGravureUnit5Drying1SettingsBlowerSpeed, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// heater mode of gravure Unit 5, drying 2
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit5Drying2SettingsHeaterMode()
        => new(SnapshotColumnIds.PrintingGravureUnit5Drying2SettingsHeaterMode, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// temperature of gravure Unit 5, drying 2
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit5Drying2SettingsTemperature()
        => new(SnapshotColumnIds.PrintingGravureUnit5Drying2SettingsTemperature, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Blower speed of gravure Unit 5, drying 2
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit5Drying2SettingsBlowerSpeed()
        => new(SnapshotColumnIds.PrintingGravureUnit5Drying2SettingsBlowerSpeed, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// heater mode of gravure Unit 5, drying 3
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit5Drying3SettingsHeaterMode()
        => new(SnapshotColumnIds.PrintingGravureUnit5Drying3SettingsHeaterMode, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// temperature of gravure Unit 5, drying 3
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit5Drying3SettingsTemperature()
        => new(SnapshotColumnIds.PrintingGravureUnit5Drying3SettingsTemperature, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Blower speed of gravure Unit 5, drying 3
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit5Drying3SettingsBlowerSpeed()
        => new(SnapshotColumnIds.PrintingGravureUnit5Drying3SettingsBlowerSpeed, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// heater mode of gravure Unit 5, drying 4
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit5Drying4SettingsHeaterMode()
        => new(SnapshotColumnIds.PrintingGravureUnit5Drying4SettingsHeaterMode, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// temperature of gravure Unit 5, drying 4
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit5Drying4SettingsTemperature()
        => new(SnapshotColumnIds.PrintingGravureUnit5Drying4SettingsTemperature, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Blower speed of gravure Unit 5, drying 4
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit5Drying4SettingsBlowerSpeed()
        => new(SnapshotColumnIds.PrintingGravureUnit5Drying4SettingsBlowerSpeed, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// heater mode of gravure Unit 6, drying 1
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit6Drying1SettingsHeaterMode()
        => new(SnapshotColumnIds.PrintingGravureUnit6Drying1SettingsHeaterMode, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// temperature of gravure Unit 6, drying 1
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit6Drying1SettingsTemperature()
        => new(SnapshotColumnIds.PrintingGravureUnit6Drying1SettingsTemperature, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Blower speed of gravure Unit 6, drying 1
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit6Drying1SettingsBlowerSpeed()
        => new(SnapshotColumnIds.PrintingGravureUnit6Drying1SettingsBlowerSpeed, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// heater mode of gravure Unit 6, drying 2
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit6Drying2SettingsHeaterMode()
        => new(SnapshotColumnIds.PrintingGravureUnit6Drying2SettingsHeaterMode, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// temperature of gravure Unit 6, drying 2
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit6Drying2SettingsTemperature()
        => new(SnapshotColumnIds.PrintingGravureUnit6Drying2SettingsTemperature, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Blower speed of gravure Unit 6, drying 2
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit6Drying2SettingsBlowerSpeed()
        => new(SnapshotColumnIds.PrintingGravureUnit6Drying2SettingsBlowerSpeed, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// heater mode of gravure Unit 6, drying 3
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit6Drying3SettingsHeaterMode()
        => new(SnapshotColumnIds.PrintingGravureUnit6Drying3SettingsHeaterMode, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// temperature of gravure Unit 6, drying 3
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit6Drying3SettingsTemperature()
        => new(SnapshotColumnIds.PrintingGravureUnit6Drying3SettingsTemperature, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Blower speed of gravure Unit 6, drying 3
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit6Drying3SettingsBlowerSpeed()
        => new(SnapshotColumnIds.PrintingGravureUnit6Drying3SettingsBlowerSpeed, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// heater mode of gravure Unit 6, drying 4
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit6Drying4SettingsHeaterMode()
        => new(SnapshotColumnIds.PrintingGravureUnit6Drying4SettingsHeaterMode, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// temperature of gravure Unit 6, drying 4
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit6Drying4SettingsTemperature()
        => new(SnapshotColumnIds.PrintingGravureUnit6Drying4SettingsTemperature, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Blower speed of gravure Unit 6, drying 4
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit6Drying4SettingsBlowerSpeed()
        => new(SnapshotColumnIds.PrintingGravureUnit6Drying4SettingsBlowerSpeed, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// heater mode of gravure Unit 7, drying 1
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit7Drying1SettingsHeaterMode()
        => new(SnapshotColumnIds.PrintingGravureUnit7Drying1SettingsHeaterMode, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// temperature of gravure Unit 7, drying 1
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit7Drying1SettingsTemperature()
        => new(SnapshotColumnIds.PrintingGravureUnit7Drying1SettingsTemperature, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Blower speed of gravure Unit 7, drying 1
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit7Drying1SettingsBlowerSpeed()
        => new(SnapshotColumnIds.PrintingGravureUnit7Drying1SettingsBlowerSpeed, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// heater mode of gravure Unit 7, drying 2
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit7Drying2SettingsHeaterMode()
        => new(SnapshotColumnIds.PrintingGravureUnit7Drying2SettingsHeaterMode, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// temperature of gravure Unit 7, drying 2
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit7Drying2SettingsTemperature()
        => new(SnapshotColumnIds.PrintingGravureUnit7Drying2SettingsTemperature, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Blower speed of gravure Unit 7, drying 2
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit7Drying2SettingsBlowerSpeed()
        => new(SnapshotColumnIds.PrintingGravureUnit7Drying2SettingsBlowerSpeed, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// heater mode of gravure Unit 7, drying 3
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit7Drying3SettingsHeaterMode()
        => new(SnapshotColumnIds.PrintingGravureUnit7Drying3SettingsHeaterMode, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// temperature of gravure Unit 7, drying 3
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit7Drying3SettingsTemperature()
        => new(SnapshotColumnIds.PrintingGravureUnit7Drying3SettingsTemperature, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Blower speed of gravure Unit 7, drying 3
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit7Drying3SettingsBlowerSpeed()
        => new(SnapshotColumnIds.PrintingGravureUnit7Drying3SettingsBlowerSpeed, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// heater mode of gravure Unit 7, drying 4
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit7Drying4SettingsHeaterMode()
        => new(SnapshotColumnIds.PrintingGravureUnit7Drying4SettingsHeaterMode, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// temperature of gravure Unit 7, drying 4
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit7Drying4SettingsTemperature()
        => new(SnapshotColumnIds.PrintingGravureUnit7Drying4SettingsTemperature, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Blower speed of gravure Unit 7, drying 4
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit7Drying4SettingsBlowerSpeed()
        => new(SnapshotColumnIds.PrintingGravureUnit7Drying4SettingsBlowerSpeed, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// heater mode of gravure Unit 8, drying 1
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit8Drying1SettingsHeaterMode()
        => new(SnapshotColumnIds.PrintingGravureUnit8Drying1SettingsHeaterMode, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// temperature of gravure Unit 8, drying 1
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit8Drying1SettingsTemperature()
        => new(SnapshotColumnIds.PrintingGravureUnit8Drying1SettingsTemperature, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Blower speed of gravure Unit 8, drying 1
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit8Drying1SettingsBlowerSpeed()
        => new(SnapshotColumnIds.PrintingGravureUnit8Drying1SettingsBlowerSpeed, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// heater mode of gravure Unit 8, drying 2
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit8Drying2SettingsHeaterMode()
        => new(SnapshotColumnIds.PrintingGravureUnit8Drying2SettingsHeaterMode, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// temperature of gravure Unit 8, drying 2
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit8Drying2SettingsTemperature()
        => new(SnapshotColumnIds.PrintingGravureUnit8Drying2SettingsTemperature, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Blower speed of gravure Unit 8, drying 2
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit8Drying2SettingsBlowerSpeed()
        => new(SnapshotColumnIds.PrintingGravureUnit8Drying2SettingsBlowerSpeed, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// heater mode of gravure Unit 8, drying 3
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit8Drying3SettingsHeaterMode()
        => new(SnapshotColumnIds.PrintingGravureUnit8Drying3SettingsHeaterMode, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// temperature of gravure Unit 8, drying 3
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit8Drying3SettingsTemperature()
        => new(SnapshotColumnIds.PrintingGravureUnit8Drying3SettingsTemperature, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Blower speed of gravure Unit 8, drying 3
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit8Drying3SettingsBlowerSpeed()
        => new(SnapshotColumnIds.PrintingGravureUnit8Drying3SettingsBlowerSpeed, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// heater mode of gravure Unit 8, drying 4
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit8Drying4SettingsHeaterMode()
        => new(SnapshotColumnIds.PrintingGravureUnit8Drying4SettingsHeaterMode, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// temperature of gravure Unit 8, drying 4
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit8Drying4SettingsTemperature()
        => new(SnapshotColumnIds.PrintingGravureUnit8Drying4SettingsTemperature, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Blower speed of gravure Unit 8, drying 4
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit8Drying4SettingsBlowerSpeed()
        => new(SnapshotColumnIds.PrintingGravureUnit8Drying4SettingsBlowerSpeed, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// heater mode of gravure Unit 9, drying 1
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit9Drying1SettingsHeaterMode()
        => new(SnapshotColumnIds.PrintingGravureUnit9Drying1SettingsHeaterMode, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// temperature of gravure Unit 9, drying 1
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit9Drying1SettingsTemperature()
        => new(SnapshotColumnIds.PrintingGravureUnit9Drying1SettingsTemperature, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Blower speed of gravure Unit 9, drying 1
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit9Drying1SettingsBlowerSpeed()
        => new(SnapshotColumnIds.PrintingGravureUnit9Drying1SettingsBlowerSpeed, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// heater mode of gravure Unit 9, drying 2
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit9Drying2SettingsHeaterMode()
        => new(SnapshotColumnIds.PrintingGravureUnit9Drying2SettingsHeaterMode, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// temperature of gravure Unit 9, drying 2
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit9Drying2SettingsTemperature()
        => new(SnapshotColumnIds.PrintingGravureUnit9Drying2SettingsTemperature, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Blower speed of gravure Unit 9, drying 2
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit9Drying2SettingsBlowerSpeed()
        => new(SnapshotColumnIds.PrintingGravureUnit9Drying2SettingsBlowerSpeed, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// heater mode of gravure Unit 9, drying 3
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit9Drying3SettingsHeaterMode()
        => new(SnapshotColumnIds.PrintingGravureUnit9Drying3SettingsHeaterMode, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// temperature of gravure Unit 9, drying 3
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit9Drying3SettingsTemperature()
        => new(SnapshotColumnIds.PrintingGravureUnit9Drying3SettingsTemperature, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Blower speed of gravure Unit 9, drying 3
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit9Drying3SettingsBlowerSpeed()
        => new(SnapshotColumnIds.PrintingGravureUnit9Drying3SettingsBlowerSpeed, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// heater mode of gravure Unit 9, drying 4
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit9Drying4SettingsHeaterMode()
        => new(SnapshotColumnIds.PrintingGravureUnit9Drying4SettingsHeaterMode, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// temperature of gravure Unit 9, drying 4
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit9Drying4SettingsTemperature()
        => new(SnapshotColumnIds.PrintingGravureUnit9Drying4SettingsTemperature, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Blower speed of gravure Unit 9, drying 4
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit9Drying4SettingsBlowerSpeed()
        => new(SnapshotColumnIds.PrintingGravureUnit9Drying4SettingsBlowerSpeed, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// heater mode of gravure Unit 10, drying 1
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit10Drying1SettingsHeaterMode()
        => new(SnapshotColumnIds.PrintingGravureUnit10Drying1SettingsHeaterMode, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// temperature of gravure Unit 10, drying 1
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit10Drying1SettingsTemperature()
        => new(SnapshotColumnIds.PrintingGravureUnit10Drying1SettingsTemperature, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Blower speed of gravure Unit 10, drying 1
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit10Drying1SettingsBlowerSpeed()
        => new(SnapshotColumnIds.PrintingGravureUnit10Drying1SettingsBlowerSpeed, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// heater mode of gravure Unit 10, drying 2
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit10Drying2SettingsHeaterMode()
        => new(SnapshotColumnIds.PrintingGravureUnit10Drying2SettingsHeaterMode, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// temperature of gravure Unit 10, drying 2
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit10Drying2SettingsTemperature()
        => new(SnapshotColumnIds.PrintingGravureUnit10Drying2SettingsTemperature, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Blower speed of gravure Unit 10, drying 2
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit10Drying2SettingsBlowerSpeed()
        => new(SnapshotColumnIds.PrintingGravureUnit10Drying2SettingsBlowerSpeed, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// heater mode of gravure Unit 10, drying 3
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit10Drying3SettingsHeaterMode()
        => new(SnapshotColumnIds.PrintingGravureUnit10Drying3SettingsHeaterMode, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// temperature of gravure Unit 10, drying 3
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit10Drying3SettingsTemperature()
        => new(SnapshotColumnIds.PrintingGravureUnit10Drying3SettingsTemperature, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Blower speed of gravure Unit 10, drying 3
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit10Drying3SettingsBlowerSpeed()
        => new(SnapshotColumnIds.PrintingGravureUnit10Drying3SettingsBlowerSpeed, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// heater mode of gravure Unit 10, drying 4
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit10Drying4SettingsHeaterMode()
        => new(SnapshotColumnIds.PrintingGravureUnit10Drying4SettingsHeaterMode, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// temperature of gravure Unit 10, drying 4
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit10Drying4SettingsTemperature()
        => new(SnapshotColumnIds.PrintingGravureUnit10Drying4SettingsTemperature, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Blower speed of gravure Unit 10, drying 4
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit10Drying4SettingsBlowerSpeed()
        => new(SnapshotColumnIds.PrintingGravureUnit10Drying4SettingsBlowerSpeed, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// heater mode of gravure Unit 11, drying 1
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit11Drying1SettingsHeaterMode()
        => new(SnapshotColumnIds.PrintingGravureUnit11Drying1SettingsHeaterMode, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// temperature of gravure Unit 11, drying 1
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit11Drying1SettingsTemperature()
        => new(SnapshotColumnIds.PrintingGravureUnit11Drying1SettingsTemperature, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Blower speed of gravure Unit 11, drying 1
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit11Drying1SettingsBlowerSpeed()
        => new(SnapshotColumnIds.PrintingGravureUnit11Drying1SettingsBlowerSpeed, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// heater mode of gravure Unit 11, drying 2
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit11Drying2SettingsHeaterMode()
        => new(SnapshotColumnIds.PrintingGravureUnit11Drying2SettingsHeaterMode, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// temperature of gravure Unit 11, drying 2
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit11Drying2SettingsTemperature()
        => new(SnapshotColumnIds.PrintingGravureUnit11Drying2SettingsTemperature, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Blower speed of gravure Unit 11, drying 2
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit11Drying2SettingsBlowerSpeed()
        => new(SnapshotColumnIds.PrintingGravureUnit11Drying2SettingsBlowerSpeed, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// heater mode of gravure Unit 11, drying 3
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit11Drying3SettingsHeaterMode()
        => new(SnapshotColumnIds.PrintingGravureUnit11Drying3SettingsHeaterMode, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// temperature of gravure Unit 11, drying 3
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit11Drying3SettingsTemperature()
        => new(SnapshotColumnIds.PrintingGravureUnit11Drying3SettingsTemperature, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Blower speed of gravure Unit 11, drying 3
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit11Drying3SettingsBlowerSpeed()
        => new(SnapshotColumnIds.PrintingGravureUnit11Drying3SettingsBlowerSpeed, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// heater mode of gravure Unit 11, drying 4
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit11Drying4SettingsHeaterMode()
        => new(SnapshotColumnIds.PrintingGravureUnit11Drying4SettingsHeaterMode, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// temperature of gravure Unit 11, drying 4
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit11Drying4SettingsTemperature()
        => new(SnapshotColumnIds.PrintingGravureUnit11Drying4SettingsTemperature, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Blower speed of gravure Unit 11, drying 4
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit11Drying4SettingsBlowerSpeed()
        => new(SnapshotColumnIds.PrintingGravureUnit11Drying4SettingsBlowerSpeed, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// heater mode of gravure Unit 12, drying 1
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit12Drying1SettingsHeaterMode()
        => new(SnapshotColumnIds.PrintingGravureUnit12Drying1SettingsHeaterMode, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// temperature of gravure Unit 12, drying 1
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit12Drying1SettingsTemperature()
        => new(SnapshotColumnIds.PrintingGravureUnit12Drying1SettingsTemperature, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Blower speed of gravure Unit 12, drying 1
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit12Drying1SettingsBlowerSpeed()
        => new(SnapshotColumnIds.PrintingGravureUnit12Drying1SettingsBlowerSpeed, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// heater mode of gravure Unit 12, drying 2
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit12Drying2SettingsHeaterMode()
        => new(SnapshotColumnIds.PrintingGravureUnit12Drying2SettingsHeaterMode, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// temperature of gravure Unit 12, drying 2
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit12Drying2SettingsTemperature()
        => new(SnapshotColumnIds.PrintingGravureUnit12Drying2SettingsTemperature, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Blower speed of gravure Unit 12, drying 2
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit12Drying2SettingsBlowerSpeed()
        => new(SnapshotColumnIds.PrintingGravureUnit12Drying2SettingsBlowerSpeed, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// heater mode of gravure Unit 12, drying 3
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit12Drying3SettingsHeaterMode()
        => new(SnapshotColumnIds.PrintingGravureUnit12Drying3SettingsHeaterMode, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// temperature of gravure Unit 12, drying 3
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit12Drying3SettingsTemperature()
        => new(SnapshotColumnIds.PrintingGravureUnit12Drying3SettingsTemperature, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Blower speed of gravure Unit 12, drying 3
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit12Drying3SettingsBlowerSpeed()
        => new(SnapshotColumnIds.PrintingGravureUnit12Drying3SettingsBlowerSpeed, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// heater mode of gravure Unit 12, drying 4
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit12Drying4SettingsHeaterMode()
        => new(SnapshotColumnIds.PrintingGravureUnit12Drying4SettingsHeaterMode, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// temperature of gravure Unit 12, drying 4
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit12Drying4SettingsTemperature()
        => new(SnapshotColumnIds.PrintingGravureUnit12Drying4SettingsTemperature, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Blower speed of gravure Unit 12, drying 4
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit12Drying4SettingsBlowerSpeed()
        => new(SnapshotColumnIds.PrintingGravureUnit12Drying4SettingsBlowerSpeed, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// heater mode of gravure Unit 13, drying 1
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit13Drying1SettingsHeaterMode()
        => new(SnapshotColumnIds.PrintingGravureUnit13Drying1SettingsHeaterMode, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// temperature of gravure Unit 13, drying 1
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit13Drying1SettingsTemperature()
        => new(SnapshotColumnIds.PrintingGravureUnit13Drying1SettingsTemperature, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Blower speed of gravure Unit 13, drying 1
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit13Drying1SettingsBlowerSpeed()
        => new(SnapshotColumnIds.PrintingGravureUnit13Drying1SettingsBlowerSpeed, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// heater mode of gravure Unit 13, drying 2
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit13Drying2SettingsHeaterMode()
        => new(SnapshotColumnIds.PrintingGravureUnit13Drying2SettingsHeaterMode, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// temperature of gravure Unit 13, drying 2
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit13Drying2SettingsTemperature()
        => new(SnapshotColumnIds.PrintingGravureUnit13Drying2SettingsTemperature, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Blower speed of gravure Unit 13, drying 2
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit13Drying2SettingsBlowerSpeed()
        => new(SnapshotColumnIds.PrintingGravureUnit13Drying2SettingsBlowerSpeed, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// heater mode of gravure Unit 13, drying 3
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit13Drying3SettingsHeaterMode()
        => new(SnapshotColumnIds.PrintingGravureUnit13Drying3SettingsHeaterMode, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// temperature of gravure Unit 13, drying 3
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit13Drying3SettingsTemperature()
        => new(SnapshotColumnIds.PrintingGravureUnit13Drying3SettingsTemperature, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Blower speed of gravure Unit 13, drying 3
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit13Drying3SettingsBlowerSpeed()
        => new(SnapshotColumnIds.PrintingGravureUnit13Drying3SettingsBlowerSpeed, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// heater mode of gravure Unit 13, drying 4
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit13Drying4SettingsHeaterMode()
        => new(SnapshotColumnIds.PrintingGravureUnit13Drying4SettingsHeaterMode, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// temperature of gravure Unit 13, drying 4
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit13Drying4SettingsTemperature()
        => new(SnapshotColumnIds.PrintingGravureUnit13Drying4SettingsTemperature, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Blower speed of gravure Unit 13, drying 4
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit13Drying4SettingsBlowerSpeed()
        => new(SnapshotColumnIds.PrintingGravureUnit13Drying4SettingsBlowerSpeed, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// heater mode of gravure Unit 14, drying 1
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit14Drying1SettingsHeaterMode()
        => new(SnapshotColumnIds.PrintingGravureUnit14Drying1SettingsHeaterMode, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// temperature of gravure Unit 14, drying 1
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit14Drying1SettingsTemperature()
        => new(SnapshotColumnIds.PrintingGravureUnit14Drying1SettingsTemperature, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Blower speed of gravure Unit 14, drying 1
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit14Drying1SettingsBlowerSpeed()
        => new(SnapshotColumnIds.PrintingGravureUnit14Drying1SettingsBlowerSpeed, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// heater mode of gravure Unit 14, drying 2
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit14Drying2SettingsHeaterMode()
        => new(SnapshotColumnIds.PrintingGravureUnit14Drying2SettingsHeaterMode, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// temperature of gravure Unit 14, drying 2
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit14Drying2SettingsTemperature()
        => new(SnapshotColumnIds.PrintingGravureUnit14Drying2SettingsTemperature, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Blower speed of gravure Unit 14, drying 2
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit14Drying2SettingsBlowerSpeed()
        => new(SnapshotColumnIds.PrintingGravureUnit14Drying2SettingsBlowerSpeed, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// heater mode of gravure Unit 14, drying 3
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit14Drying3SettingsHeaterMode()
        => new(SnapshotColumnIds.PrintingGravureUnit14Drying3SettingsHeaterMode, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// temperature of gravure Unit 14, drying 3
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit14Drying3SettingsTemperature()
        => new(SnapshotColumnIds.PrintingGravureUnit14Drying3SettingsTemperature, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Blower speed of gravure Unit 14, drying 3
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit14Drying3SettingsBlowerSpeed()
        => new(SnapshotColumnIds.PrintingGravureUnit14Drying3SettingsBlowerSpeed, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// heater mode of gravure Unit 14, drying 4
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit14Drying4SettingsHeaterMode()
        => new(SnapshotColumnIds.PrintingGravureUnit14Drying4SettingsHeaterMode, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// temperature of gravure Unit 14, drying 4
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit14Drying4SettingsTemperature()
        => new(SnapshotColumnIds.PrintingGravureUnit14Drying4SettingsTemperature, endTime, machineId, timeRanges, machineQueryTimestamp);

    /// <summary>
    /// Blower speed of gravure Unit 14, drying 4
    /// [Source: MachineSnapshot]
    /// </summary>
    public NumericSnapshotValuesDuringProduction GravureUnit14Drying4SettingsBlowerSpeed()
        => new(SnapshotColumnIds.PrintingGravureUnit14Drying4SettingsBlowerSpeed, endTime, machineId, timeRanges, machineQueryTimestamp);
}