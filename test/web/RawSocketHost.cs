using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

using Socket = Tmds.LinuxAsync.Socket;

namespace web
{
    public class RawSocketHost
    {
        private const int BufferSize = 512;
        private const string Response =
            "HTTP/1.1 200 OK\r\nDate: Tue, 31 Mar 2020 14:49:06 GMT\r\nContent-Type: application/json\r\nServer: Kestrel\r\nContent-Length: 27\r\n\r\n{\"message\":\"Hello, World!\"}";

        private static ReadOnlySpan<byte> RequestEnd => new byte[] {13, 10, 13, 10}; // "\r\n\r\n"
        
        private string[] _args;
        private readonly CommandLineOptions _options;

        private IPEndPoint _serverEndpoint;

        public RawSocketHost(CommandLineOptions options, string[] args)
        {
            _options = options;
            _args = args;
            
            ConfigurationBuilder bld = new ConfigurationBuilder();
            bld.AddCommandLine(_args);
            var cfg = bld.Build();
            string url = cfg["urls"];

            string[] data = url.Substring(7).Split(':');
            IPAddress ip = IPAddress.Parse(data[0]);
            int port = int.Parse(data[1]);

            _serverEndpoint = new IPEndPoint(ip, port);
        }

        public void Run()
        {
            RunAsync().GetAwaiter().GetResult();
        }

        public async Task RunAsync()
        {
            
            using Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(_serverEndpoint);
            listener.Listen(1024);
            Console.WriteLine($"Raw server listening on {_serverEndpoint}");
            
            byte[] receiveBuffer = new byte[BufferSize];
            byte[] sendBuffer = Encoding.ASCII.GetBytes(Response);

            while (true)
            {
                Socket handler = await listener.AcceptAsync();

                while (true)
                {
                    try
                    {
                        int count = await handler.ReceiveAsync(receiveBuffer, default);

                        if (count > 4 && receiveBuffer.AsSpan(count - 4, 4).SequenceEqual(RequestEnd))
                        {
                            await handler.SendAsync(sendBuffer, default);
                        }

                        if (count == 0)
                        {
                            break;
                        }
                    }
                    catch (SocketException)
                    {
                        break;
                    }
                }
            }
        }
    }
}