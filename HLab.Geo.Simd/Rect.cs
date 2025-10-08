using System.Diagnostics;
using System.Runtime.Intrinsics;

namespace HLab.Geo.Simd;

/// <summary>
/// Rect - The primitive which represents a rectangle.  Rects are stored as
/// X, Y (Location) and Width and Height (Size).  As a result, Rects cannot have negative
/// Width or Height.
/// </summary>
public partial struct Rect(Vector256<double> v){
   public Rect(Vector128<double> position, Vector128<double> size): this(Vector256.Create(position,size)) { }

   public Rect(Point position, Size size): this(position.V, size.V)
   {
      if (!size.IsEmpty) return;
      this = s_empty;
   }

   /// <summary>
   /// Constructor which sets the initial values to the values of the parameters.
   /// Width and Height must be non-negative
   /// </summary>
   public Rect(double x,
               double y,
               double width,
               double height):this(Vector256.Create(x, y,width, height))
   {
      if (width < 0 || height < 0)
      {
         throw new System.ArgumentException("SR.Size_WidthAndHeightCannotBeNegative");
      }
   }

   /// <summary>
   /// Constructor which sets the initial values to bound the two points provided.
   /// </summary>
   public Rect(Point point1, Point point2)
   {
      var min = Point.Min(point1, point2);
      var max = Point.Max(point1, point2);
      _position = min;
      _size = (Size)max - min;
   }

   /// <summary>
   /// Constructor which sets the initial values to bound the point provided and the point
   /// which results from point + vector.
   /// </summary>
   public Rect(Point point, Vector vector) : this(point, point + vector)
   {
   }

   /// <summary>
   /// Constructor which sets the initial values to bound the (0,0) point and the point 
   /// that results from (0,0) + size. 
   /// </summary>
   public Rect(Size size)
   {
      if (size.IsEmpty) {
         this = s_empty;
         return;
      }

      _position = Point.Zero;
      _size = size;
   }

   /// <summary>
   /// Empty - a static property which provides an Empty rectangle.  X and Y are positive-infinity
   /// and Width and Height are negative infinity.  This is the only situation where Width or
   /// Height can be negative.
   /// </summary>
   public static Rect Empty => s_empty;


   /// <summary>
   /// IsEmpty - this returns true if this rect is the Empty rectangle.
   /// Note: If width or height are 0 this Rectangle still contains a 0 or 1 dimensional set
   /// of points, so this method should not be used to check for 0 area.
   /// </summary>
   public bool IsEmpty
   {
      get
      {
         // The funny width and height tests are to handle NaNs
         Debug.Assert((!(_width < 0) && !(_height < 0)) || (this == Empty));

         return _width < 0;
      }
   }

   /// <summary>
   /// Location - The Point representing the origin of the Rectangle
   /// </summary>
   public Point Location
   {
      get
      {
         return new Point(X, Y);
      }
      set
      {
         if (IsEmpty)
         {
            throw new System.InvalidOperationException("SR.Rect_CannotModifyEmptyRect");
         }

         X = value.X;
         Y = value.Y;
      }
   }

   /// <summary>
   /// Size - The Size representing the area of the Rectangle
   /// </summary>
   public Size Size
   {
      get
      {
         if (IsEmpty) return Size.Empty;
         return _size;
      }
      set
      {
         if (value.IsEmpty)
         {
            this = s_empty;
         }
         else
         {
            if (IsEmpty)
            {
               throw new System.InvalidOperationException("SR.Rect_CannotModifyEmptyRect");
            }

            _width = value._width;
            _height = value._height;
         }
      }
   }

   /// <summary>
   /// X - The X coordinate of the Location.
   /// If this is the empty rectangle, the value will be positive infinity.
   /// If this rect is Empty, setting this property is illegal.
   /// </summary>
   public double X
   {
      get => X;
      set
      {
         if (IsEmpty)
         {
            throw new System.InvalidOperationException("SR.Rect_CannotModifyEmptyRect");
         }

         X = value;
      }
   }

