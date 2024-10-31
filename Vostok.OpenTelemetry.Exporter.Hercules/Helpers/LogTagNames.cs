namespace Vostok.OpenTelemetry.Exporter.Hercules.Helpers;

internal static class LogEventTagNames
{
    public const string UtcOffset = "utcOffset";
    public const string Exception = "exception";
    public const string MessageTemplate = "messageTemplate";
    public const string Message = "message";
    public const string Properties = "properties";
    public const string Level = "level";
    public const string StackTrace = "stackTrace";
    
    public const string TraceId = "traceId";
    public const string SpanId = "spanId";
}

internal static class ExceptionTagNames
{
    public const string Message = "message";
    public const string Type = "type";
}