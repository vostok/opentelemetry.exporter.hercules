using System.Diagnostics;
using FluentAssertions;
using NUnit.Framework;
using OpenTelemetry;
using Vostok.OpenTelemetry.Exporter.Hercules.Processors;

namespace Vostok.OpenTelemetry.Exporter.Hercules.Tests.Processors;

public class SimpleNonBlockingExportingProcessorTests
{
    [Test]
    public void SimpleNonBlockingExportProcessor_Should_Export_Single_Value()
    {
        var exporter = new TestExporter<string>();
        var processor = new SimpleNonBlockingExportProcessor<string>(exporter);

        const string value = "value";
        processor.OnEnd(value);

        exporter.ObservedBatch.Count.Should().Be(1);
        foreach (var batchValue in exporter.ObservedBatch)
            batchValue.Should().Be(value);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void SimpleActivityNonBlockingExportProcessor_Should_ExportActivity_If_Recorded(bool activityRecorded)
    {
        var exporter = new TestExporter<Activity>();
        var processor = new SimpleActivityNonBlockingExportProcessor(exporter);

        var activity = new Activity("TestOperation");
        if (activityRecorded)
            activity.ActivityTraceFlags = ActivityTraceFlags.Recorded;

        processor.OnEnd(activity);

        if (activityRecorded)
        {
            exporter.ObservedBatch.Count.Should().Be(1);
            foreach (var batchActivity in exporter.ObservedBatch)
                batchActivity.OperationName.Should().Be(activity.OperationName);
        }
        else
            exporter.ObservedBatch.Count.Should().Be(0);
    }

    private sealed class TestExporter<T> : BaseExporter<T>
        where T : class
    {
        public Batch<T> ObservedBatch { get; private set; }

        public override ExportResult Export(in Batch<T> batch)
        {
            ObservedBatch = batch;
            return ExportResult.Success;
        }
    }
}