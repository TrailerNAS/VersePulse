using System;
using System.Collections.Generic;
using System.IO;

namespace VersePulse.App.Telemetry;

public sealed class LogReader : ILogReader
{
    private long _lastPosition;
    private string? _currentLogPath;

    public LogReadResult ReadNewLines(string logPath)
    {
        if (string.IsNullOrWhiteSpace(logPath)
            || !File.Exists(logPath))
        {
            return LogReadResult.Empty;
        }

        LogReadStatus status = LogReadStatus.NoChange;

        if (!string.Equals(
                _currentLogPath,
                logPath,
                StringComparison.OrdinalIgnoreCase))
        {
            _currentLogPath = logPath;
            _lastPosition = 0;
            status = LogReadStatus.Connected;
        }

        using FileStream stream = new(
            logPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite | FileShare.Delete);

        if (_lastPosition > stream.Length)
        {
            _lastPosition = 0;
            status = LogReadStatus.Restarted;
        }

        stream.Seek(_lastPosition, SeekOrigin.Begin);

        using StreamReader reader = new(stream);
        List<string> lines = [];

        while (reader.ReadLine() is { } line)
        {
            lines.Add(line);
        }

        _lastPosition = stream.Position;

        return new LogReadResult(lines, status);
    }

    public void Reset()
    {
        _lastPosition = 0;
        _currentLogPath = null;
    }
}
