using System.Text.Json;
using LifeOS.Core.OsNavigation;
using LifeOS.Shared.Storage;

namespace LifeOS.Shared.OsNavigation;

public static class OsNavigationStorage
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public static string FilePath => LocalAppDataPath.GetFilePath("os-navigation.json");

    public static OsNavigationProfile Load()
    {
        try
        {
            if (!File.Exists(FilePath))
            {
                return OsNavigationDemoData.CreateDefaultProfile();
            }

            var json = File.ReadAllText(FilePath);

            return string.IsNullOrWhiteSpace(json)
                ? OsNavigationDemoData.CreateDefaultProfile()
                : JsonSerializer.Deserialize<OsNavigationProfile>(json, Options) ?? OsNavigationDemoData.CreateDefaultProfile();
        }
        catch
        {
            return OsNavigationDemoData.CreateDefaultProfile();
        }
    }

    public static void Save(OsNavigationProfile profile)
    {
        File.WriteAllText(FilePath, JsonSerializer.Serialize(profile, Options));
    }

    public static void ResetToDemoData()
    {
        Save(OsNavigationDemoData.CreateDefaultProfile());
    }
}
