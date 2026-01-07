using System;
using System.Collections.Generic;
using WuH.Ruby.MaterialDataHandler.Client.Enums.Lot;
using WuH.Ruby.MaterialDataHandler.Client.Models.Lot;

namespace FrameworkAPI.Test.TestHelpers;

public static class MaterialLotMock
{
    public static Lot GenerateMaterialLot(string eqNumber, string id = "WIND001")
    {
        return new Lot
        {
            GeneralProperties = new GeneralProperties
            {
                EndTime = new DateTime(year: 2023, month: 1, day: 15).ToUniversalTime(),
                EventType = EventType.NewProducedMaterial,
                Id = id,
                MachineId = eqNumber,
                Manufacturer = "MockedManufacturer",
                ManufacturerId = "MockedManufacturerId",
                MaterialDefinitionId = "MockedMaterialDefinitionId",
                MaterialLotsConsumed = new List<IdOfTheConsumedMaterialLot>(),
                ParentLot = null,
                ProcessingInformation = null,
                Quantity = 100.00,
                StartTime = new DateTime(year: 2023, month: 1, day: 14).ToUniversalTime(),
                Sublots = new List<string>(),
                TimeOfSampling = new DateTime(year: 2023, month: 1, day: 15).ToUniversalTime()
            },
            MaterialClass = TypeOfMaterial.ExtrudedRoll
        };
    }
}