using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace HLab.Geo.Simd;

[Serializable]
[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public readonly struct Vector(Vector128<double> v) : IFormattable
{
   public Vector(double x, double y) : this(Vector128.Create(x, y)) { }

   public double Length => Math.Sqrt(LengthSquared);
   public double LengthSquared => this * this;

   static Vector128<double> _signMask = Vector128.Create(-0.0); 
   public Vector Normalize()
   {
      var abs = Sse2.AndNot(_signMask, V); 
      return this / (Sse2.Max(abs, Avx.Permute(abs, 0b_01)).ToScalar() / Length);
   }
   public Vector NormalizeB()
   {
      var abs = Sse2.AndNot(_signMask, V); 
      return this / (Math.Max(V[0], V[1]) / Length);
   }

   public static double CrossProduct(Vector vector1, Vector vector2) => vector1.X * vector2.Y - vector1.Y * vector2.X;

   public static double AngleBetween(Vector vector1, Vector vector2)
   {
      var sin = CrossProduct(vector1, vector2);
      var cos = vector1 * vector2;

      return Math.Atan2(sin, cos) * (180 / Math.PI);
   }
   public static Vector Negate(Vector vector) => new(-vector.V);
   public static Vector Add(Vector vector1, Vector vector2) => new(vector1.V + vector2.V);
   public static Point Add(Vector vector, Point point) => new(point.V + vector.V);
   public static Vector Subtract(Vector vector1, Vector vector2) => new(vector1.V - vector2.V);
   public static Vector Multiply(Vector vector, double scalar) => new(vector.V * scalar);
   public static Vector Multiply(double scalar, Vector vector) => Multiply(vector, scalar);
   public static Vector Multiply(Vector vector, Matrix matrix) => matrix.Transform(vector);
   public static double DotMultiply(Vector vector1, Vector vector2) => Vector128.Sum( vector1.V * vector2.V);
   public static Vector Divide(Vector vector, double scalar) => vector * (1.0 / scalar);
   public static double Determinant(Vector vector1, Vector vector2) => CrossProduct(vector1, vector2);
   
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public static Vector ComponentMultiply(Vector vector1, Vector vector2) => new(Sse2.Multiply(vector1.V,vector2.V));

   public static Vector operator -(Vector vector) => Negate(vector);
   public static Vector operator +(Vector vector1, Vector vector2) => Add(vector1,vector2);
   public static Vector operator -(Vector vector1, Vector vector2) => Subtract(vector1,vector2);
   public static Point operator +(Vector vector, Point point) => Add(vector,point);
   public static Vector operator *(Vector vector, double scalar) => Multiply(vector, scalar);
   public static Vector operator *(double scalar, Vector vector) => Multiply(scalar, vector);
   public Vector ComponentMultiply(Vector vector) => ComponentMultiply(this,vector);

   public static Vector operator /(Vector vector, double scalar) => Divide(vector, scalar);
   public static Vector operator *(Vector vector, Matrix matrix) => Multiply(vector, matrix);
   public static double operator *(Vector vector1, Vector vector2) => DotMultiply(vector1, vector2);

   public static explicit operator Size(Vector vector) => new(Math.Abs(vector.X), Math.Abs(vector.Y));
   public static explicit operator Point(Vector vector) => new(vector.X, vector.Y);

   // ReSharper disable CompareOfFloatsByEqualityOperator
   public static bool operator ==(Vector vector1, Vector vector2) => vector1.V == vector2.V;
   public static bool operator !=(Vector vector1, Vector vector2) => !(vector1 == vector2);

   public static bool Equals(Vector vector1, Vector vector2) => vector1.X.Equals(vector2.X) && vector1.Y.Equals(vector2.Y);
   public override bool Equals(object? o) => o is Vector value && Equals(this, value);
   public bool Equals(Vector value) => Equals(this, value);

   public override int GetHashCode() => X.GetHashCode() ^ Y.GetHashCode();

   public double X => V[0];
   public double Y => V[1];
   public Vector128<double> V { get; } = v;

   public override string ToString() => ConvertToString(null /* format string */, null /* format provider */);
   public string ToString(IFormatProvider provider) => ConvertToString(null /* format string */, provider);
   string IFormattable.ToString(string format, IFormatProvider provider) => ConvertToString(format, provider);

   internal string ConvertToString(string format, IFormatProvider provider)
   {
      const char separator = ','; 
      return string.Format(provider,
          "{1:" + format + "}{0}{2:" + format + "}",
          separator,
          X,
          Y);
   }

}