using System;
using System.Linq.Expressions;
using System.Reflection;
using OpenTelemetry;
using Vostok.Hercules.Client.Abstractions;

namespace Vostok.OpenTelemetry.Exporter.Hercules.Processors;

/// <summary>
/// Export single item without thread blocking.
/// </summary>
/// <remarks><see cref="SimpleExportProcessor{T}" /> blocks current thread on every export call.
/// We can avoid it because <see cref="IHerculesSink" /> is thread safe and nonblocking.</remarks>
internal class SimpleNonBlockingExportProcessor<T> : BaseExportProcessor<T>
    where T : class
{
    private readonly Func<T, Batch<T>> createBatch;

    public SimpleNonBlockingExportProcessor(BaseExporter<T> exporter)
        : base(exporter)
    {
        createBatch = CompileBatchConstructor();
    }

    protected override void OnExport(T data)
    {
        var batch = createBatch(data);
        exporter.Export(batch);
    }

    // note (ponomaryovigor, 02.11.2024): We can't use Batch<T>(T item) for now because it is internal.
    // Migrate if it will be public
    private static Func<T, Batch<T>> CompileBatchConstructor()
    {
        var constructorInfo = typeof(Batch<T>).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, new[] {typeof(T)});
        if (constructorInfo is null)
            throw new TypeLoadException("Can't get information about Batch<T> internal constructor with one argument.");

        var parameter = Expression.Parameter(typeof(T));
        var constructorExpression = Expression.New(constructorInfo, parameter);

        var lambdaExpression = Expression.Lambda<Func<T, Batch<T>>>(constructorExpression, parameter);

        return lambdaExpression.Compile();
    }
}