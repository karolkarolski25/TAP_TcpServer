using LoginLibrary.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using WeatherLibrary.Services;

namespace ServerLibrary.Services
{
    public class ServerService : IServerService
    {
        private readonly IWeatherService _weatherService;
        private readonly ILoginService _loginService;
        private readonly ILogger<ServerService> _logger;
        private readonly ServerConfiguration _serverConfiguration;

        private readonly string enterLocationMessage = "Enter location (Only english letters, exit to disconnect): ";
        private readonly string fethcingDataFromAPIMessage = "\r\nFetching data from API\r\n";
        private readonly string enterLoginMessage = "Login: ";
        private readonly string enterPasswordMessage = "Password: ";
        private readonly string registerMessage = "Account not found, do you want to create new account? (Y/N): ";

        public ServerService(IWeatherService weatherService, ServerConfiguration serverConfiguration,
            ILoginService loginService, ILogger<ServerService> logger)
        {
            _weatherService = weatherService;
            _loginService = loginService;
            _serverConfiguration = serverConfiguration;
            _logger = logger;
        }

        /// <summary>
        /// Checks if given server configuration is correct
        /// </summary>
        /// <returns>True or false</returns>
        private (bool result, string message) IsServerConfigurationCorrect()
        {
            string wrongServerConfigurationMessage = string.Empty;

            try
            {
                if (_serverConfiguration.Port < 1024 || _serverConfiguration.Port > 65535)
                {
                    wrongServerConfigurationMessage += "- Port number is wrong\n";
                }

                if (_serverConfiguration.WeatherBufferSize != 85)
                {
                    wrongServerConfigurationMessage += "- Buffer size is incorrect\n";
                }

                IPAddress.Parse(_serverConfiguration.IpAddress);

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
                string location = Encoding.ASCII.GetString(buffer);

                if (location.IndexOf("exit") >= 0)
                {
                    return "exit";
                }

                else if (location.IndexOf("??") < 0)
                {
                    if (location.IndexOf("\r\n") < 0)
                    {
                        await stream.WriteAsync(Encoding.ASCII.GetBytes(fethcingDataFromAPIMessage), 0, fethcingDataFromAPIMessage.Length);

                        location = new string(location.Where(c => c != '\0').ToArray());

                        string weatherData = await _weatherService.GetWeather(location);

                        _logger.LogInformation($"Weather for location: {location}: \n {weatherData}\n");

                        byte[] weather = Encoding.ASCII.GetBytes(weatherData);

                        await stream.WriteAsync(weather, 0, weather.Length);

                        await stream.WriteAsync(Encoding.ASCII.GetBytes(enterLocationMessage), 0, enterLocationMessage.Length);
                    }

                    Array.Clear(buffer, 0, buffer.Length);
                }

                else if (location.IndexOf("??") >= 0)
                {
                    string nonAsciiCharsMessage = "\r\nNon ASCII char detected (use only english letters, exit to disconnect), try again\r\n\n";

                    await stream.WriteAsync(Encoding.ASCII.GetBytes(nonAsciiCharsMessage), 0, nonAsciiCharsMessage.Length);

                    await stream.WriteAsync(Encoding.ASCII.GetBytes(enterLocationMessage), 0, enterLocationMessage.Length);

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
        /// Gets login from user
        /// </summary>
        /// <param name="stream">client stream</param>
        /// <param name="buffer">buffer for weather data</param>
        /// <returns>Login from user</returns>
        private async Task<string> GetLoginString(NetworkStream stream, byte[] buffer)
        {
            string data = string.Empty;

            await stream.WriteAsync(Encoding.ASCII.GetBytes(enterLoginMessage), 0, enterLoginMessage.Length);

            await stream.ReadAsync(buffer, 0, buffer.Length).ContinueWith(
                async (t) =>
                {
                    data = Encoding.ASCII.GetString(buffer);
                });

            data = data.Replace("\0", "");

            data += ";";

            return data;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream">client stream</param>
        /// <param name="buffer">buffer for weather data</param>
        /// <param name="data">current string for sign in</param>
        /// <returns>Password from user</returns>
        private async Task<string> GetPasswordString(NetworkStream stream, byte[] buffer, string data)
        {
            await stream.WriteAsync(Encoding.ASCII.GetBytes(enterPasswordMessage), 0, enterPasswordMessage.Length);

            await stream.ReadAsync(buffer, 0, buffer.Length).ContinueWith(
                async (t) =>
                {
                    data += Encoding.ASCII.GetString(buffer);
                });
            data = data.Replace("\0", "");

            return data;
        }

        /// <summary>
        /// Opeartes welcome message
        /// </summary>
        /// <param name="stream">client stream</param>
        /// <param name="buffer">buffer for weather data</param>
        /// <param name="signInBuffer">Buffer with logging cridentials</param>
        /// <returns>task for handling logging</returns>
        private async Task HandleLogin(NetworkStream stream, byte[] signInBuffer, string data)
        {
            if (!_loginService.CheckData(data))
            {
                await stream.WriteAsync(Encoding.ASCII.GetBytes(registerMessage), 0, registerMessage.Length);

                await stream.ReadAsync(signInBuffer, 0, signInBuffer.Length).ContinueWith(
                async (t) =>
                {
                    string response = Encoding.ASCII.GetString(signInBuffer);

                    if (response[0] == 'Y' || response[0] == 'y')
                    {
                        _loginService.RegisterAccount(data);
                        Console.WriteLine($"New user: {data.Substring(0, data.IndexOf(';'))} registered");
                        _logger.LogInformation($"New user: {data.Substring(0, data.IndexOf(';'))} registered");
                    }
                });
            }
            else
            {
                Console.WriteLine($"User: {data.Substring(0, data.IndexOf(';'))} logged in");
                _logger.LogInformation($"User: {data.Substring(0, data.IndexOf(';'))} logged in");
            }

            data = data.Substring(0, data.IndexOf(';'));
            data = "Welcome " + data + "\r\n";

            await stream.WriteAsync(Encoding.ASCII.GetBytes(data), 0, data.Length);
        }

        /// <summary>
        /// Starts TCP server
        /// </summary>
        /// <returns>Task for tcp server</returns>
        public async Task StartServer()
        {
            var serverConfigurationResult = IsServerConfigurationCorrect();

            if (serverConfigurationResult.result)
            {
                TcpListener server = new TcpListener(IPAddress.Parse(_serverConfiguration.IpAddress), _serverConfiguration.Port);

                server.Start();

                Console.WriteLine($"Starting Server at ipAddress: {_serverConfiguration.IpAddress}, port: {_serverConfiguration.Port}");

                _logger.LogInformation($"Starting Server at ipAddress: {_serverConfiguration.IpAddress}, port: {_serverConfiguration.Port}");

                while (true)
                {
                    TcpClient client = await server.AcceptTcpClientAsync();

                    Console.WriteLine("Client connected");

                    _logger.LogInformation("Client connected");

                    byte[] signInBuffer = new byte[_serverConfiguration.LoginBufferSize];
                    string data = string.Empty;

                    data += await GetLoginString(client.GetStream(), signInBuffer);

                    await client.GetStream().ReadAsync(signInBuffer, 0, 2);

                    data = await GetPasswordString(client.GetStream(), signInBuffer, data);

                    data = data.Substring(0, data.Length - 1);

                    await client.GetStream().ReadAsync(signInBuffer, 0, 2);

                    await HandleLogin(client.GetStream(), signInBuffer, data);

                    byte[] weatherBudder = new byte[_serverConfiguration.WeatherBufferSize];

                    await client.GetStream().WriteAsync(Encoding.ASCII.GetBytes(enterLocationMessage), 0, enterLocationMessage.Length);

                    client.GetStream().ReadAsync(weatherBudder, 0, weatherBudder.Length).ContinueWith(
                        async (t) =>
                        {
                            while (true)
                            {
                                if (await ProcessWeatherCommunication(client.GetStream(), weatherBudder) == "exit")
                                {
                                    client.Close();
                                }
                            }
                        });
                }
            }
            else
            {
                Console.WriteLine($"Server configuration is wrong, \n{serverConfigurationResult.message}");

                _logger.LogInformation("Server configuration is wrong");
            }
        }
    }
}
