using System;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenTelemetry.Metrics;
using Vostok.Hercules.Client.Abstractions;

namespace Vostok.OpenTelemetry.Exporter.Hercules;

[PublicAPI]
public static class MeterProviderBuilderExtensions
{
    public static MeterProviderBuilder AddHerculesExporter(this MeterProviderBuilder builder) =>
        builder.AddHerculesExporter(null, null, null);

    public static MeterProviderBuilder AddHerculesExporter(this MeterProviderBuilder builder, Action<HerculesMetricExporterOptions>? configureExporter) =>
        builder.AddHerculesExporter(null, configureExporter);
    
    public static MeterProviderBuilder AddHerculesExporter(this MeterProviderBuilder builder, Action<HerculesMetricExporterOptions>? configureExporter, Action<MetricReaderOptions>? configureReader) =>
        builder.AddHerculesExporter(null, configureExporter, configureReader);
    
    public static MeterProviderBuilder AddHerculesExporter(this MeterProviderBuilder builder, string? name, Action<HerculesMetricExporterOptions>? configureExporter) =>
        builder.AddHerculesExporter(name, configureExporter, null);
    
    public static MeterProviderBuilder AddHerculesExporter(this MeterProviderBuilder builder, string? name, Action<HerculesMetricExporterOptions>? configureExporter, Action<MetricReaderOptions>? configureReader)
    {
        ArgumentNullException.ThrowIfNull(builder);

        name ??= Options.DefaultName;

        builder.ConfigureServices(services =>
        {
            if (configureExporter != null)
                services.Configure(name, configureExporter);
        });

        builder.AddReader(serviceProvider =>
        {
            var exporterOptionsProvider = () => serviceProvider.GetRequiredService<IOptionsMonitor<HerculesMetricExporterOptions>>().Get(name);
            var readerOptions = serviceProvider.GetRequiredService<IOptionsMonitor<MetricReaderOptions>>().Get(name);
            configureReader?.Invoke(readerOptions);
            readerOptions.TemporalityPreference = MetricReaderTemporalityPreference.Delta;
                
            var sink = serviceProvider.GetRequiredService<IHerculesSink>();

            return BuildExporter(sink, exporterOptionsProvider, readerOptions);
        });

        return builder;
    }

    private static MetricReader BuildExporter(IHerculesSink sink, Func<HerculesMetricExporterOptions> exporterOptionsProvider, MetricReaderOptions readerOptions)
    {
        var exporter = new HerculesMetricExporter(sink, exporterOptionsProvider);
        
        var exportInterval = readerOptions.PeriodicExportingMetricReaderOptions.ExportIntervalMilliseconds 
                             ?? (int)TimeSpan.FromMinutes(1).TotalMilliseconds;

        var exportTimeout = readerOptions.PeriodicExportingMetricReaderOptions.ExportTimeoutMilliseconds 
                            ?? Timeout.Infinite;

        return new PeriodicExportingMetricReader(exporter, exportInterval, exportTimeout)
        {
            TemporalityPreference = readerOptions.TemporalityPreference,
        };
    }
}