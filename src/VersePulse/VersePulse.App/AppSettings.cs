using System.Text.Json.Serialization;

namespace VersePulse.App
{
    public sealed class AppSettings
    {
        public string StarCitizenRootPath { get; set; } = string.Empty;

        public string LastChannel { get; set; } = string.Empty;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? StarCitizenExecutablePath { get; set; }
    }
}