   /// <summary>
   /// Y - The Y coordinate of the Location
   /// If this is the empty rectangle, the value will be positive infinity.
   /// If this rect is Empty, setting this property is illegal.
   /// </summary>
   public double Y
   {
      get => Y;
      set
      {
         if (IsEmpty)
         {
            throw new System.InvalidOperationException("SR.Rect_CannotModifyEmptyRect");
         }

         Y = value;
      }
   }

   /// <summary>
   /// Width - The Width component of the Size.  This cannot be set to negative, and will only
   /// be negative if this is the empty rectangle, in which case it will be negative infinity.
   /// If this rect is Empty, setting this property is illegal.
   /// </summary>
   public double Width
   {
      get => _size.Width;
      set
      {
         if (IsEmpty)
         {
            throw new System.InvalidOperationException("SR.Rect_CannotModifyEmptyRect");
         }

         if (value < 0)
         {
            throw new System.ArgumentException("SR.Size_WidthCannotBeNegative");
         }

         _width = value;
      }
   }

   /// <summary>
   /// Height - The Height component of the Size.  This cannot be set to negative, and will only
   /// be negative if this is the empty rectangle, in which case it will be negative infinity.
   /// If this rect is Empty, setting this property is illegal.
   /// </summary>
   public double Height => _size.Height;

   /// <summary>
   /// Left Property - This is a read-only alias for X
   /// If this is the empty rectangle, the value will be positive infinity.
   /// </summary>
   public double Left => X;

   /// <summary>
   /// Top Property - This is a read-only alias for Y
   /// If this is the empty rectangle, the value will be positive infinity.
   /// </summary>
   public double Top => Y;

   /// <summary>
   /// Right Property - This is a read-only alias for X + Width
   /// If this is the empty rectangle, the value will be negative infinity.
   /// </summary>
   public double Right
   {
      get
      {
         if (IsEmpty)
         {
            return double.NegativeInfinity;
         }

         return X + _size.Width;
      }
   }

   /// <summary>
   /// Bottom Property - This is a read-only alias for Y + Height
   /// If this is the empty rectangle, the value will be negative infinity.
   /// </summary>
   public double Bottom
   {
      get
      {
         if (IsEmpty)
         {
            return double.NegativeInfinity;
         }

         return Y + _size.Height;
      }
   }

   /// <summary>
   /// TopLeft Property - This is a read-only alias for the Point which is at X, Y
   /// If this is the empty rectangle, the value will be positive infinity, positive infinity.
   /// </summary>
   public Point TopLeft => new(Left, Top);

   /// <summary>
   /// TopRight Property - This is a read-only alias for the Point which is at X + Width, Y
   /// If this is the empty rectangle, the value will be negative infinity, positive infinity.
   /// </summary>
   public Point TopRight => new(Right, Top);

   /// <summary>
   /// BottomLeft Property - This is a read-only alias for the Point which is at X, Y + Height
   /// If this is the empty rectangle, the value will be positive infinity, negative infinity.
   /// </summary>
   public Point BottomLeft => new(Left, Bottom);

   /// <summary>
   /// BottomRight Property - This is a read-only alias for the Point which is at X + Width, Y + Height
   /// If this is the empty rectangle, the value will be negative infinity, negative infinity.
   /// </summary>
   public Point BottomRight => new(Right, Bottom);

   #endregion Public Properties

   #region Public Methods

   /// <summary>
   /// Contains - Returns true if the Point is within the rectangle, inclusive of the edges.
   /// Returns false otherwise.
   /// </summary>
   /// <param name="point"> The point which is being tested </param>
   /// <returns>
   /// Returns true if the Point is within the rectangle.
   /// Returns false otherwise
   /// </returns>
   public bool Contains(Point point) => Contains(point.X, point.Y);

