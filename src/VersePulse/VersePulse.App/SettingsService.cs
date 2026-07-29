using System;
using System.IO;
using System.Text.Json;

namespace VersePulse.App
{
    public sealed class SettingsService
    {
        private readonly string _settingsDirectory;
        private readonly string _settingsFilePath;

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

                string json = File.ReadAllText(
                    _settingsFilePath);

                return JsonSerializer.Deserialize<AppSettings>(json)
                       ?? new AppSettings();
            }
            catch
            {
                return new AppSettings();
            }
        }

        public bool Save(AppSettings settings)
        {
            try
            {
                Directory.CreateDirectory(
                    _settingsDirectory);

                string json = JsonSerializer.Serialize(
                    settings,
                    new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });

                File.WriteAllText(
                    _settingsFilePath,
                    json);

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