using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CircleTool.ViewModels;

public partial class CircleCanvasViewModel : ViewModelBase
{
    // canvas width and height in pixels
    public int Size { get; set; }

    // circle radius in cell units
    [ObservableProperty]
    private double _radius = 10;
    partial void OnRadiusChanged(double value)
    {
        updateCircle();
        updatePoints();
    }

    // maximum circle radius in cell units
    [ObservableProperty]
    private double _maxRadius = 15.5;

    // cells per canvas width or height
    [ObservableProperty]
    private int _cellCount = 16;
    partial void OnCellCountChanged(int value)
    {
        MaxRadius = value - .5;
        updateCircle();
        updatePoints();
    }

    private double pxPerCell() => Size / ((double)CellCount);


    // true circle

    [ObservableProperty]
    private double _circleSize;
    [ObservableProperty]
    private double _circleView;
    [ObservableProperty]
    private double _circleOffset;

    public void updateCircle()
    {
        CircleSize = 2 * Radius * pxPerCell();
        CircleView = (Radius + .5) * pxPerCell();
        CircleOffset = Math.Max(Size - CircleView, 0.0);
    }


    // approximated circle

    private List<Point> _cells = 
        new List<Point>(
        [
            new Point(0,  0),
            new Point(0,  10),
            new Point(4,  9),
            new Point(6,  8),
            new Point(8,  6),
            new Point(9,  4),
            new Point(10, 0)
        ]);
    
    [ObservableProperty]
    private List<Point> _points = new List<Point>();

    public void updatePoints()
    {
        double offset = CellCount - 0.5;
        Points = new List<Point>(from p in _cells select 
            (new Point(offset, offset) - p * Radius / 10) * pxPerCell());
    }
}
