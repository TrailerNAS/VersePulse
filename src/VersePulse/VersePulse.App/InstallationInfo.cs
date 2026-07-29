using System.IO;

namespace VersePulse.App
{
    public sealed class InstallationInfo
    {
        public static InstallationInfo Empty { get; } = new();

        public string RootPath { get; init; } = string.Empty;

        public string Channel { get; init; } = string.Empty;

        public string ExecutablePath { get; init; } = string.Empty;

        public string GameLogPath { get; init; } = string.Empty;

        public bool IsConfigured =>
            !string.IsNullOrWhiteSpace(RootPath);

        public bool IsValid =>
            IsConfigured
            && !string.IsNullOrWhiteSpace(Channel)
            && File.Exists(ExecutablePath);
    }
}