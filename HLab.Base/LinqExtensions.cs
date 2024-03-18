using System;
using System.Collections.Generic;

namespace HLab.Base;

public static class LinqExtensions
{
    public static (int min, int max) GetMinMax<T>(this IEnumerable<T> @this, Func<T,int> getter)
    {
        using var current = @this.GetEnumerator();
        if(!current.MoveNext()) return (0,0);
        var value = getter(current.Current);
        var min = value;
        var max = value;
        while (current.MoveNext())
        {
            value = getter(current.Current);
            if (value > max) max = value;
            else if (value < min) min = value;
        }
        return (min, max);
    }
}