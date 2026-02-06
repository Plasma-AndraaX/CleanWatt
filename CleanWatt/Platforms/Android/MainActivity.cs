using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.App;
using AndroidX.Core.Content;

namespace BatteryWidget;

[Activity(
    Theme = "@style/Maui.SplashTheme",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode |
                          ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    private const int BluetoothPermissionRequestCode = 1001;

    protected override void OnResume()
    {
        base.OnResume();
        StartBatteryService();
        RequestBluetoothPermissionIfNeeded();
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

    private void RequestBluetoothPermissionIfNeeded()
    {
        if (Build.VERSION.SdkInt < BuildVersionCodes.S)
            return;

        if (ContextCompat.CheckSelfPermission(this, Android.Manifest.Permission.BluetoothConnect)
            == Permission.Granted)
            return;

        ActivityCompat.RequestPermissions(this,
            new[] { Android.Manifest.Permission.BluetoothConnect },
            BluetoothPermissionRequestCode);
    }

    public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
    {
        base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

        if (requestCode == BluetoothPermissionRequestCode
            && grantResults.Length > 0
            && grantResults[0] == Permission.Granted)
        {
            MessagingCenter.Send<object>(this, "BluetoothPermissionGranted");
        }
    }
}
