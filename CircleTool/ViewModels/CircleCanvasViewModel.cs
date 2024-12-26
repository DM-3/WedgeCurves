using Avalonia;
using Avalonia.Media.TextFormatting;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        UpdateCircle();
        UpdateCells();
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
        UpdateCircle();
        UpdatePoints();
    }

    private double pxPerCell() => Size / ((double)CellCount);


    // true circle

    [ObservableProperty]
    private double _circleSize;
    [ObservableProperty]
    private double _circleView;
    [ObservableProperty]
    private double _circleOffset;

    public void UpdateCircle()
    {
        CircleSize = 2 * Radius * pxPerCell();
        CircleView = (Radius + .5) * pxPerCell();
        CircleOffset = Math.Max(Size - CircleView, 0.0);
    }


    // approximated circle

    private Point[] _cells = [];
    
    public void UpdateCells()
    {
        _cells = Logic.CircleApproximator.WedgesMax(Radius, Narrow);
        UpdatePoints();
    }

    private bool _narrow = false;
    public bool Narrow
    { 
        get => _narrow; 
        set
        {
            _narrow = value;
            UpdateCells();
        }
    }


    [ObservableProperty]
    private Point[] _points = [];

    public void UpdatePoints()
    {
        Point offset = new Point(CellCount - 0.5, CellCount - 0.5);
        Func<Point, Point> trans = p => (offset - p) * pxPerCell();
        Points = (from p in _cells select trans(p)).ToArray();
    }
}
