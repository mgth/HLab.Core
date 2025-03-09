namespace HLab.Geo.Simd;

public class Line(double slope, double origin)
{
   const double TOLERANCE = 1e-10;

   // Specific case of vertical line where slope is infinite
   public Line(double x) : this(double.PositiveInfinity, x)
   {
   }

   public static Line FromSegment(Segment s)
   {
      // Line is vertical
      if (Math.Abs(s.A.X - s.B.X) < TOLERANCE)
      {
         return new (s.A.X);
      }

      var v = s.A - s.B;
      var slope = v.Y / v.X;
      var origin = s.A.Y - slope * s.A.X;

      return new (slope, origin);
   }

   public double Slope => slope;

   public double OriginY => double.IsPositiveInfinity(slope) ? 0 : origin;

   public double OriginX => double.IsPositiveInfinity(slope) ? origin : (0 - origin) / Slope;

   public Point Origin => new Point(OriginX, OriginY);

   public bool IsVertical => double.IsPositiveInfinity(slope);

   public bool IsHorizontal => Math.Abs(slope) < TOLERANCE;


   public Point Intersect(Line l)
   {
      if (Math.Abs(slope - l.Slope) < TOLERANCE)
      {
         return Math.Abs(OriginY - l.OriginY) < TOLERANCE ? Origin : Point.Undefined;
      }

      if (IsVertical)
      {
         return l.IsVertical ? Point.Undefined : new (OriginX, l.Slope * OriginX + l.OriginY);
      }

      if (l.IsVertical) {
         return new (l.OriginX, slope * l.OriginX + OriginY);
      }

      var x = (OriginY - l.OriginY) / (l.Slope - slope);
      return new (x, l.Slope * x + l.OriginY);
   }

   public IEnumerable<Point> Intersect(Segment s)
   {
      var p = Intersect(s.Line);
      if (!p.IsUndefined && s.Rect.Contains(p))
      {
         yield return p;
      }
   }

   public IEnumerable<Point> Intersect(Rect rect)
   {
      foreach (var p in Intersect(rect.Segment(Side.Left))) yield return p;
      foreach (var p in Intersect(rect.Segment(Side.Right))) yield return p;
      foreach (var p in Intersect(rect.Segment(Side.Top))) yield return p;
      foreach (var p in Intersect(rect.Segment(Side.Bottom))) yield return p;
   }
}