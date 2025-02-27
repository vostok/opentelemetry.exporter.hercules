using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace Vostok.OpenTelemetry.Exporter.Hercules;

[PublicAPI]
[SuppressMessage("ApiDesign", "RS0016:Добавьте открытые типы и элементы в объявленный API")]
public class HerculesMetricExporterOptions
{
    public string FinalStream { get; set; } = "metrics_final";

    public string CountersStream { get; set; } = "metrics_counters";

    public string HistogramsStream { get; set; } = "metrics_histograms";

    public bool Enabled { get; set; } = true;
}