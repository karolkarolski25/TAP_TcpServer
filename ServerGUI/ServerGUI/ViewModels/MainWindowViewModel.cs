﻿using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Events;
using ServerLibrary;
using ServerLibrary.Events;
using ServerLibrary.Services;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
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
        public string ServerLogs { get; set; } = string.Empty;
        public bool CanStartServer { get; set; } = true;


        private DelegateCommand _startServerCommand;
        public DelegateCommand StartServerCommand => _startServerCommand ??= new DelegateCommand(StartServer).ObservesCanExecute(() => CanStartServer);

        private DelegateCommand _clearServerLogsCommand;
        public DelegateCommand ClearServerLogsCommang => _clearServerLogsCommand ??= new DelegateCommand(ClearLogs);

        private DelegateCommand _saveLogsCommand;
        public DelegateCommand SaveLogsCommand => _saveLogsCommand ??= new DelegateCommand(SaveLogs);

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
            _eventAggregator.GetEvent<ServerLogsChanged>().Subscribe(UpdateServerLogs);

            InitializeTimer();
        }

        /// <summary>
        /// Save server logs in .txt file
        /// </summary>
        private void SaveLogs()
        {
            try
            {
                SaveFileDialog dlg = new SaveFileDialog
                {
                    FileName = "ServerLogs",
                    DefaultExt = ".text",
                    Filter = "Text documents (.txt)|*.txt"
                };

                if (dlg.ShowDialog() == true)
                {
                    string filePath = dlg.FileName;

                    using var writer = new StreamWriter(filePath);

                    writer.Write(ServerLogs);

                    MessageBox.Show($"File has been sucessfully saved\n{filePath}", "Saved",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during saving file\n{ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Clear server logs in GUI
        /// </summary>
        private void ClearLogs()
        {
            ServerLogs = string.Empty;

            OnPropertyChanged(nameof(ServerLogs));
        }

        /// <summary>
        /// Append new logs into GUI
        /// </summary>
        /// <param name="obj">new logs</param>
        private void UpdateServerLogs(string obj)
        {
            ServerLogs += $"{obj}\n";

            OnPropertyChanged(nameof(ServerLogs));
        }

        /// <summary>
        /// Start time and date timer
        /// </summary>
        private void InitializeTimer()
        {
            dispatcherTimer = new DispatcherTimer(DispatcherPriority.Normal);
            dispatcherTimer.Interval = TimeSpan.FromSeconds(1);
            dispatcherTimer.Tick += (sender, eventArgs) =>
            {
                CurrentTimeAndDate = DateTime.Now.ToString("dddd, dd MMMM, yyyy\n\tHH:mm:ss");

                OnPropertyChanged(nameof(CurrentTimeAndDate));
            };
            dispatcherTimer.Start();
        }

        /// <summary>
        /// Wrong server configuration occured
        /// </summary>
        /// <param name="message">error message</param>
        private void WrongServerConfiguration(string message)
        {
            MessageBox.Show($"Wrong server configuration\n{message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        /// <summary>
        /// New user connected to server
        /// </summary>
        private void UserConnected()
        {
            UserStatus = $"Users connected: {usersConnectedCount++}";

            OnPropertyChanged(nameof(UserStatus));
        }

        /// <summary>
        /// Client disconnected from server
        /// </summary>
        private void UserDisconnected()
        {
            UserStatus = $"Users connected: {usersConnectedCount--}";

            OnPropertyChanged(nameof(UserStatus));
        }

        /// <summary>
        /// Tcp server started
        /// </summary>
        private void ServerStarted()
        {
            ServerStatus = "Status: Running";

            OnPropertyChanged(nameof(ServerStatus));

            CanStartServer = false;
        }

        /// <summary>
        /// Start tcp server
        /// </summary>
        private async void StartServer()
        {
            await Task.Run(() => _serverService.StartServer(ServerIP, ServerPort));
        }

        /// <summary>
        /// Updates GUI components
        /// </summary>
        /// <param name="propertyName">Property to update</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
