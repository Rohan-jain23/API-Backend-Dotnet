using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.DataLoaders;
using FrameworkAPI.Schema.Misc;

namespace FrameworkAPI.Services.Interfaces;

public interface IMachineShiftService
{
    Task<List<MachineShift>?> GetMachineShifts(
        ProductionPeriodByTimestampCacheDataLoader productionPeriodsCacheDataLoader,
        UserNameCacheDataLoader userNameCacheDataLoader,
        string machineId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}