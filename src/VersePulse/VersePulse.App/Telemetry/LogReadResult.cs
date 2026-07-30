using System.Collections.Generic;

namespace VersePulse.App.Telemetry;

public sealed class LogReadResult
{
    public static LogReadResult Empty { get; } =
        new([], LogReadStatus.NoChange);

    public LogReadResult(
        IReadOnlyList<string> lines,
        LogReadStatus status)
    {
        Lines = lines;
        Status = status;
    }

    public IReadOnlyList<string> Lines { get; }

    public LogReadStatus Status { get; }
}
