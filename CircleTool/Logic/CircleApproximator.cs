using Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CircleTool.Logic;

public class CircleApproximator
{
    public static Point[] Rasterize(double radius)
    {
        // determine cells on circle

        List<Point> cells = new List<Point>();

        int x = 0;
        int y = (int)Math.Floor(radius);
        while (x <= y)
        {
            cells.Add(new Point(x, y));
            if (x != y) 
                cells.Add(new Point(y, x));

            x++;
            if (x * x + y * y > radius * radius)
                y--;
        }

        cells.Sort((l, r) => (l.X - l.Y).CompareTo(r.X - r.Y));


        // expand cells to blocks

        List<Point> blocks = new List<Point>();

        foreach (var c in cells)
            blocks.AddRange(
                [
                    new Point(c.X + .5, c.Y + .5),
                    new Point(c.X + .5, c.Y - .5),
                    new Point(c.X - .5, c.Y - .5),
                    new Point(c.X - .5, c.Y + .5),
                    new Point(c.X + .5, c.Y + .5)
                ]);

        return blocks.ToArray();
    }

    public static Point[] WedgesMax(double radius, bool narrow)
    {
        // generate line segments

        List<Point> cells = new List<Point>();

        double x = -0.5;
        double y = Math.Floor(radius * Math.Sin(Math.Acos(0.5 / radius)) - 0.5) + 0.5;

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

        cells.Sort((l, r) => (l.X - l.Y).CompareTo(r.X - r.Y));


        // expand line segments to wedges and blocks

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
            cells.AddRange([ array[i], array[i] + d]);
            cells.AddRange(d.X * d.Y == 0 ? 
                [array[i] - new Point(d.Y, d.X) / (d.X + d.Y) + d,
                 array[i] - new Point(d.Y, d.X) / (d.X + d.Y)] :
                [cells.Last() - new Point(d.X, 0)]);
            cells.AddRange([ array[i], array[i] + d]);
        }

        return cells.ToArray();
    }
}
