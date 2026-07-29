using System;
using System.Collections.Generic;
using System.IO;

namespace VersePulse.App.Telemetry;

public sealed class LogReader : ILogReader
{
    private long _lastPosition;
    private string? _currentLogPath;

    public IEnumerable<string> ReadNewLines(string logPath)
    {
        if (string.IsNullOrWhiteSpace(logPath))
            yield break;

        if (!File.Exists(logPath))
            yield break;

        if (!string.Equals(
                _currentLogPath,
                logPath,
                StringComparison.OrdinalIgnoreCase))
        {
            _currentLogPath = logPath;
            _lastPosition = 0;
        }

        using FileStream stream = new(
            logPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite | FileShare.Delete);

        if (_lastPosition > stream.Length)
            _lastPosition = 0;

        stream.Seek(_lastPosition, SeekOrigin.Begin);

        using StreamReader reader = new(stream);

        string? line;

        while ((line = reader.ReadLine()) != null)
        {
            yield return line;
        }

        _lastPosition = stream.Position;
    }

    public void Reset()
    {
        _lastPosition = 0;
        _currentLogPath = null;
    }
}