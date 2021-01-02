using System.Threading.Tasks;

namespace Weather.Services
{
    public interface IWeatherService
    {
        Task<string> GetWeather(string location, int days);
        int CalculateWeatherPeriod(string weatherDate);
    }
}
