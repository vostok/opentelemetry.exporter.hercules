using System;
using System.Collections.Generic;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using Vostok.Hercules.Client.Abstractions;
using Vostok.OpenTelemetry.Exporter.Hercules.Builders;
using Vostok.OpenTelemetry.Exporter.Hercules.Helpers;

namespace Vostok.OpenTelemetry.Exporter.Hercules;

public class HerculesMetricExporter : BaseExporter<Metric>
{
    private const string CounterAggregationType = "counter";
    private const string HistogramAggregationType = "histogram";

    private readonly IHerculesSink sink;
    private readonly Func<HerculesMetricExporterOptions> optionsProvider;

    public HerculesMetricExporter(IHerculesSink sink, Func<HerculesMetricExporterOptions> optionsProvider)
    {
        this.sink = sink;
        this.optionsProvider = optionsProvider;
    }

    public override ExportResult Export(in Batch<Metric> batch)
    {
        var resource = ParentProvider.GetResource();
        var options = optionsProvider();

        foreach (var metric in batch)
            ExportMetric(metric, resource, options);

        return ExportResult.Success;
    }

    private void ExportMetric(Metric metric, Resource resource, HerculesMetricExporterOptions options)
    {
        // note (ponomaryovigor, 02.11.2024): ExponentialHistogram not supported
        switch (metric.MetricType)
        {
            case MetricType.LongSum:
            case MetricType.LongSumNonMonotonic:
                foreach (ref readonly var metricPoint in metric.GetMetricPoints())
                    ExportCounter(metric, metricPoint, metricPoint.GetSumLong(), resource, options);
                break;
            case MetricType.DoubleSum:
            case MetricType.DoubleSumNonMonotonic:
                foreach (ref readonly var metricPoint in metric.GetMetricPoints())
                    ExportCounter(metric, metricPoint, metricPoint.GetSumDouble(), resource, options);
                break;
            case MetricType.LongGauge:
                foreach (ref readonly var metricPoint in metric.GetMetricPoints())
                    ExportGauge(metric, metricPoint, metricPoint.GetGaugeLastValueLong(), resource, options);
                break;
            case MetricType.DoubleGauge:
                foreach (ref readonly var metricPoint in metric.GetMetricPoints())
                    ExportGauge(metric, metricPoint, metricPoint.GetGaugeLastValueDouble(), resource, options);
                break;
            case MetricType.Histogram:
                foreach (ref readonly var metricPoint in metric.GetMetricPoints())
                    ExportHistogram(metric, metricPoint, resource, options);
                break;
        }
    }

    private void ExportCounter(
        Metric metric,
        MetricPoint metricPoint,
        double value,
        Resource resource,
        HerculesMetricExporterOptions options)
    {
        sink.Put(options.CountersStream,
            builder => builder.BuildMetric(resource, metric, metricPoint, value, CounterAggregationType, null));
    }

    private void ExportGauge(
        Metric metric,
        MetricPoint metricPoint,
        double value,
        Resource resource,
        HerculesMetricExporterOptions options)
    {
        sink.Put(options.FinalStream,
            builder => builder.BuildMetric(resource, metric, metricPoint, value, null, null));
    }

    private void ExportHistogram(Metric metric, MetricPoint metricPoint, Resource resource, HerculesMetricExporterOptions options)
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