   /// <summary>
   /// Contains - Returns true if the Point represented by x,y is within the rectangle inclusive of the edges.
   /// Returns false otherwise.
   /// </summary>
   /// <param name="x"> X coordinate of the point which is being tested </param>
   /// <param name="y"> Y coordinate of the point which is being tested </param>
   /// <returns>
   /// Returns true if the Point represented by x,y is within the rectangle.
   /// Returns false otherwise.
   /// </returns>
   public bool Contains(double x, double y) => !IsEmpty && ContainsInternal(x, y);

   /// <summary>
   /// Contains - Returns true if the Rect non-Empty and is entirely contained within the
   /// rectangle, inclusive of the edges.
   /// Returns false otherwise
   /// </summary>
   public bool Contains(Rect rect)
   {
      if (IsEmpty || rect.IsEmpty)
      {
         return false;
      }

      return (_x <= rect.X &&
              _y <= rect.Y &&
              _x + _width >= rect.X + rect._width &&
              _y + _height >= rect.Y + rect._height);
   }

   /// <summary>
   /// IntersectsWith - Returns true if the Rect intersects with this rectangle
   /// Returns false otherwise.
   /// Note that if one edge is coincident, this is considered an intersection.
   /// </summary>
   /// <returns>
   /// Returns true if the Rect intersects with this rectangle
   /// Returns false otherwise.
   /// or Height
   /// </returns>
   /// <param name="rect"> Rect </param>
   public bool IntersectsWith(Rect rect)
   {
      if (IsEmpty || rect.IsEmpty)
      {
         return false;
      }

      return (rect.Left <= Right) &&
             (rect.Right >= Left) &&
             (rect.Top <= Bottom) &&
             (rect.Bottom >= Top);
   }

   /// <summary>
   /// Intersect - Update this rectangle to be the intersection of this and rect
   /// If either this or rect are Empty, the result is Empty as well.
   /// </summary>
   /// <param name="rect"> The rect to intersect with this </param>
   public void Intersect(Rect rect)
   {
      if (!this.IntersectsWith(rect))
      {
         this = Empty;
      }
      else
      {
         var left = Math.Max(Left, rect.Left);
         var top = Math.Max(Top, rect.Top);

         //  Max with 0 to prevent double weirdness from causing us to be (-epsilon..0)
         _width = Math.Max(Math.Min(Right, rect.Right) - left, 0);
         _height = Math.Max(Math.Min(Bottom, rect.Bottom) - top, 0);

         _x = left;
         _y = top;
      }
   }

   /// <summary>
   /// Intersect - Return the result of the intersection of rect1 and rect2.
   /// If either this or rect are Empty, the result is Empty as well.
   /// </summary>
   public static Rect Intersect(Rect rect1, Rect rect2)
   {
      rect1.Intersect(rect2);
      return rect1;
   }

   /// <summary>
   /// Union - Update this rectangle to be the union of this and rect.
   /// </summary>
   public void Union(Rect rect)
   {
      if (IsEmpty)
      {
         this = rect;
      }
      else if (!rect.IsEmpty)
      {
         var left = Math.Min(Left, rect.Left);
         var top = Math.Min(Top, rect.Top);


         // We need this check so that the math does not result in NaN
         if ((rect.Width == Double.PositiveInfinity) || (Width == Double.PositiveInfinity))
         {
            _width = double.PositiveInfinity;
         }
         else
         {
            //  Max with 0 to prevent double weirdness from causing us to be (-epsilon..0)                    
            double maxRight = Math.Max(Right, rect.Right);
            _width = Math.Max(maxRight - left, 0);
         }

         // We need this check so that the math does not result in NaN
         if ((rect.Height == Double.PositiveInfinity) || (Height == Double.PositiveInfinity))
         {
            _height = Double.PositiveInfinity;
         }
         else
         {
            //  Max with 0 to prevent double weirdness from causing us to be (-epsilon..0)
            double maxBottom = Math.Max(Bottom, rect.Bottom);
            _height = Math.Max(maxBottom - top, 0);
         }

         _x = left;
         _y = top;
      }
   }

   /// <summary>
   /// Union - Return the result of the union of rect1 and rect2.
   /// </summary>
   public static Rect Union(Rect rect1, Rect rect2)
   {
      rect1.Union(rect2);
      return rect1;
   }

