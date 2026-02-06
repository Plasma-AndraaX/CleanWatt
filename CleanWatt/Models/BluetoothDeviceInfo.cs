namespace BatteryWidget.Models;

public class BluetoothDeviceInfo
{
    public string Name { get; set; } = "";
    public string Address { get; set; } = "";
    public int BatteryLevel { get; set; } = -1;
    public BluetoothDeviceType DeviceType { get; set; } = BluetoothDeviceType.Unknown;

    public bool HasBattery => BatteryLevel >= 0;

    public string DeviceIcon => DeviceType switch
    {
        BluetoothDeviceType.Headphones => "\uD83C\uDFA7",
        BluetoothDeviceType.Speaker => "\uD83D\uDD0A",
        BluetoothDeviceType.Watch => "\u231A",
        BluetoothDeviceType.Gamepad => "\uD83C\uDFAE",
        BluetoothDeviceType.Keyboard => "\u2328\uFE0F",
        BluetoothDeviceType.Mouse => "\uD83D\uDDB1\uFE0F",
        BluetoothDeviceType.Phone => "\uD83D\uDCF1",
        BluetoothDeviceType.Computer => "\uD83D\uDCBB",
        BluetoothDeviceType.Car => "\uD83D\uDE97",
        _ => "\uD83D\uDD35"
    };
}

public enum BluetoothDeviceType
{
    Unknown,
    Headphones,
    Speaker,
    Watch,
    Gamepad,
    Keyboard,
    Mouse,
    Phone,
    Computer,
    Car
}
