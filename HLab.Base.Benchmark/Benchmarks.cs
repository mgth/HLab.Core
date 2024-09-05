using BenchmarkDotNet.Attributes;
using System;
using HLab.Base.Extensions;

namespace HLab.Base.Benchmark
{
    public class Benchmarks
    {
        readonly double[] _values = new double[1111];

        public Benchmarks()
        {
            var random = new Random();
            for (var i = 0; i < _values.Length; i++)
            {
                _values[i] = random.NextDouble();
            }
        }

        [Benchmark(Baseline = true)]
        public double StandardDeviation()
        {
            ReadOnlySpan<double> span = _values;
            return span.Cv1();
        }

        [Benchmark]
        public double StandardDeviationVector()
        {
            ReadOnlySpan<double> span = _values;
            return span.Cv();
        }
    }
}
