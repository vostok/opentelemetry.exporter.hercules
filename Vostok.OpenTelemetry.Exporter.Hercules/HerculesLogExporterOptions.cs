using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace Vostok.OpenTelemetry.Exporter.Hercules;

[PublicAPI]
[SuppressMessage("ApiDesign", "RS0016:Добавьте открытые типы и элементы в объявленный API")]
public sealed class HerculesLogExporterOptions
{
    /// <summary>
    /// Name of the Hercules stream to use.
    /// </summary>
    public string Stream { get; set; } = "common_logs_prod";

    public bool Enabled { get; set; } = true;
}