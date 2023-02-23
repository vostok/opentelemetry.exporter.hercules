using System.Diagnostics;
using JetBrains.Annotations;
using Vostok.Hercules.Client.Abstractions.Events;

namespace Vostok.OpenTelemetry.Exporter.Hercules;

[PublicAPI]
public class HerculesActivityExporter : HerculesExporter<Activity>
{
    private readonly HerculesActivityExporterSettings settings;

    public HerculesActivityExporter(HerculesActivityExporterSettings settings)
        : base(settings) =>
        this.settings = settings;

    protected override string SelectStream(Activity @event) =>
        settings.Stream;

    protected override void BuildEvent(IHerculesEventBuilder builder, Activity @event)
    {
        
    }
}