   /// <summary>
   /// Union - Update this rectangle to be the union of this and point.
   /// </summary>
   public void Union(Point point)
   {
      Union(new Rect(point, point));
   }

   /// <summary>
   /// Union - Return the result of the union of rect and point.
   /// </summary>
   public static Rect Union(Rect rect, Point point)
   {
      rect.Union(new Rect(point, point));
      return rect;
   }

   /// <summary>
   /// Offset - translate the Location by the offset provided.
   /// If this is Empty, this method is illegal.
   /// </summary>
   public void Offset(Vector offsetVector)
   {
      if (IsEmpty)
      {
         throw new System.InvalidOperationException("SR.Rect_CannotCallMethod");
      }

      _x += offsetVector.X;
      _y += offsetVector.Y;
   }

   /// <summary>
   /// Offset - translate the Location by the offset provided
   /// If this is Empty, this method is illegal.
   /// </summary>
   public void Offset(double offsetX, double offsetY)
   {
      if (IsEmpty)
      {
         throw new System.InvalidOperationException("SR.Rect_CannotCallMethod");
      }

      _x += offsetX;
      _y += offsetY;
   }

   /// <summary>
   /// Offset - return the result of offsetting rect by the offset provided
   /// If this is Empty, this method is illegal.
   /// </summary>
   public static Rect Offset(Rect rect, Vector offsetVector)
   {
      rect.Offset(offsetVector.X, offsetVector.Y);
      return rect;
   }

   /// <summary>
   /// Offset - return the result of offsetting rect by the offset provided
   /// If this is Empty, this method is illegal.
   /// </summary>
   public static Rect Offset(Rect rect, double offsetX, double offsetY)
   {
      rect.Offset(offsetX, offsetY);
      return rect;
   }

   /// <summary>
   /// Inflate - inflate the bounds by the size provided, in all directions
   /// If this is Empty, this method is illegal.
   /// </summary>
   public void Inflate(Size size)
   {
      Inflate(size._width, size._height);
   }

   /// <summary>
   /// Inflate - inflate the bounds by the size provided, in all directions.
   /// If -width is > Width / 2 or -height is > Height / 2, this Rect becomes Empty
   /// If this is Empty, this method is illegal.
   /// </summary>
   public void Inflate(double width, double height)
   {
      if (IsEmpty)
      {
         throw new System.InvalidOperationException("SR.Rect_CannotCallMethod");
      }

      _x -= width;
      _y -= height;

      // Do two additions rather than multiplication by 2 to avoid spurious overflow
      // That is: (A + 2 * B) != ((A + B) + B) if 2*B overflows.
      // Note that multiplication by 2 might work in this case because A should start
      // positive & be "clamped" to positive after, but consider A = Inf & B = -MAX.
      _width += width;
      _width += width;
      _height += height;
      _height += height;

      // We catch the case of inflation by less than -width/2 or -height/2 here.  This also
      // maintains the invariant that either the Rect is Empty or _width and _height are
      // non-negative, even if the user parameters were NaN, though this isn't strictly maintained
      // by other methods.
      if (!(_width >= 0 && _height >= 0))
      {
         this = s_empty;
      }
   }

   /// <summary>
   /// Inflate - return the result of inflating rect by the size provided, in all directions
   /// If this is Empty, this method is illegal.
   /// </summary>
   public static Rect Inflate(Rect rect, Size size)
   {
      rect.Inflate(size._width, size._height);
      return rect;
   }

   /// <summary>
   /// Inflate - return the result of inflating rect by the size provided, in all directions
   /// If this is Empty, this method is illegal.
   /// </summary>
   public static Rect Inflate(Rect rect, double width, double height)
   {
      rect.Inflate(width, height);
      return rect;
   }

