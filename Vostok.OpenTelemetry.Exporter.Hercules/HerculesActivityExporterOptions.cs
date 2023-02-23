using System.Diagnostics;
using JetBrains.Annotations;

namespace Vostok.OpenTelemetry.Exporter.Hercules;

[PublicAPI]
public class HerculesActivityExporterOptions : HerculesExporterOptions<Activity>
{
    public string Stream { get; set; } = "traces";
}