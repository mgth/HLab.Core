using System.Data;
using System.Runtime.Intrinsics;

namespace HLab.Geo.Simd;
/// <summary>
/// Point - Defaults to 0,0
/// </summary>
/// 
[Serializable]
public readonly partial struct Point(Vector128<double> v) : IFormattable
{
   public static Point Undefined {get; } = new(double.NaN, double.NaN);
   public static Point Zero {get; } = new(0, 0);

   public bool IsEmpty => X == 0 && Y == 0;
   public bool IsUndefined => double.IsNaN(X) || double.IsNaN(Y);
   public Point(double x, double y):this( Vector128.Create(x,y)) { }
   public Point Offset(double offsetX, double offsetY) => new(X + offsetX, Y + offsetY);

   public static Point Min(Point point1, Point point2) => new(Vector128.Min(point1.V,point2.V));
   public static Point Max(Point point1, Point point2) => new(Vector128.Max(point1.V,point2.V));

   public static Point operator +(Point point, Vector vector) => new(point.V + vector.V);
   public static Point operator +(Point point1, Point point2) => new(point1.V + point2.V);

   public Vector ComponentMultiply(Vector vector) => new(V * vector.V);

   public static Point Add(Point point, Vector vector) => new(point.V + vector.V);
   public static Point Subtract(Point point, Vector vector) => new(point.V - vector.V);
   public static Vector Subtract(Point point1, Point point2) => new(point1.V - point2.V);

   public static Point operator -(Point point, Vector vector) => Subtract(point, vector);
   public static Vector operator -(Point point1, Point point2) => Subtract(point1, point2);

   public static Point Multiply(Point point, Matrix matrix) => matrix.Transform(point);
   public static Point operator *(Point point, Matrix matrix) => Multiply(point, matrix);

   public static explicit operator Size(Point point) => new(Math.Abs(point.X), Math.Abs(point.Y));

   public static explicit operator Vector(Point point) => new(point.V);

   // ReSharper disable CompareOfFloatsByEqualityOperator
   public static bool operator ==(Point point1, Point point2) => point1.V == point2.V;
   public static bool operator !=(Point point1, Point point2) => !(point1 == point2);

   public static bool Equals(Point point1, Point point2) => point1.V.Equals(point2.V);
   public override bool Equals(object? o) => o is Point value && Equals(this, value);
   public bool Equals(Point value) => Equals(this, value);

   public override int GetHashCode() => V.GetHashCode();

   public double X => v[0];
   public double Y => v[1];
   public Vector128<double> V => v;

   public override string ToString() => ConvertToString(null /* format string */, null /* format provider */);
   public string ToString(IFormatProvider provider) => ConvertToString(null /* format string */, provider);
   string IFormattable.ToString(string format, IFormatProvider provider) => ConvertToString(format, provider);

   internal string ConvertToString(string format, IFormatProvider provider)
   {
      // Helper to get the numeric list separator for a given culture.
      char separator = ',';//MS.Internal.TokenizerHelper.GetNumericListSeparator(provider);
      return String.Format(provider,
          "{1:" + format + "}{0}{2:" + format + "}",
          separator,
          X,
          Y);
   }

   public Point WithX(double x) => new Point(x, Y);
   public Point WithY(double y) => new Point(X, y);
}