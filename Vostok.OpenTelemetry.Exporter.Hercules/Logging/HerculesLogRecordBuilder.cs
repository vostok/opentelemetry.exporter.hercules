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

        string? messageTemplate = null;
        builder.AddContainer(
            LogEventTagNames.Properties,
            tagsBuilder => tagsBuilder.AddProperties(logRecord, resource, out messageTemplate));

        messageTemplate ??= logRecord.Body;
        if (messageTemplate is not null)
            builder.AddValue(LogEventTagNames.MessageTemplate, messageTemplate);

        var formattedMessage = logRecord.FormattedMessage ?? logRecord.Body;
        if (formattedMessage is not null)
            builder.AddValue(LogEventTagNames.Message, formattedMessage);

        if (logRecord.Exception is not null)
        {
            builder.AddContainer(
                LogEventTagNames.Exception,
                tagsBuilder => tagsBuilder.AddExceptionData(logRecord.Exception));

            if (logRecord.Exception.StackTrace != null)
                builder.AddValue(LogEventTagNames.StackTrace, logRecord.Exception.ToString());
        }
    }
}