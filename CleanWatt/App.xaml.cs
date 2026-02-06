using BatteryWidget.Services;

namespace BatteryWidget;

public partial class App : Application
{
    public App()
    {
        LocalizationService.ApplyLanguage();
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new MainPage());
    }
}
