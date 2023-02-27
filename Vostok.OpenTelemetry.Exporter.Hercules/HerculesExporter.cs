using System;
using JetBrains.Annotations;
using OpenTelemetry;
using Vostok.Hercules.Client.Abstractions;
using Vostok.Hercules.Client.Abstractions.Events;

namespace Vostok.OpenTelemetry.Exporter.Hercules;

[PublicAPI]
public abstract class HerculesExporter<T> : BaseExporter<T>
    where T : class
{
    private readonly IHerculesSink sink;
    private readonly Func<HerculesActivityExporterOptions> optionsProvider;

    protected HerculesExporter(IHerculesSink sink, Func<HerculesActivityExporterOptions> optionsProvider)
    {
        this.sink = sink;
        this.optionsProvider = optionsProvider;
    }

    public override ExportResult Export(in Batch<T> batch)
    {
        Console.WriteLine($"Export {batch.Count} events.");
        
        foreach (var @event in batch)
            sink.Put(SelectStream(@event), b => BuildEvent(b, @event));

        return ExportResult.Success;
    }

    protected abstract string SelectStream(T @event);
    
    protected abstract void BuildEvent(IHerculesEventBuilder builder, T @event);
}