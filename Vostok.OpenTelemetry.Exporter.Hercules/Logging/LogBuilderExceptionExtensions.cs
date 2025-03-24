using System;
using System.Collections.Generic;
using System.Diagnostics;
using Vostok.Hercules.Client.Abstractions.Events;

namespace Vostok.OpenTelemetry.Exporter.Hercules.Logging;

internal static class LogBuilderExceptionExtensions
{
    // note (ponomaryovigor, 31.10.2024): Copied from Vostok.Logging.Hercules
    public static void AddExceptionData(this IHerculesTagsBuilder builder, Exception exception)
    {
        builder.AddValue(ExceptionTagNames.Message, exception.Message)
               .AddValue(ExceptionTagNames.Type, ExceptionsNormalizer.Normalize(exception.GetType().FullName));

        var stackFrames = new StackTrace(exception, true).GetFrames();
        builder.AddVectorOfContainers(
            ExceptionTagNames.StackFrames,
            stackFrames,
            (tagsBuilder, frame) => tagsBuilder.AddStackFrameData(frame));

        var innerExceptions = new List<Exception>();

        if (exception is AggregateException aggregateException)
            innerExceptions.AddRange(aggregateException.InnerExceptions);
        else if (exception.InnerException != null)
            innerExceptions.Add(exception.InnerException);

        if (innerExceptions.Count > 0)
        {
            builder.AddVectorOfContainers(
                ExceptionTagNames.InnerExceptions,
                innerExceptions,
                (tagsBuilder, e) => tagsBuilder.AddExceptionData(e));
        }
    }

    private static void AddStackFrameData(this IHerculesTagsBuilder builder, StackFrame frame)
    {
        var method = frame.GetMethod();
        if (method != null)
        {
            builder.AddValue(StackFrameTagNames.Function, ExceptionsNormalizer.Normalize(method.Name));
            if (method.DeclaringType != null)
                builder.AddValue(StackFrameTagNames.Type, ExceptionsNormalizer.Normalize(method.DeclaringType.FullName));
        }

        var fileName = frame.GetFileName();
        if (!string.IsNullOrEmpty(fileName))
            builder.AddValue(StackFrameTagNames.File, fileName);

        var lineNumber = frame.GetFileLineNumber();
        if (lineNumber > 0)
            builder.AddValue(StackFrameTagNames.Line, lineNumber);

        var columnNumber = frame.GetFileColumnNumber();
        if (columnNumber > 0)
            builder.AddValue(StackFrameTagNames.Column, columnNumber);
    }
}