using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using OpenTelemetry.Resources;
using Vostok.Commons.Formatting;
using Vostok.Commons.Time;
using Vostok.Hercules.Client.Abstractions.Events;
using Vostok.Tracing.Abstractions;
using Vostok.Tracing.Diagnostics.Helpers;

namespace Vostok.OpenTelemetry.Exporter.Hercules.Tracing;

internal static class HerculesActivityBuilder
{
    public static void BuildActivity(
        this IHerculesEventBuilder builder,
        Activity activity,
        Resource resource,
        IFormatProvider? formatProvider = null)
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

        builder.AddContainer(ActivityTagNames.Annotations,
            tagBuilder => BuildAnnotationsContainer(tagBuilder, activity, resource, formatProvider));
    }

    private static void BuildAnnotationsContainer(
        IHerculesTagsBuilder builder,
        Activity activity,
        Resource resource,
        IFormatProvider? formatProvider)
    {
        AddAnnotation(builder, WellKnownAnnotations.Common.Component, activity.Source.Name, formatProvider);
        AddAnnotation(builder, WellKnownAnnotations.Common.Operation, activity.DisplayName, formatProvider);
        AddAnnotation(builder, WellKnownAnnotations.Common.Kind, activity.Kind, formatProvider);

        if (activity.Status != ActivityStatusCode.Unset)
            AddAnnotation(builder, WellKnownAnnotations.Common.Status, activity.Status.ToString(), formatProvider);
        if (!string.IsNullOrEmpty(activity.StatusDescription))
            AddAnnotation(builder, ActivityTagNames.Error, activity.StatusDescription, formatProvider);

        foreach (ref readonly var pair in activity.EnumerateTagObjects())
            AddAnnotation(builder, pair.Key, pair.Value, formatProvider);

        foreach (var resourceAttribute in resource.Attributes)
            AddAnnotation(builder, resourceAttribute.Key, resourceAttribute.Value, formatProvider);

        BuildEventsContainer(builder, activity, formatProvider);
        BuildLinksContainer(builder, activity, formatProvider);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AddAnnotation(IHerculesTagsBuilder builder, string key, object? value, IFormatProvider? formatProvider)
    {
        if (!builder.TryAddObject(key, value))
            builder.AddValue(key, ObjectValueFormatter.Format(value!, formatProvider: formatProvider!));
    }

    private static void BuildEventsContainer(
        IHerculesTagsBuilder builder,
        Activity activity,
        IFormatProvider? formatProvider)
    {
        if (!activity.Events.Any())
        {
            return;
        }

        builder.AddVectorOfContainers(ActivityTagNames.Events,
            activity.Events.ToList(),
            (eventsBuilder, activityEvent) =>
            {
                eventsBuilder.AddValue(ActivityTagNames.Name, activityEvent.Name);
                eventsBuilder.AddValue(ActivityTagNames.TimestampUtc, activityEvent.Timestamp.ToString("O"));
                eventsBuilder.AddContainer(ActivityTagNames.Annotations,
                    eventAnnotationsBuilder =>
                    {
                        foreach (ref readonly var pair in activityEvent.EnumerateTagObjects())
                        {
                            AddAnnotation(eventAnnotationsBuilder, pair.Key, pair.Value, formatProvider);
                        }
                    });
            });
    }

    private static void BuildLinksContainer(
        IHerculesTagsBuilder builder,
        Activity activity,
        IFormatProvider? formatProvider)
    {
        if (!activity.Links.Any())
        {
            return;
        }

        builder.AddVectorOfContainers(ActivityTagNames.Links,
            activity.Links.ToList(),
            (linksBuilder, activityLink) =>
            {
                linksBuilder.AddValue(ActivityTagNames.TraceId, activityLink.Context.TraceId.ToGuid());
                linksBuilder.AddValue(ActivityTagNames.SpanId, activityLink.Context.SpanId.ToGuid());
                linksBuilder.AddContainer(ActivityTagNames.Annotations,
                    linkAnnotationsBuilder =>
                    {
                        foreach (ref readonly var pair in activityLink.EnumerateTagObjects())
                        {
                            AddAnnotation(linkAnnotationsBuilder, pair.Key, pair.Value, formatProvider);
                        }
                    });
            });
    }
}