using System.Net.Sockets;
using System.Threading.Tasks;

namespace WeatherLibrary.Services
{
    public interface IWeatherService
    {
        Task<string> GetWeather(string location, int days);
        Task<int> GetWeatherPeriod(NetworkStream stream, byte[] daysPeriodBuffer);
    }
}
