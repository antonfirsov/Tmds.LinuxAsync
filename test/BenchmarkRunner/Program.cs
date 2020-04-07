#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace BenchmarkRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            Options options = Options.Parse(args);
            BenchmarkParameterSet parameterSet = BenchmarkParameterSet.Parse(options.Parameters, options.EnvironmentVariables);

            Console.WriteLine("Benchmarking with parameters:");
            Console.WriteLine(parameterSet);
            
            string commandPrefix = $"run --no-build -c Release -- --display-output --server \"{options.Server}\" --client \"{options.Client}\" " +
                                   $"--repository {options.Repository} --branch {options.Branch} --project-file \"{options.ProjectFile}\" " +
                                   $"--warmup {options.Warmup} --duration {options.Duration} --path \"{options.Path}\" --connections {options.Connections} --display-output";

            if (options.KestrelThreadCount > 0)
            {
                commandPrefix += $" --kestrelThreadCount {options.KestrelThreadCount}";
            }

            if (options.SelfContained)
            {
                commandPrefix += $" --self-contained";
            }

            if (options.RawSocket)
            {
                commandPrefix += " --raw-socket";
            }
            
            string csvFile = $"{options.OutCsv}_{DateTime.Now:MM-dd-yyyy__HH-mm-ss}.csv";
            Console.WriteLine($"Saving output to {csvFile}");

            using var csvWriter = new CsvWriter(csvFile);
            bool includeConstants = options.IncludeConstants.HasValue && options.IncludeConstants.Value;
            csvWriter.AppendRange(includeConstants ? parameterSet.Names : parameterSet.VariableNames);


            csvWriter.Append("RPS");
            csvWriter.Append("CPU");
            csvWriter.Append("latency");
            csvWriter.EndLine();

            foreach (IReadOnlyList<BenchmarkParameterAssignment> paramLine in parameterSet.CartesianProduct())
            {
                IEnumerable<BenchmarkParameterAssignment> parameters = includeConstants
                    ? paramLine
                    : paramLine.Where(a => a.IsVariable);
                csvWriter.AppendRange(parameters.Select(p => p.Value));

                string commandSuffix = paramLine.GetBenchmarkRunnerParameterString();

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

                bool success = false;
                for (int retryCount = 0; !success && retryCount < 20; retryCount++)
                {
                    using Process process = Process.Start(startInfo);

                    StreamReader stdOut = process.StandardOutput;

                    bool roundSucceeded = true;
                    for (string l = stdOut.ReadLine(); l != null; l = stdOut.ReadLine())
                    {
                        if (!string.IsNullOrEmpty(l) && !l.StartsWith('[') && !l.StartsWith("failed"))
                        {
                            Console.WriteLine(l);
                            if (l.Contains("is invalid or not responsive", StringComparison.OrdinalIgnoreCase))
                            {
                                Console.WriteLine($"**** RETRY the run! ({retryCount})");
                                roundSucceeded = false;
                                break;
                            }
                            BenchmarkRunnerUtilities.ParseIfInt(l, "RequestsPerSecond:", ref rps);
                            BenchmarkRunnerUtilities.ParseIfInt(l, "Max CPU (%):", ref maxCpu);
                            BenchmarkRunnerUtilities.ParseIfFloat(l, "Avg. Latency (ms):", ref latency);
                        }
                    }

                    success = roundSucceeded;
                    if (success)
                    {
                        process.WaitForExit();
                        csvWriter.Append(rps);
                        csvWriter.Append(maxCpu);
                        csvWriter.Append(latency);
                        csvWriter.EndLine();
                    }
                    else 
                    {
                        process.Kill();
                    }
                }
            }
        }
    }
}
