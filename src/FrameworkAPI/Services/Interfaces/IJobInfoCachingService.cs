using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.Models;
using WuH.Ruby.ProductionPeriodsDataHandler.Client;

namespace FrameworkAPI.Services.Interfaces;

public interface IJobInfoCachingService
{
    Task<DataResult<JobInfo?>> GetLatest(string machineId, CancellationToken cancellationToken);
}