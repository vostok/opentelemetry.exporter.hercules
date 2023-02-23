using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Vostok.Hercules.Client.Abstractions;

namespace Vostok.OpenTelemetry.Exporter.Hercules;

[PublicAPI]
public class HerculesActivityExporterSettings : HerculesExporterSettings<Activity>
{
    public HerculesActivityExporterSettings(IHerculesSink sink, string stream)
        : base(sink) =>
        Stream = stream ?? throw new ArgumentNullException(nameof(stream));

    public string Stream { get; }
}