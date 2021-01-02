using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace Weather.Services
{
    public class WeatherService : IWeatherService
    {
        private readonly ILogger<WeatherService> _logger;
        private readonly WeatherApiConfiguration _weatherApiConfiguration;

        private string weatherUrl = $"http://api.openweathermap.org/data/2.5/forecast?q=@lokalizacja@&mode=xml&units=metric&appid=@api@";

        public WeatherService(WeatherApiConfiguration weatherApiConfiguration, ILogger<WeatherService> logger)
        {
            _logger = logger;
            _weatherApiConfiguration = weatherApiConfiguration;

            weatherUrl = weatherUrl.Replace("@api@", _weatherApiConfiguration.ApiKey);
        }

        /// <summary>
        /// Calculates weather period
        /// </summary>
        /// <param name="weatherDate">Date</param>
        /// <returns>Days naumber between today and given date</returns>
        public int CalculateWeatherPeriod(string weatherDate)
        {
            int days;

            if (Regex.IsMatch(weatherDate, "[0-9]{2}-{1}[0-9]{2}-{1}[0-9]{4}"))
            {
                DateTime date;

                if (DateTime.TryParse(weatherDate, out date))
                {
                    var currentDate = Convert.ToDateTime(DateTime.Now.ToShortDateString());
                    days = (int)(date - currentDate).TotalDays + 1;
                }
                else
                {
                    days = -1;
                }
            }
            else
            {
                if (!int.TryParse(weatherDate, out days))
                {
                    days = -1;
                }
            }

            return days;
        }

        /// <summary>
        /// Filters XML data
        /// </summary>
        /// <param name="xml">xml document downloaded from API</param>
        /// <returns>Filtered weather data as weather model </returns>
        private List<Weather> ParseWeather(string xml, int days)
        {
            double pressure = 0, windSpeed = 0, visibility = 0;
            double humidity = 0, feelsLikeTemp = 0;

            string windName = "", windDirection = "", cloudsName = "";

            List<Weather> weatherForecast = new List<Weather>();
            List<string> weatherMeassurementTime = new List<string>();
            List<double> temps = new List<double>();

            XmlDocument xmlDocument = new XmlDocument();

            xmlDocument.LoadXml(xml);

            foreach (XmlNode time_node in xmlDocument.SelectNodes("//time"))
            {
                string day = time_node.Attributes["from"].Value;
                day = day.Substring(0, day.IndexOf("T"));

                weatherMeassurementTime.Add(day);

                if (weatherMeassurementTime.First() == day)
                {
                    temps.Add(Convert.ToDouble(time_node.SelectSingleNode("temperature").Attributes["value"].Value.Replace('.', ',')));

                    pressure += Convert.ToDouble(time_node.SelectSingleNode("pressure").Attributes["value"].Value);

                    XmlNode windNode = time_node.SelectSingleNode("windDirection");
                    windDirection = windNode.Attributes["name"].Value;

                    windNode = time_node.SelectSingleNode("windSpeed");

                    windSpeed += Convert.ToDouble(windNode.Attributes["mps"].Value.Replace('.', ','));
                    windName = windNode.Attributes["name"].Value;

                    feelsLikeTemp += Convert.ToDouble(time_node.SelectSingleNode("feels_like").Attributes["value"].Value.Replace('.', ','));

                    visibility += Convert.ToDouble(time_node.SelectSingleNode("visibility").Attributes["value"].Value);

                    humidity += Convert.ToDouble(time_node.SelectSingleNode("humidity").Attributes["value"].Value);

                    cloudsName = time_node.SelectSingleNode("clouds").Attributes["value"].Value;
                }

                else
                {
                    weatherForecast.Add
                    (
                        new Weather()
                        {
                            Day = weatherMeassurementTime.First(),
                            Location = xmlDocument.SelectSingleNode("weatherdata/location").SelectSingleNode("name").InnerText,
                            Temperature = $"{Math.Round(temps.Average(), 2)} 'C",
                            MinTemperature = $"{Math.Round(temps.Min(), 2)} 'C",
                            MaxTemperature = $"{Math.Round(temps.Max(), 2)} 'C",
                            Humidity = $"{Math.Round(humidity / weatherMeassurementTime.Count - 1, 2)} %",
                            Pressure = $"{Math.Round(pressure / weatherMeassurementTime.Count - 1, 2)} hPa",
                            FeelsLikeTemperature = $"{Math.Round(feelsLikeTemp / weatherMeassurementTime.Count - 1, 2)} 'C",
                            Visibility = $"{Math.Round(visibility / weatherMeassurementTime.Count - 1, 2)} m",
                            WindSpeed = $"{Math.Round(windSpeed / weatherMeassurementTime.Count - 1, 2)} m/s",
                            WindName = windName,
                            WindDirection = windDirection,
                            CloudsName = cloudsName,
                        }
                    );

                    weatherMeassurementTime.Clear();
                    temps.Clear();

                    pressure = 0; windSpeed = 0; visibility = 0;
                    humidity = 0; feelsLikeTemp = 0;
                }
            }

            weatherForecast.Add
            (
                new Weather()
                {
                    Day = weatherMeassurementTime.First(),
                    Location = xmlDocument.SelectSingleNode("weatherdata/location").SelectSingleNode("name").InnerText,
                    Temperature = $"{Math.Round(temps.Average(), 2)} 'C",
                    MinTemperature = $"{Math.Round(temps.Min(), 2)} 'C",
                    MaxTemperature = $"{Math.Round(temps.Max(), 2)} 'C",
                    Humidity = $"{Math.Round(humidity / weatherMeassurementTime.Count - 1, 2)} %",
                    Pressure = $"{Math.Round(pressure / weatherMeassurementTime.Count - 1, 2)} hPa",
                    FeelsLikeTemperature = $"{Math.Round(feelsLikeTemp / weatherMeassurementTime.Count - 1, 2)} 'C",
                    Visibility = $"{Math.Round(visibility / weatherMeassurementTime.Count - 1, 2)} m",
                    WindSpeed = $"{Math.Round(windSpeed / weatherMeassurementTime.Count - 1, 2)} m/s",
                    WindName = windName,
                    WindDirection = windDirection,
                    CloudsName = cloudsName,
                }
            );

            return weatherForecast.Take(days).ToList();
        }

        /// <summary>
        /// Converts weather model into string
        /// </summary>
        /// <param name="weatherModel">weather data stored in weather model</param>
        /// <returns>weather data converted into string</returns>
        private string ConvertToString(List<Weather> weatherModel)
        {
            StringBuilder weatherString = new StringBuilder();

            weatherString.Append($"\r\nLocation: {weatherModel.First().Location}\r\n");

            foreach (var item in weatherModel)
            {
                weatherString.Append($"\r\nDay: {item.Day}\r\n\r\n");
                weatherString.Append($"Temperature: {item.Temperature}\r\n");
                weatherString.Append($"Max temperature: {item.MaxTemperature}\r\n");
                weatherString.Append($"Min temperature: {item.MinTemperature}\r\n");
                weatherString.Append($"Humidity: {item.Humidity}\r\n");
                weatherString.Append($"Pressure: {item.Pressure}\r\n");
                weatherString.Append($"Feels like temperature: {item.FeelsLikeTemperature}\r\n");
                weatherString.Append($"Visibility: {item.Visibility}\r\n");
                weatherString.Append($"Wind speed: {item.WindSpeed}\r\n");
                weatherString.Append($"Wind name: {item.WindName}\r\n");
                weatherString.Append($"Wind direction: {item.WindDirection}\r\n");
                weatherString.Append($"Clouds: {item.CloudsName}\r\n");
                weatherString.Append("\r\n");
            }

            return weatherString.ToString();
        }

        /// <summary>
        /// Returns weather data downloaded from API
        /// </summary>
        /// <param name="location">locaiotn</param>
        /// <returns>string containing weather data</returns>
        public async Task<string> GetWeather(string location, int days)
        {
            using var webClient = new WebClient();

            try
            {
                var apiContent = await webClient.DownloadStringTaskAsync(weatherUrl.Replace("@lokalizacja@", location));
                return await Task.Run(() => $"{ConvertToString(ParseWeather(apiContent, days))}\n");
            }
            catch (Exception ex)
            {
                return $"\r\nError: {ex.Message}\r\n\n";
            }
        }
    }
}
