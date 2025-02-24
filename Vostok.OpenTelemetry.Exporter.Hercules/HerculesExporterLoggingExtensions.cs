using System;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenTelemetry.Logs;
using Vostok.Hercules.Client.Abstractions;
using Vostok.OpenTelemetry.Exporter.Hercules.Processors;

namespace Vostok.OpenTelemetry.Exporter.Hercules;

[PublicAPI]
public static class HerculesExporterLoggingExtensions
{
    public static LoggerProviderBuilder AddHerculesExporter(this LoggerProviderBuilder builder) =>
        builder.AddHerculesExporter(null, null);

    public static LoggerProviderBuilder AddHerculesExporter(
        this LoggerProviderBuilder builder,
        Action<HerculesLogExporterOptions> configure) =>
        builder.AddHerculesExporter(null, configure);

    public static LoggerProviderBuilder AddHerculesExporter(
        this LoggerProviderBuilder builder,
        string? name,
        Action<HerculesLogExporterOptions>? configure)
    {
        ArgumentNullException.ThrowIfNull(builder);

        name ??= Options.DefaultName;

        if (configure is not null)
            builder.ConfigureServices(services => services.Configure(name, configure));

        builder.AddProcessor(serviceProvider =>
        {
            var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<HerculesLogExporterOptions>>();
            var sink = serviceProvider.GetRequiredService<IHerculesSink>();

            return new SimpleNonBlockingExportProcessor<LogRecord>(new HerculesLogExporter(sink, () => optionsMonitor.Get(name)));
        });

        return builder;
    }

    public static OpenTelemetryLoggerOptions AddHerculesExporter(this OpenTelemetryLoggerOptions options) =>
        options.AddHerculesExporter(null, null);

    public static OpenTelemetryLoggerOptions AddHerculesExporter(
        this OpenTelemetryLoggerOptions options,
        Action<HerculesLogExporterOptions> configure) =>
        options.AddHerculesExporter(null, configure);

    public static OpenTelemetryLoggerOptions AddHerculesExporter(
        this OpenTelemetryLoggerOptions options,
        string? name,
        Action<HerculesLogExporterOptions>? configure)
    {
        name ??= Options.DefaultName;

        options.AddProcessor(serviceProvider =>
        {
            var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<HerculesLogExporterOptions>>();
            var exporterOptions = optionsMonitor.Get(name);
            configure?.Invoke(exporterOptions);

            optionsMonitor.OnChange((newOptions, newOptionsName) =>
            {
                if (name != newOptionsName)
                    return;

                configure?.Invoke(newOptions);
                exporterOptions = newOptions;
            });

            var sink = serviceProvider.GetRequiredService<IHerculesSink>();

            return new SimpleNonBlockingExportProcessor<LogRecord>(new HerculesLogExporter(sink, () => exporterOptions));
        });

        return options;
    }
}