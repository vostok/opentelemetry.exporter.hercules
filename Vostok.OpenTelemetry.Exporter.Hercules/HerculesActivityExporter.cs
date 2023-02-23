using System.Diagnostics;
using JetBrains.Annotations;
using Vostok.Hercules.Client.Abstractions;
using Vostok.Hercules.Client.Abstractions.Events;

namespace Vostok.OpenTelemetry.Exporter.Hercules;

[PublicAPI]
public class HerculesActivityExporter : HerculesExporter<Activity>
{
    private readonly HerculesActivityExporterOptions options;

    public HerculesActivityExporter(IHerculesSink sink, HerculesActivityExporterOptions options)
        : base(sink, options) =>
        this.options = options;

    protected override string SelectStream(Activity @event) =>
        options.Stream;

    protected override void BuildEvent(IHerculesEventBuilder builder, Activity @event)
    {
        
    }
}