using System.Text.Json;
using LifeOS.Core.LifeOsSpine;
using LifeOS.Shared.Storage;

namespace LifeOS.Shared.LifeOsSpine;

public static class LifeOsSpineStorage
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public static string FilePath => LocalAppDataPath.GetFilePath("lifeos-spine-map.json");

    public static LifeOsSpineProfile Load()
    {
        try
        {
            if (!File.Exists(FilePath))
            {
                return LifeOsSpineDemoData.CreateDefaultProfile();
            }

            var json = File.ReadAllText(FilePath);

            return string.IsNullOrWhiteSpace(json)
                ? LifeOsSpineDemoData.CreateDefaultProfile()
                : JsonSerializer.Deserialize<LifeOsSpineProfile>(json, Options) ?? LifeOsSpineDemoData.CreateDefaultProfile();
        }
        catch
        {
            return LifeOsSpineDemoData.CreateDefaultProfile();
        }
    }

    public static void Save(LifeOsSpineProfile profile)
    {
        File.WriteAllText(FilePath, JsonSerializer.Serialize(profile, Options));
    }

    public static void ResetToDemoData()
    {
        Save(LifeOsSpineDemoData.CreateDefaultProfile());
    }
}
