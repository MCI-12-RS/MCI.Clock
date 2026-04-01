using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using MciClock.ViewModels;

namespace MciClock.Views;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;
    private ClockDisplayWindow? _displayWindow;

    public MainWindow()
    {
        InitializeComponent();

        _viewModel = new MainViewModel();
        DataContext = _viewModel;

        _viewModel.RequestOpenDisplayWindow += OnRequestOpenDisplayWindow;
        _viewModel.RequestCloseDisplayWindow += OnRequestCloseDisplayWindow;

        Loaded += (_, _) => RefreshScreenList();
        Closing += (_, _) =>
        {
            _displayWindow?.Close();
            _viewModel.Cleanup();
        };
    }

    private void RefreshScreenList()
    {
        var screens = Screens.All.ToList();
        var screenNames = new List<string>();

        for (int i = 0; i < screens.Count; i++)
        {
            var s = screens[i];
            var primary = s.IsPrimary ? " (Principal)" : "";
            screenNames.Add($"Tela {i + 1}: {s.Bounds.Width}x{s.Bounds.Height}{primary}");
        }

        _viewModel.RefreshScreens(screenNames);
    }

    private void OnRequestOpenDisplayWindow()
    {
        if (_displayWindow != null)
        {
            _displayWindow.Activate();
            return;
        }

        _displayWindow = new ClockDisplayWindow(_viewModel);
        _displayWindow.Closed += (_, _) =>
        {
            _viewModel.OnDisplayWindowClosed();
            _displayWindow = null;
        };

        // Position on selected screen
        var screens = Screens.All.ToList();
        var selectedIdx = _viewModel.SelectedScreenIndex;

        if (selectedIdx >= 0 && selectedIdx < screens.Count)
        {
            var targetScreen = screens[selectedIdx];
            _displayWindow.Position = new PixelPoint(
                (int)targetScreen.Bounds.X,
                (int)targetScreen.Bounds.Y);
        }

        _displayWindow.Show();
        _displayWindow.WindowState = WindowState.Maximized;
    }

    private void OnRequestCloseDisplayWindow()
    {
        _displayWindow?.Close();
        _displayWindow = null;
    }
}
