using System.Runtime.Intrinsics;

namespace HLab.Geo.Simd;

public readonly partial struct Size(Vector128<double> v) : IFormattable
{
   public static bool operator ==(Size size1, Size size2) => size1.V == size2.V;
   public static bool operator !=(Size size1, Size size2) => !(size1 == size2);

   public static bool Equals(Size size1, Size size2) => size1.IsEmpty ? size2.IsEmpty : size1.V.Equals(size2.V);
   public override bool Equals(object? o) => o is Size value && Equals(this, value);
   public bool Equals(Size value) => Equals(this, value);

   public override int GetHashCode() => IsEmpty ? 0 : V.GetHashCode();

   public override string ToString() => ConvertToString(null /* format string */, null /* format provider */);
   public string ToString(IFormatProvider provider) => ConvertToString(null /* format string */, provider);
   string IFormattable.ToString(string format, IFormatProvider provider) => ConvertToString(format, provider);
   internal string ConvertToString(string format, IFormatProvider provider)
   {
      if (IsEmpty){return "Empty";}

      // Helper to get the numeric list separator for a given culture.
      char separator = ',';//MS.Internal.TokenizerHelper.GetNumericListSeparator(provider);
      return String.Format(provider,
                           "{1:" + format + "}{0}{2:" + format + "}",
                           separator,
                           Width,
                           Height);
   }

   internal Vector128<double> V => v;

   public Size(double width, double height):this(Vector128.Create(width, height))
   {
      if (width < 0 || height < 0)
      {
         throw new ArgumentException("SR.Size_WidthAndHeightCannotBeNegative");
      }
   }

   public static Size Empty { get; } = CreateEmptySize();

   public bool IsEmpty => v[0] < 0;

   public double Width => v[0];
   public double Height => v[1];

   public static explicit operator Vector(Size size) => new(size.V);
   public static explicit operator Point(Size size) => new(size.V);
   public static explicit operator Size(Point point) => new(point.V);
   public static explicit operator Size(Vector vector) => new(vector.V);

   static Size CreateEmptySize()
   {
      var size = new Size(double.NegativeInfinity,double.NegativeInfinity);
      return size;
   }
}