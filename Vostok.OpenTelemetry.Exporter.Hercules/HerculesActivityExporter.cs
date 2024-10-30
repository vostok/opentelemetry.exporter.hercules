using System;
using System.Diagnostics;
using JetBrains.Annotations;
using OpenTelemetry;
using Vostok.Hercules.Client.Abstractions;
using Vostok.OpenTelemetry.Exporter.Hercules.Builders;

namespace Vostok.OpenTelemetry.Exporter.Hercules;

[PublicAPI]
public class HerculesActivityExporter : BaseExporter<Activity>
{
    private readonly IHerculesSink sink;
    private readonly Func<HerculesActivityExporterOptions> optionsProvider;

    public HerculesActivityExporter(IHerculesSink sink, Func<HerculesActivityExporterOptions> optionsProvider)
    {
        this.sink = sink;
        this.optionsProvider = optionsProvider;
    }

    public override ExportResult Export(in Batch<Activity> batch)
    {
        foreach (var activity in batch)
        {
            sink.Put(optionsProvider().Stream,
                builder =>
                    HerculesActivityBuilder.Build(activity, ParentProvider.GetResource(), builder, optionsProvider().FormatProvider));
        }

        return ExportResult.Success;
    }
}