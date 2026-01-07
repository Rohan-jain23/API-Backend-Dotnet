using System;
using System.Collections.Generic;
using MachineDataHandler = WuH.Ruby.MachineDataHandler.Client;
using MachineFamily = FrameworkAPI.Schema.Misc.MachineFamily;

namespace FrameworkAPI.Test.TestHelpers;

public static class MachineMock
{
    public static MachineDataHandler.Machine GenerateCastFilm(string eqNumber)
    {
        return new MachineDataHandler.Machine
        {
            MachineId = eqNumber,
            Name = "DummyCastFilm",
            MachineConfigSchemaVersion = 1,
            Features = new List<MachineDataHandler.MachineFeature>
            {
                new()
                {
                    Name = "DummyFeature",
                    FeatureVersion = 1
                }
            },
            LastModifiedDate = new DateTime(year: 2023, month: 1, day: 15).ToUniversalTime(),
            MachineType = "Extrusion",
            MachineFamily = MachineFamily.CastFilm.ToString(),
            BusinessUnit = MachineDataHandler.BusinessUnit.Extrusion,
            MachineInfosSchemaVersion = 1,
            Deactivated = false,
            OpcUaPort = 4933,
            Host = "test.wuh-intern.de",
            WhMachine = true,
            NextServerPublicKey = "Dummy NextServerPublicKey"
        };
    }

    public static MachineDataHandler.Machine GenerateBlowFilm(string eqNumber)
    {
        return new MachineDataHandler.Machine
        {
            MachineId = eqNumber,
            Name = "DummyBlowFilm",
            MachineConfigSchemaVersion = 1,
            Features =
            [
                new()
                {
                    Name = "DummyFeature",
                    FeatureVersion = 1
                }
            ],
            LastModifiedDate = new DateTime(year: 2023, month: 1, day: 15).ToUniversalTime(),
            MachineType = "Extrusion",
            MachineFamily = MachineFamily.BlowFilm.ToString(),
            BusinessUnit = MachineDataHandler.BusinessUnit.Extrusion,
            MachineInfosSchemaVersion = 1,
            Deactivated = false,
            OpcUaPort = 4933,
            Host = "test.wuh-intern.de",
            WhMachine = true,
            NextServerPublicKey = "Dummy NextServerPublicKey"
        };
    }

    public static MachineDataHandler.Machine GenerateFlexoPrint(string eqNumber)
    {
        return new MachineDataHandler.Machine
        {
            MachineId = eqNumber,
            Name = "DummyFlexoPrint",
            MachineConfigSchemaVersion = 1,
            Features = new List<MachineDataHandler.MachineFeature>
            {
                new()
                {
                    Name = "DummyFeature",
                    FeatureVersion = 1
                }
            },
            LastModifiedDate = new DateTime(year: 2023, month: 1, day: 15).ToUniversalTime(),
            MachineType = "Printing",
            MachineFamily = MachineFamily.FlexoPrint.ToString(),
            BusinessUnit = MachineDataHandler.BusinessUnit.Printing,
            MachineInfosSchemaVersion = 1,
            Deactivated = false,
            OpcUaPort = 4933,
            Host = "test.wuh-intern.de",
            WhMachine = true,
            NextServerPublicKey = "Dummy NextServerPublicKey"
        };
    }

    public static MachineDataHandler.Machine GeneratePaperSackTuber(string eqNumber)
    {
        return new MachineDataHandler.Machine
        {
            MachineId = eqNumber,
            Name = "DummyPaperSackTuber",
            MachineConfigSchemaVersion = 1,
            Features = new List<MachineDataHandler.MachineFeature>
            {
                new()
                {
                    Name = "DummyFeature",
                    FeatureVersion = 1
                }
            },
            LastModifiedDate = new DateTime(year: 2023, month: 1, day: 15).ToUniversalTime(),
            MachineType = "PaperSack",
            MachineFamily = MachineFamily.PaperSackTuber.ToString(),
            BusinessUnit = MachineDataHandler.BusinessUnit.PaperSack,
            MachineInfosSchemaVersion = 1,
            Deactivated = false,
            OpcUaPort = 4933,
            Host = "test.wuh-intern.de",
            WhMachine = true,
            NextServerPublicKey = "Dummy NextServerPublicKey"
        };
    }
}