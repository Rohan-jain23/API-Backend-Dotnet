using System;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.Models;
using FrameworkAPI.Models.Events;
using Microsoft.VisualStudio.Threading;

namespace FrameworkAPI.Services.Interfaces;

public interface IOpcUaServerTimeCachingService
{
    event AsyncEventHandler<MachineTimeChangedEventArgs>? CacheChanged;

    Task<DataResult<DateTime?>> Get(string machineId, CancellationToken cancellationToken);
}