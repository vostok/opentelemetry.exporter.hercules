using JetBrains.Annotations;

namespace Vostok.OpenTelemetry.Exporter.Hercules;

[PublicAPI]
public class HerculesMetricExporterOptions
{
    public string CountersStream { get; set; } = "metrics_counters";
}