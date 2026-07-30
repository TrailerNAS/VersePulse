namespace VersePulse.App.Telemetry;

public interface ILogReader
{
    LogReadResult ReadNewLines(string logPath);

    void Reset();
}
