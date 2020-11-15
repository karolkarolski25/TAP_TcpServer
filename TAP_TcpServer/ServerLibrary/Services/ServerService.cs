using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using WeatherLibrary.Services;
using LoginLibrary.Services;

namespace ServerLibrary.Services //TODO implement server
{
    public class ServerService : IServerService
    {
        private readonly IWeatherService _weatherService;
        private readonly ServerConfiguration _serverConfiguration;
        private readonly ILoginService _loginService;

        private readonly string enterLocationMessage = "Enter location (Only english letters, exit to disconnect): ";
        private readonly string fethcingDataFromAPIMessage = "\r\nFetching data from API\r\n";
        private readonly string enterLoginMessage = "Login: ";
        private readonly string enterPasswordMessage = "Password: ";
        private readonly string registerMessage = "Account not found, do you want to create new account? (Y/N): ";

        public ServerService(IWeatherService weatherService, ServerConfiguration serverConfiguration, ILoginService loginService)
        {
            _weatherService = weatherService;
            _serverConfiguration = serverConfiguration;
            _loginService = loginService;
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

                    string data = String.Empty;

                    await client.GetStream().WriteAsync(Encoding.ASCII.GetBytes(enterLoginMessage), 0, enterLoginMessage.Length);

                    byte[] buffer = new byte[_serverConfiguration.WeatherBufferSize];

                    await client.GetStream().ReadAsync(buffer, 0, buffer.Length).ContinueWith(
                        async (t) => await Task.Run(() => data = Encoding.ASCII.GetString(buffer)));

                    data = data.Replace("\0", "");

                    data += ";";

                    await client.GetStream().ReadAsync(buffer, 0, 2);

                    await client.GetStream().WriteAsync(Encoding.ASCII.GetBytes(enterPasswordMessage), 0, enterPasswordMessage.Length);

                    await client.GetStream().ReadAsync(buffer, 0, buffer.Length).ContinueWith(
                        async (t) => await Task.Run(() => data += Encoding.ASCII.GetString(buffer)));

                    data = data.Replace("\0", "");
                    data = data.Substring(0, data.Length-1);

                    await client.GetStream().ReadAsync(buffer, 0, 2);

                    if (!_loginService.CheckData(data))
                    {
                        await client.GetStream().WriteAsync(Encoding.ASCII.GetBytes(registerMessage), 0, registerMessage.Length);

                        await client.GetStream().ReadAsync(buffer, 0, buffer.Length).ContinueWith(
                        async (t) =>
                        {
                            string response = Encoding.ASCII.GetString(buffer);
                            if(response[0] == 'Y' || response[0] == 'y')
                            {
                                _loginService.RegisterAccount(data);
                                Console.WriteLine("New user: " + data.Substring(0, data.IndexOf(';')) + " registered");
                            }
                        });
                    }
                    else
                    {
                        Console.WriteLine("User: " + data.Substring(0, data.IndexOf(';')) + " logged in");
                    }

                    data = data.Substring(0, data.IndexOf(';'));
                    data = "Welcome " + data + "\r\n";
                    await client.GetStream().WriteAsync(Encoding.ASCII.GetBytes(data), 0, data.Length);

                    await client.GetStream().WriteAsync(Encoding.ASCII.GetBytes(enterLocationMessage), 0, enterLocationMessage.Length);


                    await client.GetStream().ReadAsync(buffer, 0, buffer.Length).ContinueWith(
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
