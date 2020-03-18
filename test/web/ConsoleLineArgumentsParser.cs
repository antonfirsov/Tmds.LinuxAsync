﻿using CommandLine;
using System;
using System.Collections.Generic;
using System.IO;
using Tmds.LinuxAsync.Transport;
using System.Text;

namespace web
{
    public enum SocketEngineType
    {
        EPoll,
        IOUring,
        IOUringTransport,
        LinuxTransport
    }

    public class CommandLineOptions
    {
        // the booleans MUST be nullable, otherwise --arg false does not work...
        // see https://github.com/commandlineparser/commandline/issues/290 for more details

        [Option('e', "engine", Required = false, Default = SocketEngineType.IOUring, HelpText = "EPoll/IOUring/IOUringTransport/LinuxTransport")]
        public SocketEngineType SocketEngine { get; set; }

        [Option('t', "thread-count", Required = false, Default = 1, HelpText = "Thread Count, default value is 1")]
        public int ThreadCount { get; set; }

        [Option('a', "aio", Required = false, Default = true, HelpText = "Use Linux AIO")]
        public bool? UseAio { get; set; }

        [Option('c', "dispatch-continuations", Required = false, Default = true, HelpText = "Dispatch Continuations")]
        public bool? DispatchContinuations { get; set; }

        [Option('s', "defer-sends", Required = false, Default = false, HelpText = "Defer Sends")]
        public bool? DeferSends { get; set; }

        [Option('r', "defer-receives", Required = false, Default = false, HelpText = "Defer Receives")]
        public bool? DeferReceives { get; set; }

        [Option('w', "wait-for-ready", Required = false, Default = true, HelpText = "Don't allocate memory for idle connections")]
        public bool? DontAllocateMemoryForIdleConnections { get; set; }

        [Option('o', "output-writer-scheduler", Required = false, Default = OutputWriterScheduler.IOQueue, HelpText = "IOQueue/Inline/IOThread")]
        public OutputWriterScheduler OutputWriterScheduler { get; set; }

        [Option('i', "inline-app", Required = false, Default = false, HelpText = "Application code is non blocking")]
        public bool? ApplicationCodeIsNonBlocking { get; set; }

        public override string ToString()
        {
            StringBuilder bld = new StringBuilder();
            bld.Append($"-e {SocketEngine} -t {ThreadCount}");
            AppendValOf(bld, 'a', UseAio);
            AppendValOf(bld, 'c', DispatchContinuations);
            AppendValOf(bld, 's', DeferSends);
            AppendValOf(bld, 'r', DeferReceives);
            AppendValOf(bld, 'w', DontAllocateMemoryForIdleConnections);
            AppendValOf(bld, 'p', CoalesceWrites);
            AppendValOf(bld, 'i', ApplicationCodeIsNonBlocking);
            return bld.ToString();
        }

        private static void AppendValOf(StringBuilder bld, char opName, bool? b)
        {
            if (b.HasValue)
            {
                bld.Append($" -{opName} {b.Value}");
            }
        }
    }

    public static class ConsoleLineArgumentsParser
    {
        public static (bool isSuccess, CommandLineOptions commandLineOptions) ParseArguments(string[] args)
        {
            bool isSuccess = false;
            CommandLineOptions commandLineOptions = null;

            using (var parser = CreateParser())
            {
                parser
                    .ParseArguments<CommandLineOptions>(Workaround(args))
                    .WithParsed(options => { commandLineOptions = options; isSuccess = true; })
                    .WithNotParsed(_ => isSuccess = false);
            }

            return (isSuccess, commandLineOptions);
        }

        // CommandLineParsers does not handle --engine=epoll properly, so we need this ugly workaround..
        private static IEnumerable<string> Workaround(string[] args)
        {
            foreach (var arg in args)
            {
                int index = arg.IndexOf('=', StringComparison.Ordinal);
                if (index > 0)
                {
                    yield return arg.Substring(0, index);
                    yield return arg.Substring(index + 1);
                }
                else
                {
                    yield return arg;
                }
            }
        }

        private static Parser CreateParser()
            => new Parser(settings =>
            {
                settings.CaseInsensitiveEnumValues = true;
                settings.CaseSensitive = false;
                settings.EnableDashDash = true;
                settings.HelpWriter = Console.Out;
                settings.IgnoreUnknownArguments = true; // for args that we pass to Host.CreateDefaultBuilder()
                settings.MaximumDisplayWidth = GetMaximumDisplayWidth();
            });

        private static int GetMaximumDisplayWidth()
        {
            const int MinimumDisplayWidth = 80;

            try
            {
                return Math.Max(MinimumDisplayWidth, Console.WindowWidth);
            }
            catch (IOException)
            {
                return MinimumDisplayWidth;
            }
        }
    }
}
