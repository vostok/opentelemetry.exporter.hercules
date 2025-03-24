using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using Vostok.Commons.Formatting;
using Vostok.Hercules.Client.Abstractions.Events;

namespace Vostok.OpenTelemetry.Exporter.Hercules.Logging;

internal static class LogBuilderPropertiesExtensions
{
    public static void AddProperties(
        this IHerculesTagsBuilder builder,
        LogRecord logRecord,
        Resource resource,
        IFormatProvider? formatProvider,
        out string? messageTemplate)
    {
        if (!string.IsNullOrEmpty(logRecord.CategoryName))
            builder.AddValue(LogEventTagNames.Category, logRecord.CategoryName);
        if (logRecord.TraceId != default)
            builder.AddValue(LogEventTagNames.TraceId, logRecord.TraceId.ToHexString());
        if (logRecord.SpanId != default)
            builder.AddValue(LogEventTagNames.SpanId, logRecord.SpanId.ToHexString());

        ProcessAttributes(builder, logRecord, formatProvider, out messageTemplate);
        ProcessScopes(builder, logRecord, formatProvider);
        ProcessResource(builder, resource, formatProvider);
    }

    private static void ProcessAttributes(
        IHerculesTagsBuilder builder,
        LogRecord logRecord,
        IFormatProvider? formatProvider,
        out string? messageTemplate)
    {
        messageTemplate = null;

        if (logRecord.Attributes is null)
            return;

        foreach (var (key, value) in logRecord.Attributes)
        {
            if (messageTemplate is null && key is LogEventTagNames.OriginalFormat && value is string format)
            {
                messageTemplate = format;
                continue;
            }

            AddProperty(builder, key, value, formatProvider);
        }
    }

    private static void ProcessScopes(IHerculesTagsBuilder builder, LogRecord logRecord, IFormatProvider? formatProvider)
    {
        var scopeProcessingState = new ScopeProcessingState {Builder = builder, FormatProvider = formatProvider};
        logRecord.ForEachScope(Process, scopeProcessingState);

        if (scopeProcessingState.FormattedScopes is not null)
            builder.AddVector(LogEventTagNames.Scope, scopeProcessingState.FormattedScopes);

        return;

        static void Process(LogRecordScope scope, ScopeProcessingState state)
        {
            foreach (var (key, value) in scope)
            {
                // note (ponomaryovigor, 28.02.2025): OTel LogRecordScope can be formatted if has empty key or contains {OriginalFormat}
                if (key == string.Empty || key == LogEventTagNames.OriginalFormat)
                {
                    if (Convert.ToString(scope.Scope) is {} formattedScopes)
                    {
                        state.FormattedScopes ??= [];
                        state.FormattedScopes.Add(formattedScopes);
                    }

                    continue;
                }

                AddProperty(state.Builder, key, value, state.FormatProvider);
            }
        }
    }

    private static void ProcessResource(IHerculesTagsBuilder builder, Resource resource, IFormatProvider? formatProvider)
    {
        foreach (var (key, value) in resource.Attributes)
            AddProperty(builder, key, value, formatProvider);
    }

    private static void AddProperty(IHerculesTagsBuilder builder, string key, object? value, IFormatProvider? formatProvider)
    {
        if (value is null || IsPositionalName(key) || FilteredProperties.Contains(key))
            return;

        if (builder.TryAddObject(key, value))
            return;

        var format = value is DateTime or DateTimeOffset ? "O" : null;

        builder.AddValue(key, ObjectValueFormatter.Format(value, format, formatProvider));
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

    private sealed class ScopeProcessingState
    {
        public IHerculesTagsBuilder Builder = null!;
        public IFormatProvider? FormatProvider;
        public List<string>? FormattedScopes;
    }
}