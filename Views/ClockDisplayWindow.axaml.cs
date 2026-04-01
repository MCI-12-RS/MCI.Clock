using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using MciClock.ViewModels;

namespace MciClock.Views;

public partial class ClockDisplayWindow : Window
{
    private MainViewModel _viewModel = null!;
    private Border _alertBackground = null!;
    private TextBlock _clockTimeText = null!;
    private TextBlock _clockDateText = null!;
    private TextBlock _modeLabel = null!;

    private static readonly IBrush NormalForeground = new SolidColorBrush(Color.Parse("#FFFFFF"));
    private static readonly IBrush AlertForeground = new SolidColorBrush(Color.Parse("#e74c3c"));
    private static readonly IBrush DateForeground = new SolidColorBrush(Color.Parse("#7ba7bc"));
    private static readonly IBrush MutedForeground = new SolidColorBrush(Color.Parse("#4a7080"));
    private static readonly IBrush NormalBackground = new SolidColorBrush(Color.Parse("#0a1e2b"));
    private static readonly IBrush AlertBackgroundBrush = new SolidColorBrush(Color.Parse("#e74c3c"));

    public ClockDisplayWindow()
    {
        InitializeComponent();
    }

    public ClockDisplayWindow(MainViewModel viewModel) : this()
    {
        _viewModel = viewModel;
        _alertBackground = this.FindControl<Border>("AlertBackground")!;
        _clockTimeText = this.FindControl<TextBlock>("ClockTimeText")!;
        _clockDateText = this.FindControl<TextBlock>("ClockDateText")!;
        _modeLabel = this.FindControl<TextBlock>("ModeLabel")!;

        _viewModel.DisplayStateChanged += OnDisplayStateChanged;

        // ESC to close
        KeyDown += (_, e) =>
        {
            if (e.Key == Key.Escape)
                Close();
        };

        // Re-assert topmost whenever the window loses focus
        Deactivated += (_, _) =>
        {
            Topmost = false;
            Topmost = true;
        };

        // Initial update
        Loaded += (_, _) => UpdateDisplay();
    }

    private void OnDisplayStateChanged()
    {
        Dispatcher.UIThread.InvokeAsync(UpdateDisplay);
    }

    private void UpdateDisplay()
    {
        if (_viewModel == null) return;

        if (_viewModel.IsClockMode)
        {
            _clockTimeText.Text = _viewModel.CurrentTime;
            _clockDateText.Text = _viewModel.CurrentDate;
            _clockDateText.IsVisible = true;
            _modeLabel.Text = "RELÓGIO";
            _modeLabel.Foreground = MutedForeground;
            _clockTimeText.Foreground = NormalForeground;
            _clockTimeText.Opacity = 1.0;
            _alertBackground.Opacity = 0;
        }
        else if (_viewModel.IsStopwatchMode)
        {
            _clockTimeText.Text = _viewModel.StopwatchDisplay;
            _clockDateText.IsVisible = false;

            // Alert / expired effect
            if (_viewModel.IsAlerting)
            {
                double opacity = _viewModel.AlertOpacity;

                // Fade text opacity for pulse effect
                _clockTimeText.Foreground = AlertForeground;
                _clockTimeText.Opacity = 0.4 + 0.6 * opacity;

                // Background red pulse
                _alertBackground.Opacity = (1.0 - opacity) * 0.18;

                if (_viewModel.IsTimeExpired)
                {
                    _modeLabel.Text = "⚠ TEMPO ESGOTADO";
                }
                else
                {
                    _modeLabel.Text = "⚠ TEMPO ACABANDO";
                }
                _modeLabel.Foreground = AlertForeground;
            }
            else
            {
                _clockTimeText.Foreground = NormalForeground;
                _clockTimeText.Opacity = 1.0;
                _alertBackground.Opacity = 0;
                _modeLabel.Foreground = MutedForeground;

                if (_viewModel.IsStopwatchRunning && !_viewModel.IsStopwatchPaused)
                {
                    _modeLabel.Text = "CRONÔMETRO";
                }
                else if (_viewModel.IsStopwatchPaused)
                {
                    _modeLabel.Text = "⏸ PAUSADO";
                }
                else
                {
                    _modeLabel.Text = "CRONÔMETRO";
                }
            }
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        _viewModel.DisplayStateChanged -= OnDisplayStateChanged;
        base.OnClosed(e);
    }
}
