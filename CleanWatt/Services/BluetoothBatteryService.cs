#if ANDROID
using Android.Bluetooth;
using Android.Content;
using Android.Content.PM;
using Android.OS;
#endif
using BatteryWidget.Models;
using BtDeviceType = BatteryWidget.Models.BluetoothDeviceType;

namespace BatteryWidget.Services;

public static class BluetoothBatteryService
{
#if ANDROID
    public static List<BluetoothDeviceInfo> GetConnectedDevices(Context context)
    {
        var devices = new List<BluetoothDeviceInfo>();

        // Check BLUETOOTH_CONNECT permission on API 31+
        if (Build.VERSION.SdkInt >= BuildVersionCodes.S)
        {
            if (context.CheckSelfPermission(Android.Manifest.Permission.BluetoothConnect)
                != Permission.Granted)
            {
                return devices;
            }
        }

        var adapter = BluetoothAdapter.DefaultAdapter;
        if (adapter == null || !adapter.IsEnabled)
            return devices;

        var bondedDevices = adapter.BondedDevices;
        if (bondedDevices == null)
            return devices;

        foreach (var device in bondedDevices)
        {
            if (device == null) continue;

            if (!IsConnected(device))
                continue;

            int batteryLevel = GetBatteryLevel(device);
            if (batteryLevel < 0)
                continue;

            devices.Add(new BluetoothDeviceInfo
            {
                Name = device.Name ?? device.Address ?? "Unknown",
                Address = device.Address ?? "",
                BatteryLevel = batteryLevel,
                DeviceType = MapDeviceType(device)
            });
        }

        return devices;
    }

    private static bool IsConnected(BluetoothDevice device)
    {
        try
        {
            var method = Java.Lang.Class.FromType(typeof(BluetoothDevice))
                .GetMethod("isConnected");
            if (method == null) return false;
            var result = method.Invoke(device);
            return result != null && (bool)result;
        }
        catch
        {
            return false;
        }
    }

    private static int GetBatteryLevel(BluetoothDevice device)
    {
        try
        {
            var method = Java.Lang.Class.FromType(typeof(BluetoothDevice))
                .GetMethod("getBatteryLevel");
            if (method == null) return -1;
            var result = method.Invoke(device);
            if (result is Java.Lang.Integer intResult)
                return intResult.IntValue();
            return -1;
        }
        catch
        {
            return -1;
        }
    }

    private static BtDeviceType MapDeviceType(BluetoothDevice device)
    {
        var btClass = device.BluetoothClass;
        if (btClass == null)
            return BtDeviceType.Unknown;

        var major = btClass.MajorDeviceClass;

        // Check specific device class first for more precise mapping
        int deviceClass = (int)btClass.DeviceClass;

        // Audio/Video major class
        if (major == MajorDeviceClass.AudioVideo)
        {
            return deviceClass switch
            {
                0x0418 => BtDeviceType.Headphones, // Headphones
                0x0404 => BtDeviceType.Headphones, // Wearable Headset
                0x0420 => BtDeviceType.Speaker,    // Loudspeaker
                _ => BtDeviceType.Headphones       // Default audio to headphones
            };
        }

        return major switch
        {
            MajorDeviceClass.Wearable => BtDeviceType.Watch,
            MajorDeviceClass.Toy => BtDeviceType.Gamepad,
            MajorDeviceClass.Phone => BtDeviceType.Phone,
            MajorDeviceClass.Computer => BtDeviceType.Computer,
            MajorDeviceClass.Peripheral => MapPeripheralType(deviceClass),
            MajorDeviceClass.Networking => BtDeviceType.Unknown,
            _ => BtDeviceType.Unknown
        };
    }

    private static BtDeviceType MapPeripheralType(int deviceClass)
    {
        // Peripheral minor class bits
        int minor = deviceClass & 0x00FC;
        return minor switch
        {
            0x0040 => BtDeviceType.Keyboard,
            0x0080 => BtDeviceType.Mouse,
            0x00C0 => BtDeviceType.Keyboard, // Combo keyboard/mouse
            0x0008 => BtDeviceType.Gamepad,   // Joystick
            0x0014 => BtDeviceType.Gamepad,   // Gamepad
            _ => BtDeviceType.Unknown
        };
    }
#endif
}
