using System;

namespace VersePulse.App.Telemetry.Parsing.Parsers;

public sealed class EnvironmentParser : ILogParser
{
    private const string EnvironmentMarker = "@env_session:";
    private const string ShardMarker = "New Shard Id:";

    public void Parse(string line, TelemetryData telemetry)
    {
        ParseEnvironmentSession(line, telemetry);
        ParseShardId(line, telemetry);
    }

    private static void ParseEnvironmentSession(
        string line,
        TelemetryData telemetry)
    {
        string? environmentSession =
            LogValueExtractor.ExtractQuotedValue(
                line,
                EnvironmentMarker);

        if (string.IsNullOrWhiteSpace(environmentSession))
        {
            return;
        }

        telemetry.ServerName = environmentSession;
        telemetry.LastEvent = "Environment detected";
    }

    private static void ParseShardId(
        string line,
        TelemetryData telemetry)
    {
        int markerIndex = line.IndexOf(
            ShardMarker,
            StringComparison.OrdinalIgnoreCase);

        if (markerIndex < 0)
        {
            return;
        }

        string shardId = line[
            (markerIndex + ShardMarker.Length)..]
            .Trim();

        int oldShardIndex = shardId.IndexOf(
            ". Old Shard Id",
            StringComparison.OrdinalIgnoreCase);

        if (oldShardIndex >= 0)
        {
            shardId = shardId[..oldShardIndex].Trim();
        }

        if (string.IsNullOrWhiteSpace(shardId))
        {
            return;
        }

        telemetry.Region = ExtractRegionFromShard(shardId);
        telemetry.LastEvent = "Shard detected";
    }

    private static string ExtractRegionFromShard(
        string shardId)
    {
        string[] parts = shardId.Split(
            '_',
            StringSplitOptions.RemoveEmptyEntries);

        foreach (string part in parts)
        {
            if (part.StartsWith(
                    "use",
                    StringComparison.OrdinalIgnoreCase))
            {
                return "US East";
            }

            if (part.StartsWith(
                    "usw",
                    StringComparison.OrdinalIgnoreCase))
            {
                return "US West";
            }

            if (part.StartsWith(
                    "euw",
                    StringComparison.OrdinalIgnoreCase)
                || part.StartsWith(
                    "euc",
                    StringComparison.OrdinalIgnoreCase)
                || part.Equals(
                    "eu",
                    StringComparison.OrdinalIgnoreCase))
            {
                return "Europe";
            }

            if (part.StartsWith(
                    "aus",
                    StringComparison.OrdinalIgnoreCase))
            {
                return "Australia";
            }

            if (part.StartsWith(
                    "asia",
                    StringComparison.OrdinalIgnoreCase))
            {
                return "Asia";
            }
        }

        return "--";
    }
}