using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.Schema.Misc;
using WuH.Ruby.ProductionPeriodsDataHandler.Client;

namespace FrameworkAPI.Services.Interfaces;

public interface ITrackProductionHistoryService
{
    Task<List<TrackHistoryEntry>?> GetProductionHistory(
        JobInfo jobInfo,
        CancellationToken cancellationToken);
}