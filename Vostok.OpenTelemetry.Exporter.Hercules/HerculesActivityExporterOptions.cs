using System;
using System.Diagnostics;
using JetBrains.Annotations;

namespace Vostok.OpenTelemetry.Exporter.Hercules;

[PublicAPI]
public class HerculesActivityExporterOptions : HerculesExporterOptions<Activity>
{
    /// <summary>
    /// Name of the Hercules stream to use.
    /// </summary>
    public string Stream { get; set; } = "traces";
    
    /// <summary>
    /// If specified, this <see cref="IFormatProvider"/> will be used when formatting annotation values to strings.
    /// </summary>
    public IFormatProvider? FormatProvider { get; set; }
}