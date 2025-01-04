using Avalonia;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace CircleTool.ViewModels;

public enum ApproximationMode
{
    Rasterize,
    Max,
    Min
}

public partial class CircleCanvasViewModel : ViewModelBase
{
    // canvas width and height in pixels
    public int Size { get; set; }

    // circle radius in cell units
    [ObservableProperty] private double _radius = 10;
    partial void OnRadiusChanged(double value)
    {
        UpdateCircle();
        UpdateCells();
    }

    // maximum circle radius in cell units
    [ObservableProperty] private double _maxRadius = 15.5;
    void UpdateMaxRadius() => MaxRadius = CellCount - .5 * Convert.ToDouble(OddCenter);

    // cells per canvas width or height
    [ObservableProperty] private int _cellCount = 16;
    partial void OnCellCountChanged(int value)
    {
        UpdateMaxRadius();
        UpdateGrid();
        UpdateCircle();
        UpdatePoints();
    }

    private double pxPerCell() => Size / ((double)CellCount);


    // true circle

    [ObservableProperty] private double _circleSize;
    [ObservableProperty] private double _circleView;
    [ObservableProperty] private double _circleOffset;
    [ObservableProperty] private double _circleCenter = 50;
    public void UpdateCircle()
    {
        CircleSize = 2 * Radius * pxPerCell();
        CircleView = (Radius + .5 * Convert.ToDouble(OddCenter)) * pxPerCell();
        CircleOffset = Math.Max(Size - CircleView, 0.0);
        CircleCenter = .5 * pxPerCell() * Convert.ToDouble(OddCenter) - 10;
    }


    // approximated circle

    [ObservableProperty] private ApproximationMode _selectedMode = ApproximationMode.Max;
    partial void OnSelectedModeChanged(ApproximationMode value) => UpdateCells();

    [ObservableProperty] private bool _outsideEdge = true;
    partial void OnOutsideEdgeChanged(bool value) => UpdateCells();

    [ObservableProperty] private bool _narrow = false;
    partial void OnNarrowChanged(bool value) => UpdateCells();

    [ObservableProperty] private bool _oddCenter = true;
    partial void OnOddCenterChanged(bool value)
    {
        UpdateMaxRadius();
        UpdateCircle();
        UpdateCells();
    }

    private Point[] _cells = [];
    
    public void UpdateCells()
    {
        Func<double, bool, bool, bool, Point[]> Gen = SelectedMode switch
        {
            ApproximationMode.Rasterize => Logic.CircleApproximator.Rasterize,
            ApproximationMode.Max       => Logic.CircleApproximator.WedgesMax,
            ApproximationMode.Min       => Logic.CircleApproximator.WedgesMin,
            _ => throw new NotImplementedException()
        };
        _cells = Gen(Radius, OddCenter, Narrow, OutsideEdge);

        UpdatePoints();
    }

    [ObservableProperty] private Point[] _points = [];

    public void UpdatePoints()
    {
        double offset = CellCount - .5 * Convert.ToDouble(OddCenter);
        Func<Point, Point> trans = 
            p => (new Point(offset, offset) - p) * pxPerCell();
        Points = (from p in _cells select trans(p)).ToArray();
    }


    // grid
    
    public ObservableCollection<Line> VerticalGridLines { get; }
        = new ObservableCollection<Line>();
    public ObservableCollection<Line> HorizontalGridLines { get; }
        = new ObservableCollection<Line>();
    public void UpdateGrid()
    {
        VerticalGridLines.Clear();
        HorizontalGridLines.Clear();

        double tStep = pxPerCell();
        uint counter = 0;
        for (double t = tStep; t < Size; t += tStep)
        {
            uint step = Convert.ToUInt32(t) - counter;
            counter += step;

            var line = new Line();
            line.Stroke = Brushes.LightGray;
            line.StrokeThickness = 1;
            line.StartPoint = new Point(0, step);
            line.EndPoint = new Point(Size, step);
            VerticalGridLines.Add(line);

            line = new Line();
            line.Stroke = Brushes.LightGray;
            line.StrokeThickness = 1;
            line.StartPoint = new Point(step, 0);
            line.EndPoint = new Point(step, Size);
            HorizontalGridLines.Add(line);
        }
    }

    [ObservableProperty]
    private bool _showGrid = true;
    partial void OnShowGridChanged(bool value)
    {
        if (value)
            UpdateGrid();
        else
        {
            VerticalGridLines.Clear();
            HorizontalGridLines.Clear();
        }
    }
}
