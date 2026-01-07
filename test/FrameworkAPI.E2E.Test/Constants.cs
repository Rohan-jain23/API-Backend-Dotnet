namespace FrameworkAPI.E2E.Test;

public static class Constants
{
    public static class Paths
    {
        public static class Printing
        {
            public static class FlexoPrint
            {
                public const string Customer = "Auftragsverwaltung;AuftragKunde";
                public const string JobSize = "Auftragsverwaltung;AuftragLaengeSoll";
                public const string Speed = "Maschine;MaschineGeschwindigkeitIst";
            }

            public static class OldFlexoPrint
            {
                public const string Customer = "AllgTEXTKundName";
                public const string JobSize = "00010/AuW1SP03SollMe__";
                public const string JobSizeAlternative = "00015/AuW1SP03SollMe__";
                public const string Speed = "BasFSPEDMashSped";
            }

            public static class GravurePrint
            {
                public const string Customer = "Auftragsverwaltung;AuftragKunde";
                public const string JobSize = "Auftragsverwaltung;AuftragLaengeSoll";
                public const string Speed = "Maschine;MaschineGeschwindigkeitIst";
            }
        }

        public static class PaperSack
        {
            public const string Customer = "KUNDE";
            public const string JobSize = "STUECKZAHL";
            public const string MaterialInformation = "PRODUKT_INFOTEXT_2";
            public const string MaterialText = "BEZ";
            public const string Speed = "Gdi_PLC_VB_Geschwindigkeit";

            public static class Tuber
            {
                public const string TubeLength = "Gdi_VB_PLC_Schlauchlaenge";
                public const string TubeWidth = "Gdi_VB_PLC_Schlauchbreite";
            }

            public static class Bottomer
            {
                public const string ValveUnit1IsActive = "Gb_VB_PLC_Ventil1Angewaehlt";
                public const string ValveUnit1Layers = "Gdi_VB_PLC_Ventil1AnzahlZettelbahnen";
                public const string SackLength = "Glr_VB_PLC_Sacklaenge";
                public const string SackWidth = "Glr_VB_PLC_Schlauchbreite";
                public const string StandUpBottomWidth = "Glr_VB_PLC_BodenbreiteAS";
                public const string ValveBottomWidth = "Glr_VB_PLC_BodenbreiteBS";
            }
        }

        public static class Extrusion
        {
            public const string Customer = "AufAKund";
            public const string JobSize = "AufAALen";
            public const string Thickness = "AufASDik";
            public const string ThroughputRate = "GravDuIW";
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
            public const string WinderAContactDrive = "WikAPrIwKont";
            public const string WinderBContactDrive = "WikBPrIwKont";

            public static class BlowFilm
            {
                public const string Width = "BreiRAktSoll";
                public const string TwoSigma = "ProfQPRASg2R";
                public const string LineSpeed = "MashAllgVIst";
                public const string IsThicknessGaugeOn = "ProfIsOn";
            }

            public static class CastFilm
            {
                public const string Width = "AufASNuz";
                public const string TwoSigma = "ProfCRSg";
                public const string LineSpeed = "ChilKuWaIstw";
                public const string IsThicknessGaugeOn = "ProfIsOn";
            }
        }
    }
}