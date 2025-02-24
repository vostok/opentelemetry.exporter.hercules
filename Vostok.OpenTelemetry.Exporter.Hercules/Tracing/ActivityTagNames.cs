namespace Vostok.OpenTelemetry.Exporter.Hercules.Tracing;

internal static class ActivityTagNames
{
    public const string TraceId = "traceId";
    public const string SpanId = "spanId";
    public const string ParentSpanId = "parentSpanId";

    public const string Error = "error";

    public const string BeginTimestampUtc = "beginTimestampUtc";
    public const string BeginTimestampUtcOffset = "beginTimestampUtcOffset";

    public const string EndTimestampUtc = "endTimestampUtc";
    public const string EndTimestampUtcOffset = "endTimestampUtcOffset";

    public const string Annotations = "annotations";
}