using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServerLibrary;
using ServerLibrary.Services;
using WeatherLibrary;
using WeatherLibrary.Services;

namespace TPM
{
    class Program
    {
        private static IConfiguration _configuration;
        private static ServiceProvider _serviceProvider;

        static void Main(string[] args)
        {
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            var servicesCollection = new ServiceCollection();
            ConfigureServices(servicesCollection);

            _serviceProvider = servicesCollection.BuildServiceProvider();

            _serviceProvider.GetRequiredService<IServerService>().Server().Wait();
        }

        private static void ConfigureServices(IServiceCollection servicesCollection)
        {
            var weatherApiConfiguration = _configuration.GetSection("WeatherApi").Get<WeatherApiConfiguration>();
            var serverConfiguration = _configuration.GetSection("ServerConfiguration").Get<ServerConfiguration>();

            servicesCollection
                .AddSingleton(_configuration)
                .AddSingleton(weatherApiConfiguration)
                .AddSingleton(serverConfiguration)
                .AddSingleton<IWeatherService, WeatherService>()
                .AddSingleton<IServerService, ServerService>();
        }
    }
}
