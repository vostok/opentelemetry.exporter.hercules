using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using Vostok.Commons.Formatting;
using Vostok.Commons.Time;
using Vostok.Hercules.Client.Abstractions.Events;
using Vostok.Logging.Abstractions;
using Vostok.OpenTelemetry.Exporter.Hercules.Helpers;
using MicrosoftLogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Vostok.OpenTelemetry.Exporter.Hercules.Builders;

internal static class HerculesLogRecordBuilder
{
    private const string OriginalFormat = "{OriginalFormat}";

    private static readonly HashSet<string> FilteredProperties = new(StringComparer.OrdinalIgnoreCase)
    {
        // Constant scope has empty string name. Skip those values
        string.Empty,

        // Skip original format attribute as we write message template to vostok-specific tag
        OriginalFormat,

        // Skip tracing tags as we already writing it from LogRecord value
        LogEventTagNames.TraceId,
        LogEventTagNames.SpanId,

        // Some vostok properties
        WellKnownProperties.TraceContext
    };

    public static void BuildLogRecord(this IHerculesEventBuilder builder, LogRecord logRecord, Resource resource)
    {
        builder
            .SetTimestamp(logRecord.Timestamp)
            .AddValue(LogEventTagNames.UtcOffset, PreciseDateTime.OffsetFromUtc.Ticks)
            .AddValue(LogEventTagNames.Level, logRecord.LogLevel.ToString());

        // note (ponomaryovigor, 31.10.2024): Body stores template only when "{OriginalFormat}" attribute is present
        var originalFormat = GetOriginalFormat(logRecord);
        if (originalFormat is not null)
            builder.AddValue(LogEventTagNames.MessageTemplate, originalFormat);
        if (logRecord.FormattedMessage is not null)
            builder.AddValue(LogEventTagNames.Message, logRecord.FormattedMessage);

        if (logRecord.Exception != null)
        {
            builder.AddContainer(
                LogEventTagNames.Exception,
                tagsBuilder => tagsBuilder.AddExceptionData(logRecord.Exception));

            if (logRecord.Exception.StackTrace != null)
                builder.AddValue(LogEventTagNames.StackTrace, logRecord.Exception.ToString());
        }

        builder.AddContainer(
            LogEventTagNames.Properties,
            tagsBuilder => tagsBuilder.AddProperties(logRecord, resource));
    }

    // note (ponomaryovigor, 31.10.2024): Copied from Vostok.Logging.Hercules
    private static void AddExceptionData(this IHerculesTagsBuilder builder, Exception exception)
    {
        builder
            .AddValue(ExceptionTagNames.Message, exception.Message)
            .AddValue(ExceptionTagNames.Type, ExceptionsNormalizer.Normalize(exception.GetType().FullName));

        var stackFrames = new StackTrace(exception, true).GetFrames();
        builder.AddVectorOfContainers(
            ExceptionTagNames.StackFrames,
            stackFrames,
            (tagsBuilder, frame) => tagsBuilder.AddStackFrameData(frame));

        var innerExceptions = new List<Exception>();

        if (exception is AggregateException aggregateException)
            innerExceptions.AddRange(aggregateException.InnerExceptions);
        else if (exception.InnerException != null)
            innerExceptions.Add(exception.InnerException);

        if (innerExceptions.Count > 0)
        {
            builder.AddVectorOfContainers(
                ExceptionTagNames.InnerExceptions,
                innerExceptions,
                (tagsBuilder, e) => tagsBuilder.AddExceptionData(e));
        }
    }

    private static void AddStackFrameData(this IHerculesTagsBuilder builder, StackFrame frame)
    {
        var method = frame.GetMethod();
        if (method != null)
        {
            builder.AddValue(StackFrameTagNames.Function, ExceptionsNormalizer.Normalize(method.Name));
            if (method.DeclaringType != null)
                builder.AddValue(StackFrameTagNames.Type, ExceptionsNormalizer.Normalize(method.DeclaringType.FullName));
        }

        var fileName = frame.GetFileName();
        if (!string.IsNullOrEmpty(fileName))
            builder.AddValue(StackFrameTagNames.File, fileName);

        var lineNumber = frame.GetFileLineNumber();
        if (lineNumber > 0)
            builder.AddValue(StackFrameTagNames.Line, lineNumber);

        var columnNumber = frame.GetFileColumnNumber();
        if (columnNumber > 0)
            builder.AddValue(StackFrameTagNames.Column, columnNumber);
    }

    private static void AddProperties(this IHerculesTagsBuilder builder, LogRecord logRecord, Resource resource)
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

    private static string? GetOriginalFormat(LogRecord logRecord)
    {
        if (logRecord.Attributes is null || logRecord.Attributes.Count == 0)
            return null;

        foreach (var (key, value) in logRecord.Attributes)
        {
            if (key is OriginalFormat && value is string format)
                return format;
        }

        return null;
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