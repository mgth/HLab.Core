﻿namespace HLab.Geo.Simd;

public class Segment
{
    Line _line;

    public Segment(Point a, Point b)
    {
        A = a; B = b;
    }
    public Point A { get; }
    public Point B { get; }

    public Line Line => _line ?? (_line = Line.FromSegment(this));

    public Rect Rect => new Rect(A, B);

    public double Size => Math.Sqrt(LengthSquared);

    public double LengthSquared
    {
        get
        {
            var r = Rect;
            return r.Width * r.Width  + r.Height * r.Height;
        }
    }

    public Point Intersect(Line l)
    {
        var p = Line.Intersect(l);
        if (p.Is) return Point.Undefined;

        Point p1 = p.Value;

        if (p1.X < Rect.X - Epsilon) return null;
        if (p1.Y < Rect.Y - Epsilon) return null;
        if (p1.X > Rect.Right + Epsilon) return null;
        if (p1.Y > Rect.Bottom + Epsilon) return null;
        //        && Rect.Contains(p.Value)) return p;
        return p1;
    }


    const double Epsilon = 0.001; 
    public Point? Intersect(Segment s)
    {
        Point? p = Line.Intersect(s.Line);

        if (p == null) return null;

        if (p.Value.X < Rect.X - Epsilon) return null;
        if (p.Value.Y < Rect.Y - Epsilon) return null;
        if (p.Value.X > Rect.Right + Epsilon) return null;
        if (p.Value.Y > Rect.Bottom + Epsilon) return null;

        return p;
    }

    public IEnumerable<Point> Intersect(Rect r)
    {
        Point? p = null;
            
        p = Intersect(new Segment(r.TopLeft, r.BottomLeft));
        if (p != null) yield return p.Value;

        p = Intersect(new Segment(r.TopRight, r.BottomRight));
        if (p != null) yield return p.Value;

        p = Intersect(new Segment(r.TopLeft, r.TopRight));
        if (p != null) yield return p.Value;

        p = Intersect(new Segment(r.BottomLeft, r.BottomRight));
        if (p != null) yield return p.Value;
    }

    public Side IntersectSide(Rect rect)
    {
        Side[] sides = {Side.Left, Side.Right, Side.Top, Side.Bottom};

        //return sides.Where(s => Intersect(r.Segment(s)) != null).FirstOrDefault();
        foreach (var side in sides)
        {
            if (Intersect(rect.Segment(side)) != null) return side;
        }
        return Side.None;
    }

    public static Side OpositeSide(Side side)
    {
        switch (side)
        {
            case Side.Left:
                return Side.Right;
            case Side.Right:
                return Side.Left;
            case Side.Top:
                return Side.Bottom;
            case Side.Bottom:
                return Side.Top;
            default:
                return Side.None;
        }
    }

}