# Providing lot data in the FrameworkAPI

## General

```plantuml
'!theme blueprint
'!theme minty
'!theme sandstone

skinparam linetype ortho
skinparam nodesep 100
skinparam ranksep 100
skinparam arrowfontsize 15
skinparam arrowmessagealignment top

skinparam {
    rectangle<<DUMMY>> {
        borderColor transparent
        stereotypeFontSize 0
        fontSize 0
    }
}

rectangle {
    rectangle padding <<DUMMY>>{
    rectangle padding <<DUMMY>>{
    rectangle padding <<DUMMY>>{
    rectangle padding <<DUMMY>>{

    class ProducedLot {
        + Id: String
        + MachineId: String
        + JobId: String
        + Product: String
        + StartTime: DateTime
        + EndTime: DateTime
        + Quantity: Int
/'
        + Sublots: List<ProducedLot>
        + ParentLot: ProducedLot
        + TestResults: List<MaterialTestResult>
'/
    }

    class PrintingLot as "PrintingProducedRoll" {}
    class PaperSack as "ProducedPaperSack" {}

    class ExtrusionLot as "ExtrusionProducedRoll" {
        + Thickness.TwoSigma: Float
        + Thickness.Average: Float
    }

    ProducedLot <|-d--- PrintingLot
    ProducedLot <|-d-- PaperSack
    ProducedLot <|-d- ExtrusionLot
/'
    ProducedLot "<color:#003300>0..1" -[#003300]- "<color:#003300>*" ProducedLot : "<color:#003300>parentLot"

    ProducedLot "<color:#000033>1" *-[#000033]- "*" ProducedLot : "<color:#000033>sublots"
'/
    }
    }
    }
    }
}
```

Data from “GeneralProperties” is taken from the original object in the lots collection. For other (extrusion-specific?) parameters, data is retrieved from the snapshots using the timestamps, where possible. This allows for the correct values to be displayed when using an MDO. Functions like “valueWithLongestDuration” or “average” are used appropriately to determine values for the whole roll (for example, for 2-Sigma or target thickness).

Note: In theory, embedding objects in parentLot/sublot can create infinite loops.

**Example** ExtrusionProducedRoll
```json
{
    "id": "WINDM0999022025030409193201",
    "machineId": "EQ99902",
    "jobId": "12345-1",
    "product": "Stretch2000",
    "startTime": "2025-01-10T14:23:51.000Z",
    "endTime": "2025-01-10T15:11:14Z.000",
    "quantity": {
        "value": 2000,
        "unit": "m"
    },
    "thicknessActual": {
        "twoSigma": {
            "value": 1.53, // average value for the timeframe of the roll from the most relevant 2-sigma
            "unit": "%"
        },
        "average": {
            "value": 25, // value with longest duration for the timeframe of the roll from most relevant set thickness
            "unit": "µm"
        },
    }

//    "sublots": [],
//    "parentLot": {},
//    "testResults": [],
}
```

Metadata is used from the source that corresponds to the values
- "GeneralProperties" -> ProducedMaterialNotifier metadata
- "Other" -> Snapshooter/parameter metadata

## Labordaten

TBD

```plantuml
'!theme blueprint
'!theme minty
'!theme sandstone

skinparam linetype ortho
skinparam nodesep 100
skinparam ranksep 100
skinparam arrowfontsize 15
skinparam arrowmessagealignment top

skinparam {
    rectangle<<DUMMY>> {
        borderColor transparent
        stereotypeFontSize 0
        fontSize 0
    }
}

rectangle {
    rectangle padding <<DUMMY>>{
    rectangle padding <<DUMMY>>{
    rectangle padding <<DUMMY>>{
    rectangle padding <<DUMMY>>{

    class ProducedLot {
        + Id: String
        + MachineId: String
        + JobId: String
        + Product: String
        + StartTime: DateTime
        + EndTime: DateTime
        + Quantity: Int
        + TestResults: List<MaterialTestResult>
/'
        + Sublots: List<ProducedLot>
        + ParentLot: ProducedLot
'/
    }

    class PrintingLot as "PrintingProducedRoll" {}
    class PaperSack as "ProducedPaperSack" {}

    class ExtrusionLot as "ExtrusionProducedRoll" {
    }

    together {
        class MaterialTestResult {
            + TestSpecification: MaterialTestSpecification
            + Lot: ProducedLot
        }

        class MaterialTestSpecification {
            + Name: String
        }
    }

    MaterialTestResult "*" -l- "1" ProducedLot : testResults
    MaterialTestSpecification "1" -u- "*" MaterialTestResult : "<color:#000033>testSpecification"

    ProducedLot <|-d--- PrintingLot
    ProducedLot <|-d-- PaperSack
    ProducedLot <|-d- ExtrusionLot

    'ProducedLot "<color:#003300>0..1" -[#003300]- "<color:#003300>*" ProducedLot : "<color:#003300>parentLot"

    'ProducedLot "<color:#000033>1" *-[#000033]- "*" ProducedLot : "<color:#000033>sublots"

    }
    }
    }
    }
}
```