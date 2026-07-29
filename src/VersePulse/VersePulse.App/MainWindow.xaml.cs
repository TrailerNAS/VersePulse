using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace VersePulse.App
{
    public partial class MainWindow : Window
    {
        private readonly DispatcherTimer
            _gameStateTimer;

        private readonly SettingsService
            _settingsService;

        private readonly StarCitizenPathService
            _pathService;

        private readonly GameStateService
            _gameStateService;

        public MainWindow()
        {
            InitializeComponent();

            _settingsService =
                new SettingsService();

            _pathService =
                new StarCitizenPathService(
                    _settingsService);

            string? executablePath =
                _pathService
                    .GetOrRequestExecutablePath();

            _gameStateService =
                new GameStateService(
                    executablePath);

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

            LogFileText.Text =
                telemetry.LogFilePath;

            GameLocationText.Text =
                _gameStateService
                    .GetConfiguredExecutablePath();
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
                    : 730;
        }

        private void ChangeGameLocationButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            _gameStateTimer.Stop();

            try
            {
                string? selectedPath =
                    _pathService
                        .RequestExecutablePath();

                if (string.IsNullOrWhiteSpace(
                        selectedPath))
                {
                    return;
                }

                _gameStateService
                    .SetStarCitizenExecutablePath(
                        selectedPath);

                GameLocationText.Text =
                    selectedPath;

                MessageBox.Show(
                    "Star Citizen location saved.",
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