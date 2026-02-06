using System.Globalization;
using BatteryWidget.Resources.Strings;

namespace BatteryWidget.Services;

public static class LocalizationService
{
    private const string LanguageKey = "app_language";
    private const string FahrenheitKey = "use_fahrenheit";

    private static string[]? _supportedLanguages;
    private static string[]? _supportedLanguageNames;

    public static string[] SupportedLanguages => _supportedLanguages ??= DiscoverLanguages();
    public static string[] SupportedLanguageNames => _supportedLanguageNames ??= SupportedLanguages
        .Select(code => new CultureInfo(code).NativeName)
        .Select(name => char.ToUpper(name[0]) + name[1..])
        .ToArray();

    public static string GetLanguage()
    {
        var saved = Preferences.Get(LanguageKey, string.Empty);
        if (!string.IsNullOrEmpty(saved))
            return saved;

        // Detect device language, fallback to English (default resource)
        var deviceLang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        return Array.Exists(SupportedLanguages, l => l == deviceLang) ? deviceLang : "en";
    }

    public static void SetLanguage(string code)
    {
        Preferences.Set(LanguageKey, code);
        ApplyLanguage();
    }

    public static bool UseFahrenheit
    {
        get => Preferences.Get(FahrenheitKey, false);
        set => Preferences.Set(FahrenheitKey, value);
    }

    public static void ApplyLanguage()
    {
        var code = GetLanguage();
        var culture = new CultureInfo(code);
        AppStrings.Culture = culture;
    }

    public static string FormatTemperature(float celsius)
    {
        if (UseFahrenheit)
        {
            float fahrenheit = celsius * 9f / 5f + 32f;
            return $"{fahrenheit:F1}°F";
        }
        return $"{celsius:F1}°C";
    }

    private static string[] DiscoverLanguages()
    {
        // English is the default resource (AppStrings.resx), always available
        var languages = new List<string> { "en" };

        // Discover satellite assemblies (fr, es, de, etc.)
        foreach (var culture in CultureInfo.GetCultures(CultureTypes.NeutralCultures))
        {
            if (culture.Equals(CultureInfo.InvariantCulture))
                continue;

            try
            {
                var rs = AppStrings.ResourceManager.GetResourceSet(culture, true, false);
                if (rs != null)
                {
                    var code = culture.TwoLetterISOLanguageName;
                    if (!languages.Contains(code))
                        languages.Add(code);
                }
            }
            catch
            {
                // No satellite assembly for this culture
            }
        }

        return languages.ToArray();
    }
}
