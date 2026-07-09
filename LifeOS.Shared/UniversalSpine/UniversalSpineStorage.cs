using System.Text.Json;
using LifeOS.Core.UniversalSpine;
using LifeOS.Shared.Storage;

namespace LifeOS.Shared.UniversalSpine;

public static class UniversalSpineStorage
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public static string FilePath => LocalAppDataPath.GetFilePath("universal-spine.json");

    public static UniversalSpineProfile Load()
    {
        try
        {
            if (!File.Exists(FilePath))
            {
                return UniversalSpineDemoData.CreateDefaultProfile();
            }

            var json = File.ReadAllText(FilePath);

            return string.IsNullOrWhiteSpace(json)
                ? UniversalSpineDemoData.CreateDefaultProfile()
                : JsonSerializer.Deserialize<UniversalSpineProfile>(json, Options) ?? UniversalSpineDemoData.CreateDefaultProfile();
        }
        catch
        {
            return UniversalSpineDemoData.CreateDefaultProfile();
        }
    }

    public static void Save(UniversalSpineProfile profile)
    {
        File.WriteAllText(FilePath, JsonSerializer.Serialize(profile, Options));
    }

    public static void ResetToDemoData()
    {
        Save(UniversalSpineDemoData.CreateDefaultProfile());
    }
}
