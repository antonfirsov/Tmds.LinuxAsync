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
                "-p",
                "\"-t=1..5 -e=epoll,iouring\""
            };
            Options options = Options.Parse(stuff);
            
            Assert.Equal(stuff[1], options.BenchmarksDriverPath);
            Assert.Equal(stuff[3], options.Server);
            Assert.Equal("/json", options.Path);
            Assert.Equal("-t=1..5 -e=epoll,iouring", options.Parameters);
        }
    }
}