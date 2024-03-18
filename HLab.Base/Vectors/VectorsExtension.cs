using System.Numerics;
using System.Runtime.Intrinsics;

namespace HLab.Base.Vectors;

public static class VectorConstants
{
   #if NET8_0_OR_GREATER
   public static Vector512<int> Range_0_15 = Vector512.Create(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15);
   public static Vector512<int> Value2 = Vector512.Create(2);
   public static Vector512<int> Value4 = Vector512.Create(4);
   public static Vector512<int> Value8 = Vector512.Create(8);
   public static Vector512<int> Value16 = Vector512.Create(16);
   public static Vector512<int> Value32 = Vector512.Create(32);
   public static Vector512<int> Value64 = Vector512.Create(64);
   #endif
}


public static class VectorsExtension
{
    public static Vector<int> Modulo(this Vector<int> @this, Vector<int> div) => @this - (@this / div) * div;
    public static Vector64<int> Modulo(this Vector64<int> @this, Vector64<int> div) => @this - (@this / div) * div;
    public static Vector128<int> Modulo(this Vector128<int> @this, Vector128<int> div) => @this - (@this / div) * div;
    public static Vector256<int> Modulo(this Vector256<int> @this, Vector256<int> div) => @this - (@this / div) * div;

    #if NET8_0_OR_GREATER
    public static Vector512<int> Modulo(this Vector512<int> @this, Vector512<int> div) => @this - (@this / div) * div;
    #endif
}