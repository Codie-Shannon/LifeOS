using System.Text.Json;
using LifeOS.Core.ItemState;
using LifeOS.Shared.Storage;

namespace LifeOS.Shared.ItemState;

public static class LifeOsItemStateStorage
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public static string FilePath => LocalAppDataPath.GetFilePath("lifeos-item-state-engine.json");

    public static LifeOsItemStateProfile Load()
    {
        try
        {
            if (!File.Exists(FilePath))
            {
                return LifeOsItemStateDemoData.CreateDefaultProfile();
            }

            var json = File.ReadAllText(FilePath);

            return string.IsNullOrWhiteSpace(json)
                ? LifeOsItemStateDemoData.CreateDefaultProfile()
                : JsonSerializer.Deserialize<LifeOsItemStateProfile>(json, Options) ?? LifeOsItemStateDemoData.CreateDefaultProfile();
        }
        catch
        {
            return LifeOsItemStateDemoData.CreateDefaultProfile();
        }
    }

    public static void Save(LifeOsItemStateProfile profile)
    {
        File.WriteAllText(FilePath, JsonSerializer.Serialize(profile, Options));
    }

    public static void ResetToDemoData()
    {
        Save(LifeOsItemStateDemoData.CreateDefaultProfile());
    }
}
