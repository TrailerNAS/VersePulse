using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace VersePulse.App
{
    public sealed class StarCitizenPathService
    {
        private readonly SettingsService _settingsService;

        public StarCitizenPathService(
            SettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        public string? GetOrRequestExecutablePath()
        {
            AppSettings settings =
                _settingsService.Load();

            if (IsValidExecutable(
                    settings.StarCitizenExecutablePath))
            {
                return settings.StarCitizenExecutablePath;
            }

            string? runningGamePath =
                FindFromRunningProcess();

            if (SaveAndReturnIfValid(
                    runningGamePath,
                    settings,
                    out string? savedRunningPath))
            {
                return savedRunningPath;
            }

            string? automaticallyDetectedPath =
                FindAutomatically();

            if (SaveAndReturnIfValid(
                    automaticallyDetectedPath,
                    settings,
                    out string? savedDetectedPath))
            {
                return savedDetectedPath;
            }

            return RequestExecutablePath();
        }

        public string? RequestExecutablePath()
        {
            AppSettings settings =
                _settingsService.Load();

            string? currentPath =
                settings.StarCitizenExecutablePath;

            OpenFileDialog dialog = new()
            {
                Title = "Locate StarCitizen.exe",
                Filter =
                    "Star Citizen executable (StarCitizen.exe)|StarCitizen.exe",
                FileName = "StarCitizen.exe",
                CheckFileExists = true,
                CheckPathExists = true,
                Multiselect = false
            };

            string? initialDirectory =
                GetInitialDirectory(currentPath);

            if (!string.IsNullOrWhiteSpace(
                    initialDirectory))
            {
                dialog.InitialDirectory =
                    initialDirectory;
            }

            bool? result = dialog.ShowDialog();

            if (result != true)
            {
                return null;
            }

            if (!IsValidExecutable(dialog.FileName))
            {
                return null;
            }

            settings.StarCitizenExecutablePath =
                dialog.FileName;

            return _settingsService.Save(settings)
                ? dialog.FileName
                : null;
        }

        public string? GetSavedExecutablePath()
        {
            AppSettings settings =
                _settingsService.Load();

            return IsValidExecutable(
                    settings.StarCitizenExecutablePath)
                ? settings.StarCitizenExecutablePath
                : null;
        }

        public static string? GetGameLogPath(
            string? executablePath)
        {
            if (!IsValidExecutable(executablePath))
            {
                return null;
            }

            string? bin64Folder =
                Path.GetDirectoryName(executablePath);

            if (string.IsNullOrWhiteSpace(
                    bin64Folder))
            {
                return null;
            }

            DirectoryInfo? gameChannelFolder =
                Directory.GetParent(bin64Folder);

            if (gameChannelFolder == null)
            {
                return null;
            }

            return Path.Combine(
                gameChannelFolder.FullName,
                "Game.log");
        }

        private static string? FindFromRunningProcess()
        {
            Process? process = null;

            try
            {
                process = Process
                    .GetProcessesByName("StarCitizen")
                    .FirstOrDefault();

                return process?.MainModule?.FileName;
            }
            catch
            {
                return null;
            }
            finally
            {
                process?.Dispose();
            }
        }

        private static string? FindAutomatically()
        {
            foreach (string candidatePath
                     in BuildCandidatePaths())
            {
                if (IsValidExecutable(candidatePath))
                {
                    return candidatePath;
                }
            }

            return null;
        }

        private static IEnumerable<string>
            BuildCandidatePaths()
        {
            foreach (DriveInfo drive
                     in DriveInfo.GetDrives())
            {
                bool isReady;

                try
                {
                    isReady = drive.IsReady;
                }
                catch
                {
                    isReady = false;
                }

                if (!isReady)
                {
                    continue;
                }

                string root =
                    drive.RootDirectory.FullName;

                string[] baseFolders =
                {
                    Path.Combine(
                        root,
                        "Program Files",
                        "Roberts Space Industries"),

                    Path.Combine(
                        root,
                        "Program Files (x86)",
                        "Roberts Space Industries"),

                    Path.Combine(
                        root,
                        "Roberts Space Industries"),

                    Path.Combine(
                        root,
                        "Games",
                        "Roberts Space Industries"),

                    Path.Combine(
                        root,
                        "RSI"),

                    Path.Combine(
                        root,
                        "Games",
                        "RSI")
                };

                string[] channels =
                {
                    "LIVE",
                    "PTU",
                    "EPTU",
                    "TECH-PREVIEW"
                };

                foreach (string baseFolder
                         in baseFolders)
                {
                    foreach (string channel
                             in channels)
                    {
                        yield return Path.Combine(
                            baseFolder,
                            "StarCitizen",
                            channel,
                            "Bin64",
                            "StarCitizen.exe");
                    }
                }
            }
        }

        private bool SaveAndReturnIfValid(
            string? executablePath,
            AppSettings settings,
            out string? savedPath)
        {
            savedPath = null;

            if (!IsValidExecutable(
                    executablePath))
            {
                return false;
            }

            settings.StarCitizenExecutablePath =
                executablePath!;

            if (!_settingsService.Save(settings))
            {
                return false;
            }

            savedPath = executablePath;

            return true;
        }

        private static string? GetInitialDirectory(
            string? executablePath)
        {
            if (!string.IsNullOrWhiteSpace(
                    executablePath))
            {
                string? savedDirectory =
                    Path.GetDirectoryName(
                        executablePath);

                if (!string.IsNullOrWhiteSpace(
                        savedDirectory)
                    && Directory.Exists(
                        savedDirectory))
                {
                    return savedDirectory;
                }
            }

            string programFiles =
                Environment.GetFolderPath(
                    Environment.SpecialFolder.ProgramFiles);

            string defaultDirectory =
                Path.Combine(
                    programFiles,
                    "Roberts Space Industries",
                    "StarCitizen");

            return Directory.Exists(defaultDirectory)
                ? defaultDirectory
                : programFiles;
        }

        private static bool IsValidExecutable(
            string? executablePath)
        {
            return !string.IsNullOrWhiteSpace(
                       executablePath)
                   && File.Exists(executablePath)
                   && string.Equals(
                       Path.GetFileName(
                           executablePath),
                       "StarCitizen.exe",
                       StringComparison.OrdinalIgnoreCase);
        }
    }
}