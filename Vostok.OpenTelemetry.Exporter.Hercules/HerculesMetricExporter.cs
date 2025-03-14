using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using Vostok.Hercules.Client.Abstractions;
using Vostok.OpenTelemetry.Exporter.Hercules.Builders;
using Vostok.OpenTelemetry.Exporter.Hercules.Helpers;

namespace Vostok.OpenTelemetry.Exporter.Hercules;

[PublicAPI]
public class HerculesMetricExporter : BaseExporter<Metric>
{
    private const string CounterAggregationType = "counter";
    private const string HistogramAggregationType = "histogram";

    private readonly IHerculesSink sink;
    private readonly Func<HerculesMetricExporterOptions> optionsProvider;
    private Resource resource = null!;

    public HerculesMetricExporter(IHerculesSink sink, Func<HerculesMetricExporterOptions> optionsProvider)
    {
        this.sink = sink;
        this.optionsProvider = optionsProvider;
    }

    public override ExportResult Export(in Batch<Metric> batch)
    {
        // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
        resource ??= ParentProvider.GetResource();

        var options = optionsProvider();

        foreach (var metric in batch)
            ExportMetric(metric, options);

        return ExportResult.Success;
    }

    private void ExportMetric(Metric metric, HerculesMetricExporterOptions options)
    {
        // note (ponomaryovigor, 02.11.2024): ExponentialHistogram not supported
        switch (metric.MetricType)
        {
            case MetricType.LongSum:
            case MetricType.LongSumNonMonotonic:
                foreach (ref readonly var metricPoint in metric.GetMetricPoints())
                    ExportCounter(metric, metricPoint, metricPoint.GetSumLong(), options);
                break;
            case MetricType.DoubleSum:
            case MetricType.DoubleSumNonMonotonic:
                foreach (ref readonly var metricPoint in metric.GetMetricPoints())
                    ExportCounter(metric, metricPoint, metricPoint.GetSumDouble(), options);
                break;
            case MetricType.LongGauge:
                foreach (ref readonly var metricPoint in metric.GetMetricPoints())
                    ExportGauge(metric, metricPoint, metricPoint.GetGaugeLastValueLong(), options);
                break;
            case MetricType.DoubleGauge:
                foreach (ref readonly var metricPoint in metric.GetMetricPoints())
                    ExportGauge(metric, metricPoint, metricPoint.GetGaugeLastValueDouble(), options);
                break;
            case MetricType.Histogram:
                foreach (ref readonly var metricPoint in metric.GetMetricPoints())
                    ExportHistogram(metric, metricPoint, options);
                break;
        }
    }

    private void ExportCounter(Metric metric, MetricPoint metricPoint, double value, HerculesMetricExporterOptions options)
    {
        sink.Put(options.CountersStream,
            builder => builder.BuildMetric(resource, metric, metricPoint, value, CounterAggregationType, null));
    }

    private void ExportGauge(Metric metric, MetricPoint metricPoint, double value, HerculesMetricExporterOptions options)
    {
        sink.Put(options.FinalStream,
            builder => builder.BuildMetric(resource, metric, metricPoint, value, null, null));
    }

    private void ExportHistogram(Metric metric, MetricPoint metricPoint, HerculesMetricExporterOptions options)
    {
        var aggregationParameters = new Dictionary<string, string>(2);

        var lowerBound = double.NegativeInfinity;
        foreach (var bucket in metricPoint.GetHistogramBuckets())
        {
            if (bucket.BucketCount != 0)
            {
                aggregationParameters[AggregationParametersNames.LowerBound] = DoubleSerializer.Serialize(lowerBound);
                aggregationParameters[AggregationParametersNames.UpperBound] = DoubleSerializer.Serialize(bucket.ExplicitBound);

                sink.Put(options.HistogramsStream,
                    builder => builder.BuildMetric(
                        resource,
                        metric,
                        metricPoint,
                        bucket.BucketCount,
                        HistogramAggregationType,
                        aggregationParameters));
            }

            lowerBound = bucket.ExplicitBound;
        }
    }
}