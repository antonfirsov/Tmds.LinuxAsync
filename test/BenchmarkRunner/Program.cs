#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;

namespace BenchmarkRunner
{
    public static class Utilities
    {
        public static void ParseIfInt(string line, string match, ref int value)
        {
            if (line.StartsWith(match))
            {
                var subString = line.AsSpan(match.Length).Trim();
                value = int.Parse(subString, NumberStyles.Integer | NumberStyles.AllowThousands);
            }
        }

        public static void ParseIfFloat(string line, string match, ref float value)
        {
            if (line.StartsWith(match))
            {
                value = float.Parse(line.AsSpan(match.Length).Trim(), NumberStyles.Float, CultureInfo.InvariantCulture);
            }
        }
    }
    
    class Program
    {
        static void Main(string[] args)
        {
            Options options = Options.Parse(args);
            BenchmarkParameterSet parameterSet = BenchmarkParameterSet.Parse(args);

            Console.WriteLine("Benchmarking with parameters:");
            Console.WriteLine(parameterSet);
            
            string commandPrefix = $"run --no-build -c Release -- --display-output --server \"{options.Server}\" --client \"{options.Client}\" --repository {options.Repository} --branch {options.Branch} --project-file \"{options.ProjectFile}\" --path \"{options.Path}\" --connections {options.Connections} --display-output";

            Console.WriteLine(commandPrefix);

            string csvFile = $"{options.OutCsv}_{DateTime.Now:MM-dd-yyyy__HH-mm-ss}.csv";
            Console.WriteLine($"Saving output to {csvFile}");

            using var csvWriter = new CsvWriter(csvFile);
            csvWriter.AppendRange(parameterSet.Names);
            csvWriter.Append("RPS");
            csvWriter.Append("CPU");
            csvWriter.Append("latency");
            csvWriter.EndLine();

            foreach (IReadOnlyList<object> paramLine in parameterSet.CartesianProduct())
            {
                csvWriter.AppendRange(paramLine);
                string commandSuffix = parameterSet.GetBenchmarkRunnerParameterStringForLine(paramLine);

                var startInfo = new ProcessStartInfo()
                {
                    FileName = "dotnet",
                    Arguments = $"{commandPrefix} {commandSuffix}",
                    WorkingDirectory = options.BenchmarksDriverPath,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                Console.WriteLine(startInfo.Arguments);

                int rps = default;
                int maxCpu = default;
                float latency = default;

                using Process process = Process.Start(startInfo);

                StreamReader stdOut = process.StandardOutput;
                for (string l = stdOut.ReadLine(); l != null; l = stdOut.ReadLine())
                {
                    if (!string.IsNullOrEmpty(l) && !l.StartsWith('[') && !l.StartsWith("failed"))
                    {
                        Console.WriteLine(l);
                        Utilities.ParseIfInt(l, "RequestsPerSecond:", ref rps);
                        Utilities.ParseIfInt(l, "Max CPU (%):", ref maxCpu);
                        Utilities.ParseIfFloat(l, "Avg. Latency (ms):", ref latency);
                    }
                }

                process.WaitForExit();

                csvWriter.Append(rps);
                csvWriter.Append(maxCpu);
                csvWriter.Append(latency);
                csvWriter.EndLine();
            }
        }
    }
}
