using System;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenTelemetry.Trace;
using Vostok.Hercules.Client.Abstractions;
using Vostok.OpenTelemetry.Exporter.Hercules.Processors;

namespace Vostok.OpenTelemetry.Exporter.Hercules;

[PublicAPI]
public static class TracerProviderBuilderExtensions
{
    public static TracerProviderBuilder AddHerculesExporter(this TracerProviderBuilder builder) =>
        builder.AddHerculesExporter(null, null);

    public static TracerProviderBuilder AddHerculesExporter(this TracerProviderBuilder builder, Action<HerculesActivityExporterOptions>? configure) =>
        builder.AddHerculesExporter(null, configure);

    public static TracerProviderBuilder AddHerculesExporter(this TracerProviderBuilder builder, string? name, Action<HerculesActivityExporterOptions>? configure)
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
            var optionsProvider = () => serviceProvider.GetRequiredService<IOptionsMonitor<HerculesActivityExporterOptions>>().Get(name);

            var sink = serviceProvider.GetRequiredService<IHerculesSink>();

            return new SimpleActivityNonBlockingExportProcessor(new HerculesActivityExporter(sink, optionsProvider));
        });

        return builder;
    }
}