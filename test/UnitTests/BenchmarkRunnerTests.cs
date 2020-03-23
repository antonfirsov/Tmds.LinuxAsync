using System.Collections.Generic;
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
            };
            Options options = Options.Parse(stuff);
            
            Assert.Equal(stuff[1], options.BenchmarksDriverPath);
            Assert.Equal(stuff[3], options.Server);
            Assert.Equal("/json", options.Path);
        }

        [Fact]
        public void BenchmarkParameterSet_Parse()
        {
            var set = BenchmarkParameterSet.Parse("-e=epoll,iouring", "-t=3..6", "r=false", "x=0..1", "--server", "http://lololol");

            BenchmarkParameter e = set['e'];
            BenchmarkParameter t = set['t'];
            BenchmarkParameter r = set['r'];
            BenchmarkParameter x = set['x'];
            
            Assert.Equal(4, set.Count);
            Assert.Equal(new char[] { 'e', 't', 'r', 'x'}, set.Names);
            Assert.Equal('e', e.Name);
            Assert.Equal(new object[]{ "epoll", "iouring"}, e.Values);
            Assert.Equal('t', t.Name);
            Assert.Equal(new object[] { 3, 4, 5, 6}, t.Values);
            Assert.Equal('r', r.Name);
            Assert.Equal(new object[] { "false"}, r.Values);
            Assert.Equal('x', x.Name);
            Assert.Equal(new object[] { 0, 1}, x.Values);
        }

        [Fact]
        public void BenchmarkParameterSet_CartesianProduct()
        {
            var set = BenchmarkParameterSet.Parse("-e=epoll,iouring", "-t=3..5", "r=false", "x=0..1");

            IReadOnlyList<object>[] data = set.CartesianProduct().ToArray();
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
            var set = BenchmarkParameterSet.Parse("-e=epoll,iouring", "r=false,true", "-t=3..4");

            string[] lines = set.CartesianProduct().Select(l => set.GetBenchmarkRunnerParameterStringForLine(l)).ToArray();
            
            Assert.Contains("--arg \"-e=epoll\" --arg \"-r=false\" --arg \"-t=3\"", lines[0]);
            Assert.Contains("--arg \"-e=iouring\" --arg \"-r=true\" --arg \"-t=4\"", lines[7]);
        }

        private static void CheckRow(IReadOnlyList<object> data, int idx, params object[] expected)
        {
            Assert.Equal(expected, data[idx]);
        }
    }
}