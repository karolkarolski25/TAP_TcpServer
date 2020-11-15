using LoginLibrary;
using LoginLibrary.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ServerLibrary;
using ServerLibrary.Services;
using System;
using WeatherLibrary;
using WeatherLibrary.Services;

namespace TAP_TcpServer
{
    class Program
    {
        private static IConfiguration _configuration;
        private static ServiceProvider _serviceProvider;

        static void Main(string[] args)
        {
            try
            {
                _configuration = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json")
                    .Build();

                var servicesCollection = new ServiceCollection();
                ConfigureServices(servicesCollection);

                _serviceProvider = servicesCollection.BuildServiceProvider();

                _serviceProvider.GetRequiredService<IServerService>().Server().Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occured during application startup, {ex}");
            }
        }

        private static void ConfigureServices(IServiceCollection servicesCollection)
        {
            var weatherApiConfiguration = _configuration.GetSection("WeatherApi").Get<WeatherApiConfiguration>();
            var serverConfiguration = _configuration.GetSection("ServerConfiguration").Get<ServerConfiguration>();
            var cryptoConfiguration = _configuration.GetSection("CryptoConfiguration").Get<CryptoConfiguration>();

            servicesCollection
                .AddSingleton(_configuration)
                .AddSingleton(weatherApiConfiguration)
                .AddSingleton(serverConfiguration)
                .AddSingleton(cryptoConfiguration)
                .AddSingleton<ILoginService, LoginService>()
                .AddSingleton<IWeatherService, WeatherService>()
                .AddSingleton<IServerService, ServerService>()
                .AddLogging(builder => builder.AddFile(_configuration.GetSection("Logs")));
        }
    }
}
