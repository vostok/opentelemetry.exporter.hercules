using JetBrains.Annotations;

namespace Vostok.OpenTelemetry.Exporter.Hercules;

[PublicAPI]
public class HerculesMetricExporterOptions
{
    public string FinalStream { get; set; } = "metrics_final";
    
    public string CountersStream { get; set; } = "metrics_counters";
}