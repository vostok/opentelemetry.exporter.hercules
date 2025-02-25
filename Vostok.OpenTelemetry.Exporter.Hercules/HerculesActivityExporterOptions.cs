using System;
using JetBrains.Annotations;

namespace Vostok.OpenTelemetry.Exporter.Hercules;

[PublicAPI]
public class HerculesActivityExporterOptions
{
    /// <summary>
    /// Name of the Hercules stream to use.
    /// </summary>
    public string Stream { get; set; } = "traces_prod";

    /// <summary>
    /// If specified, this <see cref="IFormatProvider"/> will be used when formatting annotation values to strings.
    /// </summary>
    public IFormatProvider? FormatProvider { get; set; }
}