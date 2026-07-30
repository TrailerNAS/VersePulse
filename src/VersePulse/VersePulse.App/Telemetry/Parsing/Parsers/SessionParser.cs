namespace VersePulse.App.Telemetry.Parsing.Parsers;

public sealed class SessionParser : ILogParser
{
    private const string Marker = "@session:";

    public void Parse(string line, TelemetryData telemetry)
    {
        string? sessionId =
            LogValueExtractor.ExtractQuotedValue(line, Marker);

        if (string.IsNullOrWhiteSpace(sessionId))
        {
            return;
        }

        telemetry.SessionId = sessionId;
        telemetry.LastEvent = "Session ID detected";
    }
}
