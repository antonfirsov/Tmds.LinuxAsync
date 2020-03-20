using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Tmds.LinuxAsync;
using IoUring.Transport;
using System.IO.Pipelines;
using Tmds.LinuxAsync.Transport;
#if RELEASE
using Microsoft.Extensions.Logging;
#endif

namespace web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            (bool isSuccess, CommandLineOptions options) = ConsoleLineArgumentsParser.ParseArguments(args);

            if (isSuccess)
            {
                CreateHostBuilder(args, options).Build().Run();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args, CommandLineOptions commandLineOptions)
        {
            AsyncEngine.SocketEngine = CreateAsyncEngine(commandLineOptions);

            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    if (commandLineOptions.SocketEngine == SocketEngineType.IOUringTransport)
                    {
                        webBuilder.ConfigureServices(serviceCollection =>
                            serviceCollection.AddIoUringTransport(options =>
                            {
                                options.ThreadCount = commandLineOptions.ThreadCount;
                                options.ApplicationSchedulingMode = commandLineOptions.ApplicationCodeIsNonBlocking.Value ?
                                    PipeScheduler.Inline : PipeScheduler.ThreadPool;
                            }));
                    }
                    else if (commandLineOptions.SocketEngine == SocketEngineType.LinuxTransport)
                    {
                        webBuilder.UseLinuxTransport(options =>
                        {
                            options.ThreadCount = commandLineOptions.ThreadCount;
                            options.DeferSend = commandLineOptions.DeferSends.Value;
                            options.ApplicationSchedulingMode= commandLineOptions.ApplicationCodeIsNonBlocking.Value ?
                                    PipeScheduler.Inline : PipeScheduler.ThreadPool;
                        });
                    }
                    else
                    {
                        webBuilder.UseLinuxAsyncSockets(options =>
                            {
                                options.DispatchContinuations = commandLineOptions.DispatchContinuations.Value;
                                options.DeferSends = commandLineOptions.DeferSends.Value;
                                options.DeferReceives = commandLineOptions.DeferReceives.Value;
                                options.DontAllocateMemoryForIdleConnections = commandLineOptions.DontAllocateMemoryForIdleConnections.Value;
                                options.OutputWriterScheduler = commandLineOptions.OutputWriterScheduler;
                                options.ApplicationCodeIsNonBlocking = commandLineOptions.ApplicationCodeIsNonBlocking.Value;
                            }
                        );
                    }
                });
        }

        private static AsyncEngine CreateAsyncEngine(CommandLineOptions commandLineOptions)
        {
            bool batchOnIOThread = !commandLineOptions.DispatchContinuations.Value ||
                                          commandLineOptions.OutputWriterScheduler == OutputWriterScheduler.IOThread;
            switch (commandLineOptions.SocketEngine)
            {
                case SocketEngineType.EPoll:
                    return new EPollAsyncEngine(threadCount: commandLineOptions.ThreadCount,
                        useLinuxAio: commandLineOptions.UseAio.Value,
                        batchOnIOThread);
                case SocketEngineType.IOUring:
                    return new IOUringAsyncEngine(threadCount: commandLineOptions.ThreadCount,
                        batchOnIOThread);
                case SocketEngineType.IOUringTransport:
                case SocketEngineType.LinuxTransport:
                    // Create EPollAsyncEngine with threadCount of zero.
                    return new EPollAsyncEngine(threadCount: 0,
                        useLinuxAio: false,
                        batchOnIOThread);
                default:
                    throw new NotSupportedException($"{commandLineOptions.SocketEngine} is not supported");
            }
        }
    }
}
