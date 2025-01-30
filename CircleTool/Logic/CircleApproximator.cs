using Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;

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
        var fn_lineCost = (Point from, Point to) =>
        {
            // return infinite cost if line intersects circle
            double m = (to.Y - from.Y) / (to.X - from.X);
            double n = to.Y - m * to.X;
            double r = radius - 0.02;       // prevent unreachable cells through small margin of error
            double radicand = r * r + r * r * m * m - n * n;
            if (radicand > 0)
                return double.PositiveInfinity;

            // else return area between circle and line
            double at = to.X * from.Y - from.X * to.Y;
            double a1 = from.X == 0 ? 0.5 * Math.PI : Math.Atan(from.Y / from.X);
            double a2 = Math.Atan(to.Y / to.X);
            return at - radius * radius * (a1 - a2);
        };

        // prepare graph nodes
        List<Point> cells = new List<Point>();
        double t = 0.5 * Convert.ToDouble(oddCenter);
        double x = t, y;
        do
        {
            double accY = Math.Sin(Math.Acos(x / radius)) * radius;
            y = Math.Ceiling(accY + t) - t;
            cells.Add(new Point(x, y));
        }
        while (++x < y);

        // prepare graph edge matrix
        double[,] connectionMatrix = new double[cells.Count, cells.Count];
        for (int i = 0; i < cells.Count; i++)
            for (int j = i + 1; j < cells.Count; j++)
                connectionMatrix[i, j] = fn_lineCost(cells[i], cells[j]);

        // list for shortest path algorithm to work on
        List<(int prev, double distance, bool unvisited)> list = new List<(int, double, bool)>(cells.Count);
        for (int i = 0; i < cells.Count; i++)
            list.Add((i, i == 0 ? 0 : double.PositiveInfinity, true));

        // Dijkstra shortest path
        while (list.Last().unvisited)
        {
            // move to unvisited cell with shortest path next
            double minD = list.Where(e => e.unvisited).Select(e => e.distance).Min();
            if (minD == double.PositiveInfinity)
            {
                Console.WriteLine($"Disconnected graph, endpoint unreachable for radius = {radius}");
                return [];
            }
            int i = list.FindIndex(e => e.distance == minD);
            
            // mark cell as visited
            list[i] = (list[i].prev, list[i].distance, false);      
            
            // update path costs for all neighbors of current cell
            for (int j = i + 1; j < cells.Count; j++)
            {
                double newD = minD + connectionMatrix[i, j];
                if (newD < list[j].distance)                        // check if path is better
                    list[j] = (i, newD, list[j].unvisited);
            }
        }

        // assemble points along path
        List<Point> path = new List<Point>(){ cells.Last() };
        int index = cells.Count - 1;
        do
        {
            index = list[index].prev;
            path.Add(cells[index]);
        }
        while (index != 0);

        if (oddCenter)
            path.Add(new Point(path.Last().X - 1, path.Last().Y));

        // mirror path to bottom half
        List<Point> path2 = new List<Point>();
        foreach (var p in path)
            path2.Add(new Point(p.Y, p.X));
        path.AddRange(path2);

        return LineToWedges(path, narrow,outsideEdge);
    }

    // expand line segments to wedges and blocks
    private static Point[] LineToWedges(List<Point> cells, bool narrow, bool outsideEdge)
    {
        cells.Sort((l, r) => (l.X - l.Y).CompareTo(r.X - r.Y));
        
        var array = cells.ToHashSet().ToArray();    // filter duplicates with hashset
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
