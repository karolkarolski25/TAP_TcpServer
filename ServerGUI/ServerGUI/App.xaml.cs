using LoginLibrary;
using LoginLibrary.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;
using ServerGUI.ViewModels;
using ServerLibrary;
using ServerLibrary.Services;
using Storage.DAL;
using StorageLibrary;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
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

                Task.Run(async () =>
                {
                    var storageService = _serviceProvider.GetRequiredService<IStorageService>();

                    try
                    {
                        await storageService.MigrateAsync();
                    }
                    catch
                    {
                        Console.WriteLine("ERROR");
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error occured during application startup\n{ex.Message}", "ERROR",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Configuring dependiences
        /// </summary>
        /// <param name="servicesCollection">services collection</param>
        private void ConfigureServices(IServiceCollection servicesCollection)
        {
            var weatherConfiguration = _configuration.GetSection("WeatherApi").Get<WeatherApiConfiguration>();
            var serverConfiguration = _configuration.GetSection("ServerConfiguration").Get<ServerConfiguration>();
            var cryptoConfiguration = _configuration.GetSection("CryptoConfiguration").Get<CryptoConfiguration>();
            var databaseConfiguration = _configuration.GetSection("DatabaseConfiguration").Get<DatabaseConfiguration>();

            servicesCollection
                .AddSingleton(_configuration)
                .AddSingleton(weatherConfiguration)
                .AddSingleton(serverConfiguration)
                .AddSingleton(cryptoConfiguration)
                .AddSingleton(databaseConfiguration)
                .AddSingleton<MainWindow>()
                .AddSingleton<MainWindowViewModel>()
                .AddSingleton<IWeatherService, WeatherService>()
                .AddSingleton<ILoginService, LoginService>()
                .AddSingleton<IServerService, ServerService>()
                .AddSingleton<IEventAggregator, EventAggregator>()
                .AddLogging(builder => builder.AddFile(_configuration.GetSection("Logs")));

            servicesCollection.RegisterDALDependiences();
        }

        /// <summary>
        /// Method called after application startup
        /// </summary>
        /// <param name="e">startup event</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            _logger = _serviceProvider.GetService<ILogger<App>>();

            _logger.LogDebug("Application startup");

            _serviceProvider.GetService<MainWindow>().Show();
        }

        /// <summary>
        /// Method called before application quits
        /// </summary>
        /// <param name="e">Exit event</param>
        protected override void OnExit(ExitEventArgs e)
        {
            _logger.LogInformation("Application exit");

            _serviceProvider.Dispose();

            base.OnExit(e);
        }
    }
}
