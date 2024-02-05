using BenchmarkDotNet.Running;

namespace ObjectMetaDataTagging.Benchmark
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<BenchmarkTests>();
        }
    }
}