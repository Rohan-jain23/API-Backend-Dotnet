using System;

namespace FrameworkAPI.Helpers;

internal static class ProducedJobsHelper
{
    public static string SerializeProducedJobId(string machineId, string jobId)
    {
        return $"{machineId}_{jobId}";
    }

    public static (string MachineId, string JobId) DeserializeProducedJobId(string producedJobId)
    {
        var indexOfDelimiter = producedJobId.IndexOf("_", StringComparison.Ordinal);

        if (indexOfDelimiter <= 0 || indexOfDelimiter == producedJobId.Length - 1)
        {
            throw new ArgumentException($"'{producedJobId}' is invalid", nameof(producedJobId));
        }

        var machineId = producedJobId[..indexOfDelimiter];
        var jobId = producedJobId[(indexOfDelimiter + 1)..];

        return (MachineId: machineId, JobId: jobId);
    }
}