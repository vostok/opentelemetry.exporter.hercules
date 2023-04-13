using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using OpenTelemetry.Resources;
using Vostok.Commons.Formatting;
using Vostok.Commons.Time;
using Vostok.Hercules.Client.Abstractions.Events;
using Vostok.OpenTelemetry.Exporter.Hercules.Helpers;
using Vostok.Tracing.Abstractions;
using Vostok.Tracing.Diagnostics.Helpers;

namespace Vostok.OpenTelemetry.Exporter.Hercules.Builders;

/// <summary>
/// Converts <see cref="Activity"/>s to <see cref="HerculesEvent"/>.
/// </summary>
[SuppressMessage("ApiDesign", "RS0026:Do not add multiple public overloads with optional parameters")]
internal static class HerculesActivityBuilder
{
    public static HerculesEvent Build(Activity activity, Resource resource, IFormatProvider? formatProvider = null)
    {
        var builder = new HerculesEventBuilder();
        Build(activity, resource, builder, formatProvider);
        return builder.BuildEvent();
    }

    public static void Build(Activity activity, Resource resource, IHerculesEventBuilder builder, IFormatProvider? formatProvider = null)
    {
        var endTimeUtc = activity.StartTimeUtc + activity.Duration;
        builder.SetTimestamp(endTimeUtc);

        builder
            .AddValue(ActivityTagNames.TraceId, activity.TraceId.ToGuid())
            .AddValue(ActivityTagNames.SpanId, activity.SpanId.ToGuid())
            .AddValue(ActivityTagNames.BeginTimestampUtc, EpochHelper.ToUnixTimeUtcTicks(activity.StartTimeUtc))
            .AddValue(ActivityTagNames.BeginTimestampUtcOffset, PreciseDateTime.OffsetFromUtc.Ticks)
            .AddValue(ActivityTagNames.EndTimestampUtc, EpochHelper.ToUnixTimeUtcTicks(endTimeUtc))
            .AddValue(ActivityTagNames.EndTimestampUtcOffset, PreciseDateTime.OffsetFromUtc.Ticks);

        if (activity.ParentSpanId != default)
            builder.AddValue(ActivityTagNames.ParentSpanId, activity.ParentSpanId.ToGuid());

        builder.AddContainer(ActivityTagNames.Annotations, tagBuilder => BuildAnnotationsContainer(tagBuilder, activity, resource, formatProvider));

        // todo (kungurtsev, 27.02.2023): send Activity.Events
        // todo (kungurtsev, 27.02.2023): send Activity.Links
    }

    private static void BuildAnnotationsContainer(IHerculesTagsBuilder builder, Activity activity, Resource resource, IFormatProvider? formatProvider)
    {
        AddAnnotation(builder, WellKnownAnnotations.Common.Component, activity.Source.Name, formatProvider);
        AddAnnotation(builder, WellKnownAnnotations.Common.Operation, activity.OperationName, formatProvider);
        if (!ReferenceEquals(activity.DisplayName, activity.OperationName))
            AddAnnotation(builder, "name", activity.DisplayName, formatProvider);
        AddAnnotation(builder, WellKnownAnnotations.Common.Kind, activity.Kind, formatProvider);

        if (activity.Status != ActivityStatusCode.Unset)
            AddAnnotation(builder, WellKnownAnnotations.Common.Status, activity.Status, formatProvider);
        if (!string.IsNullOrEmpty(activity.StatusDescription))
            AddAnnotation(builder, "status.description", activity.StatusDescription, formatProvider);

        foreach (ref readonly var pair in activity.EnumerateTagObjects())
            AddAnnotation(builder, pair.Key, pair.Value, formatProvider);

        foreach (var resourceAttribute in resource.Attributes)
            AddAnnotation(builder, resourceAttribute.Key, resourceAttribute.Value, formatProvider);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AddAnnotation(IHerculesTagsBuilder builder, string key, object? value, IFormatProvider? formatProvider)
    {
        if (!builder.TryAddObject(key, value))
            builder.AddValue(key, ObjectValueFormatter.Format(value!, formatProvider: formatProvider!));
    }
}