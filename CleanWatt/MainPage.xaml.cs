using BatteryWidget.Models;
using BatteryWidget.Resources.Strings;
using BatteryWidget.Services;

namespace BatteryWidget;

public partial class MainPage : ContentPage
{
    private static string[] LanguageCodes => LocalizationService.SupportedLanguages;

    public MainPage()
    {
        InitializeComponent();
        InitializeSettings();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        MessagingCenter.Subscribe<object>(this, "BatteryChanged", (sender) =>
        {
            MainThread.BeginInvokeOnMainThread(UpdateBatteryInfo);
        });
        MessagingCenter.Subscribe<object>(this, "BluetoothPermissionGranted", (sender) =>
        {
            MainThread.BeginInvokeOnMainThread(UpdateBluetoothDevices);
        });
        UpdateLocalizedStrings();
        UpdateBatteryInfo();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        MessagingCenter.Unsubscribe<object>(this, "BatteryChanged");
        MessagingCenter.Unsubscribe<object>(this, "BluetoothPermissionGranted");
    }

    private void InitializeSettings()
    {
        foreach (var name in LocalizationService.SupportedLanguageNames)
            LanguagePicker.Items.Add(name);

        var currentLang = LocalizationService.GetLanguage();
        int langIndex = Array.IndexOf(LanguageCodes, currentLang);
        LanguagePicker.SelectedIndex = langIndex >= 0 ? langIndex : 0;

        FahrenheitSwitch.IsToggled = LocalizationService.UseFahrenheit;
    }

    private void UpdateLocalizedStrings()
    {
        HeaderLabel.Text = AppStrings.Battery;
        StatusHeaderLabel.Text = AppStrings.Status;
        TempHeaderLabel.Text = AppStrings.Temperature;
        HealthHeaderLabel.Text = AppStrings.BatteryHealth;
        WidgetHeaderLabel.Text = AppStrings.Widget;
        WidgetInstructionsLabel.Text = AppStrings.WidgetInstructions;
        SettingsHeaderLabel.Text = AppStrings.Settings;
        LanguageLabel.Text = AppStrings.Language;
        TempUnitLabel.Text = AppStrings.TemperatureUnit;
        DevicesHeaderLabel.Text = AppStrings.Devices;
    }

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        if (LanguagePicker.SelectedIndex < 0 || LanguagePicker.SelectedIndex >= LanguageCodes.Length)
            return;

        var code = LanguageCodes[LanguagePicker.SelectedIndex];
        LocalizationService.SetLanguage(code);
        UpdateLocalizedStrings();
        UpdateBatteryInfo();
        TriggerWidgetUpdate();
    }

    private void OnFahrenheitToggled(object? sender, ToggledEventArgs e)
    {
        LocalizationService.UseFahrenheit = e.Value;
        UpdateBatteryInfo();
        TriggerWidgetUpdate();
    }

    private void UpdateBatteryInfo()
    {
#if ANDROID
        var context = Platform.CurrentActivity ?? Android.App.Application.Context;
        var batteryInfo = Services.BatteryService.GetBatteryInfo(context);

        LevelLabel.Text = $"{batteryInfo.Level}%";
        StatusLabel.Text = batteryInfo.StatusText;
        TempLabel.Text = LocalizationService.FormatTemperature(batteryInfo.Temperature);
        HealthLabel.Text = batteryInfo.HealthText;

        LevelLabel.TextColor = batteryInfo.Level switch
        {
            <= 20 => Color.FromArgb("#F44336"),
            <= 50 => Color.FromArgb("#FF9800"),
            _ => Color.FromArgb("#4CAF50")
        };

        UpdateBluetoothDevices();
#endif
    }

    private void UpdateBluetoothDevices()
    {
#if ANDROID
        var context = Platform.CurrentActivity ?? Android.App.Application.Context;
        var devices = Services.BluetoothBatteryService.GetConnectedDevices(context);

        DevicesContainer.Children.Clear();

        if (devices.Count == 0)
        {
            DevicesFrame.IsVisible = false;
            return;
        }

        DevicesFrame.IsVisible = true;

        foreach (var device in devices)
        {
            var row = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition(new GridLength(30)),
                    new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
                    new ColumnDefinition(new GridLength(50))
                },
                RowDefinitions =
                {
                    new RowDefinition(GridLength.Auto),
                    new RowDefinition(GridLength.Auto)
                },
                ColumnSpacing = 10,
                RowSpacing = 2
            };

            var iconLabel = new Label
            {
                Text = device.DeviceIcon,
                FontSize = 20,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center
            };
            Grid.SetColumn(iconLabel, 0);
            Grid.SetRowSpan(iconLabel, 2);

            var nameLabel = new Label
            {
                Text = device.Name,
                TextColor = Colors.White,
                FontSize = 14,
                LineBreakMode = LineBreakMode.TailTruncation
            };
            Grid.SetColumn(nameLabel, 1);
            Grid.SetRow(nameLabel, 0);

            var levelColor = device.BatteryLevel switch
            {
                <= 15 => Color.FromArgb("#F44336"),
                <= 30 => Color.FromArgb("#FF5722"),
                <= 50 => Color.FromArgb("#FF9800"),
                _ => Color.FromArgb("#4CAF50")
            };

            var progressBar = new ProgressBar
            {
                Progress = device.BatteryLevel / 100.0,
                ProgressColor = levelColor,
                HeightRequest = 4
            };
            Grid.SetColumn(progressBar, 1);
            Grid.SetRow(progressBar, 1);

            var levelLabel = new Label
            {
                Text = $"{device.BatteryLevel}%",
                TextColor = levelColor,
                FontSize = 14,
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.Center
            };
            Grid.SetColumn(levelLabel, 2);
            Grid.SetRowSpan(levelLabel, 2);

            row.Children.Add(iconLabel);
            row.Children.Add(nameLabel);
            row.Children.Add(progressBar);
            row.Children.Add(levelLabel);

            DevicesContainer.Children.Add(row);
        }
#endif
    }

    private void TriggerWidgetUpdate()
    {
#if ANDROID
        var context = Platform.CurrentActivity ?? Android.App.Application.Context;
        var appWidgetManager = Android.Appwidget.AppWidgetManager.GetInstance(context);
        if (appWidgetManager == null) return;

        var componentName = new Android.Content.ComponentName(context,
            Java.Lang.Class.FromType(typeof(BatteryWidget.Platforms.Android.Widgets.CleanWattWidget)));
        var widgetIds = appWidgetManager.GetAppWidgetIds(componentName);

        if (widgetIds != null)
        {
            foreach (var widgetId in widgetIds)
            {
                BatteryWidget.Platforms.Android.Widgets.CleanWattWidget.UpdateWidget(context, appWidgetManager, widgetId);
            }
        }
#endif
    }
}
