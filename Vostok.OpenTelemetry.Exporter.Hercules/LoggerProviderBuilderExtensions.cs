using System;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenTelemetry.Logs;
using Vostok.Hercules.Client.Abstractions;
using Vostok.OpenTelemetry.Exporter.Hercules.Processors;

namespace Vostok.OpenTelemetry.Exporter.Hercules;

[PublicAPI]
public static class LoggerProviderBuilderExtensions
{
    public static LoggerProviderBuilder AddHerculesExporter(this LoggerProviderBuilder builder) =>
        builder.AddHerculesExporter(null, null);

    public static LoggerProviderBuilder AddHerculesExporter(this LoggerProviderBuilder builder, Action<HerculesLogExporterOptions>? configure) =>
        builder.AddHerculesExporter(null, configure);

    public static LoggerProviderBuilder AddHerculesExporter(this LoggerProviderBuilder builder, string? name, Action<HerculesLogExporterOptions>? configure)
    {
        ArgumentNullException.ThrowIfNull(builder);

        name ??= Options.DefaultName;

        builder.ConfigureServices(services =>
        {
            if (configure != null)
                services.Configure(name, configure);
        });

        builder.AddProcessor(serviceProvider =>
        {
            var optionsProvider = () => serviceProvider.GetRequiredService<IOptionsMonitor<HerculesLogExporterOptions>>().Get(name);

            var sink = serviceProvider.GetRequiredService<IHerculesSink>();

            return new SimpleNonBlockingExportProcessor<LogRecord>(new HerculesLogExporter(sink, optionsProvider));
        });

        return builder;
    }
}