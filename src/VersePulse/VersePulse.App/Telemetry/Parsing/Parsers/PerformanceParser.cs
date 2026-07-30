using System;
using System.Globalization;

namespace VersePulse.App.Telemetry.Parsing.Parsers;

public sealed class PerformanceParser : ILogParser
{
    private const string WholeFrameMarker =
        "CPU (WholeFrame)";

    private const string MainThreadMarker =
        "CPU (MainThread)";

    private const string WorkingSetMarker =
        "Memory (WorkingSet)";

    private const string CommittedMemoryMarker =
        "Memory (Committed)";

    public void Parse(
        string line,
        TelemetryData telemetry)
    {
        ParseFps(
            line,
            WholeFrameMarker,
            value => telemetry.ClientFps = value);

        ParseFps(
            line,
            MainThreadMarker,
            value => telemetry.MainThreadFps = value);

        ParseMemory(
            line,
            WorkingSetMarker,
            value => telemetry.RamUsageMb = value);

        ParseMemory(
            line,
            CommittedMemoryMarker,
            value => telemetry.CommittedMemoryMb = value);
    }

    private static void ParseFps(
        string line,
        string marker,
        Action<double> assignValue)
    {
        int markerIndex = line.IndexOf(
            marker,
            StringComparison.OrdinalIgnoreCase);

        if (markerIndex < 0)
        {
            return;
        }

        int colonIndex = line.IndexOf(
            ':',
            markerIndex);

        if (colonIndex < 0)
        {
            return;
        }

        string valueSection =
            line[(colonIndex + 1)..].Trim();

        int fpsIndex = valueSection.IndexOf(
            "FPS",
            StringComparison.OrdinalIgnoreCase);

        if (fpsIndex < 0)
        {
            return;
        }

        string fpsText =
            valueSection[..fpsIndex].Trim();

        if (double.TryParse(
                fpsText,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out double fps))
        {
            assignValue(fps);
        }
    }

    private static void ParseMemory(
        string line,
        string marker,
        Action<double> assignValue)
    {
        int markerIndex = line.IndexOf(
            marker,
            StringComparison.OrdinalIgnoreCase);

        if (markerIndex < 0)
        {
            return;
        }

        int maxIndex = line.IndexOf(
            "Max",
            markerIndex,
            StringComparison.OrdinalIgnoreCase);

        if (maxIndex < 0)
        {
            return;
        }

        string valueSection =
            line[(maxIndex + 3)..].Trim();

        int spaceIndex =
            valueSection.IndexOf(' ');

        string memoryText =
            spaceIndex >= 0
                ? valueSection[..spaceIndex]
                : valueSection;

        if (double.TryParse(
                memoryText,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out double memoryMb))
        {
            assignValue(memoryMb);
        }
    }
}