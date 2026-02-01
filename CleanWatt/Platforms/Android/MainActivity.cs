using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;

namespace BatteryWidget;

[Activity(
    Theme = "@style/Maui.SplashTheme",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode |
                          ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnResume()
    {
        base.OnResume();
        StartBatteryService();
    }

    private void StartBatteryService()
    {
        var serviceIntent = new Intent(this, typeof(Platforms.Android.BatteryUpdateService));

        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            StartForegroundService(serviceIntent);
        }
        else
        {
            StartService(serviceIntent);
        }
    }
}
