using System.Text.Json;
using LifeOS.Core.MoneyObligations;
using LifeOS.Shared.Storage;

namespace LifeOS.Shared.MoneyObligations;

public static class MoneyObligationStorage
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public static string FilePath => LocalAppDataPath.GetFilePath("lifeos-money-obligations.json");

    public static MoneyObligationProfile Load()
    {
        try
        {
            if (!File.Exists(FilePath))
            {
                return MoneyObligationDemoData.CreateDefaultProfile();
            }

            var json = File.ReadAllText(FilePath);

            return string.IsNullOrWhiteSpace(json)
                ? MoneyObligationDemoData.CreateDefaultProfile()
                : JsonSerializer.Deserialize<MoneyObligationProfile>(json, Options) ?? MoneyObligationDemoData.CreateDefaultProfile();
        }
        catch
        {
            return MoneyObligationDemoData.CreateDefaultProfile();
        }
    }

    public static void Save(MoneyObligationProfile profile)
    {
        File.WriteAllText(FilePath, JsonSerializer.Serialize(profile, Options));
    }

    public static void ResetToDemoData()
    {
        Save(MoneyObligationDemoData.CreateDefaultProfile());
    }
}
