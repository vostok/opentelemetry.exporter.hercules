using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace Vostok.OpenTelemetry.Exporter.Hercules;

[PublicAPI]
[SuppressMessage("ApiDesign", "RS0016:Добавьте открытые типы и элементы в объявленный API")]
public class HerculesMetricExporterOptions
{
    /// <summary>
    /// Name of the Hercules stream for final metrics to use.
    /// </summary>
    public string FinalStream { get; set; } = "metrics_final";

    /// <summary>
    /// Name of the Hercules stream for counters to use.
    /// </summary>
    public string CountersStream { get; set; } = "metrics_counters";

    /// <summary>
    /// Name of the Hercules stream for histograms to use.
    /// </summary>
    public string HistogramsStream { get; set; } = "metrics_histograms";

    /// <summary>
    /// Enable or disable metrics exporting.
    /// </summary>
    /// <remarks>Default value is true.</remarks>
    public bool Enabled { get; set; } = true;
}