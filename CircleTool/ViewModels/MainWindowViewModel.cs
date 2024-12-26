using CommunityToolkit.Mvvm.ComponentModel;

namespace CircleTool.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private CircleCanvasViewModel _canvas = new CircleCanvasViewModel();

    public void InitCanvas(int width, int height)
    {
        int minSize = width < height ? width : height;
        Canvas.Size = minSize - 120;

        Canvas.UpdateCircle();
        Canvas.UpdateCells();
        Canvas.UpdatePoints();
    }
}
