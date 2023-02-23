using System;
using JetBrains.Annotations;
using Vostok.Hercules.Client.Abstractions;

namespace Vostok.OpenTelemetry.Exporter.Hercules;

[PublicAPI]
public abstract class HerculesExporterSettings<T>
    where T : class
{
    protected HerculesExporterSettings(IHerculesSink sink) =>
        Sink = sink ?? throw new ArgumentNullException(nameof(sink));

    public IHerculesSink Sink { get; }
}