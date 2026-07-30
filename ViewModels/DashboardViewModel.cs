using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace VersePulse.App.ViewModels
{
    public class DashboardViewModel : INotifyPropertyChanged
    {
        private string _gameStatus = "Game Closed";
        private string _clientFPS = "--";
        private string _serverFPS = "--";
        private string _ping = "--";
        private string _cpuUsage = "--";
        private string _ramUsage = "--";
        private string _committedMemory = "--";

        private string _region = "--";
        private string _serverName = "--";
        private string _session = "--";

        public event PropertyChangedEventHandler? PropertyChanged;

        #region Dashboard Cards

        public string GameStatus
        {
            get => _gameStatus;
            set => SetProperty(ref _gameStatus, value);
        }

        public string ClientFPS
        {
            get => _clientFPS;
            set => SetProperty(ref _clientFPS, value);
        }

        public string ServerFPS
        {
            get => _serverFPS;
            set => SetProperty(ref _serverFPS, value);
        }

        public string Ping
        {
            get => _ping;
            set => SetProperty(ref _ping, value);
        }

        public string CpuUsage
        {
            get => _cpuUsage;
            set => SetProperty(ref _cpuUsage, value);
        }

        public string RamUsage
        {
            get => _ramUsage;
            set => SetProperty(ref _ramUsage, value);
        }

        public string CommittedMemory
        {
            get => _committedMemory;
            set => SetProperty(ref _committedMemory, value);
        }

        public string Region
        {
            get => _region;
            set => SetProperty(ref _region, value);
        }

        public string ServerName
        {
            get => _serverName;
            set => SetProperty(ref _serverName, value);
        }

        public string Session
        {
            get => _session;
            set => SetProperty(ref _session, value);
        }

        #endregion

        #region Future Dashboard Cards

        // Reserved for future telemetry.
        // Add additional properties here as VersePulse grows.

        // GPU Usage
        // GPU Temperature
        // VRAM Usage
        // Player Location
        // Current Ship
        // Current Mission
        // Party Members
        // Network Latency
        // Packet Loss
        // etc.

        #endregion

        #region Helpers

        protected bool SetProperty<T>(
            ref T field,
            T value,
            [CallerMemberName] string propertyName = "")
        {
            if (Equals(field, value))
                return false;

            field = value;

            OnPropertyChanged(propertyName);

            return true;
        }

        protected virtual void OnPropertyChanged(
            [CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}