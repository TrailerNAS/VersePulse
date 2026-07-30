namespace VersePulse.App.Telemetry.Parsing;

public interface ILogParser
{
    void Parse(string line, TelemetryData telemetry);
}
