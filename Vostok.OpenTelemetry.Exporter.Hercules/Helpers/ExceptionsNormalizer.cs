using System.Text.RegularExpressions;

namespace Vostok.OpenTelemetry.Exporter.Hercules.Helpers;

// note (ponomaryovigor, 31.10.2024): Copied from Vostok.Logging.Hercules
internal static class ExceptionsNormalizer
{
    private static readonly Regex GuidRegex = new(
        "[0-9A-Fa-f]{8}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{12}",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
    private static readonly Regex LambdaRegex = new(
        "^lambda_method\\d+$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    public static string? Normalize(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        value = GuidRegex.Replace(value, "~");
        value = LambdaRegex.Replace(value, "lambda_method~");

        return value;
    }
}