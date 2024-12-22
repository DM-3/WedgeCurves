using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Diagnostics;

namespace CircleTool.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    static private double CelciusToFahrenheit(double C) 
        => C * 9.0 / 5.0 + 32.0;

    [ObservableProperty]
    private double _celcius;
    partial void OnCelciusChanged(double value) 
        => Fahrenheit = CelciusToFahrenheit(value);

    [ObservableProperty]
    private double _fahrenheit;
}
