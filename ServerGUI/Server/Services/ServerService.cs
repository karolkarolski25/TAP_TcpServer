using Login.Enums;
using Login.Services;
using Microsoft.Extensions.Logging;
using Prism.Events;
using Server.Events;
using Server.Resources;
using Storage.DAL;
using Storage.Models;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Weather.Services;

namespace Server.Services
{
    public class ServerService : IServerService
    {
        private readonly IWeatherService _weatherService;
        private readonly ILoginService _loginService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IStorageService _storageService;
        private readonly ILogger<ServerService> _logger;
        private readonly ServerConfiguration _serverConfiguration;

        private bool badCredentials = false;

        public ServerService(IWeatherService weatherService, ServerConfiguration serverConfiguration,
            ILoginService loginService, ILogger<ServerService> logger, IEventAggregator eventAggregator, 
            IStorageService storageService)
        {
            _weatherService = weatherService;
            _loginService = loginService;
            _serverConfiguration = serverConfiguration;
            _logger = logger;
            _eventAggregator = eventAggregator;
            _storageService = storageService;
        }

        /// <summary>
        /// Checks if given server configuration is correct
        /// </summary>
        /// <returns>True or false</returns>
        private (bool result, string message) IsServerConfigurationCorrect(string ipAddress, int port)
        {
            string wrongServerConfigurationMessage = string.Empty;

            try
            {
                if (port < 1024 || port > 65535)
                {
                    wrongServerConfigurationMessage += "- Port number is wrong\n";
                }

                if (_serverConfiguration.WeatherBufferSize != 85)
                {
                    wrongServerConfigurationMessage += "- Buffer size is incorrect\n";
                }

                IPAddress.Parse(ipAddress);

                if (wrongServerConfigurationMessage.Length > 0)
                {
                    throw new Exception();
                }

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                if (ex is FormatException || ex is ArgumentNullException)
                {
                    wrongServerConfigurationMessage += "- Server ip address is wrong\n";
                }

                _logger.LogDebug($"Server configuration message: {wrongServerConfigurationMessage}");

                return (false, wrongServerConfigurationMessage);
            }
        }

        /// <summary>
        /// Opeartes weather communication
        /// </summary>
        /// <param name="stream">client stream</param>
        /// <param name="buffer">buffer for weather data</param>
        /// <returns>string containing current state</returns>
        private async Task<string> ProcessWeatherCommunication(NetworkStream stream, byte[] buffer)
        {
            try
            {
                string receivedData = Encoding.ASCII.GetString(buffer);

                if (receivedData.IndexOf("exit") >= 0)
                {
                    return "exit";
                }
                else if (receivedData.IndexOf("change") >= 0)
                {
                    await HandlePasswordChange(stream);

                    Array.Clear(buffer, 0, buffer.Length);

                    await stream.ReadAsync(buffer, 0, _serverConfiguration.WeatherBufferSize);

                    return "ok";
                }
                else if (receivedData.IndexOf("favourite") >= 0)
                {
                    await HandleFavouriteLocationSave(stream);

                    Array.Clear(buffer, 0, buffer.Length);

                    await stream.ReadAsync(buffer, 0, _serverConfiguration.WeatherBufferSize);

                    return "ok";
                }

                else if (receivedData.IndexOf("??") < 0)
                {
                    if (receivedData.IndexOf("\r\n") < 0)
                    {
                        string[] locations = new string(receivedData.Where(c => c != '\0').ToArray()).Split(',');

                        var daysPeriodBuffer = new byte[_serverConfiguration.WeatherBufferSize];

                        int days = await GetWeatherPeriod(stream, daysPeriodBuffer);

                        foreach (var location in locations)
                        {
                            await stream.WriteAsync(Encoding.ASCII.GetBytes(ServerMessagesResources.FethcingDataFromAPIMessage),
                               0, ServerMessagesResources.FethcingDataFromAPIMessage.Length);

                            _eventAggregator.GetEvent<ServerLogsChanged>().Publish($"Weather forecast requested for {location} for {days} day(s)");

                            string weatherData = await _weatherService.GetWeather(location, days);

                            _logger.LogInformation($"Weather for location: {location} for {days} days: \n {weatherData}\n");

                            byte[] weather = Encoding.ASCII.GetBytes(weatherData);

                            await stream.WriteAsync(weather, 0, weather.Length);
                        }

                        await stream.WriteAsync(Encoding.ASCII.GetBytes(ServerMessagesResources.EnterLocationMessage),
                            0, ServerMessagesResources.EnterLocationMessage.Length);
                    }

                    Array.Clear(buffer, 0, buffer.Length);
                }

                else if (receivedData.IndexOf("??") >= 0)
                {
                    await stream.WriteAsync(Encoding.ASCII.GetBytes(ServerMessagesResources.NonAsciiCharsMessage),
                        0, ServerMessagesResources.NonAsciiCharsMessage.Length);

                    await stream.WriteAsync(Encoding.ASCII.GetBytes(ServerMessagesResources.EnterLocationMessage),
                        0, ServerMessagesResources.EnterLocationMessage.Length);

                    _logger.LogInformation("Non ASCII char detected in location");

                    Array.Clear(buffer, 0, buffer.Length);
                }

                await stream.ReadAsync(buffer, 0, _serverConfiguration.WeatherBufferSize);

                return "ok";
            }
            catch
            {
                return "exit";
            }
        }

