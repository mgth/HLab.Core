namespace HLab.Geo;
/// <summary>
/// Point - Defaults to 0,0
/// </summary>
/// 
[Serializable]
public readonly partial struct Point(double x, double y) : IFormattable
{
   public Point Offset(double offsetX, double offsetY) => new(X + offsetX, Y + offsetY);

   public static Point operator +(Point point, Vector vector) => new(point.X + vector.X, point.Y + vector.Y);
   public static Point operator +(Point point, Point vector) => new(point.X + vector.X, point.Y + vector.Y);

   public Vector ComponentMultiply(Vector vector) => new(X * vector.X, Y * vector.Y);

   public static Point Add(Point point, Vector vector) => new(point.X + vector.X, point.Y + vector.Y);
   public static Point Subtract(Point point, Vector vector) => new(point.X - vector.X, point.Y - vector.Y);
   public static Vector Subtract(Point point1, Point point2) => new(point1.X - point2.X, point1.Y - point2.Y);

   public static Point operator -(Point point, Vector vector) => Subtract(point, vector);
   public static Vector operator -(Point point1, Point point2) => Subtract(point1, point2);

   public static Point Multiply(Point point, Matrix matrix) => matrix.Transform(point);
   public static Point operator *(Point point, Matrix matrix) => Multiply(point, matrix);

   public static explicit operator Size(Point point) => new(Math.Abs(point.X), Math.Abs(point.Y));

   public static explicit operator Vector(Point point) => new(point.X, point.Y);

   // ReSharper disable CompareOfFloatsByEqualityOperator
   public static bool operator ==(Point point1, Point point2) => point1.X == point2.X && point1.Y == point2.Y;
   public static bool operator !=(Point point1, Point point2) => !(point1 == point2);

   public static bool Equals(Point point1, Point point2) => point1.X.Equals(point2.X) && point1.Y.Equals(point2.Y);
   public override bool Equals(object? o) => o is Point value && Equals(this, value);
   public bool Equals(Point value) => Equals(this, value);

   public override int GetHashCode() => X.GetHashCode() ^ Y.GetHashCode();

   public double X {get;} = x;

   public double Y {get; } = y;

   public override string ToString() => ConvertToString(null /* format string */, null /* format provider */);
   public string ToString(IFormatProvider provider) => ConvertToString(null /* format string */, provider);
   string IFormattable.ToString(string format, IFormatProvider provider) => ConvertToString(format, provider);

   internal string ConvertToString(string format, IFormatProvider provider)
   {
      // Helper to get the numeric list separator for a given culture.
      char separator = ',';//MS.Internal.TokenizerHelper.GetNumericListSeparator(provider);
      return string.Format(provider,
          "{1:" + format + "}{0}{2:" + format + "}",
          separator,
          X,
          Y);
   }

   public Point WithX(double x) => new Point(x, Y);
   public Point WithY(double y) => new Point(X, y);
}