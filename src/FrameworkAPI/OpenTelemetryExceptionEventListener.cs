using System.Diagnostics.Tracing;
using NLog;
using WuH.Ruby.Common.Core;

namespace FrameworkAPI;

public class OpenTelemetryExceptionEventListener : EventListener
{
    private ILogger? _logger;

    protected override void OnEventSourceCreated(EventSource eventSource)
    {
        if (eventSource.Name == "OpenTelemetry-Exporter-OpenTelemetryProtocol")
        {
            _logger = LogManager.GetCurrentClassLogger();
            _logger.Info("Starting to log events from 'OpenTelemetry-Exporter-OpenTelemetryProtocol'.");
            EnableEvents(eventSource, EventLevel.Informational);
        }
    }

    protected override void OnEventWritten(EventWrittenEventArgs eventData)
    {
        _logger?.Warn($"{eventData.EventSource}.{eventData.EventName}: {eventData.Message} {eventData.Payload.ToLogString()}");
    }
}