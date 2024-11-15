using System;
using System.Diagnostics;
using JetBrains.Annotations;
using OpenTelemetry;
using OpenTelemetry.Resources;
using Vostok.Hercules.Client.Abstractions;
using Vostok.OpenTelemetry.Exporter.Hercules.Builders;

namespace Vostok.OpenTelemetry.Exporter.Hercules;

[PublicAPI]
public class HerculesActivityExporter : BaseExporter<Activity>
{
    private readonly IHerculesSink sink;
    private readonly Func<HerculesActivityExporterOptions> optionsProvider;
    private Resource? resource;

    public HerculesActivityExporter(IHerculesSink sink, Func<HerculesActivityExporterOptions> optionsProvider)
    {
        this.sink = sink;
        this.optionsProvider = optionsProvider;
    }

    public override ExportResult Export(in Batch<Activity> batch)
    {
        resource ??= ParentProvider.GetResource();
        var options = optionsProvider();

        foreach (var activity in batch)
            sink.Put(options.Stream, builder => builder.BuildActivity(activity, resource, options.FormatProvider));

        return ExportResult.Success;
    }
}