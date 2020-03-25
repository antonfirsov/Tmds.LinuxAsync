using System;
using CommandLine;

namespace BenchmarkRunner
{
    public class Options
    {
        [Option( 'd', "benchmarks-driver", Required = true, HelpText = "Path of BenchmarksDriver")]
        public string BenchmarksDriverPath { get; set; }
        
        [Option( "out-csv", Required = false, Default = "./result", HelpText = "Prefix / path of the output CSV file")]
        public string OutCsv { get; set; }
        
        [Option( "include-constants", Required = false, Default = false, HelpText = "true/false")]
        public bool? IncludeConstants { get; set; }
        
        [Option( "server", Required = false, Default = "http://asp-perf-lin:5001", HelpText = "Server address")]
        public string Server { get; set; }
        
        [Option( "client", Required = false, Default = "http://asp-perf-load:5002", HelpText = "Client address")]
        public string Client { get; set; }
        
        [Option( "repository", Required = false, Default ="https://github.com/tmds/Tmds.LinuxAsync.git")]
        public string Repository { get; set; }
        
        [Option( "branch", Required = false, Default ="master")]
        public string Branch { get; set; }
        
        [Option( "project-file", Required = false, Default ="test/web/web.csproj")]
        public string ProjectFile { get; set; }
        
        [Option( "connections", Required = false, Default = 256)]
        public int Connections { get; set; }
        
        [Option( "warmup", Required = false, Default = 15)]
        public int Warmup { get; set; }
        
        [Option( "duration", Required = false, Default = 15)]
        public int Duration { get; set; }
        
        [Option("path", Required = false, Default = "/json", HelpText = "Path for benchmark")]
        public string Path { get; set; }
        
        [Option("kestrelThreadCount", Required = false, Default = 0)]
        public int KestrelThreadCount { get; set; }
        
        [Option('p', "parameters", Required = true, HelpText = "Whitespace separated list of parameter definitions. Eg. \"e=epoll,iouring c=true,false t=4..8\"")]
        public string Parameters { get; set; }
        
        [Option('e', "env", Required = false, HelpText = "Whitespace separated definition of environment variable definitions. Eg. \"COMPlus_ThreadPool_UnfairSemaphoreSpinLimit=0,1 COMPlus_HillClimbing_Disable=0,1\"")]
        public string EnvironmentVariables { get; set; }
        
        private static Parser CreateParser()
            => new Parser(settings =>
            {
                settings.CaseInsensitiveEnumValues = true;
                settings.CaseSensitive = false;
                settings.EnableDashDash = true;
                settings.HelpWriter = Console.Out;
                settings.IgnoreUnknownArguments = true;
            });

        public static Options Parse(string[] args)
        {
            using Parser parser = CreateParser();
            Options options = null;
            parser.ParseArguments<Options>(args).WithParsed(r => options = r).WithNotParsed(e =>
            {
                Console.WriteLine(e.ToString());
            });
            return options;
        }
    }
}