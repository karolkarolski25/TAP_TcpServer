using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using WeatherLibrary.Services;

namespace ServerLibrary.Services //TODO implement server
{
    public class ServerService : IServerService
    {
        private readonly IWeatherService _weatherService;
        private readonly ServerConfiguration _serverConfiguration;

        private readonly string enterLocationMessage = "Enter location (Only english letters, exit to disconnect): ";
        private readonly string fethcingDataFromAPIMessage = "\r\nFetching data from API\r\n";

        public ServerService(IWeatherService weatherService, ServerConfiguration serverConfiguration)
        {
            _weatherService = weatherService;
            _serverConfiguration = serverConfiguration;
        }

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

                return (false, wrongServerConfigurationMessage);
            }
        }

        private async Task ProcessWeatherCommunication(NetworkStream stream, byte[] buffer)
        {
            try
            {
                string location = Encoding.ASCII.GetString(buffer);

                if (location.IndexOf("exit") >= 0)
                {
                    return;
                }

                else if (location.IndexOf("??") < 0)
                {
                    if (location.IndexOf("\r\n") < 0)
                    {
                        await stream.WriteAsync(Encoding.ASCII.GetBytes(fethcingDataFromAPIMessage), 0, fethcingDataFromAPIMessage.Length);

                        location = new string(location.Where(c => c != '\0').ToArray());

                        byte[] weather = Encoding.ASCII.GetBytes(await _weatherService.GetWeather(location));

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

                    Array.Clear(buffer, 0, buffer.Length);
                }

                await stream.ReadAsync(buffer, 0, _serverConfiguration.WeatherBufferSize);
            }
            catch
            {
                return;
            }
        }

        public async Task Server()
        {
            var serverConfigurationResult = IsServerConfigurationCorrect();

            if (serverConfigurationResult.result)
            {
                TcpListener server = new TcpListener(IPAddress.Parse(_serverConfiguration.IpAddress), _serverConfiguration.Port);

                server.Start();

                Console.WriteLine("Starting Server");

                while (true)
                {
                    TcpClient client = await server.AcceptTcpClientAsync();

                    Console.WriteLine("Client connected");

                    await client.GetStream().WriteAsync(Encoding.ASCII.GetBytes(enterLocationMessage), 0, enterLocationMessage.Length);

                    byte[] buffer = new byte[_serverConfiguration.WeatherBufferSize];

                    client.GetStream().ReadAsync(buffer, 0, buffer.Length).ContinueWith(
                        async (t) =>
                        {
                            while (true)
                            {
                                await ProcessWeatherCommunication(client.GetStream(), buffer);
                            }
                        });
                }
            }
            else
            {
                Console.WriteLine($"Server configuration is wrong, \n{serverConfigurationResult.message}");
            }
        }
    }
}
