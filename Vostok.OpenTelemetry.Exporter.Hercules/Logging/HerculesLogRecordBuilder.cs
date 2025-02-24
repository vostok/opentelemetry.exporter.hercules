using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using Vostok.Hercules.Client.Abstractions.Events;

namespace Vostok.OpenTelemetry.Exporter.Hercules.Logging;

internal static class HerculesLogRecordBuilder
{
    public static void BuildLogRecord(this IHerculesEventBuilder builder, LogRecord logRecord, Resource resource)
    {
        builder.SetTimestamp(logRecord.Timestamp)
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

    private static string? GetOriginalFormat(LogRecord logRecord)
    {
        if (logRecord.Attributes is null || logRecord.Attributes.Count == 0)
            return null;

        foreach (var (key, value) in logRecord.Attributes)
        {
            if (key is LogEventTagNames.OriginalFormat && value is string format)
                return format;
        }

        return null;
    }
}