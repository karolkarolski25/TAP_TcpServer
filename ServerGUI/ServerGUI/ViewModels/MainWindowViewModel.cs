using Microsoft.Extensions.Logging;
using Prism.Commands;
using Prism.Events;
using ServerLibrary;
using ServerLibrary.Events;
using ServerLibrary.Services;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;

namespace ServerGUI.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private readonly ILogger<MainWindowViewModel> _logger;
        private readonly ServerConfiguration _serverConfiguration;
        private readonly IServiceProvider _serviceProvider;
        private readonly IServerService _serverService;
        private readonly IEventAggregator _eventAggregator;

        private int usersConnectedCount = 0;
        private DispatcherTimer dispatcherTimer;

        public string ServerIP { get; set; }
        public int ServerPort { get; set; }
        public string ServerStatus { get; set; } = "Server status: Waiting";
        public string UserStatus { get; set; } = "Users conneted: 0";
        public string CurrentTimeAndDate { get; set; } = "OK";


        private DelegateCommand _startServerCommand;
        public DelegateCommand StartServerCommand => _startServerCommand ??= new DelegateCommand(StartServer);

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindowViewModel(ILogger<MainWindowViewModel> logger, IServiceProvider serviceProvider,
            IServerService serverService, ServerConfiguration serverConfiguration, IEventAggregator eventAggregator)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _serverService = serverService;
            _serverConfiguration = serverConfiguration;
            _eventAggregator = eventAggregator;

            ServerIP = _serverConfiguration.IpAddress;
            ServerPort = _serverConfiguration.Port;

            _eventAggregator.GetEvent<ServerStartedEvent>().Subscribe(ServerStarted);
            _eventAggregator.GetEvent<UserConnectedEvent>().Subscribe(UserConnected);
            _eventAggregator.GetEvent<UserDisconnectedEvent>().Subscribe(UserDisconnected);
            _eventAggregator.GetEvent<WrongServerConfigurationEvent>().Subscribe(WrongServerConfiguration);

            InitializeTimer();
        }

        private void InitializeTimer()
        {
            dispatcherTimer = new DispatcherTimer(DispatcherPriority.Normal);
            dispatcherTimer.Interval = TimeSpan.FromSeconds(1);
            dispatcherTimer.Tick += (sender, eventArgs) =>
            {
                CurrentTimeAndDate = DateTime.Now.ToString("dddd, dd MMMM, yyyy\n\tHH:mm:ss");

                OnPropertyChanged("CurrentTimeAndDate");

                Trace.WriteLine(CurrentTimeAndDate);
            };
            dispatcherTimer.Start();
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void WrongServerConfiguration(string message)
        {
            MessageBox.Show($"Wrong server configuration\n{message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void UserConnected()
        {
            usersConnectedCount++;

            UserStatus = $"Users connected: {usersConnectedCount}";
        }

        private void UserDisconnected()
        {
            usersConnectedCount--;

            UserStatus = $"Users connected: {usersConnectedCount}";
        }

        private void ServerStarted()
        {
            ServerStatus = "Status: Running";
        }

        private void StartServer()
        {
            _serverService.StartServer(ServerIP, ServerPort).Wait();
        }
    }
}
