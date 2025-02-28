using System;
using System.Collections.Generic;
#if NET8_0_OR_GREATER
using System.Collections.Frozen;
#else
using System.Linq;
#endif

using Vostok.Logging.Abstractions;

namespace Vostok.OpenTelemetry.Exporter.Hercules.Logging;

internal static class FilteredProperties
{
#if NET8_0_OR_GREATER
    private static readonly FrozenSet<string> Properties;
#else
    private static readonly HashSet<string> Properties;
#endif

    static FilteredProperties()
    {
        IEnumerable<string> properties =
        [
            // Skip original format attribute as we write message template to vostok-specific tag
            LogEventTagNames.OriginalFormat,

            // Skip tracing tags as we already writing it from LogRecord value
            LogEventTagNames.TraceId,
            LogEventTagNames.SpanId,

            // Some vostok properties
            WellKnownProperties.TraceContext
        ];

#if NET8_0_OR_GREATER
        Properties = properties.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
#else
        Properties = properties.ToHashSet(StringComparer.OrdinalIgnoreCase);
#endif
    }

    public static bool Contains(string key) =>
        Properties.Contains(key);
}