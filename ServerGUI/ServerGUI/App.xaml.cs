using LoginLibrary;
using LoginLibrary.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ServerGUI.ViewModels;
using ServerLibrary;
using ServerLibrary.Services;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using WeatherLibrary;
using WeatherLibrary.Services;

namespace ServerGUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        private ILogger<App> _logger;

        public App()
        {
            try
            {
                _configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .Build();

                var servicesCollection = new ServiceCollection();
                ConfigureServices(servicesCollection);

                _serviceProvider = servicesCollection.BuildServiceProvider();

                DispatcherUnhandledException += OnException;
                TaskScheduler.UnobservedTaskException += OnException;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error occured during application startup\n{ex.Message}", "ERROR",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            OnException(e.Exception);
        }

        private void OnException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            OnException(e.Exception);
        }

        private void OnException(Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occured");

            MessageBox.Show("Exception occured\nContact with application developers", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void ConfigureServices(IServiceCollection servicesCollection)
        {
            var weatherApiConfiguration = _configuration.GetSection("WeatherApi").Get<WeatherApiConfiguration>();
            var serverConfiguration = _configuration.GetSection("ServerConfiguration").Get<ServerConfiguration>();
            var cryptoConfiguration = _configuration.GetSection("CryptoConfiguration").Get<CryptoConfiguration>();

            servicesCollection
                .AddSingleton(_configuration)
                .AddSingleton(weatherApiConfiguration)
                .AddSingleton(serverConfiguration)
                .AddSingleton(cryptoConfiguration)
                .AddSingleton<MainWindow>()
                .AddSingleton<MainWindowViewModel>()
                .AddSingleton<IWeatherService, WeatherService>()
                .AddSingleton<ILoginService, LoginService>()
                .AddSingleton<IServerService, ServerService>()
                .AddLogging(builder => builder.AddFile(_configuration.GetSection("Logs")));
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            _logger = _serviceProvider.GetService<ILogger<App>>();

            _logger.LogDebug("Application startup");

            _serviceProvider.GetService<MainWindow>().Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _logger.LogInformation("Application exit");

            _serviceProvider.Dispose();

            base.OnExit(e);
        }
    }
}
