using System.Text.Json;
using LifeOS.Core.SettingsSafety;
using LifeOS.Shared.Storage;

namespace LifeOS.Shared.SettingsSafety;

public static class SettingsSafetyThemeStorage
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public static string FilePath => LocalAppDataPath.GetFilePath("settings-safety-theme.json");

    public static SettingsSafetyThemeProfile Load()
    {
        try
        {
            if (!File.Exists(FilePath))
            {
                return SettingsSafetyThemeDemoData.CreateDefaultProfile();
            }

            var json = File.ReadAllText(FilePath);
            return string.IsNullOrWhiteSpace(json)
                ? SettingsSafetyThemeDemoData.CreateDefaultProfile()
                : JsonSerializer.Deserialize<SettingsSafetyThemeProfile>(json, Options) ?? SettingsSafetyThemeDemoData.CreateDefaultProfile();
        }
        catch
        {
            return SettingsSafetyThemeDemoData.CreateDefaultProfile();
        }
    }

    public static void Save(SettingsSafetyThemeProfile profile)
    {
        profile.UpdatedAt = DateTime.Now;
        File.WriteAllText(FilePath, JsonSerializer.Serialize(profile, Options));
    }

    public static void ResetToDemoData()
    {
        Save(SettingsSafetyThemeDemoData.CreateDefaultProfile());
    }
}
