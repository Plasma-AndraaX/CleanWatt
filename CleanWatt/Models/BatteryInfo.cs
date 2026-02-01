namespace BatteryWidget.Models;

public class BatteryInfo
{
    public int Level { get; set; }
    public BatteryStatus Status { get; set; }
    public float Temperature { get; set; }
    public BatteryHealth Health { get; set; }
    public bool IsPowerSaveMode { get; set; }

    public bool IsCharging => Status == BatteryStatus.Charging || Status == BatteryStatus.Full;

    public string HealthText => Health switch
    {
        BatteryHealth.Good => "Bonne",
        BatteryHealth.Overheat => "Surchauffe",
        BatteryHealth.Dead => "Morte",
        BatteryHealth.OverVoltage => "Surtension",
        BatteryHealth.Cold => "Froide",
        _ => "Inconnue"
    };

    public string StatusText => Status switch
    {
        BatteryStatus.Charging => "⚡ En charge",
        BatteryStatus.Discharging => "Décharge",
        BatteryStatus.Full => "⚡ Pleine",
        BatteryStatus.NotCharging => "Non en charge",
        _ => "Inconnu"
    };

    public string PowerSaveText => IsPowerSaveMode ? "🔋 Éco" : "";
}

// Values match Android BatteryManager constants
public enum BatteryStatus
{
    Unknown = 1,
    Charging = 2,
    Discharging = 3,
    NotCharging = 4,
    Full = 5
}

public enum BatteryHealth
{
    Unknown = 1,
    Good = 2,
    Overheat = 3,
    Dead = 4,
    OverVoltage = 5,
    Cold = 7
}
