using System;
using System.Collections.ObjectModel;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Timers;
using Timer = System.Timers.Timer;

namespace MciClock.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private Timer? _timer;
    private TimeSpan _stopwatchRemaining;
    private TimeSpan _stopwatchTotal;
    private bool _isRunning;

    [ObservableProperty]
    private string _currentTime = DateTime.Now.ToString("HH:mm:ss");

    [ObservableProperty]
    private string _currentDate = DateTime.Now.ToString("dddd, dd 'de' MMMM 'de' yyyy", new System.Globalization.CultureInfo("pt-BR"));

    [ObservableProperty]
    private bool _isClockMode = true;

    [ObservableProperty]
    private bool _isStopwatchMode;

    [ObservableProperty]
    private string _stopwatchDisplay = "00:00:00";

    [ObservableProperty]
    private string _statusText = "Relógio ativo";

    [ObservableProperty]
    private bool _isStopwatchRunning;

    [ObservableProperty]
    private bool _isStopwatchPaused;

    [ObservableProperty]
    private decimal _stopwatchHours;

    [ObservableProperty]
    private decimal _stopwatchMinutes = 5;

    [ObservableProperty]
    private decimal _stopwatchSeconds;

    [ObservableProperty]
    private decimal _alertMinutes = 3;

    [ObservableProperty]
    private double _alertOpacity = 1.0;

    [ObservableProperty]
    private bool _isAlerting;

    [ObservableProperty]
    private bool _isTimeExpired;

    [ObservableProperty]
    private bool _showDisplayWindow;

    [ObservableProperty]
    private bool _displayWindowOpen;

    [ObservableProperty]
    private int _selectedScreenIndex;

    [ObservableProperty]
    private ObservableCollection<string> _availableScreens = new();

    [ObservableProperty]
    private double _progressValue;

    [ObservableProperty]
    private double _progressMaximum = 1.0;

    [ObservableProperty]
    private bool _showOnSecondScreen = true;

    // Events for the View layer
    public event Action? RequestOpenDisplayWindow;
    public event Action? RequestCloseDisplayWindow;
    public event Action? DisplayStateChanged;

    public MainViewModel()
    {
        StartClockTimer();
    }

    private void StartClockTimer()
    {
        _timer = new Timer(100);
        _timer.Elapsed += OnTimerTick;
        _timer.Start();
    }

    private void OnTimerTick(object? sender, ElapsedEventArgs e)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            CurrentTime = DateTime.Now.ToString("HH:mm:ss");
            CurrentDate = DateTime.Now.ToString("dddd, dd 'de' MMMM 'de' yyyy", new System.Globalization.CultureInfo("pt-BR"));

            if (_isRunning && IsStopwatchMode)
            {
                _stopwatchRemaining = _stopwatchRemaining.Subtract(TimeSpan.FromMilliseconds(100));

                if (_stopwatchRemaining.TotalMilliseconds <= 0)
                {
                    _stopwatchRemaining = TimeSpan.Zero;
                    _isRunning = false;
                    IsStopwatchRunning = false;
                    IsStopwatchPaused = false;
                    IsTimeExpired = true;
                    StatusText = "⏱ Tempo esgotado!";
                }

                StopwatchDisplay = _stopwatchRemaining.ToString(@"hh\:mm\:ss");

                // Progress
                if (_stopwatchTotal.TotalSeconds > 0)
                {
                    ProgressValue = 1.0 - (_stopwatchRemaining.TotalSeconds / _stopwatchTotal.TotalSeconds);
                }

                // Alert logic
                var alertThreshold = TimeSpan.FromMinutes((double)AlertMinutes);
                if (_stopwatchRemaining > TimeSpan.Zero && _stopwatchRemaining <= alertThreshold)
                {
                    IsAlerting = true;
                    // Smooth sine-wave pulsing between 0.3 and 1.0
                    double totalAlertMs = alertThreshold.TotalMilliseconds;
                    double remainingMs = _stopwatchRemaining.TotalMilliseconds;
                    double phase = (totalAlertMs - remainingMs) / 1000.0 * Math.PI;
                    AlertOpacity = 0.3 + 0.7 * ((Math.Sin(phase) + 1.0) / 2.0);
                }
                else if (_stopwatchRemaining <= TimeSpan.Zero)
                {
                    // Finished — keep flashing red
                    IsAlerting = true;
                    double phase = Environment.TickCount64 / 500.0 * Math.PI;
                    AlertOpacity = 0.3 + 0.7 * ((Math.Sin(phase) + 1.0) / 2.0);
                }
                else
                {
                    IsAlerting = false;
                    AlertOpacity = 1.0;
                }
            }

            // Keep flashing after expired even when not running
            if (IsTimeExpired && !_isRunning)
            {
                IsAlerting = true;
                double phase = Environment.TickCount64 / 500.0 * Math.PI;
                AlertOpacity = 0.3 + 0.7 * ((Math.Sin(phase) + 1.0) / 2.0);
            }

            // Always notify display so clock mode stays updated
            DisplayStateChanged?.Invoke();
        });
    }

    [RelayCommand]
    private void SwitchToClock()
    {
        IsClockMode = true;
        IsStopwatchMode = false;
        StopStopwatch();
        StatusText = "Relógio ativo";
        DisplayStateChanged?.Invoke();
    }

    [RelayCommand]
    private void SwitchToStopwatch()
    {
        IsClockMode = false;
        IsStopwatchMode = true;
        StatusText = "Cronômetro regressivo — configure o tempo e inicie";
        ResetStopwatch();
        DisplayStateChanged?.Invoke();
    }

    [RelayCommand]
    private void StartStopwatch()
    {
        if (!IsStopwatchRunning || IsStopwatchPaused)
        {
            if (!IsStopwatchRunning)
            {
                _stopwatchRemaining = new TimeSpan((int)StopwatchHours, (int)StopwatchMinutes, (int)StopwatchSeconds);
                _stopwatchTotal = _stopwatchRemaining;

                if (_stopwatchRemaining.TotalSeconds <= 0)
                {
                    StatusText = "⚠ Defina um tempo maior que zero!";
                    return;
                }

                ProgressMaximum = 1.0;
                ProgressValue = 0;
            }

            _isRunning = true;
            IsStopwatchRunning = true;
            IsStopwatchPaused = false;
            StatusText = "⏱ Cronômetro em execução...";
            DisplayStateChanged?.Invoke();
        }
    }

    [RelayCommand]
    private void PauseStopwatch()
    {
        if (_isRunning)
        {
            _isRunning = false;
            IsStopwatchPaused = true;
            StatusText = "⏸ Cronômetro pausado";
            DisplayStateChanged?.Invoke();
        }
    }

    [RelayCommand]
    private void StopStopwatch()
    {
        _isRunning = false;
        IsStopwatchRunning = false;
        IsStopwatchPaused = false;
        IsAlerting = false;
        IsTimeExpired = false;
        AlertOpacity = 1.0;
        ProgressValue = 0;
        StopwatchDisplay = "00:00:00";
        _stopwatchRemaining = TimeSpan.Zero;
        StatusText = IsStopwatchMode ? "Cronômetro regressivo — configure o tempo e inicie" : "Relógio ativo";
        DisplayStateChanged?.Invoke();
    }

    [RelayCommand]
    private void ResetStopwatch()
    {
        StopStopwatch();
        StopwatchDisplay = new TimeSpan((int)StopwatchHours, (int)StopwatchMinutes, (int)StopwatchSeconds).ToString(@"hh\:mm\:ss");
        DisplayStateChanged?.Invoke();
    }

    [RelayCommand]
    private void OpenDisplayWindow()
    {
        DisplayWindowOpen = true;
        RequestOpenDisplayWindow?.Invoke();
    }

    [RelayCommand]
    private void CloseDisplayWindow()
    {
        DisplayWindowOpen = false;
        RequestCloseDisplayWindow?.Invoke();
    }

    public void OnDisplayWindowClosed()
    {
        DisplayWindowOpen = false;
    }

    public void RefreshScreens(System.Collections.Generic.List<string> screens)
    {
        AvailableScreens.Clear();
        for (int i = 0; i < screens.Count; i++)
        {
            AvailableScreens.Add(screens[i]);
        }

        if (screens.Count > 1 && ShowOnSecondScreen)
        {
            SelectedScreenIndex = 1;
        }
        else
        {
            SelectedScreenIndex = 0;
        }
    }

    public void Cleanup()
    {
        _timer?.Stop();
        _timer?.Dispose();
    }
}
