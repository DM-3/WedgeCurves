using Avalonia;
using System;
using System.Collections.Generic;

namespace CircleTool.Logic;

public class CircleApproximator
{
    public static Point[] Rasterize(double radius)
    {
        List<Point> cells = new List<Point>();

        int x = 0;
        int y = (int)Math.Floor(radius);
        while (x < y)
        {
            cells.Add(new Point(x, y));
            cells.Add(new Point(y, x));

            x++;
            if (x * x + y * y > radius * radius)
                y--;
        }

        cells.Sort(delegate(Point l, Point r) 
            { 
                return (l.X - l.Y).CompareTo(r.X - r.Y); 
            });

        return cells.ToArray();
    }
}
