using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

using _Socket = Tmds.LinuxAsync.Socket;

namespace web
{
    public class RawSocketHost
    {
        private const int BufferSize = 512;
        
        private string[] _args;
        private readonly CommandLineOptions _options;

        public RawSocketHost(CommandLineOptions options, string[] args)
        {
            _options = options;
            _args = args;
        }

        public void Run()
        {
            RunAsync().GetAwaiter().GetResult();
        }

        public async Task RunAsync()
        {
            ConfigurationBuilder bld = new ConfigurationBuilder();
            bld.AddCommandLine(_args);
            var cfg = bld.Build();
            string url = cfg["urls"];

            string[] data = url.Substring(7).Split(':');
            IPAddress ip = IPAddress.Parse(data[0]);
            int port = int.Parse(data[1]);

            IPEndPoint serverEndpoint = new IPEndPoint(ip, port);
            using _Socket listener = new _Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(serverEndpoint);
            listener.Listen(1024);
            
            byte[] receiveBuffer = new byte[BufferSize];
            byte[] sendBuffer = new byte[BufferSize];

            while (true)
            {
                _Socket handler = await listener.AcceptAsync();
                int count = handler.ReceiveAsync()
            }
        }

        class ConnectionHandler
        {
            
        }
    }
}