using BatteryWidget.Models;

namespace BatteryWidget;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        UpdateBatteryInfo();
    }

    private void UpdateBatteryInfo()
    {
#if ANDROID
        var context = Platform.CurrentActivity ?? Android.App.Application.Context;
        var batteryInfo = Services.BatteryService.GetBatteryInfo(context);

        LevelLabel.Text = $"{batteryInfo.Level}%";
        StatusLabel.Text = batteryInfo.StatusText;
        TempLabel.Text = $"{batteryInfo.Temperature:F1}°C";
        HealthLabel.Text = batteryInfo.HealthText;

        LevelLabel.TextColor = batteryInfo.Level switch
        {
            <= 20 => Color.FromArgb("#F44336"),
            <= 50 => Color.FromArgb("#FF9800"),
            _ => Color.FromArgb("#4CAF50")
        };
#endif
    }
}