   /// <summary>
   /// Returns the bounds of the transformed rectangle.
   /// The Empty Rect is not affected by this call.
   /// </summary>
   /// <returns>
   /// The rect which results from the transformation.
   /// </returns>
   /// <param name="rect"> The Rect to transform. </param>
   /// <param name="matrix"> The Matrix by which to transform. </param>
   public static Rect Transform(Rect rect, Matrix matrix)
   {
      MatrixUtil.TransformRect(ref rect, ref matrix);
      return rect;
   }

   /// <summary>
   /// Updates rectangle to be the bounds of the original value transformed
   /// by the matrix.
   /// The Empty Rect is not affected by this call.        
   /// </summary>
   /// <param name="matrix"> Matrix </param>
   public void Transform(Matrix matrix)
   {
      MatrixUtil.TransformRect(ref this, ref matrix);
   }

   public Rect Scale(double scaleX, double scaleY) => Scale(Vector128.Create(scaleX, scaleY));
   /// <summary>
   /// Scale the rectangle in the X and Y directions
   /// </summary>
   /// <param name="scaleX"> The scale in X </param>
   /// <param name="scaleY"> The scale in Y </param>
   public Rect Scale(Vector128<double> scale)
   {
      if (IsEmpty)
      {
         return this;
      }

      var p = _position.V * scale;
      var w = _size.V * scale;

      // If the scale in the X dimension is negative, we need to normalize X and Width
      if (scaleX < 0)
      {
         // Make X the left-most edge again
         _x += _width;

         // and make Width positive
         _width *= -1;
      }

      // Do the same for the Y dimension
      if (scaleY < 0)
      {
         // Make Y the top-most edge again
         _y += _height;

         // and make Height positive
         _height *= -1;
      }
   }

   bool ContainsInternal(double x, double y) =>
      ((x >= X) && (x - Width <= X) &&
       (y >= Y) && (y - Height <= Y));

   static  Rect CreateEmptyRect() => new(double.PositiveInfinity,double.PositiveInfinity,double.NegativeInfinity,double.NegativeInfinity);

   static readonly Rect s_empty = CreateEmptyRect();
}



[Serializable]
//[TypeConverter(typeof(RectConverter))]
//[ValueSerializer(typeof(RectValueSerializer))] // Used by MarkupWriter
partial struct Rect : IFormattable
{
   //------------------------------------------------------
   //
   //  Public Methods
   //
   //------------------------------------------------------

   #region Public Methods




   /// <summary>
   /// Compares two Rect instances for exact equality.
   /// Note that double values can acquire error when operated upon, such that
   /// an exact comparison between two values which are logically equal may fail.
   /// Furthermore, using this equality operator, Double.NaN is not equal to itself.
   /// </summary>
   /// <returns>
   /// bool - true if the two Rect instances are exactly equal, false otherwise
   /// </returns>
   /// <param name='rect1'>The first Rect to compare</param>
   /// <param name='rect2'>The second Rect to compare</param>
   public static bool operator ==(Rect rect1, Rect rect2)
   {
      return rect1.X == rect2.X &&
             rect1.Y == rect2.Y &&
             rect1.Width == rect2.Width &&
             rect1.Height == rect2.Height;
   }

   /// <summary>
   /// Compares two Rect instances for exact inequality.
   /// Note that double values can acquire error when operated upon, such that
   /// an exact comparison between two values which are logically equal may fail.
   /// Furthermore, using this equality operator, Double.NaN is not equal to itself.
   /// </summary>
   /// <returns>
   /// bool - true if the two Rect instances are exactly unequal, false otherwise
   /// </returns>
   /// <param name='rect1'>The first Rect to compare</param>
   /// <param name='rect2'>The second Rect to compare</param>
   public static bool operator !=(Rect rect1, Rect rect2)
   {
      return !(rect1 == rect2);
   }
   /// <summary>
   /// Compares two Rect instances for object equality.  In this equality
   /// Double.NaN is equal to itself, unlike in numeric equality.
   /// Note that double values can acquire error when operated upon, such that
   /// an exact comparison between two values which
   /// are logically equal may fail.
   /// </summary>
   /// <returns>
   /// bool - true if the two Rect instances are exactly equal, false otherwise
   /// </returns>
   /// <param name='rect1'>The first Rect to compare</param>
   /// <param name='rect2'>The second Rect to compare</param>
   public static bool Equals(Rect rect1, Rect rect2)
   {
      if (rect1.IsEmpty)
      {
         return rect2.IsEmpty;
      }
      else
      {
         return rect1.X.Equals(rect2.X) &&
                rect1.Y.Equals(rect2.Y) &&
                rect1.Width.Equals(rect2.Width) &&
                rect1.Height.Equals(rect2.Height);
      }
   }

