using System;
using System.Collections.Generic;
using WuH.Ruby.MachineSnapShooter.Client;

namespace FrameworkAPI;

public static class Constants
{
    public static class MachineTrend
    {
        // 8 hours + 1 minute
        //
        // Example:
        // 00:00:00 -> 08:00:00
        // Total minutes (time difference): 480
        // Total minutes: 481
        public static readonly TimeSpan TrendTimeSpan = TimeSpan.FromMinutes(8 * 60 + 1);

        public static readonly IReadOnlyList<string> TrendingSnapshotColumnIds =
        [
            SnapshotColumnIds.ExtrusionThroughput,
            SnapshotColumnIds.ExtrusionSpeed,
            SnapshotColumnIds.PrintingSpeed,
            SnapshotColumnIds.PaperSackSpeed
        ];
    }

    public static class Units
    {
        public static readonly IReadOnlyDictionary<string, string> SpecialUnitsTranslation =
            new Dictionary<string, string>
            {
                { "unit.items", "unit.items" },
                { "STK", "unit.items" },
                { "unit.itemsPerMinute", "unit.itemsPerMinute" },
                { "STKMIN", "unit.itemsPerMinute" },
                { "LABEL.SLITROLLS", "LABEL.SLITROLLS" },
                { "Nutzen", "LABEL.SLITROLLS" },
            };
    }

    public static class Identifiers
    {
        public const string Profile = "ProfQPRA";
        public const string ProfileMeanValue = "ProfQPRA_Mean";
    }

    public static class LastPartOfPath
    {
        public const string PrimaryProfile = "ProfQPRA";
        public const string PrimaryProfileMeanValue = "ProfQPRAMean";
        public const string PrimaryProfileTwoSigma = "ProfQPRASg2R";
        public const string MdoProfileA = "PrfAQPRA";
        public const string MdoProfileAMeanValue = "PrfAQPRAMean";
        public const string MdoProfileATwoSigma = "PrfAQPRASg2R";
        public const string MdoProfileB = "PrfBQPRA";
        public const string MdoProfileBMeanValue = "PrfBQPRAMean";
        public const string MdoProfileBTwoSigma = "PrfBQPRASg2R";
        public const string ControlElements = "PRg1StgP";
        public const string ProfileControl = "PRg1OnOf";
        public const string ThicknessGauge = "ProfIsOn";
        public const string WinderAContactDrive = "WikAPrIwKont";
        public const string WinderBContactDrive = "WikBPrIwKont";
        public const string ProducedMaterialNotifier = "ProducedMaterialNotifier";
    }

    public static class LicensesApplications
    {
        public const string Anilox = "anilox";
        public const string Check = "check";
        public const string Connect4Flow = "connect4flow";
        public const string Go = "go";
        public const string Track = "track";
    }
}