using System;
using System.Threading;
using System.Threading.Tasks;
using FrameworkAPI.Models;
using FrameworkAPI.Models.Events;
using Microsoft.VisualStudio.Threading;

namespace FrameworkAPI.Services.Interfaces;

public interface IMachineTimeService
{
    event AsyncEventHandler<MachineTimeChangedEventArgs>? MachineTimeChanged;

    Task<DataResult<DateTime?>> Get(string machineId, CancellationToken cancellationToken);
}