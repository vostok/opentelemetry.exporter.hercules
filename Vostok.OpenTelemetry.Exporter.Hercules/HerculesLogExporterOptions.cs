using System;
using JetBrains.Annotations;

namespace Vostok.OpenTelemetry.Exporter.Hercules;

[PublicAPI]
public sealed class HerculesLogExporterOptions
{
    /// <summary>
    /// Name of the Hercules stream to use.
    /// </summary>
    public string Stream { get; set; } = "common_logs_prod";

    /// <summary>
    /// If specified, this <see cref="IFormatProvider"/> will be used when formatting annotation values to strings.
    /// </summary>
    public IFormatProvider? FormatProvider { get; set; }

    /// <summary>
    /// Enable or disable logs exporting.
    /// </summary>
    /// <remarks>Default value is true.</remarks>
    public bool Enabled { get; set; } = true;
}