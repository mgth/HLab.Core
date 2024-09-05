using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using System.Runtime.Intrinsics;

namespace HLab.Base.Benchmark
{
    public class Program
    {
        public static void Main(string[] args)
        {

            
            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, new DebugInProcessConfig());
            return;

            // If arguments are available use BenchmarkSwitcher to run benchmarks
            if (args.Length > 0)
            {
                var summaries = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly)
                    .Run(args,BenchmarkConfig.Get());
                return;
            }
            // Else, use BenchmarkRunner
            var summary = BenchmarkRunner.Run<Benchmarks>(BenchmarkConfig.Get());
        }
    }
}