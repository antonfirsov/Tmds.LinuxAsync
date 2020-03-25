﻿using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace BenchmarkRunner
{
    public class BenchmarkRunnerTests
    {
        [Fact]
        public void ParseOptions()
        {
            string[] stuff = {
                "--benchmarks-driver", @"C:\uff\puff",
                "--server", "http://lol-foo:5001",
                "-p", "a=42"
            };
            Options options = Options.Parse(stuff);
            
            Assert.Equal(stuff[1], options.BenchmarksDriverPath);
            Assert.Equal(stuff[3], options.Server);
            Assert.Equal("/json", options.Path);
            Assert.Equal("a=42", options.Parameters);
        }

        [Fact]
        public void BenchmarkParameterSet_Parse()
        {
            var set = BenchmarkParameterSet.Parse("-e=epoll,iouring -t=3..6 r=false x=0..1 --server http://lololol", "COMPlus_Rofl=5");

            BenchmarkParameter e = set["e"];
            BenchmarkParameter t = set["t"];
            BenchmarkParameter r = set["r"];
            BenchmarkParameter x = set["x"];
            BenchmarkParameter rofl = set["COMPlus_Rofl"];
            
            Assert.Equal(5, set.Count);
            Assert.Equal(new[] { "COMPlus_Rofl", "e", "t", "r", "x"}, set.Names);
            
            Assert.Equal(BenchmarkParameterType.Argument, e.Type);
            Assert.Equal("e", e.Name);
            Assert.Equal(new object[]{ "epoll", "iouring"}, e.Values);
            
            Assert.Equal(BenchmarkParameterType.Argument, t.Type);
            Assert.Equal("t", t.Name);
            Assert.Equal(new object[] { 3, 4, 5, 6}, t.Values);
            
            Assert.Equal("r", r.Name);
            Assert.Equal(new object[] { "false"}, r.Values);
            Assert.Equal("x", x.Name);
            Assert.Equal(new object[] { 0, 1}, x.Values);
            
            Assert.Equal(BenchmarkParameterType.EnvironmentVariable, rofl.Type);
            Assert.Equal("COMPlus_Rofl", rofl.Name);
            Assert.Equal(new object[] { "5" }, rofl.Values);
        }

        [Fact]
        public void BenchmarkParameterSet_CartesianProduct()
        {
            var set = BenchmarkParameterSet.Parse("-e=epoll,iouring -t=3..5 r=false x=0..1", "");

            IReadOnlyList<BenchmarkParameterAssignment>[] data = set.CartesianProduct().ToArray();
            // 2 * 4 * 1 * 2
            Assert.Equal(2 * 3 * 1 * 2, data.Length);
            
            CheckRow(data, 0, "epoll", 3, "false", 0);
            CheckRow(data, 1, "epoll", 3, "false", 1);
            CheckRow(data, 2, "epoll", 4, "false", 0);
            CheckRow(data, 3, "epoll", 4, "false", 1);
            CheckRow(data, 4, "epoll", 5, "false", 0);
            CheckRow(data, 5, "epoll", 5, "false", 1);
            
            CheckRow(data, 6, "iouring", 3, "false", 0);
            CheckRow(data, 7, "iouring", 3, "false", 1);
            CheckRow(data, 8, "iouring", 4, "false", 0);
            CheckRow(data, 9, "iouring", 4, "false", 1);
            CheckRow(data, 10, "iouring", 5, "false", 0);
            CheckRow(data, 11, "iouring", 5, "false", 1);
        }

        [Fact]
        public void BenchmarkParameterSet_GetBenchmarkRunnerParameterStringForLine()
        {
            var set = BenchmarkParameterSet.Parse("-e=epoll,iouring r=false,true -t=3..4", "");

            string[] lines = set.CartesianProduct().Select(l => l.GetBenchmarkRunnerParameterString()).ToArray();
            
            Assert.Contains("--arg \"-e=epoll\" --arg \"-r=false\" --arg \"-t=3\"", lines[0]);
            Assert.Contains("--arg \"-e=iouring\" --arg \"-r=true\" --arg \"-t=4\"", lines[7]);
        }

        [Theory]
        [InlineData("RequestsPerSecond:           341,312", "RequestsPerSecond:", 341312)]
        [InlineData("Max CPU (%):                 86", "Max CPU (%):", 86)]
        public void Utilities_ParseIfInt(string str, string match, int expected)
        {
            int result = 0;
            BenchmarkRunnerUtilities.ParseIfInt(str, match, ref result);
            Assert.Equal(expected, result);
        }
        
        [Theory]
        [InlineData("Avg. Latency (ms):           0.83", "Avg. Latency (ms):", 0.83)]
        public void Utilities_ParseIfFloat(string str, string match, float expected)
        {
            float result = 0;
            BenchmarkRunnerUtilities.ParseIfFloat(str, match, ref result);
            Assert.Equal(expected, result);
        }

        private static void CheckRow(IReadOnlyList<BenchmarkParameterAssignment>[] data, int idx, params object[] expected)
        {
            Assert.Equal(expected, data[idx].Select(a => a.Value).ToArray());
        }
    }
}