   /// <summary>
   /// Equals - compares this Rect with the passed in object.  In this equality
   /// Double.NaN is equal to itself, unlike in numeric equality.
   /// Note that double values can acquire error when operated upon, such that
   /// an exact comparison between two values which
   /// are logically equal may fail.
   /// </summary>
   /// <returns>
   /// bool - true if the object is an instance of Rect and if it's equal to "this".
   /// </returns>
   /// <param name='o'>The object to compare to "this"</param>
   public override bool Equals(object o)
   {
      if ((null == o) || !(o is Rect))
      {
         return false;
      }

      Rect value = (Rect)o;
      return Rect.Equals(this, value);
   }

   /// <summary>
   /// Equals - compares this Rect with the passed in object.  In this equality
   /// Double.NaN is equal to itself, unlike in numeric equality.
   /// Note that double values can acquire error when operated upon, such that
   /// an exact comparison between two values which
   /// are logically equal may fail.
   /// </summary>
   /// <returns>
   /// bool - true if "value" is equal to "this".
   /// </returns>
   /// <param name='value'>The Rect to compare to "this"</param>
   public bool Equals(Rect value)
   {
      return Rect.Equals(this, value);
   }
   /// <summary>
   /// Returns the HashCode for this Rect
   /// </summary>
   /// <returns>
   /// int - the HashCode for this Rect
   /// </returns>
   public override int GetHashCode()
   {
      if (IsEmpty)
      {
         return 0;
      }
      else
      {
         // Perform field-by-field XOR of HashCodes
         return _x.GetHashCode() ^
                _y.GetHashCode() ^
                Width.GetHashCode() ^
                Height.GetHashCode();
      }
   }
   /// <summary>
   /// Creates a string representation of this object based on the current culture.
   /// </summary>
   /// <returns>
   /// A string representation of this object.
   /// </returns>
   public override string ToString() => ConvertToString(null /* format string */, null /* format provider */);

   /// <summary>
   /// Creates a string representation of this object based on the IFormatProvider
   /// passed in.  If the provider is null, the CurrentCulture is used.
   /// </summary>
   /// <returns>
   /// A string representation of this object.
   /// </returns>
   public string ToString(IFormatProvider provider) => ConvertToString(null /* format string */, provider);

   /// <summary>
   /// Creates a string representation of this object based on the format string
   /// and IFormatProvider passed in.
   /// If the provider is null, the CurrentCulture is used.
   /// See the documentation for IFormattable for more information.
   /// </summary>
   /// <returns>
   /// A string representation of this object.
   /// </returns>
   string IFormattable.ToString(string format, IFormatProvider provider) => ConvertToString(format, provider);

   /// <summary>
   /// Creates a string representation of this object based on the format string
   /// and IFormatProvider passed in.
   /// If the provider is null, the CurrentCulture is used.
   /// See the documentation for IFormattable for more information.
   /// </summary>
   /// <returns>
   /// A string representation of this object.
   /// </returns>
   internal string ConvertToString(string format, IFormatProvider provider)
   {
      if (IsEmpty)
      {
         return "Empty";
      }

      // Helper to get the numeric list separator for a given culture.
      char separator = ',';//MS.Internal.TokenizerHelper.GetNumericListSeparator(provider);
      return String.Format(provider,
          "{1:" + format + "}{0}{2:" + format + "}{0}{3:" + format + "}{0}{4:" + format + "}",
          separator,
          X,
          Y,
          _size.Width,
          _size.Height);
   }



   #endregion Internal Properties

}