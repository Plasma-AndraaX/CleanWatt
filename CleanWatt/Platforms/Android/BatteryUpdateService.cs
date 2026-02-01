using Android.App;
using Android.Appwidget;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.App;
using BatteryWidget.Platforms.Android.Widgets;

namespace BatteryWidget.Platforms.Android;

[Service(Exported = false, ForegroundServiceType = ForegroundService.TypeSpecialUse)]
public class BatteryUpdateService : Service
{
    private const int NotificationId = 1001;
    private const string ChannelId = "battery_widget_channel";
    private BatteryReceiver? _batteryReceiver;

    public override IBinder? OnBind(Intent? intent) => null;

    public override void OnCreate()
    {
        base.OnCreate();
        CreateNotificationChannel();

        // Register for battery change broadcasts (real-time updates)
        _batteryReceiver = new BatteryReceiver(this);
        var filter = new IntentFilter();
        filter.AddAction(Intent.ActionBatteryChanged);
        filter.AddAction(Intent.ActionPowerConnected);
        filter.AddAction(Intent.ActionPowerDisconnected);
        filter.AddAction(PowerManager.ActionPowerSaveModeChanged);
        RegisterReceiver(_batteryReceiver, filter);
    }

    public override StartCommandResult OnStartCommand(Intent? intent, StartCommandFlags flags, int startId)
    {
        var notification = new NotificationCompat.Builder(this, ChannelId)
            .SetContentTitle("CleanWatt")
            .SetContentText("Mise à jour en temps réel active")
            .SetSmallIcon(global::Android.Resource.Drawable.IcMenuInfoDetails)
            .SetOngoing(true)
            .SetPriority(NotificationCompat.PriorityMin)
            .Build();

        if (Build.VERSION.SdkInt >= BuildVersionCodes.UpsideDownCake)
        {
            StartForeground(NotificationId, notification, ForegroundService.TypeSpecialUse);
        }
        else
        {
            StartForeground(NotificationId, notification);
        }

        UpdateAllWidgets();

        return StartCommandResult.Sticky;
    }

    public void UpdateAllWidgets()
    {
        var appWidgetManager = AppWidgetManager.GetInstance(this);
        if (appWidgetManager == null) return;

        var componentName = new ComponentName(this, Java.Lang.Class.FromType(typeof(CleanWattWidget)));
        var widgetIds = appWidgetManager.GetAppWidgetIds(componentName);

        if (widgetIds != null)
        {
            foreach (var widgetId in widgetIds)
            {
                CleanWattWidget.UpdateWidget(this, appWidgetManager, widgetId);
            }
        }
    }

    private void CreateNotificationChannel()
    {
        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            var channel = new NotificationChannel(
                ChannelId,
                "CleanWatt - Temps réel",
                NotificationImportance.Min)
            {
                Description = "Mise à jour en temps réel du widget"
            };

            var notificationManager = GetSystemService(NotificationService) as NotificationManager;
            notificationManager?.CreateNotificationChannel(channel);
        }
    }

    public override void OnDestroy()
    {
        if (_batteryReceiver != null)
        {
            UnregisterReceiver(_batteryReceiver);
        }
        base.OnDestroy();
    }

    private class BatteryReceiver : BroadcastReceiver
    {
        private readonly BatteryUpdateService _service;

        public BatteryReceiver(BatteryUpdateService service)
        {
            _service = service;
        }

        public override void OnReceive(Context? context, Intent? intent)
        {
            if (intent == null) return;
            _service.UpdateAllWidgets();

            // Notify the app to update its UI
            MessagingCenter.Send<object>(this, "BatteryChanged");
        }
    }
}
