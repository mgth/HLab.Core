using System.Numerics;

namespace HLab.ColorTools;

public static partial class HLabColors
{
    public static ColorRGB<T> RGB<T>(T alpha, T red, T green, T blue) where T : INumber<T> => ColorRGB<T>.FromArgb(alpha, red, green, blue);
    public static ColorRGB<T> RGB<T>(T red, T green, T blue) where T : INumber<T> => ColorRGB<T>.FromArgb(ColorConst<T>.N, red, green, blue);
}

public readonly struct ColorRGB<T> : IColor<T> where T : INumber<T>
{
   ColorRGB(T alpha, T red, T green, T blue)
   {
      Alpha = alpha;
      Red = red;
      Green = green;
      Blue = blue;
   }

   internal static ColorRGB<T> FromArgb(T alpha, T red, T green, T blue) => new(alpha, red, green, blue);

   public T Alpha { get; }
   public T Red { get; }
   public T Green { get; }
   public T Blue { get; }

   public ColorRGB<T> ToRGB() => this;

   public uint ToUInt()
   {
      var a = ColorConst<byte>.Normalize(Alpha);
      var r = ColorConst<byte>.Normalize(Red);
      var g = ColorConst<byte>.Normalize(Green);
      var b = ColorConst<byte>.Normalize(Blue);
      return (uint)(
         (a << 24) |
         (r << 16) |
         (g << 8) |
         b
      );
   }

    public static ColorRGB<T> From<TFrom>(ColorRGB<TFrom> c)
        where TFrom : INumber<TFrom>
        => new(
            ColorConst<T>.Normalize(c.Alpha),
            ColorConst<T>.Normalize(c.Red),
            ColorConst<T>.Normalize(c.Green),
            ColorConst<T>.Normalize(c.Blue)
        );

    public ColorRGB<TTo> To<TTo>()
        where TTo : INumber<TTo>
        => ColorRGB<TTo>.From(this);

    IColor<TTo> IColor<T>.To<TTo>() => To<TTo>();
}