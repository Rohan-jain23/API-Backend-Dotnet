# Licence Checks

## Global Licences

Global Licences e.g. `GO` or `Anilox` are for a complete RUBY-Instance and are not tied to a machine.

To check for a licence on `Queries`, `Fields`, `Mutation` or subscription the attribute `LicenceCheck` with the 
required licence has to be added. 

```csharp
[ExtendObjectType("Query")]
public class PhysicalAssetQuery
{
    [LicenceCheck(Licence = Constants.LicensesApplications.Anilox)]
    public Task<IEnumerable<PhysicalAsset>> GetPhysicalAssets(
        [Service] IPhysicalAssetService physicalAssetService,
        CancellationToken cancellationToken)
    {
        return physicalAssetService.GetAllPhysicalAssets(cancellationToken);
    }
    ...
}
```

## Machine Licences

Machine Licences e.g. `Track` are tied to a machine and are different to another machines. The machines are identified
by the `machineId` e.g. `EQ12345`.

To check for a licence on `Queries`, `Fields`, `Mutation` or subscription the attribute `LicenceCheck` without the licence
has to be added. In the query the `ILicenceGuard` has to be called with `machineId`: <br>
If the machine has a valid licence nothing happens, <br>
if not a `InvalidLicenceException` is thrown which is caught by the `LicenceCheck` attribute and is translated to an `Error`.


```csharp
[ExtendObjectType("Query")]
public class MachineQuery
{
    [Authorize(Roles = ["go-general"])]
    [LicenceCheck]
    public async Task<Machine> GetMachine(
        [Service] ILicenceGuard licenceGuard,
        [Service] IMachineService machineService,
        string machineId,
        DateTime? timestamp,
        CancellationToken cancellationToken)
    {
        await licenceGuard.CheckMachineLicence(machineId, Constants.LicensesApplications.Track);
        var machine = await machineService.GetMachine(machineId, cancellationToken);
        machine.QueryTimestamp = timestamp?.ToUniversalTime();
        return machine;
    }
    ...
}
```