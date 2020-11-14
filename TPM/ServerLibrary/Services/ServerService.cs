using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using WeatherLibrary.Services;

namespace ServerLibrary.Services //TODO implement server
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

        private bool IsServerConfigurationCorrect()
        {
            try
            {
                IPAddress.Parse(_serverConfiguration.IpAddress);

                if (_serverConfiguration.Port < 1024 || _serverConfiguration.Port > 65535)
                {
                    throw new Exception();
                }

                if (_serverConfiguration.BufferSize != 85)
                {
                    throw new Exception();
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task Server()
        {
            if (IsServerConfigurationCorrect())
            {
                TcpListener server = new TcpListener(IPAddress.Parse(_serverConfiguration.IpAddress), _serverConfiguration.Port);

                server.Start();

                Console.WriteLine("Starting Server");

                while (true)
                {
                    TcpClient client = await server.AcceptTcpClientAsync();

                    Console.WriteLine("Client connected");

                    byte[] buffer = new byte[_serverConfiguration.BufferSize];

                    await client.GetStream().ReadAsync(buffer, 0, buffer.Length).ContinueWith(
                        async (t) =>
                        {
                            int i = t.Result;
                            while (true)
                            {
                                await client.GetStream().WriteAsync(buffer, 0, i);
                                i = await client.GetStream().ReadAsync(buffer, 0, buffer.Length);
                            }
                        });
                }
            }
            else
            {
                Console.WriteLine("Server configuration is wrong");
            }
        }
    }
}
