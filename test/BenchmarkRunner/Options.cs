using System;
using CommandLine;

namespace BenchmarkRunner
{
    public class Options
    {
        [Option( "benchmarks-driver", Required = false, Default = null, HelpText = "Path of BenchmarksDriver")]
        public string? BenchmarksDriverPath { get; set; }
        
        [Option( "server", Required = false, Default = "http://asp-perf-lin:5001", HelpText = "Server address")]
        public string Server { get; set; }
        
        [Option( "client", Required = false, Default = "http://asp-perf-load:5002", HelpText = "Client address")]
        public string Client { get; set; }
        
        [Option("path", Required = false, Default = "/json", HelpText = "Path for benchmark")]
        public string Path { get; set; }
        
        [Option('p', "parameters", Required = true, HelpText = "The parameter set string")]
        public string Parameters { get; set; }
     
        private static Parser CreateParser()
            => new Parser(settings =>
            {
                settings.CaseInsensitiveEnumValues = true;
                settings.CaseSensitive = false;
                settings.EnableDashDash = true;
                settings.HelpWriter = Console.Out;
                settings.IgnoreUnknownArguments = true; // for args that we pass to Host.CreateDefaultBuilder()
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