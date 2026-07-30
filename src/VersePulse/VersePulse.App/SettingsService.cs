using System;
using System.IO;
using System.Text.Json;

namespace VersePulse.App
{
    public sealed class SettingsService
    {
        private readonly string _settingsDirectory;
        private readonly string _settingsFilePath;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true
        };

        public SettingsService()
        {
            _settingsDirectory = Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "VersePulse");

            _settingsFilePath = Path.Combine(
                _settingsDirectory,
                "settings.json");
        }

        public AppSettings Load()
        {
            try
            {
                if (!File.Exists(_settingsFilePath))
                {
                    return new AppSettings();
                }

                string json = File.ReadAllText(_settingsFilePath);

                AppSettings? settings =
                    JsonSerializer.Deserialize<AppSettings>(
                        json,
                        JsonOptions);

                settings ??= new AppSettings();

                return settings;
            }
            catch
            {
                // Corrupted or unreadable settings.
                return new AppSettings();
            }
        }

        public bool Save(AppSettings settings)
        {
            try
            {
                Directory.CreateDirectory(_settingsDirectory);

                string json = JsonSerializer.Serialize(
                    settings,
                    JsonOptions);

                string tempFile =
                    _settingsFilePath + ".tmp";

                File.WriteAllText(
                    tempFile,
                    json);

                File.Move(
                    tempFile,
                    _settingsFilePath,
                    true);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public string GetSettingsFilePath()
        {
            return _settingsFilePath;
        }
    }
}