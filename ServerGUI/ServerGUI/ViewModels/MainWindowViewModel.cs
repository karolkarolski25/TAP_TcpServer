using Microsoft.Extensions.Logging;
using Prism.Commands;
using Prism.Mvvm;
using ServerLibrary.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Threading;

namespace ServerGUI.ViewModels
{
    public class MainWindowViewModel
    {
        private readonly ILogger<MainWindowViewModel> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IServerService _serverService;

        private DelegateCommand _startServerCommand;
        public DelegateCommand StartServerCommand => _startServerCommand ??= new DelegateCommand(StartServer);

        public MainWindowViewModel(ILogger<MainWindowViewModel> logger, IServiceProvider serviceProvider, 
            IServerService serverService)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _serverService = serverService;
        }

        private void StartServer()
        {
            _serverService.StartServer().Wait();
        }
    }
}
