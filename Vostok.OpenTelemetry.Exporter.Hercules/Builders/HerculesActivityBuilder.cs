using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using OpenTelemetry.Resources;
using Vostok.Commons.Formatting;
using Vostok.Commons.Time;
using Vostok.Hercules.Client.Abstractions.Events;
using Vostok.OpenTelemetry.Exporter.Hercules.Helpers;
using Vostok.Tracing.Abstractions;
using Vostok.Tracing.Diagnostics.Helpers;

namespace Vostok.OpenTelemetry.Exporter.Hercules.Builders;

internal static class HerculesActivityBuilder
{
    public static void BuildActivity(this IHerculesEventBuilder builder, Activity activity, Resource resource, IFormatProvider? formatProvider = null)
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
        AddAnnotation(ActivityTagNames.Scope, activity.Source.Name);
        AddAnnotation(ActivityTagNames.Name, activity.DisplayName);
        AddAnnotation(WellKnownAnnotations.Common.Kind, activity.Kind);

        if (activity.Status != ActivityStatusCode.Unset)
            AddAnnotation(WellKnownAnnotations.Common.Status, activity.Status);
        if (!string.IsNullOrEmpty(activity.StatusDescription))
            AddAnnotation(ActivityTagNames.Error, activity.StatusDescription);

        foreach (ref readonly var pair in activity.EnumerateTagObjects())
            AddAnnotation(pair.Key, pair.Value);

        foreach (var resourceAttribute in resource.Attributes)
            AddAnnotation(resourceAttribute.Key, resourceAttribute.Value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void AddAnnotation(string key, object? value)
        {
            if (!builder.TryAddObject(key, value))
                builder.AddValue(key, ObjectValueFormatter.Format(value!, formatProvider: formatProvider!));
        }
    }
}