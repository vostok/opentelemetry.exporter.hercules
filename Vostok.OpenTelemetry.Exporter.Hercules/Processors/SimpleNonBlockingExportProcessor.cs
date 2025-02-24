using OpenTelemetry;
using Vostok.Hercules.Client.Abstractions;

namespace Vostok.OpenTelemetry.Exporter.Hercules.Processors;

/// <summary>
/// Export single item without thread blocking.
/// </summary>
/// <remarks><see cref="SimpleExportProcessor{T}" /> blocks current thread on every export call.
/// We can avoid this because <see cref="IHerculesSink" /> is thread safe and nonblocking.</remarks>
internal class SimpleNonBlockingExportProcessor<T>(BaseExporter<T> exporter) : BaseExportProcessor<T>(exporter)
    where T : class
{
    protected override void OnExport(T data) =>
        exporter.Export(new Batch<T>(data));
}