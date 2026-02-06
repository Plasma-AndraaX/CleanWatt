using System.Text;
using BatteryWidget.Models;
using BatteryWidget.Resources.Strings;
using AndroidColor = global::Android.Graphics.Color;
using AndroidViewStates = global::Android.Views.ViewStates;

namespace BatteryWidget.Platforms.Android.Widgets;

public static class WidgetHelper
{
    // View visibility constants
    public static readonly AndroidViewStates ViewVisible = AndroidViewStates.Visible;
    public static readonly AndroidViewStates ViewGone = AndroidViewStates.Gone;

    // Colors based on battery level
    public static AndroidColor GetBatteryColor(int level)
    {
        return level switch
        {
            <= 15 => AndroidColor.ParseColor("#F44336"),  // Red - Critical
            <= 30 => AndroidColor.ParseColor("#FF5722"),  // Deep Orange - Low
            <= 50 => AndroidColor.ParseColor("#FF9800"),  // Orange - Medium-Low
            <= 75 => AndroidColor.ParseColor("#8BC34A"),  // Light Green - Medium-High
            _ => AndroidColor.ParseColor("#4CAF50")       // Green - Good
        };
    }

    public static string GetBatteryColorHex(int level)
    {
        return level switch
        {
            <= 15 => "#F44336",  // Red - Critical
            <= 30 => "#FF5722",  // Deep Orange - Low
            <= 50 => "#FF9800",  // Orange - Medium-Low
            <= 75 => "#8BC34A",  // Light Green - Medium-High
            _ => "#4CAF50"       // Green - Good
        };
    }

    public static AndroidColor GetBackgroundColor(int level)
    {
        return level switch
        {
            <= 15 => AndroidColor.ParseColor("#33F44336"),  // Red transparent
            <= 30 => AndroidColor.ParseColor("#33FF5722"),  // Deep Orange transparent
            <= 50 => AndroidColor.ParseColor("#33FF9800"),  // Orange transparent
            <= 75 => AndroidColor.ParseColor("#338BC34A"),  // Light Green transparent
            _ => AndroidColor.ParseColor("#334CAF50")       // Green transparent
        };
    }

    public static string GetStatusIcon(BatteryInfo info)
    {
        if (info.IsCharging)
            return "⚡";
        if (info.IsPowerSaveMode)
            return "🔋";
        return "";
    }

    public static string GetCompactStatus(BatteryInfo info)
    {
        var parts = new List<string>();

        if (info.IsCharging)
            parts.Add("⚡");
        if (info.IsPowerSaveMode)
            parts.Add(AppStrings.PowerSaveShort);

        return parts.Count > 0 ? string.Join(" ", parts) : "";
    }

    public static string FormatCompactDeviceList(List<BluetoothDeviceInfo> devices, int max = 3)
    {
        if (devices.Count == 0)
            return "";

        var sb = new StringBuilder();
        int count = Math.Min(devices.Count, max);

        for (int i = 0; i < count; i++)
        {
            if (i > 0) sb.Append("  ");
            sb.Append($"{devices[i].DeviceIcon}{devices[i].BatteryLevel}%");
        }

        if (devices.Count > max)
            sb.Append($"  +{devices.Count - max}");

        return sb.ToString();
    }
}
