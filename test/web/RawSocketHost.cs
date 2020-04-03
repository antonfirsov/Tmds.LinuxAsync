using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.Configuration;
using Tmds.LinuxAsync.Transport;
using Socket = Tmds.LinuxAsync.Socket;
using SocketAsyncEventArgs = Tmds.LinuxAsync.SocketAsyncEventArgs;

namespace web
{
    public class RawSocketHost
    {
        private const int BufferSize = 512;
        private const string Response =
            "HTTP/1.1 200 OK\r\nDate: Tue, 31 Mar 2020 14:49:06 GMT\r\nContent-Type: application/json\r\nServer: Kestrel\r\nContent-Length: 27\r\n\r\n{\"message\":\"Hello, World!\"}";

        private static ReadOnlySpan<byte> RequestEnd => new byte[] {13, 10, 13, 10}; // "\r\n\r\n"
        
        private readonly string[] _args;
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

            bool deferSends = _options.DeferSends == true;
            bool deferReceives = _options.DeferReceives == true;
            bool runContinuationsAsynchronously =
                _options.SocketContinuationScheduler == SocketContinuationScheduler.ThreadPool;

            using ClientConnectionHandler clientHandler = 
                new ClientConnectionHandler(deferSends, deferReceives, runContinuationsAsynchronously);
            
            while (true)
            {
                using Socket handlerSocket = await listener.AcceptAsync();

                clientHandler.HandleClient(handlerSocket);
            }
        }

        class ClientConnectionHandler : IDisposable
        {
            private Socket _socket;
            private readonly byte[] _sendBuffer = Encoding.ASCII.GetBytes(Response);
            private readonly byte[] _receiveBuffer = new byte[BufferSize];

            private readonly SocketAsyncEventArgs _receiveArgs;
            private readonly SocketAsyncEventArgs _sendArgs;
            private readonly ManualResetEventSlim _sendEvent = new ManualResetEventSlim(true);
            private readonly ManualResetEventSlim _receiveEvent = new ManualResetEventSlim();
            
            private int _pending;
            
            public ClientConnectionHandler(bool deferSends, bool deferReceives, bool runContinuationsAsynchronously)
            {
                _sendArgs = new SocketAsyncEventArgs()
                {
                    PreferSynchronousCompletion = !deferSends,
                    RunContinuationsAsynchronously = runContinuationsAsynchronously
                };
                _sendArgs.SetBuffer(_sendBuffer);
                _sendArgs.Completed += (s, a) => StopReceiving();

                _receiveArgs = new SocketAsyncEventArgs()
                {
                    PreferSynchronousCompletion = !deferReceives,
                    RunContinuationsAsynchronously = runContinuationsAsynchronously
                };
                _receiveArgs.SetBuffer(_receiveBuffer);
                _receiveArgs.Completed += (s,a) => CompleteReceive();
            }

            private void Reset(Socket socket)
            {
                _socket = socket;
                _pending = 1;
                _sendEvent.Set();
                _receiveEvent.Reset();
            }

            private void CompleteReceive()
            {
                int count = _receiveArgs.BytesTransferred;
                SocketError error = _receiveArgs.SocketError;

                if (error == SocketError.Success && count != 0)
                {
                    if (count > 4 && _receiveBuffer.AsSpan(count - 4, 4).SequenceEqual(RequestEnd))
                    {
                        try
                        {
                            _sendEvent.Wait();
                            _sendEvent.Reset();
                            if (!_socket.SendAsync(_sendArgs))
                            {
                                StopReceiving();
                            }
                        }
                        catch (SocketException)
                        {
                            StopReceiving();
                        }
                    }
                }
                else 
                {
                    StopReceiving();
                }
                
                _receiveEvent.Set();
            }

            private void StopReceiving()
            {
                Interlocked.Exchange(ref _pending, 0);
                _sendEvent.Set();
            }

            public void HandleClient(Socket socket)
            {
                Reset(socket);
                
                while (_pending > 0)
                {
                    try
                    {
                        _receiveEvent.Reset();
                        if (!_socket.ReceiveAsync(_receiveArgs))
                        {
                            CompleteReceive();
                        }

                        _receiveEvent.Wait();
                    }
                    catch (SocketException)
                    {
                        StopReceiving();
                    }
                }
            }

            public void Dispose()
            {
                _receiveArgs.Dispose();
                _sendArgs.Dispose();
                _sendEvent.Dispose();
            }
        }
    }
}