using WeatherLibrary;

namespace ServerLibrary //TODO implement server
{
    public class ServerService : IServerService
    {
        private readonly IWeatherService _weatherService;
        private readonly ServerConfiguration _serverConfiguration;

        public ServerService(IWeatherService weatherService, ServerConfiguration serverConfiguration)
        {
            _weatherService = weatherService;
            _serverConfiguration = serverConfiguration;
        }

        public void Run()
        {
            _weatherService.GetWeather("kolo");
        }
    }
}
