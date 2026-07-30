using System;

namespace VersePulse.App.Telemetry.Parsing.Parsers;

public sealed class EnvironmentParser : ILogParser
{
    private const string Marker = "@env_session:";

    public void Parse(string line, TelemetryData telemetry)
    {
        string? environmentSession =
            LogValueExtractor.ExtractQuotedValue(line, Marker);

        if (string.IsNullOrWhiteSpace(environmentSession))
        {
            return;
        }

        telemetry.ServerName = environmentSession;
        telemetry.Region = ExtractRegion(environmentSession);
        telemetry.LastEvent = "Environment detected";
    }

    private static string ExtractRegion(string environmentSession)
    {
        string[] parts = environmentSession.Split(
            '-',
            StringSplitOptions.RemoveEmptyEntries);

        foreach (string part in parts)
        {
            if (part.StartsWith("use", StringComparison.OrdinalIgnoreCase)
                || part.StartsWith("usw", StringComparison.OrdinalIgnoreCase)
                || part.StartsWith("euw", StringComparison.OrdinalIgnoreCase)
                || part.StartsWith("euc", StringComparison.OrdinalIgnoreCase)
                || part.StartsWith("aus", StringComparison.OrdinalIgnoreCase)
                || part.StartsWith("asia", StringComparison.OrdinalIgnoreCase))
            {
                return part.ToUpperInvariant();
            }
        }

        return "--";
    }
}
