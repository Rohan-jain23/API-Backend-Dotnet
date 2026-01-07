using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.Exceptions;
using FrameworkAPI.Schema.Misc;
using FrameworkAPI.Services.Interfaces;
using WuH.Ruby.Common.Track;
using WuH.Ruby.ProductionPeriodsDataHandler.Client;

namespace FrameworkAPI.Services;

public class TrackProductionHistoryService(IHistoryEntryService historyEntryService) : ITrackProductionHistoryService
{
    public async Task<List<TrackHistoryEntry>?> GetProductionHistory(
        JobInfo jobInfo,
        CancellationToken cancellationToken)
    {
        var response = await historyEntryService.GetJobHistory(jobInfo, cancellationToken: cancellationToken);
        if (response.HasError)
            throw new InternalServiceException(response.Error);

        return response.Items.Select(TrackHistoryEntry.CreateInstance).ToList();
    }
}