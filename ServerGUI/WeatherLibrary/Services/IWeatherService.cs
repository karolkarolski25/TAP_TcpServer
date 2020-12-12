using System.Threading.Tasks;

namespace WeatherLibrary.Services
{
    public interface IWeatherService
    {
        Task<string> GetWeather(string location, int days);
    }
}
