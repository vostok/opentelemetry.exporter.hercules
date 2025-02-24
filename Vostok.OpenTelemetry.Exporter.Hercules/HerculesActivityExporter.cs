using System;
using System.Diagnostics;
using JetBrains.Annotations;
using OpenTelemetry;
using OpenTelemetry.Resources;
using Vostok.Hercules.Client.Abstractions;
using Vostok.OpenTelemetry.Exporter.Hercules.Tracing;

namespace Vostok.OpenTelemetry.Exporter.Hercules;

[PublicAPI]
public class HerculesActivityExporter(IHerculesSink sink, Func<HerculesActivityExporterOptions> optionsProvider)
    : BaseExporter<Activity>
{
    private Resource? _resource;

    public override ExportResult Export(in Batch<Activity> batch)
    {
        _resource ??= ParentProvider.GetResource();
        var options = optionsProvider();

        foreach (var activity in batch)
            sink.Put(options.Stream, builder => builder.BuildActivity(activity, _resource, options.FormatProvider));

        return ExportResult.Success;
    }
}