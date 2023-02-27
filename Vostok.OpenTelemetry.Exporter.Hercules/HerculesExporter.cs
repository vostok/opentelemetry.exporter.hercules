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

    protected HerculesExporter(IHerculesSink sink) =>
        this.sink = sink;

    public override ExportResult Export(in Batch<T> batch)
    {
        foreach (var @event in batch)
            sink.Put(SelectStream(@event), b => BuildEvent(b, @event));

        return ExportResult.Success;
    }

    protected abstract string SelectStream(T @event);

    protected abstract void BuildEvent(IHerculesEventBuilder builder, T @event);
}