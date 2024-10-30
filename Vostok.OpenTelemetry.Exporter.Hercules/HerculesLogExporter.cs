using System;
using JetBrains.Annotations;
using OpenTelemetry;
using OpenTelemetry.Logs;
using Vostok.Hercules.Client.Abstractions;

namespace Vostok.OpenTelemetry.Exporter.Hercules;

[PublicAPI]
public sealed class HerculesLogExporter : BaseExporter<LogRecord>
{
    private readonly IHerculesSink sink;
    private readonly Func<HerculesLogExporterOptions> optionsProvider;

    public HerculesLogExporter(IHerculesSink sink, Func<HerculesLogExporterOptions> optionsProvider)
    {
        this.sink = sink;
        this.optionsProvider = optionsProvider;
    }

    public override ExportResult Export(in Batch<LogRecord> batch) =>
        ExportResult.Success;
}