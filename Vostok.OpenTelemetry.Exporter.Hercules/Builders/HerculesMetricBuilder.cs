using System.Collections.Generic;
using System.Linq;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using Vostok.Commons.Helpers.Extensions;
using Vostok.Hercules.Client.Abstractions.Events;
using Vostok.OpenTelemetry.Exporter.Hercules.Helpers;

namespace Vostok.OpenTelemetry.Exporter.Hercules.Builders;

/// <summary>
/// Converts <see cref="Metric"/>s to <see cref="HerculesEvent"/>.
/// </summary>
internal static class HerculesMetricBuilder
{
    private const string NullValue = "null";

    public static void BuildMetric(
        this IHerculesEventBuilder builder,
        Resource resource,
        Metric metric,
        MetricPoint metricPoint,
        double value,
        string? aggregationType,
        IReadOnlyDictionary<string, string>? aggregationParameters)
    {
        var tagsCount = 1 + resource.Attributes.Count() +
            metric.MeterTags?.Count() ?? 0 +
            metricPoint.Tags.Count;
        var tags = new List<KeyValuePair<string, string>>(tagsCount);

        foreach (var resourceAttribute in resource.Attributes)
            tags.Add(new KeyValuePair<string, string>(resourceAttribute.Key, resourceAttribute.Value.ToString() ?? NullValue));

        if (metric.MeterTags is not null)
        {
            foreach (var meterTag in metric.MeterTags)
                tags.Add(new(meterTag.Key, meterTag.Value?.ToString() ?? NullValue));
        }

        tags.Add(new KeyValuePair<string, string>(MetricTagNames.Name, metric.Name));

        foreach (var tag in metricPoint.Tags)
            tags.Add(new(tag.Key, tag.Value?.ToString() ?? NullValue));

        var hashCode = CalculateHash(tags);

        builder.SetTimestamp(metricPoint.EndTime);
        builder.AddValue(MetricTagNames.Value, value);
        builder.AddValue(MetricTagNames.TagsHash, hashCode);
        builder.AddVectorOfContainers(
            MetricTagNames.Tags,
            tags,
            (b, tag) =>
            {
                b.AddValue(MetricTagNames.Key, tag.Key);
                b.AddValue(MetricTagNames.Value, tag.Value);
            });

        builder.AddValue(MetricTagNames.Description, metric.Description);

        if (!string.IsNullOrEmpty(metric.Unit))
            builder.AddValue(MetricTagNames.Unit, metric.Unit);

        if (!string.IsNullOrEmpty(aggregationType))
            builder.AddValue(MetricTagNames.AggregationType, aggregationType);

        if (aggregationParameters != null)
        {
            builder.AddContainer(
                MetricTagNames.AggregationParameters,
                b =>
                {
                    foreach (var pair in aggregationParameters)
                        b.AddValue(pair.Key, pair.Value);
                });
        }
    }

    // note (kungurtsev, 12.04.2023): copied from Vostok.Metrics.Models
    private static int CalculateHash(List<KeyValuePair<string, string>> tags)
    {
        return tags.Aggregate(tags.Count, (hash, tag) => (hash * 397) ^ CalculateHash(tag));
    }

    private static int CalculateHash(KeyValuePair<string, string> tag)
    {
        unchecked
        {
            return (tag.Key.GetStableHashCode() * 397) ^ tag.Value.GetStableHashCode();
        }
    }
}