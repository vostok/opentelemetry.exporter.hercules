using System.Diagnostics;
using OpenTelemetry;

namespace Vostok.OpenTelemetry.Exporter.Hercules.Processors;

internal sealed class SimpleActivityNonBlockingExportProcessor : SimpleNonBlockingExportProcessor<Activity>
{
    public SimpleActivityNonBlockingExportProcessor(BaseExporter<Activity> exporter)
        : base(exporter)
    {
    }

    /// <inheritdoc />
    public override void OnEnd(Activity data)
    {
        if (!data.Recorded)
        {
            return;
        }

        OnExport(data);
    }
}