        /// <summary>
        /// Gets correct weather period from client
        /// </summary>
        /// <param name="stream">client stream</param>
        /// <returns>dates period eg. 3</returns>
        private async Task<int> GetWeatherPeriod(NetworkStream stream, byte[] daysPeriodBuffer)
        {
            await stream.WriteAsync(Encoding.ASCII.GetBytes(ServerMessagesResources.EnterTimePeriodMessage),
                0, ServerMessagesResources.EnterTimePeriodMessage.Length);

            do
            {
                Array.Clear(daysPeriodBuffer, 0, daysPeriodBuffer.Length);
                await stream.ReadAsync(daysPeriodBuffer, 0, daysPeriodBuffer.Length);
            } while (Encoding.ASCII.GetString(daysPeriodBuffer).Contains("\r\n"));

            string weatherDate = Encoding.ASCII.GetString(daysPeriodBuffer);

            int days;

            if (weatherDate.Contains('-'))
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

            while (days < 1 || days > 6)
            {
                await stream.WriteAsync(Encoding.ASCII.GetBytes(ServerMessagesResources.WrongTimePeriodMessage),
                    0, ServerMessagesResources.WrongTimePeriodMessage.Length);

                await stream.WriteAsync(Encoding.ASCII.GetBytes(ServerMessagesResources.EnterTimePeriodMessage),
                    0, ServerMessagesResources.EnterTimePeriodMessage.Length);

                do
                {
                    Array.Clear(daysPeriodBuffer, 0, daysPeriodBuffer.Length);
                    await stream.ReadAsync(daysPeriodBuffer, 0, daysPeriodBuffer.Length);
                } while (Encoding.ASCII.GetString(daysPeriodBuffer).Contains("\r\n"));

                weatherDate = Encoding.ASCII.GetString(daysPeriodBuffer);

                if (weatherDate.Contains('-'))
                {
                    DateTime date;

                    if (DateTime.TryParse(weatherDate, out date))
                    {
                        var currentDate = Convert.ToDateTime(DateTime.Now.ToShortDateString());
                        days = (int)(date - currentDate).TotalDays;
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
            }

            return days;
        }

        /// <summary>
        /// Gets login from user
        /// </summary>
        /// <param name="stream">client stream</param>
        /// <param name="buffer">buffer for weather data</param>
        /// <returns>Login from user</returns>
        private async Task<string> GetLoginString(NetworkStream stream, byte[] buffer)
        {
            await stream.WriteAsync(Encoding.ASCII.GetBytes(ServerMessagesResources.EnterLoginMessage),
                0, ServerMessagesResources.EnterLoginMessage.Length);

            Array.Clear(buffer, 0, buffer.Length);

            await stream.ReadAsync(buffer, 0, buffer.Length);

            return Encoding.ASCII.GetString(buffer).Replace("\0", "");
        }

        /// <summary>
        /// Gets password from user
        /// </summary>
        /// <param name="stream">client stream</param>
        /// <param name="buffer">buffer for weather data</param>
        /// <param name="data">current string for sign in</param>
        /// <returns>Password from user</returns>
        private async Task<string> GetPasswordString(NetworkStream stream, byte[] buffer)
        {
            await stream.WriteAsync(Encoding.ASCII.GetBytes(ServerMessagesResources.EnterPasswordMessage),
                0, ServerMessagesResources.EnterPasswordMessage.Length);

            Array.Clear(buffer, 0, buffer.Length);

            await stream.ReadAsync(buffer, 0, buffer.Length);

            return Encoding.ASCII.GetString(buffer).Replace("\0", "");
        }

        private async Task HandleLogin(NetworkStream stream, byte[] signInBuffer, string login, string password)
        {
            badCredentials = false;

            var userCheck = await _loginService.CheckData(login, password);

            if (userCheck == UserLoginSettings.UserNotExists)
            {
                await stream.WriteAsync(Encoding.ASCII.GetBytes(ServerMessagesResources.RegisterMessage),
                    0, ServerMessagesResources.RegisterMessage.Length);

                await stream.ReadAsync(signInBuffer, 0, signInBuffer.Length);

                string response = Encoding.ASCII.GetString(signInBuffer);

                if (char.ToUpper(response[0]) == 'Y')
                {
                    await _loginService.RegisterAccount(login, password);

                    _eventAggregator.GetEvent<ServerLogsChanged>().Publish($"New user: {login} registered");

                    _logger.LogInformation($"New user: {login} registered");
                }
                else if (char.ToUpper(response[0]) == 'N')
                {
                    badCredentials = true;
                    await stream.ReadAsync(signInBuffer, 0, signInBuffer.Length);

                    return;
                }
            }
            else if (userCheck == UserLoginSettings.LoggedIn)
            {
                _eventAggregator.GetEvent<ServerLogsChanged>().Publish($"User: {login} logged in");

                _eventAggregator.GetEvent<UserLoggedInEvent>().Publish();

                _logger.LogInformation($"User: {login} logged in");
            }
            else
            {
                _eventAggregator.GetEvent<ServerLogsChanged>().Publish($"User: {login} bad password");
                _logger.LogInformation($"User: {login} bad password");

                badCredentials = true;

                await stream.WriteAsync(Encoding.ASCII.GetBytes(ServerMessagesResources.BadPasswordMessage),
                    0, ServerMessagesResources.BadPasswordMessage.Length);

                return;
            }

            string data = $"Welcome {login}\r\n";

            await stream.WriteAsync(Encoding.ASCII.GetBytes(data), 0, data.Length);
        }

        private async Task HandlePasswordChange(NetworkStream stream)
        {
            byte[] changeBuffer = new byte[85];
            await stream.ReadAsync(changeBuffer, 0, changeBuffer.Length);
            string data = Encoding.ASCII.GetString(changeBuffer);
            data = data.Replace("\0", "");

            string login = data.Substring(0, data.IndexOf(';'));
            string password = data.Substring(data.IndexOf(';') + 1);

            if (await _loginService.ChangePassword(login, password))
            {
                _eventAggregator.GetEvent<ServerLogsChanged>().Publish($"User: {login} changed password");

                _logger.LogInformation($"User: {login} changed password");

                data = "Password changed\r\n";
            }
            else
            {
                _eventAggregator.GetEvent<ServerLogsChanged>().Publish($"Error while changing User: {login} password");

                _logger.LogInformation($"Error while changing User: {login} password");

                data = "Error password not changed\r\n";
            }
            await stream.WriteAsync(Encoding.ASCII.GetBytes(data), 0, data.Length);
        }

        private async Task HandleFavouriteLocationSave(NetworkStream stream)
        {
            byte[] locationBuffer = new byte[256];
            await stream.ReadAsync(locationBuffer, 0, locationBuffer.Length);
            string data = Encoding.ASCII.GetString(locationBuffer);
            data = data.Replace("\0", "");

            string login = data.Substring(0, data.IndexOf(';'));
            string locations = data.Substring(data.IndexOf(';') + 1);

            try
            {
                _storageService.UpdateData(new User()
                {
                    Login = login,
                    FavouriteLocations = locations
                });

                _eventAggregator.GetEvent<ServerLogsChanged>().Publish($"User: {login} saved favourite location(s) ({locations})");

                _logger.LogInformation($"User: {login} saved favourite location(s) ({locations})");

                data = "Favourite location saved\r\n";
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ServerLogsChanged>().Publish($"Error while saving User: {login} favourite location(s)");

                _logger.LogInformation($"Error while saving User: {login} favourite location(s) ({ex.Message})");

                data = "Error favourite location not saved\r\n";
            }
            finally
            {
                await stream.WriteAsync(Encoding.ASCII.GetBytes(data), 0, data.Length);
            }
        }

        /// <summary>
        /// Start Tcp server
        /// </summary>
        /// <param name="ipAddress">Server ip address</param>
        /// <param name="port">Server port number</param>
        /// <returns>Task for tcp server</returns>
        public async Task StartServer(string ipAddress, int port)
        {
            var serverConfigurationResult = IsServerConfigurationCorrect(ipAddress, port);

            if (serverConfigurationResult.result)
            {
                TcpListener server = new TcpListener(IPAddress.Parse(ipAddress), port);

                server.Start();

                _eventAggregator.GetEvent<ServerLogsChanged>().Publish($"Starting Server at ipAddress: " +
                    $"{ipAddress}, port: {port}");

                _logger.LogInformation($"Starting Server at ipAddress: {ipAddress}, port: {port}");

                _eventAggregator.GetEvent<ServerStartedEvent>().Publish();

                while (true)
                {
                    TcpClient client = await server.AcceptTcpClientAsync();

                    _eventAggregator.GetEvent<ServerLogsChanged>().Publish("Client connected");

                    _logger.LogInformation("Client connected");

                    byte[] signInBuffer = new byte[_serverConfiguration.LoginBufferSize];
                    byte[] weatherBuffer = new byte[_serverConfiguration.WeatherBufferSize];

                    string data = string.Empty;
                    string login = string.Empty;

                    Task.Run(async () =>
                     {
                         do
                         {
                             login = await GetLoginString(client.GetStream(), signInBuffer);

                             await client.GetStream().ReadAsync(signInBuffer, 0, 2);

                             var password = await GetPasswordString(client.GetStream(), signInBuffer);

                             await client.GetStream().ReadAsync(signInBuffer, 0, 2);

                             await HandleLogin(client.GetStream(), signInBuffer, login, password);

                         } while (badCredentials);

                         await client.GetStream().WriteAsync(Encoding.ASCII.GetBytes("fav" + _storageService.GetFavouriteLocations(login)));

                         await client.GetStream().WriteAsync(Encoding.ASCII.GetBytes(ServerMessagesResources.EnterLocationMessage),
                             0, ServerMessagesResources.EnterLocationMessage.Length);

                         await client.GetStream().ReadAsync(weatherBuffer, 0, weatherBuffer.Length);

                         while (true)
                         {
                             if (await ProcessWeatherCommunication(client.GetStream(), weatherBuffer) == "exit")
                             {
                                 _logger.LogDebug("Client disconnected");

                                 _eventAggregator.GetEvent<UserDisconnectedEvent>().Publish();
                                 _eventAggregator.GetEvent<ServerLogsChanged>().Publish("Client disconnected");

                                 client.Close();
                             }
                         }
                     });
                }
            }

            else
            {
                _logger.LogInformation("Server configuration is wrong");

                _eventAggregator.GetEvent<WrongServerConfigurationEvent>().Publish(serverConfigurationResult.message);
            }
        }
    }
}
