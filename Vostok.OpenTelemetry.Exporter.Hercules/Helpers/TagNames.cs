namespace Vostok.OpenTelemetry.Exporter.Hercules.Helpers;

internal static class TagNames
{
    public const string TraceId = "traceId";

    public const string SpanId = "spanId";

    public const string ParentSpanId = "parentSpanId";

    public const string BeginTimestampUtc = "beginTimestampUtc";

    public const string EndTimestampUtc = "endTimestampUtc";

    public const string Annotations = "annotations";
}