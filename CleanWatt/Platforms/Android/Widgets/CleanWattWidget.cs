using Android.App;
using Android.Appwidget;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Widget;
using BatteryWidget.Services;
using BatteryWidget.Platforms.Android.Widgets;

namespace BatteryWidget.Platforms.Android.Widgets;

[BroadcastReceiver(Label = "CleanWatt", Exported = true)]
[IntentFilter(new[] {
    "android.appwidget.action.APPWIDGET_UPDATE",
    Intent.ActionPowerConnected,
    Intent.ActionPowerDisconnected,
    PowerManager.ActionPowerSaveModeChanged
})]
[MetaData("android.appwidget.provider", Resource = "@xml/cleanwatt_widget_info")]
public class CleanWattWidget : AppWidgetProvider
{
    private enum WidgetSize
    {
        Small,      // 1x1 - percentage only
        Medium,     // 2x2 - percentage + status + progress
        Horizontal, // 4x1 - horizontal bar layout
        Large       // 4x2+ - full info
    }

    public override void OnUpdate(Context? context, AppWidgetManager? appWidgetManager, int[]? appWidgetIds)
    {
        if (context == null || appWidgetManager == null || appWidgetIds == null)
            return;

        foreach (int widgetId in appWidgetIds)
        {
            UpdateWidget(context, appWidgetManager, widgetId);
        }
    }

    public override void OnAppWidgetOptionsChanged(Context? context, AppWidgetManager? appWidgetManager, int appWidgetId, Bundle? newOptions)
    {
        base.OnAppWidgetOptionsChanged(context, appWidgetManager, appWidgetId, newOptions);

        if (context != null && appWidgetManager != null)
        {
            UpdateWidget(context, appWidgetManager, appWidgetId);
        }
    }

    public override void OnReceive(Context? context, Intent? intent)
    {
        base.OnReceive(context, intent);

        if (context == null || intent == null)
            return;

        // Update all widgets when battery state changes
        if (intent.Action == Intent.ActionPowerConnected ||
            intent.Action == Intent.ActionPowerDisconnected ||
            intent.Action == PowerManager.ActionPowerSaveModeChanged)
        {
            var appWidgetManager = AppWidgetManager.GetInstance(context);
            var componentName = new ComponentName(context, Java.Lang.Class.FromType(typeof(CleanWattWidget)));
            var widgetIds = appWidgetManager?.GetAppWidgetIds(componentName);

            if (widgetIds != null && appWidgetManager != null)
            {
                OnUpdate(context, appWidgetManager, widgetIds);
            }
        }
    }

    public static void UpdateWidget(Context context, AppWidgetManager appWidgetManager, int widgetId)
    {
        var batteryInfo = BatteryService.GetBatteryInfo(context);
        var size = GetWidgetSize(appWidgetManager, widgetId);
        var color = WidgetHelper.GetBatteryColor(batteryInfo.Level);

        RemoteViews views = size switch
        {
            WidgetSize.Small => BuildSmallLayout(context, batteryInfo, color),
            WidgetSize.Medium => BuildMediumLayout(context, batteryInfo, color),
            WidgetSize.Horizontal => BuildHorizontalLayout(context, batteryInfo, color),
            _ => BuildLargeLayout(context, batteryInfo, color)
        };

        // Set click intent to open app
        var intent = new Intent(context, typeof(MainActivity));
        var pendingIntent = PendingIntent.GetActivity(context, 0, intent,
            PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);
        views.SetOnClickPendingIntent(Resource.Id.widget_root, pendingIntent);

        appWidgetManager.UpdateAppWidget(widgetId, views);
    }

    private static WidgetSize GetWidgetSize(AppWidgetManager appWidgetManager, int widgetId)
    {
        var options = appWidgetManager.GetAppWidgetOptions(widgetId);
        int minWidth = options.GetInt(AppWidgetManager.OptionAppwidgetMinWidth);
        int minHeight = options.GetInt(AppWidgetManager.OptionAppwidgetMinHeight);

        // Determine layout based on dimensions (in dp)
        // Small: ~70x70, Medium: ~150x150, Horizontal: ~300x70, Large: ~300x150
        bool isWide = minWidth >= 180;
        bool isTall = minHeight >= 100;

        return (isWide, isTall) switch
        {
            (false, false) => WidgetSize.Small,      // 1x1
            (false, true) => WidgetSize.Medium,      // 1x2 or 2x2
            (true, false) => WidgetSize.Horizontal,  // 4x1
            (true, true) => WidgetSize.Large         // 4x2+
        };
    }

    private static RemoteViews BuildSmallLayout(Context context, Models.BatteryInfo batteryInfo, global::Android.Graphics.Color color)
    {
        var views = new RemoteViews(context.PackageName, Resource.Layout.widget_battery_small);

        views.SetTextViewText(Resource.Id.battery_level, $"{batteryInfo.Level}");
        views.SetTextColor(Resource.Id.battery_level, color);
        views.SetProgressBar(Resource.Id.battery_progress, 100, batteryInfo.Level, false);

        if (Build.VERSION.SdkInt >= BuildVersionCodes.S)
        {
            views.SetColorStateList(Resource.Id.battery_progress, "setProgressTintList",
                ColorStateList.ValueOf(color));
        }

        // Show charging or power save icon
        if (batteryInfo.IsCharging)
        {
            views.SetTextViewText(Resource.Id.battery_icon, "⚡");
            views.SetViewVisibility(Resource.Id.battery_icon, WidgetHelper.ViewVisible);
        }
        else if (batteryInfo.IsPowerSaveMode)
        {
            views.SetTextViewText(Resource.Id.battery_icon, "🔋");
            views.SetViewVisibility(Resource.Id.battery_icon, WidgetHelper.ViewVisible);
        }
        else
        {
            views.SetViewVisibility(Resource.Id.battery_icon, WidgetHelper.ViewGone);
        }

        return views;
    }

