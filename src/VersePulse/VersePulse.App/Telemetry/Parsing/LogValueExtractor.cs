using System;

namespace VersePulse.App.Telemetry.Parsing;

internal static class LogValueExtractor
{
    public static string? ExtractQuotedValue(
        string line,
        string marker)
    {
        int markerIndex = line.IndexOf(
            marker,
            StringComparison.OrdinalIgnoreCase);

        if (markerIndex < 0)
        {
            return null;
        }

        int firstQuote = line.IndexOf('\'', markerIndex);
        int secondQuote = firstQuote >= 0
            ? line.IndexOf('\'', firstQuote + 1)
            : -1;

        if (firstQuote < 0 || secondQuote <= firstQuote)
        {
            return null;
        }

        return line.Substring(
            firstQuote + 1,
            secondQuote - firstQuote - 1);
    }
}
