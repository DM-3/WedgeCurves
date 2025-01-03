using Avalonia;
using System;
using System.Collections.Generic;

namespace CircleTool.Logic;

public class CircleApproximator
{
    public static Point[] Rasterize(double radius, bool oddCenter, bool _, bool outsideEdge)
    {
        List<Point> cells = new List<Point>();
        
        double t = 0.5 * Convert.ToDouble(!oddCenter);
        double x = t;
        double y = Math.Floor(radius * Math.Sin(Math.Acos(t / radius)) - t) + t;

        cells.Add(new Point(x - 0.5, y + 0.5));
        cells.Add(new Point(y + 0.5, x - 0.5));
        while (x <= y)
        {
            if (x * x + y * y > radius * radius)
            {
                y--;
                x--;
            }

            cells.Add(new Point(x + 0.5, y + 0.5));
            if (x != y)
                cells.Add(new Point(y + 0.5, x + 0.5));
            x++;
        }

        return LineToWedges(cells, true, outsideEdge);
    }

    public static Point[] WedgesMax(double radius, bool oddCenter, bool narrow, bool outsideEdge)
    {
        List<Point> cells = new List<Point>();

        double t = 0.5 * Convert.ToDouble(oddCenter);
        double x = -t;
        double y = Math.Floor(radius * Math.Sin(Math.Acos(t / radius)) - t) + t;

        cells.Add(new Point(x, y));
        cells.Add(new Point(y, x));
        while (x <= y)
        {
            if ((x + 1) * (x + 1) + y * y > radius * radius)
            {
                cells.Add(new Point(x, y));
                if (x != y)
                    cells.Add(new Point(y, x));
                y--;
            }
            x++;
        }

        return LineToWedges(cells, narrow, outsideEdge);
    }

    public static Point[] WedgesMin(double radius, bool oddCenter, bool narrow, bool outsideEdge)
    {
        List<Point> cells = new List<Point>();

        double t = 0.5 * Convert.ToDouble(oddCenter);
        double a = radius * 0.707106781;
        double x = Math.Floor(a + 0.5 - t) + t;
        double y = Math.Floor(a + t) + 1.0 - t;

        cells.Add(new Point(x, y));
        if (x != y)
            cells.Add(new Point(y, x));

        y++;
        while (x > 0.5)
        {
            // TODO: better line-circle intersection test
            if ((x - 1) * (x - 1) + y * y < radius * radius)
            {
                cells.Add(new Point(x, y));
                cells.Add(new Point(y, x));
                y++;
            }
            x--;
        }

        cells.AddRange([ new Point(t, y), new Point(y, t) ]);
        if (oddCenter)
            cells.AddRange([ new Point(-t, y), new Point(y, -t) ]);

        return LineToWedges(cells, narrow, outsideEdge);
    }

    // expand line segments to wedges and blocks
    private static Point[] LineToWedges(List<Point> cells, bool narrow, bool outsideEdge)
    {
        cells.Sort((l, r) => (l.X - l.Y).CompareTo(r.X - r.Y));

        var array = cells.ToArray();
        cells.Clear();

        for (uint i = 0, j = 1; i < array.Length - 1; i += j)
        {
            Point d = array[i + 1] - array[i];
            // combine neighboring segments if possible
            if (!narrow)
                for (j = 1; i + j < array.Length - 1; j++)
                {
                    var td = array[i + j + 1] - array[i];
                    if (td.X * d.Y > d.X * td.Y)
                        break;
                    d = td;
                }

            // add wedge or block
            cells.AddRange([ array[i], array[i] + d ]);
            var invD = new Point(d.Y, d.X) / (d.X + d.Y) * (outsideEdge ? -1 : 1);
            cells.AddRange(d.X * d.Y == 0 ? 
                [ array[i] + invD + d, array[i] + invD ] :
                [ array[i] + (outsideEdge ? new Point(0, d.Y) : new Point(d.X, 0)) ]);
            cells.AddRange([ array[i], array[i] + d]);
        }

        return cells.ToArray();
    }
}