    private static RemoteViews BuildMediumLayout(Context context, Models.BatteryInfo batteryInfo, global::Android.Graphics.Color color)
    {
        var views = new RemoteViews(context.PackageName, Resource.Layout.widget_battery_medium);

        views.SetTextViewText(Resource.Id.battery_level, $"{batteryInfo.Level}%");
        views.SetTextColor(Resource.Id.battery_level, color);

        string statusText = batteryInfo.StatusText;
        if (batteryInfo.IsPowerSaveMode)
        {
            statusText += " • Éco";
        }
        views.SetTextViewText(Resource.Id.battery_status, statusText);

        views.SetProgressBar(Resource.Id.battery_progress, 100, batteryInfo.Level, false);

        if (Build.VERSION.SdkInt >= BuildVersionCodes.S)
        {
            views.SetColorStateList(Resource.Id.battery_progress, "setProgressTintList",
                ColorStateList.ValueOf(color));
        }

        if (batteryInfo.IsCharging)
        {
            views.SetTextViewText(Resource.Id.charging_icon, "⚡");
            views.SetViewVisibility(Resource.Id.charging_icon, WidgetHelper.ViewVisible);
        }
        else
        {
            views.SetViewVisibility(Resource.Id.charging_icon, WidgetHelper.ViewGone);
        }

        if (batteryInfo.IsPowerSaveMode)
        {
            views.SetViewVisibility(Resource.Id.powersave_icon, WidgetHelper.ViewVisible);
        }
        else
        {
            views.SetViewVisibility(Resource.Id.powersave_icon, WidgetHelper.ViewGone);
        }

        return views;
    }

    private static RemoteViews BuildHorizontalLayout(Context context, Models.BatteryInfo batteryInfo, global::Android.Graphics.Color color)
    {
        var views = new RemoteViews(context.PackageName, Resource.Layout.widget_battery_horizontal);

        views.SetTextViewText(Resource.Id.battery_level, $"{batteryInfo.Level}%");
        views.SetTextColor(Resource.Id.battery_level, color);

        views.SetTextViewText(Resource.Id.battery_status, batteryInfo.StatusText);
        views.SetTextViewText(Resource.Id.battery_temp, $"{batteryInfo.Temperature:F1}°C");
        views.SetTextViewText(Resource.Id.battery_health, batteryInfo.HealthText);

        views.SetProgressBar(Resource.Id.battery_progress, 100, batteryInfo.Level, false);

        if (Build.VERSION.SdkInt >= BuildVersionCodes.S)
        {
            views.SetColorStateList(Resource.Id.battery_progress, "setProgressTintList",
                ColorStateList.ValueOf(color));
        }

        if (batteryInfo.IsCharging)
        {
            views.SetTextViewText(Resource.Id.charging_icon, "⚡");
            views.SetViewVisibility(Resource.Id.charging_icon, WidgetHelper.ViewVisible);
        }
        else
        {
            views.SetViewVisibility(Resource.Id.charging_icon, WidgetHelper.ViewGone);
        }

        if (batteryInfo.IsPowerSaveMode)
        {
            views.SetTextViewText(Resource.Id.powersave_icon, "Éco");
            views.SetViewVisibility(Resource.Id.powersave_icon, WidgetHelper.ViewVisible);
        }
        else
        {
            views.SetViewVisibility(Resource.Id.powersave_icon, WidgetHelper.ViewGone);
        }

        return views;
    }

    private static RemoteViews BuildLargeLayout(Context context, Models.BatteryInfo batteryInfo, global::Android.Graphics.Color color)
    {
        var views = new RemoteViews(context.PackageName, Resource.Layout.widget_battery);

        views.SetTextViewText(Resource.Id.battery_level, $"{batteryInfo.Level}%");
        views.SetTextColor(Resource.Id.battery_level, color);

        views.SetTextViewText(Resource.Id.battery_status, batteryInfo.StatusText);
        views.SetTextViewText(Resource.Id.battery_temp, $"{batteryInfo.Temperature:F1}°C");
        views.SetTextViewText(Resource.Id.battery_health, batteryInfo.HealthText);

        views.SetProgressBar(Resource.Id.battery_progress, 100, batteryInfo.Level, false);

        if (Build.VERSION.SdkInt >= BuildVersionCodes.S)
        {
            views.SetColorStateList(Resource.Id.battery_progress, "setProgressTintList",
                ColorStateList.ValueOf(color));
        }

        if (batteryInfo.IsCharging)
        {
            views.SetTextViewText(Resource.Id.charging_icon, "⚡");
            views.SetViewVisibility(Resource.Id.charging_icon, WidgetHelper.ViewVisible);
        }
        else
        {
            views.SetViewVisibility(Resource.Id.charging_icon, WidgetHelper.ViewGone);
        }

        if (batteryInfo.IsPowerSaveMode)
        {
            views.SetTextViewText(Resource.Id.powersave_icon, "Éco");
            views.SetViewVisibility(Resource.Id.powersave_icon, WidgetHelper.ViewVisible);
        }
        else
        {
            views.SetViewVisibility(Resource.Id.powersave_icon, WidgetHelper.ViewGone);
        }

        return views;
    }
}
