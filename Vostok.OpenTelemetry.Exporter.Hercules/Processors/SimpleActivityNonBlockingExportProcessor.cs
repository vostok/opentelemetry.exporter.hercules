using System.Diagnostics;
using OpenTelemetry;

namespace Vostok.OpenTelemetry.Exporter.Hercules.Processors;

internal sealed class SimpleActivityNonBlockingExportProcessor(BaseExporter<Activity> exporter)
    : SimpleNonBlockingExportProcessor<Activity>(exporter)
{
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