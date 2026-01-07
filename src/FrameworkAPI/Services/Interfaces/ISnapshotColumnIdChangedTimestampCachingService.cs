using System;
using System.Threading;
using System.Threading.Tasks;

namespace FrameworkAPI.Services.Interfaces;

public interface ISnapshotColumnIdChangedTimestampCachingService
{
    Task<DateTime?> Get(
        string machineId, string columnId, CancellationToken cancellationToken);
}