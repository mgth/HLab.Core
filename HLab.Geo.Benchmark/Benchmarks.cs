using BenchmarkDotNet.Attributes;

namespace HLab.Geo.Benchmark;

public class Benchmarks
{
   int n = 10000;
   Vector a = new Vector(1, 1);
   Vector b = new Vector(2, 2);
   Simd.Vector a_ = new Simd.Vector(1, 1);
   Simd.Vector b_ = new Simd.Vector(2, 2);

   //[Benchmark(Baseline = true)]
   public Vector Add()
   {
      for (var i = 0; i < n; i++)
         a = Vector.Add(a, b);
      return a;
   }

   //[Benchmark]
   public Simd.Vector AddSimd()
   {
      for(var i = 0; i < n; i++)
         a_ = Simd.Vector.Add(a_, b_);
      return a_;
   }

   [Benchmark(Baseline = true)]
   public Vector Normalize()
   {
      return a.Normalize();
   }

   [Benchmark]
   public Simd.Vector NormalizeSimd()
   {
      return a_.Normalize();
   }

   //[Benchmark(Baseline = true)]
   public Vector Mult()
   {
      Vector s = new Vector(3.2,2.3);
      //for (var i = 0; i < n; i++)
         s += Vector.ComponentMultiply(a, b);
      return s;
   }

   //[Benchmark]
   public Simd.Vector MultSimd()
   {
      Simd.Vector s = new Simd.Vector(3.2,2.3);
      //for (var i = 0; i < n; i++)
         s += Simd.Vector.ComponentMultiply(a_, b_);
      return s;
   }
}