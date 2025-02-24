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

    public override ExportResult Export(in Batch<LogRecord> batch)
    {
        _resource ??= ParentProvider.GetResource();
        var options = optionsProvider();

        foreach (var logRecord in batch)
            sink.Put(options.Stream, builder => builder.BuildLogRecord(logRecord, _resource));

        return ExportResult.Success;
    }
}