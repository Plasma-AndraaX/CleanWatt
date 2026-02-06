using BatteryWidget.Resources.Strings;

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
        BatteryHealth.Good => AppStrings.HealthGood,
        BatteryHealth.Overheat => AppStrings.HealthOverheat,
        BatteryHealth.Dead => AppStrings.HealthDead,
        BatteryHealth.OverVoltage => AppStrings.HealthOverVoltage,
        BatteryHealth.Cold => AppStrings.HealthCold,
        _ => AppStrings.HealthUnknown
    };

    public string StatusText => Status switch
    {
        BatteryStatus.Charging => AppStrings.StatusCharging,
        BatteryStatus.Discharging => AppStrings.StatusDischarging,
        BatteryStatus.Full => AppStrings.StatusFull,
        BatteryStatus.NotCharging => AppStrings.StatusNotCharging,
        _ => AppStrings.StatusUnknown
    };

    public string PowerSaveText => IsPowerSaveMode ? AppStrings.PowerSave : "";
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
