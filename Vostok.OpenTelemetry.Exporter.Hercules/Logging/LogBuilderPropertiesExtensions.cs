using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using Vostok.Commons.Formatting;
using Vostok.Hercules.Client.Abstractions.Events;
using Vostok.Logging.Abstractions;

namespace Vostok.OpenTelemetry.Exporter.Hercules.Logging;

internal static class LogBuilderPropertiesExtensions
{
    private static readonly HashSet<string> FilteredProperties = new(StringComparer.OrdinalIgnoreCase)
    {
        // Constant scope has empty string name. Skip those values
        string.Empty,

        // Skip original format attribute as we write message template to vostok-specific tag
        LogEventTagNames.OriginalFormat,

        // Skip tracing tags as we already writing it from LogRecord value
        LogEventTagNames.TraceId,
        LogEventTagNames.SpanId,

        // Some vostok properties
        WellKnownProperties.TraceContext
    };

    public static void AddProperties(this IHerculesTagsBuilder builder, LogRecord logRecord, Resource resource)
    {
        if (!string.IsNullOrEmpty(logRecord.CategoryName))
            builder.AddValue(WellKnownProperties.SourceContext, logRecord.CategoryName);
        if (logRecord.TraceId != default)
            builder.AddValue(LogEventTagNames.TraceId, logRecord.TraceId.ToHexString());
        if (logRecord.SpanId != default)
            builder.AddValue(LogEventTagNames.SpanId, logRecord.SpanId.ToHexString());

        if (logRecord.Attributes is not null)
        {
            foreach (var (key, value) in logRecord.Attributes)
                TryAddProperty(key, value);
        }

        logRecord.ForEachScope((scope, _) =>
            {
                foreach (var (key, value) in scope)
                    TryAddProperty(key, value);
            },
            string.Empty);

        foreach (var (key, value) in resource.Attributes)
            TryAddProperty(key, value);

        return;

        void TryAddProperty(string key, object? value)
        {
            if (value is null || IsPositionalName(key) || FilteredProperties.Contains(key))
                return;

            if (builder.TryAddObject(key, value))
                return;

            var format = value is DateTime or DateTimeOffset ? "O" : null;
            builder.AddValue(key, ObjectValueFormatter.Format(value, format!));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsPositionalName(string propertyName)
    {
        foreach (var character in propertyName)
        {
            if (character is < '0' or > '9')
                return false;
        }

        return true;
    }
}