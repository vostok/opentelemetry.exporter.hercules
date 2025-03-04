using System;
using System.Collections.Generic;
using Vostok.Logging.Abstractions;

namespace Vostok.OpenTelemetry.Exporter.Hercules.Logging;

internal static class FilteredProperties
{
    private static readonly HashSet<string> Properties = new(StringComparer.OrdinalIgnoreCase)
    {
        // Skip original format attribute as we write message template to vostok-specific tag
        LogEventTagNames.OriginalFormat,

        // Skip tracing tags as we already writing it from LogRecord value
        LogEventTagNames.TraceId,
        LogEventTagNames.SpanId,

        // Some vostok properties
        WellKnownProperties.TraceContext
    };

    public static bool Contains(string key) =>
        Properties.Contains(key);
}