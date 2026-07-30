using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using VersePulse.App.Telemetry;
using VersePulse.App.Telemetry.Parsing;
using VersePulse.App.Telemetry.Parsing.Parsers;

namespace VersePulse.App
{
    public partial class MainWindow : Window
    {
        private readonly DispatcherTimer
            _gameStateTimer;

        private readonly SettingsService
            _settingsService;

        private readonly InstallationManager
            _installationManager;

        private readonly GameStateService
            _gameStateService;

        public MainWindow()
        {
            InitializeComponent();

            _settingsService =
                new SettingsService();

            _installationManager =
                new InstallationManager(
                    _settingsService);

            ILogReader logReader =
                new LogReader();

            ParserPipeline parserPipeline =
                new(
                    new ILogParser[]
                    {
                        new SessionParser(),
                        new EnvironmentParser(),
                        new GameStateParser(),
                        new PerformanceParser()
                    });

            _gameStateService =
                new GameStateService(
                    _installationManager,
                    logReader,
                    parserPipeline);

            _gameStateTimer =
                new DispatcherTimer
                {
                    Interval =
                        TimeSpan.FromSeconds(1)
                };

            _gameStateTimer.Tick +=
                GameStateTimer_Tick;

            _gameStateTimer.Start();

            UpdateGameState();
        }

        private void GameStateTimer_Tick(
            object? sender,
            EventArgs e)
        {
            UpdateGameState();
        }

        private void UpdateGameState()
        {
            TelemetryData telemetry =
                _gameStateService
                    .GetTelemetry();

            ClientFPSText.Text =
                telemetry.ClientFps.HasValue
                    ? $"{telemetry.ClientFps.Value:F1}"
                    : "--";

            CpuUsageText.Text =
                telemetry.CpuUsagePercent.HasValue
                    ? $"{telemetry.CpuUsagePercent.Value:F1} %"
                    : "-- %";

            RamUsageText.Text =
                telemetry.RamUsageMb.HasValue
                    ? $"{telemetry.RamUsageMb.Value:N0} MB"
                    : "-- MB";

            CommittedMemoryText.Text =
                telemetry.CommittedMemoryMb.HasValue
                    ? $"{telemetry.CommittedMemoryMb.Value:N0} MB"
                    : "-- MB";

            switch (telemetry.GameState)
            {
                case GameState.GameClosed:
                    SetGameStatus(
                        "Game closed",
                        240,
                        179,
                        90);
                    break;

                case GameState.LauncherOpen:
                    SetGameStatus(
                        "Launcher",
                        84,
                        174,
                        255);
                    break;

                case GameState.Starting:
                    SetGameStatus(
                        "Starting",
                        84,
                        174,
                        255);
                    break;

                case GameState.MainMenu:
                    SetGameStatus(
                        "Main menu",
                        84,
                        174,
                        255);
                    break;

                case GameState.Loading:
                    SetGameStatus(
                        "Loading",
                        255,
                        193,
                        71);
                    break;

                case GameState.InServer:
                    SetGameStatus(
                        "In server",
                        50,
                        205,
                        90);
                    break;
            }

            DeveloperStateText.Text =
                FormatGameState(
                    telemetry.GameState);

            SessionText.Text =
                telemetry.SessionId;

            EnvironmentText.Text =
                telemetry.ServerName;

            RegionText.Text =
                telemetry.Region;

            LinesParsedText.Text =
                telemetry.LinesParsed
                    .ToString("N0");

            LastEventText.Text =
                telemetry.LastEvent;

            InstallationInfo installation =
                _gameStateService
                    .GetInstallation();

            RootPathText.Text =
                DisplayValue(
                    installation.RootPath);

            ChannelText.Text =
                DisplayValue(
                    installation.Channel);

            ExecutablePathText.Text =
                DisplayValue(
                    installation.ExecutablePath);

            LogFileText.Text =
                telemetry.LogFilePath != "--"
                    ? telemetry.LogFilePath
                    : DisplayValue(
                        installation.GameLogPath);
        }

        private static string DisplayValue(
            string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? "--"
                : value;
        }

        private static string FormatGameState(
            GameState gameState)
        {
            return gameState switch
            {
                GameState.GameClosed =>
                    "Game closed",

                GameState.LauncherOpen =>
                    "Launcher",

                GameState.Starting =>
                    "Starting",

                GameState.MainMenu =>
                    "Main menu",

                GameState.Loading =>
                    "Loading",

                GameState.InServer =>
                    "In server",

                _ => "Unknown"
            };
        }

        private void SetGameStatus(
            string text,
            byte red,
            byte green,
            byte blue)
        {
            GameStatusText.Text =
                text;

            GameStatusText.Foreground =
                new SolidColorBrush(
                    Color.FromRgb(
                        red,
                        green,
                        blue));
        }

        private void DeveloperModeButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            bool isVisible =
                DeveloperPanel.Visibility
                == Visibility.Visible;

            DeveloperPanel.Visibility =
                isVisible
                    ? Visibility.Collapsed
                    : Visibility.Visible;

            DeveloperModeButton.Content =
                isVisible
                    ? "Show Developer Diagnostics"
                    : "Hide Developer Diagnostics";

            Height =
                isVisible
                    ? 431
                    : 820;
        }

        private void ChangeGameLocationButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            _gameStateTimer.Stop();

            try
            {
                InstallationInfo? installation =
                    _installationManager
                        .RequestInstallation();

                if (installation == null)
                {
                    return;
                }

                _gameStateService
                    .SetInstallation(
                        installation);

                MessageBox.Show(
                    $"Star Citizen installation saved.\n\nChannel: {installation.Channel}",
                    "VersePulse",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                UpdateGameState();
            }
            finally
            {
                _gameStateTimer.Start();
            }
        }

        protected override void OnClosed(
            EventArgs e)
        {
            _gameStateTimer.Stop();

            base.OnClosed(e);
        }
    }
}
