using System.Globalization;

namespace Vostok.OpenTelemetry.Exporter.Hercules.Metrics;

internal static class AggregationHelper
{
    public const string LowerBoundKey = "_lowerBound";
    public const string UpperBoundKey = "_upperBound";

    public static string SerializeDouble(double value) =>
        value.ToString(CultureInfo.InvariantCulture);
}