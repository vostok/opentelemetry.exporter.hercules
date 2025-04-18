using System;
using JetBrains.Annotations;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using Vostok.Hercules.Client.Abstractions;
using Vostok.OpenTelemetry.Exporter.Hercules.Logging;

namespace Vostok.OpenTelemetry.Exporter.Hercules;

[PublicAPI]
public sealed class HerculesLogExporter(IHerculesSink sink, Func<HerculesLogExporterOptions> optionsProvider)
    : BaseExporter<LogRecord>
{
    private Resource? _resource;

    private bool _shutdownCalled;

    public override ExportResult Export(in Batch<LogRecord> batch)
    {
        _resource ??= ParentProvider.GetResource();

        var options = optionsProvider();
        if (options.Enabled && !_shutdownCalled)
        {
            foreach (var logRecord in batch)
                sink.Put(options.Stream, builder => builder.BuildLogRecord(logRecord, _resource, options.FormatProvider));
        }

        return ExportResult.Success;
    }

    // note (ponomaryovigor, 28.04.2025):
    // Fix has been added only for this exporter for simplicity reasons (only logs exporting faces "use after dispose" issue). 
    // Feel free to add same logic to activity and metric exporting if needed.
    protected override bool OnShutdown(int timeoutMilliseconds)
    {
        _shutdownCalled = true;
        return base.OnShutdown(timeoutMilliseconds);
    }
}