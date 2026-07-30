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
        private readonly DispatcherTimer _gameStateTimer;
        private readonly SettingsService _settingsService;
        private readonly InstallationManager _installationManager;
        private readonly GameStateService _gameStateService;

        private static readonly Brush OfflineBrush =
            new SolidColorBrush(Color.FromRgb(240, 179, 90));

        private static readonly Brush BlueBrush =
            new SolidColorBrush(Color.FromRgb(84, 174, 255));

        private static readonly Brush LoadingBrush =
            new SolidColorBrush(Color.FromRgb(255, 193, 71));

        private static readonly Brush OnlineBrush =
            new SolidColorBrush(Color.FromRgb(50, 205, 90));

        private string _lastStatus = string.Empty;
        private Brush? _lastBrush;

        public MainWindow()
        {
            InitializeComponent();

            _settingsService = new SettingsService();

            _installationManager =
                new InstallationManager(_settingsService);

            ILogReader logReader = new LogReader();

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

            _gameStateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };

            _gameStateTimer.Tick += GameStateTimer_Tick;
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
            try
            {
                TelemetryData telemetry =
                    _gameStateService.GetTelemetry();

                SetText(
                    ClientFPSText,
                    telemetry.ClientFps.HasValue
                        ? $"{telemetry.ClientFps.Value:F1}"
                        : "--");

                SetText(
                    CpuUsageText,
                    telemetry.CpuUsagePercent.HasValue
                        ? $"{telemetry.CpuUsagePercent.Value:F1} %"
                        : "-- %");

                SetText(
                    RamUsageText,
                    telemetry.RamUsageMb.HasValue
                        ? $"{telemetry.RamUsageMb.Value:N0} MB"
                        : "-- MB");

                SetText(
                    CommittedMemoryText,
                    telemetry.CommittedMemoryMb.HasValue
                        ? $"{telemetry.CommittedMemoryMb.Value:N0} MB"
                        : "-- MB");

                switch (telemetry.GameState)
                {
                    case GameState.GameClosed:
                        SetGameStatus("Game closed", OfflineBrush);
                        break;

                    case GameState.LauncherOpen:
                        SetGameStatus("Launcher", BlueBrush);
                        break;

                    case GameState.Starting:
                        SetGameStatus("Starting", BlueBrush);
                        break;

                    case GameState.MainMenu:
                        SetGameStatus("Main menu", BlueBrush);
                        break;

                    case GameState.Loading:
                        SetGameStatus("Loading", LoadingBrush);
                        break;

                    case GameState.InServer:
                        SetGameStatus("In server", OnlineBrush);
                        break;
                }
            }
            catch
            {
                SetGameStatus("Error", OfflineBrush);
            }
        }

        private static void SetText(
            System.Windows.Controls.TextBlock textBlock,
            string value)
        {
            if (textBlock.Text != value)
            {
                textBlock.Text = value;
            }
        }

        private void SetGameStatus(
            string text,
            Brush brush)
        {
            if (_lastStatus != text)
            {
                GameStatusText.Text = text;
                _lastStatus = text;
            }

            if (!ReferenceEquals(_lastBrush, brush))
            {
                GameStatusText.Foreground = brush;
                _lastBrush = brush;
            }
        }

        private void ChangeGameLocationButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            _gameStateTimer.Stop();

            try
            {
                InstallationInfo? installation =
                    _installationManager.RequestInstallation();

                if (installation == null)
                {
                    return;
                }

                _gameStateService.SetInstallation(installation);

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

        protected override void OnClosed(EventArgs e)
        {
            _gameStateTimer.Stop();
            _gameStateTimer.Tick -= GameStateTimer_Tick;

            base.OnClosed(e);
        }
    }
}