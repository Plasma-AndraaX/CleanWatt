using BatteryWidget.Models;

namespace BatteryWidget;

public partial class MainPage : ContentPage
{
    private readonly IDispatcherTimer _timer;

    public MainPage()
    {
        InitializeComponent();

        _timer = Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(5);
        _timer.Tick += (s, e) => UpdateBatteryInfo();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        UpdateBatteryInfo();
        _timer.Start();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _timer.Stop();
    }

    private void UpdateBatteryInfo()
    {
#if ANDROID
        var context = Platform.CurrentActivity ?? Android.App.Application.Context;
        var batteryInfo = Services.BatteryService.GetBatteryInfo(context);

        MainThread.BeginInvokeOnMainThread(() =>
        {
            LevelLabel.Text = $"{batteryInfo.Level}%";
            StatusLabel.Text = batteryInfo.StatusText;
            TempLabel.Text = $"{batteryInfo.Temperature:F1}°C";
            HealthLabel.Text = batteryInfo.HealthText;

            // Change color based on level
            LevelLabel.TextColor = batteryInfo.Level switch
            {
                <= 20 => Color.FromArgb("#F44336"),
                <= 50 => Color.FromArgb("#FF9800"),
                _ => Color.FromArgb("#4CAF50")
            };
        });
#endif
    }
}
