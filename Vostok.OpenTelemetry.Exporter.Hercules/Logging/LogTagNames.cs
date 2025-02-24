namespace Vostok.OpenTelemetry.Exporter.Hercules.Logging;

internal static class LogEventTagNames
{
    public const string OriginalFormat = "{OriginalFormat}";

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
    public const string InnerExceptions = "innerExceptions";
    public const string StackFrames = "stackFrames";
}

internal static class StackFrameTagNames
{
    public const string Function = "function";
    public const string Type = "type";
    public const string Line = "line";
    public const string Column = "column";
    public const string File = "file";
}