using Microsoft.Win32;
using System.Diagnostics;
using System.IO;

namespace VersePulse.App
{
    public sealed class InstallationManager
    {
        private const string ExecutableName = "StarCitizen.exe";
        private const string Bin64FolderName = "Bin64";
        private const string GameLogFileName = "Game.log";

        private readonly SettingsService _settingsService;

        public InstallationManager(
            SettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        public InstallationInfo GetOrRequestInstallation()
        {
            AppSettings settings = _settingsService.Load();

            MigrateLegacySettings(settings);

            InstallationInfo? runningInstallation =
                FindFromRunningProcess();

            if (runningInstallation != null)
            {
                SaveInstallation(runningInstallation, settings);
                return runningInstallation;
            }

            InstallationInfo? savedInstallation =
                FindFromRoot(
                    settings.StarCitizenRootPath,
                    settings.LastChannel);

            if (savedInstallation != null)
            {
                SaveInstallation(savedInstallation, settings);
                return savedInstallation;
            }

            InstallationInfo? detectedInstallation =
                FindAutomatically();

            if (detectedInstallation != null)
            {
                SaveInstallation(detectedInstallation, settings);
                return detectedInstallation;
            }

            return RequestInstallation()
                   ?? InstallationInfo.Empty;
        }

        public InstallationInfo ResolveActiveInstallation()
        {
            AppSettings settings = _settingsService.Load();

            MigrateLegacySettings(settings);

            InstallationInfo? runningInstallation =
                FindFromRunningProcess();

            if (runningInstallation != null)
            {
                SaveInstallation(runningInstallation, settings);
                return runningInstallation;
            }

            InstallationInfo? configuredInstallation =
                FindFromRoot(
                    settings.StarCitizenRootPath,
                    settings.LastChannel);

            return configuredInstallation
                   ?? InstallationInfo.Empty;
        }

        public InstallationInfo? RequestInstallation()
        {
            AppSettings settings = _settingsService.Load();

            MigrateLegacySettings(settings);

            OpenFileDialog dialog = new()
            {
                Title = "Locate StarCitizen.exe",
                Filter =
                    "Star Citizen executable (StarCitizen.exe)|StarCitizen.exe",
                FileName = ExecutableName,
                CheckFileExists = true,
                CheckPathExists = true,
                Multiselect = false
            };

            string? initialDirectory =
                GetInitialDirectory(settings);

            if (!string.IsNullOrWhiteSpace(initialDirectory))
            {
                dialog.InitialDirectory = initialDirectory;
            }

            bool? result = dialog.ShowDialog();

            if (result != true)
            {
                return null;
            }

            InstallationInfo? installation =
                CreateFromExecutablePath(dialog.FileName);

            if (installation == null)
            {
                return null;
            }

            return SaveInstallation(installation, settings)
                ? installation
                : null;
        }

        private void MigrateLegacySettings(
            AppSettings settings)
        {
            if (!string.IsNullOrWhiteSpace(
                    settings.StarCitizenRootPath))
            {
                return;
            }

            InstallationInfo? legacyInstallation =
                CreateFromExecutablePath(
                    settings.StarCitizenExecutablePath);

            if (legacyInstallation == null)
            {
                return;
            }

            settings.StarCitizenRootPath =
                legacyInstallation.RootPath;

            settings.LastChannel =
                legacyInstallation.Channel;

            settings.StarCitizenExecutablePath = null;

            _settingsService.Save(settings);
        }

        private bool SaveInstallation(
            InstallationInfo installation,
            AppSettings settings)
        {
            bool unchanged =
                string.Equals(
                    settings.StarCitizenRootPath,
                    installation.RootPath,
                    StringComparison.OrdinalIgnoreCase)
                && string.Equals(
                    settings.LastChannel,
                    installation.Channel,
                    StringComparison.OrdinalIgnoreCase)
                && string.IsNullOrWhiteSpace(
                    settings.StarCitizenExecutablePath);

            if (unchanged)
            {
                return true;
            }

            settings.StarCitizenRootPath =
                installation.RootPath;

            settings.LastChannel =
                installation.Channel;

            settings.StarCitizenExecutablePath = null;

            return _settingsService.Save(settings);
        }

        private static InstallationInfo?
            FindFromRunningProcess()
        {
            
            try
            {
                using Process? process = Process
                    .GetProcessesByName("StarCitizen")
                    .FirstOrDefault();

                return CreateFromExecutablePath(
                    process?.MainModule?.FileName);
            }
            catch
            {
                return null;
            }
        }

        private static InstallationInfo?
            FindAutomatically()
        {
            foreach (string rootPath
                     in BuildCandidateRootPaths()
                     .Distinct(StringComparer.OrdinalIgnoreCase)
                     )
            {
                InstallationInfo? installation =
                    FindFromRoot(rootPath, null);

                if (installation != null)
                {
                    return installation;
                }
            }

            return null;
        
        }


        private static InstallationInfo?
            FindFromRoot(
                string? rootPath,
                string? preferredChannel)
        {
            if (string.IsNullOrWhiteSpace(rootPath)
                || !Directory.Exists(rootPath))
            {
                return null;
            }

            IEnumerable<string> channelFolders;

            try
            {
                channelFolders = Directory
                    .EnumerateDirectories(rootPath)
                    .OrderBy(
                        path => GetChannelPriority(
                            Path.GetFileName(path),
                            preferredChannel))
                    .ThenBy(
                        path => Path.GetFileName(path),
                        StringComparer.OrdinalIgnoreCase)
                    .ToArray();
            }
            catch
            {
                return null;
            }

            foreach (string channelFolder
                     in channelFolders)
            {
                string executablePath = Path.Combine(
                    channelFolder,
                    Bin64FolderName,
                    ExecutableName);

                InstallationInfo? installation =
                    CreateFromExecutablePath(
                        executablePath);

                if (installation != null)
                {
                    return installation;
                }
            }

            return null;
        }

        private static int GetChannelPriority(
            string channel,
            string? preferredChannel)
        {
            if (!string.IsNullOrWhiteSpace(preferredChannel)
                && string.Equals(
                    channel,
                    preferredChannel,
                    StringComparison.OrdinalIgnoreCase))
            {
                return 0;
            }

            return channel.ToUpperInvariant() switch
            {
                "LIVE" => 1,
                "PTU" => 2,
                "EPTU" => 3,
                "TECH-PREVIEW" => 4,
                _ => 5
            };
        }

        private static InstallationInfo?
            CreateFromExecutablePath(
                string? executablePath)
        {
            if (string.IsNullOrWhiteSpace(executablePath)
                || !File.Exists(executablePath)
                || !string.Equals(
                    Path.GetFileName(executablePath),
                    ExecutableName,
                    StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            DirectoryInfo? bin64Folder =
                Directory.GetParent(executablePath);

            if (bin64Folder == null
                || !string.Equals(
                    bin64Folder.Name,
                    Bin64FolderName,
                    StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            DirectoryInfo? channelFolder =
                bin64Folder.Parent;

            DirectoryInfo? rootFolder =
                channelFolder?.Parent;

            if (channelFolder == null
                || rootFolder == null)
            {
                return null;
            }

            return new InstallationInfo
            {
                RootPath = rootFolder.FullName,
                Channel = channelFolder.Name,
                ExecutablePath = executablePath,
                GameLogPath = Path.Combine(
                    channelFolder.FullName,
                    GameLogFileName)
            };
        }

        private static IEnumerable<string>
            BuildCandidateRootPaths()
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

                yield return Path.Combine(
                    root,
                    "Program Files",
                    "Roberts Space Industries",
                    "StarCitizen");

                yield return Path.Combine(
                    root,
                    "Program Files (x86)",
                    "Roberts Space Industries",
                    "StarCitizen");

                yield return Path.Combine(
                    root,
                    "Roberts Space Industries",
                    "StarCitizen");

                yield return Path.Combine(
                    root,
                    "Games",
                    "Roberts Space Industries",
                    "StarCitizen");

                yield return Path.Combine(
                    root,
                    "RSI",
                    "StarCitizen");

                yield return Path.Combine(
                    root,
                    "Games",
                    "RSI",
                    "StarCitizen");
            }
        }

        private static string? GetInitialDirectory(
            AppSettings settings)
        {
            InstallationInfo? configuredInstallation =
                FindFromRoot(
                    settings.StarCitizenRootPath,
                    settings.LastChannel);

            if (configuredInstallation != null)
            {
                return Path.GetDirectoryName(
                    configuredInstallation.ExecutablePath);
            }

            string programFiles =
                Environment.GetFolderPath(
                    Environment.SpecialFolder.ProgramFiles);

            string defaultDirectory = Path.Combine(
                programFiles,
                "Roberts Space Industries",
                "StarCitizen");

            return Directory.Exists(defaultDirectory)
                ? defaultDirectory
                : programFiles;
        }
    }
}
