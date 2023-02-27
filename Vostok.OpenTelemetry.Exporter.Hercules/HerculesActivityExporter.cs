using System;
using System.Diagnostics;
using JetBrains.Annotations;
using OpenTelemetry;
using Vostok.Hercules.Client.Abstractions;
using Vostok.Hercules.Client.Abstractions.Events;
using Vostok.OpenTelemetry.Exporter.Hercules.Builders;

namespace Vostok.OpenTelemetry.Exporter.Hercules;

[PublicAPI]
public class HerculesActivityExporter : HerculesExporter<Activity>
{
    private readonly Func<HerculesActivityExporterOptions> optionsProvider;

    public HerculesActivityExporter(IHerculesSink sink, Func<HerculesActivityExporterOptions> optionsProvider)
        : base(sink, optionsProvider) =>
        this.optionsProvider = optionsProvider;

    protected override string SelectStream(Activity @event) =>
        optionsProvider().Stream;

    protected override void BuildEvent(IHerculesEventBuilder builder, Activity @event) =>
        HerculesActivityBuilder.Build(@event, ParentProvider.GetResource(), builder, optionsProvider().FormatProvider);
}