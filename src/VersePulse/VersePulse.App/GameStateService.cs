using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using VersePulse.App.Telemetry;
using VersePulse.App.Telemetry.Parsing;

namespace VersePulse.App
{
    public sealed class GameStateService
    {
        private readonly InstallationManager _installationManager;
        private readonly ILogReader _logReader;
        private readonly ParserPipeline _parserPipeline;

        private InstallationInfo _installation =
            InstallationInfo.Empty;

        private readonly TelemetryData _telemetry =
            new();

        private readonly ProcessTelemetryService
    _processTelemetryService =
        new();

        public GameStateService(
            InstallationManager installationManager,
            ILogReader logReader,
            ParserPipeline parserPipeline)
        {
            _installationManager = installationManager;
            _logReader = logReader;
            _parserPipeline = parserPipeline;

            _installation =
                _installationManager.GetOrRequestInstallation();
        }

        public void RefreshInstallation()
        {
            InstallationInfo installation =
                _installationManager.ResolveActiveInstallation();

            if (string.Equals(
                    _installation.ExecutablePath,
                    installation.ExecutablePath,
                    StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            _installation = installation;
            ResetLogTracking();
        }

        public void SetInstallation(InstallationInfo installation)
        {
            ArgumentNullException.ThrowIfNull(installation);

            _installation = installation;
            ResetLogTracking();
        }

        public InstallationInfo GetInstallation()
        {
            return _installation;
        }

        public TelemetryData GetTelemetry()
        {
            RefreshInstallation();

            _processTelemetryService.Update(
                _telemetry);

            Process? starCitizenProcess =
                GetStarCitizenProcess();

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

            string? logPath = FindGameLogPath();

            if (string.IsNullOrWhiteSpace(logPath)
                || !File.Exists(logPath))
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
            Process[] processes;

            try
            {
                processes = Process.GetProcessesByName("RSI Launcher");
            }
            catch
            {
                return false;
            }

            foreach (Process process in processes)
            {
                try
                {
                    string processName = process.ProcessName;

                    bool containsRsi = processName.Contains(
                        "RSI",
                        StringComparison.OrdinalIgnoreCase);

                    bool containsLauncher = processName.Contains(
                        "Launcher",
                        StringComparison.OrdinalIgnoreCase);

                    if (containsRsi && containsLauncher)
                    {
                        return true;
                    }
                }
                catch
                {
                    // Ignore inaccessible processes.
                }
                finally
                {
                    process.Dispose();
                }
            }

            return false;
        }

        private string? FindGameLogPath()
        {
            return _installation.IsValid
                ? _installation.GameLogPath
                : null;
        }

        private void ReadNewLogEntries(string logPath)
        {
            try
            {
                LogReadResult result =
                    _logReader.ReadNewLines(logPath);

                if (result.Status == LogReadStatus.Connected)
                {
                    ResetSessionTelemetry("Game.log connected");
                }
                else if (result.Status == LogReadStatus.Restarted)
                {
                    ResetSessionTelemetry("Game.log restarted");
                }

                foreach (string line in result.Lines)
                {
                    _telemetry.LinesParsed++;
                    _parserPipeline.Parse(line, _telemetry);
                }
            }
            catch (IOException)
            {
                _telemetry.LastEvent =
                    "Game.log is temporarily locked";
            }
            catch (UnauthorizedAccessException)
            {
                 _telemetry.LastEvent =
                    "Access to Game.log was denied";
            }
            catch (Exception ex)
            {
                _telemetry.LastEvent = ex.Message;
            }
        }

        private void ResetSessionTelemetry(string lastEvent)
        {
            _telemetry.GameState = GameState.Starting;
            _telemetry.SessionId = "--";
            _telemetry.ServerName = "--";
            _telemetry.Region = "--";
            _telemetry.LinesParsed = 0;
            _telemetry.LastEvent = lastEvent;
            _telemetry.LogFilePath = "--";
        }

        private void ResetLogTracking()
        {
            _logReader.Reset();

            _telemetry.GameState = GameState.GameClosed;
            _telemetry.SessionId = "--";
            _telemetry.ServerName = "--";
            _telemetry.Region = "--";
            _telemetry.LogFilePath = "--";
            _telemetry.LinesParsed = 0;
            _telemetry.LastEvent = "--";
        }
    }
}
