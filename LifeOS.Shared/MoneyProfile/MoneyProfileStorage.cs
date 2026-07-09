using System.Text.Json;
using LifeOS.Core.MoneyProfile;
using LifeOS.Shared.Storage;

namespace LifeOS.Shared.MoneyProfile;

public static class MoneyProfileStorage
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public static string FilePath => LocalAppDataPath.GetFilePath("lifeos-money-profile.json");

    public static MoneyProfilePlan Load()
    {
        try
        {
            if (!File.Exists(FilePath))
            {
                return MoneyProfileDemoData.CreateDefaultProfile();
            }

            var json = File.ReadAllText(FilePath);

            return string.IsNullOrWhiteSpace(json)
                ? MoneyProfileDemoData.CreateDefaultProfile()
                : JsonSerializer.Deserialize<MoneyProfilePlan>(json, Options) ?? MoneyProfileDemoData.CreateDefaultProfile();
        }
        catch
        {
            return MoneyProfileDemoData.CreateDefaultProfile();
        }
    }

    public static void Save(MoneyProfilePlan profile)
    {
        File.WriteAllText(FilePath, JsonSerializer.Serialize(profile, Options));
    }

    public static void ResetToDemoData()
    {
        Save(MoneyProfileDemoData.CreateDefaultProfile());
    }
}
