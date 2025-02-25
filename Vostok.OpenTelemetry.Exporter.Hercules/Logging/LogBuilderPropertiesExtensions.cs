using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using Vostok.Commons.Formatting;
using Vostok.Hercules.Client.Abstractions.Events;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Abstractions.Values;

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

    public static void AddProperties(
        this IHerculesTagsBuilder builder,
        LogRecord logRecord,
        Resource resource,
        out string? messageTemplate)
    {
        messageTemplate = null;

        if (!string.IsNullOrEmpty(logRecord.CategoryName))
            builder.AddValue(WellKnownProperties.SourceContext, logRecord.CategoryName);
        if (logRecord.TraceId != default)
            builder.AddValue(LogEventTagNames.TraceId, logRecord.TraceId.ToHexString());
        if (logRecord.SpanId != default)
            builder.AddValue(LogEventTagNames.SpanId, logRecord.SpanId.ToHexString());

        if (logRecord.Attributes is not null)
        {
            foreach (var (key, value) in logRecord.Attributes)
            {
                if (messageTemplate is null && key is LogEventTagNames.OriginalFormat && value is string format)
                {
                    messageTemplate = format;
                    continue;
                }

                // if (value is OperationContextValue operationContext)
                    // value = operationContext.Select(context => context).ToString();

                AddProperty(builder, key, value);
            }
        }

        logRecord.ForEachScope((scope, providedBuilder) =>
            {
                foreach (var (key, value) in scope)
                    AddProperty(providedBuilder, key, value);
            },
            builder);

        foreach (var (key, value) in resource.Attributes)
            AddProperty(builder, key, value);
    }

    private static void AddProperty(IHerculesTagsBuilder builder, string key, object? value)
    {
        if (value is null || IsPositionalName(key) || FilteredProperties.Contains(key))
            return;

        if (builder.TryAddObject(key, value))
            return;

        var format = value is DateTime or DateTimeOffset ? "O" : null;
        builder.AddValue(key, ObjectValueFormatter.Format(value, format!));
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