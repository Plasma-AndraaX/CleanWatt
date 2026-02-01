#if ANDROID
using Android.Content;
using Android.OS;
#endif
using BatteryWidget.Models;

namespace BatteryWidget.Services;

public class BatteryService
{
#if ANDROID
    public static BatteryInfo GetBatteryInfo(Context context)
    {
        var batteryIntent = context.RegisterReceiver(null, new IntentFilter(Intent.ActionBatteryChanged));

        if (batteryIntent == null)
        {
            return new BatteryInfo { Level = -1, Status = Models.BatteryStatus.Unknown };
        }

        int level = batteryIntent.GetIntExtra(BatteryManager.ExtraLevel, -1);
        int scale = batteryIntent.GetIntExtra(BatteryManager.ExtraScale, -1);
        int status = batteryIntent.GetIntExtra(BatteryManager.ExtraStatus, -1);
        int health = batteryIntent.GetIntExtra(BatteryManager.ExtraHealth, -1);
        int temperature = batteryIntent.GetIntExtra(BatteryManager.ExtraTemperature, -1);

        int levelPercent = (int)((level / (float)scale) * 100);

        // Check power save mode
        bool isPowerSaveMode = false;
        var powerManager = context.GetSystemService(Context.PowerService) as PowerManager;
        if (powerManager != null)
        {
            isPowerSaveMode = powerManager.IsPowerSaveMode;
        }

        return new BatteryInfo
        {
            Level = levelPercent,
            Status = (Models.BatteryStatus)status,
            Health = (Models.BatteryHealth)health,
            Temperature = temperature / 10f,
            IsPowerSaveMode = isPowerSaveMode
        };
    }
#endif
}
