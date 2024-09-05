using System;
using System.Numerics;
using System.Runtime.Intrinsics;

namespace HLab.Base.Extensions;

public static class MathExtensions
{
    public static double Average(this ReadOnlySpan<double> values)
    {    
        return values.Sum() / values.Length;
    }

    public static double Sum(this ReadOnlySpan<double> values)
    {
        double sum = 0;
        foreach (var v in values)
            sum += v;

        return sum;
    }

    public static double Sum(this Vector<double> values)
    {
        double sum = 0;
        for (var i=0; i<Vector<double>.Count; i++)
            sum += values[i];

        return sum;
    }

    public static (double sd, double average) StandardDeviation(this ReadOnlySpan<double> values)
    {
        var average = values.Average();
        var averageVector = new Vector<double>(average);// * a;
        var sumVector = Vector<double>.Zero;
        var n = values.Length;
        var nVector = n - n % Vector<double>.Count;

        var i = 0;
        for (; i < nVector; i += Vector<double>.Count)
        {
            var v = new Vector<double>(values[i..]);

            var delta = v - averageVector;
            var square = delta * delta;

            sumVector += square;
        }

        var sum = sumVector.Sum();

        for (; i < n; i ++)
        {
            var v = values[i];

            var delta = v - average;
            var square = delta * delta;

            sum += square;
        }

        return (Math.Sqrt(sum/(n-1)), average);

    }

    public static (double, double) StandardDeviationV1(this ReadOnlySpan<double> values)
    {
        var average = values.Average();
        var sum = 0.0;
        var n = values.Length;

        foreach (var value in values)
        {
            var delta = value - average;
            sum += delta * delta;
        }

        return (Math.Sqrt(sum/(n-1)), average);

    }

    public static double Cv1(this ReadOnlySpan<double> values)
    {
        var(sd,average) = values.StandardDeviationV1();
        return 100.0 * sd / average;
    }
    public static double Cv(this ReadOnlySpan<double> values)
    {
        var(sd,average) = values.StandardDeviation();
        return 100.0 * sd / average;
    }
}