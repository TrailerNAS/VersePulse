using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace VersePulse.App
{
    public partial class MainWindow : Window
    {
        private readonly DispatcherTimer _gameStateTimer;
        private readonly GameStateService _gameStateService;

        public MainWindow()
        {
            InitializeComponent();

            _gameStateService = new GameStateService();

            _gameStateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };

            _gameStateTimer.Tick += GameStateTimer_Tick;
            _gameStateTimer.Start();

            UpdateGameState();
        }

        private void GameStateTimer_Tick(object? sender, EventArgs e)
        {
            UpdateGameState();
        }

        private void UpdateGameState()
        {
            GameState state = _gameStateService.GetCurrentState();

            switch (state)
            {
                case GameState.GameClosed:
                    SetGameStatus("Game closed", 240, 179, 90);
                    break;

                case GameState.LauncherOpen:
                    SetGameStatus("Launcher", 84, 174, 255);
                    break;

                case GameState.Starting:
                    SetGameStatus("Starting", 84, 174, 255);
                    break;

                case GameState.MainMenu:
                    SetGameStatus("Main menu", 84, 174, 255);
                    break;

                case GameState.Loading:
                    SetGameStatus("Loading", 255, 193, 71);
                    break;

                case GameState.InServer:
                    SetGameStatus("In server", 50, 205, 90);
                    break;
            }
        }

        private void SetGameStatus(
            string text,
            byte red,
            byte green,
            byte blue)
        {
            GameStatusText.Text = text;

            GameStatusText.Foreground = new SolidColorBrush(
                Color.FromRgb(red, green, blue));
        }

        protected override void OnClosed(EventArgs e)
        {
            _gameStateTimer.Stop();
            base.OnClosed(e);
        }
    }
}