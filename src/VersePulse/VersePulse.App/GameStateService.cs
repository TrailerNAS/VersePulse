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
        private GameState _currentState = GameState.GameClosed;

        public GameState GetCurrentState()
        {
            Process? starCitizenProcess = GetStarCitizenProcess();

            if (starCitizenProcess == null)
            {
                ResetLogTracking();

                return IsLauncherRunning()
                    ? GameState.LauncherOpen
                    : GameState.GameClosed;
            }

            string? logPath = FindGameLogPath(starCitizenProcess);

            if (string.IsNullOrWhiteSpace(logPath) || !File.Exists(logPath))
            {
                return GameState.Starting;
            }

            ReadNewLogEntries(logPath);

            return _currentState;
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
                    _currentState = GameState.Starting;
                }

                using FileStream stream = new(
                    logPath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite | FileShare.Delete);

                if (_lastLogPosition > stream.Length)
                {
                    _lastLogPosition = 0;
                    _currentState = GameState.Starting;
                }

                stream.Seek(_lastLogPosition, SeekOrigin.Begin);

                using StreamReader reader = new(stream);

                string? line;

                while ((line = reader.ReadLine()) != null)
                {
                    ProcessLogLine(line);
                }

                _lastLogPosition = stream.Position;
            }
            catch
            {
                // Keep the last known state if the log is temporarily locked.
            }
        }

        private void ProcessLogLine(string line)
        {
            if (line.Contains(
                    "Join PU",
                    StringComparison.OrdinalIgnoreCase)
                || line.Contains(
                    "Session Manager [Request Connect]",
                    StringComparison.OrdinalIgnoreCase)
                || line.Contains(
                    "Connect started",
                    StringComparison.OrdinalIgnoreCase))
            {
                _currentState = GameState.Loading;
                return;
            }

            if (_currentState == GameState.Loading
                && (line.Contains(
                        "taskname=\"InGame\"",
                        StringComparison.OrdinalIgnoreCase)
                    || line.Contains(
                        "EGameContextState::eEGS_Running",
                        StringComparison.OrdinalIgnoreCase)))
            {
                _currentState = GameState.InServer;
                return;
            }

            if (line.Contains(
                    "RequestFrontEnd",
                    StringComparison.OrdinalIgnoreCase)
                || line.Contains(
                    "Loading GameModeRecord='SC_Frontend'",
                    StringComparison.OrdinalIgnoreCase))
            {
                _currentState = GameState.MainMenu;
            }
        }

        private void ResetLogTracking()
        {
            _lastLogPosition = 0;
            _currentLogPath = null;
            _currentState = GameState.GameClosed;
        }
    }
}