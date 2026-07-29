using System.Collections.Generic;

namespace VersePulse.App.Telemetry;

public interface ILogReader
{
    IEnumerable<string> ReadNewLines(string logPath);

    void Reset();
}