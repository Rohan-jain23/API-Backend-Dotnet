using System;
using WuH.Ruby.ProductionPeriodsDataHandler.Client;

namespace FrameworkAPI.Schema.ProducedJob;

/// <summary>
/// Produced job entity of other machines.
/// </summary>
public class OtherProducedJob(JobInfo jobInfo, DateTime? machineQueryTimestamp) : ProducedJob(jobInfo, machineQueryTimestamp)
{
}