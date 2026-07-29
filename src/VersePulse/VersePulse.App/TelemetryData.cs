namespace VersePulse.App
{
    public class TelemetryData
    {
        public GameState GameState { get; set; }

        public string SessionId { get; set; } = "--";

        public string ServerName { get; set; } = "--";

        public string Region { get; set; } = "--";

        public string LogFilePath { get; set; } = "--";

        public string LastEvent { get; set; } = "Waiting for game...";

        public long LinesParsed { get; set; }

        public bool IsConnected =>
            GameState == GameState.InServer;
    }
}