namespace VersePulse.App
{
    public class TelemetryData
    {
        public GameState GameState { get; set; }

        public string SessionId { get; set; } = "--";

        public string ServerName { get; set; } = "--";

        public string Region { get; set; } = "--";

        public string LogFilePath { get; set; } = "--";

        public string LastEvent { get; set; } =
            "Waiting for game...";

        public long LinesParsed { get; set; }

        public double? ClientFps { get; set; }

        public double? MainThreadFps { get; set; }

        public double? ServerFps { get; set; }

        public double? CpuUsagePercent { get; set; }

        public double? RamUsageMb { get; set; }

        public double? CommittedMemoryMb { get; set; }

        public bool IsConnected =>
            GameState == GameState.InServer;
    }
}