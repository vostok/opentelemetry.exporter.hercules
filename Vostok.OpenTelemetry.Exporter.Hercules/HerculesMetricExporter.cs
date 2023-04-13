using System;
using System.Collections.Generic;
using OpenTelemetry;
using OpenTelemetry.Metrics;
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
        foreach (var metric in batch)
        {
            switch (metric.MetricType)
            {
                case MetricType.LongSum:
                case MetricType.LongSumNonMonotonic:
                    foreach (ref readonly var metricPoint in metric.GetMetricPoints())
                        ExportCounter(metric, metricPoint, metricPoint.GetSumLong());
                    break;
                case MetricType.DoubleSum:
                case MetricType.DoubleSumNonMonotonic:
                    foreach (ref readonly var metricPoint in metric.GetMetricPoints())
                        ExportCounter(metric, metricPoint, metricPoint.GetSumDouble());
                    break;
                case MetricType.LongGauge:
                    foreach (ref readonly var metricPoint in metric.GetMetricPoints())
                        ExportGauge(metric, metricPoint, metricPoint.GetGaugeLastValueLong());
                    break;
                case MetricType.DoubleGauge:
                    foreach (ref readonly var metricPoint in metric.GetMetricPoints())
                        ExportGauge(metric, metricPoint, metricPoint.GetGaugeLastValueDouble());
                    break;
                case MetricType.Histogram:
                    foreach (ref readonly var metricPoint in metric.GetMetricPoints())
                        ExportHistogram(metric, metricPoint);
                    break;
            }
        }

        return ExportResult.Success;
    }

    private void ExportCounter(Metric metric, MetricPoint metricPoint, double value)
    {
        sink.Put(optionsProvider().CountersStream,
            builder =>
                HerculesMetricBuilder.Build(metric, metricPoint, value, CounterAggregationType, null, ParentProvider!.GetResource(), builder));
    }

    private void ExportGauge(Metric metric, MetricPoint metricPoint, double value)
    {
        sink.Put(optionsProvider().FinalStream,
            builder =>
                HerculesMetricBuilder.Build(metric, metricPoint, value, null, null, ParentProvider!.GetResource(), builder));
    }

    private void ExportHistogram(Metric metric, MetricPoint metricPoint)
    {
        var bound = double.NegativeInfinity;
        foreach (var bucket in metricPoint.GetHistogramBuckets())
        {
            if (bucket.BucketCount != 0)
            {
                var aggregationParameters = new Dictionary<string, string>
                {
                    [AggregationParametersNames.LowerBound] = DoubleSerializer.Serialize(bound),
                    [AggregationParametersNames.UpperBound] = DoubleSerializer.Serialize(bucket.ExplicitBound)
                };
                sink.Put(optionsProvider().HistogramsStream,
                    builder =>
                        HerculesMetricBuilder.Build(metric, metricPoint, bucket.BucketCount, HistogramAggregationType, aggregationParameters, ParentProvider!.GetResource(), builder));
            }

            bound = bucket.ExplicitBound;
        }
    }
}