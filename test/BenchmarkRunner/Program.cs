#nullable enable
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace BenchmarkRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            Options options = Options.Parse(args);
            BenchmarkParameterSet parameterSet = BenchmarkParameterSet.Parse(args);

            Console.WriteLine("Benchmarking with parameters:");
            Console.WriteLine(parameterSet);

            var processStartInfo = new ProcessStartInfo()
            {
                FileName = "dotnet",
                //Arguments = $"{commandPrefix} {commandSuffix}",
                WorkingDirectory = options.BenchmarksDriverPath,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
        }
    }
}
