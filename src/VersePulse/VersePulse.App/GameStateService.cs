using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace VersePulse.App
{
    public sealed class GameStateService
    {
        private long _lastLogPosition;
        private string? _currentLogPath;

        private readonly TelemetryData _telemetry = new();

        public TelemetryData GetTelemetry()
        {
            Process? starCitizenProcess = GetStarCitizenProcess();

            if (starCitizenProcess == null)
            {
                ResetLogTracking();

                if (IsLauncherRunning())
                {
                    _telemetry.GameState = GameState.LauncherOpen;
                    _telemetry.LastEvent = "RSI Launcher detected";
                }
                else
                {
                    _telemetry.GameState = GameState.GameClosed;
                    _telemetry.LastEvent = "Star Citizen is closed";
                }

                return _telemetry;
            }

            string? logPath = FindGameLogPath(starCitizenProcess);

            if (string.IsNullOrWhiteSpace(logPath) || !File.Exists(logPath))
            {
                _telemetry.GameState = GameState.Starting;
                _telemetry.LastEvent = "Waiting for Game.log";
                _telemetry.LogFilePath = "--";

                return _telemetry;
            }

            _telemetry.LogFilePath = logPath;

            ReadNewLogEntries(logPath);

            return _telemetry;
        }

        private static Process? GetStarCitizenProcess()
        {
            return Process
                .GetProcessesByName("StarCitizen")
                .FirstOrDefault();
        }

        private static bool IsLauncherRunning()
        {
            try
            {
                return Process.GetProcesses().Any(process =>
                {
                    try
                    {
                        string processName = process.ProcessName;

                        return processName.Contains(
                                   "RSI",
                                   StringComparison.OrdinalIgnoreCase)
                               && processName.Contains(
                                   "Launcher",
                                   StringComparison.OrdinalIgnoreCase);
                    }
                    catch
                    {
                        return false;
                    }
                    finally
                    {
                        process.Dispose();
                    }
                });
            }
            catch
            {
                return false;
            }
        }

        private static string? FindGameLogPath(Process starCitizenProcess)
        {
            try
            {
                string? executablePath =
                    starCitizenProcess.MainModule?.FileName;

                if (!string.IsNullOrWhiteSpace(executablePath))
                {
                    DirectoryInfo? bin64Folder =
                        Directory.GetParent(executablePath);

                    DirectoryInfo? gameFolder =
                        bin64Folder?.Parent;

                    if (gameFolder != null)
                    {
                        string detectedPath =
                            Path.Combine(gameFolder.FullName, "Game.log");

                        if (File.Exists(detectedPath))
                        {
                            return detectedPath;
                        }
                    }
                }
            }
            catch
            {
                // Access to MainModule can occasionally fail.
            }

            string defaultPath =
                @"C:\Program Files\Roberts Space Industries\StarCitizen\LIVE\Game.log";

            return File.Exists(defaultPath)
                ? defaultPath
                : null;
        }

        private void ReadNewLogEntries(string logPath)
        {
            try
            {
                if (!string.Equals(
                        _currentLogPath,
                        logPath,
                        StringComparison.OrdinalIgnoreCase))
                {
                    _currentLogPath = logPath;
                    _lastLogPosition = 0;

                    _telemetry.GameState = GameState.Starting;
                    _telemetry.SessionId = "--";
                    _telemetry.ServerName = "--";
                    _telemetry.Region = "--";
                    _telemetry.LinesParsed = 0;
                    _telemetry.LastEvent = "Game.log connected";
                }

                using FileStream stream = new(
                    logPath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite | FileShare.Delete);

                if (_lastLogPosition > stream.Length)
                {
                    _lastLogPosition = 0;

                    _telemetry.GameState = GameState.Starting;
                    _telemetry.SessionId = "--";
                    _telemetry.ServerName = "--";
                    _telemetry.Region = "--";
                    _telemetry.LinesParsed = 0;
                    _telemetry.LastEvent = "Game.log restarted";
                }

                stream.Seek(_lastLogPosition, SeekOrigin.Begin);

                using StreamReader reader = new(stream);

                string? line;

                while ((line = reader.ReadLine()) != null)
                {
                    _telemetry.LinesParsed++;

                    ProcessLogLine(line);
                }

                _lastLogPosition = stream.Position;
            }
            catch (IOException)
            {
                _telemetry.LastEvent = "Game.log is temporarily locked";
            }
            catch (UnauthorizedAccessException)
            {
                _telemetry.LastEvent = "Access to Game.log was denied";
            }
        }

        private void ProcessLogLine(string line)
        {
            ParseSessionId(line);
            ParseEnvironmentSession(line);
            ParseGameState(line);
        }

        private void ParseSessionId(string line)
        {
            const string marker = "@session:";

            string? sessionId = ExtractQuotedValue(line, marker);

            if (!string.IsNullOrWhiteSpace(sessionId))
            {
                _telemetry.SessionId = sessionId;
                _telemetry.LastEvent = "Session ID detected";
            }
        }

        private void ParseEnvironmentSession(string line)
        {
            const string marker = "@env_session:";

            string? environmentSession =
                ExtractQuotedValue(line, marker);

            if (string.IsNullOrWhiteSpace(environmentSession))
            {
                return;
            }

            _telemetry.ServerName = environmentSession;
            _telemetry.Region = ExtractRegion(environmentSession);
            _telemetry.LastEvent = "Environment detected";
        }

        private void ParseGameState(string line)
        {
            if (line.Contains(
                    "Join PU",
                    StringComparison.OrdinalIgnoreCase))
            {
                _telemetry.GameState = GameState.Loading;
                _telemetry.LastEvent = "Joining Persistent Universe";
                return;
            }

            if (line.Contains(
                    "Session Manager [Request Connect]",
                    StringComparison.OrdinalIgnoreCase)
                || line.Contains(
                    "Connect started",
                    StringComparison.OrdinalIgnoreCase)
                || line.Contains(
                    "Expect Incoming Connection",
                    StringComparison.OrdinalIgnoreCase))
            {
                _telemetry.GameState = GameState.Loading;
                _telemetry.LastEvent = "Connecting to game server";
                return;
            }

            if (line.Contains(
                    "taskname=\"InGame\"",
                    StringComparison.OrdinalIgnoreCase)
                || line.Contains(
                    "EGameContextState::eEGS_Running",
                    StringComparison.OrdinalIgnoreCase))
            {
                _telemetry.GameState = GameState.InServer;
                _telemetry.LastEvent = "Entered game server";
                return;
            }

            if (line.Contains(
                    "RequestFrontEnd",
                    StringComparison.OrdinalIgnoreCase)
                || line.Contains(
                    "Loading GameModeRecord='SC_Frontend'",
                    StringComparison.OrdinalIgnoreCase))
            {
                _telemetry.GameState = GameState.MainMenu;
                _telemetry.LastEvent = "Entered main menu";
            }
        }

        private static string? ExtractQuotedValue(
            string line,
            string marker)
        {
            int markerIndex = line.IndexOf(
                marker,
                StringComparison.OrdinalIgnoreCase);

            if (markerIndex < 0)
            {
                return null;
            }

            int firstQuote = line.IndexOf('\'', markerIndex);

            int secondQuote = firstQuote >= 0
                ? line.IndexOf('\'', firstQuote + 1)
                : -1;

            if (firstQuote < 0 || secondQuote <= firstQuote)
            {
                return null;
            }

            return line.Substring(
                firstQuote + 1,
                secondQuote - firstQuote - 1);
        }

        private static string ExtractRegion(
            string environmentSession)
        {
            string[] parts = environmentSession.Split(
                '-',
                StringSplitOptions.RemoveEmptyEntries);

            foreach (string part in parts)
            {
                if (part.StartsWith(
                        "use",
                        StringComparison.OrdinalIgnoreCase)
                    || part.StartsWith(
                        "usw",
                        StringComparison.OrdinalIgnoreCase)
                    || part.StartsWith(
                        "euw",
                        StringComparison.OrdinalIgnoreCase)
                    || part.StartsWith(
                        "euc",
                        StringComparison.OrdinalIgnoreCase)
                    || part.StartsWith(
                        "aus",
                        StringComparison.OrdinalIgnoreCase)
                    || part.StartsWith(
                        "asia",
                        StringComparison.OrdinalIgnoreCase))
                {
                    return part.ToUpperInvariant();
                }
            }

            return "--";
        }

        private void ResetLogTracking()
        {
            _lastLogPosition = 0;
            _currentLogPath = null;

            _telemetry.GameState = GameState.GameClosed;
            _telemetry.SessionId = "--";
            _telemetry.ServerName = "--";
            _telemetry.Region = "--";
            _telemetry.LogFilePath = "--";
            _telemetry.LinesParsed = 0;
        }
    }
}